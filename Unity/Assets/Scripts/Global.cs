using System.Diagnostics;
using Game.OCR;
using GitRepository;
using Table;

namespace Game
{
    /// <summary>
    /// 提供全局变量的引用
    /// </summary>
    public static class Global
    {
        public static UserSetting UserSetting { get; internal set; }

        public static LogService LogService { get; internal set; }

        public static BackThread BackThread { get; internal set; }

        public static Process StarCraftProcess { get; internal set; }

        public static RepositoryConfig ResourceRepositoryConfig { get; internal set; }

        public static MapTime MapTime { get; internal set; }
        public static CommanderName CurrentUsedCommander { get; internal set; }
    }
}