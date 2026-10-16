using System.Windows.Controls;
using System.Windows.Input;
using PQRS.Desktop.ViewModels;

namespace PQRS.Desktop.Views
{
    public partial class BandejaSolicitudesView : UserControl
    {
        public BandejaSolicitudesView()
        {
            InitializeComponent();
            Loaded += async (_, _) =>
            {
                if (DataContext is BandejaSolicitudesViewModel vm && vm.Tipos.Count == 0)
                    await vm.CargarCommand.ExecuteAsync(null);
            };
        }

        private void TxtBuscar_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && DataContext is BandejaSolicitudesViewModel vm)
                vm.BuscarCommand.Execute(null);
        }

        private void Grid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // Solo si el doble clic fue sobre una fila (no sobre el encabezado o el espacio vacío).
            if (e.OriginalSource is System.Windows.DependencyObject origen
                && ItemsControl.ContainerFromElement((DataGrid)sender, origen) is DataGridRow
                && DataContext is BandejaSolicitudesViewModel vm)
            {
                vm.AbrirCommand.Execute(null);
            }
        }
    }
}
