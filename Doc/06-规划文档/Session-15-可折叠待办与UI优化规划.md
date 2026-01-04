# Session-15 Planning - Collapsible Todos & UI Polish

> **Created**: 2025-01-05  
> **Priority**: High ?????  
> **Estimated Time**: 1-1.5 hours  
> **Status**: ?? Ready to Start  

---

## ?? Session Overview

### Session Information

- **Session Number**: 15
- **Feature**: Collapsible Todos & UI Polish
- **Priority**: P2 (Complete Session-14 deferred items)
- **Estimated Time**: 1-1.5 hours
- **Expected Outcome**: Collapsible todo list + interface improvements

---

## ?? Goals and Objectives

### Primary Goals

1. **Implement Collapsible Todos** ?????
   - High priority deferred from Session-14
   - Quick implementation (20-30 min)
   - High user value

2. **UI/UX Polish** ????
   - Spacing and alignment
   - Icon consistency
   - Tooltip improvements

3. **Testing and Validation** ????
   - Functional testing
   - Visual testing
   - Performance check

---

## ?? Planned Features

### 1. Collapsible Todo List ?????

**Priority**: Very High (Deferred from Session-14)  
**Time**: 20-30 minutes

**Features**:
- [ ] Add IsExpanded property to TodoItemModel
- [ ] Collapse/expand button in UI
- [ ] Remember state per todo
- [ ] Animate transitions
- [ ] Keyboard shortcut integration (Ctrl+E)
- [ ] Expand all / Collapse all commands

**Benefits**:
- Reduce clutter in todo list
- Focus on important items
- Better space management
- Improved navigation

**Technical Details**:
```csharp
// TodoItemModel.cs
public class TodoItemModel
{
    private bool _isExpanded = true;
    public bool IsExpanded
    {
        get => _isExpanded;
        set
        {
            if (_isExpanded != value)
            {
                _isExpanded = value;
                OnPropertyChanged(nameof(IsExpanded));
                // Save state if enabled in settings
                SaveCollapseState();
            }
        }
    }
}
```

---

### 2. Interface Detail Improvements ????

**Priority**: High  
**Time**: 15-20 minutes

**Features**:
- [ ] Better spacing and alignment
  - Consistent margins (8dp, 16dp, 24dp)
  - Proper padding in controls
  - Aligned labels and inputs

- [ ] Improved icon consistency
  - Same icon family
  - Consistent icon sizes (16x16, 24x24, 32x32)
  - Proper icon colors

- [ ] Enhanced tooltips
  - Descriptive tooltip text
  - Keyboard shortcut hints
  - Proper delay (500ms)

- [ ] Visual states
  - Loading indicators
  - Empty state messages
  - Error state feedback

**Benefits**:
- Professional appearance
- Better user guidance
- Improved accessibility
- Consistent experience

---

### 3. Animation Integration ???

**Priority**: Medium  
**Time**: 15-20 minutes

**Features**:
- [ ] Apply collapse/expand animations
- [ ] Add todo item animations
- [ ] Search result fade-in
- [ ] Filter panel slide

**Using AnimationHelper**:
```csharp
// Example usage
private void ToggleExpand()
{
    if (EnableAnimations)
    {
        if (IsExpanded)
        {
            AnimationHelper.SlideInFromTop(SubItemsPanel);
        }
        else
        {
            AnimationHelper.SlideOutToTop(SubItemsPanel);
        }
    }
    
    IsExpanded = !IsExpanded;
}
```

---

## ?? Technical Design

### Architecture

```
Session-15 Updates:
Models/
└── TodoItemModel.cs           ?? Update (Add IsExpanded)

ViewModels/
└── MainWindowViewModel.TodoManagement.cs  ?? Update (Expand/Collapse commands)

Views/
├── TodoItemControl.xaml       ?? Update (Add collapse button)
├── TodoItemControl.xaml.cs    ?? Update (Handle collapse logic)
└── MainWindow.xaml            ?? Update (Polish spacing/alignment)

Utils/
└── AnimationHelper.cs         ?? Update (Add SlideOutToTop)
```

