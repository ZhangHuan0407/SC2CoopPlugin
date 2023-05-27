using System;
using System.IO;
using UnityEngine;

namespace Game
{
    public static class ResourcesInterface
    {
        private static AssetBundle m_AssetBundle;

        public static void Init()
        {
            if (File.Exists("assetbundle"))
            {
                try
                {
                    m_AssetBundle = AssetBundle.LoadFromFile("assetbundle");
                }
                catch (Exception ex)
                {
                    Debug.LogError(ex);
                    m_AssetBundle = null;
                }
            }
        }

        public static T Load<T>(string name) where T : UnityEngine.Object
        {
            T result = null;
            if (m_AssetBundle)
                result = m_AssetBundle.LoadAsset<T>(name.ToLowerInvariant());
            if (result == null)
                result = Resources.Load<T>(name);
            return result;
        }
    }
}