using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using SceneTodo.Models;
using MessageBox = HandyControl.Controls.MessageBox;

namespace SceneTodo.Views
{
    public partial class TagManagementWindow : HandyControl.Controls.Window
    {
        public ObservableCollection<Tag> Tags { get; set; }

        public TagManagementWindow()
        {
            InitializeComponent();
            Tags = new ObservableCollection<Tag>();
            TagsDataGrid.ItemsSource = Tags;
            LoadTags();
        }

        /// <summary>
        /// 加载所有标签
        /// </summary>
        private async void LoadTags()
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

                // 更新行号
                UpdateRowNumbers();
                
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
        /// 更新行号
        /// </summary>
        private void UpdateRowNumbers()
        {
            for (int i = 0; i < TagsDataGrid.Items.Count; i++)
            {
                var row = TagsDataGrid.ItemContainerGenerator.ContainerFromIndex(i) as System.Windows.Controls.DataGridRow;
                if (row != null)
                {
                    row.Header = (i + 1).ToString();
                }
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
                LoadTags(); // 重新加载标签列表
            }
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
                    LoadTags(); // 重新加载标签列表
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
                        LoadTags(); // 重新加载标签列表
                        
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
        /// 刷新列表
        /// </summary>
        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            LoadTags();
        }

        /// <summary>
        /// 关闭窗口
        /// </summary>
        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
