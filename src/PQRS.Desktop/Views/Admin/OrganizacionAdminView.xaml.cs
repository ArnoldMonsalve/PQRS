using System.Windows.Controls;
using PQRS.Desktop.ViewModels.Admin;

namespace PQRS.Desktop.Views.Admin
{
    public partial class OrganizacionAdminView : UserControl
    {
        public OrganizacionAdminView()
        {
            InitializeComponent();
            Loaded += async (_, _) =>
            {
                if (DataContext is OrganizacionAdminViewModel vm && string.IsNullOrEmpty(vm.NombreOrganizacion))
                    await vm.CargarCommand.ExecuteAsync(null);
            };
        }
    }
}
