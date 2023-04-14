using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Table;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Game.Editor
{
    public class UnitTableWindow : EditorWindow
    {
        private UnitTable m_UnitTable;
        public UnitTable UnitTable => m_UnitTable;

        private int m_SaveDelay;
        private string m_UnitTablePath;

        private string m_SearchText;
        private Commander m_FilterCommander;
        private UnitLabel m_FilterLabel;
        private HashSet<int> m_SelectedUnitSet;
        private List<UnitTableEntryWrapper> m_InShowList;
        private HashSet<int> m_UnfoldSet;

        private Vector2 m_ScrollPosition;

        private string m_NewEntryAnnotation;
        private int m_NewEntryCopyID;

        [MenuItem("Tools/Unit Table Edit")]
        public static UnitTableWindow OpenWindow()
        {
            UnitTableWindow editorWindow = GetWindow<UnitTableWindow>();
            var rect = editorWindow.position;
            rect.width = 830f;
            editorWindow.position = rect;
            return editorWindow;
        }

        private void OnEnable()
        {
            titleContent = new GUIContent("Unit Table Edit");
            minSize = new Vector2(550f, 400f);

            m_UnitTablePath = $"{GameDefined.ResourceSubmoduleDirectory}/Tables/UnitTable.json";
            string content = File.ReadAllText(m_UnitTablePath);
            m_UnitTable = JSONMap.ParseJSON<UnitTable>(JSONObject.Create(content));

            m_SaveDelay = -1;
            m_SearchText = string.Empty;
            m_SelectedUnitSet = new HashSet<int>();
            m_InShowList = new List<UnitTableEntryWrapper>();
            m_UnfoldSet = new HashSet<int>();
            m_ScrollPosition = Vector2.zero;
            if (m_UnitTable != null)
            {
                FilterTable();
            }
        }

        private void OnInspectorUpdate()
        {
            titleContent.text = $"Unit Table Edit {(m_SaveDelay > 0 ? "*" : string.Empty)}";
            if (m_SaveDelay-- == 0)
            {
                SaveUnitTable();
            }
            Repaint();
        }
        private void OnGUI()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("UnitTablePath");
            GUILayout.Label(m_UnitTablePath);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.Space(15f);

            // Tool
            GUILayout.BeginHorizontal();
            GUILayout.Label("Regex", GUILayout.Width(80f));
            m_SearchText = GUILayout.TextField(m_SearchText, GUILayout.MinWidth(150f), GUILayout.MaxWidth(300f));
            m_FilterCommander = (Commander)EditorGUILayout.EnumFlagsField(m_FilterCommander, GUILayout.Width(100f));
            m_FilterLabel = (UnitLabel)EditorGUILayout.EnumFlagsField(m_FilterLabel, GUILayout.Width(80f));
            if (GUILayout.Button("Search", GUILayout.Width(60f)))
            {
                EditorApplication.delayCall += () =>
                {
                    OverrideTable();
                    FilterTable();
                };
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label($"Total: {m_InShowList.Count.ToString()} entries.");
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Save", GUILayout.Width(60f)))
            {
                SaveUnitTable();
                m_SaveDelay = -1;
            }

            // Unit Table Head
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUILayout.Label("Lock", GUILayout.Width(32f));
            GUILayout.Label("ID", GUILayout.Width(68f));
            GUILayout.Label("Name", GUILayout.Width(150f));
            GUILayout.Label("Annotation", GUILayout.Width(250f));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            // Unit Table Entries
            m_ScrollPosition = GUILayout.BeginScrollView(m_ScrollPosition, GUILayout.MinHeight(300f));
            for (int i = 0; i < m_InShowList.Count; i++)
            {
                PrintUnitEntry(m_InShowList[i]);
            }
            GUILayout.EndScrollView();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Annotation", GUILayout.Width(80f));
            m_NewEntryAnnotation = GUILayout.TextField(m_NewEntryAnnotation, GUILayout.MinWidth(250f));
            if (GUILayout.Button("Add", GUILayout.Width(60f)))
            {
                EditorApplication.delayCall += () =>
                {
                    AddNewUnitEntry();
                    m_SaveDelay = 100;
                };
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            m_NewEntryCopyID = EditorGUILayout.IntField(m_NewEntryCopyID, GUILayout.Width(150f));
            if (GUILayout.Button("Copy", GUILayout.Width(60f)))
            {
                EditorApplication.delayCall += () =>
                {
                    CopyNewUnitEntry();
                    m_SaveDelay = 100;
                };
            }
            GUILayout.EndHorizontal();
        }

        private void PrintUnitEntry(UnitTableEntryWrapper entry)
        {
            GUILayout.BeginHorizontal();
            bool isSelected = m_SelectedUnitSet.Contains(entry.ID);
            bool nowSelect = GUILayout.Toggle(isSelected, string.Empty, GUILayout.Width(20f));
            if (nowSelect != isSelected)
            {
                EditorApplication.delayCall += () =>
                {
                    if (nowSelect)
                        m_SelectedUnitSet.Add(entry.ID);
                    else
                        m_SelectedUnitSet.Remove(entry.ID);
                };
            }
            GUILayout.Label(entry.ID.ToString(), GUILayout.Width(80f));
            float spaceLength = position.width - 270f - 10f - 30f;
            float annotationLength = Mathf.Min(spaceLength, 400f);
            spaceLength -= annotationLength;
            if (m_UnfoldSet.Contains(entry.ID))
            {
                StringID nameStringID = entry.Name;
                nameStringID.Key = GUILayout.TextField(nameStringID.Key, GUILayout.Width(150f));
                entry.Name = nameStringID;
                entry.Annotation = GUILayout.TextField(entry.Annotation, GUILayout.Width(annotationLength));
                GUILayout.Space(spaceLength);
                if (GUILayout.Button("▽", GUILayout.Width(30f)))
                {
                    EditorApplication.delayCall += () =>
                    {
                        m_UnfoldSet.Remove(entry.ID);
                    };
                }
            }
            else
            {
                GUILayout.Label(entry.Name.Key, GUILayout.Width(150f));
                GUILayout.Label(entry.Annotation, GUILayout.Width(annotationLength));
                GUILayout.Space(spaceLength);
                if (GUILayout.Button("▷", GUILayout.Width(30f)))
                {
                    EditorApplication.delayCall += () =>
                    {
                        m_UnfoldSet.Add(entry.ID);
                    };
                }
            }
            GUILayout.EndHorizontal();
            if (!m_UnfoldSet.Contains(entry.ID))
                return;
            int hashCode = entry.GetHashCode();
            GUILayout.BeginHorizontal();
            GUILayout.Space(20f);
            //GUILayout.Label("Name", GUILayout.MinWidth(80f));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Space(20f);
            //GUILayout.Label("Annotation", GUILayout.MinWidth(80f));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Space(20f);
            GUILayout.Label("Commander", GUILayout.MinWidth(80f));
            entry.Commander = (Commander)EditorGUILayout.EnumPopup(entry.Commander, GUILayout.Width(100f));
            GUILayout.Space(10f);
            GUILayout.Label("UnlockLevel", GUILayout.MinWidth(80f));
            entry.UnlockLevel = EditorGUILayout.IntField(entry.UnlockLevel, GUILayout.Width(50f));
            GUILayout.Space(10f);
            GUILayout.Label("HP", GUILayout.MinWidth(30f));
            entry.HP = EditorGUILayout.IntField(entry.HP, GUILayout.Width(50f));
            GUILayout.Space(10f);
            GUILayout.Label("Energy", GUILayout.MinWidth(40f));
            entry.Energy = EditorGUILayout.IntField(entry.Energy, GUILayout.Width(50f));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Space(20f);
            GUILayout.Label("Crystal", GUILayout.Width(55f));
            entry.CrystalCost = EditorGUILayout.IntField(entry.CrystalCost, GUILayout.Width(50f));
            GUILayout.Space(10f);
            GUILayout.Label("Gas", GUILayout.Width(30f));
            entry.GasCost = EditorGUILayout.IntField(entry.GasCost, GUILayout.Width(50f));
            GUILayout.Space(10f);
            GUILayout.Label("Population", GUILayout.Width(70f));
            entry.Population = EditorGUILayout.FloatField(entry.Population, GUILayout.Width(50f));
            GUILayout.Space(10f);
            GUILayout.Label("BuildDuration", GUILayout.Width(80f));
            entry.BuildDuration = EditorGUILayout.IntField(entry.BuildDuration, GUILayout.Width(50f));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Space(20f);
            GUILayout.Label("UnitLabel", GUILayout.Width(60f));
            entry.Label = (UnitLabel)EditorGUILayout.EnumFlagsField(entry.Label, GUILayout.Width(80f));
            GUILayout.Space(10f);
            GUILayout.Label("MoveSpeed", GUILayout.Width(80f));
            entry.MoveSpeed = EditorGUILayout.FloatField(entry.MoveSpeed, GUILayout.Width(50f));
            GUILayout.Space(10f);
            GUILayout.Label("StealthTech", GUILayout.Width(80f));
            entry.StealthTechnology = EditorGUILayout.TextField(entry.StealthTechnology, GUILayout.Width(120f));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            if (position.width > 829.9f)
            {
                GUILayout.BeginHorizontal();
                GUILayout.BeginVertical(GUILayout.Width(445f));
                PrintWeaponList(entry);
                GUILayout.EndVertical();
                GUILayout.BeginVertical(GUILayout.Width(380f));
                PrintGuardList(entry);
                GUILayout.EndVertical();
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }
            else
            {
                PrintWeaponList(entry);
                PrintGuardList(entry);
            }

            if (entry.GetHashCode() != hashCode)
                m_SaveDelay = 200;
        }
        private void PrintWeaponList(UnitTableEntryWrapper entry)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(120f);
            GUILayout.Label("Attack", GUILayout.Width(50f));
            GUILayout.Label("Label", GUILayout.Width(80f));
            GUILayout.Label("Speed", GUILayout.Width(60f));
            GUILayout.Label("Upgrade", GUILayout.Width(60f));
            GUILayout.Label("Technology", GUILayout.Width(100f));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            entry.Weapon0 = PrintWeapon("Weapon0", entry.Weapon0 as AttackWeaponWrapper);
            entry.Weapon1 = PrintWeapon("Weapon1", entry.Weapon1 as AttackWeaponWrapper);
            entry.Weapon2 = PrintWeapon("Weapon2", entry.Weapon2 as AttackWeaponWrapper);
        }
        private void PrintGuardList(UnitTableEntryWrapper entry)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(120f);
            GUILayout.Label("Defence", GUILayout.Width(60f));
            GUILayout.Label("Upgrade", GUILayout.Width(60f));
            GUILayout.Label("Technology", GUILayout.Width(100f));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            entry.Guard = PrintGuard("Guard", entry.Guard as GuardWrapper);
            entry.Shield = PrintGuard("Shield", entry.Shield as GuardWrapper);
        }
        private AttackWeaponWrapper PrintWeapon(string name, AttackWeaponWrapper weapon)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(20);
            GUILayout.Label(name, GUILayout.Width(60f));
            if (weapon is null && GUILayout.Button("+", GUILayout.Width(30f)))
                weapon = new AttackWeaponWrapper();
            else if (weapon != null && GUILayout.Button("-", GUILayout.Width(30f)))
                weapon = null;
            if (weapon == null)
            {
                GUILayout.EndHorizontal();
                return null;
            }

            weapon.Attack = EditorGUILayout.IntField(weapon.Attack, GUILayout.Width(50f));
            weapon.Label = (UnitLabel)EditorGUILayout.EnumFlagsField(weapon.Label, GUILayout.Width(80f));
            weapon.Speed = EditorGUILayout.FloatField(weapon.Speed, GUILayout.Width(50f));
            weapon.UpgradePreLevel = EditorGUILayout.IntField(weapon.UpgradePreLevel, GUILayout.Width(50f));
            weapon.Technology = GUILayout.TextField(weapon.Technology, GUILayout.Width(100f));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            return weapon;
        }
        private GuardWrapper PrintGuard(string name, GuardWrapper guard)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(20);
            GUILayout.Label(name, GUILayout.Width(60f));
            if (guard is null && GUILayout.Button("+", GUILayout.Width(30f)))
                guard = new GuardWrapper();
            else if (guard != null && GUILayout.Button("-", GUILayout.Width(30f)))
                guard = null;
            if (guard == null)
            {
                GUILayout.EndHorizontal();
                return null;
            }

            guard.Defence = EditorGUILayout.IntField(guard.Defence, GUILayout.Width(60f));
            guard.UpgradePreLevel = EditorGUILayout.IntField(guard.UpgradePreLevel, GUILayout.Width(60f));
            guard.Technology = GUILayout.TextField(guard.Technology, GUILayout.Width(100f));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            return guard;
        }

        private void AddNewUnitEntry()
        {
            int randomID;
            do
            {
                randomID = UnityEngine.Random.Range(1, int.MaxValue);
            } while (m_UnitTable.Data.ContainsKey(randomID));
            UnitTableEntryWrapper entry = new UnitTableEntryWrapper()
            {
                ID = randomID,
                Annotation = m_NewEntryAnnotation,
            };
            JSONObject @object = JSONMap.ToJSON(entry);
            m_UnitTable.InsertEntry_Editor(JSONMap.ParseJSON<UnitTable.Entry>(@object));
            m_NewEntryAnnotation = string.Empty;
            m_SelectedUnitSet.Add(entry.ID);
            m_InShowList.Add(entry);
        }
        private void CopyNewUnitEntry()
        {
            int randomID;
            do
            {
                randomID = UnityEngine.Random.Range(1, int.MaxValue);
            } while (m_UnitTable.Data.ContainsKey(randomID));
            UnitTable.Entry entry = m_InShowList.FirstOrDefault(e => e.ID == m_NewEntryCopyID) ?? m_UnitTable.Data[m_NewEntryCopyID];
            UnitTableEntryWrapper newEntry = JSONMap.ParseJSON<UnitTableEntryWrapper>(JSONMap.ToJSON(entry));
            newEntry.ID = randomID;
            m_SelectedUnitSet.Add(newEntry.ID);
            m_InShowList.Add(newEntry);
        }

        private void FilterTable()
        {
            HashSet<int> idSet = new HashSet<int>();
            m_InShowList.Clear();
            UnitTable.Entry[] entries = m_UnitTable.Data.Values.OrderBy(entry => entry.ID).ToArray();
            for (int i = 0; i < entries.Length; i++)
            {
                UnitTable.Entry entry = entries[i];
                if (m_SelectedUnitSet.Contains(entry.ID))
                    idSet.Add(entries[i].ID);
            }
            if (string.IsNullOrWhiteSpace(m_SearchText))
            {
                for (int i = 0; i < entries.Length && i < 200; i++)
                    idSet.Add(entries[i].ID);
            }
            else
            {
                Regex regex = new Regex(m_SearchText, RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);
                for (int i = 0; i < entries.Length; i++)
                {
                    UnitTable.Entry entry = entries[i];
                    if (regex.IsMatch(entry.ID.ToString()) ||
                        regex.IsMatch(entry.Name.Key) ||
                        regex.IsMatch(entry.Annotation))
                    {
                        idSet.Add(entry.ID);
                    }
                }
            }
            foreach (int id in idSet)
            {
                JSONObject @object = JSONMap.ToJSON(m_UnitTable.Data[id]);
                m_InShowList.Add(JSONMap.ParseJSON<UnitTableEntryWrapper>(@object));
            }
            m_ScrollPosition = Vector2.zero;
        }

        private void OverrideTable()
        {
            for (int i = 0; i < m_InShowList.Count; i++)
            {
                JSONObject @object = JSONMap.ToJSON(m_InShowList[i]);
                UnitTable.Entry entry = JSONMap.ParseJSON<UnitTable.Entry>(@object);
                m_UnitTable.InsertEntry_Editor(entry);
            }
        }
        private void SaveUnitTable()
        {
            OverrideTable();
            if (string.IsNullOrWhiteSpace(m_UnitTablePath))
            {
                Debug.LogError("Save failed");
                return;
            }
            string content = JSONMap.ToJSON(m_UnitTable).ToString();
            File.WriteAllText(m_UnitTablePath, content);
            string localUnitTablePath = $"{GameDefined.LocalResourceDirectory}/Tables/UnitTable.json";
            Directory.CreateDirectory(Path.GetDirectoryName(localUnitTablePath));
            File.Copy(m_UnitTablePath, localUnitTablePath, true);
        }
    }
}