namespace Tween
{
    /// <summary>
    /// 表述当前 Tweener 实例的状态
    /// </summary>
    public enum TweenerState
    {
        /// <summary>
        /// 还未启用的 Tweener 实例
        /// </summary>
        WaitForActivation = 0,
        IsRunnning = 1,
        Stop = 2,
        ReservedField2 = 4,
        /// <summary>
        /// Tweener 执行完成或即将完成，这个实例将在这一帧调用完成回掉
        /// <para>最后一次 OnComplete 执行前，状态变更为 Finish</para>
        /// </summary>
        Finish = 10,
        /// <summary>
        /// Tweener 执行时绑定的资源被摧毁或失效，这个实例被抛弃了
        /// 这个状态算作错误，但不会抛出异常
        /// <para>它不能变更为停止状态，也不能变更到执行状态</para>
        /// </summary>
        AssetHaveBeenDestroy = -1,
        /// <summary>
        /// Tweener 执行时出现了错误，这个实例被抛弃了
        /// <para>它不能变更为停止状态，也不能变更到执行状态</para>
        /// </summary>
        Error = -2,
    }
}