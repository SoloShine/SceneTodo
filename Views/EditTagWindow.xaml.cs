using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using SceneTodo.Models;
using MessageBox = HandyControl.Controls.MessageBox;

namespace SceneTodo.Views
{
    public partial class EditTagWindow : HandyControl.Controls.Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private Tag tag;
        public Tag Tag
        {
            get => tag;
            set
            {
                tag = value;
                OnPropertyChanged(nameof(Tag));
                System.Diagnostics.Debug.WriteLine($"Tag 已设置: Name='{tag?.Name}', Color='{tag?.Color}'");
            }
        }

        private string windowTitle;
        public string WindowTitle
        {
            get => windowTitle;
            set
            {
                windowTitle = value;
                OnPropertyChanged(nameof(WindowTitle));
            }
        }

        private readonly bool isEditMode;

        /// <summary>
        /// 构造函数 - 新建模式
        /// </summary>
        public EditTagWindow()
        {
            System.Diagnostics.Debug.WriteLine("===== 新建标签窗口 =====");
            
            // 先设置数据
            isEditMode = false;
            WindowTitle = "新建标签";
            Tag = new Tag
            {
                Id = Guid.NewGuid().ToString(),
                Name = string.Empty,
                Color = "#2196F3",
                CreatedAt = DateTime.Now
            };
            
            // 再初始化组件
            InitializeComponent();
            DataContext = this;
            
            // 最后初始化 UI
            Loaded += (s, e) =>
            {
                InitializeColorPicker();
                System.Diagnostics.Debug.WriteLine($"窗口已加载，TextBox.Text='{NameTextBox.Text}'");
            };
        }

        /// <summary>
        /// 构造函数 - 编辑模式
        /// </summary>
        public EditTagWindow(Tag existingTag)
        {
            System.Diagnostics.Debug.WriteLine("===== 编辑标签窗口 =====");
            System.Diagnostics.Debug.WriteLine($"原始标签: ID={existingTag.Id}, Name='{existingTag.Name}', Color='{existingTag.Color}'");
            
            // 先设置数据
            isEditMode = true;
            WindowTitle = "编辑标签";
            
            // 创建副本
            Tag = new Tag
            {
                Id = existingTag.Id,
                Name = existingTag.Name,
                Color = existingTag.Color,
                CreatedAt = existingTag.CreatedAt
            };
            
            System.Diagnostics.Debug.WriteLine($"副本标签: Name='{Tag.Name}', Color='{Tag.Color}'");
            
            // 再初始化组件
            InitializeComponent();
            DataContext = this;
            
            // 最后初始化 UI
            Loaded += (s, e) =>
            {
                // 确保 TextBox 显示正确的值
                NameTextBox.Text = Tag.Name;
                System.Diagnostics.Debug.WriteLine($"窗口已加载，TextBox.Text='{NameTextBox.Text}'");
                
                InitializeColorPicker();
            };
        }

        /// <summary>
        /// 初始化 ColorPicker
        /// </summary>
        private void InitializeColorPicker()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"初始化 ColorPicker: Color='{Tag.Color}'");
                var color = (Color)ColorConverter.ConvertFromString(Tag.Color);
                ColorPicker.SelectedBrush = new SolidColorBrush(color);
                System.Diagnostics.Debug.WriteLine($"? ColorPicker 初始化成功");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"? 初始化 ColorPicker 失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 预设颜色 Border 鼠标按下事件
        /// </summary>
        private void PresetColor_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.Tag is string colorHex)
            {
                System.Diagnostics.Debug.WriteLine($"?? 选择颜色: {colorHex}");
                
                Tag.Color = colorHex;
                
                // 强制刷新绑定
                OnPropertyChanged(nameof(Tag));
                
                // 同步更新 ColorPicker
                try
                {
                    var color = (Color)ColorConverter.ConvertFromString(colorHex);
                    ColorPicker.SelectedBrush = new SolidColorBrush(color);
                    System.Diagnostics.Debug.WriteLine($"? ColorPicker 已同步");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"? 颜色转换失败: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// 保存按钮点击
        /// </summary>
        private async void Save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine("===== 保存开始 =====");
                System.Diagnostics.Debug.WriteLine($"保存前 - Tag.Name: '{Tag.Name}', TextBox.Text: '{NameTextBox.Text}'");
                
                // 强制失去焦点以触发绑定
                NameTextBox.MoveFocus(new System.Windows.Input.TraversalRequest(
                    System.Windows.Input.FocusNavigationDirection.Next));
                
                // 强制更新绑定
                var binding = NameTextBox.GetBindingExpression(HandyControl.Controls.TextBox.TextProperty);
                if (binding != null)
                {
                    binding.UpdateSource();
                    System.Diagnostics.Debug.WriteLine($"绑定已更新，状态: {binding.Status}");
                }
                
                // 后备方案：直接从 TextBox 读取
                if (string.IsNullOrWhiteSpace(Tag.Name) && !string.IsNullOrWhiteSpace(NameTextBox.Text))
                {
                    System.Diagnostics.Debug.WriteLine($"?? 使用后备方案，从 TextBox 读取: '{NameTextBox.Text}'");
                    Tag.Name = NameTextBox.Text;
                }
                
                System.Diagnostics.Debug.WriteLine($"保存后 - Tag.Name: '{Tag.Name}'");
                
                // 从 ColorPicker 获取最新选择的颜色
                if (ColorPicker.SelectedBrush is SolidColorBrush brush)
                {
                    var color = brush.Color;
                    Tag.Color = $"#{color.R:X2}{color.G:X2}{color.B:X2}";
                    System.Diagnostics.Debug.WriteLine($"最终颜色: '{Tag.Color}'");
                }
                
                // 验证标签名称
                if (string.IsNullOrWhiteSpace(Tag.Name))
                {
                    System.Diagnostics.Debug.WriteLine("? 验证失败: 标签名称为空");
                    MessageBox.Show("请输入标签名称！", "验证失败", 
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    NameTextBox.Focus();
                    return;
                }

                if (Tag.Name.Length > 20)
                {
                    System.Diagnostics.Debug.WriteLine($"? 验证失败: 标签名称过长 ({Tag.Name.Length} > 20)");
                    MessageBox.Show("标签名称不能超过20个字符！", "验证失败", 
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    NameTextBox.Focus();
                    return;
                }

                System.Diagnostics.Debug.WriteLine($"? 验证通过，准备保存");
                System.Diagnostics.Debug.WriteLine($"   模式: {(isEditMode ? "编辑" : "新建")}");
                System.Diagnostics.Debug.WriteLine($"   ID: {Tag.Id}");
                System.Diagnostics.Debug.WriteLine($"   Name: '{Tag.Name}'");
                System.Diagnostics.Debug.WriteLine($"   Color: '{Tag.Color}'");

                if (isEditMode)
                {
                    // 编辑模式 - 更新标签
                    var result = await App.TagRepository.UpdateAsync(Tag);
                    System.Diagnostics.Debug.WriteLine($"更新结果: {result}");
                    
                    if (result > 0)
                    {
                        MessageBox.Show("标签更新成功！", "成功", 
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("标签更新失败：数据库中未找到该标签", "错误", 
                            MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }
                else
                {
                    // 新建模式 - 添加标签
                    var result = await App.TagRepository.AddAsync(Tag);
                    System.Diagnostics.Debug.WriteLine($"添加结果: {result}");
                    
                    MessageBox.Show("标签创建成功！", "成功", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"? 保存失败: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"   堆栈: {ex.StackTrace}");
                MessageBox.Show($"保存标签失败: {ex.Message}", "错误", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 取消按钮点击
        /// </summary>
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
