using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using SceneTodo.Models;
using SceneTodo.Services.Scheduler;
using SceneTodo.Views;
using MessageBox = HandyControl.Controls.MessageBox;

namespace SceneTodo.ViewModels
{
    /// <summary>
    /// 主窗口 ViewModel - 核心部分
    /// 包含：属性、字段、构造函数、基础方法
    /// </summary>
    public partial class MainWindowViewModel : INotifyPropertyChanged
    {
        #region 字段

        private readonly Dictionary<string, OverlayWindow> overlayWindows = [];
        private readonly TodoItemSchedulerService? _schedulerService;
        private readonly DispatcherTimer dueDateCheckTimer;
        private readonly DispatcherTimer autoInjectTimer;
        private readonly DispatcherTimer _searchDebounceTimer;
        private readonly HashSet<string> notifiedDueDateItems = new HashSet<string>();
        private MainWindowModel model;
        private object currentContent;
        private object todoListContent;

        #endregion

        #region 属性

        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Model 属性,用于绑定到主窗口的视图模型。
        /// </summary>
        public MainWindowModel Model
        {
            get => model;
            set
            {
                model = value;
                OnPropertyChanged(nameof(Model));
            }
        }

        /// <summary>
        /// 当前页面内容
        /// </summary>
        public object CurrentContent
        {
            get => currentContent;
            set
            {
                currentContent = value;
                OnPropertyChanged(nameof(CurrentContent));
            }
        }

        #region 搜索和筛选

