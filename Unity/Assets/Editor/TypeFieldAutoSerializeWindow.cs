using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Game.Editor
{
    public class TypeFieldAutoSerializeWindow : EditorWindow
    {
        private class MemberData
        {
            /* field */
            public readonly string Name;
            public readonly string PropertyName;
            public string TypeName;
            public bool IsStatic;
            public bool IsReadOnly;
            public FieldAttributes FieldAttributes;
            public FieldAttributes PropertyGetAttributes;
            public FieldAttributes PropertySetAttributes;

            /* ctor */
            public MemberData(string name, string propertyName)
            {
                if (!string.IsNullOrEmpty(name) && name.Length >= 1)
                {
                    if (name.StartsWith("m_", StringComparison.Ordinal))
                        name = name.Substring(2);
                    else if (name.StartsWith("_", StringComparison.Ordinal))
                        name = name.Substring(1);
                    if (name.Length > 1)
                    {
                        Name = "m_" + name;
                        PropertyName = name[0].ToString().ToUpper(CultureInfo.InvariantCulture) + name.Substring(1);
                        return;
                    }
                }
                if (string.IsNullOrEmpty(propertyName) || propertyName.Length < 1)
                {
                    Name = "m_A";
                    PropertyName = "A";
                }
                else
                {
                    PropertyName = propertyName[0].ToString().ToUpper(CultureInfo.InvariantCulture) + propertyName.Substring(1);
                    Name = "m_" + PropertyName;
                }
            }


            /* inter */

            /* func */
            public string FieldLine()
            {
                switch (FieldAttributes)
                {
                    case FieldAttributes.Private:
                        return $"[SerializeField]\nprivate {(IsStatic ? "static " : string.Empty)}{(IsReadOnly ? "readonly " : string.Empty)}{TypeName} {Name};";
                    case FieldAttributes.Assembly:
                        return $"[SerializeField]\ninternal {(IsStatic? "static ": string.Empty)}{(IsReadOnly ? "readonly " : string.Empty)}{TypeName} {Name};";
                    case FieldAttributes.Public:
                        return $"[SerializeField]\npublic {(IsStatic ? "static " : string.Empty)}{(IsReadOnly ? "readonly " : string.Empty)}{TypeName} {Name};";
                    case FieldAttributes.Family:
                        return $"[SerializeField]\nprotected {(IsStatic ? "static " : string.Empty)}{(IsReadOnly ? "readonly " : string.Empty)}{TypeName} {Name};";
                    default:
                        return string.Empty;
                }
            }

            public string PropertyLine()
            {
                if (PropertySetAttributes == FieldAttributes.InitOnly)
                    return string.Empty;
                // 语法上 get, set 不能分别赋予 internal 和 protected, 这里没做额外处理
                int getIndex = Array.IndexOf(SupportedAttributes, PropertyGetAttributes);
                int setIndex = Array.IndexOf(SupportedAttributes, PropertySetAttributes);
                if (getIndex == -1 || setIndex == -1)
                    return string.Empty;
                FieldAttributes accessor = SupportedAttributes[Mathf.Max(getIndex, setIndex)];
                if (accessor == FieldAttributes.InitOnly)
                    return string.Empty;
                string first;
                switch (accessor)
                {
                    case FieldAttributes.Private:
                        first = $"private {(IsStatic ? "static " : string.Empty)}{TypeName} {PropertyName}\n{{\n";
                        break;
                    case FieldAttributes.Assembly:
                        first = $"internal {(IsStatic ? "static " : string.Empty)}{TypeName} {PropertyName}\n{{\n";
                        break;
                    case FieldAttributes.Public:
                        first = $"public {(IsStatic ? "static " : string.Empty)}{TypeName} {PropertyName}\n{{\n";
                        break;
                    case FieldAttributes.Family:
                        first = $"protected {(IsStatic ? "static " : string.Empty)}{TypeName} {PropertyName}\n{{\n";
                        break;
                    default:
                        return string.Empty;
                }

                string second;
                if (PropertyGetAttributes != accessor)
                {
                    switch (PropertyGetAttributes)
                    {
                        case FieldAttributes.Private:
                            second = $"    private get => {Name};\n";
                            break;
                        case FieldAttributes.InitOnly:
                            second = string.Empty;
                            break;
                        case FieldAttributes.Assembly:
                            second = $"    internal get => {Name};\n";
                            break;
                        case FieldAttributes.Public:
                            second = $"    public get => {Name};\n";
                            break;
                        case FieldAttributes.Family:
                            second = $"    protected get => {Name};\n";
                            break;
                        default:
                            return string.Empty;
                    }
                }
                else
                    second = $"    get => {Name};\n";
                string tail;
                if (PropertySetAttributes != accessor)
                {
                    switch (PropertySetAttributes)
                    {
                        case FieldAttributes.Private:
                            tail = $"    private set => {Name} = value;\n}}";
                            break;
                        case FieldAttributes.InitOnly:
                            tail = $"}}";
                            break;
                        case FieldAttributes.Assembly:
                            tail = $"    internal set => {Name} = value;\n}}";
                            break;
                        case FieldAttributes.Public:
                            tail = $"    public set => {Name} = value;\n}}";
                            break;
                        case FieldAttributes.Family:
                            tail = $"    protected set => {Name} = value;\n}}";
                            break;
                        default:
                            return string.Empty;
                    }
                }
                else
                    tail = $"    set => {Name} = value;\n}}";
                return first + second + tail;
            }

            public override int GetHashCode()
            {
                int hashCode = -1238757342;
                hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
                hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(PropertyName);
                hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(TypeName);
                hashCode = hashCode * -1521134295 + IsStatic.GetHashCode();
                hashCode = hashCode * -1521134295 + IsReadOnly.GetHashCode();
                hashCode = hashCode * -1521134295 + FieldAttributes.GetHashCode();
                hashCode = hashCode * -1521134295 + PropertyGetAttributes.GetHashCode();
                hashCode = hashCode * -1521134295 + PropertySetAttributes.GetHashCode();
                return hashCode;
            }
            public override bool Equals(object obj)
            {
                return obj is MemberData data &&
                       Name == data.Name &&
                       PropertyName == data.PropertyName &&
                       TypeName == data.TypeName &&
                       IsStatic == data.IsStatic &&
                       FieldAttributes == data.FieldAttributes &&
                       PropertyGetAttributes == data.PropertyGetAttributes &&
                       PropertySetAttributes == data.PropertySetAttributes;
            }

            public override string ToString()
            {
                return $"{TypeName} {FieldAttributes} {Name}\n {TypeName} {PropertyName} {PropertyGetAttributes} get {PropertySetAttributes} set";
            }
        }

        /* const */
        public const int WaitTime = 5;
        public const string Template0 = "public int AA {get; set; }";
        public const string Template1 = "private static CustomType BB;";
        public static readonly FieldAttributes[] SupportedAttributes = new FieldAttributes[]
        {
            FieldAttributes.InitOnly,
            FieldAttributes.Private,
            FieldAttributes.Assembly,
            FieldAttributes.Family,
            FieldAttributes.Public,
        };
        private static readonly Lazy<string[]> SupportedAttributesString =
            new Lazy<string[]>(() =>
            {
                string[] result = new string[SupportedAttributes.Length];
                for (int i = 0; i < SupportedAttributes.Length; i++)
                    result[i] = SupportedAttributes[i].ToString();
                return result;
            });

        private static readonly Dictionary<Regex, Func<Match, MemberData>> m_TransferMap =
            new Dictionary<Regex, Func<Match, MemberData>>()
            {
                {
                    new Regex("(?<FieldAttributes>public|protected|internal|private) (static )?(readonly )?(?<Type>[\\w<,.>]+(\\[\\])?) (?<FieldName>\\w+);"),
                    (Match match) =>
                    {
                        string typeName = match.Groups["Type"].Value;
                        string fieldName = match.Groups["FieldName"].Value;
                        string matchValue = match.Value;
                        bool isReadOnly = matchValue.Contains(" readonly ");
                        MemberData memberData = new MemberData(fieldName, null)
                        {
                            TypeName = typeName,
                            PropertySetAttributes = FieldAttributes.InitOnly,
                            IsStatic = matchValue.Contains(" static "),
                            IsReadOnly = isReadOnly,
                        };
                        string fieldAttributes = match.Groups["FieldAttributes"].Value;
                        if (fieldAttributes == "public")
                        {
                            memberData.PropertyGetAttributes = FieldAttributes.Public;
                            memberData.FieldAttributes = FieldAttributes.Public;
                        }
                        else if (fieldAttributes == "protected")
                        {
                            memberData.PropertyGetAttributes = FieldAttributes.Family;
                            memberData.FieldAttributes = FieldAttributes.Family;
                        }
                        else if (fieldAttributes == "internal")
                        {
                            memberData.PropertyGetAttributes = FieldAttributes.Assembly;
                            memberData.FieldAttributes = FieldAttributes.Assembly;
                        }
                        else if (fieldAttributes == "private")
                        {
                            memberData.PropertyGetAttributes = FieldAttributes.Assembly;
                            memberData.FieldAttributes = FieldAttributes.Private;
                        }
                        if (!isReadOnly)
                            memberData.PropertySetAttributes = memberData.PropertyGetAttributes;
                        return memberData;
                    }
                },
                {
                    new Regex("(?<PropertyAttributes>public|protected|internal|private) (static )?(?<Type>[\\w<,.>]+(\\[\\])?) (?<PropertyName>\\w+)\\s?{\\s?get;\\s?(set;\\s?)?}"),
                    (Match match) =>
                    {
                        string typeName = match.Groups["Type"].Value;
                        string propertyName = match.Groups["PropertyName"].Value;
                        string matchValue = match.Value;
                        MemberData memberData = new MemberData(null, propertyName)
                        {
                            TypeName = typeName,
                            FieldAttributes = FieldAttributes.Private,
                            IsStatic = matchValue.Contains(" static "),
                            IsReadOnly = false,
                        };
                        string propertyAttributes = match.Groups["PropertyAttributes"].Value;
                        if (propertyAttributes == "public")
                            memberData.PropertyGetAttributes = FieldAttributes.Public;
                        else if (propertyAttributes == "property")
                            memberData.PropertyGetAttributes = FieldAttributes.Family;
                        else if (propertyAttributes == "internal")
                            memberData.PropertyGetAttributes = FieldAttributes.Assembly;
                        else if (propertyAttributes == "private")
                            memberData.PropertyGetAttributes = FieldAttributes.Private;

                        if (matchValue.Contains("set;"))
                            memberData.PropertySetAttributes = FieldAttributes.Assembly;
                        else
                            memberData.PropertySetAttributes = FieldAttributes.InitOnly;
                        return memberData;
                    }
                }
            };
        
        /* field */
        private string m_InputContent;
        private List<MemberData> m_MemberDataList;
        private string m_TransferResult;
        private bool m_UseUnionMemberData;
        private MemberData m_UnionMemberData;

        private Vector2 m_ScrollViewPosition;

        private int m_InputIsDirty;
        private int m_OptionIsDirty;

        /* ctor */
        [MenuItem("Tools/Type Field Auto Serialize", priority = 20)]
        public static TypeFieldAutoSerializeWindow OpenWindow()
        {
            TypeFieldAutoSerializeWindow editorWindow = GetWindow<TypeFieldAutoSerializeWindow>();
            editorWindow.titleContent = new GUIContent("类型字段自动添加序列化标记");
            return editorWindow;
        }
        private void OnEnable()
        {
            m_InputContent = string.Empty;
            m_MemberDataList = new List<MemberData>();
            m_TransferResult = string.Empty;
            m_UseUnionMemberData = true;
            m_UnionMemberData = new MemberData("Union", "Union")
            {
                IsReadOnly = false,
                IsStatic = false,
                PropertyGetAttributes = FieldAttributes.Public,
                PropertySetAttributes = FieldAttributes.Private,
            };
        }

        /* func */
        private void OnGUI()
        {
            m_ScrollViewPosition = GUILayout.BeginScrollView(m_ScrollViewPosition);
            DrawHelper();
            EditorGUILayout.Space();
            DrawUserInput();
            EditorGUILayout.Space();
            DrawTransfer();
            EditorGUILayout.Space();
            DrawUnionOption();
            DrawSeparateOptions();
            GUILayout.Space(50f);
            GUILayout.EndScrollView();
        }

        private void DrawHelper()
        {
            if (string.IsNullOrWhiteSpace(m_InputContent))
            {
                GUILayout.Label("输入示例");
                GUILayout.Label(Template0);
                GUILayout.Label(Template1);
            }
        }

        private void DrawUserInput()
        {
            GUILayout.Label("输入源码");
            string result = GUILayout.TextArea(m_InputContent);
            if (result != m_InputContent)
            {
                m_InputContent = result;
                EditorApplication.delayCall += () => m_InputIsDirty = WaitTime;
                EditorApplication.delayCall += () => m_OptionIsDirty = WaitTime;
            }
        }
        private void DrawUnionOption()
        {
            bool newValue = GUILayout.Toggle(m_UseUnionMemberData, "使用相同访问权限");
            if (newValue != m_UseUnionMemberData)
            {
                m_UseUnionMemberData = newValue;
                EditorApplication.delayCall += () => m_OptionIsDirty = 1;
            }
            if (m_UseUnionMemberData)
            {
                bool change = DrawMemberData(m_UnionMemberData);
                if (change)
                {
                    EditorApplication.delayCall += () => m_OptionIsDirty = 1;
                }
            }
        }
        private void DrawSeparateOptions()
        {
            if (m_UseUnionMemberData)
                return;
            bool haveChange = m_OptionIsDirty >= 0;
            foreach (MemberData memberData in m_MemberDataList)
            {
                bool change = DrawMemberData(memberData);
                haveChange |= change;
            }
            if (haveChange)
            {
                EditorApplication.delayCall += () => m_OptionIsDirty = 1;
            }
        }
        private static bool DrawMemberData(MemberData memberData)
        {
            int beforeHashCode = memberData.GetHashCode();
            GUILayout.BeginHorizontal();
            GUILayout.Label(memberData.Name, GUILayout.MinWidth(150f));
            memberData.IsStatic = GUILayout.Toggle(memberData.IsStatic, "static");
            memberData.IsReadOnly = GUILayout.Toggle(memberData.IsReadOnly, "readonly");
            int index = Array.IndexOf(SupportedAttributes, memberData.PropertyGetAttributes);
            if (index < 0)
                index = 0;
            index = EditorGUILayout.Popup(index, SupportedAttributesString.Value, GUILayout.MinWidth(100f));
            memberData.PropertyGetAttributes = SupportedAttributes[index];
            index = Array.IndexOf(SupportedAttributes, memberData.PropertySetAttributes);
            if (index < 0)
                index = 0;
            index = EditorGUILayout.Popup(index, SupportedAttributesString.Value, GUILayout.MinWidth(100f));
            memberData.PropertySetAttributes = SupportedAttributes[index];
            GUILayout.EndHorizontal();
            return beforeHashCode == memberData.GetHashCode();
        }

        private void DrawTransfer()
        {
            GUILayout.Label("输出");
            GUILayout.TextArea(m_TransferResult);
        }

        private void OnInspectorUpdate()
        {
            if (m_InputIsDirty > 0)
            {
                m_InputIsDirty--;
                return;
            }
            else if (m_InputIsDirty-- == 0)
            {
                RecreateMemberData();
                m_OptionIsDirty = 0;
            }
            if (m_OptionIsDirty > 0)
            {
                m_OptionIsDirty--;
                return;
            }
            else if (m_OptionIsDirty-- == 0)
            {
                SyncUnionMemberData();
                RetransferMemberData();
                Repaint();
                return;
            }
        }
        private void RecreateMemberData()
        {
            m_MemberDataList.Clear();
            string[] lines = m_InputContent.Split('\n');
            foreach (string line in lines)
            {
                foreach (var pair in m_TransferMap)
                {
                    if (pair.Key.Match(line) is Match match &&
                        match.Success)
                    {
                        MemberData memberData = pair.Value(match);
                        m_MemberDataList.Add(memberData);
                        break;
                    }
                }
            }
        }
        private void SyncUnionMemberData()
        {
            if (m_UseUnionMemberData)
            {
                foreach (MemberData memberData in m_MemberDataList)
                {
                    memberData.IsReadOnly = m_UnionMemberData.IsReadOnly;
                    memberData.IsStatic = m_UnionMemberData.IsStatic;
                    memberData.PropertyGetAttributes = m_UnionMemberData.PropertyGetAttributes;
                    memberData.PropertySetAttributes = m_UnionMemberData.PropertySetAttributes;
                }
            }
        }
        private void RetransferMemberData()
        {
            StringBuilder stringBuilder = new StringBuilder(m_MemberDataList.Count * 50);
            foreach (MemberData memberData in m_MemberDataList)
            {
                stringBuilder
                    .AppendLine(memberData.FieldLine())
                    .AppendLine(memberData.PropertyLine())
                    .AppendLine();
            }
            m_TransferResult = stringBuilder.ToString();
        }
    }
}