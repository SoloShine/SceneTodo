using System.ComponentModel;

namespace SceneTodo.Models
{
    /// <summary>
    /// 关联操作类型枚举
    /// </summary>
    public enum LinkedActionType
    {
        /// <summary>
        /// 打开网页
        /// </summary>
        [Description("打开网页")]
        OpenUrl = 0,

        /// <summary>
        /// 打开文件
        /// </summary>
        [Description("打开文件")]
        OpenFile = 1,

        /// <summary>
        /// 启动应用
        /// </summary>
        [Description("启动应用")]
        LaunchApp = 2
    }

    /// <summary>
    /// 关联操作模型
    /// </summary>
    public class LinkedAction
    {
        /// <summary>
        /// 唯一标识符
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// 显示名称
        /// </summary>
        public string DisplayName { get; set; } = string.Empty;

        /// <summary>
        /// 操作类型
        /// </summary>
        public LinkedActionType ActionType { get; set; }

        /// <summary>
        /// 操作目标（URL、文件路径或应用路径）
        /// </summary>
        public string ActionTarget { get; set; } = string.Empty;

        /// <summary>
        /// 可选参数
        /// </summary>
        public string? Arguments { get; set; }
    }
}
