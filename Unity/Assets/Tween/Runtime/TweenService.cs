using System;
using System.Collections.Generic;
using UnityEngine;

namespace Tween
{
    public class TweenService : MonoBehaviour, IDisposable
    {
#if UNITY_EDITOR
        /* field */
        internal TweenerBehaviourRecord Behaviour;
        private Queue<TweenerBehaviourRecord> BehaviourHistory;
#endif

        public static TweenService Instance { get; internal set; }

        private LinkedList<Tweener> m_Tweeners;
        protected LinkedList<Tweener> Tweeners
        {
            get => m_Tweeners = m_Tweeners ?? new LinkedList<Tweener>();
            set => m_Tweeners = value;
        }

        private LinkedList<Tweener> m_NextFrameTweeners;
        protected LinkedList<Tweener> NextFrameTweeners
        {
            get => m_NextFrameTweeners = m_NextFrameTweeners ?? new LinkedList<Tweener>();
            set => m_NextFrameTweeners = value;
        }

        protected Tweener TweenerInUsed;

        /// <summary>
        /// 已经抛弃或即将抛弃此实例
        /// </summary>
        public bool Abort { get; private set; }
        /// <summary>
        /// 标记此实例即将被抛弃，就仅仅是标记而已
        /// </summary>
        public void AbortInstance() => Abort = true;
        /// <summary>
        /// 当前实例正在刷新状态，将依次执行<see cref="Tweener"/>调用
        /// </summary>
        public bool IsUpdating { get; private set; }

        /* inter */

        /* ctor */
        private void Awake()
        {
#if UNITY_EDITOR
            BehaviourHistory = new Queue<TweenerBehaviourRecord>();
#endif

            Instance = Instance ?? this;
            Abort = false;
            IsUpdating = false;
            DontDestroyOnLoad(gameObject);
        }
        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;

            Dispose();
        }

        /* func */
        private void Update()
        {
            if (Abort)
                return;
            // 将 Tweeners 中剩余导入 NextFrameTweeners，并在首部执行
            // 这些剩余来自 RestoreImmediately, AddImmediately 的添加
            while (Tweeners.Count > 0)
            {
                NextFrameTweeners.AddFirst(Tweeners.Last);
                Tweeners.RemoveLast();
            }
            LinkedList<Tweener> temp = Tweeners;
            Tweeners = NextFrameTweeners;
            NextFrameTweeners = temp;
            UpdateTweeners();
        }
        public void UpdateTweeners()
        {
            if (IsUpdating)
                return;
            IsUpdating = true;
            while (Tweeners.Count > 0)
            {
                TweenerInUsed = Tweeners.First.Value;
                Tweeners.RemoveFirst();
                try
                {
                    bool haveNext = TweenerInUsed.Enumerator.MoveNext();
                    Tweener current = TweenerInUsed.Enumerator.Current;
                    TweenerInUsed.UpdateTween();
                    // Update 回掉中停 Tweener
                    if (TweenerInUsed.State == TweenerState.Stop)
                        haveNext = false;
                    else if (TweenerInUsed.Normalized >= 1f)
                    {
                        TweenerInUsed.State = TweenerState.Finish;
                        TweenerInUsed.CompleteTween();
                    }
                    // 资源被摧毁导致停止
                    else if (current == null)
                        TweenerInUsed.State = TweenerState.AssetHaveBeenDestroy;
                    // Update 回掉中没有停止 Tweener，资源仍然存在，能继续迭代
                    if (current != null && haveNext)
                        NextFrameTweeners.AddLast(current);
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                    // 出现错误导致停止
                    if (TweenerInUsed != null)
                        TweenerInUsed.State = TweenerState.Error;
                }
                finally
                {
                    TweenerInUsed = null;
                }
            }
            IsUpdating = false;
        }
#if UNITY_EDITOR
        private void LateUpdate()
        {
            BehaviourHistory.Enqueue(Behaviour);
            Behaviour = default;
            if (BehaviourHistory.Count > 10)
                BehaviourHistory.Dequeue();
        }
#endif

