using System.Windows.Controls;

namespace SceneTodo.Views
{
    public partial class ScheduledTasksPage : UserControl
    {
        public ScheduledTasksPage()
        {
            InitializeComponent();
            DataContext = new ViewModels.ScheduledTasksViewModel();
        }
    }
}
