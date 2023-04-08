using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Tween
{
    public abstract class Tweener : IEnumerable<Tweener>
    {
        /* field */
        /// <summary>
        /// 反馈当前 Tweener 实例的状态
        /// </summary>
        public TweenerState State { get; internal set; }

        /// <summary>
        /// 迭代器实例，对于激活的 Tweener 每帧一次调用
        /// <para>如果当前没有执行完，返回实例自身。</para>
        /// <para>如果操作对象已经不存在，返回 null。</para>
        /// <para>如果当前执行完，返回 NextTweener。</para>
        /// </summary>
        internal IEnumerator<Tweener> Enumerator;
        public event Action OnUpdate_Handle;
        public event Action OnComplete_Handle;
        /// <summary>
        /// 链式 Tweener 的头部实例
        /// <para>首个 Tweener 实例 HeadTweener 指向自己</para>
        /// </summary>
        public Tweener HeadTweener { get; protected set; }

        protected Tweener m_NextTweener;
        /// <summary>
        /// 完成后指向的下一个 Tweener 实例，可能为空
        /// </summary>
        public Tweener NextTweener
        {
            get => m_NextTweener;
            set => Then(value);
        }

        public AnimationCurve Ease { get; private set; }

        /* inter */
        /// <summary>
        /// 获得当前 Tweener 实例的归一化进度，值范围 [0, 1]
        /// <para>在子继承中实现</para>
        /// </summary>
        public abstract float Normalized { get; }
        /// <summary>
        /// 基于缓动曲线进行采样，返回采样结果
        /// <para><see cref="Ease"/>为null时，返回<see cref="Normalized"/></para>
        /// </summary>
        public float Evaluate
        {
            get
            {
                float value = Normalized;
                if (Ease != null)
                    value = Ease.Evaluate(Mathf.Clamp01(Normalized));
                return value;
            }
        }
        /// <summary>
        /// 获得当前 Tweener 实例的状态码，具体含义查询 <see cref="TweenerState"/>
        /// </summary>
        public int StateCode => (int)State;

        /* ctor */
        public Tweener()
        {
            State = TweenerState.WaitForActivation;
            Enumerator = null;
            HeadTweener = this;
            m_NextTweener = null;
            Ease = null;
        }

        /* func */
        /// <summary>
        /// 将当前 <see cref="Tweener"/> 送去启动
        /// </summary>
        /// <returns>当前 <see cref="Tweener"/> 实例</returns>
        public Tweener DoIt()
        {
#if UNITY_EDITOR
            if (TweenService.Instance != null)
                TweenService.Instance.Behaviour.AddTweenerTimes++;
#endif
            TweenService.Add(HeadTweener);
            return this;
        }
        /// <summary>
        /// 添加一个每次更新时回掉的函数
        /// 调用时 State => TweenerState.Finish || TweenerState.IsRunnning
        /// </summary>
        /// <param name="action">回掉的函数</param>
        /// <returns>当前实例，接着链式加内容</returns>
        public Tweener OnUpdate(Action action)
        {
            if (action is null)
                throw new ArgumentNullException(nameof(action));
            OnUpdate_Handle += action;
            return this;
        }
        public Tweener OnComplete(Action action)
        {
            if (action is null)
                throw new ArgumentNullException(nameof(action));
            OnComplete_Handle += action;
            return this;
        }
        /// <summary>
        /// 一个 <see cref="Tweener"/> 实例只能有一个 <see cref="NextTweener"/>，如果尝试插入第二个，第一个将被丢弃
        /// </summary>
        /// <param name="nextTweener">下一个 <see cref="Tweener"/> 实例，可以用 null 来剔除原有</param>
        /// <returns>指向的下一个 <see cref="Tweener"/> 实例</returns>
        public Tweener Then(Tweener nextTweener)
        {
            if (m_NextTweener != null)
            {
                foreach (Tweener childTweener in m_NextTweener)
                    childTweener.HeadTweener = m_NextTweener;
            }
            m_NextTweener = nextTweener;
            nextTweener.HeadTweener = HeadTweener;
            return nextTweener;
        }
        /// <summary>
        /// 设置归一化缓动曲线，定义域[0, 1]，值域[0, 1]
        /// </summary>
        /// <param name="animationCurve">缓动曲线</param>
        public Tweener SetEase(AnimationCurve animationCurve)
        {
            Ease = animationCurve;
            return this;
        }

        /// <summary>
        /// 从当前实例的 Head 实例出发，向 NextTweener 方向查找，将第一个能够停止的 Tweener 停止
        /// </summary>
        /// <param name="selectTweener">实际停止的 Tweener 实例或 null</param>
        /// <returns>成功停止了任何的 Tweener</returns>
        public bool FromHeadToEndIfNeedStop(out Tweener selectTweener)
        {
#if UNITY_EDITOR
            if (TweenService.Instance != null)
                TweenService.Instance.Behaviour.StopTweenerTimes++;
#endif
            foreach (Tweener tweener in this)
            {
                switch (tweener.State)
                {
                    case TweenerState.Error:
                    case TweenerState.AssetHaveBeenDestroy:
                    case TweenerState.Stop:
                        selectTweener = tweener;
                        return false;
                    case TweenerState.IsRunnning:
                    case TweenerState.WaitForActivation:
                        selectTweener = tweener;
                        TweenService.Remove(tweener);
                        return true;
                    default:
                        break;
                }
            }
            selectTweener = null;
            return false;
        }
        /// <summary>
        /// 从当前实例的 Head 实例出发，向 NextTweener 方向查找，将第一个<see cref="TweenerState.Stop"/>状态的 Tweener 重新运行
        /// </summary>
        /// <param name="selectTweener">如果成功找到一个 Tweener，则返回实例</param>
        /// <returns>是否成功找到<see cref="TweenerState.Stop"/>状态的 Tweener</returns>
        public bool FromHeadToEndIfNeedRestore(out Tweener selectTweener)
        {
#if UNITY_EDITOR
            if (TweenService.Instance != null)
                TweenService.Instance.Behaviour.RestoreTweenerTimes++;
#endif
            foreach (Tweener tweener in this)
            {
                switch (tweener.State)
                {
                    case TweenerState.Error:
                    case TweenerState.AssetHaveBeenDestroy:
                        selectTweener = tweener;
                        return false;
                    case TweenerState.Stop:
                        selectTweener = tweener;
                        TweenService.Restore(tweener);
                        return true;
                    default:
                        break;
                }
            }
            selectTweener = null;
            return false;
        }

        internal void UpdateTween() => OnUpdate_Handle?.Invoke();
        internal virtual void CompleteTween() => OnComplete_Handle?.Invoke();

        /* IEnumerable */
        public IEnumerator<Tweener> GetEnumerator()
        {
            Tweener tweener = HeadTweener;
            do
            {
                yield return tweener;
                tweener = tweener.NextTweener;
            } while (tweener != null);
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}