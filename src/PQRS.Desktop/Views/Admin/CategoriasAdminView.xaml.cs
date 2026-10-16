using System.Windows.Controls;
using PQRS.Desktop.ViewModels.Admin;

namespace PQRS.Desktop.Views.Admin
{
    public partial class CategoriasAdminView : UserControl
    {
        public CategoriasAdminView()
        {
            InitializeComponent();
            Loaded += async (_, _) =>
            {
                if (DataContext is CategoriasAdminViewModel vm && vm.Categorias.Count == 0)
                    await vm.CargarCommand.ExecuteAsync(null);
            };
        }
    }
}
