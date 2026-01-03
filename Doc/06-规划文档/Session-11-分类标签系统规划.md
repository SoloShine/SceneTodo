# Session-11 开发规划：分类标签系统

> **规划日期**: 2025-01-02  
> **预计工作量**: 3-4小时  
> **优先级**: ???? P0  
> **目标**: 实现待办事项的分类标签功能

---

## ?? 功能概述

根据 PRD 功能对比分析，**分类标签功能**是 P0 功能中的重大缺失项，影响评级为 ????（重要）。

**PRD 要求**:
> 支持创建和管理自定义分类标签，为待办添加多个标签，按标签筛选和搜索

**当前状态**: ? 未实现（0%）

---

## ?? 功能需求

### 核心功能

1. **数据模型扩展**
   - 创建 Tag 实体
   - TodoItem 和 Tag 多对多关系
   - 数据库迁移

2. **标签管理**
   - 创建/编辑/删除标签
   - 标签列表展示
   - 标签颜色自定义

3. **待办标签关联**
   - 编辑窗口添加标签选择
   - 支持多标签选择
   - 标签快速输入

4. **筛选和搜索**
   - 按标签筛选待办
   - 多标签组合筛选
   - 标签统计显示

---

## ?? 技术方案

### 1. 数据模型 (1小时)

#### 创建 Tag.cs
```csharp
namespace SceneTodo.Models
{
    /// <summary>
    /// 标签实体
    /// </summary>
    public class Tag : BaseModel
    {
        private string id = Guid.NewGuid().ToString();
        public string Id
        {
            get => id;
            set
            {
                if (id != value)
                {
                    id = value;
                    OnPropertyChanged(nameof(Id));
                }
            }
        }

        private string name = string.Empty;
        /// <summary>
        /// 标签名称
        /// </summary>
        public string Name
        {
            get => name;
            set
            {
                if (name != value)
                {
                    name = value;
                    OnPropertyChanged(nameof(Name));
                }
            }
        }

        private string color = "#2196F3";
        /// <summary>
        /// 标签颜色（十六进制）
        /// </summary>
        public string Color
        {
            get => color;
            set
            {
                if (color != value)
                {
                    color = value;
                    OnPropertyChanged(nameof(Color));
                    OnPropertyChanged(nameof(ColorBrush));
                }
            }
        }

        /// <summary>
        /// 颜色画刷（用于绑定）
        /// </summary>
        [JsonIgnore]
        public SolidColorBrush ColorBrush
        {
            get
            {
                try
                {
                    return new SolidColorBrush((Color)ColorConverter.ConvertFromString(Color));
                }
                catch
                {
                    return new SolidColorBrush(Colors.Gray);
                }
            }
        }

        private DateTime? createdAt = DateTime.Now;
        public DateTime? CreatedAt
        {
            get => createdAt;
            set
            {
                if (createdAt != value)
                {
                    createdAt = value;
                    OnPropertyChanged(nameof(CreatedAt));
                }
            }
        }

        /// <summary>
        /// 使用此标签的待办数量
        /// </summary>
        [JsonIgnore]
        public int UsageCount { get; set; }
    }
}
```

#### 创建 TodoItemTag.cs（关联表）
```csharp
namespace SceneTodo.Models
{
    /// <summary>
    /// 待办-标签关联实体
    /// </summary>
    public class TodoItemTag
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        /// <summary>
        /// 待办项ID
        /// </summary>
        public string TodoItemId { get; set; }
        
        /// <summary>
        /// 标签ID
        /// </summary>
        public string TagId { get; set; }
        
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
```

#### 修改 TodoItem.cs
```csharp
public class TodoItem : BaseModel
{
    // ...existing properties...
    
    private string tagsJson = "[]";
    /// <summary>
    /// 标签ID列表（JSON格式）
    /// </summary>
    public string TagsJson
    {
        get => tagsJson;
        set
        {
            if (tagsJson != value)
            {
                tagsJson = value;
                UpdatedAt = DateTime.Now;
                OnPropertyChanged(nameof(TagsJson));
            }
        }
    }
}
```

