using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using LoteriaTwo.Models;
using LoteriaTwo.Services;

namespace LoteriaTwo.Views
{
    public partial class DecimosView : UserControl
    {
        public Elemento? UltimoElemento { get; private set; }

        public DecimosView()
        {
            InitializeComponent();
        }

        private void PEspecial_Click(object sender, RoutedEventArgs e)
        {
            var el = new Elemento { Tipo = TipoElemento.PremioEspecial };
            el["Fecha"]    = TxtFecha.Text;
            el["Numero"]   = TxtNumeroEspecial.Text;
            el["Cantidad"] = TxtCantidadEspecial.Text;
            el["Serie"]    = TxtSerieEspecial.Text;
            el["Fraccion"] = TxtFraccEspecial.Text;
            Previsualizar(el);
        }
        private void EntraEspecial_Click(object sender, RoutedEventArgs e) { }
        private void SaleEspecial_Click(object sender, RoutedEventArgs e) { }

        private void PPrimero_Click(object sender, RoutedEventArgs e)
        {
            var el = new Elemento { Tipo = TipoElemento.PrimerPremio };
            el["Fecha"]          = TxtFecha.Text;
            el["Numero"]         = TxtNumeroPrimero.Text;
            el["Cantidad"]       = TxtCantidadPrimero.Text;
            el["Reintegro1"]     = TxtReintegro1.Text;
            el["Reintegro2"]     = TxtReintegro2.Text;
            el["Reintegro3"]     = TxtReintegro3.Text;
            el["ReintegroPremio"] = (ChkReintegroPrimero.IsChecked == true).ToString();
            Previsualizar(el);
        }
        private void EntraPrimero_Click(object sender, RoutedEventArgs e) { }
        private void SalePrimero_Click(object sender, RoutedEventArgs e) { }

        private void PSegundo_Click(object sender, RoutedEventArgs e)
        {
            var el = new Elemento { Tipo = TipoElemento.SegundoPremio };
            el["Fecha"]    = TxtFecha.Text;
            el["Numero"]   = TxtNumeroSegundo.Text;
            el["Cantidad"] = TxtCantidadSegundo.Text;
            Previsualizar(el);
        }
        private void EntraSegundo_Click(object sender, RoutedEventArgs e) { }
        private void SaleSegundo_Click(object sender, RoutedEventArgs e) { }

        private void PTercero_Click(object sender, RoutedEventArgs e)
        {
            var el = new Elemento { Tipo = TipoElemento.TercerPremio };
            el["Fecha"]    = TxtFecha.Text;
            el["Numero"]   = TxtNumeroTercero.Text;
            el["Cantidad"] = TxtCantidadTercero.Text;
            Previsualizar(el);
        }
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
            LogService.Instancia.Registrar(LogNivel.Cambio, "Décimos",
                $"Décimo cargado: {System.IO.Path.GetFileName(dlg.FileName)}");
        }

        private void PosicionJueves_Click(object sender, RoutedEventArgs e) { }
        private void PosicionSabado_Click(object sender, RoutedEventArgs e) { }

        private void Previsualizar(Elemento el)
        {
            ElementoRepository.Instancia.Add(el);
            UltimoElemento = el;
            LogService.Instancia.Registrar(LogNivel.Accion, el.Tipo.ToString(),
                "P → " + el.ToLogString());
            PlaylistService.Instancia.AgregarElemento(el.Tipo, el.Tipo.ToString());
            PlaylistService.Instancia.AgregarLogo(el.Tipo.ToString(), el.Tipo);
        }
    }
}
