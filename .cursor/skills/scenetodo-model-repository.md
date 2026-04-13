# SceneTodo - Model & Repository Operations Skill

> **Purpose**: 专门处理 SceneTodo 项目的数据模型和 Repository 层操作
> **Context**: MVVM 模式 + Repository 模式 + EF Core + SQLite
> **适用场景**: 数据库模型修改、Repository 操作、数据持久化

---

## 🏗️ 项目架构概览

### 分层架构
```
View (XAML) <--数据绑定--> ViewModel (C#) <--调用--> Repository <--操作--> Model (数据库)
```

- **View**: XAML 布局和展示
- **ViewModel**: 业务逻辑和数据处理
- **Repository**: 数据库访问层
- **Model**: 数据库实体模型

### 核心组件
- **TodoDbContext**: EF Core 数据库上下文 (`Services/Database/TodoDbContext.cs`)
- **Repository 模式**: 所有数据库操作
- **Service Locator**: `App.xaml.cs` 中的静态单例

---

## 📦 Models 目录结构

```
Models/
├── BaseModel.cs                    # 基类（实现 INotifyPropertyChanged）
├── TodoItem.cs                     # 待办事项实体（数据库模型）
├── TodoItemModel.cs                # 待办事项业务模型
├── TodoItemTag.cs                  # 待办-标签关联表
├── Tag.cs                         # 标签实体
├── LinkedAction.cs                 # 关联操作
├── TodoItemHistory.cs              # 历史记录
├── AutoTask.cs                    # 自动任务
├── BackupInfo.cs                   # 备份信息
├── BackupSettings.cs               # 备份设置
├── RestoreMode.cs                  # 恢复模式枚举
├── CompletionStatus.cs             # 完成状态枚举
├── DateTimeFilter.cs               # 日期时间过滤器
├── DateTimeFilterType.cs           # 日期时间过滤类型
├── SearchFilter.cs                 # 搜索过滤器
├── SearchHistoryItem.cs            # 搜索历史项
├── SearchResult.cs                # 搜索结果
├── AppSettings.cs                  # 应用设置
├── AppearanceSettings.cs          # 外观设置
├── BehaviorSettings.cs            # 行为设置
├── ShortcutSettings.cs            # 快捷键设置
├── LanguageSettings.cs            # 语言设置
├── SupportedLanguage.cs           # 支持的语言枚举
└── ...
```

---

## 🔧 Models 核心规范

### BaseModel 基类

所有模型都应继承 `BaseModel` 以实现 `INotifyPropertyChanged`：

```csharp
public abstract class BaseModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return false;

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}
```

### 模型属性规范

使用私有后备字段 + 属性更改通知：

```csharp
private string _name = string.Empty;

public string Name
{
    get => _name;
    set
    {
        if (_name != value)
        {
            _name = value;
            UpdatedAt = DateTime.Now;
            OnPropertyChanged(nameof(Name));
        }
    }
}
```

### 可空类型规范

启用可空引用类型：
- 可空类型使用 `?`
- 非空断言使用 `!`
- 非空字段必须有默认值

```csharp
public string? OptionalField { get; set; }  // 可空
public string RequiredField { get; set; } = string.Empty;  // 非空
```

---

## 🗄️ Repository 层规范

### Repository 目录结构

```
Services/Database/
├── TodoDbContext.cs              # EF Core 数据库上下文
├── DbContextFactory.cs           # 数据库上下文工厂
└── Repositories/
    ├── ITodoItemRepository.cs    # 接口
    ├── TodoItemRepository.cs     # 实现
    ├── ITagRepository.cs
    ├── TagRepository.cs
    ├── IAutoTaskRepository.cs
    ├── AutoTaskRepository.cs
    ├── IBackupService.cs         # 备份服务
    └── BackupService.cs
```

### Repository 命名规范

- 接口: `I{ModelName}Repository` (如 `ITodoItemRepository`)
- 实现: `{ModelName}Repository` (如 `TodoItemRepository`)
- 方法: `{Action}{ModelName}Async` (如 `GetTodoItemByIdAsync`)

### Repository 核心方法

```csharp
public interface ITodoItemRepository
{
    // CRUD 操作
    Task<IEnumerable<TodoItem>> GetAllAsync();
    Task<TodoItem?> GetByIdAsync(string id);
    Task<IEnumerable<TodoItem>> GetByParentIdAsync(string? parentId);
    Task<IEnumerable<TodoItem>> SearchAsync(string query);

    // 创建和更新
    Task<TodoItem> AddAsync(TodoItem todoItem);
    Task<bool> UpdateAsync(TodoItem todoItem);
    Task<bool> DeleteAsync(string id);

    // 批量操作
    Task<bool> BulkDeleteAsync(IEnumerable<string> ids);
    Task<int> CountAsync();

    // 查询
    Task<IEnumerable<TodoItem>> GetByTagIdAsync(string tagId);
    Task<IEnumerable<TodoItem>> GetByStatusAsync(bool isCompleted);
}
```

