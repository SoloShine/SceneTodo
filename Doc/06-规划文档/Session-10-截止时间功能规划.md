# Session-10 开发规划：截止时间功能

> **规划日期**: 2025-01-02  
> **预计工作量**: 3-4小时  
> **优先级**: ????? P0  
> **目标**: 实现待办事项的截止时间功能

---

## ?? 功能概述

根据 PRD 功能对比分析，**截止时间功能**是 P0 功能中的重大缺失项，影响评级为 ?????（严重）。

**PRD 要求**:
> 支持为待办事项设置标题、详情描述、**截止时间**、优先级（高/中/低）

**当前状态**: ? 未实现（0%）

---

## ?? 功能需求

### 核心功能

1. **数据模型扩展**
   - TodoItemModel 添加 DueDate 属性
   - 支持可空 DateTime（允许不设置截止时间）
   - 数据库迁移

2. **UI 界面**
   - 编辑窗口添加日期时间选择器
   - 待办列表显示截止时间
   - 日历视图按截止日期显示
   - 过期待办特殊标识

3. **提醒功能**
   - 截止前提醒（可配置提醒时间）
   - 过期提醒
   - 集成定时任务系统

4. **筛选和排序**
   - 按截止时间排序
   - 过期/即将到期筛选
   - 日历视图集成

---

## ?? 技术方案

### 1. 数据模型 (0.5小时)

#### TodoItemModel.cs
```csharp
public class TodoItemModel : INotifyPropertyChanged
{
    // ...existing properties...
    
    private DateTime? dueDate;
    /// <summary>
    /// 截止时间
    /// </summary>
    public DateTime? DueDate
    {
        get => dueDate;
        set
        {
            dueDate = value;
            OnPropertyChanged(nameof(DueDate));
            OnPropertyChanged(nameof(IsOverdue));
            OnPropertyChanged(nameof(DueDateDisplay));
        }
    }
    
    /// <summary>
    /// 是否已过期
    /// </summary>
    public bool IsOverdue => DueDate.HasValue && !IsCompleted && DueDate.Value < DateTime.Now;
    
    /// <summary>
    /// 截止时间显示文本
    /// </summary>
    public string DueDateDisplay
    {
        get
        {
            if (!DueDate.HasValue) return "无截止时间";
            
            var days = (DueDate.Value.Date - DateTime.Now.Date).Days;
            
            if (days < 0)
                return $"已过期 {Math.Abs(days)} 天";
            else if (days == 0)
                return "今天截止";
            else if (days == 1)
                return "明天截止";
            else if (days <= 7)
                return $"{days} 天后截止";
            else
                return DueDate.Value.ToString("yyyy-MM-dd");
        }
    }
}
```

#### 数据库迁移
```csharp
// 在 DatabaseInitializer.cs 中添加迁移逻辑
private async Task MigrateDueDateColumn()
{
    try
    {
        // 检查列是否存在
        var sql = "PRAGMA table_info(TodoItems)";
        var columns = await _context.Database.ExecuteSqlRawAsync(sql);
        
        // 如果不存在则添加
        if (!ColumnExists("DueDate"))
        {
            await _context.Database.ExecuteSqlRawAsync(
                "ALTER TABLE TodoItems ADD COLUMN DueDate TEXT NULL");
        }
    }
    catch (Exception ex)
    {
        Debug.WriteLine($"DueDate migration failed: {ex.Message}");
    }
}
```

### 2. UI 界面 (1.5小时)

#### EditTodoItemWindow.xaml
```xaml
<!-- 在编辑窗口添加日期时间选择器 -->
<StackPanel Margin="0,8,0,0">
    <TextBlock Text="截止时间:" Margin="0,0,0,4"/>
    <Grid>
        <Grid.ColumnDefinitions>
            <ColumnDefinition Width="*"/>
            <ColumnDefinition Width="Auto"/>
        </Grid.ColumnDefinitions>
        
        <hc:DateTimePicker x:Name="DueDatePicker"
                          Grid.Column="0"
                          SelectedDateTime="{Binding DueDate, Mode=TwoWay}"
                          hc:InfoElement.Placeholder="选择截止时间（可选）"/>
        
        <Button Grid.Column="1"
                Content="清除"
                Margin="8,0,0,0"
                Click="ClearDueDate_Click"
                Style="{StaticResource ButtonDefault}"/>
    </Grid>
</StackPanel>
```