---

### Data Model Updates

#### TodoItemModel - IsExpanded Property

```csharp
/// <summary>
/// TodoItemModel with collapse state
/// </summary>
public partial class TodoItemModel
{
    private bool _isExpanded = true;
    
    /// <summary>
    /// 是否展开子项
    /// </summary>
    [JsonIgnore]
    public bool IsExpanded
    {
        get => _isExpanded;
        set
        {
            if (_isExpanded != value)
            {
                _isExpanded = value;
                OnPropertyChanged(nameof(IsExpanded));
                
                // 如果设置中启用了记住折叠状态，则保存
                if (App.MainViewModel?.AppSettings?.Behavior?.RememberCollapseState == true)
                {
                    SaveCollapseStateAsync();
                }
            }
        }
    }
    
    /// <summary>
    /// 保存折叠状态
    /// </summary>
    private async void SaveCollapseStateAsync()
    {
        try
        {
            // 保存到数据库或配置文件
            var stateManager = App.CollapseStateManager;
            await stateManager.SaveStateAsync(this.Id, _isExpanded);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"保存折叠状态失败: {ex.Message}");
        }
    }
    
    /// <summary>
    /// 加载折叠状态
    /// </summary>
    public async Task LoadCollapseStateAsync()
    {
        if (App.MainViewModel?.AppSettings?.Behavior?.RememberCollapseState == true)
        {
            try
            {
                var stateManager = App.CollapseStateManager;
                _isExpanded = await stateManager.LoadStateAsync(this.Id);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"加载折叠状态失败: {ex.Message}");
            }
        }
    }
}
```

---

### ViewModel Updates

#### MainWindowViewModel - Expand/Collapse Commands

```csharp
/// <summary>
/// MainWindowViewModel - Expand/Collapse functionality
/// </summary>
public partial class MainWindowViewModel
{
    #region Expand/Collapse Commands
    
    public ICommand ToggleExpandCommand { get; private set; }
    public ICommand ExpandAllCommand { get; private set; }
    public ICommand CollapseAllCommand { get; private set; }
    
    private void InitializeExpandCollapseCommands()
    {
        ToggleExpandCommand = new RelayCommand(param =>
        {
            if (param is TodoItemModel todo)
            {
                todo.IsExpanded = !todo.IsExpanded;
            }
        });
        
        ExpandAllCommand = new RelayCommand(_ =>
        {
            ExpandAll(Model.TodoItems);
            Growl.Success("已展开所有待办项");
        });
        
        CollapseAllCommand = new RelayCommand(_ =>
        {
            CollapseAll(Model.TodoItems);
            Growl.Success("已折叠所有待办项");
        });
    }
    
    private void ExpandAll(ObservableCollection<TodoItemModel> items)
    {
        foreach (var item in items)
        {
            item.IsExpanded = true;
            if (item.SubItems.Count > 0)
            {
                ExpandAll(item.SubItems);
            }
        }
    }
    
    private void CollapseAll(ObservableCollection<TodoItemModel> items)
    {
        foreach (var item in items)
        {
            item.IsExpanded = false;
            if (item.SubItems.Count > 0)
            {
                CollapseAll(item.SubItems);
            }
        }
    }
    
    #endregion
}
```

---

### UI Updates

#### TodoItemControl.xaml - Add Collapse Button

