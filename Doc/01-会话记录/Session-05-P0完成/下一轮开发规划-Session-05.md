# ?? 下一轮开发规划 - Session-05

> **创建日期**: 2026-01-02 19:00:00  
> **版本**: 1.0  
> **状态**: 待执行  
> **预计工作量**: 4-6小时

---

## ?? 当前项目状态分析

### 整体完成度
- **P0功能**: 75% (3/4) - 缺少遮盖层位置选择
- **P1功能**: 0% (0/4) - 全部未开始
- **P2功能**: 0% (0/4) - 全部未开始
- **总体进度**: 约30-35%

### 核心问题
1. ?? **P1高优问题**: 历史记录窗口未集成到主界面（10分钟快速修复）
2. ? **P0剩余功能**: 遮盖层位置选择（3-4小时开发）
3. ?? **代码质量**: MainWindowViewModel.cs 过长（800+行），需要重构

---

## ?? 本轮开发目标（按优先级排序）

### ?? 必须完成（P0级别）

#### 1. 修复历史记录按钮缺失问题 ?????
**优先级**: P1 (阻断用户体验)  
**工作量**: 10分钟  
**难度**: ? (简单)

**问题描述**:
- `ShowHistoryCommand` 已在 ViewModel 中实现
- `HistoryWindow` 已完整开发
- 但 MainWindow.xaml 中缺少触发按钮
- 用户无法打开历史记录窗口

**修复方案**:
在 `MainWindow.xaml` 左侧菜单添加历史记录按钮：

```xaml
<!-- 在左侧菜单的合适位置添加 -->
<Button 
    Content="历史记录" 
    Command="{Binding ShowHistoryCommand}"
    Style="{StaticResource ButtonIcon}"
    hc:IconElement.Geometry="{StaticResource HistoryGeometry}" 
    Margin="0,5,0,0"/>
```

**验收标准**:
- ? 点击按钮能打开历史记录窗口
- ? 历史记录窗口能正常显示所有记录
- ? 恢复和删除功能正常工作

---

#### 2. 实现遮盖层位置选择功能 ?????
**优先级**: P0 (核心功能)  
**工作量**: 3-4小时  
**难度**: ??? (中等)

**功能需求**:
根据 PRD 要求：
> 挂载时可设置待办在目标应用窗口的展示位置（如左上角、右上角、左下角、右下角、居中），支持拖拽微调位置。

**技术设计**:

1. **枚举定义** (已存在，需验证):
```csharp
// Models/TodoItem.cs
public enum OverlayPosition
{
    TopLeft,      // 左上角
    TopRight,     // 右上角
    BottomLeft,   // 左下角
    BottomRight,  // 右下角
    Center,       // 居中
    Bottom        // 底部（已实现）
}
```

2. **数据模型** (已部分实现):
```csharp
// Models/TodoItem.cs
public OverlayPosition OverlayPosition { get; set; } = OverlayPosition.Bottom;
public double OverlayOffsetX { get; set; } = 0;
public double OverlayOffsetY { get; set; } = 0;
```

3. **UI改进** - 编辑窗口:
```xaml
<!-- Views/EditTodoItemWindow.xaml -->
<StackPanel>
    <TextBlock Text="悬浮窗位置" Margin="0,10,0,5"/>
    <ComboBox 
        SelectedItem="{Binding OverlayPosition}"
        ItemsSource="{Binding PositionOptions}">
        <ComboBox.ItemTemplate>
            <DataTemplate>
                <TextBlock Text="{Binding Converter={StaticResource EnumToDescriptionConverter}}"/>
            </DataTemplate>
        </ComboBox.ItemTemplate>
    </ComboBox>
    
    <TextBlock Text="水平偏移 (像素)" Margin="0,10,0,5"/>
    <hc:NumericUpDown 
        Value="{Binding OverlayOffsetX}" 
        Minimum="-500" 
        Maximum="500"/>
    
    <TextBlock Text="垂直偏移 (像素)" Margin="0,10,0,5"/>
    <hc:NumericUpDown 
        Value="{Binding OverlayOffsetY}" 
        Minimum="-500" 
        Maximum="500"/>
</StackPanel>
```

