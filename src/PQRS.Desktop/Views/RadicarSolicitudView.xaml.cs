using System.Windows.Controls;
using PQRS.Desktop.ViewModels;

namespace PQRS.Desktop.Views
{
    public partial class RadicarSolicitudView : UserControl
    {
        public RadicarSolicitudView()
        {
            InitializeComponent();
            Loaded += async (_, _) =>
            {
                if (DataContext is RadicarSolicitudViewModel vm && vm.Tipos.Count == 0)
                    await vm.CargarCommand.ExecuteAsync(null);
            };
        }
    }
}
