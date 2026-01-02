using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using SceneTodo.Models;

namespace SceneTodo.Views
{
    public partial class OverlayWindow : Window
    {
        public ObservableCollection<TodoItemModel> TodoItems { get; set; }
        private bool _isDragging = false;
        private Point _dragStartPosition;
        private double _initialLeft;
        private double _initialTop;

        public OverlayWindow(ObservableCollection<TodoItemModel> todoItems)
        {
            InitializeComponent();
            TodoItems = todoItems;
            DataContext = this;

            // 应用悬浮窗设置
            ApplyOverlaySettings();

            TodoItems.CollectionChanged += (sender, e) =>
            {

            };
        }

        /// <summary>
        /// 应用从模型中读取的悬浮窗设置
        /// </summary>
        public void ApplyOverlaySettings()
        {
            try
            {
                var model = App.MainViewModel?.Model;
                if (model != null)
                {
                    // 设置背景颜色
                    Color backgroundColor = (Color)ColorConverter.ConvertFromString(model.OverlayBackground);
                    MainBorder.Background = new SolidColorBrush(backgroundColor);

                    // 设置不透明度
                    MainBorder.Opacity = model.OverlayOpacity;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"应用悬浮窗设置时出错: {ex.Message}");
            }
        }

        /// <summary>
        /// 获取窗口句柄的辅助方法
        /// </summary>
        public IntPtr GetHandle()
        {
            return new System.Windows.Interop.WindowInteropHelper(this).Handle;
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _isDragging = true;
            _dragStartPosition = e.GetPosition(null);
            _initialLeft = this.Left;
            _initialTop = this.Top;
            
            this.MouseMove += Window_MouseMove;
            this.MouseLeftButtonUp += Window_MouseLeftButtonUp;
            
            DragMove();
        }

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDragging && e.LeftButton == MouseButtonState.Pressed)
            {
                // Drag is handled by DragMove()
            }
        }

        private void Window_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_isDragging)
            {
                _isDragging = false;
                this.MouseMove -= Window_MouseMove;
                this.MouseLeftButtonUp -= Window_MouseLeftButtonUp;
                
                SaveNewOffset();
            }
        }

        private void SaveNewOffset()
        {
            try
            {
                if (TodoItems == null || TodoItems.Count == 0) return;

                var rootItem = TodoItems[0];
                if (rootItem == null || string.IsNullOrEmpty(rootItem.AppPath)) return;

                double deltaX = this.Left - _initialLeft;
                double deltaY = this.Top - _initialTop;

                UpdateOffsetForAllItems(App.MainViewModel?.Model?.TodoItems, rootItem.AppPath, deltaX, deltaY);

                System.Diagnostics.Debug.WriteLine($"保存新偏移量: ΔX={deltaX:F2}, ΔY={deltaY:F2}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"保存偏移量时错误: {ex.Message}");
            }
        }

        private void UpdateOffsetForAllItems(ObservableCollection<TodoItemModel>? items, string appPath, double deltaX, double deltaY)
        {
            if (items == null) return;

            foreach (var item in items)
            {
                if (item.AppPath == appPath)
                {
                    item.OverlayOffsetX += deltaX;
                    item.OverlayOffsetY += deltaY;
                    App.TodoItemRepository.UpdateAsync(item).ConfigureAwait(false);
                    System.Diagnostics.Debug.WriteLine($"更新待办项 '{item.Content}' 的偏移量: X={item.OverlayOffsetX:F2}, Y={item.OverlayOffsetY:F2}");
                }

                if (item.SubItems != null && item.SubItems.Count > 0)
                {
                    UpdateOffsetForAllItems(item.SubItems, appPath, deltaX, deltaY);
                }
            }
        }
    }
}
