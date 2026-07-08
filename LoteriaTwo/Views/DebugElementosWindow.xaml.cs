using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using LoteriaTwo.Models;
using LoteriaTwo.Services;

namespace LoteriaTwo.Views
{
    public partial class DebugElementosWindow : Window
    {
        // ── Filas del DataGrid de elementos ──────────────────────────────────

        private sealed class FilaLog
        {
            public string    Hora    { get; init; } = string.Empty;
            public LogNivel  Nivel   { get; init; }
            public string    Fuente  { get; init; } = string.Empty;
            public string    Mensaje { get; init; } = string.Empty;
        }

        private sealed class FilaElemento
        {
            public Guid         Id         { get; init; }
            public TipoElemento Tipo       { get; init; }
            public DateTime     CreadoEn   { get; init; }
            public string       NombreLogo { get; init; } = string.Empty;
            public string       Resumen    { get; init; } = string.Empty;
            public Elemento     Origen     { get; init; } = null!;
        }

        public DebugElementosWindow()
        {
            InitializeComponent();
            CargarElementos();
            CargarLogos();
            CargarLog();
            LogService.Instancia.Updated += OnLogUpdated;
            Closed += (_, _) => LogService.Instancia.Updated -= OnLogUpdated;
        }

        private void OnLogUpdated()
            => Dispatcher.BeginInvoke(CargarLog);

        // ── Pestaña ELEMENTOS ─────────────────────────────────────────────────

        private void CargarElementos()
        {
            var filas = ElementoRepository.Instancia.GetAll()
                .Select(e => new FilaElemento
                {
                    Id         = e.Id,
                    Tipo       = e.Tipo,
                    CreadoEn   = e.CreadoEn,
                    NombreLogo = e.LogoId is { } id
                                     ? (LogoRepository.Instancia.Get(id)?.Nombre ?? id.ToString())
                                     : string.Empty,
                    Resumen    = BuildResumen(e),
                    Origen     = e,
                })
                .ToList();

            DgElementos.ItemsSource = filas;
            TxtConteo.Text = $"{filas.Count} elemento(s)";
            TxtDetalle.Text = string.Empty;
        }

        private void DgElementos_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TxtDetalle.Text = DgElementos.SelectedItem is FilaElemento fila
                ? BuildDetalle(fila.Origen)
                : string.Empty;
        }

        private void Eliminar_Click(object sender, RoutedEventArgs e)
        {
            if (DgElementos.SelectedItem is not FilaElemento fila) return;
            ElementoRepository.Instancia.Delete(fila.Id);
            CargarElementos();
        }

