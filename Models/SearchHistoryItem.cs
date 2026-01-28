namespace SceneTodo.Models;

/// <summary>
/// 搜索历史记录项
/// </summary>
public class SearchHistoryItem
{
    /// <summary>
    /// 搜索关键词
    /// </summary>
    public string Query { get; set; } = string.Empty;

    /// <summary>
    /// 首次搜索时间
    /// </summary>
    public DateTime SearchedAt { get; set; }

    /// <summary>
    /// 搜索次数
    /// </summary>
    public int SearchCount { get; set; }

    /// <summary>
    /// 最后搜索时间
    /// </summary>
    public DateTime LastSearchedAt { get; set; }
}
