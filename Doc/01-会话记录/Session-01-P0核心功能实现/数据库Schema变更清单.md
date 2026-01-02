# 数据库Schema变更清单

本文档记录P0功能实现所需的数据库结构变更。

---

## 1. TodoItem表需要添加的字段

### 1.1 优先级相关
```csharp
// 在 Models/TodoItem.cs 中添加
public Priority Priority { get; set; } = Priority.Medium;
```

**数据库迁移**：
```csharp
// 在 Services/Database/TodoDbContext.cs 的 OnModelCreating 方法中
modelBuilder.Entity<TodoItem>()
    .Property(t => t.Priority)
    .HasDefaultValue(Priority.Medium)
    .HasConversion<int>();  // 存储为整数
```

### 1.2 分类/标签相关
```csharp
// 简单实现：单分类
public string Category { get; set; } = string.Empty;

// 高级实现：多标签（JSON格式）
public string TagsJson { get; set; } = "[]";
```

**数据库迁移**：
```csharp
modelBuilder.Entity<TodoItem>()
    .Property(t => t.Category)
    .HasDefaultValue(string.Empty);

modelBuilder.Entity<TodoItem>()
    .Property(t => t.TagsJson)
    .HasDefaultValue("[]");
```

### 1.3 遮盖层位置相关
```csharp
public OverlayPosition OverlayPosition { get; set; } = OverlayPosition.Bottom;
public double OverlayOffsetX { get; set; } = 0;
public double OverlayOffsetY { get; set; } = 0;
```

**数据库迁移**：
```csharp
modelBuilder.Entity<TodoItem>()
    .Property(t => t.OverlayPosition)
    .HasDefaultValue(OverlayPosition.Bottom)
    .HasConversion<int>();

modelBuilder.Entity<TodoItem>()
    .Property(t => t.OverlayOffsetX)
    .HasDefaultValue(0.0);

modelBuilder.Entity<TodoItem>()
    .Property(t => t.OverlayOffsetY)
    .HasDefaultValue(0.0);
```

### 1.4 遮盖层折叠状态
```csharp
public bool IsOverlayCollapsed { get; set; } = false;
```

**数据库迁移**：
```csharp
modelBuilder.Entity<TodoItem>()
    .Property(t => t.IsOverlayCollapsed)
    .HasDefaultValue(false);
```

### 1.5 关联操作
```csharp
public string LinkedActionsJson { get; set; } = "[]";
```

**数据库迁移**：
```csharp
modelBuilder.Entity<TodoItem>()
    .Property(t => t.LinkedActionsJson)
    .HasDefaultValue("[]");
```

---

## 2. 完整的迁移代码

