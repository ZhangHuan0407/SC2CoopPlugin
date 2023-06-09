﻿#if CHINESE_GUI
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
using System.Text.RegularExpressions;
using Table;
using UnityEditor;
using UnityEngine;

namespace Game.Editor
{
    public class UnitTableWindow : EditorWindow
    {
        private static Dictionary<string, Texture> m_UnitTextureCache = new Dictionary<string, Texture>();
        private static List<string> m_TryLoadList = new List<string>();

        public UnitTable UnitTable => EditorTableManager.UnitTable;

        private int m_SaveDelay;

        private string m_SearchText;
        private CommanderName m_FilterCommander;
        private UnitLabel m_FilterLabel;
        private HashSet<int> m_SelectedUnitSet;
        private List<UnitTableEntryWrapper> m_InShowList;
        private HashSet<int> m_UnfoldSet;

        private Vector2 m_ScrollPosition;

        private string m_NewEntryAnnotation;
        private int m_NewEntryCopyID;

        [MenuItem("Tools/Unit Table Edit", priority = 41)]
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
            EditorTableManager.Refresh();

            titleContent = new GUIContent("Unit Table Edit");
            minSize = new Vector2(550f, 400f);

            m_SaveDelay = -1;
            m_SearchText = string.Empty;
            m_SelectedUnitSet = new HashSet<int>();
            m_InShowList = new List<UnitTableEntryWrapper>();
            m_UnfoldSet = new HashSet<int>();
            m_ScrollPosition = Vector2.zero;
            FilterTable();
        }

        private void OnInspectorUpdate()
        {
            titleContent.text = $"Unit Table Edit {(m_SaveDelay > 0 ? "*" : string.Empty)}";
            if (m_SaveDelay-- == 0)
            {
                SaveUnitTable();
            }
            Repaint();
            if (m_TryLoadList.Count > 0)
            {
                string textureName = m_TryLoadList[0];
                Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>($"Assets/Resources/Textures/{textureName}.png");
                if (texture != null)
                    m_UnitTextureCache[textureName] = texture;
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
            m_FilterLabel = (UnitLabel)EditorGUILayout.EnumFlagsField((UnitLabelWrapper)m_FilterLabel, GUILayout.Width(80f));
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

            // Unit Table Head
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Lock", GUILayout.Width(38f)))
                EditorApplication.delayCall += () =>
                {
                    m_SelectedUnitSet.Clear();
                };
            GUILayout.Label("ID", GUILayout.Width(62f));
            GUILayout.Label("Name", GUILayout.Width(150f));
            GUILayout.Label("Annotation", GUILayout.Width(250f));
            if (GUILayout.Button("FoldAll", GUILayout.Width(65f)))
                EditorApplication.delayCall += () =>
                {
                    m_UnfoldSet.Clear();
                };
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            // Unit Table Entries
            m_ScrollPosition = GUILayout.BeginScrollView(m_ScrollPosition, GUILayout.MinHeight(300f), GUILayout.MaxHeight(800f));
            for (int i = 0; i < m_InShowList.Count; i++)
            {
                PrintUnitEntry(m_InShowList[i]);
            }
            GUILayout.Space(40f);
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
            int hashCode = entry.GetHashCode();
            if (m_UnfoldSet.Contains(entry.ID))
            {
                if (GUILayout.Button(entry.ID.ToString(), GUILayout.Width(80f)))
                    m_NewEntryCopyID = entry.ID;
            }
            else
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
            entry.Commander = (CommanderName)EditorGUILayout.EnumPopup((CommanderWrapper)entry.Commander, GUILayout.Width(100f));
            GUILayout.Space(10f);
            GUILayout.Label("UnlockLevel", GUILayout.MinWidth(80f));
            entry.UnlockLevel = EditorGUILayout.IntField(entry.UnlockLevel, GUILayout.Width(50f));
            GUILayout.Space(10f);
            GUILayout.Label("HP", GUILayout.MinWidth(30f));
            entry.HP = EditorGUILayout.IntField(entry.HP, GUILayout.Width(50f));
            GUILayout.Space(10f);
            GUILayout.Label("HP2", GUILayout.MinWidth(30f));
            entry.HP2 = EditorGUILayout.IntField(entry.HP2, GUILayout.Width(50f));
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
            GUILayout.Space(10f);
            GUILayout.Label("Texture", GUILayout.MinWidth(40f));
            entry.Texture = EditorGUILayout.TextField(entry.Texture, GUILayout.Width(75f));
            if (!string.IsNullOrWhiteSpace(entry.Texture))
            {
                m_UnitTextureCache.TryGetValue(entry.Texture, out Texture texture);
                if (texture == null)
                    m_TryLoadList.Add(entry.Texture);
                GUILayout.Label(texture, GUILayout.Width(80f), GUILayout.Height(80f));
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Space(20f);
            GUILayout.Label("UnitLabel", GUILayout.Width(60f));
            entry.Label = (UnitLabel)EditorGUILayout.EnumFlagsField((UnitLabelWrapper)entry.Label, GUILayout.Width(80f));
            GUILayout.Space(10f);
            GUILayout.Label("MoveSpeed", GUILayout.Width(80f));
            entry.MoveSpeed = EditorGUILayout.FloatField(entry.MoveSpeed, GUILayout.Width(50f));
            GUILayout.Space(10f);
            GUILayout.Label("StealthTech", GUILayout.Width(80f));
            entry.StealthTechnology = EditorGUILayout.TextField(entry.StealthTechnology, GUILayout.Width(120f));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            if (position.width > 909.9f)
            {
                GUILayout.BeginHorizontal();
                GUILayout.BeginVertical(GUILayout.Width(475f));
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

            if (m_SaveDelay < 0 && entry.GetHashCode() != hashCode)
                m_SaveDelay = 200;
        }
        private void PrintWeaponList(UnitTableEntryWrapper entry)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(120f);
            GUILayout.Label("Attack", GUILayout.Width(50f));
            GUILayout.Label("Multiple", GUILayout.Width(60f));
            GUILayout.Label("Label", GUILayout.Width(76f));
            GUILayout.Label("Speed", GUILayout.Width(50f));
            GUILayout.Label("Range", GUILayout.Width(50f));
            GUILayout.Label("Upgrade", GUILayout.Width(60f));
            GUILayout.Label("Technology", GUILayout.Width(120f));
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
            GUILayout.Label("Technology", GUILayout.Width(120f));
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
            weapon.Multiple = EditorGUILayout.IntField(weapon.Multiple, GUILayout.Width(60f));
            weapon.Label = (UnitLabel)EditorGUILayout.EnumFlagsField((UnitLabelWrapper)weapon.Label, GUILayout.Width(80f));
            weapon.Speed = EditorGUILayout.FloatField(weapon.Speed, GUILayout.Width(50f));
            weapon.Range = EditorGUILayout.FloatField(weapon.Range, GUILayout.Width(50f));
            weapon.UpgradePreLevel = EditorGUILayout.IntField(weapon.UpgradePreLevel, GUILayout.Width(50f));
            weapon.Technology = GUILayout.TextField(weapon.Technology, GUILayout.Width(200f));
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
            guard.Technology = GUILayout.TextField(guard.Technology, GUILayout.Width(200f));
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
            } while (UnitTable.Data.ContainsKey(randomID));
            UnitTableEntryWrapper entry = new UnitTableEntryWrapper()
            {
                ID = randomID,
                Annotation = m_NewEntryAnnotation,
            };
            JSONObject @object = JSONMap.ToJSON(entry);
            UnitTable.OverrideEntry_Editor(JSONMap.ParseJSON<UnitTable.Entry>(@object));
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
            } while (UnitTable.Data.ContainsKey(randomID));
            UnitTable.Entry entry = m_InShowList.FirstOrDefault(e => e.ID == m_NewEntryCopyID) ?? UnitTable.Data[m_NewEntryCopyID];
            if (entry is null)
                Debug.LogError("not found " + m_NewEntryCopyID);
            UnitTableEntryWrapper newEntry = JSONMap.ParseJSON<UnitTableEntryWrapper>(JSONMap.ToJSON(entry));
            newEntry.ID = randomID;
            m_SelectedUnitSet.Add(newEntry.ID);
            m_InShowList.Add(newEntry);
        }

