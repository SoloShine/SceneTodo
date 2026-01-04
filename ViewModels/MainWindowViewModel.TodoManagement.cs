using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using SceneTodo.Models;
using SceneTodo.Views;
using MessageBox = HandyControl.Controls.MessageBox;

namespace SceneTodo.ViewModels
{
    /// <summary>
    /// 主窗口 ViewModel - 待办项管理
    /// 包含：待办项的增删改查等操作
    /// </summary>
    public partial class MainWindowViewModel
    {
        /// <summary>
        /// 为待办项添加子待办项
        /// </summary>
        private void AddTodoItem(object? parameter)
        {
            if (parameter == null)
            {
                var item = new TodoItemModel
                {
                    Id = Guid.NewGuid().ToString(),
                    Content = "新待办项",
                    IsCompleted = false
                };
                Model.TodoItems.Add(item);
                EditTodoItem(item);
                App.TodoItemRepository.AddAsync(item).ConfigureAwait(false);
                return;
            }

            if (parameter is TodoItemModel parentItem)
            {
                var item = new TodoItemModel
                {
                    Id = Guid.NewGuid().ToString(),
                    Content = "新子待办项",
                    IsCompleted = false,
                    ParentId = parentItem.Id,
                    IsExpanded = true,
                    Name = parentItem.Name,
                    AppPath = parentItem.AppPath,
                    IsInjected = parentItem.IsInjected,
                    TodoItemType = parentItem.TodoItemType
                };
                parentItem.IsExpanded = true;
                parentItem.SubItems?.Add(item);
                EditTodoItem(item);
                App.TodoItemRepository.AddAsync(item).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// 编辑待办事项
        /// </summary>
        private void EditTodoItem(object? parameter)
        {
            if (parameter is not TodoItemModel todo) return;
            
            var editWindow = new EditTodoItemWindow(todo);
            if (editWindow.ShowDialog() == true)
            {
                EditTodoItemModel(todo, editWindow.Todo);
            }
        }

        /// <summary>
        /// 更新待办项模型
        /// </summary>
        private void EditTodoItemModel(TodoItemModel? todo, TodoItemModel? editTodo)
        {
            if (todo == null || editTodo == null) return;

            todo.Content = editTodo.Content;
            todo.Description = editTodo.Description;
            todo.AppPath = editTodo.AppPath;
            todo.Name = editTodo.Name;
            todo.TodoItemType = editTodo.TodoItemType;
            todo.StartTime = editTodo.StartTime;
            todo.EndTime = editTodo.EndTime;
            todo.ReminderTime = editTodo.ReminderTime;
            todo.DueDate = editTodo.DueDate;
            todo.Priority = editTodo.Priority;
            todo.LinkedActionsJson = editTodo.LinkedActionsJson;
            todo.TagsJson = editTodo.TagsJson;
            todo.OverlayPosition = editTodo.OverlayPosition;
            todo.OverlayOffsetX = editTodo.OverlayOffsetX;
            todo.OverlayOffsetY = editTodo.OverlayOffsetY;
            todo.UpdatedAt = DateTime.Now;
            App.TodoItemRepository.UpdateAsync(todo).ConfigureAwait(false);
        }

        /// <summary>
        /// 删除待办项
        /// </summary>
        private void DeleteTodoItem(object? parameter)
        {
            if (parameter is not TodoItemModel item) return;

            var result = MessageBox.Show("确定要删除此待办项吗？", "确认删除", System.Windows.MessageBoxButton.YesNo);
            if (result != System.Windows.MessageBoxResult.Yes) return;

            FindAndRemoveItemById(Model.TodoItems, item.Id);
        }

        /// <summary>
        /// 递归查找并删除待办项
        /// </summary>
        private bool FindAndRemoveItemById(ObservableCollection<TodoItemModel> items, string itemId)
        {
            var directMatch = items.FirstOrDefault(t => t.Id == itemId);
            if (directMatch != null)
            {
                items.Remove(directMatch);
                App.TodoItemRepository.DeleteAsync(itemId).ConfigureAwait(false);
                _schedulerService?.UnscheduleTodoItemReminder(itemId).ConfigureAwait(false);
                return true;
            }

            foreach (var item in items.ToList())
            {
                if (item.SubItems != null && item.SubItems.Count > 0)
                {
                    if (FindAndRemoveItemById(item.SubItems, itemId))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// 切换完成状态
        /// </summary>
        private void ToggleIsCompleted(object? parameter)
        {
            if (parameter is TodoItemModel item)
            {
                App.TodoItemRepository.UpdateAsync(item).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// 执行关联操作
        /// </summary>
        private void ExecuteLinkedAction(object? parameter)
        {
            if (parameter is not LinkedAction action) return;

            try
            {
                switch (action.ActionType)
                {
                    case LinkedActionType.OpenUrl:
                        if (string.IsNullOrWhiteSpace(action.ActionTarget))
                        {
                            MessageBox.Show("URL不能为空！", "错误", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                            return;
                        }
                        Process.Start(new ProcessStartInfo(action.ActionTarget) { UseShellExecute = true });
                        break;

                    case LinkedActionType.OpenFile:
                        if (string.IsNullOrWhiteSpace(action.ActionTarget))
                        {
                            MessageBox.Show("文件路径不能为空！", "错误", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                            return;
                        }
                        if (File.Exists(action.ActionTarget))
                        {
                            Process.Start(new ProcessStartInfo(action.ActionTarget) { UseShellExecute = true });
                        }
                        else
                        {
                            MessageBox.Show($"文件不存在: {action.ActionTarget}", "错误", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                        }
                        break;

                    case LinkedActionType.LaunchApp:
                        if (string.IsNullOrWhiteSpace(action.ActionTarget))
                        {
                            MessageBox.Show("应用路径不能为空！", "错误", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                            return;
                        }
                        if (File.Exists(action.ActionTarget))
                        {
                            var startInfo = new ProcessStartInfo(action.ActionTarget) { UseShellExecute = true };
                            if (!string.IsNullOrWhiteSpace(action.Arguments))
                            {
                                startInfo.Arguments = action.Arguments;
                            }
                            Process.Start(startInfo);
                        }
                        else
                        {
                            MessageBox.Show($"应用不存在: {action.ActionTarget}", "错误", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"执行操作失败: {ex.Message}", "错误", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 重置应用配置
        /// </summary>
        private void ResetAppConfig(object? parameter)
        {
            var result = MessageBox.Show(
                "确定要重置应用吗？这将清除除开todo外的所有数据，且无法恢复。",
                "确认重置",
                System.Windows.MessageBoxButton.YesNo,
                System.Windows.MessageBoxImage.Warning);

            if (result == System.Windows.MessageBoxResult.Yes)
            {
                Cleanup();
                Model = new MainWindowModel();
                Model.SaveToFileAsync().ConfigureAwait(false);
                MessageBox.Show("应用已重置成功！", "重置完成", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
            }
        }

        /// <summary>
        /// 重置待办数据
        /// </summary>
        private void ResetTodo(object? parameter)
        {
            var result = MessageBox.Show(
                "确定要重置所有todo数据吗，清除之前请做好备份，否则无法恢复。",
                "确认重置",
                System.Windows.MessageBoxButton.YesNo,
                System.Windows.MessageBoxImage.Warning);

            if (result == System.Windows.MessageBoxResult.Yes)
            {
                Model.TodoItems = new ObservableCollection<TodoItemModel>();
                MessageBox.Show("重置成功！", "重置完成", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                App.DatabaseInitializer.ResetDatabaseAsync().ConfigureAwait(false);
                Model.TodoItems = MainWindowModel.LoadFromDatabase();
            }
        }
    }
}
