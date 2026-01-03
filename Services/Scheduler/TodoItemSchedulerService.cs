using Quartz;
using Quartz.Impl;
using System.Collections.Specialized;
using SceneTodo.Models;
using System;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.Text.Json;

namespace SceneTodo.Services.Scheduler
{
    public class TodoItemSchedulerService
    {
        private IScheduler _scheduler;

        public TodoItemSchedulerService()
        {
            InitializeScheduler().Wait();
        }

        public async Task ShutdownAsync()
        {
            if (_scheduler != null && !_scheduler.IsShutdown)
            {
                await _scheduler.Shutdown();
            }
        }

        private async Task InitializeScheduler()
        {
            NameValueCollection props = new NameValueCollection
            {
                { "quartz.serializer.type", "binary" }
            };
            StdSchedulerFactory factory = new StdSchedulerFactory(props);
            _scheduler = await factory.GetScheduler();
            await _scheduler.Start();
        }

        public async Task ScheduleTodoItemReminder(AutoTask task)
        {
            if (string.IsNullOrWhiteSpace(task.Cron))
                return;

            IJobDetail job = JobBuilder.Create<TodoItemReminderJob>()
                .WithIdentity(task.Id, "taskReminders")
                .UsingJobData("taskId", task.Id)
                .Build();

            ITrigger trigger = TriggerBuilder.Create()
                .WithIdentity($"{task.Id}_trigger", "taskReminders")
                .WithCronSchedule(task.Cron)
                .Build();

            await _scheduler.ScheduleJob(job, trigger);
        }

        public async Task UnscheduleTodoItemReminder(string taskId)
        {
            await _scheduler.DeleteJob(new JobKey(taskId, "taskReminders"));
        }

        /// <summary>
        /// Schedule an AutoTask for execution
        /// </summary>
        public async Task ScheduleAutoTask(AutoTask task)
        {
            if (string.IsNullOrWhiteSpace(task.Cron))
                return;

            if (!task.IsEnabled)
                return;

            // Validate cron expression
            if (!CronExpression.IsValidExpression(task.Cron))
            {
                throw new ArgumentException($"Invalid cron expression: {task.Cron}");
            }

            // First unschedule if already exists
            await UnscheduleAutoTask(task.Id);

            IJobDetail job = JobBuilder.Create<TodoItemReminderJob>()
                .WithIdentity(task.Id, "autoTasks")
                .UsingJobData("taskId", task.Id)
                .Build();

            ITrigger trigger = TriggerBuilder.Create()
                .WithIdentity($"{task.Id}_trigger", "autoTasks")
                .WithCronSchedule(task.Cron)
                .Build();

            await _scheduler.ScheduleJob(job, trigger);

            // Update next execution time
            task.NextExecuteTime = trigger.GetNextFireTimeUtc()?.LocalDateTime;
            await App.AutoTaskRepository.UpdateAsync(task);

            Debug.WriteLine($"Scheduled task: {task.Name} with cron: {task.Cron}");
        }

        /// <summary>
        /// Unschedule an AutoTask
        /// </summary>
        public async Task UnscheduleAutoTask(string taskId)
        {
            var jobKey = new JobKey(taskId, "autoTasks");
            if (await _scheduler.CheckExists(jobKey))
            {
                await _scheduler.DeleteJob(jobKey);
                Debug.WriteLine($"Unscheduled task: {taskId}");
            }
        }

        /// <summary>
        /// Validate cron expression
        /// </summary>
        public static bool IsValidCronExpression(string cronExpression)
        {
            if (string.IsNullOrWhiteSpace(cronExpression))
                return false;

            return CronExpression.IsValidExpression(cronExpression);
        }

        /// <summary>
        /// Get next execution time for a cron expression
        /// </summary>
        public static DateTime? GetNextExecutionTime(string cronExpression)
        {
            if (!IsValidCronExpression(cronExpression))
                return null;

            try
            {
                var cron = new CronExpression(cronExpression);
                return cron.GetNextValidTimeAfter(DateTime.Now)?.LocalDateTime;
            }
            catch
            {
                return null;
            }
        }
    }

    public class TodoItemReminderJob : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            var taskId = context.JobDetail.JobDataMap.GetString("taskId");
            if (string.IsNullOrEmpty(taskId)) return;