        private void FilterTable()
        {
            m_TryLoadList.Clear();
            HashSet<int> idSet = new HashSet<int>();
            m_InShowList.Clear();
            UnitTable.Entry[] entries = UnitTable.Data.Values.OrderBy(entry => entry.ID).ToArray();
            for (int i = 0; i < entries.Length; i++)
            {
                UnitTable.Entry entry = entries[i];
                if (m_SelectedUnitSet.Contains(entry.ID))
                    idSet.Add(entries[i].ID);
            }
            if (string.IsNullOrWhiteSpace(m_SearchText) &&
                m_FilterCommander == CommanderName.None &&
                m_FilterLabel == UnitLabel.None)
            {
                for (int i = 0; i < entries.Length; i++)
                    idSet.Add(entries[i].ID);
            }
            else
            {
                Regex regex = new Regex(m_SearchText, RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);
                for (int i = 0; i < entries.Length; i++)
                {
                    UnitTable.Entry entry = entries[i];
                    if (m_FilterCommander != CommanderName.None &&
                        (entry.Commander != m_FilterCommander))
                        continue;
                    if (m_FilterLabel != UnitLabel.None &&
                        (entry.Label & m_FilterLabel) == 0)
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
                UnitTable.Entry entry = UnitTable.Data[id];
                JSONObject @object = JSONMap.ToJSON(entry);
                m_InShowList.Add(JSONMap.ParseJSON<UnitTableEntryWrapper>(@object));
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
                UnitTable.Entry entry = JSONMap.ParseJSON<UnitTable.Entry>(@object);
                UnitTable.OverrideEntry_Editor(entry);
            }
        }
        private void SaveUnitTable()
        {
            OverrideTable();
            JSONObject @table = JSONMap.ToJSON(UnitTable);
            for (int i = 0; i < @table.list.Count; i++)
                @table.list[i].Bake(true);
            EditorTableManager.SaveTable<UnitTable>(@table);
        }
    }
}