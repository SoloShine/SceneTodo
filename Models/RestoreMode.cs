namespace SceneTodo.Models
{
    /// <summary>
    /// 恢复模式
    /// </summary>
    public enum RestoreMode
    {
        /// <summary>
        /// 完全替换现有数据
        /// </summary>
        Replace = 0,

        /// <summary>
        /// 合并数据（保留两者）
        /// </summary>
        Merge = 1,

        /// <summary>
        /// 跳过现有数据（只添加新数据）
        /// </summary>
        Skip = 2
    }
}
