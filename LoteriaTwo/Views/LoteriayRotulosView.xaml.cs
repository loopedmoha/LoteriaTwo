using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using LoteriaTwo.Models;
using LoteriaTwo.Services;

namespace LoteriaTwo.Views
{
    public partial class LoteriayRotulosView : UserControl
    {
        public Elemento? UltimoElemento { get; private set; }

        private static readonly string[] Comunidades =
        {
            "", "Andalucia", "Aragon", "Asturias", "Baleares", "Canarias", "Cantabria",
            "Castilla Y Leon", "Castilla La Mancha", "Catalunia", "Extremadura", "Galicia",
            "Madrid", "Murcia", "Navarra", "Pais Vasco", "Rioja", "Valencia", "Ceuta", "Melilla"
        };

        private static readonly string[] Logos =
        {
            "", "Bonoloto", "Primitiva", "El Gordo", "Euromillones", "EuromillonesMillon",
            "Eurodreams", "LotoTurf", "Quiniela", "Quinigol", "LoteriaNacional",
            "ApuestaHipica", "Joker", "ElMillon", "Deporte", "Cultura", "Sociedad",
            "GenericoLAE", "ProgLaSuerte", "Elige8"
        };

        private ComboBox[] _cmbLogosImagenes = Array.Empty<ComboBox>();

        private readonly Mapa[] _mapas = { new(), new(), new(), new() };
        private int _mapaActual = 0;
        private bool _ready = false;

        public LoteriayRotulosView()
        {
            InitializeComponent();
            _cmbLogosImagenes = new[] { CmbLogoFoto1, CmbLogoFoto2, CmbLogoFoto3,
                                        CmbLogoFoto4, CmbLogoFoto5, CmbLogoColas };
            PopulateComunidades();
            PopulateLogosImagenes();
            CargarMapa(0);
            _ready = true;
            RegistrarLiveData();
            RegistrarFormState();
        }