4. **位置计算逻辑改进** (部分已实现):
在 `MainWindowViewModel.cs` 的 `UpdateOverlayPosition` 方法中已经实现了基本逻辑，需要：
- ? 验证所有位置选项是否正确计算
- ? 测试偏移量是否正确应用
- ? 处理边界情况（窗口移出屏幕）

5. **拖拽微调功能** (新增):
```csharp
// Views/OverlayWindow.xaml.cs
private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
{
    if (e.ClickCount == 1)
    {
        this.DragMove();
        // 拖拽结束后更新偏移量
        UpdateOffsetFromPosition();
    }
}

private void UpdateOffsetFromPosition()
{
    // 根据当前位置计算并保存偏移量
    // 更新到数据库
}
```

**实现步骤**:
1. ? 验证 `OverlayPosition` 枚举是否完整
2. ? 在 `EditTodoItemWindow.xaml` 添加位置选择UI
3. ? 测试 `UpdateOverlayPosition` 方法的各个位置
4. ? 实现拖拽微调功能（鼠标拖动）
5. ? 实现偏移量的保存和恢复
6. ? 添加位置预览功能（可选）
7. ? 完整测试所有位置和偏移

**验收标准**:
- ? 可以在编辑窗口选择6个位置（左上、右上、左下、右下、居中、底部）
- ? 每个位置的悬浮窗显示正确
- ? 偏移量可以正负调整
- ? 拖拽悬浮窗后能保存新位置
- ? 重启应用后位置和偏移保持
- ? 边界情况处理正确（不超出屏幕）

**参考文档**:
- `Doc/01-会话记录/Session-01-P0核心功能实现/P0核心功能实现指南.md`
- `Doc/06-规划文档/PRD-初稿.md` (2.1节)

---

### ?? 建议完成（P1级别）

#### 3. 代码重构 - MainWindowViewModel ????
**优先级**: P2 (代码质量)  
**工作量**: 2-3小时  
**难度**: ??? (中等)

**问题分析**:
- `MainWindowViewModel.cs` 约800+行，违反单一职责原则
- 包含太多职责：命令处理、窗口管理、数据操作、定时任务等
- 重复的if判断和代码

**重构方案**:

1. **拆分服务类**:
```csharp
// Services/OverlayWindowService.cs
public class OverlayWindowService
{
    private readonly Dictionary<string, OverlayWindow> _overlayWindows;
    
    public void CreateOverlay(TodoItemModel item, IntPtr targetHandle) { }
    public void UpdateOverlay(string appPath, IntPtr targetHandle) { }
    public void CloseOverlay(string appPath) { }
    public void CloseAllOverlays() { }
}

// Services/TodoItemService.cs
public class TodoItemService
{
    public async Task<TodoItemModel> AddTodoItemAsync(TodoItemModel parent) { }
    public async Task UpdateTodoItemAsync(TodoItemModel item) { }
    public async Task DeleteTodoItemAsync(string id) { }
    public async Task<ObservableCollection<TodoItemModel>> LoadTodoItemsAsync() { }
}

// Services/WindowActivationService.cs
public class WindowActivationService
{
    public bool ActivateWindow(IntPtr handle, string appName) { }
    public bool LaunchApplication(string appPath) { }
    public List<IntPtr> FindWindowsForProcess(int processId) { }
}
```

2. **简化 ViewModel**:
```csharp
public class MainWindowViewModel : INotifyPropertyChanged
{
    private readonly OverlayWindowService _overlayService;
    private readonly TodoItemService _todoService;
    private readonly WindowActivationService _windowService;
    
    // 只保留命令和属性
    public ICommand AddTodoItemCommand { get; }
    public ICommand DeleteTodoItemCommand { get; }
    // ... 其他命令
    
    // 命令实现变得简单
    private async void AddTodoItem(object? parameter)
    {
        var item = await _todoService.AddTodoItemAsync(parameter as TodoItemModel);
        EditTodoItem(item);
    }
}
```