#### 修改 TodoItemModel.cs
```csharp
public class TodoItemModel : TodoItem
{
    private ObservableCollection<Tag>? tags;
    
    /// <summary>
    /// 标签集合（从JSON反序列化）
    /// </summary>
    [JsonIgnore]
    public ObservableCollection<Tag> Tags
    {
        get
        {
            if (tags == null)
            {
                try
                {
                    var tagIds = JsonSerializer.Deserialize<List<string>>(TagsJson) ?? new List<string>();
                    tags = new ObservableCollection<Tag>();
                    
                    // 从数据库加载标签实体
                    foreach (var tagId in tagIds)
                    {
                        var tag = App.TagRepository.GetByIdAsync(tagId).Result;
                        if (tag != null)
                        {
                            tags.Add(tag);
                        }
                    }
                }
                catch
                {
                    tags = new ObservableCollection<Tag>();
                }
            }
            return tags;
        }
        set
        {
            tags = value;
            // 保存标签ID列表到JSON
            var tagIds = tags.Select(t => t.Id).ToList();
            TagsJson = JsonSerializer.Serialize(tagIds);
            OnPropertyChanged(nameof(Tags));
        }
    }
}
```

#### 数据库上下文
```csharp
// 在 TodoDbContext.cs 中添加
public DbSet<Tag> Tags { get; set; }
public DbSet<TodoItemTag> TodoItemTags { get; set; }

protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    // ...existing code...
    
    // 配置 Tag 实体
    modelBuilder.Entity<Tag>(entity =>
    {
        entity.HasKey(e => e.Id);
        entity.Property(e => e.Name).IsRequired();
        entity.Property(e => e.Color).HasDefaultValue("#2196F3");
    });
    
    // 配置 TodoItemTag 关联
    modelBuilder.Entity<TodoItemTag>(entity =>
    {
        entity.HasKey(e => e.Id);
        entity.HasIndex(e => new { e.TodoItemId, e.TagId }).IsUnique();
    });
}
```

#### 创建 TagRepository.cs
```csharp
namespace SceneTodo.Services.Database.Repositories
{
    public class TagRepository
    {
        private readonly TodoDbContext dbContext;
        
        public TagRepository(TodoDbContext dbContext)
        {
            this.dbContext = dbContext;
        }
        
        public async Task<IEnumerable<Tag>> GetAllAsync()
        {
            return await dbContext.Tags.ToListAsync();
        }
        
        public async Task<Tag?> GetByIdAsync(string id)
        {
            return await dbContext.Tags.FirstOrDefaultAsync(t => t.Id == id);
        }
        
        public async Task<int> AddAsync(Tag tag)
        {
            dbContext.Tags.Add(tag);
            return await dbContext.SaveChangesAsync();
        }
        
        public async Task<int> UpdateAsync(Tag tag)
        {
            var existing = await dbContext.Tags.FindAsync(tag.Id);
            if (existing == null) return 0;
            
            dbContext.Entry(existing).CurrentValues.SetValues(tag);
            return await dbContext.SaveChangesAsync();
        }
        
        public async Task<int> DeleteAsync(string id)
        {
            var tag = await dbContext.Tags.FindAsync(id);
            if (tag != null)
            {
                // 同时删除关联
                var relations = await dbContext.TodoItemTags
                    .Where(t => t.TagId == id)
                    .ToListAsync();
                dbContext.TodoItemTags.RemoveRange(relations);
                
                dbContext.Tags.Remove(tag);
                return await dbContext.SaveChangesAsync();
            }
            return 0;
        }
        
        /// <summary>
        /// 获取待办的标签
        /// </summary>
        public async Task<IEnumerable<Tag>> GetTagsForTodoAsync(string todoItemId)
        {
            var tagIds = await dbContext.TodoItemTags
                .Where(t => t.TodoItemId == todoItemId)
                .Select(t => t.TagId)
                .ToListAsync();
            
            return await dbContext.Tags
                .Where(t => tagIds.Contains(t.Id))
                .ToListAsync();
        }
        
        /// <summary>
        /// 为待办添加标签
        /// </summary>
        public async Task<int> AddTagToTodoAsync(string todoItemId, string tagId)
        {
            var exists = await dbContext.TodoItemTags
                .AnyAsync(t => t.TodoItemId == todoItemId && t.TagId == tagId);
            
            if (!exists)
            {
                dbContext.TodoItemTags.Add(new TodoItemTag
                {
                    TodoItemId = todoItemId,
                    TagId = tagId
                });
                return await dbContext.SaveChangesAsync();
            }
            return 0;
        }
        
        /// <summary>
        /// 从待办移除标签
        /// </summary>
        public async Task<int> RemoveTagFromTodoAsync(string todoItemId, string tagId)
        {
            var relation = await dbContext.TodoItemTags
                .FirstOrDefaultAsync(t => t.TodoItemId == todoItemId && t.TagId == tagId);
            
            if (relation != null)
            {
                dbContext.TodoItemTags.Remove(relation);
                return await dbContext.SaveChangesAsync();
            }
            return 0;
        }
    }
}
```

