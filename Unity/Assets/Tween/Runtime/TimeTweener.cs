using UnityEngine;

namespace Tween
{
    /// <summary>
    /// <see cref="TimeTweener"/> 实例时间单位类型
    /// </summary>
    public enum TweenerTimeType
    {
        Mix,
        UnscaleTime,
        ScaleTime,
    }
    public class TimeTweener : Tweener
    {
        /* field */
        /// <summary>
        /// 当前 Tweener 实例的累计执行时间，最后一帧超过 DurationTime 时间
        /// </summary>
        public float CumulativeTime { get; internal set; }
        /// <summary>
        /// 当前 Tweener 实例的目标执行时间，不应当在实例准备完成后修改此值
        /// <para>小于 0 视作错误</para>
        /// </summary>
        public float DurationTime { get; internal set; }
        /// <summary>
        /// 当前 TimeTweener 实例的时间单位类型
        /// </summary>
        public TweenerTimeType TimeType { get; internal set; }

        /* inter */
        public override float Normalized
        {
            get
            {
                if (CumulativeTime > DurationTime || DurationTime == 0f)
                    return 1;
                else
                    return CumulativeTime / DurationTime;
            }
        }

        /* ctor */
        public TimeTweener() : base()
        {
            CumulativeTime = 0f;
            TimeType = TweenerTimeType.ScaleTime;
        }

        /* func */
        internal override void CompleteTween()
        {
            base.CompleteTween();
            if (NextTweener != null
                && NextTweener.State == TweenerState.WaitForActivation)
            {
                if (NextTweener is TimeTweener timeTweener
                    && TimeType != TweenerTimeType.Mix
                    && TimeType == timeTweener.TimeType)
                    timeTweener.CumulativeTime = CumulativeTime - DurationTime;
                TweenService.Add(NextTweener);
            }
        }

        internal void MoveForward()
        {
            CumulativeTime += TimeType == TweenerTimeType.UnscaleTime ? Time.unscaledDeltaTime : Time.deltaTime;
        }
    }
}