# ?? Session-06 执行规划 - 阶段性单功能开发

> **创建日期**: 2025-01-02 20:50:00  
> **规划版本**: 1.0  
> **执行模式**: AI Agent 阶段性单功能  
> **前置会话**: Session-05（P0功能100%完成）

---

## ?? 规划说明

### 规划原则
本次规划遵循**阶段性单功能**开发原则：
- ? 每个阶段只完成一个独立功能
- ? 功能边界清晰，易于测试
- ? 逐步推进，避免复杂依赖
- ? 每个功能完成后立即验证

### 与 Session-05 的区别
| 维度 | Session-05 | Session-06 |
|------|-----------|-----------|
| 开发模式 | 批量并行 | 阶段性单功能 |
| 任务范围 | 多个子任务 | 单一明确功能 |
| 执行方式 | 一次性完成 | 分阶段完成 |
| 适用场景 | P0核心功能 | P1增强功能 |

---

## ?? Session-06 目标

### 核心目标
完成两个独立的 P1 功能：
1. **历史记录内嵌页面** - 改造现有历史记录窗口
2. **日历视图功能** - 新增日历视图显示待办项

### 成功标准
- ? 每个功能独立完成
- ? 编译0错误0警告
- ? 功能测试通过
- ? 文档完整清晰

---

## ?? 阶段划分

### 阶段 1: 历史记录内嵌页面 ?? 预计 2-3小时

#### 功能描述
将现有的独立历史记录窗口改造为主窗口内嵌页面，提升用户体验。

#### 实现方案

##### 1.1 UI结构调整
**修改**: `MainWindow.xaml`

当前结构（独立窗口）：
```
MainWindow
  ├─ 左侧菜单
  ├─ 待办项列表（TreeView）
  └─ 右键菜单
  
HistoryWindow (独立窗口)
  ├─ 历史记录列表
  └─ 操作按钮
```

目标结构（内嵌页面）：
```
MainWindow
  ├─ 左侧菜单
  │  └─ [新增] 历史记录按钮
  ├─ 主内容区（可切换）
  │  ├─ 待办项列表页面（默认）
  │  └─ 历史记录页面（新增）
  └─ 右键菜单
```

**技术方案**：
```xaml
<!-- MainWindow.xaml 主内容区改造 -->
<Grid x:Name="MainContentGrid">
    <!-- 使用 ContentControl 实现页面切换 -->
    <ContentControl x:Name="MainContentControl">
        <ContentControl.Style>
            <Style TargetType="ContentControl">
                <!-- 默认显示待办项列表 -->
                <Setter Property="Content" Value="{Binding TodoListContent}"/>
            </Style>
        </ContentControl.Style>
    </ContentControl>
</Grid>
```

##### 1.2 创建用户控件
**新建**: `Views/HistoryUserControl.xaml/.cs`

将 `HistoryWindow` 的内容提取为 UserControl：
```csharp
public partial class HistoryUserControl : UserControl
{
    public HistoryUserControl()
    {
        InitializeComponent();
        // 复用 HistoryWindowViewModel
        DataContext = new HistoryWindowViewModel();
    }
}
```

**重点**：
- ? 复用现有 `HistoryWindowViewModel` 逻辑
- ? 保持现有功能不变
- ? XAML 布局与窗口版本一致

##### 1.3 ViewModel 调整
**修改**: `ViewModels/MainWindowViewModel.cs`

添加页面切换逻辑：
```csharp
private object currentContent;
public object CurrentContent
{
    get => currentContent;
    set
    {
        currentContent = value;
        OnPropertyChanged(nameof(CurrentContent));
    }
}

private ICommand showHistoryPageCommand;
public ICommand ShowHistoryPageCommand
{
    get
    {
        return showHistoryPageCommand ??= new RelayCommand(obj =>
        {
            CurrentContent = new HistoryUserControl();
        });
    }
}

private ICommand showTodoListPageCommand;
public ICommand ShowTodoListPageCommand
{
    get
    {
        return showTodoListPageCommand ??= new RelayCommand(obj =>
        {
            CurrentContent = _todoListContent; // 原来的待办列表
        });
    }
}
```

