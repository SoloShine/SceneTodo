using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Text.Json;
using SceneTodo.Models;
using MessageBox = HandyControl.Controls.MessageBox;

namespace SceneTodo.Views
{
    public partial class EditScheduledTaskWindow : HandyControl.Controls.Window
    {
        public AutoTask? TaskData { get; private set; }

        public EditScheduledTaskWindow()
        {
            InitializeComponent();
            InitializeData();
            ActionTypeComboBox.SelectedIndex = 0;
        }

        public EditScheduledTaskWindow(AutoTask task)
        {
            InitializeComponent();
            InitializeData();
            LoadTask(task);
        }

        private void InitializeData()
        {
            var todoItems = App.TodoItemRepository.GetAllAsync().Result;
            TodoItemComboBox.ItemsSource = todoItems;
        }

        private void LoadTask(AutoTask task)
        {
            TaskNameTextBox.Text = task.Name;
            DescriptionTextBox.Text = task.Description;
            CronTextBox.Text = task.Cron;
            IsEnabledCheckBox.IsChecked = task.IsEnabled;

            ActionTypeComboBox.SelectedIndex = (int)task.ActionType;

            if (!string.IsNullOrEmpty(task.ActionData))
            {
                try
                {
                    var actionData = JsonSerializer.Deserialize<ActionData>(task.ActionData);
                    if (actionData != null && !string.IsNullOrEmpty(actionData.TodoItemId))
                    {
                        TodoItemComboBox.SelectedValue = actionData.TodoItemId;
                    }
                }
                catch
                {
                    // Ignore JSON errors
                }
            }

            // Validate cron expression after loading
            ValidateCronExpression();
        }

        private void ActionTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TodoSelectionPanel == null) return;

            var selectedIndex = ActionTypeComboBox.SelectedIndex;
            TodoSelectionPanel.Visibility = (selectedIndex == 1 || selectedIndex == 2 || selectedIndex == 3) 
                ? Visibility.Visible 
                : Visibility.Collapsed;
        }

        private void CronPattern_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string pattern)
            {
                CronTextBox.Text = pattern;
            }
        }

        private void CronTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            ValidateCronExpression();
        }

        private void ValidateCronExpression()
        {
            if (CronTextBox == null || CronValidationText == null || CronNextExecutionText == null)
                return;

            var cronText = CronTextBox.Text?.Trim();
            
            if (string.IsNullOrWhiteSpace(cronText))
            {
                CronValidationText.Text = "Format: Second Minute Hour Day Month Weekday [Year]";
                CronValidationText.Foreground = System.Windows.Media.Brushes.Gray;
                CronNextExecutionText.Visibility = Visibility.Collapsed;
                return;
            }

            bool isValid = Services.Scheduler.TodoItemSchedulerService.IsValidCronExpression(cronText);

            if (isValid)
            {
                CronValidationText.Text = "? Valid cron expression";
                CronValidationText.Foreground = System.Windows.Media.Brushes.Green;

                var nextExecution = Services.Scheduler.TodoItemSchedulerService.GetNextExecutionTime(cronText);
                if (nextExecution.HasValue)
                {
                    CronNextExecutionText.Text = $"Next execution: {nextExecution.Value:yyyy-MM-dd HH:mm:ss}";
                    CronNextExecutionText.Visibility = Visibility.Visible;
                }
                else
                {
                    CronNextExecutionText.Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                CronValidationText.Text = "? Invalid cron expression";
                CronValidationText.Foreground = System.Windows.Media.Brushes.Red;
                CronNextExecutionText.Visibility = Visibility.Collapsed;
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TaskNameTextBox.Text))
            {
                MessageBox.Show("Task name is required!", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(CronTextBox.Text))
            {
                MessageBox.Show("Cron expression is required!", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Validate cron expression
            if (!Services.Scheduler.TodoItemSchedulerService.IsValidCronExpression(CronTextBox.Text))
            {
                MessageBox.Show("Invalid cron expression! Please check the format.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var actionType = (TaskActionType)ActionTypeComboBox.SelectedIndex;
                string actionData = "{}";
                string todoItemId = null;

                if (actionType != TaskActionType.Notification)
                {
                    if (TodoItemComboBox.SelectedValue == null)
                    {
                        MessageBox.Show("Please select a todo item!", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    todoItemId = TodoItemComboBox.SelectedValue.ToString();
                    actionData = JsonSerializer.Serialize(new ActionData
                    {
                        TodoItemId = todoItemId
                    });
                }

                TaskData = new AutoTask
                {
                    Id = TaskData?.Id ?? Guid.NewGuid().ToString(),
                    TodoItemId = todoItemId,
                    Name = TaskNameTextBox.Text,
                    Description = DescriptionTextBox.Text,
                    Cron = CronTextBox.Text.Trim(),
                    IsEnabled = IsEnabledCheckBox.IsChecked ?? false,
                    ActionType = actionType,
                    ActionData = actionData,
                    CreatedAt = TaskData?.CreatedAt ?? DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                // Calculate next execution time
                TaskData.UpdateNextExecuteTime();

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save task: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private class ActionData
        {
            public string? TodoItemId { get; set; }
        }
    }
}
