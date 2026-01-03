using System.Windows;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using System.Linq;
using SceneTodo.Models;
using SceneTodo.ViewModels;
using MessageBox = HandyControl.Controls.MessageBox;

namespace SceneTodo.Views
{
    public partial class EditTodoItemWindow : HandyControl.Controls.Window
    {
        public TodoItemModel Todo { get; private set; }
        private ObservableCollection<LinkedAction> linkedActions;
        private ObservableCollection<Tag> allTags;
        private List<string> selectedTagIds;

        public EditTodoItemWindow(TodoItemModel todo)
        {
            InitializeComponent();
            
            // 检查是否为子级待办项
            bool isChildItem = !string.IsNullOrEmpty(todo.ParentId);
            
            Todo = new TodoItemModel
            {
                Id = todo.Id,
                Content = todo.Content,
                Description = todo.Description,
                IsCompleted = todo.IsCompleted,
                ParentId = todo.ParentId,
                IsExpanded = todo.IsExpanded,
                SubItems = todo.SubItems,
                TodoItemType = todo.TodoItemType,
                AppPath = todo.AppPath,
                Name = todo.Name,
                IsInjected = todo.IsInjected,
                StartTime = todo.StartTime,
                ReminderTime = todo.ReminderTime,
                EndTime = todo.EndTime,
                DueDate = todo.DueDate,
                Priority = todo.Priority,
                LinkedActionsJson = todo.LinkedActionsJson,
                TagsJson = todo.TagsJson,
                OverlayPosition = todo.OverlayPosition,
                OverlayOffsetX = todo.OverlayOffsetX,
                OverlayOffsetY = todo.OverlayOffsetY,
            };

            // 初始化关联操作列表
            linkedActions = new ObservableCollection<LinkedAction>(Todo.LinkedActions);
            LinkedActionsListBox.ItemsSource = linkedActions;

            // 初始化标签列表
            InitializeTags();

            // 初始化控件值
            ContentTextBox.Text = Todo.Content;
            DescriptionTextBox.Text = Todo.Description;
            AppPathTextBox.Text = todo.AppPath;
            AppNameTextBox.Text = todo.Name;
            AppTypeComboBox.SelectedItem = todo.TodoItemType;
            StartTimePicker.SelectedDateTime = todo.StartTime;
            EndTimePicker.SelectedDateTime = todo.EndTime;
            ReminderTimePicker.SelectedDateTime = todo.ReminderTime;
            DueDatePicker.SelectedDateTime = todo.DueDate;

            // 初始化优先级
            SetPriorityComboBox(Todo.Priority);

            // 初始化遮盖层位置
            SetOverlayPositionComboBox(Todo.OverlayPosition);
            OverlayOffsetXNumericUpDown.Value = Todo.OverlayOffsetX;
            OverlayOffsetYNumericUpDown.Value = Todo.OverlayOffsetY;

            // 根据待办类型显示/隐藏遮盖层位置控件
            UpdateOverlayPositionVisibility(Todo.TodoItemType);

            // 初始化枚举ComboBox
            var converter = new Converters.EnumToComboBoxConverter();
            AppTypeComboBox.ItemsSource = converter.Convert(typeof(TodoItemType), null, null, null) as List<KeyValuePair<Enum, string>>;

            // 设置当前选中项
            foreach (var item in AppTypeComboBox.Items)
            {
                var pair = (KeyValuePair<Enum, string>)item;
                if ((TodoItemType)pair.Key == Todo.TodoItemType)
                {
                    AppTypeComboBox.SelectedItem = item;
                    break;
                }
            }
            
            // 如果是子级待办项，禁用应用绑定相关控件
            if (isChildItem)
            {
                AppTypeComboBox.IsEnabled = false;
                AppPathTextBox.IsEnabled = false;
                AppNameTextBox.IsEnabled = false;
                SelectAppButton.IsEnabled = false;
                OverlayPositionComboBox.IsEnabled = false;
                OverlayOffsetXNumericUpDown.IsEnabled = false;
                OverlayOffsetYNumericUpDown.IsEnabled = false;
                
                // 添加提示文本
                AppPathTextBlock.Text = "应用路径 (继承自父级):";
                
                // 显示灰色背景表示不可编辑
                AppPathTextBox.Background = System.Windows.Media.Brushes.LightGray;
                AppNameTextBox.Background = System.Windows.Media.Brushes.LightGray;
            }
            
            TypeChange();

            // 事件绑定
            SaveButton.Click += SaveButton_Click;
            CancelButton.Click += CancelButton_Click;
            AppTypeComboBox.SelectionChanged += AppTypeComboBox_SelectionChanged;
            SelectAppButton.Click += SelectAppButton_Click;
        }