##### 1.4 菜单按钮添加
**修改**: `MainWindow.xaml`

在左侧菜单添加历史记录按钮：
```xaml
<StackPanel Orientation="Vertical" HorizontalAlignment="Left">
    <!-- 现有按钮 -->
    <Button Content="待办列表" Command="{Binding ShowTodoListPageCommand}"/>
    <Button Content="历史记录" Command="{Binding ShowHistoryPageCommand}"/>
    <!-- 其他按钮 -->
</StackPanel>
```

##### 1.5 移除独立窗口
**修改**: `ViewModels/MainWindowViewModel.cs`

移除或禁用 `ShowHistoryCommand`（弹窗版本）：
```csharp
// 旧代码（注释掉或移除）
// private void ShowHistory(object? parameter)
// {
//     var historyWindow = new HistoryWindow();
//     historyWindow.ShowDialog();
// }

// 新代码（使用内嵌页面）
private void ShowHistory(object? parameter)
{
    CurrentContent = new HistoryUserControl();
}
```

#### 实现步骤

**步骤 1**: UI 结构准备（20分钟）
- [x] 修改 `MainWindow.xaml` 主内容区
- [x] 添加 `ContentControl` 容器
- [x] 添加页面切换按钮

**步骤 2**: 创建 UserControl（30分钟）
- [x] 创建 `Views/HistoryUserControl.xaml/.cs`
- [x] 复制 `HistoryWindow.xaml` 内容
- [x] 调整布局适配内嵌模式

**步骤 3**: ViewModel 逻辑（30分钟）
- [x] 添加 `CurrentContent` 属性
- [x] 实现页面切换命令
- [x] 更新 `ShowHistoryCommand`

**步骤 4**: 测试验证（30分钟）
- [x] 编译测试
- [x] 页面切换测试
- [x] 历史记录功能测试
- [x] 回归测试

**步骤 5**: 文档更新（30分钟）
- [x] 功能文档
- [x] 会话记录
- [x] 测试报告

#### 预期成果
- ? 历史记录以内嵌页面方式显示
- ? 可以在待办列表和历史记录之间切换
- ? 保持原有所有功能
- ? 用户体验提升

#### 技术要点
1. **ContentControl** - WPF 页面切换容器
2. **UserControl** - 可复用的用户控件
3. **ViewModel 复用** - 复用现有逻辑
4. **布局适配** - 窗口到内嵌页面的布局调整

---

### 阶段 2: 日历视图功能 ?? 预计 4-5小时

#### 功能描述
新增日历视图，按日期展示待办项，支持日期筛选和快速导航。

#### 功能需求

##### 2.1 日历视图布局
```
┌─────────────────────────────────────────┐
│  2025年1月                      ? ?    │
├─────────────────────────────────────────┤
│ 周日  周一  周二  周三  周四  周五  周六  │
├─────┬─────┬─────┬─────┬─────┬─────┬─────┤
│     │     │  1  │  2  │  3  │  4  │  5  │
│     │     │ [2] │     │     │     │     │ <- [2] 表示有2个待办项
├─────┼─────┼─────┼─────┼─────┼─────┼─────┤
│  6  │  7  │  8  │  9  │ 10  │ 11  │ 12  │
│     │ [1] │     │     │     │     │     │
├─────┼─────┼─────┼─────┼─────┼─────┼─────┤
│ ... │     │     │     │     │     │     │
└─────┴─────┴─────┴─────┴─────┴─────┴─────┘

今日待办（2025-01-02）：
  ? 完成项目文档
  ? 代码审查
  ...
```

