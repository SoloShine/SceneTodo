using Microsoft.EntityFrameworkCore;
using SceneTodo.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace SceneTodo.Services.Database
{
    /// <summary>
    /// 数据库初始化器
    /// </summary>
    public class DatabaseInitializer
    {
        private readonly TodoDbContext dbContext;
        
        public DatabaseInitializer(TodoDbContext dbContext)
        {
            this.dbContext = dbContext;
        }
        
        /// <summary>
        /// 初始化数据库
        /// </summary>
        public async Task InitializeAsync()
        {
            try
            {
                // 确保数据库已创建
                await dbContext.Database.EnsureCreatedAsync();
                
                // 检查数据库架构是否需要迁移
                bool needsMigration = await CheckIfMigrationNeededAsync();
                
                if (needsMigration)
                {
                    System.Diagnostics.Debug.WriteLine("检测到数据库架构需要更新，开始迁移...");
                    await MigrateDatabaseAsync();
                }
                else
                {
                    // 检查是否需要填充初始数据（仅当数据库为空时）
                    if (!await dbContext.TodoItems.AnyAsync())
                    {
                        await SeedTestDataAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"数据库初始化失败: {ex.Message}");
                // 如果出现任何错误，尝试迁移数据库
                await MigrateDatabaseAsync();
            }
        }

        /// <summary>
        /// 检查是否需要数据库迁移
        /// </summary>
        private async Task<bool> CheckIfMigrationNeededAsync()
        {
            try
            {
                // 获取数据库连接
                var connection = dbContext.Database.GetDbConnection();
                await connection.OpenAsync();

                try
                {
                    // 检查 TodoItems 表的列
                    using var command = connection.CreateCommand();
                    command.CommandText = "PRAGMA table_info(TodoItems)";
                    
                    var columns = new HashSet<string>();
                    using var reader = await command.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        var columnName = reader.GetString(1); // 列名在第2列（索引1）
                        columns.Add(columnName);
                    }

                    // 检查必需的列是否存在
                    var requiredColumns = new[]
                    {
                        "Id", "Name", "Description", "Content", "ParentId",
                        "IsCompleted", "IsExpanded", "AppPath", "IsInjected",
                        "TodoItemType", "GreadtedAt", "UpdatedAt", "CompletedAt",
                        "StartTime", "ReminderTime", "EndTime", "DueDate",
                        "Priority", "LinkedActionsJson", "TagsJson",
                        "OverlayPosition", "OverlayOffsetX", "OverlayOffsetY"
                    };

                    foreach (var column in requiredColumns)
                    {
                        if (!columns.Contains(column))
                        {
                            System.Diagnostics.Debug.WriteLine($"缺少列: {column}");
                            return true; // 需要迁移
                        }
                    }

                    // 检查 Tags 表是否存在
                    command.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name='Tags'";
                    var tagsTableExists = await command.ExecuteScalarAsync();
                    
                    if (tagsTableExists == null)
                    {
                        System.Diagnostics.Debug.WriteLine("缺少 Tags 表");
                        return true; // 需要迁移
                    }

                    // 检查 TodoItemTags 表是否存在
                    command.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name='TodoItemTags'";
                    var todoItemTagsTableExists = await command.ExecuteScalarAsync();
                    
                    if (todoItemTagsTableExists == null)
                    {
                        System.Diagnostics.Debug.WriteLine("缺少 TodoItemTags 表");
                        return true; // 需要迁移
                    }

                    System.Diagnostics.Debug.WriteLine("数据库架构检查通过，无需迁移");
                    return false; // 不需要迁移
                }
                finally
                {
                    await connection.CloseAsync();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"检查数据库架构时出错: {ex.Message}");
                return true; // 出错时触发迁移
            }
        }

        /// <summary>
        /// 迁移数据库 - 备份数据、删除旧库、创建新库、恢复数据
        /// </summary>
        private async Task MigrateDatabaseAsync()
        {
            System.Diagnostics.Debug.WriteLine("开始数据库迁移...");
            
            var backupPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "SceneTodo",
                $"todo_backup_{DateTime.Now:yyyyMMddHHmmss}.json");

            try
            {
                // 1. 备份现有数据
                var backupData = await BackupDataAsync();
                if (backupData != null && backupData.Count > 0)
                {
                    var jsonOptions = new JsonSerializerOptions { WriteIndented = true };
                    var json = JsonSerializer.Serialize(backupData, jsonOptions);
                    await File.WriteAllTextAsync(backupPath, json);
                    System.Diagnostics.Debug.WriteLine($"数据已备份到: {backupPath}，共 {backupData.Count} 条记录");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("数据库为空，无需备份");
                }

                // 2. 删除旧数据库
                await dbContext.Database.EnsureDeletedAsync();
                System.Diagnostics.Debug.WriteLine("旧数据库已删除");

                // 3. 创建新数据库
                await dbContext.Database.EnsureCreatedAsync();
                System.Diagnostics.Debug.WriteLine("新数据库已创建");

                // 4. 恢复数据
                if (backupData != null && backupData.Count > 0)
                {
                    await RestoreDataAsync(backupData);
                    System.Diagnostics.Debug.WriteLine($"数据已恢复，共 {backupData.Count} 条记录");
                }
                else
                {
                    // 如果没有旧数据，填充默认测试数据
                    await SeedTestDataAsync();
                    System.Diagnostics.Debug.WriteLine("已填充默认测试数据");
                }

                System.Diagnostics.Debug.WriteLine("数据库迁移完成");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"数据库迁移失败: {ex.Message}");
                
                // 如果迁移失败，尝试从备份文件恢复
                if (File.Exists(backupPath))
                {
                    System.Diagnostics.Debug.WriteLine($"备份文件已保存: {backupPath}");
                    System.Diagnostics.Debug.WriteLine("您可以手动从备份文件恢复数据");
                }
                
                throw;
            }
        }

        /// <summary>
        /// 备份数据库中的所有数据
        /// </summary>
        private async Task<List<TodoItemBackup>> BackupDataAsync()
        {
            var backupList = new List<TodoItemBackup>();
            
            try
            {
                // 使用原始SQL查询来绕过列名验证
                var connection = dbContext.Database.GetDbConnection();
                await connection.OpenAsync();

                using var command = connection.CreateCommand();
                command.CommandText = "SELECT * FROM TodoItems";
                
                using var reader = await command.ExecuteReaderAsync();
                
                while (await reader.ReadAsync())
                {
                    var backup = new TodoItemBackup
                    {
                        Id = GetStringValue(reader, "Id"),
                        Name = GetStringValue(reader, "Name"),
                        Description = GetStringValue(reader, "Description"),
                        Content = GetStringValue(reader, "Content"),
                        ParentId = GetStringValue(reader, "ParentId"),
                        IsCompleted = GetBoolValue(reader, "IsCompleted"),
                        IsExpanded = GetBoolValue(reader, "IsExpanded"),
                        AppPath = GetStringValue(reader, "AppPath"),
                        IsInjected = GetBoolValue(reader, "IsInjected"),
                        TodoItemType = GetIntValue(reader, "TodoItemType"),
                        GreadtedAt = GetDateTimeValue(reader, "GreadtedAt"),
                        UpdatedAt = GetDateTimeValue(reader, "UpdatedAt"),
                        CompletedAt = GetDateTimeValue(reader, "CompletedAt"),
                        StartTime = GetDateTimeValue(reader, "StartTime"),
                        ReminderTime = GetDateTimeValue(reader, "ReminderTime"),
                        EndTime = GetDateTimeValue(reader, "EndTime"),
                        DueDate = GetDateTimeValue(reader, "DueDate"),
                        Priority = GetIntValue(reader, "Priority", 1),
                        LinkedActionsJson = GetStringValue(reader, "LinkedActionsJson"),
                        TagsJson = GetStringValue(reader, "TagsJson"),
                        OverlayPosition = GetIntValue(reader, "OverlayPosition", 0),
                        OverlayOffsetX = GetDoubleValue(reader, "OverlayOffsetX", 0),
                        OverlayOffsetY = GetDoubleValue(reader, "OverlayOffsetY", 0)
                    };
                    
                    backupList.Add(backup);
                }

                await connection.CloseAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"备份数据时出错: {ex.Message}");
                // 返回空列表，稍后会填充默认数据
                return new List<TodoItemBackup>();
            }

            return backupList;
        }

        /// <summary>
        /// 从备份恢复数据
        /// </summary>
        private async Task RestoreDataAsync(List<TodoItemBackup> backupData)
        {
            foreach (var backup in backupData)
            {
                var todoItem = new TodoItem
                {
                    Id = backup.Id,
                    Name = backup.Name ?? string.Empty,
                    Description = backup.Description ?? string.Empty,
                    Content = backup.Content,
                    ParentId = backup.ParentId,
                    IsCompleted = backup.IsCompleted,
                    IsExpanded = backup.IsExpanded,
                    AppPath = backup.AppPath,
                    IsInjected = backup.IsInjected,
                    TodoItemType = (TodoItemType)backup.TodoItemType,
                    GreadtedAt = backup.GreadtedAt,
                    UpdatedAt = backup.UpdatedAt,
                    CompletedAt = backup.CompletedAt,
                    StartTime = backup.StartTime,
                    ReminderTime = backup.ReminderTime,
                    EndTime = backup.EndTime,
                    DueDate = backup.DueDate,
                    Priority = (Priority)backup.Priority,
                    LinkedActionsJson = backup.LinkedActionsJson ?? "[]",
                    TagsJson = backup.TagsJson ?? "[]",
                    OverlayPosition = (OverlayPosition)backup.OverlayPosition,
                    OverlayOffsetX = backup.OverlayOffsetX,
                    OverlayOffsetY = backup.OverlayOffsetY
                };

                dbContext.TodoItems.Add(todoItem);
            }

            await dbContext.SaveChangesAsync();
        }

        // 辅助方法：安全获取字符串值
        private string GetStringValue(System.Data.Common.DbDataReader reader, string columnName)
        {
            try
            {
                var ordinal = reader.GetOrdinal(columnName);
                return reader.IsDBNull(ordinal) ? string.Empty : reader.GetString(ordinal);
            }
            catch
            {
                return string.Empty;
            }
        }

        // 辅助方法：安全获取布尔值
        private bool GetBoolValue(System.Data.Common.DbDataReader reader, string columnName, bool defaultValue = false)
        {
            try
            {
                var ordinal = reader.GetOrdinal(columnName);
                return reader.IsDBNull(ordinal) ? defaultValue : Convert.ToBoolean(reader[ordinal]);
            }
            catch
            {
                return defaultValue;
            }
        }

        // 辅助方法：安全获取整数值
        private int GetIntValue(System.Data.Common.DbDataReader reader, string columnName, int defaultValue = 0)
        {
            try
            {
                var ordinal = reader.GetOrdinal(columnName);
                return reader.IsDBNull(ordinal) ? defaultValue : Convert.ToInt32(reader[ordinal]);
            }
            catch
            {
                return defaultValue;
            }
        }

        // 辅助方法：安全获取日期时间值
        private DateTime? GetDateTimeValue(System.Data.Common.DbDataReader reader, string columnName)
        {
            try
            {
                var ordinal = reader.GetOrdinal(columnName);
                if (reader.IsDBNull(ordinal))
                    return null;
                
                var value = reader[ordinal].ToString();
                return DateTime.Parse(value);
            }
            catch
            {
                return null;
            }
        }

        // 辅助方法：安全获取双精度值
        private double GetDoubleValue(System.Data.Common.DbDataReader reader, string columnName, double defaultValue = 0)
        {
            try
            {
                var ordinal = reader.GetOrdinal(columnName);
                return reader.IsDBNull(ordinal) ? defaultValue : Convert.ToDouble(reader[ordinal]);
            }
            catch
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// 重置数据库 - 清除所有数据并重新填充测试数据
        /// </summary>
        public async Task ResetDatabaseAsync()
        {
            try
            {
                // 清除所有待办事项数据
                await ClearAllDataAsync();

                // 重新填充测试数据
                await SeedTestDataAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"重置数据库时出错: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// 清除数据库中的所有数据
        /// </summary>
        private async Task ClearAllDataAsync()
        {
            // 删除所有待办事项
            var allItems = await dbContext.TodoItems.ToListAsync();
            dbContext.TodoItems.RemoveRange(allItems);

            // 如果后续添加了其他表，也在这里清除

            // 保存更改
            await dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// 初始化测试数据
        /// </summary>
        private async Task SeedTestDataAsync()
        { 
            // 记事本应用
            var notepadPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "notepad.exe");
            //获取notepad名称
            var notepadName = Path.GetFileName(notepadPath);
            // 添加待办事项
            // 普通待办事项
            var todoItem1 = new TodoItem
            {
                Id = Guid.NewGuid().ToString(),
                Content = "普通待办项1",
                ParentId = string.Empty,
                IsCompleted = false,
                Description = "普通待办事项示例1"
            };

            var todoItem1_1 = new TodoItem
            {
                Id = Guid.NewGuid().ToString(),
                Content = "普通待办项1_1",
                ParentId = todoItem1.Id,
                IsCompleted = false,
                Description = "普通待办事项示例1_1"
            };
            var todoItem1_2 = new TodoItem
            {
                Id = Guid.NewGuid().ToString(),
                Content = "普通待办项1_2",
                ParentId = todoItem1.Id,
                IsCompleted = false,
                Description = "普通待办事项示例1_2"
            };

            var todoItem2 = new TodoItem
            {
                Id = Guid.NewGuid().ToString(),
                Content = "普通待办项2",
                ParentId = string.Empty,
                IsCompleted = false,
                Description = "普通待办事项示例2"
            };
            
            // 记事本应用待办事项
            var todoItem3 = new TodoItem
            {
                Id = Guid.NewGuid().ToString(),
                Content = "记事本待办项1",
                ParentId = string.Empty,
                AppPath = notepadPath,
                Name = notepadName,
                TodoItemType = TodoItemType.App,
                IsCompleted = false,
                Description = "记事本应用待办事项示例1"
            };

            var todoItem3_1 = new TodoItem
            {
                Id = Guid.NewGuid().ToString(),
                Content = "记事本待办项1",
                ParentId = todoItem3.Id,
                AppPath = notepadPath,
                Name = notepadName,
                TodoItemType = TodoItemType.App,
                IsCompleted = false,
                Description = "记事本应用待办事项示例1"
            };
            
            dbContext.TodoItems.AddRange(todoItem1, todoItem1_1, todoItem1_2, todoItem2, todoItem3, todoItem3_1);
            
            // 保存所有更改
            await dbContext.SaveChangesAsync();
        }

        /// <summary>
        /// 数据备份模型
        /// </summary>
        private class TodoItemBackup
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public string Content { get; set; }
            public string ParentId { get; set; }
            public bool IsCompleted { get; set; }
            public bool IsExpanded { get; set; }
            public string AppPath { get; set; }
            public bool IsInjected { get; set; }
            public int TodoItemType { get; set; }
            public DateTime? GreadtedAt { get; set; }
            public DateTime? UpdatedAt { get; set; }
            public DateTime? CompletedAt { get; set; }
            public DateTime? StartTime { get; set; }
            public DateTime? ReminderTime { get; set; }
            public DateTime? EndTime { get; set; }
            public DateTime? DueDate { get; set; }
            public int Priority { get; set; }
            public string LinkedActionsJson { get; set; }
            public string TagsJson { get; set; }
            public int OverlayPosition { get; set; }
            public double OverlayOffsetX { get; set; }
            public double OverlayOffsetY { get; set; }
        }
    }
}
