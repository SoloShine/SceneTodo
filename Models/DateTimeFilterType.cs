namespace SceneTodo.Models;

/// <summary>
/// 日期筛选类型枚举
/// </summary>
public enum DateTimeFilterType
{
    /// <summary>
    /// 全部
    /// </summary>
    All,
    
    /// <summary>
    /// 今天
    /// </summary>
    Today,
    
    /// <summary>
    /// 本周
    /// </summary>
    ThisWeek,
    
    /// <summary>
    /// 本月
    /// </summary>
    ThisMonth,
    
    /// <summary>
    /// 已过期
    /// </summary>
    Overdue,
    
    /// <summary>
    /// 自定义范围
    /// </summary>
    Custom
}
