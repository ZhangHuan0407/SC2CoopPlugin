using System;
using UnityEngine;

namespace Tween
{
    public class TweenConfig : ScriptableObject
    {
        /* const */
        public const string AssetPath = "Assets/Tween/Resources/TweenConfig.asset";
        public const string ResourceLoadPath = "TweenConfig";
        public const int CodeVersion = 1;

        [Serializable]
        public struct EaseCurve
        {
            [SerializeField]
            private string m_Name;
            public string Name
            {
                get => m_Name;
            }

            [SerializeField]
            private AnimationCurve m_Curve;
            public AnimationCurve Curve
            {
                get => m_Curve;
            }
        }

        /* field */
        /// <summary>
        /// <see cref="Tween"/> 配置文件
        /// </summary>
        public static TweenConfig Config;
        private static bool m_Inited = false;

        [SerializeField]
        private EaseCurve[] m_EaseCurves;
        public EaseCurve[] EaseCurves => m_EaseCurves;
        [SerializeField]
        private int m_Version;
        public int Version => m_Version;

        /* func */
#if UNITY_EDITOR
        [UnityEditor.MenuItem("Tools/Tween/CreateTweenConfig", priority = 1000)]
        private static void CreateTweenConfig()
        {
            TweenConfig config = CreateInstance<TweenConfig>();
            config.m_EaseCurves = new EaseCurve[0];
            config.m_Version = CodeVersion;
            UnityEditor.AssetDatabase.CreateAsset(config, AssetPath);
        }
#endif

        /// <summary>
        /// <see cref="TweenEase"/> 将在首帧之前完成初始化，特殊情况下可以提前强制初始化
        /// <para>初始化时将使用<see cref="Resources"/>加载必要资源</para>
        /// </summary>
        public static void ForceInit() => InitFromLoadingThread();
        [RuntimeInitializeOnLoadMethod]
        private static void InitFromLoadingThread()
        {
            if (m_Inited)
                return;
            m_Inited = true;

            Config = Resources.Load<TweenConfig>(ResourceLoadPath);
            if (Config)
                TweenEase.InitCurveMap(Config.EaseCurves);
            else
            {
                TweenEase.InitCurveMap(new EaseCurve[0]);
                Debug.LogError("Can not found TweenConfig at: " + AssetPath);
            }
        }
    }
}