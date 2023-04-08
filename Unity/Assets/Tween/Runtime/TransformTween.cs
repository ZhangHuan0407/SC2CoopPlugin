using System;
using System.Collections.Generic;
using UnityEngine;

namespace Tween
{
    public static class TransformTween
    {
        internal static IEnumerator<Tweener> DoPosition_Internal(TimeTweener tweener, Transform transform,
            Vector3 targetPosition, Vector3? overrideStartPosition)
        {
            if (!transform)
            {
                yield return null;
                yield break;
            }

            tweener.MoveForward();
            Vector3 startPosition = overrideStartPosition ?? transform.position;
            transform.position = tweener.Evaluate * (targetPosition - startPosition) + startPosition;

            while (tweener.Normalized < 1f)
            {
                yield return tweener;
                if (!transform)
                {
                    yield return null;
                    yield break;
                }
                tweener.MoveForward();
                transform.position = tweener.Evaluate * (targetPosition - startPosition) + startPosition;
            }
        }
        public static Tweener DoPosition(this Transform transform,
            Vector3 targetPosition, float duration, Vector3? overrideStartPosition = null)
        {
            if (transform == null)
                throw new ArgumentNullException(nameof(transform));
            if (duration < 0f)
                throw new ArgumentException(nameof(duration));

            TimeTweener tweener = new TimeTweener()
            {
                DurationTime = duration,
                TimeType = TweenerTimeType.ScaleTime,
            };
            tweener.Enumerator = DoPosition_Internal(tweener, transform, targetPosition, overrideStartPosition);
            return tweener;
        }

        internal static IEnumerator<Tweener> DoLocalPosition_Internal(TimeTweener tweener, Transform transform,
            Vector3 targetPosition, Vector3? overrideStartLocalPosition)
        {
            if (!transform)
            {
                yield return null;
                yield break;
            }

            tweener.MoveForward();
            Vector3 startPosition = overrideStartLocalPosition ?? transform.localPosition;
            transform.localPosition = tweener.Evaluate * (targetPosition - startPosition) + startPosition;

            while (tweener.Normalized < 1f)
            {
                yield return tweener;
                if (!transform)
                {
                    yield return null;
                    yield break;
                }
                tweener.MoveForward();
                transform.localPosition = tweener.Evaluate * (targetPosition - startPosition) + startPosition;
            }
        }
        public static Tweener DoLocalPosition(this Transform transform,
            Vector3 targetPosition, float duration, Vector3? overrideStartLocalPosition = null)
        {
            if (transform == null)
                throw new ArgumentNullException(nameof(transform));
            if (duration < 0f)
                throw new ArgumentException(nameof(duration));

            TimeTweener tweener = new TimeTweener()
            {
                DurationTime = duration,
                TimeType = TweenerTimeType.ScaleTime,
            };
            tweener.Enumerator = DoLocalPosition_Internal(tweener, transform, targetPosition, overrideStartLocalPosition);
            return tweener;
        }

        private static IEnumerator<Tweener> DoLocalPosX_Internal(TimeTweener tweener, Transform transform,
            float targetPosX, float? overrideStartLocalPosX)
        {
            if (!transform)
            {
                yield return null;
                yield break;
            }

            tweener.MoveForward();
            float startLocalPosX = overrideStartLocalPosX ?? transform.localPosition.x;
            Vector3 localPosition = transform.localPosition;
            localPosition.x = tweener.Evaluate * (targetPosX - startLocalPosX) + startLocalPosX;
            transform.localPosition = localPosition;

            while (tweener.Normalized < 1f)
            {
                yield return tweener;
                if (!transform)
                {
                    yield return null;
                    yield break;
                }
                tweener.MoveForward();
                localPosition = transform.localPosition;
                localPosition.x = tweener.Evaluate * (targetPosX - startLocalPosX) + startLocalPosX;
                transform.localPosition = localPosition;
            }
        }
        public static Tweener DoLocalPosX(this Transform transform,
            float targetPosX, float duration, float? overrideStartLocalPosX = null)
        {
            if (transform == null)
                throw new ArgumentNullException(nameof(transform));
            if (duration < 0f)
                throw new ArgumentException(nameof(duration));

            TimeTweener tweener = new TimeTweener()
            {
                DurationTime = duration,
                TimeType = TweenerTimeType.ScaleTime,
            };
            tweener.Enumerator = DoLocalPosX_Internal(tweener, transform, targetPosX, overrideStartLocalPosX);
            return tweener;
        }

        private static IEnumerator<Tweener> DoLocalPosY_Internal(TimeTweener tweener, Transform transform,
            float targetPosY, float? overrideStartLocalPosY)
        {
            if (!transform)
            {
                yield return null;
                yield break;
            }

            tweener.MoveForward();
            float startLocalPosY = overrideStartLocalPosY ?? transform.localPosition.y;
            Vector3 localPosition = transform.localPosition;
            localPosition.y = tweener.Evaluate * (targetPosY - startLocalPosY) + startLocalPosY;
            transform.localPosition = localPosition;

            while (tweener.Normalized < 1f)
            {
                yield return tweener;
                if (!transform)
                {
                    yield return null;
                    yield break;
                }
                tweener.MoveForward();
                localPosition = transform.localPosition;
                localPosition.y = tweener.Evaluate * (targetPosY - startLocalPosY) + startLocalPosY;
                transform.localPosition = localPosition;
            }
        }
        public static Tweener DoLocalPosY(this Transform transform,
            float targetPosY, float duration, float? overrideStartLocalPosY = null)
        {
            if (transform == null)
                throw new ArgumentNullException(nameof(transform));
            if (duration < 0f)
                throw new ArgumentException(nameof(duration));

            TimeTweener tweener = new TimeTweener()
            {
                DurationTime = duration,
                TimeType = TweenerTimeType.ScaleTime,
            };
            tweener.Enumerator = DoLocalPosY_Internal(tweener, transform, targetPosY, overrideStartLocalPosY);
            return tweener;
        }