**实现步骤**:
1. ? 创建 `OverlayWindowService` 类
2. ? 创建 `TodoItemService` 类
3. ? 创建 `WindowActivationService` 类
4. ? 重构 `MainWindowViewModel`，使用新服务
5. ? 完整回归测试
6. ? 更新文档

**验收标准**:
- ? `MainWindowViewModel.cs` 少于400行
- ? 每个服务类职责单一明确
- ? 所有功能正常工作
- ? 代码可读性提高

---

#### 4. 历史记录分页功能 ???
**优先级**: P1 (性能优化)  
**工作量**: 2-3小时  
**难度**: ?? (简单-中等)

**问题描述**:
- 当前一次性加载所有历史记录
- 大量数据时影响性能和用户体验

**实现方案**:

1. **分页参数**:
```csharp
// ViewModels/HistoryWindowViewModel.cs
private int _currentPage = 1;
private int _pageSize = 50;
private int _totalCount = 0;

public int CurrentPage 
{ 
    get => _currentPage; 
    set { _currentPage = value; LoadPageAsync(); }
}
```

2. **Repository改进**:
```csharp
// Services/Database/Repositories/TodoItemRepository.cs
public async Task<(List<TodoItemHistory> Items, int TotalCount)> GetHistoriesPagedAsync(
    int pageIndex, 
    int pageSize)
{
    var totalCount = await _context.TodoItemHistories.CountAsync();
    var items = await _context.TodoItemHistories
        .OrderByDescending(h => h.CreatedAt)
        .Skip((pageIndex - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync();
    
    return (items, totalCount);
}
```

3. **UI改进**:
```xaml
<!-- Views/HistoryWindow.xaml -->
<StackPanel>
    <!-- 历史记录列表 -->
    <DataGrid x:Name="HistoryDataGrid" ... />
    
    <!-- 分页控件 -->
    <StackPanel Orientation="Horizontal" HorizontalAlignment="Center">
        <Button Content="上一页" Command="{Binding PrevPageCommand}"/>
        <TextBlock Text="{Binding CurrentPage}" Margin="10,0"/>
        <TextBlock Text="/" Margin="5,0"/>
        <TextBlock Text="{Binding TotalPages}" Margin="5,0"/>
        <Button Content="下一页" Command="{Binding NextPageCommand}"/>
        <ComboBox 
            ItemsSource="{Binding PageSizeOptions}" 
            SelectedItem="{Binding PageSize}"/>
    </StackPanel>
</StackPanel>
```

**验收标准**:
- ? 默认每页显示50条
- ? 可以切换页面
- ? 可以选择每页数量（20/50/100）
- ? 显示总页数和当前页
- ? 性能良好（大量数据时）

---

### ?? 可选完成（加分项）

#### 5. 添加单元测试 ???
**优先级**: P2 (代码质量)  
**工作量**: 3-4小时  
**难度**: ??? (中等)

**实现方案**:

1. **创建测试项目**:
```bash
dotnet new xunit -n SceneTodo.Tests
dotnet add reference ../SceneTodo/SceneTodo.csproj
dotnet add package Moq
dotnet add package FluentAssertions
```

2. **测试重点**:
```csharp
// Tests/Models/TodoItemModelTests.cs
public class TodoItemModelTests
{
    [Fact]
    public void RecTodoItems_ShouldFilterByAppPath()
    {
        // Arrange
        var items = CreateTestData();
        
        // Act
        var result = TodoItemModel.RecTodoItems(items, "notepad.exe");
        
        // Assert
        result.Should().HaveCount(2);
    }
}

// Tests/Services/TodoItemServiceTests.cs
public class TodoItemServiceTests
{
    [Fact]
    public async Task AddTodoItem_ShouldSaveToDatabase()
    {
        // Arrange
        var mockRepo = new Mock<ITodoItemRepository>();
        var service = new TodoItemService(mockRepo.Object);
        
        // Act
        await service.AddTodoItemAsync(new TodoItemModel());
        
        // Assert
        mockRepo.Verify(r => r.AddAsync(It.IsAny<TodoItem>()), Times.Once);
    }
}
```

