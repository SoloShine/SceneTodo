namespace SceneTodo.Models;

/// <summary>
/// 搜索筛选条件模型
/// </summary>
public class SearchFilter
{
    /// <summary>
    /// 搜索关键词
    /// </summary>
    public string? SearchText { get; set; }
    
    /// <summary>
    /// 优先级筛选
    /// </summary>
    public List<Priority>? Priorities { get; set; }
    
    /// <summary>
    /// 完成状态筛选
    /// </summary>
    public CompletionStatus? CompletionStatus { get; set; }
    
    /// <summary>
    /// 标签筛选
    /// </summary>
    public List<string>? TagIds { get; set; }
    
    /// <summary>
    /// 截止时间筛选
    /// </summary>
    public DateTimeFilter? DueDateFilter { get; set; }
    
    /// <summary>
    /// 创建时间筛选
    /// </summary>
    public DateTimeFilter? CreatedAtFilter { get; set; }
    
    /// <summary>
    /// 关联应用筛选
    /// </summary>
    public List<string>? AppNames { get; set; }
    
    /// <summary>
    /// 是否使用正则表达式
    /// </summary>
    public bool UseRegex { get; set; }
    
    /// <summary>
    /// 检查筛选条件是否为空
    /// </summary>
    public bool IsEmpty()
    {
        return string.IsNullOrWhiteSpace(SearchText) &&
               (Priorities == null || Priorities.Count == 0) &&
               CompletionStatus == null &&
               (TagIds == null || TagIds.Count == 0) &&
               DueDateFilter == null &&
               CreatedAtFilter == null &&
               (AppNames == null || AppNames.Count == 0);
    }
}
