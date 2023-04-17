using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tween
{
    public class WaitTween
    {
        internal static IEnumerator<Tweener> DoWaitUntil_Internal(ProgressTweener tweener, Func<bool> waitUntil)
        {
            while (!waitUntil())
            {
                yield return tweener;
            }
            tweener.Progress = 1f;
        }
        public static Tweener WaitUntil(Func<bool> waitUntil)
        {
            if (waitUntil == null)
                throw new ArgumentNullException(nameof(waitUntil));

            ProgressTweener tweener = new ProgressTweener()
            {
                Progress = 0f,
                Target = 1f,
            };
            tweener.Enumerator = DoWaitUntil_Internal(tweener, waitUntil);
            return tweener;
        }

        internal static IEnumerator<Tweener> DoWaitWhile_Internal(ProgressTweener tweener, Func<bool> waitWhile)
        {
            while (waitWhile())
            {
                yield return tweener;
            }
            tweener.Progress = 1f;
        }
        public static Tweener WaitWhile(Func<bool> waitWhile)
        {
            if (waitWhile == null)
                throw new ArgumentNullException(nameof(waitWhile));

            ProgressTweener tweener = new ProgressTweener()
            {
                Progress = 0f,
                Target = 1f,
            };
            tweener.Enumerator = DoWaitWhile_Internal(tweener, waitWhile);
            return tweener;
        }

        internal static IEnumerator<Tweener> DoBothAreFinish_Internal(ProgressTweener tweener, Tweener[] tweeners)
        {
            Tweener[] copy = new Tweener[tweeners.Length];
            tweeners.CopyTo(copy, 0);
            for (int i = 0; i < copy.Length; i++)
            {
                switch (copy[i].State)
                {
                    case TweenerState.WaitForActivation:
                    case TweenerState.IsRunnning:
                        yield return tweener;
                        break;
                    case TweenerState.Finish:
                        tweener.Progress++;
                        break;
                    case TweenerState.AssetHaveBeenDestroy:
                    case TweenerState.Error:
                        yield return null;
                        break;
                    default:
                        throw new NotImplementedException(copy[i].State.ToString());
                }
            }
        }
        public static Tweener BothAreFinish(params Tweener[] tweeners)
        {
            if (tweeners == null)
                throw new ArgumentNullException(nameof(tweeners));
            foreach (Tweener tweener in tweeners)
            {
                if (tweener == null)
                    throw new ArgumentException(nameof(tweeners));
            }

            ProgressTweener result = new ProgressTweener()
            {
                Progress = 0f,
                Target = tweeners.Length,
            };
            result.Enumerator = DoBothAreFinish_Internal(result, tweeners);
            return result;
        }
    }
}