**验收标准**:
- ? 核心业务逻辑有测试覆盖
- ? 测试通过率100%
- ? 代码覆盖率>60%

---

#### 6. 改进用户反馈机制 ??
**优先级**: P2 (用户体验)  
**工作量**: 1-2小时  
**难度**: ?? (简单)

**实现方案**:

1. **Toast通知替代MessageBox**:
```csharp
// 使用 HandyControl 的 Growl
Growl.Success("待办项已添加");
Growl.Warning("未找到关联的应用");
Growl.Error("操作失败：" + ex.Message);
Growl.Info("数据已自动保存");
```

2. **加载指示器**:
```xaml
<!-- 长操作时显示加载动画 -->
<hc:LoadingCircle IsRunning="{Binding IsLoading}"/>
```

3. **操作确认对话框改进**:
```csharp
// 使用 HandyControl 的 Dialog
var result = Dialog.Show(new MessageBoxInfo
{
    Message = "确定要删除吗？",
    Caption = "确认",
    Button = MessageBoxButton.YesNo,
    IconBrushKey = ResourceToken.AccentBrush,
    IconKey = ResourceToken.AskGeometry
});
```

**验收标准**:
- ? 减少阻塞式MessageBox
- ? 增加非阻塞Toast提示
- ? 长操作有加载指示
- ? 用户体验更流畅

---

## ?? 开发顺序建议

### 第一阶段：快速修复（30分钟）
1. ? 修复历史记录按钮缺失 (10分钟)
2. ? 测试历史记录功能 (10分钟)
3. ? 验证现有位置选择逻辑 (10分钟)

### 第二阶段：P0核心功能（3-4小时）
4. ? 改进遮盖层位置选择UI (1小时)
5. ? 实现拖拽微调功能 (1小时)
6. ? 完整测试所有位置选项 (1小时)
7. ? 边界情况处理和优化 (30分钟)
8. ? 文档更新 (30分钟)

### 第三阶段：代码质量改进（2-3小时）
9. ? 创建服务类 (1小时)
10. ? 重构 MainWindowViewModel (1小时)
11. ? 回归测试 (30分钟)
12. ? 文档更新 (30分钟)

### 第四阶段：性能优化（可选，2-3小时）
13. ? 实现历史记录分页 (2小时)
14. ? 测试和优化 (1小时)

### 第五阶段：锦上添花（可选，3-5小时）
15. ? 添加单元测试 (3小时)
16. ? 改进用户反馈机制 (2小时)

---

## ?? 测试计划

### 功能测试

#### 1. 历史记录按钮测试
- [ ] 点击按钮能打开窗口
- [ ] 窗口显示所有历史记录
- [ ] 恢复功能正常
- [ ] 删除功能正常
- [ ] 关闭窗口不影响主窗口

#### 2. 遮盖层位置选择测试
**位置测试**:
- [ ] 左上角位置正确
- [ ] 右上角位置正确
- [ ] 左下角位置正确
- [ ] 右下角位置正确
- [ ] 居中位置正确
- [ ] 底部位置正确

**偏移测试**:
- [ ] 水平偏移正向生效
- [ ] 水平偏移负向生效
- [ ] 垂直偏移正向生效
- [ ] 垂直偏移负向生效
- [ ] 极限偏移（±500）正确处理

**拖拽测试**:
- [ ] 可以拖拽悬浮窗
- [ ] 拖拽后位置保存
- [ ] 重启应用位置恢复
- [ ] 拖拽不超出屏幕边界

**边界测试**:
- [ ] 窗口最小化时悬浮窗隐藏
- [ ] 窗口恢复时悬浮窗显示
- [ ] 目标窗口移动时悬浮窗跟随
- [ ] 多显示器场景正常工作

