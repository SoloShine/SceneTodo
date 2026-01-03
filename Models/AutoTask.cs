using Quartz;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SceneTodo.Models
{
    public class AutoTask : BaseModel
    {
        private string id = Guid.NewGuid().ToString();
        /// <summary>
        /// 任务ID
        /// </summary>
        public string Id
        {
            get { return id; }
            set { id = value; OnPropertyChanged(nameof(Id)); }
        }

        private string? todoItemId;
        /// <summary>
        /// 关联的待办事项ID
        /// </summary>
        public string? TodoItemId
        {
            get => todoItemId;
            set
            {
                if (todoItemId != value)
                {
                    todoItemId = value;
                    OnPropertyChanged(nameof(TodoItemId));
                }
            }
        }

        private string? name = string.Empty;
        /// <summary>
        /// 任务名称
        /// </summary>
        public string? Name
        {
            get => name;
            set
            {
                if (name != value)
                {
                    name = value;
                    OnPropertyChanged(nameof(Name));
                }
            }
        }

        private string? description = string.Empty;
        /// <summary>
        /// 任务描述
        /// </summary>
        public string? Description
        {
            get => description;
            set
            {
                if (description != value)
                {
                    description = value;
                    OnPropertyChanged(nameof(Description));
                }
            }
        }

        private string? cron = string.Empty;
        /// <summary>
        /// Cron表达式
        /// </summary>
        public string? Cron
        {
            get => cron;
            set
            {
                if (cron != value)
                {
                    cron = value;
                    OnPropertyChanged(nameof(Cron));
                    UpdateNextExecuteTime();
                }
            }
        }

        private bool isEnabled = true;
        /// <summary>
        /// 是否启用
        /// </summary>
        public bool IsEnabled
        {
            get => isEnabled;
            set
            {
                if (isEnabled != value)
                {
                    isEnabled = value;
                    OnPropertyChanged(nameof(IsEnabled));
                }
            }
        }

        private TaskActionType actionType = TaskActionType.Notification;
        /// <summary>
        /// 任务执行类型
        /// </summary>
        public TaskActionType ActionType
        {
            get => actionType;
            set
            {
                if (actionType != value)
                {
                    actionType = value;
                    OnPropertyChanged(nameof(ActionType));
                }
            }
        }

        private string? actionData;
        /// <summary>
        /// 任务执行数据（JSON格式，根据ActionType不同存储不同内容）
        /// </summary>
        public string? ActionData
        {
            get => actionData;
            set
            {
                if (actionData != value)
                {
                    actionData = value;
                    OnPropertyChanged(nameof(ActionData));
                }
            }
        }

        private DateTime? nextExecuteTime = null;
        /// <summary>
        /// 下次执行时间
        /// </summary>
        public DateTime? NextExecuteTime
        {
            get => nextExecuteTime;
            set
            {
                nextExecuteTime = value;
                OnPropertyChanged(nameof(NextExecuteTime));
            }
        }

        private DateTime? lastExecuteTime = null;
        /// <summary>
        /// 上次执行时间
        /// </summary>
        public DateTime? LastExecuteTime
        {
            get => lastExecuteTime;
            set
            {
                lastExecuteTime = value;
                OnPropertyChanged(nameof(LastExecuteTime));
            }
        }

        private DateTime createdAt = DateTime.Now;
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedAt
        {
            get => createdAt;
            set
            {
                createdAt = value;
                OnPropertyChanged(nameof(CreatedAt));
            }
        }

        private DateTime updatedAt = DateTime.Now;
        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime UpdatedAt
        {
            get => updatedAt;
            set
            {
                updatedAt = value;
                OnPropertyChanged(nameof(UpdatedAt));
            }
        }

        /// <summary>
        /// 更新下次执行时间
        /// </summary>
        public void UpdateNextExecuteTime()
        {
            if (string.IsNullOrWhiteSpace(Cron) || !IsEnabled)
            {
                NextExecuteTime = null;
                return;
            }

            NextExecuteTime = Services.Scheduler.TodoItemSchedulerService.GetNextExecutionTime(Cron);
        }
    }

    /// <summary>
    /// 任务执行类型
    /// </summary>
    public enum TaskActionType
    {
        /// <summary>
        /// 通知提醒
        /// </summary>
        [Description("通知提醒")]
        Notification = 0,

        /// <summary>
        /// 执行关联操作
        /// </summary>
        [Description("执行关联操作")]
        ExecuteLinkedAction = 1,

        /// <summary>
        /// 打开待办详情
        /// </summary>
        [Description("打开待办详情")]
        OpenTodoDetail = 2,

        /// <summary>
        /// 标记完成
        /// </summary>
        [Description("标记完成")]
        MarkAsCompleted = 3
    }
}
