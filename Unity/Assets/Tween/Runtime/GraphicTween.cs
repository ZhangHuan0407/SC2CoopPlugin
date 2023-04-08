using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Tween
{
    public static class GraphicTween
    {
        internal static IEnumerator<Tweener> DoColor_Internal(TimeTweener tweener, Graphic graphic, Color targetColor, Color? overrideStartColor)
        {
            if (!graphic)
            {
                yield return null;
                yield break;
            }

            tweener.MoveForward();
            Color startColor = overrideStartColor ?? graphic.color;
            graphic.color = tweener.Evaluate * (targetColor - startColor) + startColor;

            while (tweener.Normalized < 1f)
            {
                yield return tweener;
                if (!graphic)
                {
                    yield return null;
                    yield break;
                }
                tweener.MoveForward();
                graphic.color = tweener.Evaluate * (targetColor - startColor) + startColor;
            }
        }
        public static Tweener DoColor(this Graphic graphic, Color targetColor, float duration, Color? overrideStartColor = null)
        {
            if (graphic == null)
                throw new ArgumentNullException(nameof(graphic));
            if (duration < 0f)
                throw new ArgumentException(nameof(duration));

            TimeTweener tweener = new TimeTweener()
            {
                DurationTime = duration,
                TimeType = TweenerTimeType.ScaleTime,
            };
            tweener.Enumerator = DoColor_Internal(tweener, graphic, targetColor, overrideStartColor);
            return tweener;
        }

        internal static IEnumerator<Tweener> DoFade_Internal(TimeTweener tweener, Graphic graphic,
            float targetAlpha, float? overrideStartAlpha)
        {
            if (!graphic)
            {
                yield return null;
                yield break;
            }

            tweener.MoveForward();
            Color color = graphic.color;
            float startAlpha = overrideStartAlpha ?? color.a;
            color.a = tweener.Evaluate * (targetAlpha - startAlpha) + startAlpha;
            graphic.color = color;

            while (tweener.Normalized < 1f)
            {
                yield return tweener;
                if (!graphic)
                {
                    yield return null;
                    yield break;
                }
                tweener.MoveForward();
                color = graphic.color;
                color.a = tweener.Evaluate * (targetAlpha - startAlpha) + startAlpha;
                graphic.color = color;
            }
        }
        public static Tweener DoFade(this Graphic graphic, float targetAlpha, float duration, float? overrideAlpha = null)
        {
            if (graphic == null)
                throw new ArgumentNullException(nameof(graphic));
            if (duration < 0f)
                throw new ArgumentException(nameof(duration));

            TimeTweener tweener = new TimeTweener()
            {
                DurationTime = duration,
                TimeType = TweenerTimeType.ScaleTime,
            };
            tweener.Enumerator = DoFade_Internal(tweener, graphic, targetAlpha, overrideAlpha);
            return tweener;
        }

        internal static IEnumerator<Tweener> DoFillAmount_Internal(TimeTweener tweener, Image image, float fillAmount, float? overrideStartFillAmout)
        {
            if (!image)
            {
                yield return null;
                yield break;
            }

            tweener.MoveForward();
            float startFillAmount = overrideStartFillAmout ?? image.fillAmount;
            image.fillAmount = tweener.Evaluate * (fillAmount - startFillAmount) + startFillAmount;

            while (tweener.Normalized < 1f)
            {
                yield return tweener;
                if (!image)
                {
                    yield return null;
                    yield break;
                }
                tweener.MoveForward();
                image.fillAmount = tweener.Evaluate * (fillAmount - startFillAmount) + startFillAmount;
            }
        }
        public static Tweener DoFillAmount(this Image image, float fillAmount, float duration, float? overrideStartFillAmout = null)
        {
            if (image == null)
                throw new ArgumentNullException(nameof(image));
            if (fillAmount < 0f)
                throw new ArgumentException(nameof(fillAmount));
            else if (fillAmount > 1f)
                fillAmount = 1f;
            if (duration < 0f)
                throw new ArgumentException(nameof(duration));

            TimeTweener tweener = new TimeTweener()
            {
                DurationTime = duration,
                TimeType = TweenerTimeType.ScaleTime,
            };
            tweener.Enumerator = DoFillAmount_Internal(tweener, image, fillAmount, overrideStartFillAmout);
            return tweener;
        }
    }
}