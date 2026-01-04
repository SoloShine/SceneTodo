# Session-15 Quick Start Guide

> **Created**: 2025-01-05  
> **Estimated Time**: 1-1.5 hours  
> **Goal**: Collapsible Todos & UI Polish

---

## ?? Quick Overview

### This Session
Complete Session-14 deferred items: implement collapsible todo list and polish interface.

### Core Features
1. ????? Collapsible Todo List (20-30 min)
2. ???? UI/UX Polish (15-20 min)
3. ??? Animation Integration (15-20 min)

---

## ? Time Allocation

| Phase | Task | Time |
|-------|------|------|
| Phase 1 | Collapsible Todos | 20-30 min |
| Phase 2 | UI Polish | 15-20 min |
| Phase 3 | Animation Integration | 15-20 min |
| **Total** | | **1-1.5h** |

---

## ?? Prerequisites

### Environment Check
- [ ] Session-14 completed and compiled
- [ ] AnimationHelper utilities available
- [ ] Settings system working
- [ ] Git working directory clean

### Knowledge Required
- [ ] TodoItemModel structure
- [ ] MVVM pattern
- [ ] Data binding
- [ ] WPF animations

---

## ?? Quick Start Steps

### Step 1: Add IsExpanded Property (5 min)

Update `Models/TodoItemModel.cs`:

```csharp
private bool _isExpanded = true;

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
            SaveCollapseStateAsync();
        }
    }
}
```

### Step 2: Create Collapse State Manager (10 min)

Create `Services/CollapseStateManager.cs`:

```csharp
public class CollapseStateManager
{
    private const string StateFilePath = "collapse_states.json";
    private Dictionary<string, bool> _states = new();
    
    public async Task SaveStateAsync(string todoId, bool isExpanded)
    {
        _states[todoId] = isExpanded;
        await SaveToFileAsync();
    }
    
    public async Task<bool> LoadStateAsync(string todoId)
    {
        await LoadFromFileAsync();
        return _states.TryGetValue(todoId, out var state) ? state : true;
    }
    
    // ... implementation
}
```

### Step 3: Update TodoItemControl UI (10 min)

In `Views/TodoItemControl.xaml`, add collapse button:

```xml
<!-- Collapse/Expand Button -->
<Button Grid.Column="0"
        Width="20"
        Height="20"
        Command="{Binding ToggleExpandCommand}"
        Visibility="{Binding SubItems.Count, Converter={StaticResource CountToVisibilityConverter}}">
    <Path Data="{Binding IsExpanded, Converter={StaticResource ExpandCollapseIconConverter}}"
          Fill="{DynamicResource PrimaryTextBrush}"/>
</Button>

<!-- Sub-items Container -->
<ItemsControl ItemsSource="{Binding SubItems}"
              Visibility="{Binding IsExpanded, Converter={StaticResource Boolean2VisibilityConverter}}"/>
```

### Step 4: Add Commands (5 min)

In `ViewModels/MainWindowViewModel.TodoManagement.cs`:

```csharp
public ICommand ToggleExpandCommand { get; private set; }
public ICommand ExpandAllCommand { get; private set; }
public ICommand CollapseAllCommand { get; private set; }

// In constructor:
ToggleExpandCommand = new RelayCommand(param =>
{
    if (param is TodoItemModel todo)
        todo.IsExpanded = !todo.IsExpanded;
});

ExpandAllCommand = new RelayCommand(_ => ExpandAll(Model.TodoItems));
CollapseAllCommand = new RelayCommand(_ => CollapseAll(Model.TodoItems));
```

### Step 5: UI Polish (15 min)

**Consistent Spacing**:
- Review all margins: use 8dp, 16dp, 24dp
- Review all paddings: use 8dp, 12dp, 16dp
- Fix alignment issues

**Icon Consistency**:
- Standardize sizes: 16x16, 24x24, 32x32
- Use same icon family
- Apply proper colors

**Tooltips**:
- Add descriptive tooltips
- Include keyboard shortcuts
- Set proper delays (500ms)

### Step 6: Animations (15 min)

Add collapse/expand animations:

```csharp
private void ToggleExpand()
{
    if (AppSettings.Appearance.EnableAnimations)
    {
        if (IsExpanded)
            AnimationHelper.SlideInFromTop(SubItemsPanel);
        else
            AnimationHelper.SlideOutToTop(SubItemsPanel);
    }
    
    IsExpanded = !IsExpanded;
}
```

---

## ?? Development Checklist

