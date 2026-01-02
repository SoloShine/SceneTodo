using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using SceneTodo.Models;

namespace SceneTodo.ViewModels
{
    /// <summary>
    /// 历史记录窗口视图模型
    /// </summary>
    public class HistoryWindowViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private ObservableCollection<TodoItemModel> historyItems = new();
        /// <summary>
        /// 历史记录项集合
        /// </summary>
        public ObservableCollection<TodoItemModel> HistoryItems
        {
            get => historyItems;
            set
            {
                historyItems = value;
                OnPropertyChanged(nameof(HistoryItems));
            }
        }

        private DateTime? startDate;
        /// <summary>
        /// 开始日期筛选
        /// </summary>
        public DateTime? StartDate
        {
            get => startDate;
            set
            {
                startDate = value;
                OnPropertyChanged(nameof(StartDate));
            }
        }

        private DateTime? endDate;
        /// <summary>
        /// 结束日期筛选
        /// </summary>
        public DateTime? EndDate
        {
            get => endDate;
            set
            {
                endDate = value;
                OnPropertyChanged(nameof(EndDate));
            }
        }

        public ICommand FilterCommand { get; }
        public ICommand RestoreTodoCommand { get; }
        public ICommand DeleteHistoryCommand { get; }

        public HistoryWindowViewModel()
        {
            FilterCommand = new RelayCommand(async _ => await LoadHistoryAsync());
            RestoreTodoCommand = new RelayCommand(RestoreTodo);
            DeleteHistoryCommand = new RelayCommand(DeleteHistory);

            // 初始加载
            LoadHistoryAsync().ConfigureAwait(false);
        }

        /// <summary>
        /// 加载历史记录
        /// </summary>
        public async Task LoadHistoryAsync()
        {
            var allItems = await App.TodoItemRepository.GetAllAsync();
            var completed = allItems.Where(t => t.IsCompleted);

            if (StartDate.HasValue)
                completed = completed.Where(t => t.CompletedAt >= StartDate.Value);

            if (EndDate.HasValue)
                completed = completed.Where(t => t.CompletedAt <= EndDate.Value.AddDays(1).AddSeconds(-1));

            HistoryItems = new ObservableCollection<TodoItemModel>(
                completed.OrderByDescending(t => t.CompletedAt)
                         .Select(t => new TodoItemModel(t))
            );
        }

        /// <summary>
        /// 恢复待办项
        /// </summary>
        private async void RestoreTodo(object? parameter)
        {
            if (parameter is not TodoItemModel item)
            {
                System.Diagnostics.Debug.WriteLine($"RestoreTodo: 参数类型不正确 - {parameter?.GetType().Name ?? "null"}");
                return;
            }

            System.Diagnostics.Debug.WriteLine($"RestoreTodo: 恢复待办项 - ID={item.Id}, Content={item.Content}");

            // 确认操作
            var result = HandyControl.Controls.MessageBox.Show(
                $"确定要恢复待办项 \"{item.Content}\" 吗？",
                "确认恢复",
                System.Windows.MessageBoxButton.YesNo,
                System.Windows.MessageBoxImage.Question);

            if (result != System.Windows.MessageBoxResult.Yes)
                return;

            item.IsCompleted = false;
            item.CompletedAt = null;
            await App.TodoItemRepository.UpdateAsync(item);

            // 从历史记录中移除
            HistoryItems.Remove(item);

            // 重新加载主窗口的待办列表
            if (App.MainViewModel != null)
            {
                App.MainViewModel.Model.TodoItems = MainWindowModel.LoadFromDatabase();
            }

            HandyControl.Controls.MessageBox.Show(
                "待办项已恢复！",
                "成功",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Information);
        }

        /// <summary>
        /// 删除历史记录
        /// </summary>
        private async void DeleteHistory(object? parameter)
        {
            if (parameter is not TodoItemModel item) return;

            var result = HandyControl.Controls.MessageBox.Show(
                "确定要永久删除此历史记录吗？此操作无法撤销。",
                "确认删除",
                System.Windows.MessageBoxButton.YesNo,
                System.Windows.MessageBoxImage.Warning);

            if (result == System.Windows.MessageBoxResult.Yes)
            {
                await App.TodoItemRepository.DeleteAsync(item.Id);
                HistoryItems.Remove(item);
            }
        }
    }
}
