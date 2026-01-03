using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using SceneTodo.Models;
using MessageBox = HandyControl.Controls.MessageBox;

namespace SceneTodo.ViewModels
{
    public class ScheduledTasksViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private ObservableCollection<AutoTask> tasks;
        public ObservableCollection<AutoTask> Tasks
        {
            get => tasks;
            set
            {
                tasks = value;
                OnPropertyChanged(nameof(Tasks));
            }
        }

        public ICommand AddTaskCommand { get; }
        public ICommand EditTaskCommand { get; }
        public ICommand DeleteTaskCommand { get; }
        public ICommand ToggleTaskCommand { get; }

        public ScheduledTasksViewModel()
        {
            tasks = new ObservableCollection<AutoTask>();
            
            AddTaskCommand = new RelayCommand(AddTask);
            EditTaskCommand = new RelayCommand(EditTask);
            DeleteTaskCommand = new RelayCommand(DeleteTask);
            ToggleTaskCommand = new RelayCommand(ToggleTask);

            LoadTasks();
        }

        private async void LoadTasks()
        {
            try
            {
                var allTasks = await App.AutoTaskRepository.GetAllAsync();
                Tasks = new ObservableCollection<AutoTask>(allTasks);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load tasks: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddTask(object? parameter)
        {
            var editWindow = new Views.EditScheduledTaskWindow();
            if (editWindow.ShowDialog() == true && editWindow.TaskData != null)
            {
                var task = editWindow.TaskData;
                task.UpdateNextExecuteTime();
                
                Tasks.Add(task);
                SaveTaskAsync(task);
                
                if (task.IsEnabled)
                {
                    ScheduleTaskAsync(task);
                }
            }
        }

        private void EditTask(object? parameter)
        {
            if (parameter is not AutoTask task) return;

            var editWindow = new Views.EditScheduledTaskWindow(task);
            if (editWindow.ShowDialog() == true && editWindow.TaskData != null)
            {
                var updatedTask = editWindow.TaskData;
                updatedTask.UpdateNextExecuteTime();
                
                var index = Tasks.IndexOf(task);
                if (index >= 0)
                {
                    Tasks[index] = updatedTask;
                    SaveTaskAsync(updatedTask);
                    
                    // Unschedule old task and schedule new one if enabled
                    UnscheduleTaskAsync(task.Id);
                    if (updatedTask.IsEnabled)
                    {
                        ScheduleTaskAsync(updatedTask);
                    }
                }
            }
        }

        private async void DeleteTask(object? parameter)
        {
            if (parameter is not AutoTask task) return;

            var result = MessageBox.Show(
                $"Are you sure you want to delete task '{task.Name}'?",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                Tasks.Remove(task);
                
                await UnscheduleTaskAsync(task.Id);
                await App.AutoTaskRepository.DeleteAsync(task.Id);
            }
        }

        private async void ToggleTask(object? parameter)
        {
            if (parameter is not AutoTask task) return;

            task.IsEnabled = !task.IsEnabled;
            task.UpdatedAt = DateTime.Now;
            task.UpdateNextExecuteTime();
            
            await App.AutoTaskRepository.UpdateAsync(task);

            if (task.IsEnabled)
            {
                await ScheduleTaskAsync(task);
            }
            else
            {
                await UnscheduleTaskAsync(task.Id);
            }

            OnPropertyChanged(nameof(Tasks));
        }

        private async void SaveTaskAsync(AutoTask task)
        {
            try
            {
                if (string.IsNullOrEmpty(task.Id))
                {
                    task.Id = Guid.NewGuid().ToString();
                    task.CreatedAt = DateTime.Now;
                }
                
                task.UpdatedAt = DateTime.Now;
                await App.AutoTaskRepository.AddAsync(task);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to save task: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task ScheduleTaskAsync(AutoTask task)
        {
            try
            {
                await App.SchedulerService.ScheduleAutoTask(task);
                HandyControl.Controls.Growl.Success($"Task '{task.Name}' scheduled successfully");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to schedule task: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task UnscheduleTaskAsync(string taskId)
        {
            try
            {
                await App.SchedulerService.UnscheduleAutoTask(taskId);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to unschedule task: {ex.Message}");
            }
        }
    }
}
