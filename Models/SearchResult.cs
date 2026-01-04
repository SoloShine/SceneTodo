namespace SceneTodo.Models;

/// <summary>
/// 搜索结果模型
/// </summary>
public class SearchResult
{
    /// <summary>
    /// 匹配的待办项列表
    /// </summary>
    public List<TodoItemModel> Items { get; set; } = new();
    
    /// <summary>
    /// 总匹配数量
    /// </summary>
    public int TotalCount { get; set; }
    
    /// <summary>
    /// 搜索耗时（毫秒）
    /// </summary>
    public long ElapsedMilliseconds { get; set; }
}