        private void RegistrarFormState()
        {
            FormStateService.Instancia.RegistrarSeccion("LoteriayRotulos",
                leer: () =>
                {
                    GuardarMapaActual();
                    var d = new Dictionary<string, string>
                    {
                        ["TxtF1"]          = TxtF1.Text,
                        ["TxtF2"]          = TxtF2.Text,
                        ["TxtF3"]          = TxtF3.Text,
                        ["TxtF4"]          = TxtF4.Text,
                        ["TxtF5"]          = TxtF5.Text,
                        ["LogoFoto1"]      = CmbLogoFoto1.SelectedItem?.ToString()     ?? string.Empty,
                        ["LogoFoto2"]      = CmbLogoFoto2.SelectedItem?.ToString()     ?? string.Empty,
                        ["LogoFoto3"]      = CmbLogoFoto3.SelectedItem?.ToString()     ?? string.Empty,
                        ["LogoFoto4"]      = CmbLogoFoto4.SelectedItem?.ToString()     ?? string.Empty,
                        ["LogoFoto5"]      = CmbLogoFoto5.SelectedItem?.ToString()     ?? string.Empty,
                        ["LogoColas"]      = CmbLogoColas.SelectedItem?.ToString()     ?? string.Empty,
                        ["TipoRotulo"]     = GetCheckedRadio(this, "TipoRotulo"),
                        ["Imagenes"]       = GetCheckedRadio(this, "Imagenes"),
                        ["Linea1Primera"]  = TxtLinea1Primera.Text,
                        ["Linea1Segunda"]  = TxtLinea1Segunda.Text,
                        ["Linea2"]         = TxtLinea2.Text,
                        ["MapaActual"]     = _mapaActual.ToString(),
                    };
                    for (int i = 0; i < _mapas.Length; i++)
                    {
                        var m = _mapas[i];
                        var pre = $"M{i}_";
                        d[$"{pre}Fecha"]      = m.Fecha;
                        d[$"{pre}Ciudad1"]    = m.Ciudad1;
                        d[$"{pre}Ciudad2"]    = m.Ciudad2;
                        d[$"{pre}Ciudad3"]    = m.Ciudad3;
                        d[$"{pre}Ciudad4"]    = m.Ciudad4;
                        d[$"{pre}Ciudad5"]    = m.Ciudad5;
                        d[$"{pre}Comunidad1"] = m.Comunidad1;
                        d[$"{pre}Comunidad2"] = m.Comunidad2;
                        d[$"{pre}Comunidad3"] = m.Comunidad3;
                        d[$"{pre}Comunidad4"] = m.Comunidad4;
                        d[$"{pre}Comunidad5"] = m.Comunidad5;
                        d[$"{pre}Logo"]       = m.Logo;
                        d[$"{pre}Texto1"]     = m.Texto1;
                        d[$"{pre}Texto2"]     = m.Texto2;
                    }
                    return d;
                },
                escribir: d =>
                {
                    TxtF1.Text           = d.Gv("TxtF1");
                    TxtF2.Text           = d.Gv("TxtF2");
                    TxtF3.Text           = d.Gv("TxtF3");
                    TxtF4.Text           = d.Gv("TxtF4");
                    TxtF5.Text           = d.Gv("TxtF5");
                    FormHelper.RestoreCombo(CmbLogoFoto1,     d.Gv("LogoFoto1"));
                    FormHelper.RestoreCombo(CmbLogoFoto2,     d.Gv("LogoFoto2"));
                    FormHelper.RestoreCombo(CmbLogoFoto3,     d.Gv("LogoFoto3"));
                    FormHelper.RestoreCombo(CmbLogoFoto4,     d.Gv("LogoFoto4"));
                    FormHelper.RestoreCombo(CmbLogoFoto5,     d.Gv("LogoFoto5"));
                    FormHelper.RestoreCombo(CmbLogoColas,     d.Gv("LogoColas"));
                    FormHelper.SetCheckedRadio(this, "TipoRotulo", d.Gv("TipoRotulo"));
                    FormHelper.SetCheckedRadio(this, "Imagenes",   d.Gv("Imagenes"));
                    TxtLinea1Primera.Text = d.Gv("Linea1Primera");
                    TxtLinea1Segunda.Text = d.Gv("Linea1Segunda");
                    TxtLinea2.Text        = d.Gv("Linea2");
                    for (int i = 0; i < _mapas.Length; i++)
                    {
                        var m   = _mapas[i];
                        var pre = $"M{i}_";
                        m.Fecha      = d.Gv($"{pre}Fecha");
                        m.Ciudad1    = d.Gv($"{pre}Ciudad1");
                        m.Ciudad2    = d.Gv($"{pre}Ciudad2");
                        m.Ciudad3    = d.Gv($"{pre}Ciudad3");
                        m.Ciudad4    = d.Gv($"{pre}Ciudad4");
                        m.Ciudad5    = d.Gv($"{pre}Ciudad5");
                        m.Comunidad1 = d.Gv($"{pre}Comunidad1");
                        m.Comunidad2 = d.Gv($"{pre}Comunidad2");
                        m.Comunidad3 = d.Gv($"{pre}Comunidad3");
                        m.Comunidad4 = d.Gv($"{pre}Comunidad4");
                        m.Comunidad5 = d.Gv($"{pre}Comunidad5");
                        m.Logo       = d.Gv($"{pre}Logo");
                        m.Texto1     = d.Gv($"{pre}Texto1");
                        m.Texto2     = d.Gv($"{pre}Texto2");
                    }
                    int idx = int.TryParse(d.Gv("MapaActual"), out int v) && v >= 0 && v < _mapas.Length ? v : 0;
                    _mapaActual = idx;
                    CargarMapa(idx);
                });
        }