### 2. 标签管理界面 (1小时)

#### 创建 TagManagementWindow.xaml
```xaml
<hc:Window x:Class="SceneTodo.Views.TagManagementWindow"
          xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
          xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
          xmlns:hc="https://handyorg.github.io/handycontrol"
          Title="标签管理"
          Width="600"
          Height="500"
          WindowStartupLocation="CenterScreen">
    <Grid Margin="15">
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="*"/>
            <RowDefinition Height="Auto"/>
        </Grid.RowDefinitions>
        
        <!-- 标题和新建按钮 -->
        <Grid Grid.Row="0" Margin="0,0,0,15">
            <TextBlock Text="标签管理" FontSize="18" FontWeight="Bold" VerticalAlignment="Center"/>
            <Button Content="? 新建标签"
                   HorizontalAlignment="Right"
                   Style="{StaticResource ButtonPrimary}"
                   Click="AddTag_Click"/>
        </Grid>
        
        <!-- 标签列表 -->
        <DataGrid Grid.Row="1"
                 x:Name="TagsDataGrid"
                 AutoGenerateColumns="False"
                 CanUserAddRows="False"
                 SelectionMode="Single"
                 HeadersVisibility="Column">
            <DataGrid.Columns>
                <!-- 颜色预览 -->
                <DataGridTemplateColumn Header="颜色" Width="60">
                    <DataGridTemplateColumn.CellTemplate>
                        <DataTemplate>
                            <Border Width="30" Height="20" 
                                   Background="{Binding ColorBrush}"
                                   CornerRadius="3"
                                   BorderBrush="Gray"
                                   BorderThickness="1"/>
                        </DataTemplate>
                    </DataGridTemplateColumn.CellTemplate>
                </DataGridTemplateColumn>
                
                <!-- 名称 -->
                <DataGridTextColumn Header="标签名称" 
                                   Binding="{Binding Name}" 
                                   Width="*"/>
                
                <!-- 使用次数 -->
                <DataGridTextColumn Header="使用次数" 
                                   Binding="{Binding UsageCount}" 
                                   Width="100"/>
                
                <!-- 创建时间 -->
                <DataGridTextColumn Header="创建时间" 
                                   Binding="{Binding CreatedAt, StringFormat={}{0:yyyy-MM-dd}}" 
                                   Width="120"/>
                
                <!-- 操作按钮 -->
                <DataGridTemplateColumn Header="操作" Width="150">
                    <DataGridTemplateColumn.CellTemplate>
                        <DataTemplate>
                            <StackPanel Orientation="Horizontal">
                                <Button Content="编辑" 
                                       Margin="0,0,5,0"
                                       Tag="{Binding}"
                                       Click="EditTag_Click"/>
                                <Button Content="删除" 
                                       Style="{StaticResource ButtonDanger}"
                                       Tag="{Binding}"
                                       Click="DeleteTag_Click"/>
                            </StackPanel>
                        </DataTemplate>
                    </DataGridTemplateColumn.CellTemplate>
                </DataGridTemplateColumn>
            </DataGrid.Columns>
        </DataGrid>
        
        <!-- 底部按钮 -->
        <Button Grid.Row="2"
               Content="关闭"
               Width="100"
               Height="35"
               Margin="0,15,0,0"
               HorizontalAlignment="Right"
               Click="Close_Click"/>
    </Grid>
</hc:Window>
```

