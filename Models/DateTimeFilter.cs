namespace SceneTodo.Models;

/// <summary>
/// 日期时间筛选条件
/// </summary>
public class DateTimeFilter
{
    /// <summary>
    /// 筛选类型
    /// </summary>
    public DateTimeFilterType Type { get; set; } = DateTimeFilterType.All;
    
    /// <summary>
    /// 开始日期（用于自定义范围）
    /// </summary>
    public DateTime? StartDate { get; set; }
    
    /// <summary>
    /// 结束日期（用于自定义范围）
    /// </summary>
    public DateTime? EndDate { get; set; }
}
