using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using SceneTodo.Models;

namespace SceneTodo.ViewModels
{
    /// <summary>
    /// 日历视图 ViewModel
    /// </summary>
    public class CalendarViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private DateTime currentMonth;
        private DateTime selectedDate;
        private bool isPopupOpen;

        /// <summary>
        /// 日历天数集合
        /// </summary>
        public ObservableCollection<CalendarDay> CalendarDays { get; set; }

        /// <summary>
        /// 选中日期的待办项（保持层级结构）
        /// </summary>
        public ObservableCollection<TodoItemModel> SelectedDateTodos { get; set; }

        /// <summary>
        /// Popup 是否打开
        /// </summary>
        public bool IsPopupOpen
        {
            get => isPopupOpen;
            set
            {
                isPopupOpen = value;
                OnPropertyChanged(nameof(IsPopupOpen));
            }
        }

        /// <summary>
        /// 星期标题
        /// </summary>
        public List<string> WeekDays { get; } = new List<string> 
        { 
            "Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat" 
        };

        /// <summary>
        /// 当前月份文本
        /// </summary>
        public string CurrentMonthText => currentMonth.ToString("yyyy MMMM");

        /// <summary>
        /// 选中日期文本
        /// </summary>
        public string SelectedDateText => selectedDate.ToString("yyyy-MM-dd dddd");

        /// <summary>
        /// 上一月命令
        /// </summary>
        public ICommand PreviousMonthCommand { get; }

        /// <summary>
        /// 下一月命令
        /// </summary>
        public ICommand NextMonthCommand { get; }

        /// <summary>
        /// 选择日期命令
        /// </summary>
        public ICommand SelectDateCommand { get; }

        /// <summary>
        /// 关闭 Popup 命令
        /// </summary>
        public ICommand ClosePopupCommand { get; }

        public CalendarViewModel()
        {
            currentMonth = DateTime.Now;
            selectedDate = DateTime.Today;

            CalendarDays = new ObservableCollection<CalendarDay>();
            SelectedDateTodos = new ObservableCollection<TodoItemModel>();

            PreviousMonthCommand = new RelayCommand(_ => ChangeMonth(-1));
            NextMonthCommand = new RelayCommand(_ => ChangeMonth(1));
            SelectDateCommand = new RelayCommand(SelectDate);
            ClosePopupCommand = new RelayCommand(_ => IsPopupOpen = false);

            LoadCalendar();
        }

        /// <summary>
        /// 加载日历
        /// </summary>
        private void LoadCalendar()
        {
            CalendarDays.Clear();

            // 获取当月第一天
            var firstDay = new DateTime(currentMonth.Year, currentMonth.Month, 1);
            // 获取日历起始日期（从周日开始）
            var startDay = firstDay.AddDays(-(int)firstDay.DayOfWeek);

            // 生成6周 x 7天 = 42天
            for (int i = 0; i < 42; i++)
            {
                var date = startDay.AddDays(i);
                var todoCount = GetTodoCountForDate(date);

                CalendarDays.Add(new CalendarDay
                {
                    Date = date,
                    Day = date.Day,
                    IsCurrentMonth = date.Month == currentMonth.Month,
                    IsToday = date.Date == DateTime.Today,
                    TodoCount = todoCount
                });
            }
        }

        /// <summary>
        /// 获取指定日期的待办项数量（递归统计）
        /// </summary>
        private int GetTodoCountForDate(DateTime date)
        {
            var allTodos = App.MainViewModel?.Model?.TodoItems;
            if (allTodos == null) return 0;

            return CountTodosRecursive(allTodos, date);
        }

        /// <summary>
        /// 递归统计待办项数量
        /// </summary>
        private int CountTodosRecursive(ObservableCollection<TodoItemModel> todos, DateTime date)
        {
            int count = 0;
            foreach (var todo in todos)
            {
                if (IsTodoOnDate(todo, date))
                {
                    count++;
                }
                if (todo.SubItems != null && todo.SubItems.Count > 0)
                {
                    count += CountTodosRecursive(todo.SubItems, date);
                }
            }
            return count;
        }

        /// <summary>
        /// 获取指定日期的根级待办项（保持层级结构）
        /// </summary>
        private List<TodoItemModel> GetRootTodosForDate(DateTime date)
        {
            var allTodos = App.MainViewModel?.Model?.TodoItems;
            if (allTodos == null) return new List<TodoItemModel>();

            return allTodos.Where(t => HasTodoOnDate(t, date)).ToList();
        }

        /// <summary>
        /// 判断待办项或其子项是否在指定日期
        /// </summary>
        private bool HasTodoOnDate(TodoItemModel todo, DateTime date)
        {
            // 检查当前项
            if (IsTodoOnDate(todo, date))
                return true;

            // 递归检查子项
            if (todo.SubItems != null && todo.SubItems.Count > 0)
            {
                foreach (var subItem in todo.SubItems)
                {
                    if (HasTodoOnDate(subItem, date))
                        return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 判断待办项是否在指定日期
        /// </summary>
        private bool IsTodoOnDate(TodoItemModel todo, DateTime date)
        {
            // 检查各个时间字段
            if (todo.DueDate?.Date == date.Date) return true;
            if (todo.StartTime?.Date == date.Date) return true;
            if (todo.EndTime?.Date == date.Date) return true;
            if (todo.ReminderTime?.Date == date.Date) return true;
            if (todo.GreadtedAt?.Date == date.Date) return true;

            return false;
        }

        /// <summary>
        /// 切换月份
        /// </summary>
        private void ChangeMonth(int offset)
        {
            currentMonth = currentMonth.AddMonths(offset);
            LoadCalendar();
            OnPropertyChanged(nameof(CurrentMonthText));
        }

        /// <summary>
        /// 选择日期
        /// </summary>
        private void SelectDate(object? parameter)
        {
            DateTime date;
            
            if (parameter is DateTime dateTime)
            {
                date = dateTime;
            }
            else if (parameter is CalendarDay calendarDay)
            {
                date = calendarDay.Date;
            }
            else
            {
                return;
            }

            selectedDate = date;
            LoadSelectedDateTodos();
            OnPropertyChanged(nameof(SelectedDateText));
            
            // 如果有待办项，打开 Popup
            if (SelectedDateTodos.Count > 0)
            {
                IsPopupOpen = true;
            }
        }

        /// <summary>
        /// 加载选中日期的待办项（保持层级结构）
        /// </summary>
        private void LoadSelectedDateTodos()
        {
            SelectedDateTodos.Clear();
            var todos = GetRootTodosForDate(selectedDate);
            foreach (var todo in todos)
            {
                SelectedDateTodos.Add(todo);
            }
        }
    }
}