        private void RegistrarLiveData()
        {
            LiveDataService.Instancia.Registrar(TipoElemento.Rotulo,
                () => $"Tipo: {GetCheckedRadio(this, "TipoRotulo")}  " +
                      $"L1: {TxtLinea1Primera.Text} {TxtLinea1Segunda.Text}  L2: {TxtLinea2.Text}");

            LiveDataService.Instancia.Registrar(TipoElemento.LogoCiudades, () =>
            {
                var m = _mapas[_mapaActual];
                return $"Logo: {m.Logo}  Fecha: {m.Fecha}  " +
                       $"{m.Ciudad1} {m.Ciudad2} {m.Ciudad3} {m.Ciudad4} {m.Ciudad5}";
            });
        }

        // ── Logos de imágenes ────────────────────────────────────────────────

        private void PopulateLogosImagenes()
        {
            foreach (var cmb in _cmbLogosImagenes)
            {
                foreach (var logo in Logos)
                    cmb.Items.Add(logo);
                cmb.SelectedIndex = 0;
            }
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

        private void WebPreview_Click(object sender, RoutedEventArgs e)
        {
            Previsualizar(new Elemento { Tipo = TipoElemento.Web });
        }
        private void WebEntra_Click(object sender, RoutedEventArgs e)
            => BrainstormService.Instancia.Entra(new Elemento { Tipo = TipoElemento.Web });
        private void WebSale_Click(object sender, RoutedEventArgs e)
            => BrainstormService.Instancia.Sale(new Elemento { Tipo = TipoElemento.Web });

        // ── IMÁGENES ─────────────────────────────────────────────────────────

        private void ImagenesPreview_Click(object sender, RoutedEventArgs e)
        {
            var fotoActiva = GetCheckedRadio(this, "Imagenes");

            var el = new Elemento { Tipo = TipoElemento.Imagen };
            el["Foto"]          = fotoActiva;
            el["RutaFoto1"]     = TxtF1.Text;
            el["RutaFoto2"]     = TxtF2.Text;
            el["RutaFoto3"]     = TxtF3.Text;
            el["RutaFoto4"]     = TxtF4.Text;
            el["RutaFoto5"]     = TxtF5.Text;
            el["LogoFoto1"]     = CmbLogoFoto1.SelectedItem?.ToString()     ?? string.Empty;
            el["LogoFoto2"]     = CmbLogoFoto2.SelectedItem?.ToString()     ?? string.Empty;
            el["LogoFoto3"]     = CmbLogoFoto3.SelectedItem?.ToString()     ?? string.Empty;
            el["LogoFoto4"]     = CmbLogoFoto4.SelectedItem?.ToString()     ?? string.Empty;
            el["LogoFoto5"]     = CmbLogoFoto5.SelectedItem?.ToString()     ?? string.Empty;

            // Logo del slot activo → guardado en LogoRepository
            var logoNombre = fotoActiva switch
            {
                "Foto 1" => CmbLogoFoto1.SelectedItem?.ToString(),
                "Foto 2" => CmbLogoFoto2.SelectedItem?.ToString(),
                "Foto 3" => CmbLogoFoto3.SelectedItem?.ToString(),
                "Foto 4" => CmbLogoFoto4.SelectedItem?.ToString(),
                "Foto 5" => CmbLogoFoto5.SelectedItem?.ToString(),
                _        => null
            };
            if (!string.IsNullOrEmpty(logoNombre))
                el.LogoId = LogoRepository.Instancia.GetOrCreate(logoNombre).Id;

            Previsualizar(el);
        }
        private void ImagenesEntra_Click(object sender, RoutedEventArgs e)
        {
            var fotoActiva = GetCheckedRadio(this, "Imagenes");
            var el = new Elemento { Tipo = TipoElemento.Imagen };
            el["Foto"]      = fotoActiva;
            el["RutaFoto1"] = TxtF1.Text;
            el["RutaFoto2"] = TxtF2.Text;
            el["RutaFoto3"] = TxtF3.Text;
            el["RutaFoto4"] = TxtF4.Text;
            el["RutaFoto5"] = TxtF5.Text;
            BrainstormService.Instancia.Entra(el);
        }
        private void ImagenesSale_Click(object sender, RoutedEventArgs e)
            => BrainstormService.Instancia.Sale(new Elemento { Tipo = TipoElemento.Imagen });
        // ── COLAS ────────────────────────────────────────────────────────────

        private void ColasPreview_Click(object sender, RoutedEventArgs e)
        {
            var el = new Elemento { Tipo = TipoElemento.Imagen };
            el["Foto"] = "Colas";
            var logoNombre = CmbLogoColas.SelectedItem?.ToString();
            if (!string.IsNullOrEmpty(logoNombre))
                el.LogoId = LogoRepository.Instancia.GetOrCreate(logoNombre).Id;
            Previsualizar(el);
        }

        private void ColasEntra_Click(object sender, RoutedEventArgs e)
            => SceneController.Instancia.PantallaSDI();

        private void ColasSale_Click(object sender, RoutedEventArgs e)
            => SceneController.Instancia.PantallaNDI();

        private void F1_Click(object sender, RoutedEventArgs e) => AbrirImagen(TxtF1);
        private void F2_Click(object sender, RoutedEventArgs e) => AbrirImagen(TxtF2);
        private void F3_Click(object sender, RoutedEventArgs e) => AbrirImagen(TxtF3);
        private void F4_Click(object sender, RoutedEventArgs e) => AbrirImagen(TxtF4);
        private void F5_Click(object sender, RoutedEventArgs e) => AbrirImagen(TxtF5);

        private static void AbrirImagen(TextBox destino)
        {
            var dlg = new Microsoft.Win32.OpenFileDialog
            {
                Title  = "Seleccionar imagen",
                Filter = "Imágenes|*.jpg;*.jpeg;*.png;*.bmp;*.gif;*.tiff;*.webp|Todos los archivos|*.*"
            };
            if (dlg.ShowDialog() != true) return;

            string path = dlg.FileName;
            if (RemoteShareService.Instancia.Configurado)
            {
                try
                {
                    path = RemoteShareService.Instancia.CopiarImagen(dlg.FileName);
                }
                catch (System.Exception ex)
                {
                    LogService.Instancia.Registrar(LogNivel.Error, "Imágenes",
                        $"Error al copiar a carpeta remota: {ex.Message}");
                }
            }

            destino.Text = path;
            LogService.Instancia.Registrar(LogNivel.Cambio, "Imágenes",
                $"Imagen cargada: {System.IO.Path.GetFileName(dlg.FileName)} → {path}");
        }

        // ── RÓTULOS ──────────────────────────────────────────────────────────

        private void PresenterName_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
                TxtLinea1Primera.Text = btn.Tag?.ToString() ?? string.Empty;
        }
        private void EntrarRotulo_Click(object sender, RoutedEventArgs e)
        {
            var el = new Elemento { Tipo = TipoElemento.Rotulo };
            el["Tipo"]         = GetCheckedRadio(this, "TipoRotulo");
            el["Linea1Primera"] = TxtLinea1Primera.Text;
            el["Linea1Segunda"] = TxtLinea1Segunda.Text;
            el["Linea2"]        = TxtLinea2.Text;
            BrainstormService.Instancia.Entra(el);
        }
        private void SaleRotulo_Click(object sender, RoutedEventArgs e)
            => BrainstormService.Instancia.Sale(new Elemento { Tipo = TipoElemento.Rotulo });
        private void PreviewRotulo_Click(object sender, RoutedEventArgs e)
        {
            var el = new Elemento { Tipo = TipoElemento.Rotulo };
            el["Tipo"]         = GetCheckedRadio(this, "TipoRotulo");
            el["Linea1Primera"] = TxtLinea1Primera.Text;
            el["Linea1Segunda"] = TxtLinea1Segunda.Text;
            el["Linea2"]        = TxtLinea2.Text;
            Previsualizar(el);
        }