        private string _searchText = string.Empty;
        /// <summary>
        /// 搜索文本
        /// </summary>
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (_searchText != value)
                {
                    _searchText = value;
                    OnPropertyChanged(nameof(SearchText));
                    // 延迟搜索
                    _searchDebounceTimer?.Stop();
                    _searchDebounceTimer?.Start();
                }
            }
        }

        private bool _isFilterPanelVisible;
        /// <summary>
        /// 筛选面板是否可见
        /// </summary>
        public bool IsFilterPanelVisible
        {
            get => _isFilterPanelVisible;
            set
            {
                if (_isFilterPanelVisible != value)
                {
                    _isFilterPanelVisible = value;
                    OnPropertyChanged(nameof(IsFilterPanelVisible));
                }
            }
        }

        private SearchFilter _currentFilter = new SearchFilter();
        /// <summary>
        /// 当前搜索筛选条件
        /// </summary>
        public SearchFilter CurrentFilter
        {
            get => _currentFilter;
            set
            {
                if (_currentFilter != value)
                {
                    _currentFilter = value;
                    OnPropertyChanged(nameof(CurrentFilter));
                }
            }
        }

        private ObservableCollection<TodoItemModel> _searchResults = new ObservableCollection<TodoItemModel>();
        /// <summary>
        /// 搜索结果
        /// </summary>
        public ObservableCollection<TodoItemModel> SearchResults
        {
            get => _searchResults;
            set
            {
                if (_searchResults != value)
                {
                    _searchResults = value;
                    OnPropertyChanged(nameof(SearchResults));
                }
            }
        }

        private bool _isSearching;
        /// <summary>
        /// 是否正在搜索
        /// </summary>
        public bool IsSearching
        {
            get => _isSearching;
            set
            {
                if (_isSearching != value)
                {
                    _isSearching = value;
                    OnPropertyChanged(nameof(IsSearching));
                }
            }
        }

        #endregion

        #endregion

        #region Commands

        public ICommand ForceLaunchCommand { get; }
        public ICommand DeleteTodoItemCommand { get; }
        public ICommand AddTodoItemCommand { get; }
        public ICommand ToggleIsInjectedCommand { get; }
        public ICommand ResetAppConfigCommand { get; }
        public ICommand ResetTodoCommand { get; }
        public ICommand EditTodoItemCommand { get; }
        public ICommand ToggleIsCompletedCommand { get; }
        public ICommand ThemeSettingsCommand { get; }
        public ICommand AboutCommand { get; }
        public ICommand ExecuteLinkedActionCommand { get; }
        public ICommand ShowHistoryCommand { get; }
        public ICommand ShowHistoryPageCommand { get; }
        public ICommand ShowTodoListPageCommand { get; }
        public ICommand ShowCalendarViewCommand { get; }
        public ICommand ShowScheduledTasksCommand { get; }
        public ICommand BackupManagementCommand { get; }
        
        // 搜索和筛选命令
        public ICommand SearchCommand { get; }
        public ICommand ToggleFilterPanelCommand { get; }
        public ICommand ResetFiltersCommand { get; }
        public ICommand ClearSearchCommand { get; }

        #endregion

        #region 构造函数

        public MainWindowViewModel()
        {
            var loadedModel = MainWindowModel.LoadFromFile();
            model = loadedModel ?? new MainWindowModel();

            // 初始化设置
            InitializeSettingsCommands();
            InitializeSettings();

            // 初始化搜索延迟定时器
            _searchDebounceTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(300)
            };
            _searchDebounceTimer.Tick += (s, e) =>
            {
                _searchDebounceTimer.Stop();
                _ = ExecuteSearchAsync();
            };

            // 初始化命令
            ForceLaunchCommand = new RelayCommand(ForceLaunch);
            DeleteTodoItemCommand = new RelayCommand(DeleteTodoItem);
            AddTodoItemCommand = new RelayCommand(AddTodoItem);
            ToggleIsInjectedCommand = new RelayCommand(ToggleIsInjected);
            ResetAppConfigCommand = new RelayCommand(ResetAppConfig);
            ResetTodoCommand = new RelayCommand(ResetTodo);
            EditTodoItemCommand = new RelayCommand(EditTodoItem);
            ToggleIsCompletedCommand = new RelayCommand(ToggleIsCompleted);
            ThemeSettingsCommand = new RelayCommand(ThemeSettings);
            AboutCommand = new RelayCommand(About);
            ExecuteLinkedActionCommand = new RelayCommand(ExecuteLinkedAction);
            ShowHistoryCommand = new RelayCommand(ShowHistory);
            ShowHistoryPageCommand = new RelayCommand(ShowHistoryPage);
            ShowTodoListPageCommand = new RelayCommand(ShowTodoListPage);
            ShowCalendarViewCommand = new RelayCommand(ShowCalendarView);
            ShowScheduledTasksCommand = new RelayCommand(ShowScheduledTasks);
            BackupManagementCommand = new RelayCommand(OpenBackupManagement);
            
            // 搜索和筛选命令
            SearchCommand = new RelayCommand(async _ => await ExecuteSearchAsync());
            ToggleFilterPanelCommand = new RelayCommand(_ => ToggleFilterPanel());
            ResetFiltersCommand = new RelayCommand(async _ => await ResetFiltersAsync());
            ClearSearchCommand = new RelayCommand(_ => ClearSearch());

            // 初始化页面内容
            InitializePageContent();

            // 初始化自动注入定时器
            autoInjectTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            autoInjectTimer.Tick += AutoInjectOverlays;
            autoInjectTimer.Start();

            // 初始化截止时间检查定时器
            dueDateCheckTimer = new DispatcherTimer { Interval = TimeSpan.FromHours(1) };
            dueDateCheckTimer.Tick += CheckDueDateReminders;
            dueDateCheckTimer.Start();
            CheckDueDateReminders(null, EventArgs.Empty);
        }

        #endregion

        #region 基础方法

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// 初始化数据（在数据库初始化完成后调用）
        /// </summary>
        public void InitializeData()
        {
            Model.TodoItems = MainWindowModel.LoadFromDatabase();
        }

        /// <summary>
        /// 初始化页面内容
        /// </summary>
        private void InitializePageContent()
        {
            todoListContent = Application.LoadComponent(new Uri("/SceneTodo;component/Views/TodoListPage.xaml", UriKind.Relative));
            CurrentContent = todoListContent;
        }

        /// <summary>
        /// 清理所有关联的悬浮窗和定时器
        /// </summary>
        public void Cleanup()
        {
            foreach (var window in overlayWindows.Values)
            {
                window.Close();
            }
            Model.SaveToFileAsync().ConfigureAwait(false);
            overlayWindows.Clear();
            autoInjectTimer.Stop();
            dueDateCheckTimer?.Stop();
            _schedulerService?.ShutdownAsync().ConfigureAwait(false);
        }

        #endregion
    }
}
