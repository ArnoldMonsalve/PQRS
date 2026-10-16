using System.Windows;
using System.Windows.Controls;
using PQRS.Desktop.ViewModels.Admin;

namespace PQRS.Desktop.Views.Admin
{
    public partial class UsuariosAdminView : UserControl
    {
        public UsuariosAdminView()
        {
            InitializeComponent();

            DataContextChanged += (_, e) =>
            {
                if (e.OldValue is UsuariosAdminViewModel anterior) anterior.ClaveLimpiada -= LimpiarClave;
                if (e.NewValue is UsuariosAdminViewModel nuevo) nuevo.ClaveLimpiada += LimpiarClave;
            };

            Loaded += async (_, _) =>
            {
                if (DataContext is UsuariosAdminViewModel vm && vm.Usuarios.Count == 0)
                    await vm.CargarCommand.ExecuteAsync(null);
            };
        }

        private void LimpiarClave() => TxtClave.Clear();

        private void TxtClave_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is UsuariosAdminViewModel vm) vm.Clave = TxtClave.Password;
        }
    }
}
