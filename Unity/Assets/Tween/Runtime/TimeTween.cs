using System;
using System.Collections.Generic;

namespace Tween
{
    public static class TimeTween
    {
        internal static IEnumerator<Tweener> DoTime_Internal(TimeTweener tweener)
        {
            tweener.MoveForward();
            while (tweener.Normalized < 1f)
            {
                yield return tweener;
                tweener.MoveForward();
            }
        }
        public static Tweener DoTime(float duration)
        {
            if (duration < 0f)
                throw new ArgumentException(nameof(duration));

            TimeTweener tweener = new TimeTweener()
            {
                DurationTime = duration,
                TimeType = TweenerTimeType.ScaleTime,
            };
            tweener.Enumerator = DoTime_Internal(tweener);
            return tweener;
        }

        internal static IEnumerator<Tweener> DoUnscaleTime_Internal(TimeTweener tweener)
        {
            tweener.MoveForward();
            while (tweener.Normalized < 1f)
            {
                yield return tweener;
                tweener.MoveForward();
            }
        }
        public static Tweener DoUnscaleTime(float duration)
        {
            if (duration < 0f)
                throw new ArgumentException(nameof(duration));

            TimeTweener tweener = new TimeTweener()
            {
                DurationTime = duration,
                TimeType = TweenerTimeType.UnscaleTime,
            };
            tweener.Enumerator = DoUnscaleTime_Internal(tweener);
            return tweener;
        }

        internal static IEnumerator<Tweener> DoFrame_Internal(ProgressTweener tweener)
        {
            tweener.Progress++;
            while (tweener.Normalized < 1f)
            {
                yield return tweener;
                tweener.Progress++;
            }
        }
        public static Tweener DoFrame(int count)
        {
            if (count < 0)
                throw new ArgumentException(nameof(count));

            ProgressTweener tweener = new ProgressTweener()
            {
                Target = count,
            };
            tweener.Enumerator = DoFrame_Internal(tweener);
            return tweener;
        }
    }
}