### 2.1 创建迁移类
**文件**: `Services/Database/Migrations/AddP0Fields.cs`

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace SceneTodo.Services.Database.Migrations
{
    [DbContext(typeof(TodoDbContext))]
    [Migration("20250103_AddP0Fields")]
    public class AddP0Fields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 添加优先级字段
            migrationBuilder.AddColumn<int>(
                name: "Priority",
                table: "TodoItems",
                type: "INTEGER",
                nullable: false,
                defaultValue: 1);  // Medium

            // 添加分类字段
            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "TodoItems",
                type: "TEXT",
                nullable: true,
                defaultValue: "");

            // 添加标签JSON字段
            migrationBuilder.AddColumn<string>(
                name: "TagsJson",
                table: "TodoItems",
                type: "TEXT",
                nullable: true,
                defaultValue: "[]");

            // 添加遮盖层位置字段
            migrationBuilder.AddColumn<int>(
                name: "OverlayPosition",
                table: "TodoItems",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);  // Bottom

            migrationBuilder.AddColumn<double>(
                name: "OverlayOffsetX",
                table: "TodoItems",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "OverlayOffsetY",
                table: "TodoItems",
                type: "REAL",
                nullable: false,
                defaultValue: 0.0);

            // 添加遮盖层折叠状态
            migrationBuilder.AddColumn<bool>(
                name: "IsOverlayCollapsed",
                table: "TodoItems",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            // 添加关联操作JSON字段
            migrationBuilder.AddColumn<string>(
                name: "LinkedActionsJson",
                table: "TodoItems",
                type: "TEXT",
                nullable: true,
                defaultValue: "[]");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "Priority", table: "TodoItems");
            migrationBuilder.DropColumn(name: "Category", table: "TodoItems");
            migrationBuilder.DropColumn(name: "TagsJson", table: "TodoItems");
            migrationBuilder.DropColumn(name: "OverlayPosition", table: "TodoItems");
            migrationBuilder.DropColumn(name: "OverlayOffsetX", table: "TodoItems");
            migrationBuilder.DropColumn(name: "OverlayOffsetY", table: "TodoItems");
            migrationBuilder.DropColumn(name: "IsOverlayCollapsed", table: "TodoItems");
            migrationBuilder.DropColumn(name: "LinkedActionsJson", table: "TodoItems");
        }
    }
}
```

### 2.2 简单的迁移方式（无EF Core Migrations）

如果不使用EF Core Migrations，可以在`DatabaseInitializer`中检查并添加字段：

**文件**: `Services/Database/DatabaseInitializer.cs`

```csharp
public async Task MigrateToP0Async()
{
    try
    {
        // 检查字段是否存在，如果不存在则添加
        await using var connection = _dbContext.Database.GetDbConnection();
        await connection.OpenAsync();
        
        var command = connection.CreateCommand();
        
        // 获取表信息
        command.CommandText = "PRAGMA table_info(TodoItems)";
        var reader = await command.ExecuteReaderAsync();
        
        var columns = new List<string>();
        while (await reader.ReadAsync())
        {
            columns.Add(reader.GetString(1));  // 列名在索引1
        }
        reader.Close();
        
        // 添加缺失的字段
        if (!columns.Contains("Priority"))
        {
            await ExecuteNonQueryAsync(connection, 
                "ALTER TABLE TodoItems ADD COLUMN Priority INTEGER NOT NULL DEFAULT 1");
        }
        
        if (!columns.Contains("Category"))
        {
            await ExecuteNonQueryAsync(connection, 
                "ALTER TABLE TodoItems ADD COLUMN Category TEXT DEFAULT ''");
        }
        
        if (!columns.Contains("TagsJson"))
        {
            await ExecuteNonQueryAsync(connection, 
                "ALTER TABLE TodoItems ADD COLUMN TagsJson TEXT DEFAULT '[]'");
        }
        
        if (!columns.Contains("OverlayPosition"))
        {
            await ExecuteNonQueryAsync(connection, 
                "ALTER TABLE TodoItems ADD COLUMN OverlayPosition INTEGER NOT NULL DEFAULT 0");
        }
        
        if (!columns.Contains("OverlayOffsetX"))
        {
            await ExecuteNonQueryAsync(connection, 
                "ALTER TABLE TodoItems ADD COLUMN OverlayOffsetX REAL NOT NULL DEFAULT 0.0");
        }
        
        if (!columns.Contains("OverlayOffsetY"))
        {
            await ExecuteNonQueryAsync(connection, 
                "ALTER TABLE TodoItems ADD COLUMN OverlayOffsetY REAL NOT NULL DEFAULT 0.0");
        }
        
        if (!columns.Contains("IsOverlayCollapsed"))
        {
            await ExecuteNonQueryAsync(connection, 
                "ALTER TABLE TodoItems ADD COLUMN IsOverlayCollapsed INTEGER NOT NULL DEFAULT 0");
        }
        
        if (!columns.Contains("LinkedActionsJson"))
        {
            await ExecuteNonQueryAsync(connection, 
                "ALTER TABLE TodoItems ADD COLUMN LinkedActionsJson TEXT DEFAULT '[]'");
        }
        
        Debug.WriteLine("数据库迁移到P0完成");
    }
    catch (Exception ex)
    {
        Debug.WriteLine($"数据库迁移失败: {ex.Message}");
        throw;
    }
}

