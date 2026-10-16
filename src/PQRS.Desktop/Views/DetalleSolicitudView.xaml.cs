using System.Windows.Controls;
using System.Windows.Input;
using PQRS.Desktop.ViewModels;

namespace PQRS.Desktop.Views
{
    public partial class DetalleSolicitudView : UserControl
    {
        public DetalleSolicitudView()
        {
            InitializeComponent();
        }

        private void Adjuntos_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // Doble clic sobre un adjunto = abrirlo (solo si se hizo sobre un elemento de la lista).
            if (e.OriginalSource is System.Windows.DependencyObject origen
                && ItemsControl.ContainerFromElement((ListBox)sender, origen) is ListBoxItem
                && DataContext is DetalleSolicitudViewModel vm)
            {
                vm.AbrirAdjuntoCommand.Execute(null);
            }
        }
    }
}