### Async/Await 规范

**强制要求**：
- 所有数据库操作必须是异步的
- 异步方法必须以 `Async` 结尾
- 返回类型必须是 `Task<T>` 或 `Task`

```csharp
// ✅ 正确
public async Task<TodoItem?> GetByIdAsync(string id)
{
    return await dbContext.TodoItems
        .FirstOrDefaultAsync(t => t.Id == id);
}

// ❌ 错误 - 同步方法
public TodoItem? GetById(string id)
{
    return dbContext.TodoItems
        .FirstOrDefault(t => t.Id == id);
}
```

### ConfigureAwait 规范

在非 UI 代码中使用 `ConfigureAwait(false)`（repositories、services）：

```csharp
public async Task<IEnumerable<TodoItem>> GetAllAsync()
{
    return await dbContext.TodoItems
        .ToListAsync()
        .ConfigureAwait(false);
}
```

在 ViewModel 中不使用 `ConfigureAwait(false)`，确保在 UI 线程执行。

---

## 🔗 EF Core 数据库配置

### TodoDbContext 配置

```csharp
public class TodoDbContext : DbContext
{
    public DbSet<TodoItem> TodoItems { get; set; }
    public DbSet<Tag> Tags { get; set; }
    public DbSet<TodoItemTag> TodoItemTags { get; set; }
    public DbSet<TodoItemHistory> TodoItemHistories { get; set; }
    public DbSet<AutoTask> AutoTasks { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var dbPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "SceneTodo",
            "todo.db");

        optionsBuilder.UseSqlite($"Data Source={dbPath}");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // 配置关系
        modelBuilder.Entity<TodoItem>()
            .HasMany(t => t.Children)
            .WithOne(t => t.Parent)
            .HasForeignKey(t => t.ParentId)
            .OnDelete(DeleteBehavior.Cascade);

        // 配置索引
        modelBuilder.Entity<TodoItem>()
            .HasIndex(t => t.ParentId);

        // 配置 JSON 字段
        modelBuilder.Entity<TodoItem>()
            .Property(t => t.LinkedActionsJson)
            .IsRequired();
    }
}
```

### JSON 字段存储

对于复杂数据结构，使用 JSON 字符串存储：

```csharp
public class TodoItem : BaseModel
{
    // JSON 存储关联操作
    public string LinkedActionsJson { get; set; } = "[]";

    // 不映射到数据库的属性
    [NotMapped]
    public List<LinkedAction> LinkedActions
    {
        get => JsonConvert.DeserializeObject<List<LinkedAction>>(LinkedActionsJson) ?? new();
        set => LinkedActionsJson = JsonConvert.SerializeObject(value);
    }
}
```

---

## 🎯 Service Locator 模式

### App.xaml.cs 中的静态单例

```csharp
public partial class App : Application
{
    // 数据库上下文
    public static TodoDbContext DbContext { get; private set; } = null!;

    // Repositories
    public static ITodoItemRepository TodoItemRepository { get; private set; } = null!;
    public static ITagRepository TagRepository { get; private set; } = null!;
    public static IAutoTaskRepository AutoTaskRepository { get; private set; } = null!;

    // Services
    public static BackupService BackupService { get; private set; } = null!;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // 初始化数据库上下文
        var dbContextFactory = new DbContextFactory();
        DbContext = dbContextFactory.CreateDbContext();

        // 确保数据库已创建
        DbContext.Database.EnsureCreated();

        // 初始化 Repositories
        TodoItemRepository = new TodoItemRepository(DbContext);
        TagRepository = new TagRepository(DbContext);
        AutoTaskRepository = new AutoTaskRepository(DbContext);

        // 初始化 Services
        BackupService = new BackupService(DbContext, TodoItemRepository);
    }

    protected override void OnExit(ExitEventArgs e)
    {
        DbContext?.Dispose();
        base.OnExit(e);
    }
}
```

### 在代码中使用

```csharp
// 在 ViewModel 或其他类中
using SceneTodo;

// 直接访问静态属性
var todoItems = await App.TodoItemRepository.GetAllAsync();

// 添加待办事项
var newItem = new TodoItem
{
    Title = "New Todo",
    Description = "Description"
};
await App.TodoItemRepository.AddAsync(newItem);
```