```xml
<UserControl x:Class="SceneTodo.Views.TodoItemControl">
    <Grid>
        <Grid.ColumnDefinitions>
            <ColumnDefinition Width="Auto"/>  <!-- Collapse button -->
            <ColumnDefinition Width="Auto"/>  <!-- Checkbox -->
            <ColumnDefinition Width="*"/>     <!-- Content -->
            <ColumnDefinition Width="Auto"/>  <!-- Actions -->
        </Grid.ColumnDefinitions>
        
        <!-- 折叠/展开按钮 -->
        <Button Grid.Column="0"
                x:Name="ExpandCollapseButton"
                Width="20"
                Height="20"
                Margin="4,0"
                Style="{StaticResource ButtonIcon}"
                Command="{Binding ToggleExpandCommand}"
                CommandParameter="{Binding}"
                Visibility="{Binding SubItems.Count, Converter={StaticResource GreaterThanZeroToVisibilityConverter}}"
                ToolTip="{Binding IsExpanded, Converter={StaticResource ExpandCollapseTooltipConverter}}">
            <Button.Content>
                <Path Data="{Binding IsExpanded, Converter={StaticResource ExpandCollapseIconConverter}}"
                      Fill="{DynamicResource PrimaryTextBrush}"
                      Stretch="Uniform"
                      Width="12"
                      Height="12"/>
            </Button.Content>
        </Button>
        
        <!-- Existing controls... -->
        
    </Grid>
    
    <!-- 子项容器 (可折叠) -->
    <ItemsControl Grid.Row="1"
                  x:Name="SubItemsContainer"
                  ItemsSource="{Binding SubItems}"
                  Visibility="{Binding IsExpanded, Converter={StaticResource Boolean2VisibilityConverter}}"
                  Margin="24,0,0,0">
        <!-- ... -->
    </ItemsControl>
</UserControl>
```

---

## ?? UI/UX Polish Guidelines

### Spacing Standards

```xml
<!-- Consistent spacing -->
<Style x:Key="StandardMargin">
    <Setter Property="Margin" Value="8"/>      <!-- Small -->
    <Setter Property="Margin" Value="16"/>     <!-- Medium -->
    <Setter Property="Margin" Value="24"/>     <!-- Large -->
</Style>

<!-- Consistent padding -->
<Style x:Key="StandardPadding">
    <Setter Property="Padding" Value="8"/>     <!-- Small -->
    <Setter Property="Padding" Value="12"/>    <!-- Medium -->
    <Setter Property="Padding" Value="16"/>    <!-- Large -->
</Style>
```

### Icon Standards

```xml
<!-- Standard icon sizes -->
<Style x:Key="SmallIcon" TargetType="Path">
    <Setter Property="Width" Value="16"/>
    <Setter Property="Height" Value="16"/>
</Style>

<Style x:Key="MediumIcon" TargetType="Path">
    <Setter Property="Width" Value="24"/>
    <Setter Property="Height" Value="24"/>
</Style>

<Style x:Key="LargeIcon" TargetType="Path">
    <Setter Property="Width" Value="32"/>
    <Setter Property="Height" Value="32"/>
</Style>
```

### Tooltip Standards

```xml
<!-- Standard tooltip with shortcut -->
<Button ToolTip="添加新待办 (Ctrl+N)"
        ToolTipService.InitialShowDelay="500"
        ToolTipService.ShowDuration="5000"/>
```

---

## ?? Implementation Plan

### Phase 1: Collapsible Todos (20-30 min)

**Step 1: Update Data Model** (5 min)
- [ ] Add IsExpanded property to TodoItemModel
- [ ] Add SaveCollapseStateAsync method
- [ ] Add LoadCollapseStateAsync method

**Step 2: Create Collapse State Manager** (10 min)
- [ ] Create CollapseStateManager.cs
- [ ] Implement Save/Load methods
- [ ] JSON file persistence

**Step 3: Update UI** (10 min)
- [ ] Add collapse button to TodoItemControl
- [ ] Add visibility binding for sub-items
- [ ] Add collapse icon converter
- [ ] Apply collapse animation

**Step 4: Add Commands** (5 min)
- [ ] ToggleExpandCommand
- [ ] ExpandAllCommand
- [ ] CollapseAllCommand
- [ ] Hook up Ctrl+E shortcut

---

### Phase 2: UI Polish (15-20 min)

**Step 1: Spacing Improvements** (8 min)
- [ ] Review all margins and paddings
- [ ] Apply consistent spacing (8dp, 16dp, 24dp)
- [ ] Fix alignment issues

