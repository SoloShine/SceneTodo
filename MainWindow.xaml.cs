using System.Windows;
using SceneTodo.Models;
using SceneTodo.Utils;
using SceneTodo.ViewModels;
using SceneTodo.Views;
using MessageBox = HandyControl.Controls.MessageBox;

namespace SceneTodo
{
    public partial class MainWindow : HandyControl.Controls.Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = App.MainViewModel;
            Closed += MainWindow_Closed;
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            //弹出提示是关闭应用还是隐藏到托盘，选择关闭则正常退出，选择隐藏到托盘则隐藏窗口，取消则不关闭窗口
            var result = MessageBox.Show("关闭应用还是最小化到托盘？是直接退出，否最小化到托盘", "确认退出", MessageBoxButton.YesNoCancel);
            if (result == MessageBoxResult.Yes)
            {
                return;
            }
            else if (result == MessageBoxResult.No)
            {
                // 阻止窗口关闭，改为隐藏窗口
                e.Cancel = true;
                this.Hide();

                // 发送通知气泡提示用户
                TrayIconManager.SendMessage("应用已最小化到托盘。");
            }
            else if (result == MessageBoxResult.Cancel)
            {
                e.Cancel = true;
                return;
            }
        }

        private void MainWindow_Closed(object? sender, System.EventArgs e)
        {
            if (DataContext is MainWindowViewModel vm)
            {
                vm.Cleanup();
            }
        }

        /// <summary>
        /// 标签筛选请求处理
        /// </summary>
        private void TagsPanel_TagFilterRequested(object? sender, Tag tag)
        {
            if (DataContext is MainWindowViewModel vm)
            {
                vm.FilterByTag(tag);
                // 更新标签面板的筛选状态
                TagsPanel.SetFilterStatus(tag);
            }
        }
    }
}