#### 3. 代码重构测试（回归测试）
- [ ] 所有待办项CRUD操作正常
- [ ] 优先级功能正常
- [ ] 关联操作功能正常
- [ ] 历史记录功能正常
- [ ] 注入/解除注入功能正常
- [ ] 自动注入功能正常
- [ ] 强制启动功能正常
- [ ] 右键菜单功能正常

#### 4. 历史记录分页测试
- [ ] 分页加载正常
- [ ] 翻页功能正常
- [ ] 每页数量选择生效
- [ ] 页码显示正确
- [ ] 性能良好（1000+条记录）

### 性能测试
- [ ] 启动时间 < 3秒
- [ ] 待办项加载时间 < 500ms
- [ ] 悬浮窗响应时间 < 200ms
- [ ] 内存占用 < 150MB（重构后可能增加）

### 兼容性测试
- [ ] Windows 10 正常运行
- [ ] Windows 11 正常运行
- [ ] 不同分辨率正常显示
- [ ] 多显示器配置正常工作

---

## ?? 文档更新清单

### 必须更新的文档
1. ? `Doc/00-必读/项目状态总览.md`
   - 更新P0完成度为100%
   - 更新整体完成度
   - 记录新增功能

2. ? `Doc/00-必读/交接文档-最新版.md`
   - 添加Session-05会话记录
   - 更新已知问题列表
   - 更新下一步工作建议

3. ? 创建 `Doc/01-会话记录/Session-05-P0完成/`
   - 交接文档
   - 功能实现记录
   - 测试指南
   - 代码重构说明

4. ? `Doc/02-功能文档/遮盖层位置选择.md` (新建)
   - 功能说明
   - 使用方法
   - 配置选项
   - 常见问题

### 可选更新的文档
5. ?? `Doc/04-技术文档/架构设计.md` (新建)
   - 服务层架构
   - 依赖注入
   - 设计模式

6. ?? `Doc/03-测试文档/自动化测试指南.md` (新建)
   - 单元测试编写
   - 测试运行方法
   - CI/CD集成

---

## ?? 风险和注意事项

### 技术风险
1. **拖拽功能复杂度**
   - 风险：拖拽悬浮窗可能影响目标窗口操作
   - 缓解：添加配置选项，允许禁用拖拽

2. **代码重构影响**
   - 风险：大规模重构可能引入新Bug
   - 缓解：完整回归测试，小步提交

3. **分页性能**
   - 风险：大量数据时分页查询可能慢
   - 缓解：添加索引，优化查询

### 时间风险
1. **预估时间不足**
   - 风险：实际开发时间可能超过预估
   - 缓解：优先完成必须项，可选项灵活调整

2. **测试时间不足**
   - 风险：功能测试不充分
   - 缓解：自动化测试，减少手动测试

### 用户体验风险
1. **拖拽交互不直观**
   - 风险：用户不知道可以拖拽
   - 缓解：添加提示，更新用户文档

2. **位置选择过于复杂**
   - 风险：6个位置+偏移量可能让用户困惑
   - 缓解：提供预设位置，偏移量可选

---

## ?? 成功标准

### 本轮开发成功的标志
1. ? P0功能100%完成（4/4）
2. ? 历史记录按钮已集成
3. ? 代码质量显著提升（ViewModel<400行）
4. ? 所有功能正常工作（回归测试通过）
5. ? 文档完整更新
6. ? 用户体验改善（Toast提示等）

### 可以进入下一阶段的条件
1. ? 所有P0功能完成并测试通过
2. ? 无P0/P1级别已知问题
3. ? 代码质量达标（无明显技术债）
4. ? 文档完整
5. ? 用户反馈良好

---

## ?? 需要讨论的问题

### 产品决策
1. **拖拽功能的优先级**
   - 问题：PRD要求"支持拖拽微调位置"，但这增加复杂度
   - 建议：先实现位置选择，拖拽作为增强功能
   - 需要确认：是否P0必须完成？

