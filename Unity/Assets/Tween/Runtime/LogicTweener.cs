using System;
using System.Collections.Generic;
using UnityEngine;

namespace Tween
{
    public class LogicTweener : Tweener
    {
        /* field */

        /* inter */
        public override float Normalized => State == TweenerState.Finish ? 2f : 0;

        /* ctor */
        public LogicTweener() : base()
        {
        }

        /* func */
        internal override void CompleteTween()
        {
            base.CompleteTween();
            if (NextTweener != null
                && NextTweener.State == TweenerState.WaitForActivation)
                TweenService.Add(NextTweener);
        }

        /// <summary>
        /// 逻辑迭代器，相当于携程。同一个 <see cref="LogicTweener"/> 只能设置一次逻辑
        /// <para>可以返回其它的 <see cref="Tweener"/> 实例，将尝试等待他们</para>
        /// <para>不能返回 null，但可以返回实例自身，当作延迟一帧使用</para>
        /// </summary>
        /// <param name="enumerator">一帧一调用的逻辑</param>
        /// <returns><see cref="Tweener"/> 实例自身</returns>
        public Tweener SetLogic(IEnumerator<Tweener> enumerator)
        {
            if (enumerator is null)
                throw new ArgumentNullException(nameof(enumerator));
            else if (Enumerator != null)
                throw new ArgumentException(nameof(Enumerator));

            Enumerator = RunLogic();
            return this;

            IEnumerator<Tweener> RunLogic()
            {
                Action awakeLogicTweener = () => TweenService.RestoreImmediately(this);
                Tweener rouser = null;
                while (enumerator.MoveNext())
                {
                    Tweener tweener = enumerator.Current;
                    TweenerState state = tweener.State;
                    // 普通的等待一帧
                    if (tweener == this)
                        yield return this;
                    switch (state)
                    {
                        // 一个还未启动的 Tweener，立即启动它，并添加完成时回调激活当前 Tweener
                        case TweenerState.WaitForActivation:
                            TweenService.Remove(this);
                            tweener.OnComplete_Handle += awakeLogicTweener;
                            rouser = tweener;
                            TweenService.Add(tweener);
                            yield return null;
                            break;
                        // 一个正在运行的 Tweener，添加完成时回调激活当前 Tweener 
                        case TweenerState.IsRunnning:
                        case TweenerState.Stop:
                            TweenService.Remove(this);
                            tweener.OnComplete_Handle += awakeLogicTweener;
                            rouser = tweener;
                            yield return null;
                            break;
                        // 返回一个已完成的，立即继续前进
                        case TweenerState.Finish:
                            continue;
                        // 一个出现错误无法继续执行的 Tweener，自身变更为 AssetHaveBeenDestroy
                        case TweenerState.AssetHaveBeenDestroy:
                        case TweenerState.Error:
                            yield return null;
                            break;
                        case TweenerState.ReservedField2:
                        default:
                            throw new Exception($"Not defined operator in {nameof(LogicTweener)}, {nameof(state)} is {state}.");
                    }
                    // 不论以何种原因苏醒，应当注销监听tweener的完成激励
                    if (rouser != null)
                        rouser.OnComplete_Handle -= awakeLogicTweener;
                }
            }
        }
    }
}