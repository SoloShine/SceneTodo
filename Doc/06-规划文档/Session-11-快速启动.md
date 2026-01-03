# ?? Session-11 快速启动指南

> **当前会话**: Session-11  
> **任务**: 分类标签系统  
> **预计时间**: 3-4小时  
> **优先级**: ???? P0

---

## ? 前置准备

### 1. Session-10 已完成 ?
- [x] 截止时间功能已实现
- [x] 所有 Bug 已修复
- [x] 文档已归档
- [x] 代码已提交到 Git

### 2. 代码状态检查 ?
- [x] 所有更改已提交
- [x] main 分支已推送到远程
- [x] 构建成功无错误
- [x] 数据库正常运行

### 3. 参考文档准备 ?
- [Session-11 详细规划](./Session-11-分类标签系统规划.md)
- [开发路线图](./开发路线图-v1.0.md)
- [PRD 对比分析](../02-功能文档/PRD功能实现对比分析报告.md)

---

## ?? Session-11 目标

### 核心功能
1. ? **数据模型**: Tag 实体和多对多关系
2. ? **标签管理**: 创建/编辑/删除标签
3. ? **待办关联**: 为待办添加标签
4. ? **标签显示**: 列表中显示标签
5. ? **筛选功能**: 按标签筛选待办

---

## ?? 实施清单

### Step 1: 数据层开发 (1.5h)

#### 1.1 创建 Tag.cs
```bash
位置: Models/Tag.cs
```

**核心属性**:
- Id (string)
- Name (string)
- Color (string, 十六进制)
- ColorBrush (SolidColorBrush, 计算属性)
- CreatedAt (DateTime?)
- UsageCount (int, 统计用)

**测试点**:
- [ ] Tag 对象可以正常创建
- [ ] ColorBrush 正确转换颜色
- [ ] 属性变更触发通知

#### 1.2 创建 TodoItemTag.cs
```bash
位置: Models/TodoItemTag.cs
```

**核心属性**:
- Id (string)
- TodoItemId (string)
- TagId (string)
- CreatedAt (DateTime)

#### 1.3 修改 TodoItem.cs
```csharp
// 添加属性
public string TagsJson { get; set; } = "[]";
```

**测试点**:
- [ ] 新属性不影响现有功能
- [ ] 默认值为空数组 JSON

#### 1.4 修改 TodoItemModel.cs
```csharp
// 添加属性
public ObservableCollection<Tag> Tags { get; set; }
```

**实现逻辑**:
- 从 TagsJson 反序列化标签ID
- 从数据库加载 Tag 实体
- 支持双向更新

**测试点**:
- [ ] Tags 集合正确加载
- [ ] 修改 Tags 更新 TagsJson
- [ ] JSON 序列化正确

#### 1.5 创建 TagRepository.cs
```bash
位置: Services/Database/Repositories/TagRepository.cs
```

**必需方法**:
```csharp
Task<IEnumerable<Tag>> GetAllAsync()
Task<Tag?> GetByIdAsync(string id)
Task<int> AddAsync(Tag tag)
Task<int> UpdateAsync(Tag tag)
Task<int> DeleteAsync(string id)
Task<IEnumerable<Tag>> GetTagsForTodoAsync(string todoItemId)
Task<int> AddTagToTodoAsync(string todoItemId, string tagId)
Task<int> RemoveTagFromTodoAsync(string todoItemId, string tagId)
```

**测试点**:
- [ ] GetAllAsync 返回所有标签
- [ ] AddAsync 成功添加标签
- [ ] UpdateAsync 成功更新标签
- [ ] DeleteAsync 删除标签和关联
- [ ] GetTagsForTodoAsync 返回待办的标签
- [ ] AddTagToTodoAsync 建立关联
- [ ] RemoveTagFromTodoAsync 移除关联

#### 1.6 修改 TodoDbContext.cs
```csharp
// 添加 DbSet
public DbSet<Tag> Tags { get; set; }
public DbSet<TodoItemTag> TodoItemTags { get; set; }

// OnModelCreating 中配置
modelBuilder.Entity<Tag>(entity =>
{
    entity.HasKey(e => e.Id);
    entity.Property(e => e.Name).IsRequired();
});

modelBuilder.Entity<TodoItemTag>(entity =>
{
    entity.HasKey(e => e.Id);
    entity.HasIndex(e => new { e.TodoItemId, e.TagId }).IsUnique();
});
```

**测试点**:
- [ ] DbContext 正常编译
- [ ] 数据库迁移成功
- [ ] 表和索引创建正确

