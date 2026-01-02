using System.Collections.ObjectModel;
using System.ComponentModel;

namespace SceneTodo.Models
{
    public class TodoItem : BaseModel
    {
        private string id = string.Empty;
        /// <summary>
        /// 唯一标识符
        /// </summary>
        public string Id
        {
            get => id;
            set
            {
                if (id != value)
                {
                    id = value;
                    OnPropertyChanged(nameof(Id));
                }
            }
        }
        private string name = string.Empty;
        /// <summary>
        /// 名称
        /// </summary>
        public string Name
        {
            get => name;
            set
            {
                if (name != value)
                {
                    name = value;
                    UpdatedAt = DateTime.Now;
                    OnPropertyChanged(nameof(Name));
                }
            }
        }
        private string description = string.Empty;
        /// <summary>
        /// 描述
        /// </summary>
        public string Description
        {
            get => description;
            set
            {
                if (description != value)
                {
                    description = value;
                    UpdatedAt = DateTime.Now;
                    OnPropertyChanged(nameof(Description));
                }
            }
        }
        private string? parentId = string.Empty;
        /// <summary>
        /// 父节点id
        /// </summary>
        public string? ParentId
        {
            get => parentId;
            set
            {
                if (parentId != value)
                {
                    parentId = value;
                    UpdatedAt = DateTime.Now;
                    OnPropertyChanged(nameof(ParentId));
                }
            }
        }

        private string content = "";
        /// <summary>
        /// 内容
        /// </summary>
        public string Content
        {
            get => content;
            set
            {
                if (content != value)
                {
                    content = value;
                    UpdatedAt = DateTime.Now;
                    OnPropertyChanged(nameof(Content));
                }
            }
        }

        private bool isCompleted = false;
        /// <summary>
        /// 是否完成
        /// </summary>
        public bool IsCompleted
        {
            get => isCompleted;
            set
            {
                if (isCompleted != value)
                {
                    isCompleted = value;
                    UpdatedAt = DateTime.Now;
                    if (isCompleted)
                    {
                        CompletedAt = DateTime.Now;
                    }
                    else
                    {
                        CompletedAt = null;
                    }
                    OnPropertyChanged(nameof(IsCompleted));
                }
            }
        }

        private bool isExpanded = false;
        /// <summary>
        /// 是否展开节点
        /// </summary>
        public bool IsExpanded
        {
            get => isExpanded;
            set
            {
                if (isExpanded != value)
                {
                    isExpanded = value;
                    UpdatedAt = DateTime.Now;
                    OnPropertyChanged(nameof(IsExpanded));
                }
            }
        }

        private string? appPath = string.Empty;
        /// <summary>
        /// 应用路径
        /// </summary>
        public string? AppPath
        {
            get => appPath;
            set
            {
                if (appPath != value)
                {
                    appPath = value;
                    UpdatedAt = DateTime.Now;
                    OnPropertyChanged(nameof(AppPath));
                }
            }
        }

        private bool isInjected = false;
        /// <summary>
        /// 是否注入软件
        /// </summary>
        public bool IsInjected
        {
            get => isInjected;
            set
            {
                if (isInjected != value)
                {
                    isInjected = value;
                    UpdatedAt = DateTime.Now;
                    OnPropertyChanged(nameof(IsInjected));
                }
            }
        }

        private TodoItemType todoItemType = TodoItemType.Normal;
        /// <summary>
        /// 待办类型
        /// </summary>
        public TodoItemType TodoItemType
        {
            get => todoItemType;
            set
            {
                if (todoItemType != value)
                {
                    todoItemType = value;
                    UpdatedAt = DateTime.Now;
                    OnPropertyChanged(nameof(TodoItemType));
                }
            }
        }

        private DateTime? createdAt = DateTime.Now;
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime? GreadtedAt
        {
            get => createdAt;
            set
            {
                if (createdAt != value)
                {
                    createdAt = value;
                    OnPropertyChanged(nameof(GreadtedAt));
                }
            }
        }

        private DateTime? updatedAt = DateTime.Now;

        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime? UpdatedAt
        {
            get => updatedAt;
            set
            {
                if (updatedAt != value)
                {
                    updatedAt = value;
                    OnPropertyChanged(nameof(UpdatedAt));
                }
            }
        }

        private DateTime? completedAt = null;
        /// <summary>
        /// 完成时间
        /// </summary>
        public DateTime? CompletedAt
        {
            get => completedAt;
            set
            {
                if (completedAt != value)
                {
                    completedAt = value;
                    OnPropertyChanged(nameof(CompletedAt));
                }
            }
        }

