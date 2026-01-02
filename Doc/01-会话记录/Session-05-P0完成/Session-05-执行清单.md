# ? Session-05 执行清单

> **创建日期**: 2026-01-02 19:00:00  
> **预计完成时间**: 4-6小时  
> **优先级**: P0核心功能完成

---

## ?? 必须完成项（按顺序）

### 1?? 快速修复 - 历史记录按钮 ?? 10分钟
**文件**: `MainWindow.xaml`

**任务**:
- [ ] 在左侧菜单添加"历史记录"按钮
- [ ] 绑定到 `ShowHistoryCommand`
- [ ] 使用 HandyControl 图标样式
- [ ] 测试按钮功能

**代码**:
```xaml
<Button 
    Content="历史记录" 
    Command="{Binding ShowHistoryCommand}"
    Style="{StaticResource ButtonIcon}"
    hc:IconElement.Geometry="{StaticResource HistoryGeometry}" 
    Margin="0,5,0,0"/>
```

---

### 2?? P0功能 - 遮盖层位置选择 ?? 3-4小时

#### 步骤1: 验证数据模型 ?? 15分钟
**文件**: `Models/TodoItem.cs`

- [ ] 确认 `OverlayPosition` 枚举包含所有位置
- [ ] 确认 `OverlayOffsetX` 和 `OverlayOffsetY` 属性存在
- [ ] 验证默认值设置

**需要的枚举值**:
```csharp
public enum OverlayPosition
{
    TopLeft,      // 左上角
    TopRight,     // 右上角
    BottomLeft,   // 左下角
    BottomRight,  // 右下角
    Center,       // 居中
    Bottom        // 底部
}
```

#### 步骤2: 添加位置选择UI ?? 45分钟
**文件**: `Views/EditTodoItemWindow.xaml`

- [ ] 添加位置选择ComboBox
- [ ] 添加水平偏移NumericUpDown
- [ ] 添加垂直偏移NumericUpDown
- [ ] 添加位置预览（可选）
- [ ] 绑定到ViewModel

**UI布局**:
```xaml
<GroupBox Header="悬浮窗设置" Margin="0,10,0,0">
    <StackPanel>
        <TextBlock Text="显示位置" Margin="0,5,0,5"/>
        <ComboBox 
            SelectedItem="{Binding Todo.OverlayPosition}"
            DisplayMemberPath="Value">
            <ComboBoxItem Content="左上角" Tag="{x:Static local:OverlayPosition.TopLeft}"/>
            <ComboBoxItem Content="右上角" Tag="{x:Static local:OverlayPosition.TopRight}"/>
            <ComboBoxItem Content="左下角" Tag="{x:Static local:OverlayPosition.BottomLeft}"/>
            <ComboBoxItem Content="右下角" Tag="{x:Static local:OverlayPosition.BottomRight}"/>
            <ComboBoxItem Content="居中" Tag="{x:Static local:OverlayPosition.Center}"/>
            <ComboBoxItem Content="底部" Tag="{x:Static local:OverlayPosition.Bottom}"/>
        </ComboBox>
        
        <TextBlock Text="水平偏移 (像素)" Margin="0,10,0,5"/>
        <hc:NumericUpDown 
            Value="{Binding Todo.OverlayOffsetX}" 
            Minimum="-500" 
            Maximum="500"
            Increment="10"/>
        
        <TextBlock Text="垂直偏移 (像素)" Margin="0,10,0,5"/>
        <hc:NumericUpDown 
            Value="{Binding Todo.OverlayOffsetY}" 
            Minimum="-500" 
            Maximum="500"
            Increment="10"/>
    </StackPanel>
</GroupBox>
```

#### 步骤3: 验证位置计算逻辑 ?? 30分钟
**文件**: `ViewModels/MainWindowViewModel.cs`

- [ ] 检查 `UpdateOverlayPosition` 方法
- [ ] 确认所有6个位置的计算正确
- [ ] 测试偏移量应用
- [ ] 处理边界情况（超出屏幕）

**关键代码位置**: 第XXX行的 `UpdateOverlayPosition` 方法

