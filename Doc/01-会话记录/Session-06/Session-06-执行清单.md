# ? Session-06 执行清单

> **创建日期**: 2025-01-02 20:55:00  
> **执行模式**: 阶段性单功能  
> **预计时长**: 6-8小时

---

## ?? 两大功能

### 阶段 1: 历史记录内嵌页面 (2-3小时)
- [ ] 修改主窗口布局（ContentControl）
- [ ] 创建 HistoryUserControl
- [ ] 实现页面切换逻辑
- [ ] 添加菜单按钮
- [ ] 测试验证

### 阶段 2: 日历视图功能 (4-5小时)
- [ ] 创建 CalendarDay 模型
- [ ] 实现 CalendarViewModel
- [ ] 创建 CalendarViewControl
- [ ] 实现日历生成逻辑
- [ ] 实现日期筛选
- [ ] 测试验证

---

## ?? 详细清单

### 阶段 1: 历史记录内嵌页面

#### 步骤 1: UI 结构（20分钟）
- [ ] 修改 `MainWindow.xaml` 主内容区
- [ ] 添加 `ContentControl` 容器
- [ ] 添加左侧菜单按钮

#### 步骤 2: UserControl（30分钟）
- [ ] 新建 `Views/HistoryUserControl.xaml`
- [ ] 新建 `Views/HistoryUserControl.xaml.cs`
- [ ] 复制 HistoryWindow 的 XAML 内容
- [ ] 调整布局适配内嵌模式

#### 步骤 3: ViewModel（30分钟）
- [ ] 修改 `ViewModels/MainWindowViewModel.cs`
- [ ] 添加 `CurrentContent` 属性
- [ ] 添加 `ShowHistoryPageCommand`
- [ ] 添加 `ShowTodoListPageCommand`
- [ ] 更新 `ShowHistoryCommand`

#### 步骤 4: 测试（30分钟）
- [ ] 编译测试（0错误 0警告）
- [ ] 页面切换测试
- [ ] 历史记录功能测试
- [ ] 回归测试（原有功能）

#### 步骤 5: 文档（30分钟）
- [ ] 创建功能文档
- [ ] 创建会话记录
- [ ] 创建测试报告

---

### 阶段 2: 日历视图功能

#### 步骤 1: 数据模型（30分钟）
- [ ] 创建 `CalendarDay` 类
  - [ ] Date 属性
  - [ ] Day 属性
  - [ ] IsCurrentMonth 属性
  - [ ] IsToday 属性
  - [ ] TodoCount 属性
  - [ ] HasTodos 属性

#### 步骤 2: ViewModel（1小时）
- [ ] 新建 `ViewModels/CalendarViewModel.cs`
- [ ] 实现基础属性
  - [ ] CurrentMonth
  - [ ] SelectedDate
  - [ ] CalendarDays
  - [ ] SelectedDateTodos
- [ ] 实现命令
  - [ ] PreviousMonthCommand
  - [ ] NextMonthCommand
  - [ ] SelectDateCommand
- [ ] 实现核心方法
  - [ ] LoadCalendar() - 生成日历
  - [ ] GetTodosForDate() - 获取日期待办
  - [ ] IsTodoOnDate() - 判断待办是否在日期
  - [ ] ChangeMonth() - 切换月份
  - [ ] SelectDate() - 选择日期

#### 步骤 3: UI 控件（1.5小时）
- [ ] 新建 `Views/CalendarViewControl.xaml`
- [ ] 新建 `Views/CalendarViewControl.xaml.cs`
- [ ] 实现月份导航
  - [ ] 上一月按钮
  - [ ] 当前月份显示
  - [ ] 下一月按钮
- [ ] 实现星期标题
  - [ ] 周日-周六 7列
- [ ] 实现日期网格
  - [ ] UniformGrid 6行7列
  - [ ] 日期显示
  - [ ] 待办数量显示
  - [ ] 点击事件绑定
- [ ] 实现今日待办
  - [ ] 选中日期显示
  - [ ] 待办项列表
- [ ] 样式美化
  - [ ] 当前月份高亮
  - [ ] 今日特殊标识
  - [ ] 有待办的日期标识