#### TodoItemControl.xaml
```xaml
<!-- 显示截止时间 -->
<StackPanel Orientation="Horizontal" Margin="0,4,0,0">
    <iconPacks:PackIconFontAwesome Kind="ClockRegular"
                                  Width="12"
                                  Height="12"
                                  Foreground="{Binding IsOverdue, Converter={StaticResource OverdueToBrushConverter}}"
                                  Visibility="{Binding DueDate, Converter={StaticResource NullableToVisibilityConverter}}"/>
    <TextBlock Text="{Binding DueDateDisplay}"
              Margin="4,0,0,0"
              FontSize="11"
              Foreground="{Binding IsOverdue, Converter={StaticResource OverdueToBrushConverter}}"
              Visibility="{Binding DueDate, Converter={StaticResource NullableToVisibilityConverter}}"/>
</StackPanel>
```

#### 颜色转换器
```csharp
// Converters/OverdueToBrushConverter.cs
public class OverdueToBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isOverdue)
        {
            return isOverdue 
                ? new SolidColorBrush(Colors.Red) 
                : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF888888"));
        }
        return Brushes.Gray;
    }
    
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
```

### 3. 提醒功能 (1小时)

#### 创建截止时间提醒任务
```csharp
// 在 MainWindowViewModel.cs 或新建 DueDateReminderService.cs
private async Task SetupDueDateReminders()
{
    foreach (var todo in Model.TodoItems)
    {
        if (todo.DueDate.HasValue && !todo.IsCompleted)
        {
            // 创建截止前提醒任务
            var reminderTask = new AutoTask
            {
                Id = $"duedate_reminder_{todo.Id}",
                Name = $"截止提醒: {todo.Content}",
                Description = $"待办 '{todo.Content}' 即将到期",
                TodoItemId = todo.Id,
                IsEnabled = true,
                ActionType = TaskActionType.Notification,
                // 提前1小时提醒
                Cron = GetCronForDateTime(todo.DueDate.Value.AddHours(-1))
            };
            
            await App.SchedulerService.ScheduleAutoTask(reminderTask);
        }
    }
}

private string GetCronForDateTime(DateTime dt)
{
    return $"{dt.Second} {dt.Minute} {dt.Hour} {dt.Day} {dt.Month} ? {dt.Year}";
}
```

### 4. 日历视图集成 (0.5小时)

#### CalendarViewModel.cs
```csharp
private void LoadTodoItems()
{
    var allItems = App.TodoItemRepository.GetAllAsync().Result;
    
    foreach (var item in allItems)
    {
        // 按创建日期添加
        if (item.CreatedAt.HasValue)
        {
            AddTodoToCalendar(item.CreatedAt.Value.Date, item);
        }
        
        // 按截止日期添加（新增）
        if (item.DueDate.HasValue)
        {
            var day = Days.FirstOrDefault(d => d.Date.Date == item.DueDate.Value.Date);
            if (day != null && !day.TodoItems.Contains(item))
            {
                day.TodoItems.Add(item);
                day.HasDueTodos = true; // 新增标记
            }
        }
    }
}
```

---

## ??? 文件清单

### 需要修改的文件

1. **Models/TodoItemModel.cs**
   - 添加 DueDate 属性
   - 添加 IsOverdue 属性
   - 添加 DueDateDisplay 属性

2. **Services/Database/DatabaseInitializer.cs**
   - 添加数据库迁移逻辑

3. **Views/EditTodoItemWindow.xaml**
   - 添加日期时间选择器

4. **Views/EditTodoItemWindow.xaml.cs**
   - 添加清除截止时间事件处理

