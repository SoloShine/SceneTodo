using System;

namespace SceneTodo.Models
{
    /// <summary>
    /// 备份设置
    /// </summary>
    public class BackupSettings
    {
        /// <summary>
        /// 是否启用自动备份
        /// </summary>
        public bool AutoBackupEnabled { get; set; } = false;

        /// <summary>
        /// 自动备份频率
        /// </summary>
        public BackupFrequency Frequency { get; set; } = BackupFrequency.Daily;

        /// <summary>
        /// 备份保留数量
        /// </summary>
        public int RetentionCount { get; set; } = 10;

        /// <summary>
        /// 备份目录
        /// </summary>
        public string? BackupDirectory { get; set; }

        /// <summary>
        /// 上次备份时间
        /// </summary>
        public DateTime? LastBackupTime { get; set; }
    }

    /// <summary>
    /// 备份频率
    /// </summary>
    public enum BackupFrequency
    {
        /// <summary>
        /// 每天
        /// </summary>
        Daily = 0,

        /// <summary>
        /// 每周
        /// </summary>
        Weekly = 1,

        /// <summary>
        /// 每月
        /// </summary>
        Monthly = 2
    }
}
