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
