using System;

namespace SceneTodo.Models
{
    /// <summary>
    /// 待办-标签关联实体
    /// </summary>
    public class TodoItemTag
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        /// <summary>
        /// 待办项ID
        /// </summary>
        public string TodoItemId { get; set; } = string.Empty;
        
        /// <summary>
        /// 标签ID
        /// </summary>
        public string TagId { get; set; } = string.Empty;
        
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
