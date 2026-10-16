using System.Windows;
using PQRS.Desktop.ViewModels;

namespace PQRS.Desktop
{
    public partial class MainWindow : Window
    {
        public MainWindow(MainViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