5. **Views/TodoItemControl.xaml**
   - 显示截止时间信息

6. **ViewModels/CalendarViewModel.cs**
   - 按截止日期显示待办

7. **Models/CalendarDay.cs**
   - 添加 HasDueTodos 属性

### 需要创建的文件

1. **Converters/OverdueToBrushConverter.cs**
   - 过期状态颜色转换器

2. **Services/DueDateReminderService.cs** (可选)
   - 截止时间提醒服务

---

## ? 验收标准

### 功能测试

- [ ] 可以在编辑窗口设置截止时间
- [ ] 可以清除截止时间
- [ ] 待办列表正确显示截止时间
- [ ] 过期待办显示为红色
- [ ] 即将到期显示相对时间（"今天"、"明天"、"3天后"）
- [ ] 日历视图按截止日期显示待办
- [ ] 截止前收到提醒通知
- [ ] 过期待办在顶部显示

### 数据测试

- [ ] 截止时间正确保存到数据库
- [ ] 截止时间正确从数据库加载
- [ ] 旧数据兼容（NULL 截止时间）
- [ ] 完成后不再显示过期

### UI测试

- [ ] 日期选择器正常工作
- [ ] 清除按钮正常工作
- [ ] 过期颜色正确显示
- [ ] 相对时间正确计算
- [ ] 响应式布局正常

---

## ?? 开发进度估算

| 任务 | 预计时间 | 难度 |
|-----|---------|------|
| 数据模型扩展 | 0.5h | ?? |
| 数据库迁移 | 0.5h | ?? |
| 编辑界面 | 1h | ??? |
| 显示界面 | 0.5h | ?? |
| 颜色转换器 | 0.5h | ?? |
| 提醒功能 | 1h | ???? |
| 日历集成 | 0.5h | ??? |
| 测试和调试 | 0.5h | ?? |
| **总计** | **3-4h** | ??? |

---

## ?? 实施步骤

### Step 1: 数据模型和数据库 (1h)

1. 修改 TodoItemModel.cs
2. 添加数据库迁移逻辑
3. 测试数据库迁移

### Step 2: 编辑界面 (1h)

1. 修改 EditTodoItemWindow.xaml
2. 添加事件处理
3. 测试日期选择

### Step 3: 显示界面 (1h)

1. 修改 TodoItemControl.xaml
2. 创建颜色转换器
3. 测试显示效果

### Step 4: 提醒和集成 (1h)

1. 实现提醒逻辑
2. 日历视图集成
3. 完整测试

---

## ?? 注意事项

### 技术要点

1. **时区处理**
   - 使用 DateTime.Now 比较当前时间
   - 存储时使用 ISO 8601 格式
   - 显示时转换为本地时间

2. **性能优化**
   - IsOverdue 使用计算属性，不存储
   - 批量创建提醒任务
   - 过期检查仅在必要时执行

3. **用户体验**
   - 截止时间可选，不强制
   - 清除按钮方便取消
   - 相对时间更友好
   - 过期提示明显但不打扰

### 潜在问题

1. **数据库迁移**
   - 需要处理现有数据
   - NULL 值兼容性

2. **提醒性能**
   - 大量待办时可能影响性能
   - 考虑批量处理

3. **时间计算**
   - 跨天跨月计算
   - 夏令时问题（中国无此问题）

---

## ?? 相关文档

- [PRD功能对比分析](../../02-功能文档/PRD功能实现对比分析报告.md)
- [PRD快速参考](../../02-功能文档/PRD对比分析-快速参考.md)
- [定时任务功能](../Session-07/Session-07完成总结.md)

---

## ?? 下一步计划

完成 Session-10 后：

**Session-11**: 分类标签系统 (3-4h) ????
**Session-12**: 数据备份恢复 (2-3h) ????

---

**规划版本**: 1.0  
**创建时间**: 2025-01-02  
**规划者**: SceneTodo 团队

**准备开始 Session-10 开发！** ??
