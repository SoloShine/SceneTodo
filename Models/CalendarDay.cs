using System;

namespace SceneTodo.Models
{
    /// <summary>
    /// 日历日期模型
    /// </summary>
    public class CalendarDay
    {
        /// <summary>
        /// 日期
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// 日期数字（1-31）
        /// </summary>
        public int Day { get; set; }

        /// <summary>
        /// 是否属于当前月份
        /// </summary>
        public bool IsCurrentMonth { get; set; }

        /// <summary>
        /// 是否是今天
        /// </summary>
        public bool IsToday { get; set; }

        /// <summary>
        /// 该日期的待办项数量
        /// </summary>
        public int TodoCount { get; set; }

        /// <summary>
        /// 是否有待办项
        /// </summary>
        public bool HasTodos => TodoCount > 0;
    }
}
