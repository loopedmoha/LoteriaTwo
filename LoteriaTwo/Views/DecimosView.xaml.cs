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
            RegistrarFormState();
            LiveDataService.Instancia.Registrar(TipoElemento.PremioEspecial,
                () => $"Num: {TxtNumeroEspecial.Text}  Cant: {TxtCantidadEspecial.Text}  Serie: {TxtSerieEspecial.Text}  Fracc: {TxtFraccEspecial.Text}");
            LiveDataService.Instancia.Registrar(TipoElemento.PrimerPremio,
                () => $"Num: {TxtNumeroPrimero.Text}  Cant: {TxtCantidadPrimero.Text}  Rein: {TxtReintegro1.Text}/{TxtReintegro2.Text}/{TxtReintegro3.Text}");
            LiveDataService.Instancia.Registrar(TipoElemento.SegundoPremio,
                () => $"Num: {TxtNumeroSegundo.Text}  Cant: {TxtCantidadSegundo.Text}");
            LiveDataService.Instancia.Registrar(TipoElemento.TercerPremio,
                () => $"Num: {TxtNumeroTercero.Text}  Cant: {TxtCantidadTercero.Text}");
        }

        private void RegistrarFormState()
        {
            FormStateService.Instancia.RegistrarSeccion("Decimos",
                leer: () => new Dictionary<string, string>
                {
                    ["Fecha"]             = TxtFecha.Text,
                    ["NumeroEspecial"]    = TxtNumeroEspecial.Text,
                    ["CantidadEspecial"]  = TxtCantidadEspecial.Text,
                    ["SerieEspecial"]     = TxtSerieEspecial.Text,
                    ["FraccEspecial"]     = TxtFraccEspecial.Text,
                    ["NumeroPrimero"]     = TxtNumeroPrimero.Text,
                    ["CantidadPrimero"]   = TxtCantidadPrimero.Text,
                    ["Reintegro1"]        = TxtReintegro1.Text,
                    ["Reintegro2"]        = TxtReintegro2.Text,
                    ["Reintegro3"]        = TxtReintegro3.Text,
                    ["ReintegroPrimero"]  = (ChkReintegroPrimero.IsChecked == true).ToString(),
                    ["NumeroSegundo"]     = TxtNumeroSegundo.Text,
                    ["CantidadSegundo"]   = TxtCantidadSegundo.Text,
                    ["NumeroTercero"]     = TxtNumeroTercero.Text,
                    ["CantidadTercero"]   = TxtCantidadTercero.Text,
                },
                escribir: d =>
                {
                    TxtFecha.Text            = d.Gv("Fecha");
                    TxtNumeroEspecial.Text   = d.Gv("NumeroEspecial");
                    TxtCantidadEspecial.Text = d.Gv("CantidadEspecial");
                    TxtSerieEspecial.Text    = d.Gv("SerieEspecial");
                    TxtFraccEspecial.Text    = d.Gv("FraccEspecial");
                    TxtNumeroPrimero.Text    = d.Gv("NumeroPrimero");
                    TxtCantidadPrimero.Text  = d.Gv("CantidadPrimero");
                    TxtReintegro1.Text       = d.Gv("Reintegro1");
                    TxtReintegro2.Text       = d.Gv("Reintegro2");
                    TxtReintegro3.Text       = d.Gv("Reintegro3");
                    ChkReintegroPrimero.IsChecked = d.Gv("ReintegroPrimero") == "True";
                    TxtNumeroSegundo.Text    = d.Gv("NumeroSegundo");
                    TxtCantidadSegundo.Text  = d.Gv("CantidadSegundo");
                    TxtNumeroTercero.Text    = d.Gv("NumeroTercero");
                    TxtCantidadTercero.Text  = d.Gv("CantidadTercero");
                });
        }

        private Elemento BuildEspecialElemento()
        {
            var el = new Elemento { Tipo = TipoElemento.PremioEspecial };
            el["Fecha"]    = TxtFecha.Text;
            el["Numero"]   = TxtNumeroEspecial.Text;
            el["Cantidad"] = TxtCantidadEspecial.Text;
            el["Serie"]    = TxtSerieEspecial.Text;
            el["Fraccion"] = TxtFraccEspecial.Text;
            return el;
        }

        private Elemento BuildPrimeroElemento()
        {
            var el = new Elemento { Tipo = TipoElemento.PrimerPremio };
            el["Fecha"]          = TxtFecha.Text;
            el["Numero"]         = TxtNumeroPrimero.Text;
            el["Cantidad"]       = TxtCantidadPrimero.Text;
            el["Reintegro1"]     = TxtReintegro1.Text;
            el["Reintegro2"]     = TxtReintegro2.Text;
            el["Reintegro3"]     = TxtReintegro3.Text;
            el["ReintegroPremio"] = (ChkReintegroPrimero.IsChecked == true).ToString();
            return el;
        }

        private Elemento BuildSegundoElemento()
        {
            var el = new Elemento { Tipo = TipoElemento.SegundoPremio };
            el["Fecha"]    = TxtFecha.Text;
            el["Numero"]   = TxtNumeroSegundo.Text;
            el["Cantidad"] = TxtCantidadSegundo.Text;
            return el;
        }

        private Elemento BuildTerceroElemento()
        {
            var el = new Elemento { Tipo = TipoElemento.TercerPremio };
            el["Fecha"]    = TxtFecha.Text;
            el["Numero"]   = TxtNumeroTercero.Text;
            el["Cantidad"] = TxtCantidadTercero.Text;
            return el;
        }

        private void PEspecial_Click(object sender, RoutedEventArgs e)
            => Previsualizar(BuildEspecialElemento(), BuildEspecialElemento);
        private void EntraEspecial_Click(object sender, RoutedEventArgs e)
        {
            var el = new Elemento { Tipo = TipoElemento.PremioEspecial };
            el["Fecha"] = TxtFecha.Text; el["Numero"] = TxtNumeroEspecial.Text;
            el["Cantidad"] = TxtCantidadEspecial.Text; el["Serie"] = TxtSerieEspecial.Text;
            el["Fraccion"] = TxtFraccEspecial.Text;
            BrainstormService.Instancia.Entra(el);
        }
        private void SaleEspecial_Click(object sender, RoutedEventArgs e)
            => BrainstormService.Instancia.Sale(new Elemento { Tipo = TipoElemento.PremioEspecial });

        private void PPrimero_Click(object sender, RoutedEventArgs e)
            => Previsualizar(BuildPrimeroElemento(), BuildPrimeroElemento);
        private void EntraPrimero_Click(object sender, RoutedEventArgs e)
        {
            var el = new Elemento { Tipo = TipoElemento.PrimerPremio };
            el["Fecha"] = TxtFecha.Text; el["Numero"] = TxtNumeroPrimero.Text;
            el["Cantidad"] = TxtCantidadPrimero.Text; el["Serie"] = TxtSerieEspecial.Text;
            el["Fraccion"] = TxtFraccEspecial.Text;
            el["Reintegro1"] = TxtReintegro1.Text; el["Reintegro2"] = TxtReintegro2.Text;
            el["Reintegro3"] = TxtReintegro3.Text;
            el["ReintegroPremio"] = (ChkReintegroPrimero.IsChecked == true).ToString();
            BrainstormService.Instancia.Entra(el);
        }
        private void SalePrimero_Click(object sender, RoutedEventArgs e)
            => BrainstormService.Instancia.Sale(new Elemento { Tipo = TipoElemento.PrimerPremio });

        private void PSegundo_Click(object sender, RoutedEventArgs e)
            => Previsualizar(BuildSegundoElemento(), BuildSegundoElemento);
        private void EntraSegundo_Click(object sender, RoutedEventArgs e)
        {
            var el = new Elemento { Tipo = TipoElemento.SegundoPremio };
            el["Fecha"] = TxtFecha.Text; el["Numero"] = TxtNumeroSegundo.Text;
            el["Cantidad"] = TxtCantidadSegundo.Text;
            BrainstormService.Instancia.Entra(el);
        }
        private void SaleSegundo_Click(object sender, RoutedEventArgs e)
            => BrainstormService.Instancia.Sale(new Elemento { Tipo = TipoElemento.SegundoPremio });

        private void PTercero_Click(object sender, RoutedEventArgs e)
            => Previsualizar(BuildTerceroElemento(), BuildTerceroElemento);
        private void EntraTercero_Click(object sender, RoutedEventArgs e)
        {
            var el = new Elemento { Tipo = TipoElemento.TercerPremio };
            el["Fecha"] = TxtFecha.Text; el["Numero"] = TxtNumeroTercero.Text;
            el["Cantidad"] = TxtCantidadTercero.Text;
            BrainstormService.Instancia.Entra(el);
        }
        private void SaleTercero_Click(object sender, RoutedEventArgs e)
            => BrainstormService.Instancia.Sale(new Elemento { Tipo = TipoElemento.TercerPremio });

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

            string path = dlg.FileName;
            if (RemoteShareService.Instancia.Configurado)
            {
                try
                {
                    path = RemoteShareService.Instancia.CopiarDecimo(dlg.FileName);
                }
                catch (Exception ex)
                {
                    LogService.Instancia.Registrar(LogNivel.Error, "Décimos",
                        $"Error al copiar a carpeta remota: {ex.Message}");
                }
            }

            BrainstormService.Instancia.EnviarTexFile("LoteriaDecimo", path);

            LogService.Instancia.Registrar(LogNivel.Cambio, "Décimos",
                $"Décimo cargado: {System.IO.Path.GetFileName(dlg.FileName)} → {path}");
        }

        private void PosicionJueves_Click(object sender, RoutedEventArgs e) { }
        private void PosicionSabado_Click(object sender, RoutedEventArgs e) { }

        private void Previsualizar(Elemento el, Func<Elemento?>? buildActual = null)
        {
            ElementoRepository.Instancia.Add(el);
            UltimoElemento = el;
            LogService.Instancia.Registrar(LogNivel.Accion, el.Tipo.ToString(),
                "P → " + el.ToLogString());
            PlaylistService.Instancia.AgregarElemento(el, buildActual);
            PlaylistService.Instancia.AgregarLogo("LoteriaNacional", el.Tipo);
        }
    }
}
