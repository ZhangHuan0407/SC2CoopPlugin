#if CHINESE_GUI
using CommanderWrapper = Game.Editor.CommanderChinese;
using AmonAINameWrapper = Game.Editor.AmonAINameChinese;
using MapNameWrapper = Game.Editor.MapNameChinese;
using UnitLabelWrapper = Game.Editor.UnitLabelChinese;
#else
using CommanderWrapper = Table.CommanderName;
using AmonAINameWrapper = Table.AmonAIName;
using MapNameWrapper = Table.MapName;
using UnitLabelWrapper = Table.UnitLabel;
#endif

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Table;
using UnityEditor;
using UnityEngine;
using System.Text.RegularExpressions;

namespace Game.Editor
{
    public class TechnologyTableWindow : EditorWindow
    {
        private static Dictionary<string, Texture> m_TechTextureCache = new Dictionary<string, Texture>();
        private static List<string> m_TryLoadList = new List<string>();

        private int m_SaveDelay;

        private string m_SearchText;
        private CommanderName m_FilterCommander;
        private HashSet<int> m_SelectedSet;
        private List<TechnologyTableEntryWrapper> m_InShowList;

        private Vector2 m_ScrollPosition;

        private int m_NewEntryCopyID;

        [MenuItem("Tools/Tech Table Edit")]
        public static TechnologyTableWindow OpenWindow()
        {
            TechnologyTableWindow editorWindow = GetWindow<TechnologyTableWindow>();
            var rect = editorWindow.position;
            rect.width = 830f;
            editorWindow.position = rect;
            return editorWindow;
        }

        private void OnEnable()
        {
            EditorTableManager.Refresh();

            titleContent = new GUIContent("Tech Table Edit");
            minSize = new Vector2(550f, 400f);

            m_SaveDelay = -1;
            m_SearchText = string.Empty;
            m_SelectedSet = new HashSet<int>();
            m_InShowList = new List<TechnologyTableEntryWrapper>();
            m_ScrollPosition = Vector2.zero;
            FilterTable();
        }

        private void OnInspectorUpdate()
        {
            titleContent.text = $"Tech Table Edit {(m_SaveDelay > 0 ? "*" : string.Empty)}";
            if (m_SaveDelay-- == 0)
            {
                SaveTechTable();
            }
            Repaint();
            if (m_TryLoadList.Count > 0)
            {
                string textureName = m_TryLoadList[0];
                Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>($"Assets/Resources/Textures/{textureName}.png");
                if (texture != null)
                    m_TechTextureCache[textureName] = texture;
                m_TryLoadList.RemoveAll((str) => str == textureName);
            }
        }

        private void OnGUI()
        {
            // Tool
            GUILayout.BeginHorizontal();
            GUILayout.Label("Regex", GUILayout.Width(80f));
            m_SearchText = GUILayout.TextField(m_SearchText, GUILayout.MinWidth(150f), GUILayout.MaxWidth(300f));
            m_FilterCommander = (CommanderName)EditorGUILayout.EnumPopup((CommanderWrapper)m_FilterCommander, GUILayout.Width(100f));
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
                SaveTechTable();
                m_SaveDelay = -1;
            }
            GUILayout.EndHorizontal();

            // Tech Table Head
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Lock", GUILayout.Width(38f)))
                EditorApplication.delayCall += () =>
                {
                    m_SelectedSet.Clear();
                };
            GUILayout.Label("ID", GUILayout.Width(62f));
            GUILayout.Label("Name", GUILayout.Width(150f));
            GUILayout.Label("Commander", GUILayout.Width(100f));
            GUILayout.Label("Annotation", GUILayout.Width(250f));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            // Unit Table Entries
            m_ScrollPosition = GUILayout.BeginScrollView(m_ScrollPosition, GUILayout.MinHeight(300f), GUILayout.MaxHeight(800f));
            for (int i = 0; i < m_InShowList.Count; i++)
            {
                PrintTechEntry(m_InShowList[i]);
            }
            GUILayout.Space(40f);
            GUILayout.EndScrollView();

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

