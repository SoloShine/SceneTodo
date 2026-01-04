using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using SceneTodo.Models;

namespace SceneTodo.ViewModels
{
    /// <summary>
    /// 主窗口 ViewModel - 标签筛选
    /// 包含：按标签筛选待办项功能
    /// </summary>
    public partial class MainWindowViewModel
    {
        private ObservableCollection<TodoItemModel>? allTodoItems;
        private Tag? currentFilterTag;

        /// <summary>
        /// 按标签筛选待办项
        /// </summary>
        public void FilterByTag(Tag? tag)
        {
            if (allTodoItems == null)
            {
                allTodoItems = new ObservableCollection<TodoItemModel>(Model.TodoItems);
            }

            if (currentFilterTag == tag)
            {
                ClearTagFilter();
                return;
            }

            currentFilterTag = tag;

            if (tag == null)
            {
                Model.TodoItems = allTodoItems;
                return;
            }

            var filteredItems = new ObservableCollection<TodoItemModel>();
            FilterByTagRecursive(allTodoItems, tag.Id, filteredItems);
            Model.TodoItems = filteredItems;
        }

        /// <summary>
        /// 递归筛选包含指定标签的待办项
        /// </summary>
        private void FilterByTagRecursive(ObservableCollection<TodoItemModel> items, string tagId, ObservableCollection<TodoItemModel> result)
        {
            foreach (var item in items)
            {
                var itemTagIds = System.Text.Json.JsonSerializer.Deserialize<List<string>>(item.TagsJson) ?? new List<string>();

                if (itemTagIds.Contains(tagId))
                {
                    var itemCopy = new TodoItemModel(item);

                    if (item.SubItems != null && item.SubItems.Count > 0)
                    {
                        FilterByTagRecursive(item.SubItems, tagId, itemCopy.SubItems);
                    }

                    result.Add(itemCopy);
                }
                else if (item.SubItems != null && item.SubItems.Count > 0)
                {
                    var tempSubItems = new ObservableCollection<TodoItemModel>();
                    FilterByTagRecursive(item.SubItems, tagId, tempSubItems);

                    if (tempSubItems.Count > 0)
                    {
                        var itemCopy = new TodoItemModel(item);
                        itemCopy.SubItems = tempSubItems;
                        result.Add(itemCopy);
                    }
                }
            }
        }

        /// <summary>
        /// 清除标签筛选
        /// </summary>
        public void ClearTagFilter()
        {
            if (allTodoItems != null)
            {
                Model.TodoItems = allTodoItems;
                allTodoItems = null;
            }
            currentFilterTag = null;
        }
    }
}