#### 1.7 数据库迁移
```csharp
// 在 DatabaseInitializer.cs 中添加
private async Task MigrateTagTables()
{
    // 创建 Tags 表
    // 创建 TodoItemTags 表
    // 为 TodoItems 添加 TagsJson 列
}
```

**测试点**:
- [ ] 启动应用自动迁移
- [ ] 表结构正确
- [ ] 旧数据兼容

#### 1.8 注册 TagRepository
```csharp
// 在 App.xaml.cs 中
public static TagRepository TagRepository { get; private set; }

// 在 OnStartup 中
TagRepository = new TagRepository(dbContext);
```

**测试点**:
- [ ] TagRepository 可访问
- [ ] 依赖注入正确

---

### Step 2: 标签管理界面 (1h)

#### 2.1 创建 TagManagementWindow.xaml
```bash
位置: Views/TagManagementWindow.xaml
```

**UI 组件**:
- DataGrid 显示标签列表
- 新建标签按钮
- 编辑/删除按钮
- 颜色预览列
- 使用次数列

**测试点**:
- [ ] 窗口可正常打开
- [ ] DataGrid 显示标签
- [ ] 按钮响应正常

#### 2.2 创建 TagManagementWindow.xaml.cs
```csharp
// 主要方法
private async void LoadTags()
private void AddTag_Click(object sender, RoutedEventArgs e)
private void EditTag_Click(object sender, RoutedEventArgs e)
private void DeleteTag_Click(object sender, RoutedEventArgs e)
```

**测试点**:
- [ ] 标签列表正确加载
- [ ] 新建标签成功
- [ ] 编辑标签成功
- [ ] 删除标签确认并成功
- [ ] 删除标签时清理关联

#### 2.3 创建 EditTagWindow.xaml
```bash
位置: Views/EditTagWindow.xaml
```

**UI 组件**:
- 标签名称输入框
- HandyControl ColorPicker
- 实时预览
- 保存/取消按钮

**测试点**:
- [ ] 窗口可正常打开
- [ ] ColorPicker 工作正常
- [ ] 预览实时更新
- [ ] 保存返回正确数据

#### 2.4 创建 EditTagWindow.xaml.cs
```csharp
public Tag? TagData { get; private set; }

private void Save_Click(object sender, RoutedEventArgs e)
{
    // 验证标签名称
    if (string.IsNullOrWhiteSpace(NameTextBox.Text))
    {
        MessageBox.Show("标签名称不能为空");
        return;
    }
    
    // 创建 Tag 对象
    TagData = new Tag
    {
        Name = NameTextBox.Text,
        Color = ColorPicker.SelectedBrush.ToString()
    };
    
    DialogResult = true;
    Close();
}
```

**测试点**:
- [ ] 验证标签名称
- [ ] 正确返回 Tag 数据
- [ ] 颜色正确保存

---

### Step 3: 待办标签集成 (1h)

#### 3.1 修改 EditTodoItemWindow.xaml
```xaml
<!-- 在时间设置分组后添加 -->
<Expander Header="??? 标签" IsExpanded="False">
    <StackPanel>
        <!-- 标签选择 ListBox -->
        <ListBox x:Name="TagsListBox"
                Height="150"
                SelectionMode="Multiple">
            <ListBox.ItemTemplate>
                <DataTemplate>
                    <Border Background="{Binding ColorBrush}"
                           CornerRadius="3"
                           Padding="8,4">
                        <TextBlock Text="{Binding Name}"
                                  Foreground="White"/>
                    </Border>
                </DataTemplate>
            </ListBox.ItemTemplate>
            <ListBox.ItemsPanel>
                <ItemsPanelTemplate>
                    <WrapPanel/>
                </ItemsPanelTemplate>
            </ListBox.ItemsPanel>
        </ListBox>
        
        <!-- 管理标签按钮 -->
        <Button Content="管理标签" Click="ManageTags_Click"/>
    </StackPanel>
</Expander>
```

**测试点**:
- [ ] 标签列表显示正确
- [ ] 可以多选标签
- [ ] 已选标签高亮显示
- [ ] 管理标签按钮工作

#### 3.2 修改 EditTodoItemWindow.xaml.cs
```csharp
// 初始化时加载标签
private async void InitializeTags()
{
    var allTags = await App.TagRepository.GetAllAsync();
    TagsListBox.ItemsSource = allTags;
    
    // 设置已选标签
    foreach (var tag in Todo.Tags)
    {
        var item = allTags.FirstOrDefault(t => t.Id == tag.Id);
        if (item != null)
        {
            TagsListBox.SelectedItems.Add(item);
        }
    }
}

// 保存时更新标签
private void SaveButton_Click(object sender, RoutedEventArgs e)
{
    // ...existing code...
    
    // 保存标签
    Todo.Tags = new ObservableCollection<Tag>(
        TagsListBox.SelectedItems.Cast<Tag>());
    
    DialogResult = true;
    Close();
}

// 管理标签
private void ManageTags_Click(object sender, RoutedEventArgs e)
{
    var window = new TagManagementWindow();
    if (window.ShowDialog() == true)
    {
        // 重新加载标签列表
        InitializeTags();
    }
}
```

