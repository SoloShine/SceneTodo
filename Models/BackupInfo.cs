using System;

namespace SceneTodo.Models
{
    /// <summary>
    /// 备份信息
    /// </summary>
    public class BackupInfo
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// 备份文件路径
        /// </summary>
        public string FilePath { get; set; } = string.Empty;

        /// <summary>
        /// 备份类型
        /// </summary>
        public BackupType Type { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// 文件大小（字节）
        /// </summary>
        public long FileSize { get; set; }

        /// <summary>
        /// 备份说明
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// 数据库版本
        /// </summary>
        public string? DbVersion { get; set; }

        /// <summary>
        /// 应用版本
        /// </summary>
        public string? AppVersion { get; set; }
    }

    /// <summary>
    /// 备份类型
    /// </summary>
    public enum BackupType
    {
        /// <summary>
        /// 手动备份
        /// </summary>
        Manual = 0,

        /// <summary>
        /// 自动备份
        /// </summary>
        Automatic = 1,

        /// <summary>
        /// 快照备份
        /// </summary>
        Snapshot = 2
    }
}
