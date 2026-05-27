using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using LoteriaTwo.Models;
using LoteriaTwo.Services;

namespace LoteriaTwo.Views
{
    public partial class SorteosBotesView : UserControl
    {
        // Configuración de cada juego en PREMIADOS: nombre, bolas principales, bolas extra
        private static readonly (string Nombre, int Bolas, int Extras)[] JuegosPremiados =
        {
            ("BONOLOTO",       6, 2),  // 6 números + complementario + reintegro
            ("EUROMILLONES M", 5, 2),  // 5 números + 2 estrellas
            ("PRIMITIVA",      6, 2),  // 6 números + complementario + reintegro
            ("EL GORDO",       5, 1),  // 5 números + 1 clave
            ("LOTOTURF",       7, 0),  // 7 números
            ("EURODREAMS",     6, 1),  // 6 números + 1 dream
        };

        private readonly RadioButton[] _rdbPremiado    = new RadioButton[6];
        private readonly CheckBox[]    _chkBotePremiado = new CheckBox[6];
        private readonly TextBox[][]   _txtNumeros      = new TextBox[6][];
        private readonly TextBox[][]   _txtOtros        = new TextBox[6][];
        private readonly TextBox[]     _txtFechaPremiado = new TextBox[6];
        private string _eurodreamsDia = string.Empty;

        public Elemento? UltimoElemento { get; private set; }

        public SorteosBotesView()
        {
            InitializeComponent();
            BuildPremiados();
            RegistrarLiveData();
            RegistrarFormState();
        }

