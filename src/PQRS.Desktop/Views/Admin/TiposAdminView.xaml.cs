using System.Windows.Controls;
using PQRS.Desktop.ViewModels.Admin;

namespace PQRS.Desktop.Views.Admin
{
    public partial class TiposAdminView : UserControl
    {
        public TiposAdminView()
        {
            InitializeComponent();
            Loaded += async (_, _) =>
            {
                if (DataContext is TiposAdminViewModel vm && vm.Tipos.Count == 0)
                    await vm.CargarCommand.ExecuteAsync(null);
            };
        }
    }
}
