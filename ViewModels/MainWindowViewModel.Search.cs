using SceneTodo.Models;
using SceneTodo.Services;
using System.Windows;
using System.Collections.ObjectModel;

namespace SceneTodo.ViewModels
{
    /// <summary>
    /// ������ ViewModel - ������ɸѡ����
    /// </summary>
    public partial class MainWindowViewModel
    {
        private SearchService? _searchService;
        private SearchHistoryManager? _searchHistoryManager;

        /// <summary>
        /// ��ʼ����������
        /// </summary>
        private void InitializeSearchServices()
        {
            _searchService = new SearchService(App.DbContext);
            _searchHistoryManager = new SearchHistoryManager();
        }

        /// <summary>
        /// ִ������
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

                // ��������ɸѡ����
                var filter = new SearchFilter
                {
                    SearchText = SearchText
                };

                // ��������ɸѡ������Ҳ���ӽ�ȥ
                if (CurrentFilter != null)
                {
                    filter.Priorities = CurrentFilter.Priorities;
                    filter.CompletionStatus = CurrentFilter.CompletionStatus;
                    filter.TagIds = CurrentFilter.TagIds;
                    filter.DueDateFilter = CurrentFilter.DueDateFilter;
                    filter.CreatedAtFilter = CurrentFilter.CreatedAtFilter;
                    filter.AppNames = CurrentFilter.AppNames;
                }

                // ִ������
                var result = await _searchService!.SearchAsync(filter);

                // ����������ʷ
                if (!string.IsNullOrWhiteSpace(SearchText))
                {
                    _searchHistoryManager?.SaveSearch(SearchText);
                }