##### 2.2 功能特性
- ? **月视图** - 显示整月日历
- ? **待办数量** - 每日显示待办项数量
- ? **日期筛选** - 点击日期查看该日待办
- ? **月份切换** - 上一月/下一月导航
- ? **今日高亮** - 当前日期特殊标识
- ? **快速跳转** - 点击日期跳转到待办列表

#### 实现方案

##### 2.1 创建日历控件
**新建**: `Views/CalendarViewControl.xaml/.cs`

```xaml
<UserControl x:Class="SceneTodo.Views.CalendarViewControl"
             xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    <Grid>
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/> <!-- 月份导航 -->
            <RowDefinition Height="Auto"/> <!-- 星期标题 -->
            <RowDefinition Height="*"/>    <!-- 日期网格 -->
            <RowDefinition Height="Auto"/> <!-- 今日待办 -->
        </Grid.RowDefinitions>
        
        <!-- 月份导航 -->
        <StackPanel Grid.Row="0" Orientation="Horizontal" HorizontalAlignment="Center">
            <Button Content="?" Command="{Binding PreviousMonthCommand}"/>
            <TextBlock Text="{Binding CurrentMonthText}" Margin="20,0"/>
            <Button Content="?" Command="{Binding NextMonthCommand}"/>
        </StackPanel>
        
        <!-- 星期标题 -->
        <ItemsControl Grid.Row="1" ItemsSource="{Binding WeekDays}">
            <ItemsControl.ItemsPanel>
                <ItemsPanelTemplate>
                    <UniformGrid Rows="1" Columns="7"/>
                </ItemsPanelTemplate>
            </ItemsControl.ItemsPanel>
            <ItemsControl.ItemTemplate>
                <DataTemplate>
                    <TextBlock Text="{Binding}" HorizontalAlignment="Center"/>
                </DataTemplate>
            </ItemsControl.ItemTemplate>
        </ItemsControl>
        
        <!-- 日期网格 -->
        <ItemsControl Grid.Row="2" ItemsSource="{Binding CalendarDays}">
            <ItemsControl.ItemsPanel>
                <ItemsPanelTemplate>
                    <UniformGrid Rows="6" Columns="7"/>
                </ItemsPanelTemplate>
            </ItemsControl.ItemsPanel>
            <ItemsControl.ItemTemplate>
                <DataTemplate>
                    <Border BorderBrush="Gray" BorderThickness="1">
                        <Button Command="{Binding DataContext.SelectDateCommand, 
                                RelativeSource={RelativeSource AncestorType=ItemsControl}}"
                                CommandParameter="{Binding Date}">
                            <StackPanel>
                                <TextBlock Text="{Binding Day}"/>
                                <TextBlock Text="{Binding TodoCount, StringFormat='[{0}]'}"
                                          Visibility="{Binding HasTodos, Converter={StaticResource BoolToVisibilityConverter}}"/>
                            </StackPanel>
                        </Button>
                    </Border>
                </DataTemplate>
            </ItemsControl.ItemTemplate>
        </ItemsControl>
        
        <!-- 今日待办 -->
        <StackPanel Grid.Row="3">
            <TextBlock Text="{Binding SelectedDateText}"/>
            <ListBox ItemsSource="{Binding SelectedDateTodos}">
                <!-- 待办项显示模板 -->
            </ListBox>
        </StackPanel>
    </Grid>
</UserControl>
```

##### 2.2 创建 ViewModel
**新建**: `ViewModels/CalendarViewModel.cs`

