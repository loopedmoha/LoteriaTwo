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
        private static readonly string[] NombresPremiados =
        {
            "BONOLOTO", "EUROMILLONES M", "PRIMITIVA", "EL GORDO", "LOTOTURF", "EURODREAMS", "JOKER"
        };

        private RadioButton[] _rdbPremiado     = new RadioButton[7];
        private CheckBox[]    _chkBotePremiado  = new CheckBox[7];
        private TextBox[][]   _txtNumeros       = new TextBox[7][];
        private TextBox[][]   _txtOtros         = new TextBox[7][];
        private TextBox[]     _txtFechaPremiado = new TextBox[7];
        private string _eurodreamsDia = string.Empty;

        private Elemento? _ultimoLogo;
        private Elemento? _ultimoBote;
        private Elemento? _ultimoPremiado;

        public Elemento? UltimoElemento { get; private set; }

        public SorteosBotesView()
        {
            InitializeComponent();
            InitArraysFromXaml();
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
                        ["EuromillonesMillon"] = TxtEuromillonesMillon.Text,
                        ["EurodreamsDia"]      = TxtEurodreamsDia.Text,
                        ["EurodreamsMes"]      = TxtEurodreamsMes.Text,
                        ["EurodreamsDiaField"] = _eurodreamsDia,
                        ["ChkSeguridad"]       = (ChkSeguridad.IsChecked    == true).ToString(),
                        ["ChkPasarGanador"]    = (ChkPasarGanador.IsChecked == true).ToString(),
                    };
                    for (int i = 0; i < NombresPremiados.Length; i++)
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
                    TxtEuromillonesMillon.Text = d.Gv("EuromillonesMillon");
                    TxtEurodreamsDia.Text      = d.Gv("EurodreamsDia");
                    TxtEurodreamsMes.Text      = d.Gv("EurodreamsMes");
                    _eurodreamsDia             = d.Gv("EurodreamsDiaField");
                    ChkSeguridad.IsChecked    = d.Gv("ChkSeguridad")    == "True";
                    ChkPasarGanador.IsChecked = d.Gv("ChkPasarGanador") == "True";
                    for (int i = 0; i < NombresPremiados.Length; i++)
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
                return $"Juego: {NombresPremiados[idx]}  " +
                       $"Nums: {string.Join("-", _txtNumeros[idx].Select(t => t.Text))}  " +
                       $"Extras: {string.Join("-", _txtOtros[idx].Select(t => t.Text))}  " +
                       $"Fecha: {_txtFechaPremiado[idx].Text}";
            });

            LiveDataService.Instancia.Registrar(TipoElemento.ElMillon,
                () => $"Fecha: {TxtMillonFecha.Text}  Número: {TxtMillonMartes.Text}");

            LiveDataService.Instancia.Registrar(TipoElemento.EuromillonesMosca,
                () => $"Número: {TxtEuromillonesMillon.Text}");
        }

        // ── Inicialización de arrays desde controles XAML ────────────────────

        private void InitArraysFromXaml()
        {
            _rdbPremiado[0]     = RdbP0; _rdbPremiado[1]     = RdbP1; _rdbPremiado[2]     = RdbP2;
            _rdbPremiado[3]     = RdbP3; _rdbPremiado[4]     = RdbP4; _rdbPremiado[5]     = RdbP5;
            _chkBotePremiado[0] = ChkBoteP0; _chkBotePremiado[1] = ChkBoteP1; _chkBotePremiado[2] = ChkBoteP2;
            _chkBotePremiado[3] = ChkBoteP3; _chkBotePremiado[4] = ChkBoteP4; _chkBotePremiado[5] = ChkBoteP5;
            _txtNumeros[0] = new[] { TxtP0N0, TxtP0N1, TxtP0N2, TxtP0N3, TxtP0N4, TxtP0N5 };
            _txtNumeros[1] = new[] { TxtP1N0, TxtP1N1, TxtP1N2, TxtP1N3, TxtP1N4 };
            _txtNumeros[2] = new[] { TxtP2N0, TxtP2N1, TxtP2N2, TxtP2N3, TxtP2N4, TxtP2N5 };
            _txtNumeros[3] = new[] { TxtP3N0, TxtP3N1, TxtP3N2, TxtP3N3, TxtP3N4 };
            _txtNumeros[4] = new[] { TxtP4N0, TxtP4N1, TxtP4N2, TxtP4N3, TxtP4N4, TxtP4N5 };
            _txtNumeros[5] = new[] { TxtP5N0, TxtP5N1, TxtP5N2, TxtP5N3, TxtP5N4, TxtP5N5 };
            _txtOtros[0]   = new[] { TxtP0O0, TxtP0O1 };
            _txtOtros[1]   = new[] { TxtP1O0, TxtP1O1 };
            _txtOtros[2]   = new[] { TxtP2O0, TxtP2O1 };
            _txtOtros[3]   = new[] { TxtP3O0 };
            _txtOtros[4]   = new[] { TxtP4O0, TxtP4O1 };
            _txtOtros[5]   = new[] { TxtP5O0 };
            _txtFechaPremiado[0] = TxtFechaP0; _txtFechaPremiado[1] = TxtFechaP1; _txtFechaPremiado[2] = TxtFechaP2;
            _txtFechaPremiado[3] = TxtFechaP3; _txtFechaPremiado[4] = TxtFechaP4; _txtFechaPremiado[5] = TxtFechaP5;
            _txtFechaPremiado[6] = TxtFechaP6;
            _rdbPremiado[6]     = RdbP6; _chkBotePremiado[6] = ChkBoteP6;
            _txtNumeros[6] = new[] { TxtP6N0, TxtP6N1, TxtP6N2, TxtP6N3, TxtP6N4, TxtP6N5, TxtP6N6 };
            _txtOtros[6]   = Array.Empty<TextBox>();
        }

        private void BotePremiado_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox cb && cb.Tag is string juego)
                BrainstormService.Instancia.SetBoteCantidad(GetCantidadBoteParaJuego(juego));
        }

        private void NumPremiado_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Tab && ChkPasarGanador.IsChecked == true && sender is TextBox tb
                && tb.Tag is string tag && int.TryParse(tag, out int pos))
                BrainstormService.Instancia.EntraFaldonCifra(pos, tb.Text);
        }

        // ── Handlers — LOGOS ─────────────────────────────────────────────────

        private void LogoEntra_Click(object sender, RoutedEventArgs e)
        {
            var el = BuildLogoElemento();
            BrainstormService.Instancia.Entra(el);
            _ultimoLogo = el;
        }
        private void LogoSale_Click(object sender, RoutedEventArgs e)
            => BrainstormService.Instancia.Sale(BuildLogoElemento());
        private void LogoEncadena_Click(object sender, RoutedEventArgs e)
        {
            if (_ultimoLogo is not null) BrainstormService.Instancia.Sale(_ultimoLogo);
            var el = BuildLogoElemento();
            BrainstormService.Instancia.Entra(el);
            _ultimoLogo = el;
        }
        private void LogoP_Click(object sender, RoutedEventArgs e)
            => Previsualizar(BuildLogoElemento(), BuildLogoElemento);

        private Elemento BuildLogoElemento()
        {
            var el = new Elemento { Tipo = TipoElemento.Logo };
            el["Logo"] = (CmbLogos.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? string.Empty;
            return el;
        }

        // ── Handlers — BOTES ─────────────────────────────────────────────────

        private void BoteEntra_Click(object sender, RoutedEventArgs e)
        {
            var el = BuildBoteElemento();
            BrainstormService.Instancia.Entra(el);
            _ultimoBote = el;
        }
        private void BoteSale_Click(object sender, RoutedEventArgs e)
            => BrainstormService.Instancia.Sale(BuildBoteElemento());
        private void BoteEncadena_Click(object sender, RoutedEventArgs e)
        {
            if (_ultimoBote is not null) BrainstormService.Instancia.Sale(_ultimoBote);
            var el = BuildBoteElemento();
            BrainstormService.Instancia.Entra(el);
            _ultimoBote = el;
        }
        private void BoteP_Click(object sender, RoutedEventArgs e)
        {
            var el    = BuildBoteElemento();
            var juego = el["Juego"];
            Previsualizar(el, () => BuildBoteElementoParaJuego(juego));
        }

        private Elemento BuildBoteElementoParaJuego(string juego)
        {
            var el = new Elemento { Tipo = TipoElemento.Bote };
            el["Juego"] = juego;
            (el["Cantidad"], el["Fecha"]) = juego switch
            {
                "BONOLOTO"     => (TxtCantBonoloto.Text,     TxtFechaBoteBonoloto.Text),
                "QUINIGOL"     => (TxtCantQuinigol.Text,     TxtFechaBoteQuinigol.Text),
                "EUROMILLONES" => (TxtCantEuromillones.Text, TxtFechaBoteEuromillones.Text),
                "LOTERIA"      => (TxtCantLoteria.Text,      TxtFechaBoteLoteria.Text),
                "PRIMITIVA"    => (TxtCantPrimitiva.Text,    TxtFechaBotePrimitiva.Text),
                "QUINIELA"     => (TxtCantQuiniela.Text,     TxtFechaBoteQuiniela.Text),
                "EL GORDO"     => (TxtCantElGordo.Text,      TxtFechaBoteElGordo.Text),
                "LOTOTURF"     => (TxtCantLototurf.Text,     TxtFechaBoteLototurf.Text),
                "JOKER"        => (TxtCantJoker.Text,        TxtFechaBoteJoker.Text),
                "Eurodreams"   => (TxtCantEurodreams.Text,   TxtFechaBoteEurodreams.Text),
                _              => (string.Empty, string.Empty)
            };
            return el;
        }

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
            int idx = Array.FindIndex(_rdbPremiado, r => r.IsChecked == true);
            if (idx < 0) return;
            var el = BuildPremiadoElemento(idx);
            if (el is not null) Previsualizar(el, () => BuildPremiadoElemento(idx));
        }

        private Elemento? BuildPremiadoElemento()
        {
            int idx = Array.FindIndex(_rdbPremiado, r => r.IsChecked == true);
            return idx < 0 ? null : BuildPremiadoElemento(idx);
        }

        private Elemento? BuildPremiadoElemento(int idx)
        {
            if (idx < 0 || idx >= NombresPremiados.Length) return null;
            var el = new Elemento { Tipo = TipoElemento.Premiado };
            el["Juego"]         = NombresPremiados[idx];
            el["Bote"]          = (_chkBotePremiado[idx].IsChecked == true).ToString();
            el["BoteCantidad"]  = GetCantidadBoteParaJuego(NombresPremiados[idx]);
            el["Numeros"]       = string.Join(",", _txtNumeros[idx].Select(t => t.Text));
            el["Extras"]        = string.Join(",", _txtOtros[idx].Select(t => t.Text));
            el["Fecha"]         = _txtFechaPremiado[idx].Text;
            return el;
        }
        private void PremiadosEntra_Click(object sender, RoutedEventArgs e)
        {
            var el = BuildPremiadoElemento();
            if (el is null) return;
            BrainstormService.Instancia.Entra(el);
            _ultimoPremiado = el;
        }
        private void PremiadosSale_Click(object sender, RoutedEventArgs e)
        {
            var el = BuildPremiadoElemento();
            if (el is not null) BrainstormService.Instancia.Sale(el);
        }
        private void PremiadosEncadena_Click(object sender, RoutedEventArgs e)
        {
            if (_ultimoPremiado is not null) BrainstormService.Instancia.Sale(_ultimoPremiado);
            var el = BuildPremiadoElemento();
            if (el is null) return;
            BrainstormService.Instancia.Entra(el);
            _ultimoPremiado = el;
        }
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
            => Previsualizar(BuildMillonElemento(), BuildMillonElemento);
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
            Previsualizar(el, () => { var e2 = new Elemento { Tipo = TipoElemento.EuromillonesMosca }; e2["Numero"] = TxtEuromillonesMillon.Text; return e2; });
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
            Previsualizar(el, () => { var e2 = new Elemento { Tipo = TipoElemento.Eurodreams }; e2["DiaSemana"] = _eurodreamsDia; e2["Dia"] = TxtEurodreamsDia.Text; e2["Mes"] = TxtEurodreamsMes.Text; return e2; });
        }
        private void EurodreamsPremiado_Click(object sender, RoutedEventArgs e) { }
        private void EurodreamsProximo_Click(object sender, RoutedEventArgs e) { }

        // ── Helpers ──────────────────────────────────────────────────────────

        private void Previsualizar(Elemento el, Func<Elemento?>? buildActual = null)
        {
            ElementoRepository.Instancia.Add(el);
            UltimoElemento = el;
            LogService.Instancia.Registrar(LogNivel.Accion, el.Tipo.ToString(),
                "P → " + el.ToLogString());
            PlaylistService.Instancia.AgregarElemento(el, buildActual);
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
            "JOKER"          => "Joker",
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
