using SceneTodo.Models;
using SceneTodo.Services.Scheduler;
using SceneTodo.Views;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace SceneTodo.ViewModels
{
    /// <summary>
    /// ������ ViewModel - ���Ĳ���
    /// ���������ԡ��ֶΡ����캯������������
    /// </summary>
    public partial class MainWindowViewModel : INotifyPropertyChanged
    {
        #region �ֶ�

        private readonly Dictionary<string, OverlayWindow> overlayWindows = [];
        private readonly TodoItemSchedulerService? _schedulerService;
        private readonly DispatcherTimer dueDateCheckTimer;
        private readonly DispatcherTimer autoInjectTimer;
        private readonly DispatcherTimer _searchDebounceTimer;
        private readonly HashSet<string> notifiedDueDateItems = new HashSet<string>();
        private MainWindowModel model;
        private object currentContent;
        private object todoListContent;
        private bool _isSearchVisible = true;

        #endregion

        #region ����

        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Model ����,���ڰ󶨵������ڵ���ͼģ�͡�
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
        /// ��ǰҳ������
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

        /// <summary>
        /// 搜索框是否可见
        /// </summary>
        public bool IsSearchVisible
        {
            get => _isSearchVisible;
            set
            {
                if (_isSearchVisible != value)
                {
                    _isSearchVisible = value;
                    OnPropertyChanged(nameof(IsSearchVisible));
                }
            }
        }

        #region ������ɸѡ

        private string _searchText = string.Empty;
        /// <summary>
        /// �����ı�
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
                    // �ӳ�����
                    _searchDebounceTimer?.Stop();
                    _searchDebounceTimer?.Start();
                }
            }
        }

        private bool _isFilterPanelVisible;
        /// <summary>
        /// ɸѡ����Ƿ�ɼ�
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
        /// ��ǰ����ɸѡ����
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
        /// �������
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
        /// �Ƿ���������
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

        // ������ɸѡ����
        public ICommand SearchCommand { get; }
        public ICommand ToggleFilterPanelCommand { get; }
        public ICommand ResetFiltersCommand { get; }
        public ICommand ClearSearchCommand { get; }

        #endregion

        #region ���캯��

        public MainWindowViewModel()
        {
            var loadedModel = MainWindowModel.LoadFromFile();
            model = loadedModel ?? new MainWindowModel();

            // ��ʼ������
            InitializeSettingsCommands();
            InitializeSettings();

            // ��ʼ�������ӳٶ�ʱ��
            _searchDebounceTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(300)
            };
            _searchDebounceTimer.Tick += (s, e) =>
            {
                _searchDebounceTimer.Stop();
                _ = ExecuteSearchAsync();
            };

            // ��ʼ������
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

            // ������ɸѡ����
            SearchCommand = new RelayCommand(async _ => await ExecuteSearchAsync());
            ToggleFilterPanelCommand = new RelayCommand(_ => ToggleFilterPanel());
            ResetFiltersCommand = new RelayCommand(async _ => await ResetFiltersAsync());
            ClearSearchCommand = new RelayCommand(_ => ClearSearch());

            // ��ʼ��ҳ������
            InitializePageContent();

            // ��ʼ���Զ�ע�붨ʱ��
            autoInjectTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            autoInjectTimer.Tick += AutoInjectOverlays;
            autoInjectTimer.Start();

            // ��ʼ����ֹʱ���鶨ʱ��
            dueDateCheckTimer = new DispatcherTimer { Interval = TimeSpan.FromHours(1) };
            dueDateCheckTimer.Tick += CheckDueDateReminders;
            dueDateCheckTimer.Start();
            CheckDueDateReminders(null, EventArgs.Empty);
        }

        #endregion

        #region ��������

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// ��ʼ�����ݣ������ݿ��ʼ����ɺ���ã�
        /// </summary>
        public void InitializeData()
        {
            Model.TodoItems = MainWindowModel.LoadFromDatabase();
        }

        /// <summary>
        /// ��ʼ��ҳ������
        /// </summary>
        private void InitializePageContent()
        {
            todoListContent = Application.LoadComponent(new Uri("/SceneTodo;component/Views/TodoListPage.xaml", UriKind.Relative));
            CurrentContent = todoListContent;
        }

        /// <summary>
        /// �������й������������Ͷ�ʱ��
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
