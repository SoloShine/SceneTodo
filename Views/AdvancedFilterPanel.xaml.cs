using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using SceneTodo.Models;
using SceneTodo.ViewModels;

namespace SceneTodo.Views
{
    /// <summary>
    /// AdvancedFilterPanel.xaml 的交互逻辑
    /// </summary>
    public partial class AdvancedFilterPanel : UserControl
    {
        private SearchFilter _filter = new SearchFilter();

        public AdvancedFilterPanel()
        {
            InitializeComponent();
            Loaded += AdvancedFilterPanel_Loaded;
        }

        private async void AdvancedFilterPanel_Loaded(object sender, RoutedEventArgs e)
        {
            // 加载标签列表
            await LoadTagsAsync();
        }

        /// <summary>
        /// 加载标签列表
        /// </summary>
        private async System.Threading.Tasks.Task LoadTagsAsync()
        {
            try
            {
                var tags = await App.TagRepository!.GetAllAsync();
                TagsListBox.ItemsSource = tags.OrderByDescending(t => t.UsageCount);
            }
            catch (Exception ex)
            {
                HandyControl.Controls.Growl.Error($"加载标签失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 优先级复选框变化
        /// </summary>
        private void PriorityCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkBox && checkBox.Tag is string priorityStr)
            {
                if (_filter.Priorities == null)
                    _filter.Priorities = new List<Priority>();

                var priority = Enum.Parse<Priority>(priorityStr);

                if (checkBox.IsChecked == true)
                {
                    if (!_filter.Priorities.Contains(priority))
                        _filter.Priorities.Add(priority);
                }
                else
                {
                    _filter.Priorities.Remove(priority);
                }
            }
        }

        /// <summary>
        /// 完成状态下拉框变化
        /// </summary>
        private void CompletionStatusComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CompletionStatusComboBox.SelectedItem is ComboBoxItem item && item.Tag is string statusStr)
            {
                _filter.CompletionStatus = Enum.Parse<CompletionStatus>(statusStr);
            }
        }

        /// <summary>
        /// 标签列表框选择变化
        /// </summary>
        private void TagsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_filter.TagIds == null)
                _filter.TagIds = new List<string>();

            _filter.TagIds.Clear();

            foreach (Tag tag in TagsListBox.SelectedItems)
            {
                _filter.TagIds.Add(tag.Id);
            }
        }

        /// <summary>
        /// 截止时间筛选下拉框变化
        /// </summary>
        private void DueDateFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DueDateFilterComboBox.SelectedItem is ComboBoxItem item && item.Tag is string typeStr)
            {
                var filterType = Enum.Parse<DateTimeFilterType>(typeStr);

                if (_filter.DueDateFilter == null)
                    _filter.DueDateFilter = new DateTimeFilter();

                _filter.DueDateFilter.Type = filterType;
                if (CustomDateRangePanel == null) return;
                // 显示/隐藏自定义日期范围
                CustomDateRangePanel.Visibility = filterType == DateTimeFilterType.Custom
                    ? Visibility.Visible
                    : Visibility.Collapsed;
            }
        }

        /// <summary>
        /// 创建时间筛选下拉框变化
        /// </summary>
        private void CreatedAtFilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CreatedAtFilterComboBox.SelectedItem is ComboBoxItem item && item.Tag is string typeStr)
            {
                var filterType = Enum.Parse<DateTimeFilterType>(typeStr);

                if (_filter.CreatedAtFilter == null)
                    _filter.CreatedAtFilter = new DateTimeFilter();

                _filter.CreatedAtFilter.Type = filterType;
            }
        }

        /// <summary>
        /// Apply filter button click
        /// </summary>
        private async void ApplyFilter_Click(object sender, RoutedEventArgs e)
        {
            // Update custom date range if Custom is selected
            if (_filter.DueDateFilter != null && _filter.DueDateFilter.Type == DateTimeFilterType.Custom)
            {
                _filter.DueDateFilter.StartDate = StartDatePicker.SelectedDate;
                _filter.DueDateFilter.EndDate = EndDatePicker.SelectedDate;
            }

            if (DataContext is MainWindowViewModel vm)
            {
                await vm.ApplyFilterAsync(_filter);
            }
        }

        /// <summary>
        /// Reset filter button click
        /// </summary>
        private async void ResetFilter_Click(object sender, RoutedEventArgs e)
        {
            // Reset filter conditions
            _filter = new SearchFilter();

            // Reset UI controls
            ResetUI();

            // Execute reset
            if (DataContext is MainWindowViewModel vm)
            {
                await vm.ResetFiltersAsync();
            }
        }

        /// <summary>
        /// Reset UI controls
        /// </summary>
        private void ResetUI()
        {
            // Reset priority checkboxes
            foreach (var child in ((WrapPanel)((Expander)((StackPanel)Content).Children[1]).Content).Children)
            {
                if (child is CheckBox checkBox)
                    checkBox.IsChecked = false;
            }

            // Reset completion status
            CompletionStatusComboBox.SelectedIndex = 0;

            // Reset tag selection
            TagsListBox.SelectedItems.Clear();

            // Reset due date
            DueDateFilterComboBox.SelectedIndex = 0;
            CustomDateRangePanel.Visibility = Visibility.Collapsed;
            StartDatePicker.SelectedDate = null;
            EndDatePicker.SelectedDate = null;

            // Reset created date
            CreatedAtFilterComboBox.SelectedIndex = 0;
        }

        /// <summary>
        /// Quick filter - Due today
        /// </summary>
        private async void QuickFilterTodayDue_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainWindowViewModel vm)
            {
                await vm.QuickFilterTodayDueAsync();
            }
        }

        /// <summary>
        /// Quick filter - Overdue
        /// </summary>
        private async void QuickFilterOverdue_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainWindowViewModel vm)
            {
                await vm.QuickFilterOverdueAsync();
            }
        }

        /// <summary>
        /// Quick filter - High priority
        /// </summary>
        private async void QuickFilterHighPriority_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainWindowViewModel vm)
            {
                await vm.QuickFilterHighPriorityAsync();
            }
        }

        /// <summary>
        /// Quick filter - Incomplete
        /// </summary>
        private async void QuickFilterIncomplete_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainWindowViewModel vm)
            {
                await vm.QuickFilterIncompleteAsync();
            }
        }
    }
}
