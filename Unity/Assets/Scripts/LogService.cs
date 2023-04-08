using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using UnityEngine;

namespace Game
{
    public class LogService
    {
        /* const */
        public readonly string LogFilePath;
        private StringBuilder m_StringBuilder;
        private object m_Lock;
        private bool m_IsDirty;
        internal int LoadingThreadID { get; set; }

        /* field */

        /* ctor */
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        private static void Init()
        {
            Global.LogService = new LogService();
            Global.LogService.InitFromLoadingThread();
            Debug.Log(nameof(LogService));
        }

        public LogService()
        {
            LogFilePath = $"./GameLog/{DateTime.Now:yy-MM-dd HH-mm-ss}.txt";
#if !ALPHA
            return;
#endif
            m_Lock = new object();
            m_StringBuilder = new StringBuilder(512);
            Directory.CreateDirectory(Path.GetDirectoryName(LogFilePath));
            LoadingThreadID = -1;
            m_IsDirty = false;
        }

        /* func */
        public static void System(string action, string info)
        {
#if ALPHA
            Global.LogService.Log($"System {action}\n{info}");
#else
            return;
#endif
        }
        public static void Table(string action, string info)
        {
#if ALPHA
            Global.LogService.Log(action, info);
#else
            return;
#endif
        }
        public static void Error(string action, object obj, bool skipUnityLog = false)
        {
            string info = obj?.ToString() ?? string.Empty;
            if (!skipUnityLog)
                Debug.LogError($"{action}\n{info}");
            Global.LogService.Log(action, info);
        }
        public static void UserAction(string action, string info)
        {
            Global.LogService.Log(action, info?.ToString() ?? string.Empty);
        }

        public void Log(string content, [CallerMemberName] string caller = null)
        {
            lock (m_Lock)
            {
                int threadID = Thread.CurrentThread.ManagedThreadId;
                m_StringBuilder.AppendLine($"[{DateTime.Now:yy-MM-dd HH:mm:ss}] {(threadID == LoadingThreadID ? "LoadingThread" : "Thread " + threadID)} caller {caller}")
                    .AppendLine(content)
                    .AppendLine();
            }
            MarkDirty();
        }

        /// <summary>
        /// 初始化时必须由主线程调用获取主线程id
        /// </summary>
        public void InitFromLoadingThread()
        {
            LoadingThreadID = Thread.CurrentThread.ManagedThreadId;
        }

        private void MarkDirty()
        {
#if !ALPHA
            return;
#endif
            lock (m_Lock)
            {
                if (m_IsDirty)
                    return;
                else
                    m_IsDirty = true;
            }
            Task.Run(async () =>
            {
                await Task.Delay(200);
                WriteOut();
            });
        }
        private void WriteOut()
        {
            lock (m_Lock)
            {
                if (!m_IsDirty)
                    return;
                m_IsDirty = false;
                byte[] bytes = Encoding.UTF8.GetBytes(m_StringBuilder.ToString());
                m_StringBuilder.Clear();
                using (FileStream outputStream = new FileStream(LogFilePath, FileMode.Append, FileAccess.Write))
                {
                    outputStream.Write(bytes, 0, bytes.Length);
                    outputStream.Flush();
                }
            }
        }
    }
}