```csharp
public class CalendarViewModel : INotifyPropertyChanged
{
    private DateTime currentMonth;
    private DateTime selectedDate;
    
    public ObservableCollection<CalendarDay> CalendarDays { get; set; }
    public ObservableCollection<TodoItemModel> SelectedDateTodos { get; set; }
    
    public string CurrentMonthText => currentMonth.ToString("yyyy年M月");
    public string SelectedDateText => selectedDate.ToString("yyyy-MM-dd 待办");
    
    public ICommand PreviousMonthCommand { get; }
    public ICommand NextMonthCommand { get; }
    public ICommand SelectDateCommand { get; }
    
    public CalendarViewModel()
    {
        currentMonth = DateTime.Now;
        selectedDate = DateTime.Now;
        
        PreviousMonthCommand = new RelayCommand(_ => ChangeMonth(-1));
        NextMonthCommand = new RelayCommand(_ => ChangeMonth(1));
        SelectDateCommand = new RelayCommand(SelectDate);
        
        LoadCalendar();
    }
    
    private void LoadCalendar()
    {
        CalendarDays = new ObservableCollection<CalendarDay>();
        
        // 获取当月第一天是星期几
        var firstDay = new DateTime(currentMonth.Year, currentMonth.Month, 1);
        var startDay = firstDay.AddDays(-(int)firstDay.DayOfWeek);
        
        // 生成6周 x 7天 = 42天
        for (int i = 0; i < 42; i++)
        {
            var date = startDay.AddDays(i);
            var todosOnDate = GetTodosForDate(date);
            
            CalendarDays.Add(new CalendarDay
            {
                Date = date,
                Day = date.Day,
                IsCurrentMonth = date.Month == currentMonth.Month,
                IsToday = date.Date == DateTime.Today,
                TodoCount = todosOnDate.Count,
                HasTodos = todosOnDate.Any()
            });
        }
    }
    
    private List<TodoItemModel> GetTodosForDate(DateTime date)
    {
        // 从主数据源获取指定日期的待办项
        var allTodos = App.MainViewModel?.Model?.TodoItems;
        if (allTodos == null) return new List<TodoItemModel>();
        
        return GetAllTodosFlat(allTodos)
            .Where(t => IsTodoOnDate(t, date))
            .ToList();
    }
    
    private bool IsTodoOnDate(TodoItemModel todo, DateTime date)
    {
        // 检查待办项是否在指定日期
        if (todo.StartTime?.Date == date.Date) return true;
        if (todo.EndTime?.Date == date.Date) return true;
        if (todo.ReminderTime?.Date == date.Date) return true;
        if (todo.GreadtedAt?.Date == date.Date) return true;
        
        return false;
    }
    
    private void ChangeMonth(int offset)
    {
        currentMonth = currentMonth.AddMonths(offset);
        LoadCalendar();
        OnPropertyChanged(nameof(CurrentMonthText));
    }
    
    private void SelectDate(object parameter)
    {
        if (parameter is DateTime date)
        {
            selectedDate = date;
            SelectedDateTodos = new ObservableCollection<TodoItemModel>(GetTodosForDate(date));
            OnPropertyChanged(nameof(SelectedDateText));
            OnPropertyChanged(nameof(SelectedDateTodos));
        }
    }
}

public class CalendarDay
{
    public DateTime Date { get; set; }
    public int Day { get; set; }
    public bool IsCurrentMonth { get; set; }
    public bool IsToday { get; set; }
    public int TodoCount { get; set; }
    public bool HasTodos { get; set; }
}
```

##### 2.3 集成到主窗口
**修改**: `MainWindow.xaml`

添加日历视图按钮：
```xaml
<Button Content="日历视图" Command="{Binding ShowCalendarViewCommand}"/>
```

**修改**: `ViewModels/MainWindowViewModel.cs`

添加命令：
```csharp
public ICommand ShowCalendarViewCommand
{
    get
    {
        return showCalendarViewCommand ??= new RelayCommand(obj =>
        {
            CurrentContent = new CalendarViewControl();
        });
    }
}
```

#### 实现步骤

**步骤 1**: 数据模型准备（30分钟）
- [x] 创建 `CalendarDay` 模型
- [x] 设计日期筛选逻辑
- [x] 准备测试数据

**步骤 2**: ViewModel 实现（1小时）
- [x] 创建 `CalendarViewModel.cs`
- [x] 实现日历生成逻辑
- [x] 实现月份切换
- [x] 实现日期选择
- [x] 实现待办项筛选