**测试点**:
- [ ] 标签列表正确加载
- [ ] 已选标签正确显示
- [ ] 保存时标签正确更新
- [ ] 管理标签窗口打开正常

#### 3.3 修改 TodoItemControl.xaml
```xaml
<!-- 在第二行添加标签显示 -->
<ItemsControl ItemsSource="{Binding Tags}"
             Grid.Column="1"
             Grid.Row="1">
    <ItemsControl.ItemsPanel>
        <ItemsPanelTemplate>
            <StackPanel Orientation="Horizontal"/>
        </ItemsPanelTemplate>
    </ItemsControl.ItemsPanel>
    <ItemsControl.ItemTemplate>
        <DataTemplate>
            <Border Background="{Binding ColorBrush}"
                   CornerRadius="3"
                   Padding="5,2"
                   Margin="2">
                <TextBlock Text="{Binding Name}"
                          Foreground="White"
                          FontSize="10"
                          FontWeight="Bold"/>
            </Border>
        </DataTemplate>
    </ItemsControl.ItemTemplate>
</ItemsControl>
```

**测试点**:
- [ ] 标签正确显示
- [ ] 颜色显示正确
- [ ] 布局不错乱
- [ ] 没有标签时不显示

---

### Step 4: 筛选功能 (30分钟)

#### 4.1 修改 MainWindow.xaml
```xaml
<!-- 在工具栏添加标签筛选 -->
<ComboBox x:Name="TagFilterComboBox"
         Width="150"
         hc:InfoElement.Placeholder="按标签筛选"
         SelectionChanged="TagFilter_Changed">
    <ComboBox.ItemTemplate>
        <DataTemplate>
            <StackPanel Orientation="Horizontal">
                <Border Width="15" Height="15"
                       Background="{Binding ColorBrush}"
                       CornerRadius="2"
                       Margin="0,0,5,0"/>
                <TextBlock Text="{Binding Name}"/>
            </StackPanel>
        </DataTemplate>
    </ComboBox.ItemTemplate>
</ComboBox>
```

**测试点**:
- [ ] 下拉框显示所有标签
- [ ] 标签颜色正确显示
- [ ] 选择标签触发筛选

#### 4.2 修改 MainWindowViewModel.cs
```csharp
// 添加属性
private ObservableCollection<Tag> allTags = new();
public ObservableCollection<Tag> AllTags
{
    get => allTags;
    set
    {
        allTags = value;
        OnPropertyChanged(nameof(AllTags));
    }
}

// 加载所有标签
private async Task LoadAllTags()
{
    var tags = await App.TagRepository.GetAllAsync();
    AllTags = new ObservableCollection<Tag>(tags);
}

// 筛选逻辑
private void ApplyTagFilter(Tag? selectedTag)
{
    if (selectedTag == null)
    {
        // 显示所有待办
        Model.TodoItems = MainWindowModel.LoadFromDatabase();
    }
    else
    {
        // 筛选包含指定标签的待办
        var allItems = MainWindowModel.LoadFromDatabase();
        var filtered = new ObservableCollection<TodoItemModel>();
        
        foreach (var item in allItems)
        {
            if (item.Tags.Any(tag => tag.Id == selectedTag.Id))
            {
                filtered.Add(item);
            }
        }
        
        Model.TodoItems = filtered;
    }
}
```

**测试点**:
- [ ] 标签列表正确加载
- [ ] 选择标签正确筛选
- [ ] 清除筛选显示所有
- [ ] 筛选结果准确

---

## ?? 测试清单

### 功能测试
- [ ] 可以创建标签
- [ ] 可以编辑标签
- [ ] 可以删除标签
- [ ] 可以为待办添加标签
- [ ] 可以从待办移除标签
- [ ] 待办列表显示标签
- [ ] 按标签筛选正常
- [ ] 标签管理窗口正常
- [ ] 颜色选择器正常
- [ ] 标签预览正常

### 边界测试
- [ ] 标签名称为空时验证
- [ ] 删除标签确认提示
- [ ] 删除标签时清理关联
- [ ] 标签颜色格式验证
- [ ] 大量标签性能测试

