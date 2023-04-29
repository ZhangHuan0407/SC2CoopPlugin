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
        private static MasteriesTable m_MasteriesTable;
        private static UnitTable m_UnitTable;
        private static TechnologyTable m_TechnologyTable;

        private static void LoadInnerTables()
        {
            for (int i = 0; i < GameDefined.JSONSerializedRegisterTypes.Length; i++)
                JSONMap.RegisterType(GameDefined.JSONSerializedRegisterTypes[i]);

            m_AttackWaveTable = LoadTable<AttackWaveTable>("AttackWaveTable.json");
            m_MasteriesTable = LoadTable<MasteriesTable>("MasteriesTable.json");
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
            AppendEnum<CommanderName>();
            AppendEnum<MapName>();
            AppendEnum<SystemLanguage>();
            AppendEnum<UnitLabel>();

            foreach (MasteriesTable.Entry[] entries in m_MasteriesTable.Data.Values)
            {
                for (int i = 0; i < entries.Length; i++)
                {
                    MasteriesTable.Entry entry = entries[i];
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

        //[MenuItem("Tools/Unused/Fix Table")]
        //public static void Fix()
        //{
        //    LoadInnerTables();
        //    Dictionary<string, string> fixMap = new Dictionary<string, string>();
        //    foreach (UnitTable.Entry entry in m_UnitTable.Data.Values)
        //    {
        //        string before, after;
        //        if (entry.Label.HasFlag(UnitLabel.Building))
        //        {
        //            var a = entry.Name;
        //            before = a.Key;
        //            a.Key = $"Building.{a.Key}";
        //            after = a.Key;
        //            entry.Name = a;
        //        }
        //        else
        //        {
        //            var a = entry.Name;
        //            before = a.Key;
        //            a.Key = $"Unit.{a.Key}";
        //            after = a.Key;
        //            entry.Name = a;
        //        }
        //        fixMap[before] = after;
        //    }

        //    JSONObject @table = JSONMap.ToJSON(m_UnitTable);
        //    for (int i = 0; i < @table.list.Count; i++)
        //        @table.list[i].Bake(true);
        //    string content = @table.ToString(true);
        //    File.WriteAllText($"{GameDefined.ResourceSubmoduleDirectory}/Tables/UnitTable.json", content);

        //    string submoduleLocalizationDirectory = $"{GameDefined.ResourceSubmoduleDirectory}/Localization";
        //    foreach (string filePath in Directory.GetFiles(submoduleLocalizationDirectory, "*.json"))
        //    {
        //        string languageName = Path.GetFileNameWithoutExtension(filePath);
        //        if (!Enum.TryParse<SystemLanguage>(languageName, out SystemLanguage language))
        //            continue;
        //        JSONObject @object = JSONObject.Create(File.ReadAllText(filePath));
        //        SortedDictionary<string, JSONObject> sortedDictionary = new SortedDictionary<string, JSONObject>(@object.dictionary);
        //        foreach (var pair in fixMap)
        //        {
        //            if (sortedDictionary.ContainsKey(pair.Key))
        //            {
        //                JSONObject str = sortedDictionary[pair.Key];
        //                sortedDictionary.Remove(pair.Key);
        //                sortedDictionary[pair.Value] = str;
        //            }
        //        }
        //        @object.dictionary = new Dictionary<string, JSONObject>(sortedDictionary);
        //        File.WriteAllText(filePath, @object.ToString(true));
        //    }
        //}
    }
}