using System;
using System.Windows;
using SceneTodo.Views;
using MessageBox = HandyControl.Controls.MessageBox;

namespace SceneTodo.ViewModels
{
    /// <summary>
    /// 主窗口 ViewModel - 导航和窗口管理
    /// 包含：页面导航、窗口打开等方法
    /// </summary>
    public partial class MainWindowViewModel
    {
        /// <summary>
        /// 主题设置
        /// </summary>
        private void ThemeSettings(object? parameter)
        {
            var themeSettingsWindow = new ThemeSettingsWindow();
            themeSettingsWindow.ShowDialog();
        }

        /// <summary>
        /// 关于窗口
        /// </summary>
        private void About(object? parameter)
        {
            var aboutWindow = new AboutWindow();
            aboutWindow.ShowDialog();
        }

        /// <summary>
        /// 显示历史记录窗口
        /// </summary>
        private void ShowHistory(object? parameter)
        {
            var historyWindow = new HistoryWindow();
            historyWindow.ShowDialog();
        }

        /// <summary>
        /// 显示历史记录页面
        /// </summary>
        private void ShowHistoryPage(object? parameter)
        {
            CurrentContent = new HistoryUserControl();
        }

        /// <summary>
        /// 显示待办列表页面
        /// </summary>
        private void ShowTodoListPage(object? parameter)
        {
            CurrentContent = todoListContent;
        }

        /// <summary>
        /// 显示日历视图
        /// </summary>
        private void ShowCalendarView(object? parameter)
        {
            CurrentContent = new CalendarViewControl();
        }

        /// <summary>
        /// 显示定时任务页面
        /// </summary>
        private void ShowScheduledTasks(object? parameter)
        {
            CurrentContent = Application.LoadComponent(new Uri("/SceneTodo;component/Views/ScheduledTasksPage.xaml", UriKind.Relative));
        }

        /// <summary>
        /// 打开备份管理窗口
        /// </summary>
        private void OpenBackupManagement(object? parameter)
        {
            try
            {
                var backupWindow = new BackupManagementWindow
                {
                    Owner = Application.Current.MainWindow
                };
                backupWindow.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Error($"打开备份管理窗口失败: {ex.Message}", "错误");
            }
        }
    }
}