                // 修复：将筛选结果应用到 Model.TodoItems 以便在 UI 中显示
                if (!filter.IsEmpty())
                {
                    // 获取所有待办项
                    var allItems = (await App.TodoItemRepository.GetAllAsync()).ToList();

                    // 应用筛选条件
                    var filteredItems = FilterTodos(allItems, filter);

                    // 构建树结构
                    var tree = MainWindowModel.BuildTodoItemTree(filteredItems);

                    // 更新 UI
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        Model.TodoItems = new ObservableCollection<TodoItemModel>(tree);
                    });
                }
                else
                {
                    // 清空筛选时重新加载所有数据
                    var allItems = (await App.TodoItemRepository.GetAllAsync()).ToList();
                    var tree = MainWindowModel.BuildTodoItemTree(allItems);
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        Model.TodoItems = new ObservableCollection<TodoItemModel>(tree);
                    });
                }

                // �����������
                SearchResults.Clear();
                foreach (var item in result.Items)
                {
                    SearchResults.Add(item);
                }

                // ��ʾ�������ͳ��
                if (!filter.IsEmpty())
                {
                    HandyControl.Controls.Growl.Info($"�ҵ� {result.TotalCount} ��ƥ���� (��ʱ {result.ElapsedMilliseconds}ms)");
                }
            }
            catch (Exception ex)
            {
                HandyControl.Controls.Growl.Error($"����ʧ��: {ex.Message}");
            }
            finally
            {
                IsSearching = false;
            }
        }

        /// <summary>
        /// 根据筛选条件过滤待办事项
        /// </summary>
        private List<TodoItem> FilterTodos(List<TodoItem> items, SearchFilter filter)
        {
            List<TodoItem> result = items;

            // 按优先级筛选
            if (filter.Priorities != null && filter.Priorities.Count > 0)
            {
                result = result.Where(t => filter.Priorities.Contains(t.Priority)).ToList();
            }

            // 按完成状态筛选
            if (filter.CompletionStatus != null)
            {
                switch (filter.CompletionStatus)
                {
                    case CompletionStatus.Completed:
                        result = result.Where(t => t.IsCompleted).ToList();
                        break;
                    case CompletionStatus.Incomplete:
                        result = result.Where(t => !t.IsCompleted).ToList();
                        break;
                }
            }

            // 按标签筛选
            if (filter.TagIds != null && filter.TagIds.Count > 0)
            {
                result = result.Where(t =>
                {
                    try
                    {
                        var tagIds = System.Text.Json.JsonSerializer.Deserialize<List<string>>(t.TagsJson ?? "[]");
                        return tagIds != null && tagIds.Any(id => filter.TagIds.Contains(id));
                    }
                    catch
                    {
                        return false;
                    }
                }).ToList();
            }

            // 按截止日期筛选
            if (filter.DueDateFilter != null)
            {
                result = ApplyDateTimeFilter(result, filter.DueDateFilter, isCreatedAt: false);
            }

            // 按创建日期筛选
            if (filter.CreatedAtFilter != null)
            {
                result = ApplyDateTimeFilter(result, filter.CreatedAtFilter, isCreatedAt: true);
            }

            // 按关联应用筛选
            if (filter.AppNames != null && filter.AppNames.Count > 0)
            {
                result = result.Where(t =>
                {
                    try
                    {
                        var actions = System.Text.Json.JsonSerializer.Deserialize<List<LinkedAction>>(t.LinkedActionsJson ?? "[]");
                        return actions != null && actions.Any(a => filter.AppNames.Contains(a.ActionTarget));
                    }
                    catch
                    {
                        return false;
                    }
                }).ToList();
            }

            return result.OrderByDescending(t => t.GreadtedAt).ToList();
        }

        /// <summary>
        /// 应用日期时间筛选
        /// </summary>
        private List<TodoItem> ApplyDateTimeFilter(List<TodoItem> items, DateTimeFilter filter, bool isCreatedAt)
        {
            var now = DateTime.Now;
            var todayStart = now.Date;
            var todayEnd = todayStart.AddDays(1).AddTicks(-1);
            var weekStart = todayStart.AddDays(-(int)now.DayOfWeek);
            var weekEnd = weekStart.AddDays(7).AddTicks(-1);
            var monthStart = new DateTime(now.Year, now.Month, 1);
            var monthEnd = monthStart.AddMonths(1).AddTicks(-1);

            return items.Where(t =>
            {
                DateTime? dateToCheck = isCreatedAt ? t.GreadtedAt : t.DueDate;
                if (dateToCheck == null) return false;

                return filter.Type switch
                {
                    DateTimeFilterType.Today => dateToCheck >= todayStart && dateToCheck <= todayEnd,
                    DateTimeFilterType.ThisWeek => dateToCheck >= weekStart && dateToCheck <= weekEnd,
                    DateTimeFilterType.ThisMonth => dateToCheck >= monthStart && dateToCheck <= monthEnd,
                    DateTimeFilterType.Overdue => dateToCheck < now && !t.IsCompleted,
                    DateTimeFilterType.Custom => dateToCheck >= filter.StartDate && dateToCheck <= filter.EndDate,
                    _ => true
                };
            }).ToList();
        }

        /// <summary>
        /// �л�ɸѡ�����ʾ
        /// </summary>
        private void ToggleFilterPanel()
        {
            IsFilterPanelVisible = !IsFilterPanelVisible;
        }

        /// <summary>
        /// ��������ɸѡ����
        /// </summary>
        public async Task ResetFiltersAsync()
        {
            SearchText = string.Empty;
            CurrentFilter = new SearchFilter();
            SearchResults.Clear();

            HandyControl.Controls.Growl.Info("����������ɸѡ����");

            // ִ�п���������ʾ������
            await ExecuteSearchAsync();
        }

        /// <summary>
        /// �������
        /// </summary>
        private void ClearSearch()
        {
            SearchText = string.Empty;
            SearchResults.Clear();
        }

        /// <summary>
        /// Ӧ��ɸѡ����
        /// </summary>
        public async Task ApplyFilterAsync(SearchFilter filter)
        {
            CurrentFilter = filter;
            await ExecuteSearchAsync();
        }

        /// <summary>
        /// ����ɸѡ - ���쵽��
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
        /// ����ɸѡ - �ѹ���
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
        /// ����ɸѡ - �����ȼ�
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
        /// ����ɸѡ - δ���
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
        /// ��ȡ��������
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
        /// ���������ʷ
        /// </summary>
        public void ClearSearchHistory()
        {
            _searchHistoryManager?.ClearHistory();
            HandyControl.Controls.Growl.Success("�����������ʷ");
        }
    }
}