**Step 2: Icon Consistency** (5 min)
- [ ] Standardize icon sizes
- [ ] Use consistent icon family
- [ ] Apply proper colors

**Step 3: Tooltip Enhancements** (7 min)
- [ ] Add tooltips to all buttons
- [ ] Include keyboard shortcuts
- [ ] Set proper delays

---

### Phase 3: Animation Integration (15-20 min)

**Step 1: Collapse/Expand Animations** (10 min)
- [ ] Implement SlideInFromTop
- [ ] Implement SlideOutToTop
- [ ] Apply to sub-items container
- [ ] Respect EnableAnimations setting

**Step 2: Other Animations** (10 min)
- [ ] Add todo animation on add
- [ ] Search result fade-in
- [ ] Filter panel slide

---

## ? Validation and Testing

### Functional Testing

- [ ] Collapse/expand button works
- [ ] State persists across restarts
- [ ] Keyboard shortcut (Ctrl+E) works
- [ ] Expand all / Collapse all works
- [ ] Animations play smoothly
- [ ] Settings are respected

### Visual Testing

- [ ] Spacing is consistent
- [ ] Icons are aligned
- [ ] Tooltips display correctly
- [ ] Animations are smooth (60 FPS)
- [ ] No visual glitches

### Performance Testing

- [ ] No performance degradation
- [ ] Fast collapse/expand
- [ ] Memory usage stable
- [ ] Responsive UI

---

## ?? Success Criteria

### Must Have ?

- [ ] Collapsible todos working
- [ ] State persistence working
- [ ] Animations smooth
- [ ] No bugs or errors

### Should Have ??

- [ ] Keyboard shortcuts working
- [ ] Spacing improved
- [ ] Tooltips added
- [ ] Icons consistent

### Nice to Have ?

- [ ] Advanced animations
- [ ] Custom collapse icons
- [ ] Animation customization

---

## ?? Quick Start Checklist

### Before Starting

- [ ] Session-14 code compiled successfully
- [ ] Review TodoItemModel structure
- [ ] Review AnimationHelper utilities
- [ ] Understand collapse state requirements

### Development

- [ ] Create feature branch (optional)
- [ ] Follow phase-by-phase approach
- [ ] Test after each phase
- [ ] Commit frequently

### After Completion

- [ ] Run full application test
- [ ] Check all collapse scenarios
- [ ] Verify state persistence
- [ ] Update documentation

---

## ?? Resources

### HandyControl
- [Expander Control](https://handyorg.github.io/handycontrol/extend_controls/expander/)
- [Icon Packs](https://github.com/MahApps/MahApps.Metro.IconPacks)

### WPF
- [Visibility Binding](https://docs.microsoft.com/en-us/dotnet/desktop/wpf/data/how-to-bind-to-an-enumeration)
- [ToolTip Service](https://docs.microsoft.com/en-us/dotnet/api/system.windows.controls.tooltipservice)

### Project Docs
- [Session-14 Final Report](../../01-会话记录/Session-14/Session-14最终完成报告.md)
- [Development Standards](../../05-开发文档/SceneTodo开发规范与最佳实践.md)
- [AnimationHelper](../../Utils/AnimationHelper.cs)

---

## ?? Related Documents

### Session-14 Documents
- [Session-14 Final Report](../../01-会话记录/Session-14/Session-14最终完成报告.md)
- [Session-14 Archive Report](../../01-会话记录/Session-14/Session-14归档与Session-15启动报告.md)

### Planning Documents
- [Development Roadmap](./开发路线图-v1.0.md)
- [Development Standards](../../05-开发文档/SceneTodo开发规范与最佳实践.md)

---

**Planning Version**: 1.0  
**Created**: 2025-01-05  
**Status**: ?? Ready for Implementation  
**Recommended Time**: Morning or focused 1-hour session

**?? Complete Session-14 deferred items and polish the UI!** ?
