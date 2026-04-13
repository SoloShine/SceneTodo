using SceneTodo.Views;
using System.Windows;
using MessageBox = HandyControl.Controls.MessageBox;

namespace SceneTodo.ViewModels
{
    /// <summary>
    /// ������ ViewModel - �����ʹ��ڹ���
    /// ������ҳ�浼�������ڴ򿪵ȷ���
    /// </summary>
    public partial class MainWindowViewModel
    {
        /// <summary>
        /// ��������
        /// </summary>
        private void ThemeSettings(object? parameter)
        {
            var themeSettingsWindow = new ThemeSettingsWindow();
            themeSettingsWindow.ShowDialog();
        }

        /// <summary>
        /// ���ڴ���
        /// </summary>
        private void About(object? parameter)
        {
            var aboutWindow = new AboutWindow();
            aboutWindow.ShowDialog();
        }

        /// <summary>
        /// ��ʾ��ʷ��¼����
        /// </summary>
        private void ShowHistory(object? parameter)
        {
            var historyWindow = new HistoryWindow();
            historyWindow.ShowDialog();
        }

        /// <summary>
        /// ��ʾ��ʷ��¼ҳ��
        /// </summary>
        private void ShowHistoryPage(object? parameter)
        {
            CurrentContent = new HistoryUserControl();
            IsSearchVisible = false;
        }

        /// <summary>
        /// ��ʾ�����б�ҳ��
        /// </summary>
        private void ShowTodoListPage(object? parameter)
        {
            CurrentContent = todoListContent;
            IsSearchVisible = true;
        }

        /// <summary>
        /// ��ʾ������ͼ
        /// </summary>
        private void ShowCalendarView(object? parameter)
        {
            CurrentContent = new CalendarViewControl();
            IsSearchVisible = false;
        }

        /// <summary>
        /// ��ʾ��ʱ����ҳ��
        /// </summary>
        private void ShowScheduledTasks(object? parameter)
        {
            CurrentContent = Application.LoadComponent(new Uri("/SceneTodo;component/Views/ScheduledTasksPage.xaml", UriKind.Relative));
            IsSearchVisible = false;
        }

        /// <summary>
        /// �򿪱��ݹ�������
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
                MessageBox.Error($"�򿪱��ݹ�������ʧ��: {ex.Message}", "����");
            }
        }
    }
}
