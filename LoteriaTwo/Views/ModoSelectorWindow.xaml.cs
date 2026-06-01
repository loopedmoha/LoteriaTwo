using System.Windows;
using LoteriaTwo.Config;

namespace LoteriaTwo.Views
{
    public partial class ModoSelectorWindow : Window
    {
        public ModoEstudio ModoSeleccionado { get; private set; }

        public ModoSelectorWindow()
        {
            InitializeComponent();
        }

        private void Prado_Click(object sender, RoutedEventArgs e)
        {
            ModoSeleccionado = ModoEstudio.Prado;
            DialogResult = true;
        }

        private void Torre_Click(object sender, RoutedEventArgs e)
        {
            ModoSeleccionado = ModoEstudio.Torre;
            DialogResult = true;
        }
    }
}
