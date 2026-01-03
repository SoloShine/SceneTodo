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
                MessageBox.Show($"Failed to load tags: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 更新总数文本
        /// </summary>
        private void UpdateTotalText()
        {
            TotalTagsText.Text = $"Total: {Tags.Count} tags";
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
                    $"Are you sure you want to delete tag '{tag.Name}'?\n\n" +
                    $"This tag is currently used by {tag.UsageCount} todo items.\n" +
                    "After deletion, the tag associations will be removed.",
                    "Confirm Delete",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        await App.TagRepository.DeleteAsync(tag.Id);
                        LoadTags();

                        MessageBox.Show("Tag deleted successfully!", "Success",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Failed to delete tag: {ex.Message}", "Error",
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
    }
}
