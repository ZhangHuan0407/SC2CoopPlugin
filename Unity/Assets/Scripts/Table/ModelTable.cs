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

        public TModel InstantiateModel<TModel>(object modelName)
        {
            JSONObject @object = TryGet(modelName.ToString());
            object instance = JSONMap.ParseJSON(typeof(TModel), @object);
            if (instance == null)
                Debug.LogError($"{nameof(InstantiateModel)} instantiate failed, TModel: {typeof(TModel).Name}, modelName: {modelName}");
            return (TModel)instance;
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
                {
                    FileInfo fileInfo = new FileInfo(files[i]);
                    m_ModelDictionary[Path.GetFileNameWithoutExtension(files[i])] = new Entry()
                    {
                        FileInfo = fileInfo,
                        Cache = null,
                    };
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
    }
}