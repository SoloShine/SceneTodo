using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using SceneTodo.Models;

namespace SceneTodo.ViewModels
{
    /// <summary>
    /// 主窗口 ViewModel - 截止时间提醒
    /// 包含：截止时间检查和通知功能
    /// </summary>
    public partial class MainWindowViewModel
    {
        /// <summary>
        /// 检查即将到期和已过期的待办项
        /// </summary>
        private void CheckDueDateReminders(object? sender, EventArgs e)
        {
            var now = DateTime.Now;
            var tomorrow = now.AddDays(1).Date;
            var todayEnd = now.Date.AddDays(1).AddSeconds(-1);

            CheckDueDateRecursive(Model.TodoItems, now, tomorrow, todayEnd);
        }

        /// <summary>
        /// 递归检查待办项的截止时间
        /// </summary>
        private void CheckDueDateRecursive(ObservableCollection<TodoItemModel> items, DateTime now, DateTime tomorrow, DateTime todayEnd)
        {
            foreach (var item in items)
            {
                if (item.IsCompleted) continue;

                if (item.DueDate.HasValue)
                {
                    var dueDate = item.DueDate.Value;
                    var notificationKey = $"{item.Id}_{dueDate:yyyyMMddHH}";

                    if (notifiedDueDateItems.Contains(notificationKey))
                    {
                        if (dueDate.Date < now.Date.AddDays(-1))
                        {
                            notifiedDueDateItems.Remove(notificationKey);
                        }
                        else
                        {
                            goto CheckChildren;
                        }
                    }

                    if (dueDate < now)
                    {
                        ShowDueDateNotification(item, "已过期", $"待办 '{item.Content}' 已过期！");
                        notifiedDueDateItems.Add(notificationKey);
                    }
                    else if (dueDate <= todayEnd)
                    {
                        var hoursLeft = (dueDate - now).TotalHours;
                        if (hoursLeft <= 1)
                        {
                            ShowDueDateNotification(item, "即将到期", $"待办 '{item.Content}' 将在 {Math.Ceiling(hoursLeft * 60)} 分钟后到期！");
                            notifiedDueDateItems.Add(notificationKey);
                        }
                        else if (hoursLeft <= 3)
                        {
                            ShowDueDateNotification(item, "今天到期", $"待办 '{item.Content}' 今天 {dueDate:HH:mm} 到期");
                            notifiedDueDateItems.Add(notificationKey);
                        }
                    }
                    else if (dueDate.Date == tomorrow)
                    {
                        ShowDueDateNotification(item, "明天到期", $"待办 '{item.Content}' 明天 {dueDate:HH:mm} 到期");
                        notifiedDueDateItems.Add(notificationKey);
                    }
                }

            CheckChildren:
                if (item.SubItems != null && item.SubItems.Count > 0)
                {
                    CheckDueDateRecursive(item.SubItems, now, tomorrow, todayEnd);
                }
            }
        }

        /// <summary>
        /// 显示截止时间通知
        /// </summary>
        private static void ShowDueDateNotification(TodoItemModel item, string title, string message)
        {
            try
            {
                HandyControl.Controls.Growl.Warning(new HandyControl.Data.GrowlInfo
                {
                    Message = message,
                    WaitTime = 5,
                    ShowDateTime = true
                });

                Debug.WriteLine($"[截止时间提醒] {title}: {message}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"显示截止时间通知失败: {ex.Message}");
            }
        }
    }
}