        /// <summary>
        /// 初始化标签列表
        /// </summary>
        private async void InitializeTags()
        {
            try
            {
                // 加载所有标签
                var tags = await App.TagRepository.GetAllAsync();
                allTags = new ObservableCollection<Tag>(tags);
                TagsListBox.ItemsSource = allTags;

                // 解析当前待办的标签ID
                selectedTagIds = new List<string>();
                try
                {
                    selectedTagIds = System.Text.Json.JsonSerializer.Deserialize<List<string>>(Todo.TagsJson) 
                                    ?? new List<string>();
                }
                catch
                {
                    selectedTagIds = new List<string>();
                }

                // 设置已选中的标签
                foreach (var tag in allTags)
                {
                    if (selectedTagIds.Contains(tag.Id))
                    {
                        TagsListBox.SelectedItems.Add(tag);
                    }
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"加载标签失败: {ex.Message}", "错误", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 管理标签按钮点击
        /// </summary>
        private void ManageTagsButton_Click(object sender, RoutedEventArgs e)
        {
            var tagManagementWindow = new TagManagementWindow();
            tagManagementWindow.ShowDialog();
            
            // 刷新标签列表
            InitializeTags();
        }

        private void SetPriorityComboBox(Priority priority)
        {
            foreach (ComboBoxItem item in PriorityComboBox.Items)
            {
                if (item.Tag.ToString() == priority.ToString())
                {
                    PriorityComboBox.SelectedItem = item;
                    break;
                }
            }
        }

        private void SetOverlayPositionComboBox(OverlayPosition position)
        {
            foreach (ComboBoxItem item in OverlayPositionComboBox.Items)
            {
                if (item.Tag.ToString() == position.ToString())
                {
                    OverlayPositionComboBox.SelectedItem = item;
                    break;
                }
            }
        }

        private OverlayPosition GetSelectedOverlayPosition()
        {
            if (OverlayPositionComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                return selectedItem.Tag.ToString() switch
                {
                    "Bottom" => OverlayPosition.Bottom,
                    "TopLeft" => OverlayPosition.TopLeft,
                    "TopRight" => OverlayPosition.TopRight,
                    "BottomLeft" => OverlayPosition.BottomLeft,
                    "BottomRight" => OverlayPosition.BottomRight,
                    "Center" => OverlayPosition.Center,
                    _ => OverlayPosition.Bottom
                };
            }
            return OverlayPosition.Bottom;
        }

        private void UpdateOverlayPositionVisibility(TodoItemType todoItemType)
        {
            var visibility = todoItemType == TodoItemType.App ? Visibility.Visible : Visibility.Collapsed;
            OverlayPositionPanel.Visibility = visibility;
            OverlayOffsetPanel.Visibility = visibility;
        }

        private Priority GetSelectedPriority()
        {
            if (PriorityComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                return selectedItem.Tag.ToString() switch
                {
                    "VeryHigh" => Priority.VeryHigh,
                    "High" => Priority.High,
                    "Medium" => Priority.Medium,
                    "Low" => Priority.Low,
                    "VeryLow" => Priority.VeryLow,
                    _ => Priority.Medium
                };
            }
            return Priority.Medium;
        }

        private void AppTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TypeChange();
        }

        private void TypeChange()
        {
            if (AppTypeComboBox.SelectedItem is KeyValuePair<Enum, string> selectedPair)
            {
                Todo.TodoItemType = (TodoItemType)selectedPair.Key;
                // 根据类型显示或隐藏控件
                if (Todo.TodoItemType == TodoItemType.App)
                {
                    AppPathTextBox.Visibility = Visibility.Visible;
                    AppNameTextBox.Visibility = Visibility.Visible;
                    AppNameTextBlock.Visibility = Visibility.Visible;
                    AppPathTextBlock.Visibility = Visibility.Visible;
                    SelectAppButton.Visibility = Visibility.Visible;
                }
                else
                {
                    AppPathTextBox.Visibility = Visibility.Collapsed;
                    AppNameTextBox.Visibility = Visibility.Collapsed;
                    AppNameTextBlock.Visibility = Visibility.Collapsed;
                    AppPathTextBlock.Visibility = Visibility.Collapsed;
                    SelectAppButton.Visibility = Visibility.Collapsed;
                }

                // 更新遮盖层位置控件的可见性
                UpdateOverlayPositionVisibility(Todo.TodoItemType);
            }
        }

        private void SelectAppButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "是否为当前运行软件添加待办事项？选\"是\"将检测正在运行的软件，选\"否\"将打开文件选择器。",
                "选择操作",
                MessageBoxButton.YesNo);

            if (result == MessageBoxResult.Yes)
            {
                // 调用选择运行中软件的方法
                var mainViewModel = (MainWindowViewModel)Application.Current.MainWindow.DataContext;
                TodoItemModel.SelectRunningApp(Todo);
                AppPathTextBox.Text = Todo.AppPath;
                AppNameTextBox.Text = Todo.Name;
            }
            else if (result == MessageBoxResult.No)
            {
                // 调用选择文件的方法
                var mainViewModel = (MainWindowViewModel)Application.Current.MainWindow.DataContext;
                TodoItemModel.SelectApp(Todo);
                AppPathTextBox.Text = Todo.AppPath;
                AppNameTextBox.Text = Todo.Name;
            }
        }