**步骤 3**: UI 控件开发（1.5小时）
- [x] 创建 `CalendarViewControl.xaml/.cs`
- [x] 实现月份导航
- [x] 实现日期网格
- [x] 实现待办项显示
- [x] 样式美化

**步骤 4**: 集成主窗口（30分钟）
- [x] 添加日历视图按钮
- [x] 实现页面切换
- [x] 测试导航

**步骤 5**: 功能测试（1小时）
- [x] 编译测试
- [x] 日历显示测试
- [x] 月份切换测试
- [x] 日期筛选测试
- [x] 待办项显示测试

**步骤 6**: 文档更新（30分钟）
- [x] 功能文档
- [x] 会话记录
- [x] 测试报告

#### 预期成果
- ? 日历视图显示整月日历
- ? 每日显示待办项数量
- ? 可以切换月份
- ? 可以选择日期查看待办
- ? 今日高亮显示

#### 技术要点
1. **UniformGrid** - 日历网格布局
2. **日期计算** - 月初星期几、天数计算
3. **数据筛选** - 根据日期筛选待办项
4. **命令绑定** - RelativeSource 绑定
5. **样式设计** - 日历样式美化

---

## ?? 时间预估

### 总体时间
| 阶段 | 功能 | 预计时间 | 说明 |
|------|------|---------|------|
| 阶段1 | 历史记录内嵌页面 | 2-3小时 | UI改造 + 页面切换 |
| 阶段2 | 日历视图功能 | 4-5小时 | 新建控件 + 日期逻辑 |
| **总计** | | **6-8小时** | 分两次会话或一次完成 |

### 建议执行方式

#### 方案 A: 单次完成（推荐）
```
Session-06 (6-8小时):
├─ 阶段1: 历史记录内嵌页面 (2-3小时)
├─ 中间验证和休息 (30分钟)
└─ 阶段2: 日历视图功能 (4-5小时)
```

#### 方案 B: 分次完成
```
Session-06-Part1 (2-3小时):
└─ 阶段1: 历史记录内嵌页面

Session-06-Part2 (4-5小时):
└─ 阶段2: 日历视图功能
```

---

## ?? 执行检查清单

### 阶段 1 检查清单

#### UI 结构
- [ ] MainWindow.xaml 主内容区改造完成
- [ ] ContentControl 容器添加
- [ ] 页面切换按钮添加

#### UserControl
- [ ] HistoryUserControl.xaml/.cs 创建
- [ ] 复用 HistoryWindowViewModel
- [ ] 布局适配内嵌模式

#### ViewModel
- [ ] CurrentContent 属性添加
- [ ] ShowHistoryPageCommand 实现
- [ ] ShowTodoListPageCommand 实现

#### 测试
- [ ] 编译通过
- [ ] 页面切换正常
- [ ] 历史记录功能正常
- [ ] 无回归问题

#### 文档
- [ ] 功能文档创建
- [ ] 会话记录创建
- [ ] 测试报告创建

### 阶段 2 检查清单

#### 数据模型
- [ ] CalendarDay 模型创建
- [ ] 日期筛选逻辑实现
- [ ] 测试数据准备

#### ViewModel
- [ ] CalendarViewModel.cs 创建
- [ ] 日历生成逻辑实现
- [ ] 月份切换实现
- [ ] 日期选择实现
- [ ] 待办项筛选实现

#### UI 控件
- [ ] CalendarViewControl.xaml/.cs 创建
- [ ] 月份导航实现
- [ ] 日期网格实现
- [ ] 待办项显示实现
- [ ] 样式美化完成

#### 集成
- [ ] 日历视图按钮添加
- [ ] 页面切换实现
- [ ] 导航测试通过

#### 测试
- [ ] 编译通过
- [ ] 日历显示正常
- [ ] 月份切换正常
- [ ] 日期筛选正常
- [ ] 待办项显示正常

#### 文档
- [ ] 功能文档创建
- [ ] 会话记录创建
- [ ] 测试报告创建