        private static IEnumerator<Tweener> DoLocalPosZ_Internal(TimeTweener tweener, Transform transform,
            float targetPosZ, float? overrideStartLocalPosZ)
        {
            if (!transform)
            {
                yield return null;
                yield break;
            }

            tweener.MoveForward();
            float startLocalPosZ = overrideStartLocalPosZ ?? transform.localPosition.z;
            Vector3 localPosition = transform.localPosition;
            localPosition.z = tweener.Evaluate * (targetPosZ - startLocalPosZ) + startLocalPosZ;
            transform.localPosition = localPosition;

            while (tweener.Normalized < 1f)
            {
                yield return tweener;
                if (!transform)
                {
                    yield return null;
                    yield break;
                }
                tweener.MoveForward();
                localPosition = transform.localPosition;
                localPosition.z = tweener.Evaluate * (targetPosZ - startLocalPosZ) + startLocalPosZ;
                transform.localPosition = localPosition;
            }
        }
        public static Tweener DoLocalPosZ(this Transform transform,
            float targetPosZ, float duration, float? overrideStartLocalPosZ = null)
        {
            if (transform == null)
                throw new ArgumentNullException(nameof(transform));
            if (duration < 0f)
                throw new ArgumentException(nameof(duration));

            TimeTweener tweener = new TimeTweener()
            {
                DurationTime = duration,
                TimeType = TweenerTimeType.ScaleTime,
            };
            tweener.Enumerator = DoLocalPosZ_Internal(tweener, transform, targetPosZ, overrideStartLocalPosZ);
            return tweener;
        }

        internal static IEnumerator<Tweener> DoLocalScale_Internal(TimeTweener tweener, Transform transform, Vector3 targetLocalScale, Vector3? overrideStartLocalScale)
        {
            if (!transform)
            {
                yield return null;
                yield break;
            }

            tweener.MoveForward();
            Vector3 startLocalScale = overrideStartLocalScale ?? transform.localScale;
            transform.localScale = tweener.Evaluate * (targetLocalScale - startLocalScale) + startLocalScale;

            while (tweener.Normalized < 1f)
            {
                yield return tweener;
                if (!transform)
                {
                    yield return null;
                    yield break;
                }
                tweener.MoveForward();
                transform.localScale = tweener.Evaluate * (targetLocalScale - startLocalScale) + startLocalScale;
            }
        }
        public static Tweener DoLocalScale(this Transform transform, Vector3 targetLocalScale, float duration, Vector3? overrideStartLocalScale = null)
        {
            if (transform == null)
                throw new ArgumentNullException(nameof(transform));
            if (duration < 0f)
                throw new ArgumentException(nameof(duration));

            TimeTweener tweener = new TimeTweener()
            {
                DurationTime = duration,
                TimeType = TweenerTimeType.ScaleTime,
            };
            tweener.Enumerator = DoLocalScale_Internal(tweener, transform, targetLocalScale, overrideStartLocalScale);
            return tweener;
        }

        internal static IEnumerator<Tweener> DoLocalEuler_Internal(TimeTweener tweener, Transform transform, Vector3 targetEuler, Vector3 startEuler)
        {
            if (!transform)
            {
                yield return null;
                yield break;
            }

            tweener.MoveForward();
            transform.localEulerAngles = tweener.Evaluate * (targetEuler - startEuler) + startEuler;

            while (tweener.Normalized < 1f)
            {
                yield return tweener;
                if (!transform)
                {
                    yield return null;
                    yield break;
                }
                tweener.MoveForward();
                transform.localEulerAngles = tweener.Evaluate * (targetEuler - startEuler) + startEuler;
            }
        }
        /// <summary>
        /// 对一个 Transform 对象创建补间动画实例
        /// </summary>
        /// <param name="transform">执行补间动画的实例</param>
        /// <param name="targetEuler">结束时欧拉角度</param>
        /// <param name="duration">旋转持续时间</param>
        /// <param name="startEuler">因为存在优角劣角问题，不能自行获取起始欧拉角度</param>
        /// <returns>Tweener 实例</returns>
        public static Tweener DoLocalEuler(this Transform transform, Vector3 targetEuler, float duration, Vector3 startEuler)
        {
            if (transform == null)
                throw new ArgumentNullException(nameof(transform));
            if (duration < 0f)
                throw new ArgumentException(nameof(duration));

            TimeTweener tweener = new TimeTweener()
            {
                DurationTime = duration,
                TimeType = TweenerTimeType.ScaleTime,
            };
            tweener.Enumerator = DoLocalEuler_Internal(tweener, transform, targetEuler, startEuler);
            return tweener;
        }
    }
}