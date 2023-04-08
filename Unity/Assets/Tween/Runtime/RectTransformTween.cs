using System;
using System.Collections.Generic;
using UnityEngine;

namespace Tween
{
    public static class RectTransformTween
    {
        internal static IEnumerator<Tweener> DoAnchoredPosition_Internal(TimeTweener tweener, RectTransform rectTransform,
            Vector2 targetPosition, Vector2? overrideStartAnchoredPosition)
        {
            if (!rectTransform)
            {
                yield return null;
                yield break;
            }

            tweener.MoveForward();
            Vector2 startPosition = overrideStartAnchoredPosition ?? rectTransform.anchoredPosition;
            rectTransform.anchoredPosition = tweener.Evaluate * (targetPosition - startPosition) + startPosition;

            while (tweener.Normalized < 1f)
            {
                yield return tweener;
                if (!rectTransform)
                {
                    yield return null;
                    yield break;
                }
                tweener.MoveForward();
                rectTransform.anchoredPosition = tweener.Evaluate * (targetPosition - startPosition) + startPosition;
            }
        }
        public static Tweener DoAnchoredPosition(this RectTransform rectTransform,
            Vector2 targetPosition, float duration, Vector2? overrideStartAnchoredPosition = null)
        {
            if (rectTransform == null)
                throw new ArgumentNullException(nameof(rectTransform));
            if (duration < 0f)
                throw new ArgumentException(nameof(duration));

            TimeTweener tweener = new TimeTweener()
            {
                DurationTime = duration,
                TimeType = TweenerTimeType.ScaleTime,
            };
            tweener.Enumerator = DoAnchoredPosition_Internal(tweener, rectTransform, targetPosition, overrideStartAnchoredPosition);
            return tweener;
        }

        internal static IEnumerator<Tweener> DoAnchoredPosX_Internal(TimeTweener tweener, RectTransform rectTransform,
            float targetPosX, float? overrideStartAnchoredPosX)
        {
            if (!rectTransform)
            {
                yield return null;
                yield break;
            }

            tweener.MoveForward();
            Vector2 anchoredPos = rectTransform.anchoredPosition;
            float startAnchoredPosX = overrideStartAnchoredPosX ?? anchoredPos.x;
            anchoredPos.x = tweener.Evaluate * (targetPosX - startAnchoredPosX) + startAnchoredPosX;
            rectTransform.anchoredPosition = anchoredPos;

            while (tweener.Normalized < 1f)
            {
                yield return tweener;
                if (!rectTransform)
                {
                    yield return null;
                    yield break;
                }
                tweener.MoveForward();
                anchoredPos = rectTransform.anchoredPosition;
                anchoredPos.x = tweener.Evaluate * (targetPosX - startAnchoredPosX) + startAnchoredPosX;
                rectTransform.anchoredPosition = anchoredPos;
            }
        }
        public static Tweener DoAnchoredPosX(this RectTransform rectTransform,
            float targetPosX, float duration, float? overrideStartAnchoredPosX = null)
        {
            if (rectTransform == null)
                throw new ArgumentNullException(nameof(rectTransform));
            if (duration < 0f)
                throw new ArgumentException(nameof(duration));

            TimeTweener tweener = new TimeTweener()
            {
                DurationTime = duration,
                TimeType = TweenerTimeType.ScaleTime,
            };
            tweener.Enumerator = DoAnchoredPosX_Internal(tweener, rectTransform, targetPosX, overrideStartAnchoredPosX);
            return tweener;
        }

        internal static IEnumerator<Tweener> DoAnchoredPosY_Internal(TimeTweener tweener, RectTransform rectTransform,
            float targetPosY, float? overrideStartAnchoredPosY)
        {
            if (!rectTransform)
            {
                yield return null;
                yield break;
            }

            tweener.MoveForward();
            Vector2 anchoredPos = rectTransform.anchoredPosition;
            float startAnchoredPosY = overrideStartAnchoredPosY ?? anchoredPos.y;
            anchoredPos.y = tweener.Evaluate * (targetPosY - startAnchoredPosY) + startAnchoredPosY;
            rectTransform.anchoredPosition = anchoredPos;

            while (tweener.Normalized < 1f)
            {
                yield return tweener;
                if (!rectTransform)
                {
                    yield return null;
                    yield break;
                }
                tweener.MoveForward();
                anchoredPos = rectTransform.anchoredPosition;
                anchoredPos.y = tweener.Evaluate * (targetPosY - startAnchoredPosY) + startAnchoredPosY;
                rectTransform.anchoredPosition = anchoredPos;
            }
        }
        public static Tweener DoAnchoredPosY(this RectTransform rectTransform,
            float targetPosY, float duration, float? overrideStartAnchoredPosY = null)
        {
            if (rectTransform == null)
                throw new ArgumentNullException(nameof(rectTransform));
            if (duration < 0f)
                throw new ArgumentException(nameof(duration));

            TimeTweener tweener = new TimeTweener()
            {
                DurationTime = duration,
                TimeType = TweenerTimeType.ScaleTime,
            };
            tweener.Enumerator = DoAnchoredPosY_Internal(tweener, rectTransform, targetPosY, overrideStartAnchoredPosY);
            return tweener;
        }

        internal static IEnumerator<Tweener> DoSizeDelta_Internal(TimeTweener tweener, RectTransform rectTransform,
            Vector2 targetSizeDelta, Vector2? overrideSizeDelta)
        {
            if (!rectTransform)
            {
                yield return null;
                yield break;
            }

            tweener.MoveForward();
            Vector2 startSizeDelta = overrideSizeDelta ?? rectTransform.sizeDelta;
            rectTransform.sizeDelta = tweener.Evaluate * (targetSizeDelta - startSizeDelta) + startSizeDelta;

            while (tweener.Normalized < 1f)
            {
                yield return tweener;
                if (!rectTransform)
                {
                    yield return null;
                    yield break;
                }
                tweener.MoveForward();
                rectTransform.sizeDelta = tweener.Evaluate * (targetSizeDelta - startSizeDelta) + startSizeDelta;
            }
        }
        public static Tweener DoSizeDelta(this RectTransform rectTransform,
            Vector2 sizeDelta, float duration, Vector2? overrideSizeDelta = null)
        {
            if (rectTransform == null)
                throw new ArgumentNullException(nameof(rectTransform));
            if (duration < 0f)
                throw new ArgumentException(nameof(duration));

            TimeTweener tweener = new TimeTweener()
            {
                DurationTime = duration,
                TimeType = TweenerTimeType.ScaleTime,
            };
            tweener.Enumerator = DoSizeDelta_Internal(tweener, rectTransform, sizeDelta, overrideSizeDelta);
            return tweener;
        }
    }
}