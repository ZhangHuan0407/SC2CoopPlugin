using System;
using System.IO;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using UnityEngine;
using GitRepository;
using Game.Model;
using Game;
using System.Linq;

namespace Table
{
    public class CommanderPipelineTable
    {
        private const string BuildInDirectory = "CommanderPipeline-BuildIn";
        private const string PlayerProvidedDirectory = "CommanderPipeline-PlayerProvided";

        [Serializable]
        [StructLayout(LayoutKind.Auto)]
        public class Entry
        {
            [SerializeField]
            private CommanderName m_CommanderName;
            public CommanderName CommanderName
            {
                get => m_CommanderName;
                internal set => m_CommanderName = value;
            }

            [SerializeField]
            private FileInfo m_Fileinfo;
            public FileInfo Fileinfo
            {
                get => m_Fileinfo;
                internal set => m_Fileinfo = value;
            }

            [SerializeField]
            private SystemLanguage m_Language;
            public SystemLanguage Language
            {
                get => m_Language;
                internal set => m_Language = value;
            }

            [SerializeField]
            private int m_Level;
            public int Level
            {
                get => m_Level;
                internal set => m_Level = value;
            }
            
            [SerializeField]
            private int m_Masteries;
            public int Masteries
            {
                get => m_Masteries;
                internal set => m_Masteries = value;
            }

            [SerializeField]
            private string m_Id;
            public string Id
            {
                get => m_Id;
                internal set => m_Id = value;
            }

            [SerializeField]
            private int m_Prestige;
            public int Prestige
            {
                get => m_Prestige;
                internal set => m_Prestige = value;
            }

            [SerializeField]
            private string m_Title;
            public string Title
            {
                get => m_Title;
                internal set => m_Title = value;
            }

            /// <summary>
            /// Custom Commander Pipeline
            /// </summary>
            public bool IsCCP
            {
                get;
                internal set;
            }
        }

        [SerializeField]
        private Dictionary<string, Entry> m_IdToEntries;
        public Dictionary<string, Entry> IdToEntries
        {
            get => m_IdToEntries;
            private set => m_IdToEntries = value;
        }

        private List<Entry> m_AllEntries;
        public List<Entry> AllEntries
        {
            get => m_AllEntries;
            private set => m_AllEntries = value;
        }

        public FileInfo this[string id]
        {
            get => IdToEntries[id].Fileinfo;
        }

        public CommanderPipelineTable()
        {
            IdToEntries = new Dictionary<string, Entry>();
            AllEntries = new List<Entry>();
        }

        public void SearchAllModelFrom(GitRepository.RepositoryConfig repositoryConfig)
        {
            IdToEntries.Clear();
            AllEntries.Clear();
            repositoryConfig.IOLock.EnterReadLock();
            try
            {
                string directoryPath = $"{repositoryConfig.LocalDirectory}/{BuildInDirectory}";
                string[] files = Directory.GetFiles(directoryPath, "*.json");
                for (int i = 0; i < files.Length; i++)
                    LoadAndRecord(files[i], directoryPath, false);
                directoryPath = $"{repositoryConfig.LocalDirectory}/{PlayerProvidedDirectory}";
                foreach (string directoryName in Directory.GetDirectories(directoryPath))
                {
                    string subDirectoryPath = $"{directoryPath}/{new DirectoryInfo(directoryName).Name}";
                    files = Directory.GetFiles(subDirectoryPath, "*.json");
                    for (int i = 0; i < files.Length; i++)
                        LoadAndRecord(files[i], subDirectoryPath, false);
                }
                directoryPath = GameDefined.CustomCommanderPipelineDirectoryPath;
                files = Directory.GetFiles(directoryPath, "*.json");
                for (int i = 0; i < files.Length; i++)
                    LoadAndRecord(files[i], new DirectoryInfo(directoryPath).Parent.FullName.Replace("\\", "/"), true);

                void LoadAndRecord(string filePath, string path, bool isCCP)
                {
                    Entry entry = null;
                    CommanderPipeline pipeline;
                    try
                    {
                        JSONObject @object = JSONObject.Create(File.ReadAllText(filePath));
                        pipeline = JSONMap.ParseJSON<CommanderPipeline>(@object);
                        filePath = filePath.Replace("\\", "/");
                        int lastIndexOf = filePath.LastIndexOf(path);

                        entry = new Entry()
                        {
                            CommanderName = pipeline.Commander,
                            Fileinfo = new FileInfo(filePath),
                            Language = pipeline.Language,
                            Level = pipeline.Level,
                            Id = path.Substring(lastIndexOf + 1),
                            Prestige = pipeline.Prestige,
                            Masteries = Enumerable.Sum(pipeline.Masteries),
                            Title = pipeline.Title,
                            IsCCP = isCCP,
                        };
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError(ex);
                        entry = null;
                    }
                    if (entry != null)
                    {
                        IdToEntries[entry.Id] = entry;
                        AllEntries.Add(entry);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
            }
            finally
            {
                repositoryConfig.IOLock.ExitReadLock();
            }
        }

        public List<Entry> Filter(string str, SystemLanguage? language, CommanderName? commanderName, int level)
        {
            List<Entry> allEntries = AllEntries;
            List<Entry> result = new List<Entry>();
            for (int i = 0; i < allEntries.Count && result.Count < 40; i++)
            {
                Entry entry = allEntries[i];
                if (language != null)
                {
                    if (entry.Language != language)
                        continue;
                }
                if (commanderName != null)
                {
                    if (entry.CommanderName != commanderName)
                        continue;
                }
                if (entry.Level > level)
                    continue;
                if (!string.IsNullOrWhiteSpace(str) &&
                    !entry.Title.Contains(str))
                    continue;
                result.Add(entry);
            }
            return result;
        }
        
        public CommanderPipeline Instantiate(string id)
        {
            FileInfo fileInfo = IdToEntries[id].Fileinfo;
            JSONObject @object = JSONObject.Create(File.ReadAllText(fileInfo.FullName));
            return JSONMap.ParseJSON<CommanderPipeline>(@object);
        }
    }
}