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
        private static AttackWaveTable m_AttackWaveTable;
        private static PrestigeTable m_PrestigeTable;
        private static UnitTable m_UnitTable;
        private static TechnologyTable m_TechnologyTable;

        private static void LoadInnerTables()
        {
            m_AttackWaveTable = LoadTable<AttackWaveTable>("AttackWaveTable.json");
            m_PrestigeTable = LoadTable<PrestigeTable>("PrestigeTable.json");
            m_UnitTable = LoadTable<UnitTable>("UnitTable.json");
            m_TechnologyTable = LoadTable<TechnologyTable>("TechnologyTable.json");

            T LoadTable<T>(string tableName)
            {
                string content = File.ReadAllText($"{GameDefined.ResourceSubmoduleDirectory}/Tables/{tableName}");
                return JSONMap.ParseJSON<T>(JSONObject.Create(content));
            }
        }
        [MenuItem("Tools/Merge StringID")]
        public static void MergeStringID()
        {
            LoadInnerTables();
            HashSet<string> stringIDSet = new HashSet<string>();
            AppendEnum<AmonAIName>();
            AppendEnum<Commander>();
            AppendEnum<MapName>();
            AppendEnum<UnitLabel>();

            foreach (PrestigeTable.Entry[] entries in m_PrestigeTable.Data.Values)
            {
                for (int i = 0; i < entries.Length; i++)
                {
                    PrestigeTable.Entry entry = entries[i];
                    stringIDSet.Add(entry.Name.Key);
                    stringIDSet.Add(entry.Describe.Key);
                }
            }

            foreach (UnitTable.Entry entry in m_UnitTable.Data.Values)
            {
                stringIDSet.Add(entry.Name.Key);
            }

            foreach (TechnologyTable.Entry entry in m_TechnologyTable.Data.Values)
            {
                stringIDSet.Add(entry.Name.Key);
                stringIDSet.Add(entry.Describe.Key);
            }

            Debug.Log($"total string id: {stringIDSet.Count}");
            foreach (string filePath in Directory.GetFiles($"{GameDefined.LocalResourceDirectory}/Localization", "*.json"))
            {
                string languageName = Path.GetFileNameWithoutExtension(filePath);
                if (!Enum.TryParse<SystemLanguage>(languageName, out SystemLanguage language))
                    continue;
                JSONObject @object = JSONObject.Create(File.ReadAllText(filePath));
                SortedDictionary<string, JSONObject> sortedDictionary = new SortedDictionary<string, JSONObject>(@object.dictionary);
                foreach (string stringID in stringIDSet)
                {
                    if (!sortedDictionary.ContainsKey(stringID))
                        sortedDictionary.Add(stringID, JSONObject.CreateStringObject(string.Empty));
                }
                @object.dictionary = new Dictionary<string, JSONObject>(sortedDictionary);
                File.WriteAllText(filePath, @object.ToString(true));
            }

            void AppendEnum<T>() where T : Enum
            {
                string[] names = Enum.GetNames(typeof(T)) as string[];
                for (int i = 0; i < names.Length; i++)
                    stringIDSet.Add($"{typeof(T).Name}.{names[i]}");
            }
        }
    }
}