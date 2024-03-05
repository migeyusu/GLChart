namespace GLChart.Samples.Charts;

/// <summary>
/// 图表交互模式
/// </summary>
public enum ChartInteractMode
{
    /// <summary>
    /// 手动缩放
    /// </summary>
    Manual = 0,

    /// <summary>
    /// 自动全局视图
    /// </summary>
    AutoAll = 1,

    /// <summary>
    /// 自动滑动，锁定视图X轴为X轴边界最后特定间隔
    /// </summary>
    AutoScroll = 2,

    /// <summary>
    /// 历史回溯
    /// </summary>
    History = 3,
}