#### 步骤4: 实现拖拽功能 ?? 1小时
**文件**: `Views/OverlayWindow.xaml.cs`

- [ ] 添加 `MouseLeftButtonDown` 事件处理
- [ ] 实现拖拽逻辑
- [ ] 计算并保存新的偏移量
- [ ] 更新数据库

**代码框架**:
```csharp
private bool _isDragging = false;
private Point _dragStartPoint;
private Point _windowStartPosition;

private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
{
    if (e.ClickCount == 1)
    {
        _isDragging = true;
        _dragStartPoint = e.GetPosition(null);
        _windowStartPosition = new Point(this.Left, this.Top);
        this.DragMove();
    }
}

private void Window_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
{
    if (_isDragging)
    {
        _isDragging = false;
        SaveNewOffset();
    }
}

private void SaveNewOffset()
{
    // 计算新偏移量
    // 更新TodoItem
    // 保存到数据库
}
```

#### 步骤5: 完整测试 ?? 1小时
**测试清单**:

**位置测试**:
- [ ] 左上角：悬浮窗在目标窗口左上角
- [ ] 右上角：悬浮窗在目标窗口右上角
- [ ] 左下角：悬浮窗在目标窗口左下角
- [ ] 右下角：悬浮窗在目标窗口右下角
- [ ] 居中：悬浮窗在目标窗口中心
- [ ] 底部：悬浮窗在目标窗口底部

**偏移测试**:
- [ ] 水平偏移+50：向右移动
- [ ] 水平偏移-50：向左移动
- [ ] 垂直偏移+50：向下移动
- [ ] 垂直偏移-50：向上移动
- [ ] 极限偏移：±500像素

**拖拽测试**:
- [ ] 可以拖拽悬浮窗
- [ ] 释放后位置保存
- [ ] 重启应用位置恢复
- [ ] 不超出屏幕边界

**边界测试**:
- [ ] 目标窗口最小化时隐藏
- [ ] 目标窗口恢复时显示
- [ ] 目标窗口移动时跟随
- [ ] 多显示器正常工作

---

### 3?? 代码重构 - MainWindowViewModel ?? 2-3小时

#### 步骤1: 创建服务类 ?? 1小时

**创建文件**:
- [ ] `Services/OverlayWindowService.cs`
- [ ] `Services/TodoItemService.cs`
- [ ] `Services/WindowActivationService.cs`

**OverlayWindowService.cs**:
```csharp
public class OverlayWindowService
{
    private readonly Dictionary<string, OverlayWindow> _overlayWindows = new();
    
    public void CreateOrUpdateOverlay(TodoItemModel item, IntPtr targetHandle) { }
    public void CloseOverlay(string appPath) { }
    public void CloseAllOverlays() { }
    public bool HasOverlay(string appPath) { }
}
```

**TodoItemService.cs**:
```csharp
public class TodoItemService
{
    private readonly ITodoItemRepository _repository;
    
    public async Task<TodoItemModel> AddAsync(TodoItemModel parent = null) { }
    public async Task UpdateAsync(TodoItemModel item) { }
    public async Task DeleteAsync(string id) { }
    public bool FindAndRemove(ObservableCollection<TodoItemModel> items, string id) { }
}
```

**WindowActivationService.cs**:
```csharp
public class WindowActivationService
{
    public bool ActivateWindow(IntPtr handle, string appName) { }
    public bool LaunchApplication(string appPath, string appName) { }
    public List<IntPtr> FindWindowsForProcess(int processId) { }
}
```

#### 步骤2: 重构ViewModel ?? 1小时

**文件**: `ViewModels/MainWindowViewModel.cs`

- [ ] 添加服务字段
- [ ] 构造函数注入服务
- [ ] 重构命令实现，调用服务
- [ ] 删除重复代码
- [ ] 确保代码少于400行

**重构前后对比**:
```csharp
// 重构前 (800+行)
private void AddTodoItem(object? parameter)
{
    // 大量业务逻辑代码...
}

// 重构后 (精简)
private async void AddTodoItem(object? parameter)
{
    var item = await _todoService.AddAsync(parameter as TodoItemModel);
    EditTodoItem(item);
}
```