#### 步骤 4: 集成（30分钟）
- [ ] 修改 `MainWindow.xaml`
  - [ ] 添加日历视图按钮
- [ ] 修改 `ViewModels/MainWindowViewModel.cs`
  - [ ] 添加 `ShowCalendarViewCommand`
- [ ] 测试页面切换

#### 步骤 5: 测试（1小时）
- [ ] 编译测试
- [ ] 日历显示测试
  - [ ] 月份正确
  - [ ] 日期正确
  - [ ] 星期对应正确
- [ ] 月份切换测试
  - [ ] 上一月
  - [ ] 下一月
  - [ ] 连续切换
- [ ] 日期选择测试
  - [ ] 点击日期
  - [ ] 待办项显示
  - [ ] 多个日期切换
- [ ] 待办项显示测试
  - [ ] 数量准确
  - [ ] 内容准确
  - [ ] 空日期处理
- [ ] 边界测试
  - [ ] 月初月末
  - [ ] 跨年
  - [ ] 今日标识

#### 步骤 6: 文档（30分钟）
- [ ] 创建功能文档
- [ ] 创建会话记录
- [ ] 创建测试报告

---

## ?? 验证标准

### 编译验证
- [ ] 0 编译错误
- [ ] 0 编译警告
- [ ] 代码分析通过

### 功能验证

#### 阶段 1
- [ ] 历史记录页面正常显示
- [ ] 可以切换到历史记录页面
- [ ] 可以切换回待办列表页面
- [ ] 历史记录所有功能正常
- [ ] 原有功能无回归问题

#### 阶段 2
- [ ] 日历正确显示当月
- [ ] 可以切换到上一月
- [ ] 可以切换到下一月
- [ ] 点击日期可以查看待办
- [ ] 待办数量显示准确
- [ ] 今日有特殊标识
- [ ] 页面切换流畅

---

## ?? 文件清单

### 新建文件

#### 阶段 1
```
Views/
└── HistoryUserControl.xaml         [新建]
    └── HistoryUserControl.xaml.cs  [新建]

Doc/01-会话记录/Session-06-Part1/
├── README.md                       [新建]
├── 阶段1完成报告.md                [新建]
└── 测试报告-历史记录内嵌.md        [新建]

Doc/02-功能文档/
└── 历史记录内嵌页面功能.md         [新建]
```

#### 阶段 2
```
Models/
└── CalendarDay.cs                  [新建]

ViewModels/
└── CalendarViewModel.cs            [新建]

Views/
└── CalendarViewControl.xaml        [新建]
    └── CalendarViewControl.xaml.cs [新建]

Doc/01-会话记录/Session-06-Part2/
├── README.md                       [新建]
├── 阶段2完成报告.md                [新建]
└── 测试报告-日历视图.md            [新建]

Doc/02-功能文档/
└── 日历视图功能.md                 [新建]
```

### 修改文件

#### 阶段 1
```
MainWindow.xaml                     [修改] 主内容区改造
ViewModels/MainWindowViewModel.cs  [修改] 添加页面切换
```

#### 阶段 2
```
MainWindow.xaml                     [修改] 添加日历按钮
ViewModels/MainWindowViewModel.cs  [修改] 添加日历命令
```

---

## ?? 快速开始

### 执行阶段 1
```bash
读取: Doc/06-规划文档/Session-06-执行规划.md
开始: 阶段1 - 历史记录内嵌页面
预计: 2-3小时
```

### 执行阶段 2
```bash
完成: 阶段1验证
开始: 阶段2 - 日历视图功能
预计: 4-5小时
```

---

## ?? 参考文档

### 规划文档
- [Session-06-执行规划.md](Session-06-执行规划.md) - 完整规划

### 历史会话
- [Session-05完成总结](../01-会话记录/Session-05-P0完成/Session-05完成总结.md)
- [项目状态总览](../00-必读/项目状态总览.md)

### 技术参考
- WPF ContentControl 文档
- WPF UserControl 文档
- WPF UniformGrid 文档

---

**执行清单版本**: 1.0  
**创建日期**: 2025-01-02 20:55:00  
**目标**: Session-06 两大功能  
**状态**: 准备就绪 ?
