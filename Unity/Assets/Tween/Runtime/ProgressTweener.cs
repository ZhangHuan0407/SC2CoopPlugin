namespace Tween
{
    public class ProgressTweener : Tweener
    {
        /* field */
        /// <summary>
        /// 当前 Tweener 实例的执行进度
        /// </summary>
        public float Progress { get; internal set; }
        /// <summary>
        /// 当前 Tweener 实例的目标执行进度，不应当在实例准备完成后修改此值
        /// <para>小于 0 视作错误</para>
        /// </summary>
        public float Target { get; internal set; }

        /* inter */
        public override float Normalized
        {
            get
            {
                if (Progress > Target)
                    return 1;
                else
                    return Progress / Target;
            }
        }

        /* ctor */
        public ProgressTweener() : base()
        {
            Progress = 0f;
        }

        /* func */
        internal override void CompleteTween()
        {
            base.CompleteTween();
            if (NextTweener != null
                && NextTweener.State == TweenerState.WaitForActivation)
                TweenService.Add(NextTweener);
        }
    }
}