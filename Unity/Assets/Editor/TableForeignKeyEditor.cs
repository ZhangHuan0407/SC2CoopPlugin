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
        [MenuItem("Tools/Merge StringID", priority = 40)]
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

        [MenuItem("Tools/Unused/Create Tech Table Entries")]
        public static void CreateTechTableEntries()
        {
            string[] allFiles = Directory.GetFiles("Assets/Resources/abc", "*.png");
            JSONObject @list = new JSONObject(JSONObject.Type.ARRAY);
            for (int i = 0; i < allFiles.Length; i++)
            {
                string filePath = allFiles[i];
                string fileName = Path.GetFileNameWithoutExtension(filePath);
                CommanderName? commanderName = null;
                //if (fileName.StartsWith("PA"))
                //commanderName = CommanderName.Artanis;
                // else if (fileName.StartsWith("PB"))
                //commanderName = CommanderName.Karax;
                //else if (fileName.StartsWith("PC"))
                //    commanderName = CommanderName.Alarak;
                //else if (fileName.StartsWith("PD"))
                //    commanderName = CommanderName.Fenix;
                //if (fileName.StartsWith("TB"))
                //    commanderName = CommanderName.Raynor;
                //else if (fileName.StartsWith("TC"))
                //    commanderName = CommanderName.Tychus;
                //else if (fileName.StartsWith("TD"))
                //    commanderName = CommanderName.HanAndHorner;
                //else if (fileName.StartsWith("TE"))
                //    commanderName = CommanderName.Mengsk;
                //else if (fileName.StartsWith("TF"))
                //    commanderName = CommanderName.Nova;
                //if (fileName.StartsWith("ZA"))
                //    commanderName = CommanderName.Kerrigan;
                //else if (fileName.StartsWith("ZB"))
                //    commanderName = CommanderName.Stetmann;
                //else if (fileName.StartsWith("ZC"))
                //    commanderName = CommanderName.Zagara;
                //else if (fileName.StartsWith("ZD"))
                //    commanderName = CommanderName.Dehaka;

                if (commanderName == null)
                {
                    continue;
                }
                TechnologyTableEntryWrapper entry = new TechnologyTableEntryWrapper()
                {
                    ID = UnityEngine.Random.Range(1, int.MaxValue),
                    Annotation = $"{commanderName} {fileName}",
                    Commander = (CommanderName)commanderName,
                    Name = new StringID(fileName),
                    Texture = fileName,
                };
                list.Add(JSONMap.ToJSON(entry));
            }

            for (int i = 0; i < list.Count; i++)
            {
                list[i].Bake(true);
            }
            string content = @list.ToString(true);
            UnityEngine.Debug.Log(content);
        }

        [MenuItem("Tools/Unused/Copy Unit Entries")]
        public static void CopyUnitEntries()
        {
            EditorTableManager.Refresh();
            JSONObject @list = new JSONObject(JSONObject.Type.ARRAY);
            foreach (UnitTable.Entry entry in EditorTableManager.UnitTable.Data.Values)
            {
                if (entry.Commander == CommanderName.Amon && entry.Annotation.Contains(" P "))
                {
                    JSONObject @object = JSONMap.ToJSON(entry);
                    @object.SetField("m_ID", UnityEngine.Random.Range(1, int.MaxValue));
                    @object.SetField("m_Annotation", @object.GetField("m_Annotation").str.Replace("Amon", "Artanis"));
                    @object.SetField("m_Commander", CommanderName.Artanis.ToString());
                    @list.Add(@object);
                }
            }
            for (int i = 0; i < @list.Count; i++)
            {
                @list.list[i].Bake(true);
            }
            string content = @list.ToString(true);
            UnityEngine.Debug.Log(content);
        }
    }
}