using System.Windows.Controls;
using PQRS.Desktop.ViewModels;

namespace PQRS.Desktop.Views
{
    public partial class ReportesView : UserControl
    {
        public ReportesView()
        {
            InitializeComponent();
            Loaded += async (_, _) =>
            {
                if (DataContext is ReportesViewModel vm && !vm.HayResumen)
                    await vm.GenerarCommand.ExecuteAsync(null);
            };
        }
    }
}
