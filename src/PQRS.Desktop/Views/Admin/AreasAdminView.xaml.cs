using System.Windows.Controls;
using PQRS.Desktop.ViewModels.Admin;

namespace PQRS.Desktop.Views.Admin
{
    public partial class AreasAdminView : UserControl
    {
        public AreasAdminView()
        {
            InitializeComponent();
            Loaded += async (_, _) =>
            {
                if (DataContext is AreasAdminViewModel vm && vm.Areas.Count == 0)
                    await vm.CargarCommand.ExecuteAsync(null);
            };
        }
    }
}
