# SceneTodo - 智能体开发指南

## 项目概述

这是一个 **WPF 桌面应用程序**（SceneTodo/TodoOverlayApp）——一个待办事项管理器，带有悬浮窗口，可以将待办事项与特定的应用程序窗口关联。

**技术栈：**
- .NET 8.0 Windows (WPF)
- Entity Framework Core + SQLite（本地数据库）
- Quartz.NET（任务调度）
- HandyControl（UI 组件）

**架构：**
- **MVVM 模式**，使用分部类 ViewModels
- Repository 模式用于数据访问
- Service Locator 模式（App.xaml.cs 中的静态单例）

---

## 构建与开发命令

```bash
# 构建解决方案
dotnet build SceneTodo.sln

# 构建单个项目（Release 版本）
dotnet build SceneTodo.csproj -c Release

# 运行应用程序
dotnet run --project SceneTodo.csproj

# 发布（Windows x64）
dotnet publish SceneTodo.csproj -c Release -r win-x64 --self-contained false -o publish

# 类型检查（编译时启用可空引用类型）
dotnet build SceneTodo.sln

# 格式化/代码检查（需要 dotnet-format 工具）
dotnet tool install -g dotnet-format
dotnet format SceneTodo.sln
```

**测试：**
- 当前此仓库中不存在测试项目
- 添加测试：创建 xUnit/NUnit/MSTest 项目，添加到解决方案中，引用主项目
- 运行测试：`dotnet test <TestProject.csproj>`
- 运行单个测试：`dotnet test --filter "FullyQualifiedName=Namespace.ClassName.MethodName"`

---

## 代码风格指南

### 命名约定

- **类/方法/属性：** `PascalCase`
  ```csharp
  public class TodoItemRepository
  public async Task<IEnumerable<TodoItem>> GetAllAsync()
  public string Name { get; set; }
  ```

- **私有字段：** `camelCase`，带下划线前缀 `_`
  ```csharp
  private string _searchText;
  private readonly TodoItemSchedulerService? _schedulerService;
  ```

- **常量/静态只读：** `PascalCase`
  ```csharp
  public static readonly string DatabasePath;
  ```

- **接口：** `I` 前缀 + `PascalCase`（在此代码库中很少使用）
- **事件：** `PascalCase` + `EventHandler?` 后缀
- **异步方法：** 返回 `Task<T>` 的方法使用 `Async` 后缀

### 导入与 using 语句

- 标准 .NET 命名空间顺序：System.* → Microsoft.* → 第三方 → 项目命名空间
- 启用隐式 using（`<ImplicitUsings>enable</ImplicitUsings>`）
- using 语句放在文件顶部，按命名空间层级分组

### 代码组织

#### MVVM 模式

**Views**（`Views/*.xaml` + `Views/*.xaml.cs`）：
- XAML 用于布局和数据绑定
- Code-behind 仅用于 UI 事件处理，不包含业务逻辑

**ViewModels**（`ViewModels/*.cs`）：
- 大型 ViewModels 按功能拆分为 **分部类**（例如 `MainWindowViewModel.Core.cs`、`MainWindowViewModel.Search.cs`）
- 实现 `INotifyPropertyChanged`（通过 `BaseModel` 或直接实现）
- 使用 `RelayCommand` 作为 `ICommand` 属性
- 为列表公开 `ObservableCollection<T>`
- 私有字段使用 `_camelCase`，公共属性使用 `PascalCase`

**Models**（`Models/*.cs`）：
- 领域实体和设置模型
- 通过 `BaseModel` 实现 `INotifyPropertyChanged` 以进行数据绑定
- 使用私有后备字段和属性更改通知：
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

### 数据库与数据访问

- 所有数据库操作使用 **Repository 模式**
- 所有数据库调用使用 **Async/await**（异步方法必须以 `Async` 结尾）
- Repository 类位于 `Services/Database/Repositories/`
- EF Core Fluent API 位于 `TodoDbContext.OnModelCreating()`
- JSON 存储在字符串字段中以实现灵活数据（例如 `LinkedActionsJson`、`TagsJson`）

```csharp
// Repository 模式示例
public async Task<TodoItem?> GetByIdAsync(string id)
{
    return await dbContext.TodoItems.FirstOrDefaultAsync(t => t.Id == id);
}
```

### Async/Await 模式

- 所有数据库操作必须是异步的
- 在非 UI 代码中使用 `ConfigureAwait(false)`（repositories、services）
- 使用 `Dispatcher.Invoke` 或 `DispatcherTimer` 进行 UI 线程操作

### 值转换器

- 位于 `Converters/` 文件夹
- 实现 `IValueConverter` 或 `IMultiValueConverter`
- 命名：`<Source>To<Destination>Converter`（例如 `PriorityToBorderBrushConverter`）