#### 创建 EditTagWindow.xaml
```xaml
<hc:Window x:Class="SceneTodo.Views.EditTagWindow"
          xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
          xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
          xmlns:hc="https://handyorg.github.io/handycontrol"
          Title="编辑标签"
          Width="400"
          Height="250"
          WindowStartupLocation="CenterScreen">
    <Grid Margin="20">
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="*"/>
            <RowDefinition Height="Auto"/>
        </Grid.RowDefinitions>
        
        <!-- 标签名称 -->
        <StackPanel Grid.Row="0" Margin="0,0,0,15">
            <TextBlock Text="标签名称:" Margin="0,0,0,5"/>
            <TextBox x:Name="NameTextBox" 
                    hc:InfoElement.Placeholder="输入标签名称"/>
        </StackPanel>
        
        <!-- 标签颜色 -->
        <StackPanel Grid.Row="1" Margin="0,0,0,15">
            <TextBlock Text="标签颜色:" Margin="0,0,0,5"/>
            <hc:ColorPicker x:Name="ColorPicker" 
                          ShowAlpha="False"/>
        </StackPanel>
        
        <!-- 预览 -->
        <StackPanel Grid.Row="2" Margin="0,0,0,15">
            <TextBlock Text="预览:" Margin="0,0,0,5"/>
            <Border x:Name="PreviewBorder"
                   Height="40"
                   Background="{Binding ElementName=ColorPicker, Path=SelectedBrush}"
                   CornerRadius="5"
                   Padding="10">
                <TextBlock Text="{Binding ElementName=NameTextBox, Path=Text}"
                          Foreground="White"
                          FontWeight="Bold"
                          VerticalAlignment="Center"
                          HorizontalAlignment="Center"/>
            </Border>
        </StackPanel>
        
        <!-- 按钮 -->
        <StackPanel Grid.Row="3" 
                   Orientation="Horizontal" 
                   HorizontalAlignment="Right">
            <Button Content="保存"
                   Width="80"
                   Margin="0,0,10,0"
                   Style="{StaticResource ButtonPrimary}"
                   Click="Save_Click"/>
            <Button Content="取消"
                   Width="80"
                   Click="Cancel_Click"/>
        </StackPanel>
    </Grid>
</hc:Window>
```

### 3. 待办标签关联 (1小时)

#### 修改 EditTodoItemWindow.xaml
```xaml
<!-- 在时间设置分组后添加标签分组 -->
<Expander Header="??? 标签" IsExpanded="False" Margin="0,0,0,10">
    <StackPanel Margin="10">
        <Grid Margin="0,0,0,10">
            <Grid.ColumnDefinitions>
                <ColumnDefinition Width="*"/>
                <ColumnDefinition Width="Auto"/>
            </Grid.ColumnDefinitions>
            <TextBlock Text="选择标签:" Grid.Column="0" VerticalAlignment="Center"/>
            <Button Grid.Column="1"
                   Content="管理标签"
                   Click="ManageTags_Click"/>
        </Grid>
        
        <!-- 标签选择列表 -->
        <ListBox x:Name="TagsListBox"
                Height="150"
                SelectionMode="Multiple"
                Background="{DynamicResource SecondaryRegionBrush}">
            <ListBox.ItemTemplate>
                <DataTemplate>
                    <Border Background="{Binding ColorBrush}"
                           CornerRadius="3"
                           Padding="8,4"
                           Margin="2">
                        <TextBlock Text="{Binding Name}"
                                  Foreground="White"
                                  FontWeight="Bold"/>
                    </Border>
                </DataTemplate>
            </ListBox.ItemTemplate>
            <ListBox.ItemsPanel>
                <ItemsPanelTemplate>
                    <WrapPanel/>
                </ItemsPanelTemplate>
            </ListBox.ItemsPanel>
        </ListBox>
        
        <!-- 已选标签预览 -->
        <TextBlock Text="已选标签:" Margin="0,10,0,5"/>
        <ItemsControl x:Name="SelectedTagsPreview">
            <ItemsControl.ItemsPanel>
                <ItemsPanelTemplate>
                    <WrapPanel/>
                </ItemsPanelTemplate>
            </ItemsControl.ItemsPanel>
            <ItemsControl.ItemTemplate>
                <DataTemplate>
                    <Border Background="{Binding ColorBrush}"
                           CornerRadius="3"
                           Padding="6,3"
                           Margin="2">
                        <StackPanel Orientation="Horizontal">
                            <TextBlock Text="{Binding Name}"
                                      Foreground="White"
                                      FontSize="11"/>
                            <Button Content="×"
                                   Foreground="White"
                                   Background="Transparent"
                                   BorderThickness="0"
                                   Margin="5,0,0,0"
                                   Padding="0"
                                   Width="15"
                                   Height="15"
                                   Tag="{Binding}"
                                   Click="RemoveTag_Click"/>
                        </StackPanel>
                    </Border>
                </DataTemplate>
            </ItemsControl.ItemTemplate>
        </ItemsControl>
    </StackPanel>
</Expander>
```

