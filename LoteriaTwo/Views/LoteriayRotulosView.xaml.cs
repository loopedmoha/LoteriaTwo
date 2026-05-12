using System.Windows;
using System.Windows.Controls;

namespace LoteriaTwo.Views
{
    public partial class LoteriayRotulosView : UserControl
    {
        public LoteriayRotulosView()
        {
            InitializeComponent();
        }

        // WEB
        private void WebPreview_Click(object sender, RoutedEventArgs e) { }
        private void WebEntra_Click(object sender, RoutedEventArgs e) { }
        private void WebSale_Click(object sender, RoutedEventArgs e) { }

        // IMÁGENES
        private void ImagenesPreview_Click(object sender, RoutedEventArgs e) { }
        private void ImagenesEntra_Click(object sender, RoutedEventArgs e) { }
        private void ImagenesSale_Click(object sender, RoutedEventArgs e) { }
        private void F1_Click(object sender, RoutedEventArgs e) { }
        private void F2_Click(object sender, RoutedEventArgs e) { }
        private void F3_Click(object sender, RoutedEventArgs e) { }
        private void F4_Click(object sender, RoutedEventArgs e) { }
        private void F5_Click(object sender, RoutedEventArgs e) { }

        // RÓTULOS
        private void PresenterName_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
                TxtLinea1Primera.Text = btn.Tag?.ToString() ?? string.Empty;
        }
        private void EntrarRotulo_Click(object sender, RoutedEventArgs e) { }
        private void SaleRotulo_Click(object sender, RoutedEventArgs e) { }
        private void PreviewRotulo_Click(object sender, RoutedEventArgs e) { }

        // DÉCIMOS
        private void BuscarDecimo_Click(object sender, RoutedEventArgs e) { }
        private void PosicionJueves_Click(object sender, RoutedEventArgs e) { }
        private void PosicionSabado_Click(object sender, RoutedEventArgs e) { }

        // CIUDADES — LOGO
        private void LstLogos_SelectionChanged(object sender, SelectionChangedEventArgs e) { }
        private void LogoPreview_Click(object sender, RoutedEventArgs e) { }
        private void LogoEntra_Click(object sender, RoutedEventArgs e) { }
        private void LogoSale_Click(object sender, RoutedEventArgs e) { }
    }
}