---

## ?? 成功标准

### 代码质量
- ? 编译: 0错误 0警告
- ? 代码分析: 100%通过
- ? 命名规范: 统一一致
- ? 注释完整: 关键逻辑有注释

### 功能完整性
- ? 阶段1: 历史记录内嵌页面100%完成
- ? 阶段2: 日历视图功能100%完成
- ? 页面切换: 流畅无卡顿
- ? 数据准确: 日历和待办项准确显示

### 用户体验
- ? 界面美观: 符合整体风格
- ? 操作流畅: 响应及时
- ? 逻辑清晰: 功能易于理解
- ? 无Bug: 主流程无明显问题

### 文档质量
- ? 功能文档: 完整详细
- ? 会话记录: 清晰准确
- ? 测试报告: 覆盖全面
- ? 代码示例: 可运行可参考

---

## ?? 技术参考

### WPF 相关
1. **ContentControl** - 内容容器控件
   - 用途: 页面切换
   - 文档: https://learn.microsoft.com/zh-cn/dotnet/api/system.windows.controls.contentcontrol

2. **UserControl** - 用户控件
   - 用途: 可复用的自定义控件
   - 文档: https://learn.microsoft.com/zh-cn/dotnet/api/system.windows.controls.usercontrol

3. **UniformGrid** - 均匀网格
   - 用途: 日历网格布局
   - 文档: https://learn.microsoft.com/zh-cn/dotnet/api/system.windows.controls.primitives.uniformgrid

4. **ItemsControl** - 项集合控件
   - 用途: 动态生成日期单元格
   - 文档: https://learn.microsoft.com/zh-cn/dotnet/api/system.windows.controls.itemscontrol

### MVVM 模式
1. **Command** - 命令模式
2. **INotifyPropertyChanged** - 属性通知
3. **RelayCommand** - 命令实现

### 日期处理
1. **DateTime** - 日期时间类
2. **DayOfWeek** - 星期枚举
3. **AddDays/AddMonths** - 日期计算

---

## ?? 执行指令

### 开始执行
```bash
# AI Agent 执行命令
开始 Session-06 开发
- 读取规划文档: Doc/06-规划文档/Session-06-执行规划.md
- 执行模式: 阶段性单功能
- 验证模式: 每阶段后立即验证
- 文档模式: 功能完成后同步更新
```

### 阶段执行
```bash
# 执行阶段1
执行 Session-06 阶段1
- 功能: 历史记录内嵌页面
- 预计时间: 2-3小时
- 验证: 完成后立即编译和测试

# 执行阶段2
执行 Session-06 阶段2
- 功能: 日历视图功能
- 预计时间: 4-5小时
- 验证: 完成后立即编译和测试
```

---

## ?? 问题处理

### 遇到问题时
1. ?? 查看相关文档和代码
2. ?? 分析问题根本原因
3. ?? 提出解决方案
4. ? 验证解决方案
5. ?? 记录问题和解决过程

### 需要帮助时
- 检查项目文档
- 查看历史会话记录
- 参考技术文档
- 查看类似实现

---

## ?? 预期成果

### 阶段 1 成果
- ? 历史记录以内嵌页面方式显示
- ? 主窗口左侧有页面切换按钮
- ? 可以在不同页面之间流畅切换
- ? 所有原有功能保持不变

### 阶段 2 成果
- ? 日历视图显示整月日历
- ? 每日显示待办项数量
- ? 可以切换月份查看
- ? 可以选择日期查看待办
- ? 今日有特殊标识

### 整体成果
- ? 两个 P1 功能完成
- ? 用户体验提升
- ? 代码质量优秀
- ? 文档完整清晰

---

**规划版本**: 1.0  
**创建日期**: 2025-01-02 20:50:00  
**目标会话**: Session-06  
**执行模式**: 阶段性单功能开发  
**预计时长**: 6-8小时

**准备开始 Session-06！** ??