### UI 测试
- [ ] 标签显示美观
- [ ] 颜色对比度良好
- [ ] 布局不错乱
- [ ] 响应式正常
- [ ] 交互流畅

---

## ?? 需要的文件

### 需要创建（7个文件）
1. `Models/Tag.cs`
2. `Models/TodoItemTag.cs`
3. `Services/Database/Repositories/TagRepository.cs`
4. `Views/TagManagementWindow.xaml`
5. `Views/TagManagementWindow.xaml.cs`
6. `Views/EditTagWindow.xaml`
7. `Views/EditTagWindow.xaml.cs`

### 需要修改（10个文件）
1. `Models/TodoItem.cs`
2. `Models/TodoItemModel.cs`
3. `Services/Database/TodoDbContext.cs`
4. `Services/Database/DatabaseInitializer.cs`
5. `Views/EditTodoItemWindow.xaml`
6. `Views/EditTodoItemWindow.xaml.cs`
7. `Views/TodoItemControl.xaml`
8. `Views/MainWindow.xaml`
9. `ViewModels/MainWindowViewModel.cs`
10. `App.xaml.cs`

---

## ?? 常见问题

### Q1: HandyControl ColorPicker 在哪？
**A**: HandyControl 提供 `hc:ColorPicker` 控件，支持颜色选择

**使用示例**:
```xaml
<hc:ColorPicker x:Name="ColorPicker" 
               ShowAlpha="False"
               SelectedBrush="{Binding Color}"/>
```

### Q2: 多对多关系如何实现？
**A**: 使用中间表 TodoItemTag

**关系图**:
```
TodoItem (1) ←→ (N) TodoItemTag (N) ←→ (1) Tag
```

### Q3: 如何避免重复添加标签？
**A**: 在 TodoItemTag 表上建立唯一索引

```csharp
modelBuilder.Entity<TodoItemTag>(entity =>
{
    entity.HasIndex(e => new { e.TodoItemId, e.TagId }).IsUnique();
});
```

### Q4: 标签颜色如何选择？
**A**: 提供预设颜色 + ColorPicker

**推荐颜色**:
- Red: #F44336
- Blue: #2196F3
- Green: #4CAF50
- Orange: #FF9800
- Purple: #9C27B0

### Q5: 删除标签时怎么处理关联？
**A**: 级联删除

```csharp
public async Task<int> DeleteAsync(string id)
{
    // 先删除关联
    var relations = await dbContext.TodoItemTags
        .Where(t => t.TagId == id)
        .ToListAsync();
    dbContext.TodoItemTags.RemoveRange(relations);
    
    // 再删除标签
    var tag = await dbContext.Tags.FindAsync(id);
    if (tag != null)
    {
        dbContext.Tags.Remove(tag);
    }
    
    return await dbContext.SaveChangesAsync();
}
```

---

## ?? 提示

### 开发顺序建议
1. **先做数据层** → 确保数据正确存取
2. **再做管理界面** → 验证CRUD操作
3. **然后集成** → 待办添加标签
4. **最后筛选** → 验证整体功能

### 调试技巧
1. 使用 SQLite 浏览器查看数据库
2. Debug.WriteLine 输出关键信息
3. 分步测试每个功能
4. 先测试单个标签，再测试多标签

### 代码风格
1. 遵循现有代码风格
2. 添加 XML 注释
3. 异常处理要完善
4. 资源及时释放

---

## ?? 预期成果

### 功能完成
- ? 标签 CRUD 功能完整
- ? 待办可以添加标签
- ? 标签显示清晰美观
- ? 筛选功能工作正常

### 代码质量
- ? 无编译错误
- ? 无运行时异常
- ? 数据正确持久化
- ? UI 响应流畅

### 文档完成
- ? Session-11 完成总结
- ? 功能使用文档
- ? 代码注释完整

---

## ?? 参考链接

- [HandyControl 文档](https://handyorg.github.io/handycontrol/)
- [EF Core 多对多关系](https://docs.microsoft.com/en-us/ef/core/modeling/relationships)
- [SQLite 外键约束](https://www.sqlite.org/foreignkeys.html)

---

## ? 完成标准

当以下所有项都完成时，Session-11 才算完成：

- [ ] 所有功能测试通过
- [ ] 所有边界测试通过
- [ ] 代码无编译错误
- [ ] 构建成功
- [ ] 更改已提交到 Git
- [ ] Session-11 完成总结已创建
- [ ] 路线图已更新

---

**准备好了吗？开始 Session-11 开发！** ??

**提示**: 从 Step 1 开始，一步一步完成，不要跳过测试！

**预计完成时间**: 3-4小时

**目标**: P0 完成度提升到 80%+