---

## 🔍 常见 Repository 操作示例

### 查询所有待办事项

```csharp
public async Task<IEnumerable<TodoItem>> GetAllAsync()
{
    return await dbContext.TodoItems
        .OrderBy(t => t.CreatedAt)
        .ToListAsync()
        .ConfigureAwait(false);
}
```

### 按父 ID 查询子项

```csharp
public async Task<IEnumerable<TodoItem>> GetByParentIdAsync(string? parentId)
{
    return await dbContext.TodoItems
        .Where(t => t.ParentId == parentId)
        .OrderBy(t => t.CreatedAt)
        .ToListAsync()
        .ConfigureAwait(false);
}
```

### 搜索待办事项

```csharp
public async Task<IEnumerable<TodoItem>> SearchAsync(string query)
{
    return await dbContext.TodoItems
        .Where(t => t.Title.Contains(query) ||
                    t.Description.Contains(query))
        .ToListAsync()
        .ConfigureAwait(false);
}
```

### 添加待办事项

```csharp
public async Task<TodoItem> AddAsync(TodoItem todoItem)
{
    todoItem.CreatedAt = DateTime.Now;
    todoItem.UpdatedAt = DateTime.Now;

    dbContext.TodoItems.Add(todoItem);
    await dbContext.SaveChangesAsync().ConfigureAwait(false);

    return todoItem;
}
```

### 更新待办事项

```csharp
public async Task<bool> UpdateAsync(TodoItem todoItem)
{
    todoItem.UpdatedAt = DateTime.Now;

    dbContext.TodoItems.Update(todoItem);
    var result = await dbContext.SaveChangesAsync().ConfigureAwait(false);

    return result > 0;
}
```

### 删除待办事项

```csharp
public async Task<bool> DeleteAsync(string id)
{
    var item = await GetByIdAsync(id);
    if (item == null)
        return false;

    dbContext.TodoItems.Remove(item);
    var result = await dbContext.SaveChangesAsync().ConfigureAwait(false);

    return result > 0;
}
```

### 批量删除

```csharp
public async Task<bool> BulkDeleteAsync(IEnumerable<string> ids)
{
    var items = await dbContext.TodoItems
        .Where(t => ids.Contains(t.Id))
        .ToListAsync()
        .ConfigureAwait(false);

    dbContext.TodoItems.RemoveRange(items);
    var result = await dbContext.SaveChangesAsync().ConfigureAwait(false);

    return result > 0;
}
```

---

## 🗂️ 数据库迁移

### 添加新模型属性

1. 在 Model 类中添加属性
2. 在 `TodoDbContext.OnModelCreating()` 中配置
3. 删除数据库（开发环境）或创建迁移

```bash
# 开发环境：删除数据库重新创建
rm $env:LOCALAPPDATA\SceneTodo\todo.db
```

### 数据库位置

```
%LocalAppData%\SceneTodo\todo.db
```

例如：
```
C:\Users\YourName\AppData\Local\SceneTodo\todo.db
```

---

## ⚠️ 重要注意事项

### Service Locator 模式

- **无 DI 容器**: `App.xaml.cs` 中的手动 Service Locator
- **通过静态属性访问**: 不要使用构造函数注入
- **单例模式**: 所有服务和 Repository 都是单例

### 异步操作

- **强制异步**: 所有数据库操作必须是异步的
- **方法命名**: 异步方法必须以 `Async` 结尾
- **ConfigureAwait**: 非UI 代码中使用 `ConfigureAwait(false)`

### 线程安全

- **UI 线程操作**: 使用 `Dispatcher.Invoke` 或 `DispatcherTimer`
- **非 UI 代码**: Repository 和 Service 层使用 `ConfigureAwait(false)`

---

## 📚 相关文档

### 必读文档
- [AGENTS.md](../../AGENTS.md) - 代码风格指南
- [项目状态总览](../Doc/00-必读/项目状态总览.md) - 项目状态
- [快速上手指南](../Doc/00-必读/快速上手指南.md) - 快速指南

### 技术文档
- [Overlay Position Selection Design](../Doc/02-技术文档/Overlay Position Selection Design.md)
- [PRD vs Implementation Comparison Analysis](../Doc/02-技术文档/PRD vs Implementation Comparison Analysis.md)

---

**注意**: 本文档基于 SceneTodo 项目的数据访问层定制编写，请遵循项目的 MVVM 和 Repository 模式规范。