### 错误处理

- 对外部操作使用 try-catch（文件 I/O、数据库、调度器）
- 使用 HandyControl 的 `Growl` 或 `MessageBox` 显示面向用户的消息
- 使用 `System.Diagnostics.Debug.WriteLine` 进行调试输出
- **避免**空 catch 块——始终处理或记录异常

### 注释与文档

- 公共 API 使用 XML 文档注释
- 许多现有注释是中文的——现有代码保持中文，新代码使用英文
- 注释应解释 **为什么**，而不是 **是什么**（代码应该是自文档化的）

### XAML 指南

- 使用 `Binding` 配合 `UpdateSourceTrigger=PropertyChanged` 进行实时更新
- 可编辑属性使用 `Mode=TwoWay`
- 应用 `Converters/` 文件夹中的转换器
- 优先使用 HandyControl 组件而非标准 WPF 控件
- 在 `App.xaml` 中使用资源字典进行共享样式

### Service Locator 模式

- `App.xaml.cs` 中的静态属性保存单例服务：
  - `App.DbContext`
  - `App.TodoItemRepository`
  - `App.AutoTaskRepository`
  - `App.SchedulerService`
- 通过静态属性访问，**不要**使用构造函数注入

### 定时器与调度

- **UI 定时器：** `DispatcherTimer`（主 UI 线程）
- **Cron 调度：** 通过 `TodoItemSchedulerService` 使用 Quartz.NET
- **防抖：** 使用短间隔的 `DispatcherTimer`（例如搜索时 300ms）

---

## 项目结构

```
SceneTodo/
├── Views/                    # XAML UI 文件（.xaml + .xaml.cs）
├── ViewModels/              # ViewModel 类（按功能拆分的分部类）
├── Models/                  # 领域模型和设置
├── Services/
│   ├── Database/            # EF Core DbContext、repositories、factory
│   ├── Scheduler/           # Quartz.NET 调度逻辑
│   ├── BackupService.cs     # 备份/恢复功能
│   ├── LocalizationService.cs
│   ├── SearchService.cs
│   └── SearchHistoryManager.cs
├── Converters/              # XAML 值转换器
├── Utils/                   # 辅助类（TrayIcon、原生 interop）
├── Resources/               # 本地化字符串（.resx）
├── App.xaml.cs              # 应用程序引导和服务连接
├── MainWindow.xaml          # 主窗口 XAML
├── SceneTodo.csproj         # 项目文件
└── SceneTodo.sln            # 解决方案文件
```

---

## 常见任务的修改文件

| 任务 | 需要修改的文件 |
|------|---------------|
| 添加/修改数据库模型 | `Models/TodoItem.cs`（或其他模型）、`Services/Database/TodoDbContext.cs` |
| 更改持久化逻辑 | `Services/Database/Repositories/*.cs` |
| 修改定时任务 | `Services/Scheduler/TodoItemSchedulerService.cs` |
| 更改 UI 布局 | `Views/*.xaml`、`ViewModels/*.cs` |
| 添加新服务 | 在 `Services/` 中创建，在 `App.xaml.cs` 中连接 |
| 更改数据库架构 | `Models/*.cs`、`TodoDbContext.OnModelCreating()` |

---

## 重要说明

- **无 DI 容器：** `App.xaml.cs` 中的手动 Service Locator
- **启用可空引用类型：** 可空类型使用 `?`，非空断言使用 `!`
- **启用不安全块：** .csproj 中 `AllowUnsafeBlocks=true`（用于 Win32 interop）
- **仅限 Windows：** 带有 Win32 API 调用的 WPF 项目，不支持跨平台
- **数据库位置：** `%LocalAppData%\SceneTodo\todo.db`
- **现有 AI 生成代码：** 存在一些代码风格不一致（在 README 中已注明）——遵循本指南中的模式以保持一致性

---

## Cursor/Copilot 规则

此仓库中不存在 `.cursor/rules/`、`.cursorrules` 或 `.github/copilot-instructions.md` 文件。

---

## 测试指南

**当前状态：** 不存在测试项目。

**添加测试：**
```bash
dotnet new xunit -o Tests/SceneTodo.Tests
dotnet sln add Tests/SceneTodo.Tests/SceneTodo.Tests.csproj
dotnet add Tests/SceneTodo.Tests/SceneTodo.Tests.csproj reference SceneTodo.csproj
dotnet test Tests/SceneTodo.Tests/SceneTodo.Tests.csproj
```

**运行测试：**
```bash
# 解决方案中的所有测试
dotnet test

# 特定测试项目
dotnet test Tests/SceneTodo.Tests/SceneTodo.Tests.csproj

# 单个测试
dotnet test --filter "FullyQualifiedName=Namespace.ClassName.MethodName"
```
