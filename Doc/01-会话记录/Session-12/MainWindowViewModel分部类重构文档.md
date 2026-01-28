# MainWindowViewModel 分部类重构文档

> **重构日期**: 2025-01-02  
> **重构原因**: 原文件过长（~1300行），难以维护和扩展  
> **重构方法**: 使用分部类（partial class）拆分  
> **状态**: ? 完成并编译成功

---

## ?? 重构前后对比

### 重构前
```
ViewModels/
└── MainWindowViewModel.cs  (~1300行)
```

### 重构后
```
ViewModels/
├── MainWindowViewModel.Core.cs              (~160行) - 核心部分
├── MainWindowViewModel.Navigation.cs        (~70行)  - 导航和窗口
├── MainWindowViewModel.TodoManagement.cs    (~260行) - 待办项管理
├── MainWindowViewModel.OverlayManagement.cs (~320行) - 悬浮窗管理
├── MainWindowViewModel.DueDateReminders.cs  (~90行)  - 截止时间提醒
└── MainWindowViewModel.TagFilter.cs         (~100行) - 标签筛选
```

**总计**: 6个文件，~1000行代码

---

## ?? 文件职责说明

### 1. MainWindowViewModel.Core.cs (核心部分)

**职责**: 类定义、属性、字段、命令、构造函数、基础方法

**包含内容**:
- `INotifyPropertyChanged` 实现
- 字段定义（overlayWindows, _schedulerService, dueDateCheckTimer, etc.）
- 属性定义（Model, CurrentContent）
- 所有命令定义（Commands）
- 构造函数
- 基础方法（InitializeData, InitializePageContent, Cleanup）

**代码行数**: ~160行

---

### 2. MainWindowViewModel.Navigation.cs (导航和窗口管理)

**职责**: 页面导航和窗口打开

**包含方法**:
- `ThemeSettings()` - 打开主题设置窗口
- `About()` - 打开关于窗口
- `ShowHistory()` - 打开历史记录窗口
- `ShowHistoryPage()` - 显示历史记录页面
- `ShowTodoListPage()` - 显示待办列表页面
- `ShowCalendarView()` - 显示日历视图
- `ShowScheduledTasks()` - 显示定时任务页面
- `OpenBackupManagement()` - 打开备份管理窗口 ? 新增

**代码行数**: ~70行

---

### 3. MainWindowViewModel.TodoManagement.cs (待办项管理)

**职责**: 待办项的增删改查和相关操作

**包含方法**:
- `AddTodoItem()` - 添加待办项
- `EditTodoItem()` - 编辑待办项
- `EditTodoItemModel()` - 更新待办项模型
- `DeleteTodoItem()` - 删除待办项
- `FindAndRemoveItemById()` - 递归查找并删除
- `ToggleIsCompleted()` - 切换完成状态
- `ExecuteLinkedAction()` - 执行关联操作
- `ResetAppConfig()` - 重置应用配置
- `ResetTodo()` - 重置待办数据

**代码行数**: ~260行

---

### 4. MainWindowViewModel.OverlayManagement.cs (悬浮窗管理)

**职责**: 悬浮窗创建、更新、关闭和应用启动

**包含方法**:
- `AutoInjectOverlays()` - 自动注入悬浮窗
- `ToggleIsInjected()` - 切换注入状态
- `HandleOverlayWindow()` - 处理悬浮窗逻辑
- `UpdateOverlayPosition()` - 更新悬浮窗位置
- `ForceLaunch()` - 强制启动应用
- `ActivateWindow()` - 激活窗口
- `FindWindowsForProcess()` - 查找进程窗口

**代码行数**: ~320行

**说明**: 这是最大的单个分部类文件，因为悬浮窗逻辑复杂且相关方法需要放在一起。

---

### 5. MainWindowViewModel.DueDateReminders.cs (截止时间提醒)

**职责**: 检查截止时间并发送通知

**包含方法**:
- `CheckDueDateReminders()` - 检查截止时间（定时器回调）
- `CheckDueDateRecursive()` - 递归检查所有待办项
- `ShowDueDateNotification()` - 显示通知

**代码行数**: ~90行

---

### 6. MainWindowViewModel.TagFilter.cs (标签筛选)

**职责**: 按标签筛选待办项

**包含字段**:
- `allTodoItems` - 所有待办项备份
- `currentFilterTag` - 当前筛选标签

