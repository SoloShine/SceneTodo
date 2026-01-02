using System.Windows;
using Microsoft.Win32;
using SceneTodo.Models;
using MessageBox = HandyControl.Controls.MessageBox;
using System.Windows.Controls;

namespace SceneTodo.Views
{
    public partial class EditLinkedActionWindow : HandyControl.Controls.Window
    {
        public LinkedAction Action { get; private set; }

        public EditLinkedActionWindow(LinkedAction? action = null)
        {
            InitializeComponent();

            if (action != null)
            {
                Action = new LinkedAction
                {
                    Id = action.Id,
                    DisplayName = action.DisplayName,
                    ActionType = action.ActionType,
                    ActionTarget = action.ActionTarget,
                    Arguments = action.Arguments
                };

                DisplayNameTextBox.Text = Action.DisplayName;
                ActionTargetTextBox.Text = Action.ActionTarget;
                ArgumentsTextBox.Text = Action.Arguments;

                // 设置操作类型
                foreach (ComboBoxItem item in ActionTypeComboBox.Items)
                {
                    if (item.Tag.ToString() == Action.ActionType.ToString())
                    {
                        ActionTypeComboBox.SelectedItem = item;
                        break;
                    }
                }
            }
            else
            {
                Action = new LinkedAction();
            }

            UpdateUIForActionType();
        }

        private void ActionTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateUIForActionType();
        }

        private void UpdateUIForActionType()
        {
            if (ActionTypeComboBox.SelectedItem is not ComboBoxItem selectedItem)
                return;

            // 添加初始化检查,防止在 InitializeComponent 期间访问尚未初始化的控件
            if (ActionTargetLabel == null)
                return;

            string actionType = selectedItem.Tag.ToString() ?? "OpenUrl";

            switch (actionType)
            {
                case "OpenUrl":
                    ActionTargetLabel.Text = "URL地址:";
                    BrowseButton.Visibility = Visibility.Collapsed;
                    ArgumentsLabel.Visibility = Visibility.Collapsed;
                    ArgumentsTextBox.Visibility = Visibility.Collapsed;
                    break;

                case "OpenFile":
                    ActionTargetLabel.Text = "文件路径:";
                    BrowseButton.Visibility = Visibility.Visible;
                    ArgumentsLabel.Visibility = Visibility.Collapsed;
                    ArgumentsTextBox.Visibility = Visibility.Collapsed;
                    break;

                case "LaunchApp":
                    ActionTargetLabel.Text = "应用路径:";
                    BrowseButton.Visibility = Visibility.Visible;
                    ArgumentsLabel.Visibility = Visibility.Visible;
                    ArgumentsTextBox.Visibility = Visibility.Visible;
                    break;
            }
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            if (ActionTypeComboBox.SelectedItem is not ComboBoxItem selectedItem)
                return;

            string actionType = selectedItem.Tag.ToString() ?? "OpenUrl";

            var openFileDialog = new OpenFileDialog();

            if (actionType == "OpenFile")
            {
                openFileDialog.Filter = "所有文件 (*.*)|*.*";
                openFileDialog.Title = "选择文件";
            }
            else if (actionType == "LaunchApp")
            {
                openFileDialog.Filter = "可执行文件 (*.exe)|*.exe|所有文件 (*.*)|*.*";
                openFileDialog.Title = "选择应用程序";
            }

            if (openFileDialog.ShowDialog() == true)
            {
                ActionTargetTextBox.Text = openFileDialog.FileName;
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // 验证数据
            if (string.IsNullOrWhiteSpace(DisplayNameTextBox.Text))
            {
                MessageBox.Show("显示名称不能为空！", "验证错误", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(ActionTargetTextBox.Text))
            {
                MessageBox.Show("操作目标不能为空！", "验证错误", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // 保存数据
            Action.DisplayName = DisplayNameTextBox.Text;
            Action.ActionTarget = ActionTargetTextBox.Text;
            Action.Arguments = ArgumentsTextBox.Text;

            if (ActionTypeComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                Action.ActionType = selectedItem.Tag.ToString() switch
                {
                    "OpenFile" => LinkedActionType.OpenFile,
                    "LaunchApp" => LinkedActionType.LaunchApp,
                    _ => LinkedActionType.OpenUrl
                };
            }

            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
