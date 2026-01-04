using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using SceneTodo.Models;
using SceneTodo.Services;
using SceneTodo.Services.Database;

namespace SceneTodo.ViewModels
{
    /// <summary>
    /// 主窗口 ViewModel - 搜索和筛选部分
    /// </summary>
    public partial class MainWindowViewModel
    {
        private SearchService? _searchService;
        private SearchHistoryManager? _searchHistoryManager;

        /// <summary>
        /// 初始化搜索服务
        /// </summary>
        private void InitializeSearchServices()
        {
            _searchService = new SearchService(App.DbContext);
            _searchHistoryManager = new SearchHistoryManager();
        }

        /// <summary>
        /// 执行搜索
        /// </summary>
        public async Task ExecuteSearchAsync()
        {
            if (_searchService == null)
            {
                InitializeSearchServices();
            }

            try
            {
                IsSearching = true;

                // 构建搜索筛选条件
                var filter = new SearchFilter
                {
                    SearchText = SearchText
                };

                // 如果有其他筛选条件，也添加进去
                if (CurrentFilter != null)
                {
                    filter.Priorities = CurrentFilter.Priorities;
                    filter.CompletionStatus = CurrentFilter.CompletionStatus;
                    filter.TagIds = CurrentFilter.TagIds;
                    filter.DueDateFilter = CurrentFilter.DueDateFilter;
                    filter.CreatedAtFilter = CurrentFilter.CreatedAtFilter;
                    filter.AppNames = CurrentFilter.AppNames;
                }

                // 执行搜索
                var result = await _searchService!.SearchAsync(filter);

                // 保存搜索历史
                if (!string.IsNullOrWhiteSpace(SearchText))
                {
                    _searchHistoryManager?.SaveSearch(SearchText);
                }

                // 更新搜索结果
                SearchResults.Clear();
                foreach (var item in result.Items)
                {
                    SearchResults.Add(item);
                }

                // 显示搜索结果统计
                if (!filter.IsEmpty())
                {
                    HandyControl.Controls.Growl.Info($"找到 {result.TotalCount} 个匹配项 (耗时 {result.ElapsedMilliseconds}ms)");
                }
            }
            catch (Exception ex)
            {
                HandyControl.Controls.Growl.Error($"搜索失败: {ex.Message}");
            }
            finally
            {
                IsSearching = false;
            }
        }

        /// <summary>
        /// 切换筛选面板显示
        /// </summary>
        private void ToggleFilterPanel()
        {
            IsFilterPanelVisible = !IsFilterPanelVisible;
        }

        /// <summary>
        /// 重置所有筛选条件
        /// </summary>
        public async Task ResetFiltersAsync()
        {
            SearchText = string.Empty;
            CurrentFilter = new SearchFilter();
            SearchResults.Clear();
            
            HandyControl.Controls.Growl.Info("已重置所有筛选条件");
            
            // 执行空搜索以显示所有项
            await ExecuteSearchAsync();
        }

        /// <summary>
        /// 清空搜索
        /// </summary>
        private void ClearSearch()
        {
            SearchText = string.Empty;
            SearchResults.Clear();
        }

        /// <summary>
        /// 应用筛选条件
        /// </summary>
        public async Task ApplyFilterAsync(SearchFilter filter)
        {
            CurrentFilter = filter;
            await ExecuteSearchAsync();
        }

        /// <summary>
        /// 快速筛选 - 今天到期
        /// </summary>
        public async Task QuickFilterTodayDueAsync()
        {
            CurrentFilter = new SearchFilter
            {
                DueDateFilter = new DateTimeFilter
                {
                    Type = DateTimeFilterType.Today
                }
            };
            await ExecuteSearchAsync();
        }

        /// <summary>
        /// 快速筛选 - 已过期
        /// </summary>
        public async Task QuickFilterOverdueAsync()
        {
            CurrentFilter = new SearchFilter
            {
                DueDateFilter = new DateTimeFilter
                {
                    Type = DateTimeFilterType.Overdue
                }
            };
            await ExecuteSearchAsync();
        }

        /// <summary>
        /// 快速筛选 - 高优先级
        /// </summary>
        public async Task QuickFilterHighPriorityAsync()
        {
            CurrentFilter = new SearchFilter
            {
                Priorities = new System.Collections.Generic.List<Priority>
                {
                    Priority.VeryHigh,
                    Priority.High
                }
            };
            await ExecuteSearchAsync();
        }

        /// <summary>
        /// 快速筛选 - 未完成
        /// </summary>
        public async Task QuickFilterIncompleteAsync()
        {
            CurrentFilter = new SearchFilter
            {
                CompletionStatus = CompletionStatus.Incomplete
            };
            await ExecuteSearchAsync();
        }

        /// <summary>
        /// 获取搜索建议
        /// </summary>
        public System.Collections.Generic.List<string> GetSearchSuggestions(string input)
        {
            if (_searchHistoryManager == null)
            {
                InitializeSearchServices();
            }
            
            return _searchHistoryManager?.GetSuggestions(input) 
                ?? new System.Collections.Generic.List<string>();
        }

        /// <summary>
        /// 清空搜索历史
        /// </summary>
        public void ClearSearchHistory()
        {
            _searchHistoryManager?.ClearHistory();
            HandyControl.Controls.Growl.Success("已清空搜索历史");
        }
    }
}