        private void LimpiarTodo_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("¿Eliminar todos los elementos?", "Confirmar",
                    MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes) return;
            ElementoRepository.Instancia.Clear();
            CargarElementos();
        }

        private void Refrescar_Click(object sender, RoutedEventArgs e) => CargarElementos();

        // ── Pestaña LOGOS ─────────────────────────────────────────────────────

        private void CargarLogos()
        {
            var logos = LogoRepository.Instancia.GetAll().ToList();
            DgLogos.ItemsSource = logos;
            TxtConteoLogos.Text = $"{logos.Count} logo(s)";
        }

        private void EliminarLogo_Click(object sender, RoutedEventArgs e)
        {
            if (DgLogos.SelectedItem is not Logo logo) return;
            LogoRepository.Instancia.Delete(logo.Id);
            CargarLogos();
        }

        private void LimpiarTodoLogos_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("¿Eliminar todos los logos?", "Confirmar",
                    MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes) return;
            LogoRepository.Instancia.Clear();
            CargarLogos();
        }

        private void RefrescarLogos_Click(object sender, RoutedEventArgs e) => CargarLogos();

        // ── Pestaña LOG ───────────────────────────────────────────────────────

        private void CargarLog()
        {
            var filas = LogService.Instancia.GetAll()
                .Select(e => new FilaLog
                {
                    Hora    = e.Timestamp.ToString("HH:mm:ss.fff"),
                    Nivel   = e.Nivel,
                    Fuente  = e.Fuente,
                    Mensaje = e.Mensaje,
                })
                .ToList();

            DgLog.ItemsSource = filas;
            TxtConteoLog.Text = $"{filas.Count} entrada(s)";
            TxtRutaLog.Text   = LogService.Instancia.RutaArchivo;

            if (filas.Count > 0)
                DgLog.ScrollIntoView(filas[^1]);
        }

        private void LimpiarLog_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("¿Limpiar el log completo?", "Confirmar",
                    MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes) return;
            LogService.Instancia.Clear();
        }

        private void RefrescarLog_Click(object sender, RoutedEventArgs e) => CargarLog();

        // ── Pestaña DATOS DE PRUEBA ───────────────────────────────────────────

        private void RellenarPrueba_Click(object sender, RoutedEventArgs e)
        {
            const string fecha = "22/12/2024";

            // ── Décimos ──────────────────────────────────────────────────────
            FormStateService.Instancia.EscribirSeccion("Decimos", new()
            {
                ["Fecha"]            = fecha,
                ["NumeroPrimero"]    = "72480",
                ["CantidadPrimero"]  = "400.000",
                ["Reintegro1"]       = "0", ["Reintegro2"] = "0", ["Reintegro3"] = "0",
                ["ReintegroPrimero"] = "False",
                ["NumeroEspecial"]   = "72480",
                ["CantidadEspecial"] = "400.000",
                ["SerieEspecial"]    = "001",
                ["FraccEspecial"]    = "10",
                ["NumeroSegundo"]    = "35291",
                ["CantidadSegundo"]  = "120.000",
                ["NumeroTercero"]    = "48763",
                ["CantidadTercero"]  = "40.000",
            });

            // ── Sorteos y Botes ──────────────────────────────────────────────
            FormStateService.Instancia.EscribirSeccion("SorteosBotes", new()
            {
                ["CantBonoloto"]      = "4.200.000",   ["FechaBonoloto"]     = "20/06/2024",
                ["CantEuromillones"]  = "45.000.000",  ["FechaEuromillones"] = "21/06/2024",
                ["CantPrimitiva"]     = "8.500.000",   ["FechaPrimitiva"]    = "22/06/2024",
                ["CantElGordo"]       = "3.000.000",   ["FechaElGordo"]      = "23/06/2024",
                ["CantLototurf"]      = "250.000",     ["FechaLototurf"]     = "22/06/2024",
                ["CantJoker"]         = "600.000",     ["FechaJoker"]        = "21/06/2024",
                ["CantEurodreams"]    = "2.000.000",   ["FechaEurodreams"]   = "20/06/2024",
                ["CantQuiniela"]      = "1.800.000",   ["FechaQuiniela"]     = "23/06/2024",
                ["CantQuinigol"]      = "150.000",     ["FechaQuinigol"]     = "22/06/2024",
                ["CantLoteria"]       = "5.000.000",   ["FechaLoteria"]      = "22/12/2024",
                ["MillonFecha"]       = "21/06/2024",  ["MillonMartes"]      = "ABC123",
                ["EuromillonesMillon"]= "XYZ789",
                ["EurodreamsDia"]     = "Jueves",      ["EurodreamsMes"]     = "Junio",
                // PREMIADOS — BONOLOTO (P0)
                ["P0_Sel"]="True",  ["P0_Bote"]="False", ["P0_Fecha"]=fecha,
                ["P0_N0"]="05", ["P0_N1"]="14", ["P0_N2"]="23", ["P0_N3"]="31", ["P0_N4"]="38", ["P0_N5"]="42",
                ["P0_O0"]="7",
                // EUROMILLONES M (P1)
                ["P1_Sel"]="False", ["P1_Bote"]="False", ["P1_Fecha"]=fecha,
                ["P1_N0"]="03", ["P1_N1"]="11", ["P1_N2"]="27", ["P1_N3"]="34", ["P1_N4"]="49",
                ["P1_O0"]="02", ["P1_O1"]="10",
                // PRIMITIVA (P2)
                ["P2_Sel"]="False", ["P2_Bote"]="True", ["P2_Fecha"]=fecha,
                ["P2_N0"]="08", ["P2_N1"]="15", ["P2_N2"]="22", ["P2_N3"]="30", ["P2_N4"]="37", ["P2_N5"]="43",
                ["P2_O0"]="9",
                // EL GORDO (P3)
                ["P3_Sel"]="False", ["P3_Bote"]="False", ["P3_Fecha"]=fecha,
                ["P3_N0"]="04", ["P3_N1"]="12", ["P3_N2"]="25", ["P3_N3"]="33", ["P3_N4"]="47",
                ["P3_O0"]="2",
                // LOTOTURF (P4)
                ["P4_Sel"]="False", ["P4_Bote"]="False", ["P4_Fecha"]=fecha,
                ["P4_N0"]="01", ["P4_N1"]="06", ["P4_N2"]="11", ["P4_N3"]="16", ["P4_N4"]="21", ["P4_N5"]="26",
                // EURODREAMS (P5)
                ["P5_Sel"]="False", ["P5_Bote"]="False", ["P5_Fecha"]=fecha,
                ["P5_N0"]="07", ["P5_N1"]="13", ["P5_N2"]="20", ["P5_N3"]="28", ["P5_N4"]="36", ["P5_N5"]="41",
                ["P5_O0"]="3",
                // JOKER (P6)
                ["P6_Sel"]="False", ["P6_Bote"]="False", ["P6_Fecha"]=fecha,
                ["P6_N0"]="4", ["P6_N1"]="7", ["P6_N2"]="2", ["P6_N3"]="9", ["P6_N4"]="1", ["P6_N5"]="6", ["P6_N6"]="3",
            });

            // ── Quiniela ─────────────────────────────────────────────────────
            string[] locales    = { "Real Madrid",   "FC Barcelona",  "Atlético Madrid", "Sevilla FC",
                                    "Valencia CF",   "Athletic Club", "Real Sociedad",   "Villarreal CF",
                                    "Real Betis",    "Celta de Vigo", "Getafe CF",       "Osasuna",
                                    "Rayo Vallecano","UD Almería",    "Deportivo Alavés" };
            string[] visitantes = { "Girona FC",     "Rayo Vallecano","Getafe CF",       "Real Betis",
                                    "Osasuna",       "Celta de Vigo", "Villarreal CF",   "UD Las Palmas",
                                    "Mallorca",      "Alavés",        "Granada CF",      "Cádiz CF",
                                    "Mallorca",      "Athletic Club", "Espanyol" };
            string[] resultados = { "1","2","X","1","1","X","2","1","X","1","1","X","1","2","1" };

            var quiniela = new Dictionary<string, string>
            {
                ["Fecha"]           = fecha,
                ["Jornada"]         = "17",
                ["AcertantesPleno"] = "2",
                ["BotePleno"]       = "458320",
            };
            for (int i = 0; i < 15; i++)
            {
                quiniela[$"Local{i}"]     = locales[i];
                quiniela[$"Visitante{i}"] = visitantes[i];
                quiniela[$"Resultado{i}"] = resultados[i];
                quiniela[$"Jugado{i}"]    = "True";
            }
            FormStateService.Instancia.EscribirSeccion("Quiniela", quiniela);

            // ── Lotería y Rótulos ────────────────────────────────────────────
            FormStateService.Instancia.EscribirSeccion("LoteriayRotulos", new()
            {
                ["Linea1Primera"] = "LOTERÍA NACIONAL",
                ["Linea1Segunda"] = "EN DIRECTO",
                ["Linea2"]        = "WWW.LOTERIASYAPUESTAS.ES",
            });

            LogService.Instancia.Registrar(LogNivel.Accion, "Debug", "Formulario rellenado con datos de prueba");
            MessageBox.Show("Campos rellenados con datos de prueba.", "Datos de prueba",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // ── Formateo ─────────────────────────────────────────────────────────

        private static string BuildResumen(Elemento e)
        {
            if (e.DatosQuiniela is { } q)
                return $"Jornada={q.Jornada}  Fecha={q.Fecha}  ({q.Partidos.Length} partidos)";

            var txt = string.Join("  ", e.Datos
                .Where(kv => !string.IsNullOrEmpty(kv.Value))
                .Select(kv => $"{kv.Key}={kv.Value}"));

            return txt.Length > 90 ? txt[..90] + "…" : txt;
        }

        private static string BuildDetalle(Elemento e)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Id      : {e.Id}");
            sb.AppendLine($"Tipo    : {e.Tipo}");
            sb.AppendLine($"Creado  : {e.CreadoEn:dd/MM/yyyy HH:mm:ss}");

            if (e.LogoId is { } logoId)
            {
                var logo = LogoRepository.Instancia.Get(logoId);
                sb.AppendLine($"Logo    : {logo?.Nombre ?? logoId.ToString()}");
            }

            if (e.Datos.Count > 0)
            {
                sb.AppendLine();
                foreach (var kv in e.Datos)
                    sb.AppendLine($"{kv.Key,-20}: {kv.Value}");
            }

            if (e.DatosQuiniela is { } q)
            {
                sb.AppendLine();
                sb.AppendLine($"{"Fecha",-20}: {q.Fecha}");
                sb.AppendLine($"{"Jornada",-20}: {q.Jornada}");
                sb.AppendLine($"{"Acertantes Pleno",-20}: {q.AcertantesPleno}");
                sb.AppendLine($"{"Bote Pleno",-20}: {q.BotePleno}");
                sb.AppendLine();
                for (int i = 0; i < q.Partidos.Length; i++)
                {
                    var p = q.Partidos[i];
                    var signo = Partido.Signo(p.Resultado, i == 14);
                    sb.AppendLine($"  {i + 1,2}. {p.EquipoLocal,-22} vs {p.EquipoVisitante,-22}  {p.Resultado,5}  {signo}");
                }
            }

            return sb.ToString();
        }
    }
}
