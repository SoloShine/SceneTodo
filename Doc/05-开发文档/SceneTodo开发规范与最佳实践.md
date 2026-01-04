# SceneTodo 开发规范与最佳实践

> **创建日期**: 2025-01-02  
> **最后更新**: 2025-01-02  
> **版本**: v1.0  
> **状态**: 正式发布  
> **适用范围**: 所有 SceneTodo 项目开发者

---

## ?? 目录

1. [代码组织规范](#代码组织规范)
2. [分部类使用规范](#分部类使用规范) ? 新增
3. [命名规范](#命名规范)
4. [注释规范](#注释规范)
5. [MVVM 架构规范](#mvvm-架构规范)
6. [数据库规范](#数据库规范)
7. [异步编程规范](#异步编程规范)
8. [XAML 规范](#xaml-规范)
9. [Git 提交规范](#git-提交规范)
10. [测试规范](#测试规范)

---

## ?? 代码组织规范

### 目录结构

```
SceneTodo/
├── Models/              # 数据模型
├── ViewModels/          # 视图模型
├── Views/               # 视图/窗口
├── Services/            # 服务层
│   ├── Database/        # 数据库服务
│   └── Scheduler/       # 调度服务
├── Utils/               # 工具类
├── Converters/          # 值转换器
└── Resources/           # 资源文件
```

### 文件组织原则

1. **单一职责**: 每个文件只负责一个功能模块
2. **高内聚**: 相关功能放在同一目录
3. **低耦合**: 减少模块间依赖
4. **清晰命名**: 文件名应说明其功能

---

## ? 分部类使用规范

### 何时使用分部类

? **应该使用的场景**:

1. **单个类超过 500 行**
   ```csharp
   // 原文件太大
   public class MainWindowViewModel { } // 1300行 ?
   
   // 拆分为分部类
   public partial class MainWindowViewModel { } // 多个文件，每个<400行 ?
   ```

2. **类有多个清晰的功能模块**
   ```csharp
   // 功能混在一起
   public class MainWindowViewModel
   {
       // 导航功能
       // 待办管理功能
       // 悬浮窗功能
       // 标签筛选功能
   } // ?
   
   // 按功能拆分
   public partial class MainWindowViewModel { } // Core
   public partial class MainWindowViewModel { } // Navigation
   public partial class MainWindowViewModel { } // TodoManagement
   // ?
   ```

3. **自动生成的代码**
   ```csharp
   // Designer 生成的代码
   public partial class MainWindow { } // MainWindow.xaml.cs
   public partial class MainWindow { } // MainWindow.g.cs (自动生成)
   ```

? **不应该使用的场景**:

1. **小型类（<300行）** - 没必要拆分
2. **功能单一的类** - 拆分反而增加复杂度
3. **逻辑紧密耦合的类** - 难以拆分

### 分部类命名规范

**格式**: `<ClassName>.<Feature>.cs`

**示例**:
```
ViewModels/
├── MainWindowViewModel.Core.cs              # 核心定义
├── MainWindowViewModel.Navigation.cs        # 导航功能
├── MainWindowViewModel.TodoManagement.cs    # 待办管理
├── MainWindowViewModel.OverlayManagement.cs # 悬浮窗管理
├── MainWindowViewModel.DueDateReminders.cs  # 截止提醒
└── MainWindowViewModel.TagFilter.cs         # 标签筛选
```

**命名要求**:
- Feature 部分使用 PascalCase
- Feature 名称应清晰说明功能
- 避免使用 Part1, Part2 等无意义命名

### 分部类代码组织

#### 1. Core 文件结构

**职责**: 类定义、字段、属性、命令、构造函数

```csharp
// MainWindowViewModel.Core.cs
using System;
// ... 其他 using

namespace SceneTodo.ViewModels
{
    /// <summary>
    /// 主窗口 ViewModel - 核心部分
    /// 包含：属性、字段、构造函数、基础方法
    /// </summary>
    public partial class MainWindowViewModel : INotifyPropertyChanged
    {
        #region 字段
        
        private readonly DispatcherTimer timer;
        private MainWindowModel model;
        
        #endregion

        #region 属性
        
        public MainWindowModel Model
        {
            get => model;
            set
            {
                model = value;
                OnPropertyChanged(nameof(Model));
            }
        }
        
        #endregion

        #region Commands
        
        public ICommand SaveCommand { get; }
        public ICommand LoadCommand { get; }
        
        #endregion

        #region 构造函数
        
        public MainWindowViewModel()
        {
            SaveCommand = new RelayCommand(Save);
            LoadCommand = new RelayCommand(Load);
        }
        
        #endregion

        #region 基础方法
        
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        
        public void Cleanup()
        {
            // 清理逻辑
        }
        
        #endregion
    }
}
```

#### 2. 功能文件结构

**职责**: 特定功能的方法实现

```csharp
// MainWindowViewModel.TodoManagement.cs
using System;
// ... 其他 using

namespace SceneTodo.ViewModels
{
    /// <summary>
    /// 主窗口 ViewModel - 待办项管理
    /// 包含：待办项的增删改查等操作
    /// </summary>
    public partial class MainWindowViewModel
    {
        /// <summary>
        /// 添加待办项
        /// </summary>
        private void AddTodoItem(object? parameter)
        {
            // 实现逻辑
        }

        /// <summary>
        /// 编辑待办项
        /// </summary>
        private void EditTodoItem(object? parameter)
        {
            // 实现逻辑
        }

        /// <summary>
        /// 删除待办项
        /// </summary>
        private void DeleteTodoItem(object? parameter)
        {
            // 实现逻辑
        }
    }
}
```

### 分部类最佳实践

#### ? DO - 推荐做法

1. **字段和属性集中定义**
   ```csharp
   // ? 在 Core.cs 中定义所有字段
   public partial class MainWindowViewModel
   {
       private readonly Timer timer;
       private MainWindowModel model;
   }
   ```

2. **命令集中定义和初始化**
   ```csharp
   // ? 在 Core.cs 中定义命令
   public ICommand SaveCommand { get; }
   
   // ? 在构造函数中初始化
   SaveCommand = new RelayCommand(Save);
   
   // ? 在功能文件中实现
   private void Save(object? parameter) { }
   ```

3. **相关方法放在一起**
   ```csharp
   // ? TodoManagement.cs 中所有待办相关方法
   private void AddTodoItem() { }
   private void EditTodoItem() { }
   private void DeleteTodoItem() { }
   private bool FindTodoItemById() { }
   ```

4. **每个文件添加清晰的注释**
   ```csharp
   /// <summary>
   /// 主窗口 ViewModel - 待办项管理
   /// 包含：待办项的增删改查等操作
   /// </summary>
   public partial class MainWindowViewModel
   ```

5. **控制文件大小**
   ```csharp
   // ? 单个文件不超过 400 行
   // ? 如果超过，考虑进一步拆分功能
   ```

#### ? DON'T - 避免做法

1. **不要在多个文件中定义字段**
   ```csharp
   // ? Core.cs
   private Timer timer1;
   
   // ? Navigation.cs
   private Timer timer2; // 不要这样！
   ```

2. **不要随意拆分**
   ```csharp
   // ? 不要按字母顺序拆分
   MainWindowViewModel.A-M.cs
   MainWindowViewModel.N-Z.cs
   
   // ? 按功能拆分
   MainWindowViewModel.TodoManagement.cs
   MainWindowViewModel.Navigation.cs
   ```

3. **不要过度拆分**
   ```csharp
   // ? 太多小文件
   MainWindowViewModel.AddTodo.cs      // 10行
   MainWindowViewModel.EditTodo.cs     // 10行
   MainWindowViewModel.DeleteTodo.cs   // 10行
   
   // ? 合并为功能模块
   MainWindowViewModel.TodoManagement.cs // 100行
   ```

4. **不要循环依赖**
   ```csharp
   // ? Navigation.cs 调用 TodoManagement 的私有方法
   // ? TodoManagement.cs 调用 Navigation 的私有方法
   
   // ? 使用公共方法或共享逻辑
   ```

### 分部类重构步骤

当一个类需要拆分为分部类时，按以下步骤进行：

#### Step 1: 分析功能模块

```
原文件: MainWindowViewModel.cs (1300行)
分析功能:
- 核心定义（字段、属性、命令、构造） - 150行
- 导航管理 - 70行
- 待办项管理 - 250行
- 悬浮窗管理 - 300行
- 截止时间提醒 - 80行
- 标签筛选 - 100行
```

#### Step 2: 创建 Core 文件

```csharp
// 1. 创建 MainWindowViewModel.Core.cs
// 2. 复制类定义、using、字段、属性
// 3. 复制命令定义和构造函数
// 4. 保留基础方法
```

#### Step 3: 创建功能文件

```csharp
// 为每个功能模块创建文件
MainWindowViewModel.Navigation.cs
MainWindowViewModel.TodoManagement.cs
// ...

// 复制对应的方法实现
```

#### Step 4: 删除原文件

```csharp
// 确保所有代码都已迁移后，删除原始文件
```

#### Step 5: 编译测试

```bash
# 编译检查
dotnet build

# 功能测试
# 确保所有功能正常
```

### 分部类维护指南

#### 添加新功能

```csharp
// 1. 在 Core.cs 中添加命令定义
public ICommand NewFeatureCommand { get; }

// 2. 在构造函数中初始化
NewFeatureCommand = new RelayCommand(ExecuteNewFeature);

// 3. 创建新的功能文件（可选）
// MainWindowViewModel.NewFeature.cs
public partial class MainWindowViewModel
{
    private void ExecuteNewFeature(object? parameter)
    {
        // 新功能实现
    }
}

// 或者添加到现有功能文件中
```

#### 修改现有功能

```csharp
// 1. 找到对应的功能文件
// 例如: MainWindowViewModel.TodoManagement.cs

// 2. 修改对应的方法
private void EditTodoItem(object? parameter)
{
    // 修改实现
}

// 3. 如果需要新增字段，在 Core.cs 中添加
```

---

## ?? 命名规范

### 类命名

```csharp
// ? PascalCase，清晰描述用途
public class TodoItemModel { }
public class TodoItemRepository { }
public class BackupService { }

// ? 避免
public class todo { }          // 小写
public class Item1 { }         // 无意义数字
public class MyClass { }       // 太泛化
```

### 方法命名

```csharp
// ? PascalCase，动词开头
public void SaveData() { }
public async Task LoadDataAsync() { }
private void OnPropertyChanged() { }

// ? 避免
public void data() { }         // 小写
public void DoIt() { }         // 不清晰
```

### 变量命名

```csharp
// ? camelCase
private string userName;
private int itemCount;
private DateTime createdAt;

// ? 私有字段使用下划线前缀
private readonly IService _service;
private Timer _timer;

// ? 避免
private string UserName;       // PascalCase（公共字段用）
private int n;                 // 单字母
```

### 常量命名

```csharp
// ? PascalCase 或 UPPER_CASE
public const string DefaultPath = "C:\\Data";
public const int MAX_RETRY_COUNT = 3;

// ? 避免
public const string path = "C:\\Data";
```

### XAML 命名

```xml
<!-- ? x:Name 使用 PascalCase -->
<Button x:Name="SaveButton" />
<TextBox x:Name="UserNameTextBox" />

<!-- ? 避免 -->
<Button x:Name="button1" />
<TextBox x:Name="txtUserName" />
```

---

## ?? 注释规范

### XML 文档注释

```csharp
/// <summary>
/// 备份服务，提供数据备份和恢复功能
/// </summary>
public class BackupService
{
    /// <summary>
    /// 创建备份
    /// </summary>
    /// <param name="type">备份类型</param>
    /// <param name="progress">进度报告器</param>
    /// <returns>备份文件路径</returns>
    /// <exception cref="FileNotFoundException">数据库文件不存在</exception>
    public async Task<string> CreateBackupAsync(
        BackupType type, 
        IProgress<int>? progress = null)
    {
        // 实现
    }
}
```

### 代码注释

```csharp
// ? 解释为什么这样做
// 关闭数据库连接以确保文件可以被复制
await dbContext.Database.CloseConnectionAsync();

// ? 说明复杂逻辑
// 使用 DPI 转换矩阵将屏幕坐标转换为 WPF 坐标
Matrix transformMatrix = source.CompositionTarget.TransformFromDevice;

// ? 避免无用注释
// 设置值
value = 10;

// ? 避免注释掉的代码（使用版本控制）
// OldMethod();
```

### 区域注释

```csharp
#region 字段

private readonly Timer _timer;
private string _userName;

#endregion

#region 属性

public string UserName
{
    get => _userName;
    set => _userName = value;
}

#endregion

#region 公共方法

public void Save() { }
public void Load() { }

#endregion

#region 私有方法

private void ValidateData() { }

#endregion
```

---

## ??? MVVM 架构规范

### 职责划分

```
View (XAML)
    ↓ 数据绑定
ViewModel
    ↓ 业务逻辑
Model
    ↓ 数据访问
Repository/Service
    ↓
Database
```

### ViewModel 规范

```csharp
public class TodoItemViewModel : INotifyPropertyChanged
{
    private TodoItemModel model;
    
    // ? 属性实现 INotifyPropertyChanged
    public string Content
    {
        get => model.Content;
        set
        {
            model.Content = value;
            OnPropertyChanged(nameof(Content));
        }
    }
    
    // ? 命令使用 ICommand
    public ICommand SaveCommand { get; }
    
    public TodoItemViewModel()
    {
        SaveCommand = new RelayCommand(Save);
    }
    
    private void Save(object? parameter)
    {
        // 保存逻辑
    }
    
    public event PropertyChangedEventHandler? PropertyChanged;
    
    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
```

### View 规范

```xml
<Window x:Class="SceneTodo.Views.MainWindow"
        DataContext="{StaticResource MainViewModel}">
    
    <!-- ? 使用数据绑定 -->
    <TextBox Text="{Binding Content, UpdateSourceTrigger=PropertyChanged}" />
    
    <!-- ? 使用命令绑定 -->
    <Button Content="Save" Command="{Binding SaveCommand}" />
    
    <!-- ? 避免在代码后台写业务逻辑 -->
</Window>
```

---

## ??? 数据库规范

### 实体定义

```csharp
// ? 清晰的主键
public class TodoItem
{
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    // ? 必需字段使用 required
    [Required]
    public string Content { get; set; } = string.Empty;
    
    // ? 可空字段使用 ?
    public DateTime? DueDate { get; set; }
    
    // ? 导航属性
    public virtual ICollection<Tag> Tags { get; set; }
}
```

### DbContext 规范

```csharp
public class TodoDbContext : DbContext
{
    public DbSet<TodoItem> TodoItems { get; set; }
    public DbSet<Tag> Tags { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // ? 配置实体关系
        modelBuilder.Entity<TodoItemTag>()
            .HasKey(tt => new { tt.TodoItemId, tt.TagId });
            
        // ? 配置索引
        modelBuilder.Entity<TodoItem>()
            .HasIndex(t => t.CreatedAt);
    }
}
```

### Repository 模式

```csharp
public class TodoItemRepository
{
    private readonly TodoDbContext _context;
    
    public TodoItemRepository(TodoDbContext context)
    {
        _context = context;
    }
    
    // ? 异步方法
    public async Task<TodoItem?> GetByIdAsync(string id)
    {
        return await _context.TodoItems
            .Include(t => t.Tags)
            .FirstOrDefaultAsync(t => t.Id == id);
    }
    
    // ? 批量操作
    public async Task<List<TodoItem>> GetAllAsync()
    {
        return await _context.TodoItems
            .Include(t => t.Tags)
            .ToListAsync();
    }
}
```

---

## ? 异步编程规范

### 方法命名

```csharp
// ? 异步方法以 Async 结尾
public async Task<string> CreateBackupAsync() { }
public async Task LoadDataAsync() { }

// ? 避免
public async Task<string> CreateBackup() { }  // 缺少 Async
public Task<string> CreateBackupAsync() { }   // 没有 async 关键字
```

### 异步等待

```csharp
// ? 使用 await
public async Task<string> SaveDataAsync()
{
    await _repository.AddAsync(item);
    return "Success";
}

// ? 避免
public async Task<string> SaveDataAsync()
{
    _repository.AddAsync(item).Wait();  // 同步等待
    return "Success";
}
```

### ConfigureAwait

```csharp
// ? 在库代码中使用 ConfigureAwait(false)
public async Task SaveAsync()
{
    await _dbContext.SaveChangesAsync()
        .ConfigureAwait(false);
}

// ? 在 UI 代码中不使用 ConfigureAwait
public async Task UpdateUIAsync()
{
    var data = await LoadDataAsync();
    // 需要在 UI 线程上更新
    TextBlock.Text = data;
}
```

---

## ?? XAML 规范

### 属性顺序

```xml
<!-- ? 推荐顺序：
1. x:Name
2. x:Key
3. x:Class
4. 布局属性 (Width, Height, Margin, Padding)
5. 数据绑定
6. 事件处理
7. 其他属性
-->
<Button x:Name="SaveButton"
        Width="100"
        Height="30"
        Margin="10"
        Content="{Binding SaveText}"
        Command="{Binding SaveCommand}"
        Style="{StaticResource ButtonPrimary}" />
```

### 资源定义

```xml
<!-- ? 在 App.xaml 或 ResourceDictionary 中定义全局资源 -->
<Application.Resources>
    <Style x:Key="ButtonPrimary" TargetType="Button">
        <Setter Property="Background" Value="#2196F3" />
        <Setter Property="Foreground" Value="White" />
    </Style>
</Application.Resources>

<!-- ? 在窗口中定义局部资源 -->
<Window.Resources>
    <local:FileSizeConverter x:Key="FileSizeConverter" />
</Window.Resources>
```

---

## ?? Git 提交规范

### 提交消息格式

```
<type>(<scope>): <subject>

<body>

<footer>
```

### Type 类型

- `feat`: 新功能
- `fix`: Bug修复
- `docs`: 文档更新
- `style`: 代码格式（不影响功能）
- `refactor`: 重构（不是新功能也不是修复）
- `perf`: 性能优化
- `test`: 测试相关
- `chore`: 构建/工具相关

### 示例

```
feat(backup): 添加数据备份恢复功能

- 实现 BackupService 核心服务
- 支持手动和自动备份
- 添加备份列表管理
- 实现快照和回滚机制

Closes #12
```

```
refactor(viewmodel): 将 MainWindowViewModel 拆分为分部类

- 创建 6 个分部类文件
- 按功能模块组织代码
- 提升代码可维护性

Related to #15
```

---

## ? 测试规范

### 单元测试命名

```csharp
// ? 格式：MethodName_Scenario_ExpectedBehavior
[Test]
public void CreateBackup_WithValidData_ReturnsFilePath() { }

[Test]
public void RestoreBackup_FileNotFound_ThrowsException() { }
```

### 测试结构

```csharp
[Test]
public void AddTodoItem_ValidItem_AddsToCollection()
{
    // Arrange（准备）
    var viewModel = new MainWindowViewModel();
    var item = new TodoItemModel { Content = "Test" };
    
    // Act（执行）
    viewModel.AddTodoItemCommand.Execute(item);
    
    // Assert（断言）
    Assert.That(viewModel.Model.TodoItems.Count, Is.EqualTo(1));
    Assert.That(viewModel.Model.TodoItems[0].Content, Is.EqualTo("Test"));
}
```

---

## ?? 参考资源

### 官方文档

- [C# Coding Conventions](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- [.NET Design Guidelines](https://docs.microsoft.com/en-us/dotnet/standard/design-guidelines/)
- [WPF Best Practices](https://docs.microsoft.com/en-us/dotnet/desktop/wpf/)

### 项目文档

- [分部类重构文档](./MainWindowViewModel分部类重构文档.md)
- [项目交接文档](../00-必读/交接文档-最新版.md)
- [开发路线图](../06-规划文档/开发路线图-v1.0.md)

---

## ?? 检查清单

在提交代码前，请检查以下项目：

### 代码质量

- [ ] 代码符合命名规范
- [ ] 有适当的注释
- [ ] 没有编译警告
- [ ] 没有代码重复

### 架构规范

- [ ] 遵循 MVVM 模式
- [ ] 单个类不超过 500 行（使用分部类）
- [ ] 方法不超过 100 行
- [ ] 职责单一清晰

### 异步编程

- [ ] 异步方法以 Async 结尾
- [ ] 使用 await 而不是 .Wait()
- [ ] 正确使用 ConfigureAwait

### 数据库

- [ ] 使用异步方法
- [ ] 正确配置实体关系
- [ ] 添加必要的索引

### Git

- [ ] 提交消息符合规范
- [ ] 一次提交只做一件事
- [ ] 提交前已测试

---

## ?? 版本历史

| 版本 | 日期 | 变更内容 |
|------|------|----------|
| v1.0 | 2025-01-02 | 初始版本，添加分部类规范 |

---

**文档版本**: v1.0  
**创建日期**: 2025-01-02  
**维护者**: SceneTodo Team  

**遵循规范，编写优质代码！** ??
