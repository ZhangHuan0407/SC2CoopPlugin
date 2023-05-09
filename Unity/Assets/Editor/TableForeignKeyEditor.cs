using System;
using UnityEditor;
using Table;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

namespace Game.Editor
{
    public static class TableForeignKeyEditor
    {
        [MenuItem("Tools/Merge StringID")]
        public static void MergeStringID()
        {
            EditorTableManager.Refresh();
            HashSet<string> stringIDSet = new HashSet<string>();
            AppendEnum<AmonAIName>();
            AppendEnum<CommanderName>();
            AppendEnum<MapName>();
            AppendEnum<MapSubType>();
            AppendEnum<SystemLanguage>();
            AppendEnum<UnitLabel>();

            foreach (MasteriesTable.Entry[] entries in EditorTableManager.MasteriesTable.Data.Values)
            {
                for (int i = 0; i < entries.Length; i++)
                {
                    MasteriesTable.Entry entry = entries[i];
                    stringIDSet.Add(entry.Name.Key);
                    stringIDSet.Add(entry.Describe.Key);
                }
            }

            foreach (PrestigeTable.Entry[] entries in EditorTableManager.PrestigeTable.Data.Values)
            {
                for (int i = 0; i < entries.Length; i++)
                {
                    PrestigeTable.Entry entry = entries[i];
                    stringIDSet.Add(entry.Name.Key);
                    stringIDSet.Add(entry.Describe.Key);
                }
            }

            foreach (UnitTable.Entry entry in EditorTableManager.UnitTable.Data.Values)
            {
                stringIDSet.Add(entry.Name.Key);
            }

            foreach (TechnologyTable.Entry entry in EditorTableManager.TechnologyTable.Data.Values)
            {
                stringIDSet.Add(entry.Name.Key);
                //stringIDSet.Add(entry.Describe.Key);
            }

            foreach (string line in File.ReadAllLines("Assets/Editor/UI.StringID.txt"))
            {
                stringIDSet.Add(line);
            }

            Debug.Log($"Total StringId Count: {stringIDSet.Count}");
            HashSet<string> needDeleteSet = new HashSet<string>();
            string submoduleLocalizationDirectory = $"{GameDefined.ResourceSubmoduleDirectory}/Localization";
            int maxPreviousCount = 0;
            int maxAfterEditCount = 0;
            foreach (string filePath in Directory.GetFiles(submoduleLocalizationDirectory, "*.json"))
            {
                string languageName = Path.GetFileNameWithoutExtension(filePath);
                if (!Enum.TryParse<SystemLanguage>(languageName, out SystemLanguage language))
                    continue;
                JSONObject @object = JSONObject.Create(File.ReadAllText(filePath));
                SortedDictionary<string, JSONObject> sortedDictionary = new SortedDictionary<string, JSONObject>(@object.dictionary);
                if (maxPreviousCount < sortedDictionary.Count)
                    maxPreviousCount = sortedDictionary.Count;
                foreach (string stringID in stringIDSet)
                {
                    if (!sortedDictionary.ContainsKey(stringID))
                        sortedDictionary.Add(stringID, JSONObject.CreateStringObject(string.Empty));
                }
                if (maxAfterEditCount < sortedDictionary.Count)
                    maxAfterEditCount = sortedDictionary.Count;
                @object.dictionary = new Dictionary<string, JSONObject>(sortedDictionary);
                File.WriteAllText(filePath, @object.ToString(true));
                foreach (string key in sortedDictionary.Keys)
                {
                    if (!stringIDSet.Contains(key))
                        needDeleteSet.Add(key);
                }
            }
            string localLocalizationDirectory = $"{GameDefined.LocalResourceDirectory}/Localization";
            if (Directory.Exists(localLocalizationDirectory))
                Directory.Delete(localLocalizationDirectory, true);
            new DirectoryInfo(submoduleLocalizationDirectory).CopyFilesTo(new DirectoryInfo(localLocalizationDirectory), false, "*.json");

            Debug.Log($"(PreviousMax {maxPreviousCount} Union {stringIDSet.Count}) => {maxAfterEditCount}");
            Debug.Log("Need Delete:\n" + string.Join("\n", needDeleteSet));

            void AppendEnum<T>() where T : Enum
            {
                string[] names = Enum.GetNames(typeof(T)) as string[];
                for (int i = 0; i < names.Length; i++)
                    stringIDSet.Add($"{typeof(T).Name}.{names[i]}");
            }
        }

        [MenuItem("Tools/Unused/Create Prestige Table")]
        public static void CreatePrestigeTable()
        {
            PrestigeTable table = new PrestigeTable();
            Dictionary<CommanderName, PrestigeTable.Entry[]> dictionary = table.Data as Dictionary<CommanderName, PrestigeTable.Entry[]>;
            foreach (CommanderName commanderName in Enum.GetValues(typeof(CommanderName)))
            {
                dictionary[commanderName] = new PrestigeTable.Entry[4];
                JSONObject @entry = JSONMap.ToJSON(new PrestigeTable.Entry());
                for (int t = 0; t < 4; t++)
                {
                    @entry.SetField("m_Commander", commanderName.ToString());
                    @entry.SetField("m_Name", $"{commanderName}_Prestige_{t}_Name");
                    @entry.SetField("m_Describe", $"{commanderName}_Prestige_{t}_Desc");
                    @entry.SetField("m_Index", t);
                    dictionary[commanderName][t] = JSONMap.ParseJSON<PrestigeTable.Entry>(@entry);
                }
            }
            string content = JSONMap.ToJSON(table).ToString(false);
            Debug.Log(content);
        }

        [MenuItem("Tools/Unused/Create Attack Wave Table")]
        public static void CreateAttackWaveTable()
        {
            AttackWaveTable table = new AttackWaveTable();
            Dictionary<AmonAIName, AttackWaveTable.Entry[]> dictionary = table.Data as Dictionary<AmonAIName, AttackWaveTable.Entry[]>;
            foreach (AmonAIName AIName in Enum.GetValues(typeof(AmonAIName)))
            {
                dictionary[AIName] = new AttackWaveTable.Entry[7];
                JSONObject @entry = JSONMap.ToJSON(new AttackWaveTable.Entry());
                for (int t = 0; t < 7; t++)
                {
                    @entry.SetField("m_AI", AIName.ToString());
                    @entry.SetField("m_Technology", t + 1);
                    @entry.SetField("m_UnitID", new JSONObject(JSONObject.Type.ARRAY));
                    dictionary[AIName][t] = JSONMap.ParseJSON<AttackWaveTable.Entry>(@entry);
                }
            }
            string content = JSONMap.ToJSON(table).ToString(false);
            Debug.Log(content);
        }
    }
}