private async Task ExecuteNonQueryAsync(DbConnection connection, string sql)
{
    var command = connection.CreateCommand();
    command.CommandText = sql;
    await command.ExecuteNonQueryAsync();
    Debug.WriteLine($"执行SQL: {sql}");
}
```

在`InitializeAsync`方法中调用：
```csharp
public async Task InitializeAsync()
{
    await _dbContext.Database.EnsureCreatedAsync();
    await MigrateToP0Async();  // 添加这一行
}
```

---

## 3. 新增表（可选）

### 3.1 用户表（如果实现账号系统）

**模型**: `Models/User.cs`
```csharp
public class User : BaseModel
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? LastLoginAt { get; set; }
}
```

**DbContext配置**:
```csharp
public DbSet<User> Users { get; set; }

protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    // ...existing code...
    
    modelBuilder.Entity<User>(entity =>
    {
        entity.HasKey(e => e.Id);
        entity.HasIndex(e => e.Email).IsUnique();
        entity.HasIndex(e => e.Username).IsUnique();
    });
}
```

### 3.2 备份记录表

**模型**: `Models/BackupRecord.cs`
```csharp
public class BackupRecord : BaseModel
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string BackupPath { get; set; } = string.Empty;
    public DateTime BackupTime { get; set; } = DateTime.Now;
    public BackupType BackupType { get; set; } = BackupType.Local;
    public long FileSizeBytes { get; set; }
}

public enum BackupType
{
    Local,
    Cloud
}
```

**DbContext配置**:
```csharp
public DbSet<BackupRecord> BackupRecords { get; set; }

protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    // ...existing code...
    
    modelBuilder.Entity<BackupRecord>(entity =>
    {
        entity.HasKey(e => e.Id);
        entity.HasIndex(e => e.BackupTime);
    });
}
```

---

## 4. 完整的更新后TodoDbContext

**文件**: `Services/Database/TodoDbContext.cs`

添加或确保存在以下配置：

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);
    
    modelBuilder.Entity<TodoItem>(entity =>
    {
        entity.HasKey(e => e.Id);
        entity.HasIndex(e => e.ParentId);
        entity.HasIndex(e => e.IsCompleted);
        entity.HasIndex(e => e.Priority);  // 新增
        entity.HasIndex(e => e.Category);  // 新增
        
        // 设置默认值
        entity.Property(e => e.Priority)
            .HasDefaultValue(Priority.Medium);
            
        entity.Property(e => e.Category)
            .HasDefaultValue(string.Empty);
            
        entity.Property(e => e.TagsJson)
            .HasDefaultValue("[]");
            
        entity.Property(e => e.OverlayPosition)
            .HasDefaultValue(OverlayPosition.Bottom);
            
        entity.Property(e => e.OverlayOffsetX)
            .HasDefaultValue(0.0);
            
        entity.Property(e => e.OverlayOffsetY)
            .HasDefaultValue(0.0);
            
        entity.Property(e => e.IsOverlayCollapsed)
            .HasDefaultValue(false);
            
        entity.Property(e => e.LinkedActionsJson)
            .HasDefaultValue("[]");
    });
    
    modelBuilder.Entity<AutoTask>(entity =>
    {
        entity.HasKey(e => e.Id);
        // ...existing configuration...
    });
    
    // 如果添加了新表
    // modelBuilder.Entity<User>(...);
    // modelBuilder.Entity<BackupRecord>(...);
}
```

---

## 5. 执行迁移步骤

### 方法1: 使用DatabaseInitializer（推荐）

1. 确保在`DatabaseInitializer`中添加了`MigrateToP0Async`方法
2. 在`InitializeAsync`中调用该方法
3. 运行应用程序，迁移会自动执行

### 方法2: 手动执行SQL

如果需要手动执行，可以使用SQLite工具：

