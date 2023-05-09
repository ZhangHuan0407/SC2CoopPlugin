using System;
using System.IO;
using Table;
using UnityEditor;

namespace Game.Editor
{
    public static class EditorTableManager
    {
        private static AttackWaveTable m_AttackWaveTable;
        public static AttackWaveTable AttackWaveTable => m_AttackWaveTable;
        private static MasteriesTable m_MasteriesTable;
        public static MasteriesTable MasteriesTable => m_MasteriesTable;
        private static PrestigeTable m_PrestigeTable;
        public static PrestigeTable PrestigeTable => m_PrestigeTable;
        private static UnitTable m_UnitTable;
        public static UnitTable UnitTable => m_UnitTable;
        private static TechnologyTable m_TechnologyTable;
        public static TechnologyTable TechnologyTable => m_TechnologyTable;

        private static bool m_Init = false;

        public static void Refresh()
        {
            if (!m_Init)
                ReloadInnerTables();
        }

        [MenuItem("Tools/Reload Tables")]
        public static void ReloadInnerTables()
        {
            for (int i = 0; i < GameDefined.JSONSerializedRegisterTypes.Length; i++)
                JSONMap.RegisterType(GameDefined.JSONSerializedRegisterTypes[i]);

            m_AttackWaveTable = LoadTable<AttackWaveTable>("AttackWaveTable.json");
            m_MasteriesTable = LoadTable<MasteriesTable>("MasteriesTable.json");
            m_PrestigeTable = LoadTable<PrestigeTable>("PrestigeTable.json");
            m_UnitTable = LoadTable<UnitTable>("UnitTable.json");
            m_TechnologyTable = LoadTable<TechnologyTable>("TechnologyTable.json");

            T LoadTable<T>(string tableName)
            {
                string content = File.ReadAllText($"{GameDefined.ResourceSubmoduleDirectory}/Tables/{tableName}");
                return JSONMap.ParseJSON<T>(JSONObject.Create(content));
            }
        }

        public static void SaveTable<T>(JSONObject @table)
        {
            string tablePath = $"{GameDefined.ResourceSubmoduleDirectory}/Tables/{typeof(T).Name}.json";
        }
    }
}