using System;
using System.IO;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using UnityEngine;
using GitRepository;
using Game.Model;
using Game;

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
            public CommanderName CommanderName;
            public FileInfo Fileinfo;
            public SystemLanguage Language;
            public int Level;
            public string Id;
            public int Prestige;
            public string Title;
        }

        public Dictionary<string, Entry> IdToEntries;
        public List<Entry> AllEntries;

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
                    LoadAndRecord(files[i], directoryPath);
                directoryPath = $"{repositoryConfig.LocalDirectory}/{PlayerProvidedDirectory}";
                foreach (string directoryName in Directory.GetDirectories(directoryPath))
                {
                    string subDirectoryPath = $"{directoryPath}/{new DirectoryInfo(directoryName).Name}";
                    files = Directory.GetFiles(subDirectoryPath, "*.json");
                    for (int i = 0; i < files.Length; i++)
                        LoadAndRecord(files[i], subDirectoryPath);
                }
                directoryPath = GameDefined.CustomCommanderPipelineDirectoryPath;
                files = Directory.GetFiles(directoryPath, "*.json");
                for (int i = 0; i < files.Length; i++)
                    LoadAndRecord(files[i], new DirectoryInfo(directoryPath).Parent.FullName.Replace("\\", "/"));

                void LoadAndRecord(string filePath, string path)
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
                            Title = pipeline.Title,
                        };
                        IdToEntries[entry.Id] = entry;
                        AllEntries.Add(entry);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError(ex);
                        entry = null;
                    }
                    if (entry != null)
                        AllEntries.Add(entry);
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