        private void PrintTechEntry(TechnologyTableEntryWrapper entry)
        {
            int hashCode = entry.GetHashCode();
            GUILayout.BeginHorizontal();
            bool isSelected = m_SelectedSet.Contains(entry.ID);
            bool nowSelect = GUILayout.Toggle(isSelected, string.Empty, GUILayout.Width(20f));
            if (nowSelect != isSelected)
            {
                EditorApplication.delayCall += () =>
                {
                    if (nowSelect)
                        m_SelectedSet.Add(entry.ID);
                    else
                        m_SelectedSet.Remove(entry.ID);
                };
            }
            if (GUILayout.Button(entry.ID.ToString(), GUILayout.Width(80f)))
                m_NewEntryCopyID = entry.ID;
            StringID nameStringID = entry.Name;
            nameStringID.Key = GUILayout.TextField(nameStringID.Key, GUILayout.Width(150f));
            entry.Name = nameStringID;
            entry.Commander = (CommanderName)EditorGUILayout.EnumPopup((CommanderWrapper)entry.Commander, GUILayout.Width(100f));
            entry.Annotation = GUILayout.TextField(entry.Annotation, GUILayout.Width(450f));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("CrystalCost", GUILayout.Width(80f));
            entry.CrystalCost = EditorGUILayout.IntField(entry.CrystalCost, GUILayout.Width(100f));
            GUILayout.Label("GasCost", GUILayout.Width(80f));
            entry.GasCost = EditorGUILayout.IntField(entry.GasCost, GUILayout.Width(80f));
            GUILayout.Label("Duration", GUILayout.Width(80f));
            entry.Duration = EditorGUILayout.IntField(entry.Duration, GUILayout.Width(80f));
            GUILayout.Label("Unlock", GUILayout.Width(80f));
            entry.UnlockLevel = EditorGUILayout.IntField(entry.UnlockLevel, GUILayout.Width(80f));

            entry.Unit0 = EditorGUILayout.IntField(entry.Unit0, GUILayout.Width(150f));
            entry.Unit1 = EditorGUILayout.IntField(entry.Unit1, GUILayout.Width(150f));
            entry.Texture = EditorGUILayout.TextField(entry.Texture, GUILayout.Width(180f));
            if (!string.IsNullOrWhiteSpace(entry.Texture))
            {
                m_TechTextureCache.TryGetValue(entry.Texture, out Texture texture);
                if (texture == null)
                    m_TryLoadList.Add(entry.Texture);
                GUILayout.Label(texture, GUILayout.Width(80f), GUILayout.Height(80f));
            }

            GUILayout.EndHorizontal();

            if (m_SaveDelay < 0 && entry.GetHashCode() != hashCode)
                m_SaveDelay = 200;
        }

        private void CopyNewUnitEntry()
        {
            int randomID;
            do
            {
                randomID = UnityEngine.Random.Range(1, int.MaxValue);
            } while (EditorTableManager.TechnologyTable.Data.ContainsKey(randomID));
            TechnologyTable.Entry entry = m_InShowList.FirstOrDefault(e => e.ID == m_NewEntryCopyID) ?? EditorTableManager.TechnologyTable.Data[m_NewEntryCopyID];
            if (entry is null)
                Debug.LogError("not found " + m_NewEntryCopyID);
            TechnologyTableEntryWrapper newEntry = JSONMap.ParseJSON<TechnologyTableEntryWrapper>(JSONMap.ToJSON(entry));
            newEntry.ID = randomID;
            m_SelectedSet.Add(newEntry.ID);
            m_InShowList.Add(newEntry);
        }

        private void FilterTable()
        {
            HashSet<int> idSet = new HashSet<int>();
            m_InShowList.Clear();
            TechnologyTable.Entry[] entries = EditorTableManager.TechnologyTable.Data.Values.OrderBy(entry => entry.ID).ToArray();
            for (int i = 0; i < entries.Length; i++)
            {
                TechnologyTable.Entry entry = entries[i];
                if (m_SelectedSet.Contains(entry.ID))
                    idSet.Add(entries[i].ID);
            }
            if (string.IsNullOrWhiteSpace(m_SearchText) &&
                m_FilterCommander == CommanderName.None)
            {
                for (int i = 0; i < entries.Length; i++)
                    idSet.Add(entries[i].ID);
            }
            else
            {
                Regex regex = new Regex(m_SearchText, RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);
                for (int i = 0; i < entries.Length; i++)
                {
                    TechnologyTable.Entry entry = entries[i];
                    if (m_FilterCommander != CommanderName.None &&
                        (entry.Commander != m_FilterCommander))
                        continue;
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
                TechnologyTable.Entry entry = EditorTableManager.TechnologyTable.Data[id];
                JSONObject @object = JSONMap.ToJSON(entry);
                m_InShowList.Add(JSONMap.ParseJSON<TechnologyTableEntryWrapper>(@object));
                if (m_InShowList.Count > 150)
                    break;
            }
            m_ScrollPosition = Vector2.zero;
        }

        private void OverrideTable()
        {
            for (int i = 0; i < m_InShowList.Count; i++)
            {
                JSONObject @object = JSONMap.ToJSON(m_InShowList[i]);
                TechnologyTable.Entry entry = JSONMap.ParseJSON<TechnologyTable.Entry>(@object);
                EditorTableManager.TechnologyTable.OverrideEntry_Editor(entry);
            }
        }
        private void SaveTechTable()
        {
            OverrideTable();
            JSONObject @table = JSONMap.ToJSON(EditorTableManager.TechnologyTable);
            for (int i = 0; i < @table.list.Count; i++)
                @table.list[i].Bake(true);
            string content = @table.ToString(true);
            EditorTableManager.SaveTable<TechnologyTable>(@table);
        }
    }
}