        // ── CIUDADES — LOGO ──────────────────────────────────────────────────

        private void LstLogos_SelectionChanged(object sender, SelectionChangedEventArgs e) { }
        private void LogoPreview_Click(object sender, RoutedEventArgs e)
        {
            GuardarMapaActual();
            var m = _mapas[_mapaActual];
            var el = new Elemento { Tipo = TipoElemento.LogoCiudades };
            el["Logo"]       = m.Logo;
            el["Fecha"]      = m.Fecha;
            el["Ciudad1"]    = m.Ciudad1;
            el["Ciudad2"]    = m.Ciudad2;
            el["Ciudad3"]    = m.Ciudad3;
            el["Ciudad4"]    = m.Ciudad4;
            el["Ciudad5"]    = m.Ciudad5;
            el["Comunidad1"] = m.Comunidad1;
            el["Comunidad2"] = m.Comunidad2;
            el["Comunidad3"] = m.Comunidad3;
            el["Comunidad4"] = m.Comunidad4;
            el["Comunidad5"] = m.Comunidad5;
            el["Texto1"]     = m.Texto1;
            el["Texto2"]     = m.Texto2;
            Previsualizar(el);
        }
        private void LogoEntra_Click(object sender, RoutedEventArgs e)
        {
            GuardarMapaActual();
            var m = _mapas[_mapaActual];
            var el = new Elemento { Tipo = TipoElemento.LogoCiudades };
            el["Logo"] = m.Logo; el["Fecha"] = m.Fecha;
            el["Ciudad1"] = m.Ciudad1; el["Ciudad2"] = m.Ciudad2; el["Ciudad3"] = m.Ciudad3;
            el["Ciudad4"] = m.Ciudad4; el["Ciudad5"] = m.Ciudad5;
            el["Comunidad1"] = m.Comunidad1; el["Comunidad2"] = m.Comunidad2;
            el["Comunidad3"] = m.Comunidad3; el["Comunidad4"] = m.Comunidad4;
            el["Comunidad5"] = m.Comunidad5;
            el["Texto1"] = m.Texto1; el["Texto2"] = m.Texto2;
            BrainstormService.Instancia.Entra(el);
        }
        private void LogoSale_Click(object sender, RoutedEventArgs e)
            => BrainstormService.Instancia.Sale(new Elemento { Tipo = TipoElemento.LogoCiudades });