        private void AddLinkedActionButton_Click(object sender, RoutedEventArgs e)
        {
            var editWindow = new EditLinkedActionWindow();
            if (editWindow.ShowDialog() == true)
            {
                linkedActions.Add(editWindow.Action);
            }
        }

        private void EditLinkedActionButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is LinkedAction action)
            {
                var editWindow = new EditLinkedActionWindow(action);
                if (editWindow.ShowDialog() == true)
                {
                    // 更新现有操作
                    var index = linkedActions.IndexOf(action);
                    if (index >= 0)
                    {
                        linkedActions[index] = editWindow.Action;
                        LinkedActionsListBox.Items.Refresh();
                    }
                }
            }
        }

        private void DeleteLinkedActionButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is LinkedAction action)
            {
                var result = MessageBox.Show(
                    $"确定要删除关联操作 \"{action.DisplayName}\" 吗？",
                    "确认删除",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    linkedActions.Remove(action);
                }
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // 验证数据
            if (string.IsNullOrWhiteSpace(ContentTextBox.Text))
            {
                MessageBox.Show("内容不能为空！", "验证错误", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 检查是否为子级待办项
            bool isChildItem = !string.IsNullOrEmpty(Todo.ParentId);

            // 保存数据
            Todo.Content = ContentTextBox.Text;
            Todo.Description = DescriptionTextBox.Text;
            Todo.StartTime = StartTimePicker.SelectedDateTime;
            Todo.EndTime = EndTimePicker.SelectedDateTime;
            Todo.ReminderTime = ReminderTimePicker.SelectedDateTime;
            Todo.DueDate = DueDatePicker.SelectedDateTime;
            Todo.Priority = GetSelectedPriority();
            Todo.LinkedActions = linkedActions;

            // 保存选中的标签
            var selectedTags = TagsListBox.SelectedItems.Cast<Tag>().Select(t => t.Id).ToList();
            Todo.TagsJson = System.Text.Json.JsonSerializer.Serialize(selectedTags);

            // 只有根级待办项才能修改应用绑定信息
            if (!isChildItem)
            {
                Todo.AppPath = AppPathTextBox.Text;
                Todo.Name = AppNameTextBox.Text;
                Todo.OverlayPosition = GetSelectedOverlayPosition();
                Todo.OverlayOffsetX = OverlayOffsetXNumericUpDown.Value;
                Todo.OverlayOffsetY = OverlayOffsetYNumericUpDown.Value;
            }
            // 子级待办项保持从父级继承的应用绑定信息不变

            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void ClearDueDate_Click(object sender, RoutedEventArgs e)
        {
            // 先清空选中的日期时间
            DueDatePicker.SelectedDateTime = null;
            
            // 强制刷新 DateTimePicker 的显示
            // 这是 HandyControl DateTimePicker 的一个已知问题的解决方案
            DueDatePicker.Text = string.Empty;
            
            // 同时更新 Todo 对象
            Todo.DueDate = null;
        }
    }
}