#### 修改 TodoItemControl.xaml
```xaml
<!-- 在第二行时间信息后添加标签显示 -->
<ItemsControl ItemsSource="{Binding Tags}"
             Grid.Column="1"
             Grid.Row="1"
             Margin="5,0,0,0"
             VerticalAlignment="Center">
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

### 4. 筛选和搜索 (30分钟)

#### 在 MainWindow.xaml 添加标签筛选器
```xaml
<!-- 在工具栏添加标签筛选下拉框 -->
<ComboBox x:Name="TagFilterComboBox"
         Width="150"
         Margin="10,0,0,0"
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
                <TextBlock Text="{Binding UsageCount, StringFormat={}({0})}"
                          Foreground="Gray"
                          Margin="5,0,0,0"/>
            </StackPanel>
        </DataTemplate>
    </ComboBox.ItemTemplate>
</ComboBox>
```

#### 在 MainWindowViewModel.cs 添加筛选逻辑
```csharp
public class MainWindowViewModel : INotifyPropertyChanged
{
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
    
    private Tag? selectedTagFilter;
    public Tag? SelectedTagFilter
    {
        get => selectedTagFilter;
        set
        {
            selectedTagFilter = value;
            OnPropertyChanged(nameof(selectedTagFilter));
            ApplyTagFilter();
        }
    }
    
    private void ApplyTagFilter()
    {
        if (SelectedTagFilter == null)
        {
            // 显示所有待办
            Model.TodoItems = MainWindowModel.LoadFromDatabase();
        }
        else
        {
            // 筛选包含指定标签的待办
            var allItems = MainWindowModel.LoadFromDatabase();
            var filtered = allItems.Where(item => 
                item.Tags.Any(tag => tag.Id == SelectedTagFilter.Id))
                .ToList();
            
            Model.TodoItems = new ObservableCollection<TodoItemModel>(filtered);
        }
    }
    
    /// <summary>
    /// 加载所有标签
    /// </summary>
    private async Task LoadAllTags()
    {
        var tags = await App.TagRepository.GetAllAsync();
        AllTags = new ObservableCollection<Tag>(tags);
        
        // 更新使用次数
        foreach (var tag in AllTags)
        {
            tag.UsageCount = await GetTagUsageCount(tag.Id);
        }
    }
    
