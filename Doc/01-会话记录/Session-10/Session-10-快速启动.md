# ?? Session-10 快速启动指南

> **当前会话**: Session-10  
> **任务**: 截止时间功能  
> **预计时间**: 3-4小时  
> **优先级**: ?????

---

## ? 前置准备

### 1. 文档已同步 ?
- [x] Session-07/08/09 文档已提交
- [x] PRD 对比分析已完成
- [x] 开发路线图已创建
- [x] Session-10 规划已完成

### 2. 代码状态 ?
- [x] 所有更改已提交到 Git
- [x] main 分支已推送到远程
- [x] 构建成功无错误

### 3. 参考文档 ?
- [Session-10 详细规划](./Session-10-截止时间功能规划.md)
- [开发路线图](./开发路线图-v1.0.md)
- [PRD 对比分析](../02-功能文档/PRD功能实现对比分析报告.md)

---

## ?? Session-10 目标

### 核心功能
1. ? **数据模型**: 添加 DueDate 属性
2. ? **数据库**: 迁移支持截止时间
3. ? **编辑界面**: 日期时间选择器
4. ? **显示界面**: 截止时间展示和过期提示
5. ? **提醒功能**: 集成定时任务系统
6. ? **日历集成**: 按截止日期显示

---

## ?? 实施清单

### Step 1: 数据模型和数据库 (1h)

#### 1.1 修改 TodoItemModel.cs
```csharp
// 添加以下属性
private DateTime? dueDate;
public DateTime? DueDate { get; set; }
public bool IsOverdue { get; }
public string DueDateDisplay { get; }
```

#### 1.2 修改 DatabaseInitializer.cs
```csharp
// 添加数据库迁移
private async Task MigrateDueDateColumn()
{
    // 检查并添加 DueDate 列
}
```

#### 1.3 测试
- [ ] 启动应用，检查数据库迁移
- [ ] 验证旧数据兼容性

---

### Step 2: 编辑界面 (1h)

#### 2.1 修改 EditTodoItemWindow.xaml
```xaml
<hc:DateTimePicker x:Name="DueDatePicker"
                  SelectedDateTime="{Binding DueDate, Mode=TwoWay}"/>
<Button Content="清除" Click="ClearDueDate_Click"/>
```

#### 2.2 修改 EditTodoItemWindow.xaml.cs
```csharp
private void ClearDueDate_Click(object sender, RoutedEventArgs e)
{
    DueDatePicker.SelectedDateTime = null;
}
```

#### 2.3 测试
- [ ] 创建待办并设置截止时间
- [ ] 编辑待办并修改截止时间
- [ ] 清除截止时间按钮工作正常

---

### Step 3: 显示界面 (1h)

#### 3.1 创建 OverdueToBrushConverter.cs
```csharp
public class OverdueToBrushConverter : IValueConverter
{
    public object Convert(object value, ...)
    {
        return (bool)value ? Brushes.Red : Brushes.Gray;
    }
}
```

#### 3.2 修改 TodoItemControl.xaml
```xaml
<StackPanel Orientation="Horizontal">
    <iconPacks:PackIconFontAwesome Kind="ClockRegular"/>
    <TextBlock Text="{Binding DueDateDisplay}"/>
</StackPanel>
```

#### 3.3 在 App.xaml 注册转换器
```xaml
<Application.Resources>
    <converters:OverdueToBrushConverter x:Key="OverdueToBrushConverter"/>
</Application.Resources>
```

#### 3.4 测试
- [ ] 待办列表正确显示截止时间
- [ ] 过期待办显示为红色
- [ ] 相对时间正确（今天、明天、X天后）

---

### Step 4: 提醒和集成 (1h)

#### 4.1 添加截止时间提醒
在 `MainWindowViewModel.cs` 或创建新服务：

```csharp
private async Task SetupDueDateReminders()
{
    foreach (var todo in Model.TodoItems)
    {
        if (todo.DueDate.HasValue && !todo.IsCompleted)
        {
            await CreateDueDateReminder(todo);
        }
    }
}
```