2. **位置选项的数量**
   - 问题：6个位置是否足够？是否需要更多？
   - 建议：先实现6个，根据用户反馈调整
   - 需要确认：是否需要"左侧""右侧"等中间位置？

3. **分页默认数量**
   - 问题：默认每页50条是否合适？
   - 建议：50条对大多数用户足够
   - 需要确认：是否需要可配置？

### 技术决策
1. **服务类的注入方式**
   - 问题：是否引入依赖注入容器（如Microsoft.Extensions.DependencyInjection）？
   - 建议：暂时手动注入，保持简单
   - 需要确认：是否需要更完善的DI？

2. **单元测试的范围**
   - 问题：哪些代码需要单元测试？
   - 建议：核心业务逻辑（Models、Services）
   - 需要确认：ViewModel是否需要测试？

3. **Toast通知的实现**
   - 问题：使用HandyControl的Growl还是自定义？
   - 建议：使用HandyControl，保持UI一致
   - 需要确认：是否有其他偏好？

---

## ?? 时间表（建议）

### Day 1 - 快速修复和准备（2小时）
- **上午** (9:00-11:00)
  - 修复历史记录按钮 (10分钟)
  - 验证现有位置选择逻辑 (30分钟)
  - 设计位置选择UI (30分钟)
  - 讨论产品和技术决策 (30分钟)

### Day 2 - P0核心功能开发（4小时）
- **上午** (9:00-12:00)
  - 实现位置选择UI (1小时)
  - 完善位置计算逻辑 (1小时)
  - 测试所有位置选项 (1小时)

- **下午** (14:00-15:00)
  - 实现拖拽微调功能 (1小时)

### Day 3 - 代码重构（3小时）
- **上午** (9:00-11:00)
  - 创建服务类 (1小时)
  - 重构ViewModel (1小时)

- **下午** (14:00-15:00)
  - 完整回归测试 (1小时)

### Day 4 - 性能优化和文档（2-3小时）
- **上午** (9:00-11:00)
  - 实现历史记录分页 (2小时)

- **下午** (14:00-15:00)
  - 更新所有文档 (1小时)

### Day 5 - 可选功能（3-5小时）
- **全天**
  - 添加单元测试（可选）
  - 改进用户反馈机制（可选）
  - 用户验收测试

---

## ?? 期望成果

完成本轮开发后，项目将达到：

### 功能层面
- ? **P0功能100%完成** - 所有核心功能实现
- ? **用户体验提升** - 位置选择、Toast提示
- ? **性能改善** - 历史记录分页

### 代码层面
- ? **代码质量提升** - ViewModel重构，职责清晰
- ? **可维护性增强** - 服务层抽象，易于扩展
- ? **测试覆盖** - 核心逻辑有单元测试（可选）

### 项目层面
- ? **完成度提升** - 从30%→40%+
- ? **文档完善** - 完整的会话记录和功能文档
- ? **技术债减少** - 主要代码问题解决

### 里程碑
?? **达成v1.0里程碑 - P0核心功能完整实现**

---

## ?? 参考资料

### 内部文档
- `Doc/00-必读/交接文档-最新版.md`
- `Doc/00-必读/项目状态总览.md`
- `Doc/06-规划文档/PRD-初稿.md`
- `Doc/01-会话记录/Session-01-P0核心功能实现/`
- `Doc/01-会话记录/Session-03-右键菜单改进/`

### 技术文档
- WPF拖拽：https://learn.microsoft.com/zh-cn/dotnet/desktop/wpf/advanced/drag-and-drop-overview
- HandyControl Growl：https://handyorg.github.io/handycontrol/growl/
- Entity Framework分页：https://learn.microsoft.com/zh-cn/ef/core/querying/pagination

### 设计模式
- Service Layer Pattern
- Repository Pattern
- MVVM Pattern

---

**规划版本**: 1.0  
**创建日期**: 2026-01-02 19:00:00  
**规划者**: AI Development Agent  
**预计完成日期**: 2026-01-07 (5个工作日)

**让我们开始这一轮精彩的开发吧！** ??
