using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace LoteriaTwo.Views
{
    public partial class DecimosView : UserControl
    {
        public DecimosView()
        {
            InitializeComponent();
        }

        private void PEspecial_Click(object sender, RoutedEventArgs e) { }
        private void EntraEspecial_Click(object sender, RoutedEventArgs e) { }
        private void SaleEspecial_Click(object sender, RoutedEventArgs e) { }

        private void PPrimero_Click(object sender, RoutedEventArgs e) { }
        private void EntraPrimero_Click(object sender, RoutedEventArgs e) { }
        private void SalePrimero_Click(object sender, RoutedEventArgs e) { }

        private void PSegundo_Click(object sender, RoutedEventArgs e) { }
        private void EntraSegundo_Click(object sender, RoutedEventArgs e) { }
        private void SaleSegundo_Click(object sender, RoutedEventArgs e) { }

        private void PTercero_Click(object sender, RoutedEventArgs e) { }
        private void EntraTercero_Click(object sender, RoutedEventArgs e) { }
        private void SaleTercero_Click(object sender, RoutedEventArgs e) { }

        // ── DÉCIMOS ──────────────────────────────────────────────────────────

        private void BuscarDecimo_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new Microsoft.Win32.OpenFileDialog
            {
                Title = "Seleccionar imagen del décimo",
                Filter = "Imágenes|*.jpg;*.jpeg;*.png;*.bmp;*.gif;*.tiff|Todos los archivos|*.*"
            };
            if (dlg.ShowDialog() != true) return;

            var bmp = new BitmapImage(new Uri(dlg.FileName));
            ImgDecimo.Source = bmp;
            ImgDecimo.Visibility = Visibility.Visible;
            TxtDecimoPlaceholder.Visibility = Visibility.Collapsed;
        }

        private void PosicionJueves_Click(object sender, RoutedEventArgs e) { }
        private void PosicionSabado_Click(object sender, RoutedEventArgs e) { }
    }
}