        public static void Add(Tweener tweener, bool force = false)
        {
            if (tweener is null)
                throw new ArgumentNullException(nameof(tweener));
            Instance.Add_Internal(tweener, force);
        }
        internal void Add_Internal(Tweener tweener, bool force)
        {
            if (Abort)
                return;
            else if (tweener.State == TweenerState.WaitForActivation
                || force)
            {
#if UNITY_EDITOR
                Behaviour.AddTweenerSuccessTimes++;
#endif
                tweener.State = TweenerState.IsRunnning;
                NextFrameTweeners.AddLast(tweener);
            }
        }
        public static void AddImmediately(Tweener tweener)
        {
            if (tweener is null)
                throw new ArgumentNullException(nameof(tweener));
            Instance.AddImmediately_Internal(tweener);
        }
        internal void AddImmediately_Internal(Tweener tweener)
        {
            if (Abort)
                return;
            else if (tweener.State == TweenerState.WaitForActivation)
            {
#if UNITY_EDITOR
                Behaviour.AddTweenerSuccessTimes++;
#endif
                tweener.State = TweenerState.IsRunnning;
                Tweeners.AddLast(tweener);
            }
        }

        public static void Remove(Tweener tweener)
        {
            if (tweener is null)
                throw new ArgumentNullException(nameof(tweener));
            Instance.Remove_Internal(tweener);
        }
        internal void Remove_Internal(Tweener tweener)
        {
            if (Abort)
                return;
            else if (tweener.State == TweenerState.WaitForActivation)
            {
#if UNITY_EDITOR
                Behaviour.StopTweenerSuccessTimes++;
#endif
                tweener.State = TweenerState.Stop;
            }
            else if (tweener.State == TweenerState.IsRunnning)
            {
#if UNITY_EDITOR
                Behaviour.StopTweenerSuccessTimes++;
#endif
                tweener.State = TweenerState.Stop;
                bool removeSuccess = NextFrameTweeners.Remove(tweener) || Tweeners.Remove(tweener) || tweener == TweenerInUsed;
                if (!removeSuccess)
                    Debug.LogError($"Not found tweener in list, but it's state is running.");
            }
            else if (tweener.State == TweenerState.Finish
                && tweener.NextTweener != null)
            {
#if UNITY_EDITOR
                Behaviour.StopTweenerSuccessTimes++;
#endif
                tweener.NextTweener.State = TweenerState.Stop;
            }
        }

        public static void Restore(Tweener tweener)
        {
            if (tweener is null)
                throw new ArgumentNullException(nameof(tweener));
            Instance.Restore_Internal(tweener);
        }
        internal void Restore_Internal(Tweener tweener)
        {
            if (Abort)
                return;
            else if (tweener.State == TweenerState.Stop)
            {
#if UNITY_EDITOR
                Behaviour.RestoreTweenerSuccessTimes++;
#endif
                tweener.State = TweenerState.IsRunnning;
                NextFrameTweeners.AddLast(tweener);
            }
        }

        /// <summary>
        /// 恢复一个 <see cref="Tweener"/> 实例的执行，尽可能添加到当前帧而不是下一帧
        /// </summary>
        /// <param name="tweener">需要恢复的 <see cref="Tweener"/> 实例</param>
        public static void RestoreImmediately(Tweener tweener)
        {
            if (tweener is null)
                throw new ArgumentNullException(nameof(tweener));
            Instance.RestoreImmediately_Internal(tweener);
        }
        internal void RestoreImmediately_Internal(Tweener tweener)
        {
            if (Abort)
                return;
            else if (tweener.State == TweenerState.Stop)
            {
#if UNITY_EDITOR
                Behaviour.RestoreTweenerSuccessTimes++;
#endif
                tweener.State = TweenerState.IsRunnning;
                Tweeners.AddLast(tweener);
            }
        }

        /* IDisposable */
        public void Dispose()
        {
            Abort = true;
            if (m_Tweeners != null)
            {
                foreach (Tweener tweener in m_Tweeners)
                {
                    if (tweener.State == TweenerState.IsRunnning)
                        tweener.State = TweenerState.Stop;
                }
                m_Tweeners = null;
            }
            if (m_NextFrameTweeners != null)
            {
                foreach (Tweener tweener in m_NextFrameTweeners)
                {
                    if (tweener.State == TweenerState.IsRunnning)
                        tweener.State = TweenerState.Stop;
                }
                m_NextFrameTweeners = null;
            }
            // TweenerInUsed 暂时不做清理
        }
    }
}