            try
            {
                // 从数据库获取task
                var task = await App.AutoTaskRepository.GetByIdAsync(taskId);
                if (task == null) return;

                // 更新上次执行时间
                task.LastExecuteTime = DateTime.Now;
                task.UpdateNextExecuteTime();
                await App.AutoTaskRepository.UpdateAsync(task);

                // 根据任务类型执行不同的操作
                await App.Current.Dispatcher.InvokeAsync(async () =>
                {
                    try
                    {
                        switch (task.ActionType)
                        {
                            case TaskActionType.Notification:
                                await ExecuteNotificationAction(task);
                                break;

                            case TaskActionType.ExecuteLinkedAction:
                                await ExecuteLinkedActionAction(task);
                                break;

                            case TaskActionType.OpenTodoDetail:
                                await ExecuteOpenTodoDetailAction(task);
                                break;

                            case TaskActionType.MarkAsCompleted:
                                await ExecuteMarkAsCompletedAction(task);
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"执行任务失败: {ex.Message}");
                        HandyControl.Controls.Growl.Error($"定时任务执行失败: {ex.Message}");
                    }
                });

                Debug.WriteLine($"定时任务执行成功: {task.Name}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"定时任务执行异常: {ex.Message}");
            }
        }

        private static async Task ExecuteNotificationAction(AutoTask task)
        {
            var notification = new HandyControl.Controls.Notification
            {
                Title = "定时任务提醒",
                Content = $"{task.Name}\n{task.Description ?? ""}"
            };
            notification.Show();

            // 如果关联了待办，也显示待办信息
            if (!string.IsNullOrEmpty(task.TodoItemId))
            {
                var todo = await App.TodoItemRepository.GetByIdAsync(task.TodoItemId);
                if (todo != null)
                {
                    var todoNotification = new HandyControl.Controls.Notification
                    {
                        Title = $"关联待办: {todo.Name}",
                        Content = todo.Content
                    };
                    todoNotification.Show();
                }
            }
        }

        private static async Task ExecuteLinkedActionAction(AutoTask task)
        {
            if (string.IsNullOrEmpty(task.TodoItemId))
            {
                HandyControl.Controls.Growl.Warning("任务未关联待办，无法执行关联操作");
                return;
            }

            var todo = await App.TodoItemRepository.GetByIdAsync(task.TodoItemId);
            if (todo == null)
            {
                HandyControl.Controls.Growl.Error("找不到关联的待办项");
                return;
            }

            var linkedActions = JsonSerializer.Deserialize<ObservableCollection<LinkedAction>>(
                todo.LinkedActionsJson ?? "[]");

            if (linkedActions == null || linkedActions.Count == 0)
            {
                HandyControl.Controls.Growl.Warning("待办项没有关联操作");
                return;
            }

            // 执行第一个关联操作
            var action = linkedActions[0];
            App.MainViewModel?.ExecuteLinkedActionCommand.Execute(action);

            HandyControl.Controls.Growl.Success($"已执行关联操作: {action.DisplayName}");
        }

        private static async Task ExecuteOpenTodoDetailAction(AutoTask task)
        {
            if (string.IsNullOrEmpty(task.TodoItemId))
            {
                HandyControl.Controls.Growl.Warning("任务未关联待办");
                return;
            }

            var todo = await App.TodoItemRepository.GetByIdAsync(task.TodoItemId);
            if (todo == null)
            {
                HandyControl.Controls.Growl.Error("找不到关联的待办项");
                return;
            }

            // 打开编辑窗口
            App.MainViewModel?.EditTodoItemCommand.Execute(todo);
        }

        private static async Task ExecuteMarkAsCompletedAction(AutoTask task)
        {
            if (string.IsNullOrEmpty(task.TodoItemId))
            {
                HandyControl.Controls.Growl.Warning("任务未关联待办");
                return;
            }

            var todo = await App.TodoItemRepository.GetByIdAsync(task.TodoItemId);
            if (todo == null)
            {
                HandyControl.Controls.Growl.Error("找不到关联的待办项");
                return;
            }

            if (!todo.IsCompleted)
            {
                todo.IsCompleted = true;
                todo.CompletedAt = DateTime.Now;
                await App.TodoItemRepository.UpdateAsync(todo);
                HandyControl.Controls.Growl.Success($"待办 '{todo.Name}' 已标记为完成");
            }
        }
    }
}
