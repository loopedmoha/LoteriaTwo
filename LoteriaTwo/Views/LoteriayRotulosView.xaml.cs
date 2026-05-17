using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using LoteriaTwo.Models;

namespace LoteriaTwo.Views
{
    public partial class LoteriayRotulosView : UserControl
    {
        private static readonly string[] Comunidades =
        {
            "", "Andalucia", "Aragon", "Asturias", "Baleares", "Canarias", "Cantabria",
            "Castilla Y Leon", "Castilla La Mancha", "Catalunia", "Extremadura", "Galicia",
            "Madrid", "Murcia", "Navarra", "Pais Vasco", "Rioja", "Valencia", "Ceuta", "Melilla"
        };

        private readonly Mapa[] _mapas = { new(), new(), new(), new() };
        private int _mapaActual = 0;
        private bool _ready = false;

        public LoteriayRotulosView()
        {
            InitializeComponent();
            PopulateComunidades();
            CargarMapa(0);
            _ready = true;
        }

        // ── Comunidades ──────────────────────────────────────────────────────

        private void PopulateComunidades()
        {
            foreach (var cmb in new[] { CmbComunidad1, CmbComunidad2, CmbComunidad3, CmbComunidad4, CmbComunidad5 })
            {
                foreach (var c in Comunidades)
                    cmb.Items.Add(c);
                cmb.SelectedIndex = 0;
            }
        }

        // ── Mapa switching ───────────────────────────────────────────────────

        private void MapaSeleccionado_Checked(object sender, RoutedEventArgs e)
        {
            if (!_ready) return;
            if (sender is not RadioButton rb || !int.TryParse(rb.Tag?.ToString(), out int idx)) return;

            GuardarMapaActual();
            _mapaActual = idx;
            CargarMapa(idx);
        }

        private void GuardarMapaActual()
        {
            var m = _mapas[_mapaActual];
            m.Fecha       = TxtCiudadesFecha.Text;
            m.Ciudad1     = TxtCiudad1.Text;
            m.Ciudad2     = TxtCiudad2.Text;
            m.Ciudad3     = TxtCiudad3.Text;
            m.Ciudad4     = TxtCiudad4.Text;
            m.Ciudad5     = TxtCiudad5.Text;
            m.Comunidad1  = CmbComunidad1.SelectedItem?.ToString() ?? string.Empty;
            m.Comunidad2  = CmbComunidad2.SelectedItem?.ToString() ?? string.Empty;
            m.Comunidad3  = CmbComunidad3.SelectedItem?.ToString() ?? string.Empty;
            m.Comunidad4  = CmbComunidad4.SelectedItem?.ToString() ?? string.Empty;
            m.Comunidad5  = CmbComunidad5.SelectedItem?.ToString() ?? string.Empty;
            m.Logo        = (LstLogos.SelectedItem as ListBoxItem)?.Content?.ToString() ?? string.Empty;
            m.Texto1      = TxtTexto1.Text;
            m.Texto2      = TxtTexto2.Text;
        }

        private void CargarMapa(int idx)
        {
            var m = _mapas[idx];
            TxtCiudadesFecha.Text = m.Fecha;
            TxtCiudad1.Text       = m.Ciudad1;
            TxtCiudad2.Text       = m.Ciudad2;
            TxtCiudad3.Text       = m.Ciudad3;
            TxtCiudad4.Text       = m.Ciudad4;
            TxtCiudad5.Text       = m.Ciudad5;
            SetComboValue(CmbComunidad1, m.Comunidad1);
            SetComboValue(CmbComunidad2, m.Comunidad2);
            SetComboValue(CmbComunidad3, m.Comunidad3);
            SetComboValue(CmbComunidad4, m.Comunidad4);
            SetComboValue(CmbComunidad5, m.Comunidad5);
            LstLogos.SelectedItem = LstLogos.Items
                .OfType<ListBoxItem>()
                .FirstOrDefault(i => i.Content?.ToString() == m.Logo);
            TxtTexto1.Text = m.Texto1;
            TxtTexto2.Text = m.Texto2;
        }

        private static void SetComboValue(ComboBox cmb, string value)
        {
            if (cmb.Items.Count == 0) return;
            cmb.SelectedItem = value;
            if (cmb.SelectedIndex < 0) cmb.SelectedIndex = 0;
        }

        // ── WEB ──────────────────────────────────────────────────────────────

        private void WebPreview_Click(object sender, RoutedEventArgs e) { }
        private void WebEntra_Click(object sender, RoutedEventArgs e) { }
        private void WebSale_Click(object sender, RoutedEventArgs e) { }

        // ── IMÁGENES ─────────────────────────────────────────────────────────

        private void ImagenesPreview_Click(object sender, RoutedEventArgs e) { }
        private void ImagenesEntra_Click(object sender, RoutedEventArgs e) { }
        private void ImagenesSale_Click(object sender, RoutedEventArgs e) { }
        private void F1_Click(object sender, RoutedEventArgs e) { }
        private void F2_Click(object sender, RoutedEventArgs e) { }
        private void F3_Click(object sender, RoutedEventArgs e) { }
        private void F4_Click(object sender, RoutedEventArgs e) { }
        private void F5_Click(object sender, RoutedEventArgs e) { }

        // ── RÓTULOS ──────────────────────────────────────────────────────────

        private void PresenterName_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
                TxtLinea1Primera.Text = btn.Tag?.ToString() ?? string.Empty;
        }
        private void EntrarRotulo_Click(object sender, RoutedEventArgs e) { }
        private void SaleRotulo_Click(object sender, RoutedEventArgs e) { }
        private void PreviewRotulo_Click(object sender, RoutedEventArgs e) { }

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

        // ── CIUDADES — LOGO ──────────────────────────────────────────────────

        private void LstLogos_SelectionChanged(object sender, SelectionChangedEventArgs e) { }
        private void LogoPreview_Click(object sender, RoutedEventArgs e) { }
        private void LogoEntra_Click(object sender, RoutedEventArgs e) { }
        private void LogoSale_Click(object sender, RoutedEventArgs e) { }
    }
}