    private async Task<int> GetTagUsageCount(string tagId)
    {
        var relations = await dbContext.TodoItemTags
            .CountAsync(t => t.TagId == tagId);
        return relations;
    }
}
```

---

## ??? 文件清单

### 需要创建的文件

1. **Models/Tag.cs**
   - 标签实体模型

2. **Models/TodoItemTag.cs**
   - 待办-标签关联模型

3. **Services/Database/Repositories/TagRepository.cs**
   - 标签数据访问层

4. **Views/TagManagementWindow.xaml**
   - 标签管理窗口

5. **Views/TagManagementWindow.xaml.cs**
   - 标签管理逻辑

6. **Views/EditTagWindow.xaml**
   - 标签编辑窗口

7. **Views/EditTagWindow.xaml.cs**
   - 标签编辑逻辑

### 需要修改的文件

1. **Models/TodoItem.cs**
   - 添加 TagsJson 属性

2. **Models/TodoItemModel.cs**
   - 添加 Tags 集合属性

3. **Services/Database/TodoDbContext.cs**
   - 添加 Tags 和 TodoItemTags DbSet
   - 配置实体关系

4. **Services/Database/DatabaseInitializer.cs**
   - 添加标签表迁移逻辑

5. **Views/EditTodoItemWindow.xaml**
   - 添加标签选择区域

6. **Views/EditTodoItemWindow.xaml.cs**
   - 添加标签选择逻辑

7. **Views/TodoItemControl.xaml**
   - 显示标签

8. **Views/MainWindow.xaml**
   - 添加标签筛选控件

9. **ViewModels/MainWindowViewModel.cs**
   - 添加标签筛选逻辑

10. **App.xaml.cs**
    - 注册 TagRepository

---

## ? 验收标准

### 功能测试

- [ ] 可以创建新标签
- [ ] 可以编辑标签名称和颜色
- [ ] 可以删除标签
- [ ] 标签列表显示正确
- [ ] 可以为待办添加标签
- [ ] 可以从待办移除标签
- [ ] 待办列表正确显示标签
- [ ] 按标签筛选工作正常
- [ ] 标签使用次数统计准确
- [ ] 删除标签时清理关联

### 数据测试

- [ ] 标签正确保存到数据库
- [ ] 标签关联正确保存
- [ ] 标签颜色正确显示
- [ ] 多标签支持正常
- [ ] 数据迁移兼容旧数据

### UI测试

- [ ] 标签管理窗口布局合理
- [ ] 颜色选择器正常工作
- [ ] 标签预览正确
- [ ] 标签选择界面友好
- [ ] 标签显示美观
- [ ] 筛选下拉框工作正常

---

## ?? 开发进度估算

| 任务 | 预计时间 | 难度 |
|-----|---------|------|
| 数据模型和Repository | 1h | ??? |
| 数据库迁移 | 0.5h | ?? |
| 标签管理窗口 | 1h | ??? |
| 待办标签关联UI | 0.5h | ?? |
| 标签显示 | 0.5h | ?? |
| 筛选功能 | 0.5h | ??? |
| 测试和调试 | 0.5h | ?? |
| **总计** | **3-4h** | ??? |

---

## ?? 实施步骤

### Step 1: 数据层 (1.5h)

1. 创建 Tag.cs 和 TodoItemTag.cs
2. 修改 TodoItem.cs 和 TodoItemModel.cs
3. 创建 TagRepository.cs
4. 修改 TodoDbContext.cs
5. 添加数据库迁移
6. 在 App.xaml.cs 注册 TagRepository

### Step 2: 标签管理界面 (1h)

1. 创建 TagManagementWindow
2. 创建 EditTagWindow
3. 实现 CRUD 逻辑
4. 测试标签管理

### Step 3: 待办标签集成 (1h)

1. 修改 EditTodoItemWindow
2. 添加标签选择逻辑
3. 修改 TodoItemControl 显示标签
4. 测试标签关联

### Step 4: 筛选功能 (0.5h)

1. 修改 MainWindow 添加筛选器
2. 实现筛选逻辑
3. 完整测试

---

## ?? 注意事项

### 技术要点

1. **多对多关系**
   - 使用关联表 TodoItemTag
   - 避免直接在 TodoItem 中存储 Tag 集合
   - JSON 存储标签ID列表作为冗余

2. **性能优化**
   - 标签列表缓存
   - 延迟加载标签详情
   - 批量加载关联

3. **用户体验**
   - 标签颜色鲜明易识别
   - 快速添加/移除标签
   - 支持多标签筛选（扩展功能）

### 潜在问题

1. **数据一致性**
   - 删除标签时清理关联
   - 标签重命名时更新显示
   - JSON 和数据库同步

2. **UI 性能**
   - 大量标签时的渲染性能
   - 标签列表分页（扩展功能）

3. **颜色选择**
   - HandyControl ColorPicker 使用
   - 预设常用颜色

---

## ?? 设计参考

### 推荐颜色方案
```csharp
public static class TagColors
{
    public static readonly string[] DefaultColors = new[]
    {
        "#F44336", // Red
        "#E91E63", // Pink
        "#9C27B0", // Purple
        "#673AB7", // Deep Purple
        "#3F51B5", // Indigo
        "#2196F3", // Blue
        "#03A9F4", // Light Blue
        "#00BCD4", // Cyan
        "#009688", // Teal
        "#4CAF50", // Green
        "#8BC34A", // Light Green
        "#CDDC39", // Lime
        "#FFEB3B", // Yellow
        "#FFC107", // Amber
        "#FF9800", // Orange
        "#FF5722", // Deep Orange
    };
}
```

### 标签显示样式
- 圆角边框（CornerRadius="3"）
- 白色文字（Foreground="White"）
- 小字号（FontSize="10-11"）
- 粗体（FontWeight="Bold"）
- 适当间距（Margin="2"）

---

## ?? 相关文档

- [PRD功能对比分析](../../02-功能文档/PRD功能实现对比分析报告.md)
- [PRD快速参考](../../02-功能文档/PRD对比分析-快速参考.md)
- [开发路线图](../../06-规划文档/开发路线图-v1.0.md)

---

## ?? 下一步计划

完成 Session-11 后：

**Session-12**: 数据备份恢复 (2-3h) ????
**Session-13**: UI/UX 优化 (3-4h) ???

---

## ?? 成功指标

### 用户角度
- ? 可以轻松创建和管理标签
- ? 可以为待办快速添加标签
- ? 标签显示清晰美观
- ? 按标签筛选快速准确

### 技术角度
- ? 数据模型设计合理
- ? 数据库查询高效
- ? UI 响应流畅
- ? 代码可维护性高

### 项目角度
- ? P0 完成度提升到 80%+
- ? 产品竞争力提升
- ? 用户体验改善

---

**规划版本**: 1.0  
**创建时间**: 2025-01-02  
**规划者**: SceneTodo 团队

**准备开始 Session-11 开发！** ??
