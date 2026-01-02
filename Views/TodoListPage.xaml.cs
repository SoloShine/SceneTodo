using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using SceneTodo.Models;

namespace SceneTodo.Views
{
    /// <summary>
    /// TodoListPage.xaml 的交互逻辑
    /// </summary>
    public partial class TodoListPage : UserControl
    {
        public TodoListPage()
        {
            InitializeComponent();
        }

        private void TodoInputTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                // 获取文本内容
                string todoContent = TodoInputTextBox.Text.Trim();
                if (!string.IsNullOrEmpty(todoContent))
                {
                    // 创建新的TodoItemModel
                    var newTodo = new TodoItemModel
                    {
                        Id = Guid.NewGuid().ToString(),
                        Content = todoContent,
                        IsCompleted = false
                    };

                    // 将新待办添加到ViewModel的TodoItems集合中
                    App.MainViewModel?.Model.TodoItems.Add(newTodo);
                    //调用编辑
                    App.MainViewModel?.EditTodoItemCommand.Execute(newTodo);
                    // 保存到数据库
                    App.TodoItemRepository.AddAsync(newTodo).ConfigureAwait(false);
                    // 清空文本框
                    TodoInputTextBox.Clear();
                }
            }
        }
    }
}
