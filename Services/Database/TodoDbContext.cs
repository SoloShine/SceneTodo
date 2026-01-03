using Microsoft.EntityFrameworkCore;
using SceneTodo.Models;

namespace SceneTodo.Services.Database
{
    /// <summary>
    /// EF Core 数据库上下文
    /// </summary>
    public class TodoDbContext : DbContext
    {
        public TodoDbContext(DbContextOptions<TodoDbContext> options) : base(options)
        {
        }

        /// <summary>
        /// 待办事项表
        /// </summary>
        public DbSet<TodoItem> TodoItems { get; set; }

        public DbSet<AutoTask> AutoTasks { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<TodoItemTag> TodoItemTags { get; set; }

        /// <summary>
        /// 配置实体关系和约束
        /// </summary>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // 配置 TodoItem 实体
            modelBuilder.Entity<TodoItem>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Content).IsRequired();
            });

            // 配置 AutoTask 实体
            modelBuilder.Entity<AutoTask>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired();
                entity.Property(e => e.Cron).IsRequired();
            });

            // 配置 Tag 实体
            modelBuilder.Entity<Tag>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired();
                entity.Property(e => e.Color).HasDefaultValue("#2196F3");
            });

            // 配置 TodoItemTag 关联
            modelBuilder.Entity<TodoItemTag>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.TodoItemId, e.TagId }).IsUnique();
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}
