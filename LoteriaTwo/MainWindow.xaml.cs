using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using LoteriaTwo.Config;
using LoteriaTwo.Models;
using LoteriaTwo.Services;
using LoteriaTwo.Views;

namespace LoteriaTwo
{
    public partial class MainWindow : Window
    {
        private AppConfig              _config;
        private ModoEstudio            _modo;
        private BrainstormConnection[] _connections;
        private bool                   _conectadoAnterior;

        private readonly List<(Ellipse Dot, TextBlock Label)> _indicadores = new();

        public MainWindow(AppConfig config, ModoEstudio modo, BrainstormConnection[] connections)
        {
            InitializeComponent();
            _config      = config;
            _modo        = modo;
            _connections = connections;

            foreach (var conn in _connections)
                conn.StateChanged += _ => Dispatcher.BeginInvoke(ActualizarIndicador);

            Loaded += (_, _) => { BuildIndicadores(); ActualizarIndicador(); };

            this.AddHandler(Button.ClickEvent, new RoutedEventHandler(OnAnyButtonClick));

            LogService.Instancia.Registrar(LogNivel.Info, "App", "Aplicación iniciada");
            FormStateService.Instancia.Cargar();
        }

        // ── Conexión ─────────────────────────────────────────────────────────

        private void BuildIndicadores()
        {
            var secondary = (Brush)Application.Current.Resources["BrushTextSecondary"];
            var border    = (Brush)Application.Current.Resources["BrushBorder"];

            PnlConexion.Children.Clear();
            _indicadores.Clear();

            for (int i = 0; i < _connections.Length; i++)
            {
                if (i > 0)
                {
                    PnlConexion.Children.Add(new Border
                    {
                        Width = 1,
                        Background = border,
                        Margin = new Thickness(14, 2, 14, 2)
                    });
                }

                var dot = new Ellipse
                {
                    Width = 10, Height = 10,
                    Fill = new SolidColorBrush(Color.FromRgb(0xDC, 0x26, 0x26)),
                    Margin = new Thickness(0, 0, 6, 0),
                    VerticalAlignment = VerticalAlignment.Center
                };

                var lbl = new TextBlock
                {
                    FontFamily = new FontFamily("Segoe UI"),
                    FontSize = 12,
                    FontWeight = FontWeights.SemiBold,
                    Foreground = secondary,
                    VerticalAlignment = VerticalAlignment.Center
                };

                var sp = new StackPanel { Orientation = Orientation.Horizontal };
                sp.Children.Add(dot);
                sp.Children.Add(lbl);
                PnlConexion.Children.Add(sp);

                _indicadores.Add((dot, lbl));
            }
        }

        private void ActualizarIndicador()
        {
            if (_indicadores.Count == 0) return;

            bool todosOk = true;
            for (int i = 0; i < _connections.Length; i++)
            {
                bool ok = _connections[i].State == ConnectionState.Connected;
                if (!ok) todosOk = false;

                _indicadores[i].Dot.Fill = new SolidColorBrush(ok
                    ? Color.FromRgb(0x16, 0xA3, 0x4A)
                    : Color.FromRgb(0xDC, 0x26, 0x26));

                _indicadores[i].Label.Text = ok
                    ? (_connections.Length > 1 ? $"IPF {i + 1} — {_connections[i].Ip}" : $"IPF conectado — {_connections[i].Ip}")
                    : (_connections.Length > 1 ? $"IPF {i + 1} sin conexión" : "Sin conexión IPF");
            }

            if (todosOk == _conectadoAnterior) return;
            _conectadoAnterior = todosOk;
            LogService.Instancia.Registrar(LogNivel.Conexion, "IPF",
                todosOk
                    ? $"Conectado ({string.Join(", ", _connections.Select(c => c.Ip))})"
                    : "Conexión perdida");
        }

