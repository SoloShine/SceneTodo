using System.Diagnostics;
using System.IO;
using System.Text.Json;
using SceneTodo.Models;

namespace SceneTodo.Services;

/// <summary>
/// 搜索历史管理器
/// </summary>
public class SearchHistoryManager
{
    private const int MaxHistoryCount = 10;
    private readonly string _historyFilePath;
    private List<SearchHistoryItem> _history;

    public SearchHistoryManager()
    {
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var appFolder = Path.Combine(appDataPath, "SceneTodo");
        Directory.CreateDirectory(appFolder);
        _historyFilePath = Path.Combine(appFolder, "SearchHistory.json");
        _history = LoadHistory();
    }

    /// <summary>
    /// 保存搜索记录
    /// </summary>
    public void SaveSearch(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return;

        var existing = _history.FirstOrDefault(h =>
            h.Query.Equals(query, StringComparison.OrdinalIgnoreCase)
        );

        if (existing != null)
        {
            existing.SearchCount++;
            existing.LastSearchedAt = DateTime.Now;
        }
        else
        {
            _history.Add(new SearchHistoryItem
            {
                Query = query,
                SearchedAt = DateTime.Now,
                SearchCount = 1,
                LastSearchedAt = DateTime.Now
            });
        }

        // 保持最多 MaxHistoryCount 条记录
        _history = _history
            .OrderByDescending(h => h.LastSearchedAt)
            .Take(MaxHistoryCount)
            .ToList();

        SaveHistory();
    }

    /// <summary>
    /// 获取搜索历史
    /// </summary>
    public List<SearchHistoryItem> GetHistory()
    {
        return _history
            .OrderByDescending(h => h.LastSearchedAt)
            .ToList();
    }

    /// <summary>
    /// 获取搜索建议
    /// </summary>
    public List<string> GetSuggestions(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return _history.OrderByDescending(h => h.LastSearchedAt).Select(h => h.Query).ToList();

        return _history
            .Where(h => h.Query.Contains(input, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(h => h.SearchCount)
            .Select(h => h.Query)
            .ToList();
    }

    /// <summary>
    /// 删除单条历史记录
    /// </summary>
    public void DeleteHistory(string query)
    {
        _history.RemoveAll(h => h.Query.Equals(query, StringComparison.OrdinalIgnoreCase));
        SaveHistory();
    }

    /// <summary>
    /// 清空所有历史记录
    /// </summary>
    public void ClearHistory()
    {
        _history.Clear();
        SaveHistory();
    }

    /// <summary>
    /// 加载历史记录
    /// </summary>
    private List<SearchHistoryItem> LoadHistory()
    {
        if (!File.Exists(_historyFilePath))
            return new List<SearchHistoryItem>();

        try
        {
            var json = File.ReadAllText(_historyFilePath);
            return JsonSerializer.Deserialize<List<SearchHistoryItem>>(json)
                ?? new List<SearchHistoryItem>();
        }
        catch
        {
            return new List<SearchHistoryItem>();
        }
    }

    /// <summary>
    /// 保存历史记录
    /// </summary>
    private void SaveHistory()
    {
        try
        {
            var json = JsonSerializer.Serialize(_history, new JsonSerializerOptions
            {
                WriteIndented = true
            });
            File.WriteAllText(_historyFilePath, json);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"保存搜索历史失败: {ex.Message}");
        }
    }
}