        // ── Helpers ──────────────────────────────────────────────────────────

        private void Previsualizar(Elemento el)
        {
            ElementoRepository.Instancia.Add(el);
            UltimoElemento = el;
            LogService.Instancia.Registrar(LogNivel.Accion, el.Tipo.ToString(),
                "P → " + el.ToLogString());
            AgregarAPlaylist(el);
        }

        private static void AgregarAPlaylist(Elemento el)
        {
            PlaylistService.Instancia.AgregarElemento(el);
            PlaylistService.Instancia.AgregarLogo(GetLogoNombre(el), el.Tipo);
        }

        private static string GetLogoNombre(Elemento el) => el.Tipo switch
        {
            TipoElemento.Web          => "GenericoLAE",
            TipoElemento.Imagen       => el.LogoId is { } id
                                             ? LogoRepository.Instancia.Get(id)?.Nombre ?? "GenericoLAE"
                                             : "GenericoLAE",
            TipoElemento.Rotulo       => el["Tipo"] is { Length: > 0 } t ? t : "GenericoLAE",
            TipoElemento.LogoCiudades => el["Logo"] is { Length: > 0 } l ? l : "GenericoLAE",
            _                         => "GenericoLAE",
        };

        private static string GetCheckedRadio(DependencyObject root, string groupName)
        {
            if (root is RadioButton rb && rb.GroupName == groupName && rb.IsChecked == true)
                return rb.Content?.ToString() ?? string.Empty;
            foreach (object child in LogicalTreeHelper.GetChildren(root))
                if (child is DependencyObject dep)
                {
                    var result = GetCheckedRadio(dep, groupName);
                    if (!string.IsNullOrEmpty(result)) return result;
                }
            return string.Empty;
        }
    }
}