        private DateTime? startTime = null;
        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime? StartTime 
        { 
            get => startTime; 
            set 
            { 
                startTime = value; 
                OnPropertyChanged(nameof(StartTime)); 
            } 
        }

        private DateTime? reminderTime = null;
        /// <summary>
        /// 提醒时间
        /// </summary>
        public DateTime? ReminderTime 
        { 
            get => reminderTime; 
            set 
            { 
                reminderTime = value; 
                OnPropertyChanged(nameof(ReminderTime)); 
            } 
        }
        private DateTime? endTime = null;
        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime? EndTime 
        { 
            get => endTime; 
            set 
            { 
                endTime = value; 
                OnPropertyChanged(nameof(EndTime)); 
            } 
        }

        private Priority priority = Priority.Medium;
        /// <summary>
        /// 优先级
        /// </summary>
        public Priority Priority
        {
            get => priority;
            set
            {
                if (priority != value)
                {
                    priority = value;
                    UpdatedAt = DateTime.Now;
                    OnPropertyChanged(nameof(Priority));
                }
            }
        }

        private string linkedActionsJson = "[]";
        /// <summary>
        /// 关联操作JSON字符串
        /// </summary>
        public string LinkedActionsJson
        {
            get => linkedActionsJson;
            set
            {
                if (linkedActionsJson != value)
                {
                    linkedActionsJson = value;
                    UpdatedAt = DateTime.Now;
                    OnPropertyChanged(nameof(LinkedActionsJson));
                }
            }
        }

        private OverlayPosition overlayPosition = OverlayPosition.Bottom;
        /// <summary>
        /// 遮盖层位置
        /// </summary>
        public OverlayPosition OverlayPosition
        {
            get => overlayPosition;
            set
            {
                if (overlayPosition != value)
                {
                    overlayPosition = value;
                    UpdatedAt = DateTime.Now;
                    OnPropertyChanged(nameof(OverlayPosition));
                }
            }
        }

        private double overlayOffsetX = 0;
        /// <summary>
        /// 遮盖层X轴偏移量
        /// </summary>
        public double OverlayOffsetX
        {
            get => overlayOffsetX;
            set
            {
                if (Math.Abs(overlayOffsetX - value) > 0.01)
                {
                    overlayOffsetX = value;
                    UpdatedAt = DateTime.Now;
                    OnPropertyChanged(nameof(OverlayOffsetX));
                }
            }
        }

        private double overlayOffsetY = 0;
        /// <summary>
        /// 遮盖层Y轴偏移量
        /// </summary>
        public double OverlayOffsetY
        {
            get => overlayOffsetY;
            set
            {
                if (Math.Abs(overlayOffsetY - value) > 0.01)
                {
                    overlayOffsetY = value;
                    UpdatedAt = DateTime.Now;
                    OnPropertyChanged(nameof(OverlayOffsetY));
                }
            }
        }
    }

    public enum TodoItemType
    {
        /// <summary>
        /// 普通待办
        /// </summary>
        [Description("普通待办")]
        Normal,
        /// <summary>
        /// 软件待办
        /// </summary>
        [Description("软件待办")]
        App,
    }

    public enum Priority
    {
        /// <summary>
        /// 极低
        /// </summary>
        [Description("极低")]
        VeryLow,
        /// <summary>
        /// 低
        /// </summary>
        [Description("低")]
        Low,
        /// <summary>
        /// 中
        /// </summary>
        [Description("中")]
        Medium,
        /// <summary>
        /// 高
        /// </summary>
        [Description("高")]
        High,
        /// <summary>
        /// 极高
        /// </summary>
        [Description("极高")]
        VeryHigh
    }

    public enum OverlayPosition
    {
        /// <summary>
        /// 窗口下方（默认）
        /// </summary>
        [Description("窗口下方")]
        Bottom = 0,
        /// <summary>
        /// 左上角
        /// </summary>
        [Description("左上角")]
        TopLeft = 1,
        /// <summary>
        /// 右上角
        /// </summary>
        [Description("右上角")]
        TopRight = 2,
        /// <summary>
        /// 左下角
        /// </summary>
        [Description("左下角")]
        BottomLeft = 3,
        /// <summary>
        /// 右下角
        /// </summary>
        [Description("右下角")]
        BottomRight = 4,
        /// <summary>
        /// 居中
        /// </summary>
        [Description("居中")]
        Center = 5
    }
}
