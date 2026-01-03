using Microsoft.EntityFrameworkCore;
using SceneTodo.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SceneTodo.Services.Database.Repositories
{
    public class TagRepository
    {
        private readonly TodoDbContext dbContext;
        
        public TagRepository(TodoDbContext dbContext)
        {
            this.dbContext = dbContext;
        }
        
        public async Task<IEnumerable<Tag>> GetAllAsync()
        {
            return await dbContext.Tags.ToListAsync();
        }
        
        public async Task<Tag?> GetByIdAsync(string id)
        {
            return await dbContext.Tags.FirstOrDefaultAsync(t => t.Id == id);
        }
        
        public async Task<int> AddAsync(Tag tag)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"?? 准备添加标签: ID={tag.Id}, Name={tag.Name}, Color={tag.Color}");
                
                dbContext.Tags.Add(tag);
                var result = await dbContext.SaveChangesAsync();
                
                System.Diagnostics.Debug.WriteLine($"? 标签添加成功: 影响行数={result}");
                return result;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"? 标签添加失败: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"   内部异常: {ex.InnerException?.Message}");
                throw;
            }
        }
        
        public async Task<int> UpdateAsync(Tag tag)
        {
            // 先从数据库分离现有跟踪
            var existingEntry = dbContext.ChangeTracker.Entries<Tag>()
                .FirstOrDefault(e => e.Entity.Id == tag.Id);
            
            if (existingEntry != null)
            {
                dbContext.Entry(existingEntry.Entity).State = EntityState.Detached;
            }
            
            // 检查数据库中是否存在
            var existing = await dbContext.Tags.AsNoTracking().FirstOrDefaultAsync(t => t.Id == tag.Id);
            if (existing == null)
            {
                System.Diagnostics.Debug.WriteLine($"?? 标签不存在: ID={tag.Id}");
                return 0;
            }
            
            // 更新实体
            dbContext.Tags.Update(tag);
            
            try
            {
                var result = await dbContext.SaveChangesAsync();
                System.Diagnostics.Debug.WriteLine($"? 标签更新成功: ID={tag.Id}, Name={tag.Name}, 影响行数={result}");
                return result;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"? 标签更新失败: {ex.Message}");
                throw;
            }
        }
        
        public async Task<int> DeleteAsync(string id)
        {
            var tag = await dbContext.Tags.FindAsync(id);
            if (tag != null)
            {
                // 同时删除关联
                var relations = await dbContext.TodoItemTags
                    .Where(t => t.TagId == id)
                    .ToListAsync();
                dbContext.TodoItemTags.RemoveRange(relations);
                
                dbContext.Tags.Remove(tag);
                return await dbContext.SaveChangesAsync();
            }
            return 0;
        }
        
        /// <summary>
        /// 获取待办的标签
        /// </summary>
        public async Task<IEnumerable<Tag>> GetTagsForTodoAsync(string todoItemId)
        {
            var tagIds = await dbContext.TodoItemTags
                .Where(t => t.TodoItemId == todoItemId)
                .Select(t => t.TagId)
                .ToListAsync();
            
            return await dbContext.Tags
                .Where(t => tagIds.Contains(t.Id))
                .ToListAsync();
        }
        
        /// <summary>
        /// 为待办添加标签
        /// </summary>
        public async Task<int> AddTagToTodoAsync(string todoItemId, string tagId)
        {
            var exists = await dbContext.TodoItemTags
                .AnyAsync(t => t.TodoItemId == todoItemId && t.TagId == tagId);
            
            if (!exists)
            {
                dbContext.TodoItemTags.Add(new TodoItemTag
                {
                    TodoItemId = todoItemId,
                    TagId = tagId
                });
                return await dbContext.SaveChangesAsync();
            }
            return 0;
        }
        
        /// <summary>
        /// 从待办移除标签
        /// </summary>
        public async Task<int> RemoveTagFromTodoAsync(string todoItemId, string tagId)
        {
            var relation = await dbContext.TodoItemTags
                .FirstOrDefaultAsync(t => t.TodoItemId == todoItemId && t.TagId == tagId);
            
            if (relation != null)
            {
                dbContext.TodoItemTags.Remove(relation);
                return await dbContext.SaveChangesAsync();
            }
            return 0;
        }

        /// <summary>
        /// 获取标签的使用次数
        /// </summary>
        public async Task<int> GetTagUsageCountAsync(string tagId)
        {
            return await dbContext.TodoItemTags
                .Where(t => t.TagId == tagId)
                .CountAsync();
        }
    }
}
