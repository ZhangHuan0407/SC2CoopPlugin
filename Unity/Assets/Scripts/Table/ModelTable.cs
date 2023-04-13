using System;
using System.Collections.Generic;
using System.IO;
using Game;
using UnityEngine;

namespace Table
{
    public class ModelTable
    {
        public struct Entry
        {
            public FileInfo FileInfo { get; set; }
            public JSONObject Cache { get; set; }
        }
        private Dictionary<string, Entry> m_ModelDictionary;
        public IReadOnlyDictionary<string, Entry> ModelDictionary;

        public ModelTable()
        {
            m_ModelDictionary = new Dictionary<string, Entry>();
        }

        public TModel InstantiateModel<TModel>(object modelName) where TModel : struct
        {
            JSONObject @object = TryGet(modelName.ToString());
            return JSONMap.ParseJSON<TModel>(@object);
        }
        private JSONObject TryGet(string key)
        {
            if (!m_ModelDictionary.TryGetValue(key, out Entry entry))
                return new JSONObject(JSONObject.Type.NULL);
            if (entry.Cache != null)
                return entry.Cache;
            Global.ResourceRepositoryConfig.IOLock.EnterReadLock();
            Exception exception = null;
            try
            {
                string content = File.ReadAllText(entry.FileInfo.FullName);
                entry.Cache = JSONObject.Create(content);
                return entry.Cache;
            }
            catch (Exception ex)
            {
                exception = ex;
                UnityEngine.Debug.LogError(ex.ToString());
                return new JSONObject(JSONObject.Type.NULL);
            }
            finally
            {
                Global.ResourceRepositoryConfig.IOLock.ExitReadLock();
            }
        }

        public void SearchAllModelFrom(GitRepository.RepositoryConfig repositoryConfig)
        {
            repositoryConfig.IOLock.EnterReadLock();
            try
            {
                string[] files = Directory.GetFiles($"{repositoryConfig.LocalDirectory}/Models", "*.json");
                for (int i = 0; i < files.Length; i++)
                    m_ModelDictionary[files[i]] = new Entry()
                    {
                        FileInfo = new FileInfo(files[i]),
                        Cache = null,
                    };
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
    }
}