        private void RegistrarFormState()
        {
            FormStateService.Instancia.RegistrarSeccion("SorteosBotes",
                leer: () =>
                {
                    var d = new Dictionary<string, string>
                    {
                        ["LogoCombo"]          = (CmbLogos.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? string.Empty,
                        ["BoteGame"]           = GetCheckedRadio(this, "BoteGame"),
                        ["CantBonoloto"]       = TxtCantBonoloto.Text,
                        ["CantQuinigol"]       = TxtCantQuinigol.Text,
                        ["CantEuromillones"]   = TxtCantEuromillones.Text,
                        ["CantLoteria"]        = TxtCantLoteria.Text,
                        ["CantPrimitiva"]      = TxtCantPrimitiva.Text,
                        ["CantQuiniela"]       = TxtCantQuiniela.Text,
                        ["CantElGordo"]        = TxtCantElGordo.Text,
                        ["CantLototurf"]       = TxtCantLototurf.Text,
                        ["CantJoker"]          = TxtCantJoker.Text,
                        ["CantEurodreams"]     = TxtCantEurodreams.Text,
                        ["FechaBonoloto"]      = TxtFechaBoteBonoloto.Text,
                        ["FechaQuinigol"]      = TxtFechaBoteQuinigol.Text,
                        ["FechaEuromillones"]  = TxtFechaBoteEuromillones.Text,
                        ["FechaLoteria"]       = TxtFechaBoteLoteria.Text,
                        ["FechaPrimitiva"]     = TxtFechaBotePrimitiva.Text,
                        ["FechaQuiniela"]      = TxtFechaBoteQuiniela.Text,
                        ["FechaElGordo"]       = TxtFechaBoteElGordo.Text,
                        ["FechaLototurf"]      = TxtFechaBoteLototurf.Text,
                        ["FechaJoker"]         = TxtFechaBoteJoker.Text,
                        ["FechaEurodreams"]    = TxtFechaBoteEurodreams.Text,
                        ["MillonFecha"]        = TxtMillonFecha.Text,
                        ["MillonMartes"]       = TxtMillonMartes.Text,
                        ["JokerJueves"]        = TxtJokerJueves.Text,
                        ["EuromillonesMillon"] = TxtEuromillonesMillon.Text,
                        ["EurodreamsDia"]      = TxtEurodreamsDia.Text,
                        ["EurodreamsMes"]      = TxtEurodreamsMes.Text,
                        ["EurodreamsDiaField"] = _eurodreamsDia,
                        ["ChkSeguridad"]       = (ChkSeguridad.IsChecked    == true).ToString(),
                        ["ChkPasarGanador"]    = (ChkPasarGanador.IsChecked == true).ToString(),
                    };
                    for (int i = 0; i < JuegosPremiados.Length; i++)
                    {
                        var pre = $"P{i}_";
                        d[$"{pre}Sel"]   = (_rdbPremiado[i].IsChecked    == true).ToString();
                        d[$"{pre}Bote"]  = (_chkBotePremiado[i].IsChecked == true).ToString();
                        d[$"{pre}Fecha"] = _txtFechaPremiado[i].Text;
                        for (int j = 0; j < _txtNumeros[i].Length; j++)
                            d[$"{pre}N{j}"] = _txtNumeros[i][j].Text;
                        for (int j = 0; j < _txtOtros[i].Length; j++)
                            d[$"{pre}O{j}"] = _txtOtros[i][j].Text;
                    }
                    return d;
                },
                escribir: d =>
                {
                    FormHelper.RestoreComboByTag(CmbLogos, d.Gv("LogoCombo"));
                    FormHelper.SetCheckedRadio(this, "BoteGame", d.Gv("BoteGame"));
                    TxtCantBonoloto.Text      = d.Gv("CantBonoloto");
                    TxtCantQuinigol.Text      = d.Gv("CantQuinigol");
                    TxtCantEuromillones.Text  = d.Gv("CantEuromillones");
                    TxtCantLoteria.Text       = d.Gv("CantLoteria");
                    TxtCantPrimitiva.Text     = d.Gv("CantPrimitiva");
                    TxtCantQuiniela.Text      = d.Gv("CantQuiniela");
                    TxtCantElGordo.Text       = d.Gv("CantElGordo");
                    TxtCantLototurf.Text      = d.Gv("CantLototurf");
                    TxtCantJoker.Text         = d.Gv("CantJoker");
                    TxtCantEurodreams.Text    = d.Gv("CantEurodreams");
                    TxtFechaBoteBonoloto.Text     = d.Gv("FechaBonoloto");
                    TxtFechaBoteQuinigol.Text     = d.Gv("FechaQuinigol");
                    TxtFechaBoteEuromillones.Text = d.Gv("FechaEuromillones");
                    TxtFechaBoteLoteria.Text      = d.Gv("FechaLoteria");
                    TxtFechaBotePrimitiva.Text    = d.Gv("FechaPrimitiva");
                    TxtFechaBoteQuiniela.Text     = d.Gv("FechaQuiniela");
                    TxtFechaBoteElGordo.Text      = d.Gv("FechaElGordo");
                    TxtFechaBoteLototurf.Text     = d.Gv("FechaLototurf");
                    TxtFechaBoteJoker.Text        = d.Gv("FechaJoker");
                    TxtFechaBoteEurodreams.Text   = d.Gv("FechaEurodreams");
                    TxtMillonFecha.Text        = d.Gv("MillonFecha");
                    TxtMillonMartes.Text       = d.Gv("MillonMartes");
                    TxtJokerJueves.Text        = d.Gv("JokerJueves");
                    TxtEuromillonesMillon.Text = d.Gv("EuromillonesMillon");
                    TxtEurodreamsDia.Text      = d.Gv("EurodreamsDia");
                    TxtEurodreamsMes.Text      = d.Gv("EurodreamsMes");
                    _eurodreamsDia             = d.Gv("EurodreamsDiaField");
                    ChkSeguridad.IsChecked    = d.Gv("ChkSeguridad")    == "True";
                    ChkPasarGanador.IsChecked = d.Gv("ChkPasarGanador") == "True";
                    for (int i = 0; i < JuegosPremiados.Length; i++)
                    {
                        var pre = $"P{i}_";
                        _rdbPremiado[i].IsChecked     = d.Gv($"{pre}Sel")  == "True";
                        _chkBotePremiado[i].IsChecked = d.Gv($"{pre}Bote") == "True";
                        _txtFechaPremiado[i].Text     = d.Gv($"{pre}Fecha");
                        for (int j = 0; j < _txtNumeros[i].Length; j++)
                            _txtNumeros[i][j].Text = d.Gv($"{pre}N{j}");
                        for (int j = 0; j < _txtOtros[i].Length; j++)
                            _txtOtros[i][j].Text = d.Gv($"{pre}O{j}");
                    }
                });
        }

        private void RegistrarLiveData()
        {
            LiveDataService.Instancia.Registrar(TipoElemento.Bote, () =>
            {
                var juego = GetCheckedRadio(this, "BoteGame");
                var (cant, fecha) = juego switch
                {
                    "BONOLOTO"     => (TxtCantBonoloto.Text,     TxtFechaBoteBonoloto.Text),
                    "QUINIGOL"     => (TxtCantQuinigol.Text,     TxtFechaBoteQuinigol.Text),
                    "EUROMILLONES" => (TxtCantEuromillones.Text,  TxtFechaBoteEuromillones.Text),
                    "LOTERIA"      => (TxtCantLoteria.Text,      TxtFechaBoteLoteria.Text),
                    "PRIMITIVA"    => (TxtCantPrimitiva.Text,    TxtFechaBotePrimitiva.Text),
                    "QUINIELA"     => (TxtCantQuiniela.Text,     TxtFechaBoteQuiniela.Text),
                    "EL GORDO"     => (TxtCantElGordo.Text,      TxtFechaBoteElGordo.Text),
                    "LOTOTURF"     => (TxtCantLototurf.Text,     TxtFechaBoteLototurf.Text),
                    "JOKER"        => (TxtCantJoker.Text,        TxtFechaBoteJoker.Text),
                    "Eurodreams"   => (TxtCantEurodreams.Text,   TxtFechaBoteEurodreams.Text),
                    _              => (string.Empty, string.Empty)
                };
                return $"Juego: {juego}  Cant: {cant}  Fecha: {fecha}";
            });

            LiveDataService.Instancia.Registrar(TipoElemento.Premiado, () =>
            {
                int idx = Array.FindIndex(_rdbPremiado, r => r.IsChecked == true);
                if (idx < 0) return "sin selección";
                return $"Juego: {JuegosPremiados[idx].Nombre}  " +
                       $"Nums: {string.Join("-", _txtNumeros[idx].Select(t => t.Text))}  " +
                       $"Extras: {string.Join("-", _txtOtros[idx].Select(t => t.Text))}  " +
                       $"Fecha: {_txtFechaPremiado[idx].Text}";
            });

            LiveDataService.Instancia.Registrar(TipoElemento.ElMillon,
                () => $"Fecha: {TxtMillonFecha.Text}  Número: {TxtMillonMartes.Text}");

            LiveDataService.Instancia.Registrar(TipoElemento.EuromillonesMosca,
                () => $"Número: {TxtEuromillonesMillon.Text}");
        }

        // ── Construcción programática de filas PREMIADOS ─────────────────────

        private void BuildPremiados()
        {
            var primary   = (Brush)Application.Current.Resources["BrushTextPrimary"];
            var secondary = (Brush)Application.Current.Resources["BrushTextSecondary"];

            GridPremiados.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(30) });   // radio
            GridPremiados.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(155) });  // nombre
            GridPremiados.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(60) });   // BOTE
            GridPremiados.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(255) });  // números
            GridPremiados.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(105) });  // otros
            GridPremiados.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(115) }); // fecha

            // Fila de cabeceras
            GridPremiados.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            PlaceP(MakeHeader("NUMEROS PREMIADOS", primary), 0, 3);
            PlaceP(MakeHeader("OTROS",             primary), 0, 4);
            PlaceP(MakeHeader("FECHA",             primary), 0, 5);

            // Filas de juegos
            for (int i = 0; i < JuegosPremiados.Length; i++)
            {
                var (nombre, bolas, extras) = JuegosPremiados[i];
                GridPremiados.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
                int row = i + 1;
                int idx = i;

                // Radio button de selección
                _rdbPremiado[i] = new RadioButton
                {
                    GroupName = "PremiadoGame",
                    Margin = new Thickness(0, 2, 4, 2),
                    VerticalAlignment = VerticalAlignment.Center,
                    Foreground = primary
                };
                PlaceP(_rdbPremiado[i], row, 0);

                // Nombre del juego
                PlaceP(new TextBlock
                {
                    Text = nombre,
                    FontWeight = FontWeights.SemiBold,
                    Foreground = primary,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(0, 2, 8, 2)
                }, row, 1);

                // Checkbox BOTE
                _chkBotePremiado[i] = new CheckBox
                {
                    Content = "BOTE",
                    Foreground = primary,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(0, 2, 4, 2),
                    FontSize = 10
                };
                _chkBotePremiado[i].Checked += (_, _) =>
                {
                    var juego   = JuegosPremiados[idx].Nombre;
                    var cantidad = GetCantidadBoteParaJuego(juego);
                    Debug.WriteLine($"[Bote] Checked — juego={juego}  cantidad='{cantidad}'");
                    BrainstormService.Instancia.SetBoteCantidad(cantidad);
                };
                PlaceP(_chkBotePremiado[i], row, 2);

                // Cajas de números premiados
                var spNums = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center };
                _txtNumeros[i] = new TextBox[bolas];
                for (int j = 0; j < bolas; j++)
                {
                    var tb = new TextBox
                    {
                        Width = 33,
                        MaxLength = 2,
                        HorizontalContentAlignment = HorizontalAlignment.Center,
                        Margin = new Thickness(0, 2, 3, 2)
                    };
                    int pos = j + 1;
                    tb.PreviewKeyDown += (_, args) =>
                    {
                        if (args.Key == Key.Tab && ChkPasarGanador.IsChecked == true)
                            BrainstormService.Instancia.EntraFaldonCifra(pos, tb.Text);
                    };
                    _txtNumeros[i][j] = tb;
                    spNums.Children.Add(tb);
                }
                PlaceP(spNums, row, 3);

                // Cajas de números extra (estrellas, complementario, etc.)
                var spOtros = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center };
                _txtOtros[i] = new TextBox[extras];
                for (int j = 0; j < extras; j++)
                {
                    var tb = new TextBox
                    {
                        Width = 44,
                        MaxLength = 2,
                        HorizontalContentAlignment = HorizontalAlignment.Center,
                        Margin = new Thickness(0, 2, 3, 2)
                    };
                    int posExtra = bolas + j + 1;
                    tb.PreviewKeyDown += (_, args) =>
                    {
                        if (args.Key == Key.Tab && ChkPasarGanador.IsChecked == true)
                            BrainstormService.Instancia.EntraFaldonCifra(posExtra, tb.Text);
                    };
                    _txtOtros[i][j] = tb;
                    spOtros.Children.Add(tb);
                }
                PlaceP(spOtros, row, 4);

                // Fecha
                _txtFechaPremiado[i] = new TextBox { Margin = new Thickness(0, 2, 0, 2), VerticalAlignment = VerticalAlignment.Center, Width = 110 };
                PlaceP(_txtFechaPremiado[i], row, 5);
            }
        }

        private void PlaceP(UIElement el, int row, int col)
        {
            Grid.SetRow(el, row);
            Grid.SetColumn(el, col);
            GridPremiados.Children.Add(el);
        }

        private static TextBlock MakeHeader(string text, Brush foreground) =>
            new TextBlock
            {
                Text = text,
                FontWeight = FontWeights.Bold,
                FontSize = 10,
                Foreground = foreground,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 4)
            };

        // ── Handlers — LOGOS ─────────────────────────────────────────────────

        private void LogoEntra_Click(object sender, RoutedEventArgs e)
            => BrainstormService.Instancia.Entra(BuildLogoElemento());
        private void LogoSale_Click(object sender, RoutedEventArgs e)
            => BrainstormService.Instancia.Sale(BuildLogoElemento());
        private void LogoEncadena_Click(object sender, RoutedEventArgs e) { }
        private void LogoP_Click(object sender, RoutedEventArgs e)
            => Previsualizar(BuildLogoElemento());

        private Elemento BuildLogoElemento()
        {
            var el = new Elemento { Tipo = TipoElemento.Logo };
            el["Logo"] = (CmbLogos.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? string.Empty;
            return el;
        }

        // ── Handlers — BOTES ─────────────────────────────────────────────────

        private void BoteEntra_Click(object sender, RoutedEventArgs e)
            => BrainstormService.Instancia.Entra(BuildBoteElemento());
        private void BoteSale_Click(object sender, RoutedEventArgs e)
            => BrainstormService.Instancia.Sale(BuildBoteElemento());
        private void BoteEncadena_Click(object sender, RoutedEventArgs e) { }
        private void BoteP_Click(object sender, RoutedEventArgs e)
            => Previsualizar(BuildBoteElemento());

        private Elemento BuildBoteElemento()
        {
            var el = new Elemento { Tipo = TipoElemento.Bote };
            el["Juego"] = GetCheckedRadio(this, "BoteGame");
            (el["Cantidad"], el["Fecha"]) = el["Juego"] switch
            {
                "BONOLOTO"     => (TxtCantBonoloto.Text,    TxtFechaBoteBonoloto.Text),
                "QUINIGOL"     => (TxtCantQuinigol.Text,    TxtFechaBoteQuinigol.Text),
                "EUROMILLONES" => (TxtCantEuromillones.Text, TxtFechaBoteEuromillones.Text),
                "LOTERIA"      => (TxtCantLoteria.Text,     TxtFechaBoteLoteria.Text),
                "PRIMITIVA"    => (TxtCantPrimitiva.Text,   TxtFechaBotePrimitiva.Text),
                "QUINIELA"     => (TxtCantQuiniela.Text,    TxtFechaBoteQuiniela.Text),
                "EL GORDO"     => (TxtCantElGordo.Text,     TxtFechaBoteElGordo.Text),
                "LOTOTURF"     => (TxtCantLototurf.Text,    TxtFechaBoteLototurf.Text),
                "JOKER"        => (TxtCantJoker.Text,       TxtFechaBoteJoker.Text),
                "Eurodreams"   => (TxtCantEurodreams.Text,  TxtFechaBoteEurodreams.Text),
                _              => (string.Empty, string.Empty)
            };
            return el;
        }

        // ── Handlers — PREMIADOS ─────────────────────────────────────────────

        private void PremiadosP_Click(object sender, RoutedEventArgs e)
        {
            var el = BuildPremiadoElemento();
            if (el is not null) Previsualizar(el);
        }

        private Elemento? BuildPremiadoElemento()
        {
            int idx = Array.FindIndex(_rdbPremiado, r => r.IsChecked == true);
            if (idx < 0) return null;

            var el = new Elemento { Tipo = TipoElemento.Premiado };
            el["Juego"]         = JuegosPremiados[idx].Nombre;
            el["Bote"]          = (_chkBotePremiado[idx].IsChecked == true).ToString();
            el["BoteCantidad"]  = GetCantidadBoteParaJuego(JuegosPremiados[idx].Nombre);
            el["Numeros"]       = string.Join(",", _txtNumeros[idx].Select(t => t.Text));
            el["Extras"]        = string.Join(",", _txtOtros[idx].Select(t => t.Text));
            el["Fecha"]         = _txtFechaPremiado[idx].Text;
            return el;
        }
        private void PremiadosEntra_Click(object sender, RoutedEventArgs e)
        {
            var el = BuildPremiadoElemento();
            if (el is not null) BrainstormService.Instancia.Entra(el);
        }
        private void PremiadosSale_Click(object sender, RoutedEventArgs e)
        {
            var el = BuildPremiadoElemento();
            if (el is not null) BrainstormService.Instancia.Sale(el);
        }
        private void PremiadosEncadena_Click(object sender, RoutedEventArgs e) { }
        private void PremiadosOrdenar_Click(object sender, RoutedEventArgs e) { }

        // ── Handlers — FALDONES ──────────────────────────────────────────────

        private void FaldonEntra_Click(object sender, RoutedEventArgs e)
        {
            var el = BuildPremiadoElemento();
            if (el is not null) BrainstormService.Instancia.EntraFaldon(el);
        }
        private void FaldonSale_Click(object sender, RoutedEventArgs e)
            => BrainstormService.Instancia.SaleFaldon();
        private void FaldonCorregir_Click(object sender, RoutedEventArgs e) { }

        // ── Handlers — EL MILLÓN / JOKER ─────────────────────────────────────

        private Elemento BuildMillonElemento()
        {
            var el = new Elemento { Tipo = TipoElemento.ElMillon };
            el["Fecha"]  = TxtMillonFecha.Text;
            el["Numero"] = TxtMillonMartes.Text;
            return el;
        }
        private void MillonP_Click(object sender, RoutedEventArgs e)
            => Previsualizar(BuildMillonElemento());
        private void MillonEntra_Click(object sender, RoutedEventArgs e)
            => BrainstormService.Instancia.Entra(BuildMillonElemento());
        private void MillonSale_Click(object sender, RoutedEventArgs e)
        {
            BrainstormService.Instancia.Sale(new Elemento { Tipo = TipoElemento.ElMillon });
        }
        private void JokerEntra_Click(object sender, RoutedEventArgs e) { }
        private void JokerSale_Click(object sender, RoutedEventArgs e) { }

        // ── Handlers — EUROMILLONES ──────────────────────────────────────────

        private void EuroMillonEntra_Click(object sender, RoutedEventArgs e)
        {
            var el = new Elemento { Tipo = TipoElemento.EuromillonesMosca };
            el["Numero"] = TxtEuromillonesMillon.Text;
            BrainstormService.Instancia.Entra(el);
        }
        private void EuroMillonSale_Click(object sender, RoutedEventArgs e)
        {
            BrainstormService.Instancia.Sale(new Elemento { Tipo = TipoElemento.EuromillonesMosca });
        }
        private void EuroMillonP_Click(object sender, RoutedEventArgs e)
        {
            var el = new Elemento { Tipo = TipoElemento.EuromillonesMosca };
            el["Numero"] = TxtEuromillonesMillon.Text;
            Previsualizar(el);
        }

        // ── Handlers — EURODREAMS ────────────────────────────────────────────

        private void EurodreamsLunes_Click(object sender, RoutedEventArgs e)  { _eurodreamsDia = "LUNES"; }
        private void EurodreamsJueves_Click(object sender, RoutedEventArgs e) { _eurodreamsDia = "JUEVES"; }
        private void EurodreamsEntra_Click(object sender, RoutedEventArgs e)
        {
            var el = new Elemento { Tipo = TipoElemento.Eurodreams };
            el["DiaSemana"] = _eurodreamsDia;
            el["Dia"]       = TxtEurodreamsDia.Text;
            el["Mes"]       = TxtEurodreamsMes.Text;
            BrainstormService.Instancia.Entra(el);
        }
        private void EurodreamsSale_Click(object sender, RoutedEventArgs e)
        {
            BrainstormService.Instancia.Sale(new Elemento { Tipo = TipoElemento.Eurodreams });
        }
        private void EurodreamsP_Click(object sender, RoutedEventArgs e)
        {
            var el = new Elemento { Tipo = TipoElemento.Eurodreams };
            el["DiaSemana"] = _eurodreamsDia;
            el["Dia"]       = TxtEurodreamsDia.Text;
            el["Mes"]       = TxtEurodreamsMes.Text;
            Previsualizar(el);
        }
        private void EurodreamsPremiado_Click(object sender, RoutedEventArgs e) { }
        private void EurodreamsProximo_Click(object sender, RoutedEventArgs e) { }

        // ── Helpers ──────────────────────────────────────────────────────────

        private void Previsualizar(Elemento el)
        {
            ElementoRepository.Instancia.Add(el);
            UltimoElemento = el;
            LogService.Instancia.Registrar(LogNivel.Accion, el.Tipo.ToString(),
                "P → " + el.ToLogString());
            PlaylistService.Instancia.AgregarElemento(el);
            PlaylistService.Instancia.AgregarLogo(GetLogoNombre(el), el.Tipo);
        }

        private static string GetLogoNombre(Elemento el) => el.Tipo switch
        {
            TipoElemento.Logo              => el["Logo"],
            TipoElemento.Bote              => BoteALogo(el["Juego"]),
            TipoElemento.Premiado          => PremiadoALogo(el["Juego"]),
            TipoElemento.ElMillon          => "ElMillon",
            TipoElemento.EuromillonesMosca => "Euromillones",
            TipoElemento.Eurodreams        => "Eurodreams",
            _                              => el.Tipo.ToString(),
        };

        private static string BoteALogo(string juego) => juego switch
        {
            "BONOLOTO"     => "Bonoloto",
            "QUINIGOL"     => "Quinigol",
            "EUROMILLONES" => "Euromillones",
            "LOTERIA"      => "LoteriaNacional",
            "PRIMITIVA"    => "Primitiva",
            "QUINIELA"     => "Quiniela",
            "EL GORDO"     => "El Gordo",
            "LOTOTURF"     => "LotoTurf",
            "JOKER"        => "Joker",
            "Eurodreams"   => "Eurodreams",
            _              => juego,
        };

        private string GetCantidadBoteParaJuego(string juego) => juego switch
        {
            "BONOLOTO"       => TxtCantBonoloto.Text,
            "EUROMILLONES M" => TxtCantEuromillones.Text,
            "PRIMITIVA"      => TxtCantPrimitiva.Text,
            "EL GORDO"       => TxtCantElGordo.Text,
            "LOTOTURF"       => TxtCantLototurf.Text,
            "EURODREAMS"     => TxtCantEurodreams.Text,
            _                => string.Empty,
        };

        private static string PremiadoALogo(string juego) => juego switch
        {
            "BONOLOTO"       => "Bonoloto",
            "EUROMILLONES M" => "EuromillonesMillon",
            "PRIMITIVA"      => "Primitiva",
            "EL GORDO"       => "El Gordo",
            "LOTOTURF"       => "LotoTurf",
            "EURODREAMS"     => "Eurodreams",
            _                => juego,
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
