using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Tween;
using UnityEngine;

namespace Game
{
    /// <summary>
    /// 后台线程
    /// </summary>
    public class BackThread : MonoBehaviour
    {
        public enum UpdateMode
        {
            /// <summary>
            /// 在下一个触发点尽快执行
            /// </summary>
            AsSoonAsPossible,
            Update,
            LateUpdate,
        }

        /* field */
        private static GameObject m_InstanceGo;
        public static GameObject InstanceGo
        {
            get
            {
                if (!m_InstanceGo)
                {
                    m_InstanceGo = new GameObject();
                    DontDestroyOnLoad(m_InstanceGo);
                    m_InstanceGo.name = "BackThread";
                    m_InstanceGo.SetActive(true);
                }
                return m_InstanceGo;
            }
        }

        public object LockedObject { get; private set; }
        public string Label { get; private set; }
        public int BackThreadID => m_Thread.ManagedThreadId;
        public ThreadState ThreadState => m_Thread.ThreadState;

        private bool m_Inited;
        private volatile bool m_Quit;
        private Thread m_Thread;
        private Queue<Task> m_BackTasks;
        private LinkedList<(Task, UpdateMode)> m_MainTasks;
        private AutoResetEvent m_ResetEvent;

        /* ctor */
        private void Awake()
        {
            LockedObject = new object();
            m_Inited = false;
            m_Thread = new Thread(BackThreadUpdate);
            m_BackTasks = new Queue<Task>();
            m_MainTasks = new LinkedList<(Task, UpdateMode)>();
            m_ResetEvent = new AutoResetEvent(false);
            m_Thread.Start();
            LogService.System($"{nameof(BackThread)}.{nameof(Awake)}",
                              $"{nameof(Thread.ManagedThreadId)}: {m_Thread.ManagedThreadId}, {nameof(Thread.IsBackground)}: {m_Thread.IsBackground}, {nameof(Thread.ThreadState)}: {m_Thread.ThreadState}");
        }
        private void Start()
        {
            m_Inited = true;
        }

        private void OnDestroy()
        {
            lock (LockedObject)
            {
                SetNeedQuit();
                if (m_Thread != null)
                {
                    ThreadState threadState = m_Thread.ThreadState;
                    if (threadState == ThreadState.AbortRequested || threadState == ThreadState.Aborted ||
                        threadState == ThreadState.StopRequested || threadState == ThreadState.Stopped)
                        return;
                    if (threadState != ThreadState.Aborted)
                        m_Thread.Abort();
                    LogService.System($"{nameof(BackThread)}.{nameof(OnDestroy)}",
                              $"{nameof(Thread.ManagedThreadId)}: {m_Thread.ManagedThreadId}, {nameof(Thread.IsBackground)}: {m_Thread.IsBackground}, {nameof(Thread.ThreadState)}: {m_Thread.ThreadState}");
                    m_Thread = null;
                }
            }
        }

        public static BackThread CreateNew(string label)
        {
            BackThread backThread = InstanceGo.AddComponent<BackThread>();
            backThread.Label = label;
            return backThread;
        }

        /* func */
        public void SetNeedQuit()
        {
            m_Quit = true;
        }

        public void RunInMainThread(Action action, UpdateMode updateMode) => RunInMainThread(new Task(action), updateMode);
        public void RunInMainThread(Task task, UpdateMode updateMode)
        {
            lock (LockedObject)
            {
                m_MainTasks.AddLast((task, updateMode));
            }
        }

        private void Update()
        {
            UpdateWithMode((updateMode) => updateMode == UpdateMode.AsSoonAsPossible || updateMode == UpdateMode.Update);
        }
        private void LateUpdate()
        {
            UpdateWithMode((updateMode) => updateMode == UpdateMode.AsSoonAsPossible || updateMode == UpdateMode.LateUpdate);
        }
        private void UpdateWithMode(Predicate<UpdateMode> predicate)
        {
            lock (LockedObject)
            {
                LinkedListNode<(Task, UpdateMode)> node = m_MainTasks.First;
                while (node != null)
                {
                    LinkedListNode<(Task, UpdateMode)> next = node.Next;
                    if (predicate(node.Value.Item2))
                    {
                        Task task = node.Value.Item1;
                        m_MainTasks.Remove(node);
                        task.RunSynchronously();
                        if (task.Status == TaskStatus.Faulted)
                            LogService.Error($"{nameof(BackThread)}.{Label} task", task.Exception);
                    }
                    node = next;
                }
            }
        }

        public void RunInBackThread(Task task)
        {
            if (task is null)
                throw new ArgumentNullException(nameof(task));

            lock (LockedObject)
            {
                m_BackTasks.Enqueue(task);
                m_ResetEvent.Set();
            }
        }

        public Tweener WaitingBackThreadTweener(Action action)
        {
            if (action is null)
                throw new ArgumentNullException(nameof(action));
            Task task = new Task(action);
            lock (LockedObject)
            {
                m_BackTasks.Enqueue(task);
                m_ResetEvent.Set();
            }
            return LogicTween.WaitUntil(() => task.IsCompleted);
        }

        private void BackThreadUpdate()
        {
            while (!m_Inited && !m_Quit)
            {
                Thread.Sleep(1);
            }
            while (!m_Quit)
            {
                Task task = null;
                lock (LockedObject)
                {
                    if (m_BackTasks.Count > 0)
                    {
                        task = m_BackTasks.Dequeue();
                    }
                }
                if (task == null)
                {
                    m_ResetEvent.WaitOne();
                }
                else
                {
                    task.RunSynchronously();
                    if (task.Status == TaskStatus.Faulted)
                        LogService.Error($"{nameof(BackThread)}.{Label} task", task.Exception);
                }
            }
        }
    }
}