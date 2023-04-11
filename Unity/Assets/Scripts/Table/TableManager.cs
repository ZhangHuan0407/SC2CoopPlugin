using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Collections;
using Game;

namespace Table
{
    /// <summary>
    /// 所有的数据表
    /// </summary>
    public static class TableManager
    {
        /* field */
        private static LocalizationTable m_LocalizationTable;
        public static LocalizationTable LocalizationTable
        {
            get => m_LocalizationTable;
        }
        public static AttackWaveTable AttackWaveTable { get; private set; }
        public static PrestigeTable PrestigeTable { get; private set; }
        public static UnitTable UnitTable { get; private set; }

        /* func */
        public static void LoadInnerTables()
        {
            GitRepository.RepositoryConfig resourceRepositoryConfig = Global.ResourceRepositoryConfig;
            resourceRepositoryConfig.IOLock.EnterReadLock();
            try
            {
                AttackWaveTable = LoadTable<AttackWaveTable>("AttackWaveTable.json");
                PrestigeTable = LoadTable<PrestigeTable>("PrestigeTable.json");
                UnitTable = LoadTable<UnitTable>("UnitTable.json");
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                return;
            }
            finally
            {
                resourceRepositoryConfig.IOLock.ExitReadLock();
            }

            T LoadTable<T>(string tableName)
            {
                string content = File.ReadAllText($"{resourceRepositoryConfig.LocalDirectory}/Tables/{tableName}");
                return JSONMap.ParseJSON<T>(JSONObject.Create(content));
            }
        }

        public static void LoadLocalizationTable(SystemLanguage systemLanguage)
        {
            GitRepository.RepositoryConfig resourceRepositoryConfig = Global.ResourceRepositoryConfig;
            resourceRepositoryConfig.IOLock.EnterReadLock();
            string describeContent;
            try
            {
                describeContent = File.ReadAllText($"{resourceRepositoryConfig.LocalDirectory}/Localization/{systemLanguage}");
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                return;
            }
            finally
            {
                resourceRepositoryConfig.IOLock.ExitReadLock();
            }
            JSONObject @object = JSONObject.Create(describeContent);
            LocalizationTable table = new LocalizationTable();
            foreach (var pair in @object.dictionary)
                table[pair.Key] = pair.Value.str;
            m_LocalizationTable = table;
        }
    }
}