### Phase 1: Collapsible Todos ?
- [ ] IsExpanded property added
- [ ] CollapseStateManager created
- [ ] Collapse button in UI
- [ ] Sub-items visibility binding
- [ ] Toggle command working
- [ ] Expand/Collapse all commands
- [ ] State persistence working

### Phase 2: UI Polish ?
- [ ] Spacing reviewed and fixed
- [ ] Icons standardized
- [ ] Tooltips added
- [ ] Alignment improved
- [ ] Colors consistent

### Phase 3: Animations ?
- [ ] Collapse animation
- [ ] Expand animation
- [ ] Smooth transitions
- [ ] Respects settings

### Testing ?
- [ ] Collapse/expand works
- [ ] State persists
- [ ] Animations smooth
- [ ] No visual glitches
- [ ] Performance good

---

## ?? Key Code Snippets

### 1. Collapse State Persistence

```csharp
// Save state
public async Task SaveStateAsync(string todoId, bool isExpanded)
{
    _states[todoId] = isExpanded;
    var json = JsonSerializer.Serialize(_states);
    await File.WriteAllTextAsync(StateFilePath, json);
}

// Load state
public async Task<bool> LoadStateAsync(string todoId)
{
    if (File.Exists(StateFilePath))
    {
        var json = await File.ReadAllTextAsync(StateFilePath);
        _states = JsonSerializer.Deserialize<Dictionary<string, bool>>(json);
    }
    return _states.TryGetValue(todoId, out var state) ? state : true;
}
```

### 2. Recursive Expand/Collapse

```csharp
private void ExpandAll(ObservableCollection<TodoItemModel> items)
{
    foreach (var item in items)
    {
        item.IsExpanded = true;
        if (item.SubItems.Count > 0)
            ExpandAll(item.SubItems);
    }
}

private void CollapseAll(ObservableCollection<TodoItemModel> items)
{
    foreach (var item in items)
    {
        item.IsExpanded = false;
        if (item.SubItems.Count > 0)
            CollapseAll(item.SubItems);
    }
}
```

### 3. Icon Converter

```csharp
public class ExpandCollapseIconConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        bool isExpanded = (bool)value;
        return isExpanded 
            ? "M7.41,8.58L12,13.17L16.59,8.58L18,10L12,16L6,10L7.41,8.58Z"  // Down arrow
            : "M8.59,16.58L13.17,12L8.59,7.41L10,6L16,12L10,18L8.59,16.58Z"; // Right arrow
    }
    
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
```

---

## ?? Common Issues

### Q1: Collapse button not showing?
**A**: Check SubItems.Count > 0 and visibility converter is registered.

### Q2: State not persisting?
**A**: Verify RememberCollapseState is true in settings and file path is correct.

### Q3: Animations not playing?
**A**: Check EnableAnimations setting and animation duration.

### Q4: Icons not aligned?
**A**: Ensure all icons use same Width/Height and VerticalAlignment.

---

## ?? Reference Documents

### This Session
- [Session-15 Detailed Planning](./Session-15-可折叠待办与UI优化规划.md)
- [Development Roadmap](./开发路线图-v1.0.md)

### Previous Sessions
- [Session-14 Final Report](../../01-会话记录/Session-14/Session-14最终完成报告.md)
- [Session-14 Archive](../../01-会话记录/Session-14/Session-14归档与Session-15启动报告.md)

### Resources
- [AnimationHelper](../../Utils/AnimationHelper.cs)
- [Development Standards](../../05-开发文档/SceneTodo开发规范与最佳实践.md)

---

## ?? Quick Commands

### Build and Test
```powershell
# Build
dotnet build

# Run
dotnet run

# Check errors
dotnet build 2>&1 | Select-String "error"
```

### Git
```powershell
git status
git add .
git commit -m "feat(ui): 实现可折叠待办和UI优化"
git push origin main
```

---

## ? Completion Criteria

### Must Complete
- [x] Collapsible todos working
- [x] State persistence
- [x] UI spacing improved
- [x] Compile successful

### Should Complete
- [ ] Animations smooth
- [ ] Tooltips added
- [ ] Icons consistent
- [ ] Keyboard shortcuts

### Nice to Have
- [ ] Custom collapse icons
- [ ] Animation customization
- [ ] Advanced tooltips

---

**Quick Start Version**: 1.0  
**Created**: 2025-01-05  
**Status**: ?? Ready to Start

**?? Let's complete Session-14 deferred items!** ??