```sql
-- 添加所有新字段
ALTER TABLE TodoItems ADD COLUMN Priority INTEGER NOT NULL DEFAULT 1;
ALTER TABLE TodoItems ADD COLUMN Category TEXT DEFAULT '';
ALTER TABLE TodoItems ADD COLUMN TagsJson TEXT DEFAULT '[]';
ALTER TABLE TodoItems ADD COLUMN OverlayPosition INTEGER NOT NULL DEFAULT 0;
ALTER TABLE TodoItems ADD COLUMN OverlayOffsetX REAL NOT NULL DEFAULT 0.0;
ALTER TABLE TodoItems ADD COLUMN OverlayOffsetY REAL NOT NULL DEFAULT 0.0;
ALTER TABLE TodoItems ADD COLUMN IsOverlayCollapsed INTEGER NOT NULL DEFAULT 0;
ALTER TABLE TodoItems ADD COLUMN LinkedActionsJson TEXT DEFAULT '[]';

-- 创建索引以提高查询性能
CREATE INDEX IF NOT EXISTS IX_TodoItems_Priority ON TodoItems(Priority);
CREATE INDEX IF NOT EXISTS IX_TodoItems_Category ON TodoItems(Category);
```

---

## 6. 数据库版本管理

建议在数据库中添加版本表来跟踪迁移状态：

```csharp
public class DbVersion
{
    public int Version { get; set; }
    public DateTime AppliedAt { get; set; }
    public string Description { get; set; }
}

// 在DbContext中
public DbSet<DbVersion> DbVersions { get; set; }

// 在DatabaseInitializer中
private async Task<int> GetCurrentVersionAsync()
{
    try
    {
        return await _dbContext.DbVersions
            .OrderByDescending(v => v.Version)
            .Select(v => v.Version)
            .FirstOrDefaultAsync();
    }
    catch
    {
        return 0;
    }
}

private async Task UpdateVersionAsync(int version, string description)
{
    _dbContext.DbVersions.Add(new DbVersion
    {
        Version = version,
        AppliedAt = DateTime.Now,
        Description = description
    });
    await _dbContext.SaveChangesAsync();
}
```

---

## 7. 验证迁移

迁移完成后，使用以下代码验证：

```csharp
public async Task VerifyMigrationAsync()
{
    try
    {
        // 尝试查询新字段
        var testItem = await _dbContext.TodoItems.FirstOrDefaultAsync();
        if (testItem != null)
        {
            _ = testItem.Priority;
            _ = testItem.Category;
            _ = testItem.TagsJson;
            _ = testItem.OverlayPosition;
            _ = testItem.OverlayOffsetX;
            _ = testItem.OverlayOffsetY;
            _ = testItem.IsOverlayCollapsed;
            _ = testItem.LinkedActionsJson;
            
            Debug.WriteLine("数据库迁移验证成功！");
        }
    }
    catch (Exception ex)
    {
        Debug.WriteLine($"数据库迁移验证失败: {ex.Message}");
        throw;
    }
}
```

---

## 8. 回滚计划

如果迁移出现问题，可以使用以下方法回滚：

1. 备份数据库文件（`todo.db`）
2. 如果使用了Migration类，执行`Down`方法
3. 如果手动修改，执行DROP COLUMN（注意：SQLite不直接支持DROP COLUMN，需要重建表）

**SQLite回滚示例**：
```sql
-- 创建新表（不包含新字段）
CREATE TABLE TodoItems_Backup AS SELECT 
    Id, Name, Description, ParentId, Content, IsCompleted, IsExpanded,
    AppPath, IsInjected, TodoItemType, GreadtedAt, UpdatedAt, CompletedAt,
    StartTime, ReminderTime, EndTime
FROM TodoItems;

-- 删除原表
DROP TABLE TodoItems;

-- 重命名备份表
ALTER TABLE TodoItems_Backup RENAME TO TodoItems;
```

---

**重要提示**：
1. 在执行迁移前一定要备份数据库
2. 在测试环境中先验证迁移
3. 记录每次迁移的版本和时间
4. 保持迁移代码的向后兼容性
