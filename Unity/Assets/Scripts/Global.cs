using System.Collections.Generic;
using UnityEngine;

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
    }
}