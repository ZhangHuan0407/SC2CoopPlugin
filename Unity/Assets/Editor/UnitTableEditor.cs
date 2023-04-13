﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Table;
using UnityEditor;
using UnityEngine;

namespace Game.Editor
{
    public class UnitTableWindow : EditorWindow
    {
        private const string UnitTablePathKey = "UnitTablePathKey";
        
        private UnitTable m_UnitTable;
        public UnitTable UnitTable => m_UnitTable;

        private int m_SaveDelay;
        private string m_UnitTablePath;

        private string m_SearchText;
        private HashSet<int> m_SelectedUnitSet;
        private List<UnitTableEntryWrapper> m_InShowList;
        private HashSet<int> m_UnfoldSet;

        private Vector2 m_ScrollPosition;

        private string m_NewEntryAnnotation;

        [MenuItem("Tools/Unit Table Edit")]
        public static UnitTableWindow OpenWindow()
        {
            UnitTableWindow editorWindow = GetWindow<UnitTableWindow>();
            return editorWindow;
        }

        private void OnEnable()
        {
            titleContent = new GUIContent("Unit Table Edit");
            minSize = new Vector2(500f, 400f);

            m_UnitTablePath = EditorPrefs.GetString(UnitTablePathKey);
            if (File.Exists(m_UnitTablePath))
            {
                string content = File.ReadAllText(m_UnitTablePath);
                m_UnitTable = JSONMap.ParseJSON<UnitTable>(JSONObject.Create(content));
            }
            else
                m_UnitTable = null;

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
            string path = GUILayout.TextField(m_UnitTablePath, GUILayout.MinWidth(400f));
            if (path != m_UnitTablePath)
            {
                m_UnitTablePath = path;
                EditorPrefs.SetString(UnitTablePathKey, path);
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(15f);
            if (m_UnitTable == null)
            {
                GUILayout.Label("设置UnitTable.json路径后重启窗体");
                return;
            }

            GUILayout.BeginHorizontal();
            GUILayout.Label("Regex", GUILayout.Width(80f));
            m_SearchText = GUILayout.TextField(m_SearchText, GUILayout.MinWidth(250f));
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
            GUILayout.EndHorizontal();

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
        }

        private void PrintUnitEntry(UnitTableEntryWrapper entry)
        {
            GUILayout.BeginHorizontal();
            bool isSelected = m_SelectedUnitSet.Contains(entry.ID);
            bool nowSelect = GUILayout.Toggle(isSelected, string.Empty, GUILayout.Width(25f));
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
            if (m_UnfoldSet.Contains(entry.ID))
            {
                GUILayout.FlexibleSpace();
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
                GUILayout.Label(entry.Name.Key, GUILayout.Width(200f));
                GUILayout.Label(entry.Annotation);
                GUILayout.FlexibleSpace();
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
            GUILayout.Label("Name", GUILayout.MinWidth(80f));
            StringID nameStringID = entry.Name;
            nameStringID.Key = GUILayout.TextField(nameStringID.Key, GUILayout.MinWidth(150f));
            GUILayout.FlexibleSpace();
            entry.Name = nameStringID;
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Space(20f);
            GUILayout.Label("Annotation", GUILayout.MinWidth(80f));
            entry.Annotation = GUILayout.TextField(entry.Annotation, GUILayout.MinWidth(300f));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Space(20f);
            GUILayout.Label("Commander", GUILayout.MinWidth(80f));
            entry.Commander = (Commander)EditorGUILayout.EnumPopup(entry.Commander, GUILayout.Width(100f));
            GUILayout.Space(10f);
            GUILayout.Label("UnlockLevel", GUILayout.MinWidth(80f));
            entry.UnlockLevel = EditorGUILayout.IntField(entry.UnlockLevel, GUILayout.Width(100f));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Space(20f);
            GUILayout.Label("Crystal", GUILayout.Width(60f));
            entry.CrystalCost = EditorGUILayout.IntField(entry.CrystalCost, GUILayout.Width(100f));
            GUILayout.Space(10f);
            GUILayout.Label("Gas", GUILayout.Width(60f));
            entry.GasCost = EditorGUILayout.IntField(entry.GasCost, GUILayout.Width(100f));
            GUILayout.Space(10f);
            GUILayout.Label("Population", GUILayout.Width(60f));
            entry.Population = EditorGUILayout.FloatField(entry.Population, GUILayout.Width(100f));
            GUILayout.Space(10f);
            GUILayout.Label("BuildDuration", GUILayout.Width(60f));
            entry.BuildDuration = EditorGUILayout.IntField(entry.BuildDuration, GUILayout.Width(100f));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Space(20f);
            GUILayout.Label("UnitLabel", GUILayout.Width(60f));
            entry.Label = (UnitLabel)EditorGUILayout.EnumFlagsField(entry.Label, GUILayout.Width(100f));
            GUILayout.Space(10f);
            GUILayout.Label("StealthTech", GUILayout.Width(80f));
            entry.StealthTechnology = EditorGUILayout.TextField(entry.StealthTechnology, GUILayout.Width(150f));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            if (entry.GetHashCode() != hashCode)
                m_SaveDelay = 200;
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

        private void FilterTable()
        {
            List<int> idList = new List<int>();
            m_InShowList.Clear();
            UnitTable.Entry[] entries = m_UnitTable.Data.Values.OrderBy(entry => entry.ID).ToArray();
            for (int i = 0; i < entries.Length; i++)
            {
                UnitTable.Entry entry = entries[i];
                if (m_SelectedUnitSet.Contains(entry.ID))
                    idList.Add(entries[i].ID);
            }
            if (string.IsNullOrWhiteSpace(m_SearchText))
            {
                for (int i = 0; i < entries.Length && i < 200; i++)
                    idList.Add(entries[i].ID);
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
                        idList.Add(entry.ID);
                    }
                }
            }
            idList.Sort();
            for (int i = 0; i < idList.Count; i++)
            {
                JSONObject @object = JSONMap.ToJSON(m_UnitTable.Data[idList[i]]);
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
            string localUnitTablePath = $"{Application.streamingAssetsPath}/{GameDefined.LocalResourceDirectory}/Tables/UnitTable.json";
            Directory.CreateDirectory(Path.GetDirectoryName(localUnitTablePath));
            File.Copy(m_UnitTablePath, localUnitTablePath, true);
        }
    }
}