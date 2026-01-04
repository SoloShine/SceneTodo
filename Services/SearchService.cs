using System.Diagnostics;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SceneTodo.Models;
using SceneTodo.Services.Database;

namespace SceneTodo.Services;

/// <summary>
/// 搜索服务
/// </summary>
public class SearchService
{
    private readonly TodoDbContext _dbContext;

    public SearchService(TodoDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// 执行搜索和筛选
    /// </summary>
    public async Task<SearchResult> SearchAsync(SearchFilter filter)
    {
        var stopwatch = Stopwatch.StartNew();
        var query = _dbContext.TodoItems.AsQueryable();

        // 应用搜索条件
        if (!string.IsNullOrWhiteSpace(filter.SearchText))
        {
            var searchLower = filter.SearchText.ToLower();
            query = query.Where(t =>
                t.Content.ToLower().Contains(searchLower) ||
                (t.Description != null && t.Description.ToLower().Contains(searchLower))
            );
        }

        // 应用优先级筛选
        if (filter.Priorities != null && filter.Priorities.Count > 0)
        {
            query = query.Where(t => filter.Priorities.Contains(t.Priority));
        }

        // 应用完成状态筛选
        if (filter.CompletionStatus != null)
        {
            switch (filter.CompletionStatus)
            {
                case CompletionStatus.Completed:
                    query = query.Where(t => t.IsCompleted);
                    break;
                case CompletionStatus.Incomplete:
                    query = query.Where(t => !t.IsCompleted);
                    break;
            }
        }

        // 应用标签筛选 (需要在内存中执行，因为 TagsJson 是 JSON 字段)
        List<TodoItem> items;
        
        if (filter.TagIds != null && filter.TagIds.Count > 0)
        {
            // 先获取所有数据
            var allItems = await query.ToListAsync();
            
            // 在内存中筛选标签
            items = allItems.Where(t =>
            {
                try
                {
                    var tagIds = JsonSerializer.Deserialize<List<string>>(t.TagsJson ?? "[]");
                    return tagIds != null && tagIds.Any(id => filter.TagIds.Contains(id));
                }
                catch
                {
                    return false;
                }
            }).ToList();
        }
        else
        {
            // 应用截止时间筛选
            if (filter.DueDateFilter != null)
            {
                query = ApplyDateTimeFilter(query, filter.DueDateFilter, isCreatedAt: false);
            }

            // 应用创建时间筛选
            if (filter.CreatedAtFilter != null)
            {
                query = ApplyDateTimeFilter(query, filter.CreatedAtFilter, isCreatedAt: true);
            }

            // 应用关联应用筛选 (需要在内存中执行)
            if (filter.AppNames != null && filter.AppNames.Count > 0)
            {
                var allItems = await query.ToListAsync();
                items = allItems.Where(t =>
                {
                    try
                    {
                        var actions = JsonSerializer.Deserialize<List<LinkedAction>>(t.LinkedActionsJson ?? "[]");
                        return actions != null && actions.Any(a => filter.AppNames.Contains(a.ActionTarget));
                    }
                    catch
                    {
                        return false;
                    }
                }).ToList();
            }
            else
            {
                // 执行查询
                items = await query
                    .OrderByDescending(t => t.GreadtedAt)
                    .ToListAsync();
            }
        }

        // 转换为 TodoItemModel
        var models = items.Select(t => new TodoItemModel(t)).ToList();

        stopwatch.Stop();

        return new SearchResult
        {
            Items = models,
            TotalCount = models.Count,
            ElapsedMilliseconds = stopwatch.ElapsedMilliseconds
        };
    }

    /// <summary>
    /// 应用日期时间筛选
    /// </summary>
    private IQueryable<TodoItem> ApplyDateTimeFilter(
        IQueryable<TodoItem> query,
        DateTimeFilter filter,
        bool isCreatedAt)
    {
        var now = DateTime.Now;

        switch (filter.Type)
        {
            case DateTimeFilterType.Today:
                var today = now.Date;
                var tomorrow = today.AddDays(1);
                if (isCreatedAt)
                {
                    query = query.Where(t => t.GreadtedAt >= today && t.GreadtedAt < tomorrow);
                }
                else
                {
                    query = query.Where(t => t.DueDate.HasValue && t.DueDate.Value >= today && t.DueDate.Value < tomorrow);
                }
                break;

            case DateTimeFilterType.ThisWeek:
                var startOfWeek = now.Date.AddDays(-(int)now.DayOfWeek);
                var endOfWeek = startOfWeek.AddDays(7);
                if (isCreatedAt)
                {
                    query = query.Where(t => t.GreadtedAt >= startOfWeek && t.GreadtedAt < endOfWeek);
                }
                else
                {
                    query = query.Where(t => t.DueDate.HasValue && t.DueDate.Value >= startOfWeek && t.DueDate.Value < endOfWeek);
                }
                break;

            case DateTimeFilterType.ThisMonth:
                var startOfMonth = new DateTime(now.Year, now.Month, 1);
                var endOfMonth = startOfMonth.AddMonths(1);
                if (isCreatedAt)
                {
                    query = query.Where(t => t.GreadtedAt >= startOfMonth && t.GreadtedAt < endOfMonth);
                }
                else
                {
                    query = query.Where(t => t.DueDate.HasValue && t.DueDate.Value >= startOfMonth && t.DueDate.Value < endOfMonth);
                }
                break;

            case DateTimeFilterType.Overdue:
                if (!isCreatedAt)
                {
                    query = query.Where(t => t.DueDate.HasValue && t.DueDate.Value < now && !t.IsCompleted);
                }
                break;

            case DateTimeFilterType.Custom:
                if (filter.StartDate.HasValue)
                {
                    if (isCreatedAt)
                    {
                        query = query.Where(t => t.GreadtedAt >= filter.StartDate.Value);
                    }
                    else
                    {
                        query = query.Where(t => t.DueDate.HasValue && t.DueDate.Value >= filter.StartDate.Value);
                    }
                }
                if (filter.EndDate.HasValue)
                {
                    if (isCreatedAt)
                    {
                        query = query.Where(t => t.GreadtedAt <= filter.EndDate.Value);
                    }
                    else
                    {
                        query = query.Where(t => t.DueDate.HasValue && t.DueDate.Value <= filter.EndDate.Value);
                    }
                }
                break;
        }

        return query;
    }
}
