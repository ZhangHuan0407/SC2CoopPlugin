using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Collections;
using Game;
using System.Threading;

namespace Table
{
    /// <summary>
    /// 所有的数据表
    /// <para>由于数据表字段是线程独立数据，因此字段只读线程安全</para>
    /// <para>方法会请求全局锁</para>
    /// </summary>
    public static class TableManager
    {
        /* field */
        private static int LoadingThreadID;

        [ThreadStatic]
        private static LocalizationTable m_LocalizationTable;
        public static LocalizationTable LocalizationTable
        {
            get => m_LocalizationTable;
            private set => m_LocalizationTable = value;
        }

        [ThreadStatic]
        private static AttackWaveTable m_AttackWaveTable;
        public static AttackWaveTable AttackWaveTable
        {
            get => m_AttackWaveTable;
            private set => m_AttackWaveTable = value;
        }

        [ThreadStatic]
        private static PrestigeTable m_PrestigeTable;
        public static PrestigeTable PrestigeTable
        {
            get => m_PrestigeTable;
            private set => m_PrestigeTable = value;
        }

        [ThreadStatic]
        private static UnitTable m_UnitTable;
        public static UnitTable UnitTable
        {
            get => m_UnitTable;
            private set => m_UnitTable = value;
        }

        [ThreadStatic]
        private static TechnologyTable m_TechnologyTable;
        public static TechnologyTable TechnologyTable
        {
            get => m_TechnologyTable;
            private set => m_TechnologyTable = value;
        }

        [ThreadStatic]
        private static ModelTable m_ModelTable;
        public static ModelTable ModelTable
        {
            get => m_ModelTable;
            private set => m_ModelTable = value;
        }

        /* ctor */
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        private static void Init()
        {
            LoadingThreadID = Thread.CurrentThread.ManagedThreadId;
        }

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
                TechnologyTable = LoadTable<TechnologyTable>("TechnologyTable.json");
                ModelTable = new ModelTable();
                ModelTable.SearchAllModelFrom(resourceRepositoryConfig);
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

        public static event Action MainThread_ReloadLocalizeTable_Handle;
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
            LocalizationTable = JSONMap.ParseJSON<LocalizationTable>(JSONObject.Create(describeContent));
            if (Thread.CurrentThread.ManagedThreadId == LoadingThreadID)
                MainThread_ReloadLocalizeTable_Handle?.Invoke();
        }
    }
}