        private async void ReconectarIPF_Click(object sender, RoutedEventArgs e)
        {
            var selector = new Views.ModoSelectorWindow { Owner = this };
            if (selector.ShowDialog() != true) return;

            foreach (var conn in _connections)
                conn.Dispose();

            _modo = selector.ModoSeleccionado;
            _connections = _modo == ModoEstudio.Prado
                ? [new BrainstormConnection(_config.PradoIP)]
                : [new BrainstormConnection(_config.TorreIP1),
                   new BrainstormConnection(_config.TorreIP2)];

            foreach (var conn in _connections)
                conn.StateChanged += _ => Dispatcher.BeginInvoke(ActualizarIndicador);

            BrainstormService.Instancia.Inicializar(_connections, _config.BrainstormDB);
            SceneController.Instancia.Inicializar(UnrealConfig.Load(_modo));

            BuildIndicadores();
            ActualizarIndicador();

            LogService.Instancia.Registrar(LogNivel.Conexion, "IPF", "Reconectando…");
            bool ok = true;
            foreach (var conn in _connections)
                ok &= await conn.ConnectAsync();

            if (!ok)
                MessageBox.Show("Error al conectar a uno o más IPF.",
                                "Error de conexión", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        // ── Logger global de botones ──────────────────────────────────────────

        private void OnAnyButtonClick(object sender, RoutedEventArgs e)
        {
            if (e.Source is not Button btn) return;
            var label = btn.Content?.ToString() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(label)) return;

            var fuente = GetFuenteTab(btn) ?? "General";
            LogService.Instancia.Registrar(LogNivel.Accion, fuente, label);
        }

        private static string? GetFuenteTab(DependencyObject el)
        {
            var current = el;
            while (current != null)
            {
                if (current is FrameworkElement fe && fe.Visibility == Visibility.Visible)
                {
                    if (current is Views.LoteriayRotulosView) return "Lotería y Rótulos";
                    if (current is Views.DecimosView)         return "Décimos";
                    if (current is Views.QuinielaView)        return "Quiniela";
                    if (current is Views.SorteosBotesView)    return "Sorteos y Botes";
                }
                current = VisualTreeHelper.GetParent(current);
            }
            return null;
        }

        // ── Navegación lateral ────────────────────────────────────────────────

        private void Nav_Checked(object sender, RoutedEventArgs e)
        {
            if (ViewLoteria is null) return;
            ViewLoteria.Visibility      = NavLoteria.IsChecked      == true ? Visibility.Visible : Visibility.Collapsed;
            ViewDecimos.Visibility      = NavDecimos.IsChecked      == true ? Visibility.Visible : Visibility.Collapsed;
            ViewQuiniela.Visibility     = NavQuiniela.IsChecked     == true ? Visibility.Visible : Visibility.Collapsed;
            ViewSorteos.Visibility      = NavSorteos.IsChecked      == true ? Visibility.Visible : Visibility.Collapsed;
            ViewControlMundo.Visibility = NavControlMundo.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
        }

        // ── Debug ─────────────────────────────────────────────────────────────

        private void CalendarioSoporte_Click(object sender, RoutedEventArgs e)
            => System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(
                "https://trd-rtve.github.io/Calendario-de-Soporte/") { UseShellExecute = true });

        private void DebugElementos_Click(object sender, RoutedEventArgs e)
        {
            new DebugElementosWindow { Owner = this }.Show();
        }

        // ── GENERAL ───────────────────────────────────────────────────────────

        private void SaleUltimo_Click(object sender, RoutedEventArgs e) { }
        private void LimpiarFormulario_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("¿Limpiar todos los campos del formulario?", "Confirmar limpieza",
                    MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes) return;
            FormStateService.Instancia.Limpiar();
        }
        private void Guardar_Click(object sender, RoutedEventArgs e)
        {
            try { FormStateService.Instancia.Guardar(); }
            catch { MessageBox.Show("Error al guardar el estado.", "Error", MessageBoxButton.OK, MessageBoxImage.Error); }
        }
        private void EntrarFondo_Click(object sender, RoutedEventArgs e)
            => BrainstormService.Instancia.EntraFondo();
        private void SaleFondo_Click(object sender, RoutedEventArgs e)
            => BrainstormService.Instancia.SaleFondo();
        private void AbrirPlaylist_Click(object sender, RoutedEventArgs e) { }
        private void Reset_Click(object sender, RoutedEventArgs e) { }
    }
}
