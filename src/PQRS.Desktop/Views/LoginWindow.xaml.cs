using System.Windows;
using PQRS.Desktop.ViewModels;

namespace PQRS.Desktop.Views
{
    public partial class LoginWindow : Window
    {
        private readonly LoginViewModel _viewModel;

        public LoginWindow(LoginViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = viewModel;

            viewModel.LoginExitoso += () => DialogResult = true;
            Loaded += (_, _) => TxtUsuario.Focus();
        }

        private void TxtClave_PasswordChanged(object sender, RoutedEventArgs e) =>
            _viewModel.Clave = TxtClave.Password;
    }
}
