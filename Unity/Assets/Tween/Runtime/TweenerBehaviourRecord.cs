namespace Tween
{
    public struct TweenerBehaviourRecord
    {
        /* field */
        /// <summary>
        /// Tweener 实例尝试添加到执行队列的次数
        /// </summary>
        public float AddTweenerTimes;
        /// <summary>
        /// TweenIncentive 成功添加到执行队列的次数
        /// </summary>
        public float AddTweenerSuccessTimes;
        /// <summary>
        /// Tweener 实例尝试暂停执行的次数
        /// </summary>
        public float StopTweenerTimes;
        /// <summary>
        /// TweenIncentive 成功暂停执行的次数
        /// </summary>
        public float StopTweenerSuccessTimes;
        /// <summary>
        /// Tweener 实例尝试恢复执行的次数
        /// </summary>
        public float RestoreTweenerTimes;
        /// <summary>
        /// TweenIncentive 成功恢复执行的次数
        /// </summary>
        public float RestoreTweenerSuccessTimes;

        /* oeprator */
        public static TweenerBehaviourRecord operator +(TweenerBehaviourRecord left, TweenerBehaviourRecord right)
        {
            return new TweenerBehaviourRecord()
            {
                AddTweenerTimes = left.AddTweenerTimes + right.AddTweenerTimes,
                AddTweenerSuccessTimes = left.AddTweenerSuccessTimes + right.AddTweenerSuccessTimes,
                StopTweenerTimes = left.StopTweenerTimes + right.StopTweenerTimes,
                StopTweenerSuccessTimes = left.StopTweenerSuccessTimes + right.StopTweenerSuccessTimes,
                RestoreTweenerTimes = left.RestoreTweenerTimes + right.RestoreTweenerTimes,
                RestoreTweenerSuccessTimes = left.RestoreTweenerSuccessTimes + right.RestoreTweenerSuccessTimes,
            };
        }
        public static TweenerBehaviourRecord operator /(TweenerBehaviourRecord value, float ratio)
        {
            return new TweenerBehaviourRecord()
            {
                AddTweenerTimes = value.AddTweenerTimes / ratio,
                AddTweenerSuccessTimes = value.AddTweenerSuccessTimes / ratio,
                StopTweenerTimes = value.StopTweenerTimes / ratio,
                StopTweenerSuccessTimes = value.StopTweenerSuccessTimes / ratio,
                RestoreTweenerTimes = value.RestoreTweenerTimes / ratio,
                RestoreTweenerSuccessTimes = value.RestoreTweenerSuccessTimes / ratio,
            };
        }
    }
}