**包含方法**:
- `FilterByTag()` - 按标签筛选
- `FilterByTagRecursive()` - 递归筛选
- `ClearTagFilter()` - 清除筛选

**代码行数**: ~100行

---

## ? 重构优势

### 1. **可维护性提升**
- 每个文件职责明确，易于理解
- 修改某个功能只需要打开对应文件
- 减少代码合并冲突

### 2. **可扩展性提升**
- 添加新功能只需创建新的分部类文件
- 例如：`MainWindowViewModel.BackupManagement.cs`
- 不会影响现有代码结构

### 3. **代码组织清晰**
- 按功能模块分类
- 文件命名直观（一看就知道是什么功能）
- 更符合单一职责原则

### 4. **团队协作友好**
- 不同开发者可以同时修改不同功能
- 代码审查更容易聚焦
- 减少文件锁定冲突

---

## ?? 最佳实践

### 分部类使用原则

1. **按功能领域拆分**
   - 不要按字母顺序或随机拆分
   - 相关功能放在同一个文件中

2. **控制文件大小**
   - 单个文件建议不超过 300-400 行
   - 如果单个功能模块超过 400 行，考虑进一步拆分

3. **命名规范**
   - 格式: `MainClass.Feature.cs`
   - 例如: `MainWindowViewModel.TodoManagement.cs`

4. **字段和属性放在 Core 文件中**
   - 所有字段定义放在核心文件
   - 避免字段分散在多个文件中

5. **命令定义集中管理**
   - 所有 `ICommand` 属性放在 Core 文件
   - 命令的实现方法放在对应功能文件中

---

## ?? 如何添加新功能

### 示例：添加备份管理功能

1. **创建新的分部类文件**
```csharp
// ViewModels/MainWindowViewModel.BackupManagement.cs
namespace SceneTodo.ViewModels
{
    public partial class MainWindowViewModel
    {
        private void OpenBackupManagement(object? parameter)
        {
            // 备份管理逻辑
        }
    }
}
```

2. **在 Core.cs 中添加命令**
```csharp
// MainWindowViewModel.Core.cs
public ICommand BackupManagementCommand { get; }

// 构造函数中初始化
BackupManagementCommand = new RelayCommand(OpenBackupManagement);
```

3. **完成！**
   - 新功能代码独立在新文件中
   - 不影响现有代码
   - 易于维护和测试

---

## ?? 迁移检查清单

- [x] 拆分原文件为6个分部类文件
- [x] 确保所有方法和属性都被迁移
- [x] 确保所有 using 指令正确
- [x] 删除原始的 MainWindowViewModel.cs
- [x] 编译成功，无错误
- [x] 功能测试（待办项CRUD、悬浮窗、截止时间、标签筛选）
- [x] 性能测试（确保没有性能下降）

---

## ?? 已知问题

### 无

---

## ?? 未来改进

### 可能的进一步拆分

如果 `MainWindowViewModel.OverlayManagement.cs` 继续增长（目前320行），可以考虑进一步拆分为：

1. `MainWindowViewModel.OverlayManagement.cs` - 悬浮窗管理
2. `MainWindowViewModel.AppLauncher.cs` - 应用启动和激活

---

## ?? 相关资源

### C# 分部类文档
- [Microsoft Docs - Partial Classes and Methods](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/partial-classes-and-methods)

### 最佳实践文章
- [When to use Partial Classes in C#](https://stackoverflow.com/questions/536457/when-to-use-partial-classes-in-c-sharp)

---

## ?? 代码统计

### 文件统计
| 文件 | 行数 | 占比 |
|-----|------|------|
| Core.cs | 160 | 16% |
| Navigation.cs | 70 | 7% |
| TodoManagement.cs | 260 | 26% |
| OverlayManagement.cs | 320 | 32% |
| DueDateReminders.cs | 90 | 9% |
| TagFilter.cs | 100 | 10% |
| **总计** | **1000** | **100%** |

### 方法统计
- 总方法数: ~40个
- 公开方法: ~5个
- 私有方法: ~35个
- 静态方法: ~3个

---

**重构完成时间**: 2025-01-02 23:30  
**重构耗时**: 约30分钟  
**重构者**: GitHub Copilot  
**编译状态**: ? 成功

**备注**: 此重构为 Session-12 的准备工作，使得添加备份管理功能更加容易。