#### 4.2 修改 CalendarViewModel.cs
```csharp
// 按截止日期添加待办到日历
if (item.DueDate.HasValue)
{
    var day = Days.FirstOrDefault(d => 
        d.Date.Date == item.DueDate.Value.Date);
    if (day != null)
    {
        day.TodoItems.Add(item);
    }
}
```

#### 4.3 测试
- [ ] 截止前收到提醒通知
- [ ] 日历视图按截止日期显示
- [ ] 过期提醒正常工作

---

## ?? 测试清单

### 功能测试
- [ ] 可以设置截止时间
- [ ] 可以清除截止时间
- [ ] 截止时间正确保存
- [ ] 截止时间正确加载
- [ ] 过期待办显示红色
- [ ] 相对时间正确计算
- [ ] 日历视图正确显示
- [ ] 提醒功能正常工作

### 边界测试
- [ ] NULL 截止时间正常处理
- [ ] 过去时间可以设置
- [ ] 跨天跨月计算正确
- [ ] 已完成待办不显示过期

### UI 测试
- [ ] 日期选择器正常工作
- [ ] 清除按钮正常工作
- [ ] 颜色显示正确
- [ ] 布局不错乱

---

## ?? 需要的文件

### 需要修改
1. `Models/TodoItemModel.cs`
2. `Services/Database/DatabaseInitializer.cs`
3. `Views/EditTodoItemWindow.xaml`
4. `Views/EditTodoItemWindow.xaml.cs`
5. `Views/TodoItemControl.xaml`
6. `ViewModels/CalendarViewModel.cs`
7. `App.xaml`

### 需要创建
1. `Converters/OverdueToBrushConverter.cs`

---

## ?? 常见问题

### Q1: HandyControl 的 DateTimePicker 在哪？
**A**: HandyControl 提供 `hc:DatePicker` 和 `hc:TimePicker`，可以组合使用或使用 `hc:DateTimePicker`

### Q2: 数据库迁移失败怎么办？
**A**: 
1. 检查 SQLite 语法
2. 使用 `PRAGMA table_info(TodoItems)` 检查列
3. 如果失败，删除数据库重新创建

### Q3: 时间计算不准确？
**A**: 
1. 确保使用 `DateTime.Now` 而非 `DateTime.UtcNow`
2. 使用 `.Date` 进行日期比较
3. 注意时区问题

### Q4: 颜色转换器不生效？
**A**: 
1. 检查是否在 App.xaml 中注册
2. 检查 Binding 路径是否正确
3. 检查转换器返回值类型

---

## ?? 提示

### 开发顺序建议
1. **先做数据层** → 模型和数据库
2. **再做编辑** → 确保可以保存
3. **然后显示** → 验证数据正确
4. **最后集成** → 提醒和日历

### 调试技巧
1. 使用 `Debug.WriteLine` 输出关键信息
2. 在 `DueDateDisplay` 中断点查看计算
3. 用简单的测试数据验证

### 代码风格
1. 遵循现有代码风格
2. 添加适当的注释
3. 使用有意义的变量名

---

## ?? 预期成果

### 功能完成
- ? 可以设置和显示截止时间
- ? 过期待办醒目提示
- ? 集成到日历视图
- ? 提醒功能工作

### 代码质量
- ? 无编译错误
- ? 无运行时异常
- ? 数据正确持久化
- ? UI 响应流畅

### 文档完成
- ? Session-10 完成总结
- ? 功能使用文档
- ? 代码注释完整

---

## ?? 参考链接

- [HandyControl 文档](https://handyorg.github.io/handycontrol/)
- [SQLite 日期时间函数](https://www.sqlite.org/lang_datefunc.html)
- [C# DateTime 文档](https://docs.microsoft.com/en-us/dotnet/api/system.datetime)

---

## ? 完成标准

当以下所有项都完成时，Session-10 才算完成：

- [ ] 所有功能测试通过
- [ ] 所有边界测试通过
- [ ] 代码无编译错误
- [ ] 构建成功
- [ ] 更改已提交到 Git
- [ ] Session-10 完成总结已创建
- [ ] 路线图已更新

---

**准备好了吗？开始 Session-10 开发！** ??

**提示**: 从 Step 1 开始，一步一步完成，不要跳过测试！