#### 步骤3: 回归测试 ?? 30分钟

- [ ] 添加待办项
- [ ] 编辑待办项
- [ ] 删除待办项
- [ ] 优先级功能
- [ ] 关联操作功能
- [ ] 历史记录功能
- [ ] 注入/解除注入
- [ ] 自动注入
- [ ] 强制启动
- [ ] 右键菜单

---

## ?? 建议完成项

### 4?? 历史记录分页 ?? 2-3小时

#### Repository改进
**文件**: `Services/Database/Repositories/TodoItemRepository.cs`

- [ ] 添加分页查询方法
- [ ] 返回总数和数据

#### ViewModel改进
**文件**: `ViewModels/HistoryWindowViewModel.cs`

- [ ] 添加分页属性
- [ ] 添加翻页命令
- [ ] 实现分页加载逻辑

#### UI改进
**文件**: `Views/HistoryWindow.xaml`

- [ ] 添加分页控件
- [ ] 绑定翻页命令
- [ ] 显示页码信息

---

### 5?? 用户反馈改进 ?? 1小时

**替换MessageBox为Growl**:
- [ ] 成功操作：Growl.Success
- [ ] 警告信息：Growl.Warning
- [ ] 错误信息：Growl.Error
- [ ] 提示信息：Growl.Info

**主要修改位置**:
- `ViewModels/MainWindowViewModel.cs`
- `Views/EditTodoItemWindow.xaml.cs`
- `Views/EditLinkedActionWindow.xaml.cs`

---

## ?? 文档更新清单

### 必须更新
- [ ] `Doc/00-必读/项目状态总览.md`
  - P0完成度：75% → 100%
  - 整体完成度：30% → 40%+
  - 新增功能记录

- [ ] `Doc/00-必读/交接文档-最新版.md`
  - 添加Session-05记录
  - 更新已知问题
  - 更新下一步工作

- [ ] 创建 `Doc/01-会话记录/Session-05-P0完成/`
  - 交接文档.md
  - 功能实现记录.md
  - 测试指南.md
  - 代码重构说明.md

- [ ] 创建 `Doc/02-功能文档/遮盖层位置选择.md`
  - 功能说明
  - 使用方法
  - 配置选项
  - 常见问题

---

## ?? 测试验收标准

### 功能验收
- [ ] 历史记录按钮正常工作
- [ ] 6个位置选项全部正确
- [ ] 偏移量正负调整生效
- [ ] 拖拽功能正常保存
- [ ] 边界情况正确处理
- [ ] 所有现有功能正常（回归测试）

### 代码验收
- [ ] MainWindowViewModel < 400行
- [ ] 服务类职责清晰
- [ ] 无代码重复
- [ ] 命名规范统一

### 性能验收
- [ ] 启动时间 < 3秒
- [ ] 悬浮窗响应 < 200ms
- [ ] 内存占用 < 150MB

---

## ?? 进度追踪

### 第一天
- [ ] 快速修复（历史记录按钮）
- [ ] 位置选择数据模型验证
- [ ] 位置选择UI开发

### 第二天
- [ ] 位置计算逻辑完善
- [ ] 拖拽功能实现
- [ ] 完整功能测试

### 第三天
- [ ] 服务类创建
- [ ] ViewModel重构
- [ ] 回归测试

### 第四天
- [ ] 历史记录分页（可选）
- [ ] 用户反馈改进（可选）
- [ ] 文档更新

---

## ?? 注意事项

1. **拖拽功能**：确保不影响目标窗口的正常操作
2. **边界检查**：悬浮窗不能超出屏幕范围
3. **性能影响**：重构后要保证性能不下降
4. **向后兼容**：旧数据要能正常加载（默认位置）
5. **多显示器**：测试多显示器场景

---

## ?? 完成标志

? **Session-05成功完成的标志**:
1. P0功能100%完成（4/4）
2. 代码质量显著提升
3. 所有功能正常工作
4. 文档完整更新
5. 达成v1.0里程碑

---

**清单版本**: 1.0  
**创建日期**: 2026-01-02 19:00:00  
**预计完成**: 2026-01-07

**开始执行，逐项勾选，稳步推进！** ??
