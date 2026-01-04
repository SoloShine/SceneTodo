using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using SceneTodo.Models;
using MessageBox = HandyControl.Controls.MessageBox;

namespace SceneTodo.Views
{
    public partial class TagsPanelControl : UserControl
    {
        public ObservableCollection<Tag> Tags { get; set; }

        // 标签筛选事件
        public event EventHandler<Tag>? TagFilterRequested;

        private Tag? currentFilterTag;

        public TagsPanelControl()
        {
            InitializeComponent();
            Tags = new ObservableCollection<Tag>();
            TagsItemsControl.ItemsSource = Tags;
            LoadTags();
        }

        /// <summary>
        /// 加载所有标签
        /// </summary>
        public async void LoadTags()
        {
            try
            {
                Tags.Clear();

                var tags = await App.TagRepository.GetAllAsync();

                // 加载使用次数
                foreach (var tag in tags)
                {
                    tag.UsageCount = await App.TagRepository.GetTagUsageCountAsync(tag.Id);
                    Tags.Add(tag);
                }

                // 按使用次数降序排序
                var sortedTags = Tags.OrderByDescending(t => t.UsageCount).ToList();
                Tags.Clear();
                foreach (var tag in sortedTags)
                {
                    Tags.Add(tag);
                }

                // 更新统计文本
                UpdateTotalText();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载标签失败: {ex.Message}", "错误",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 更新总数文本
        /// </summary>
        private void UpdateTotalText()
        {
            TotalTagsText.Text = $"总计: {Tags.Count} 个标签";
        }

        /// <summary>
        /// 显示筛选状态
        /// </summary>
        /// <param name="tag">当前筛选的标签，null表示清除筛选</param>
        public void SetFilterStatus(Tag? tag)
        {
            currentFilterTag = tag;
            
            if (tag != null)
            {
                FilterStatusBorder.Visibility = Visibility.Visible;
                FilterStatusText.Text = $"?? 筛选: {tag.Name}";
            }
            else
            {
                FilterStatusBorder.Visibility = Visibility.Collapsed;
                FilterStatusText.Text = string.Empty;
            }
        }

        /// <summary>
        /// 新建标签
        /// </summary>
        private void AddTag_Click(object sender, RoutedEventArgs e)
        {
            var editWindow = new EditTagWindow();
            if (editWindow.ShowDialog() == true)
            {
                LoadTags();
            }
        }

        /// <summary>
        /// 管理标签
        /// </summary>
        private void ManageTags_Click(object sender, RoutedEventArgs e)
        {
            var window = new TagManagementWindow();
            window.ShowDialog();
            LoadTags(); // 刷新标签列表
        }

        /// <summary>
        /// 刷新标签
        /// </summary>
        private void RefreshTags_Click(object sender, RoutedEventArgs e)
        {
            LoadTags();
        }

        /// <summary>
        /// 编辑标签
        /// </summary>
        private void EditTag_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Tag tag)
            {
                var editWindow = new EditTagWindow(tag);
                if (editWindow.ShowDialog() == true)
                {
                    LoadTags();
                }
            }
        }

        /// <summary>
        /// 删除标签
        /// </summary>
        private async void DeleteTag_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Tag tag)
            {
                var result = MessageBox.Show(
                    $"确定要删除标签 '{tag.Name}' 吗？\n\n" +
                    $"此标签当前被 {tag.UsageCount} 个待办项使用。\n" +
                    "删除后，所有标签关联将被移除。",
                    "确认删除",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        // 如果删除的是当前筛选标签，先清除筛选
                        if (currentFilterTag?.Id == tag.Id)
                        {
                            ClearFilter_Click(null, null);
                        }

                        await App.TagRepository.DeleteAsync(tag.Id);
                        LoadTags();

                        MessageBox.Show("标签删除成功！", "成功",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"删除标签失败: {ex.Message}", "错误",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        /// <summary>
        /// 按标签筛选
        /// </summary>
        private void FilterByTag_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Tag tag)
            {
                // 触发筛选事件
                TagFilterRequested?.Invoke(this, tag);
            }
        }

        /// <summary>
        /// 清除筛选
        /// </summary>
        private void ClearFilter_Click(object? sender, RoutedEventArgs? e)
        {
            // 通过主窗口的ViewModel清除筛选
            var mainWindow = Window.GetWindow(this) as MainWindow;
            if (mainWindow?.DataContext is ViewModels.MainWindowViewModel vm)
            {
                vm.ClearTagFilter();
                SetFilterStatus(null);
            }
        }
    }
}
