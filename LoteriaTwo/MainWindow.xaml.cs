using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using LoteriaTwo.Config;
using LoteriaTwo.Models;
using LoteriaTwo.Services;
using LoteriaTwo.Views;

namespace LoteriaTwo
{
    public partial class MainWindow : Window
    {
        private AppConfig _config = new();
        private BrainstormConnection? _connection;
        private ConnectionState _estadoAnterior = ConnectionState.Disconnected;

        public MainWindow(AppConfig config, BrainstormConnection connection)
        {
            InitializeComponent();
            _config = config;
            _connection = connection;
            _connection.StateChanged += state => Dispatcher.BeginInvoke(() => ActualizarIndicador(state));
            ActualizarIndicador(_connection.State);

            // Captura global de pulsaciones de botones en toda la ventana
            this.AddHandler(Button.ClickEvent, new RoutedEventHandler(OnAnyButtonClick));

            LogService.Instancia.Registrar(LogNivel.Info, "App", "Aplicación iniciada");
            FormStateService.Instancia.Cargar();
        }

        // ── Conexión ─────────────────────────────────────────────────────────

        private void ActualizarIndicador(ConnectionState state)
        {
            bool ok = state == ConnectionState.Connected;
            EllipseConexion.Fill = new SolidColorBrush(ok
                ? Color.FromRgb(0x16, 0xA3, 0x4A)
                : Color.FromRgb(0xDC, 0x26, 0x26));
            TxtConexion.Text = ok ? $"IPF conectado ({_config.BrainstormIP})" : "Sin conexión IPF";

            if (state == _estadoAnterior) return;
            _estadoAnterior = state;
            LogService.Instancia.Registrar(LogNivel.Conexion, "IPF",
                ok ? $"Conectado — {_config.BrainstormIP}" : "Conexión perdida");
        }

        private async void ReconectarIPF_Click(object sender, RoutedEventArgs e)
        {
            if (_connection is null) return;
            LogService.Instancia.Registrar(LogNivel.Conexion, "IPF", "Reconectando…");
            bool ok = await _connection.ConnectAsync();
            if (!ok)
                MessageBox.Show($"Error al conectar a IPF ({_config.BrainstormIP})",
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
                // Sidebar nav: look for named content views
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
            ViewLoteria.Visibility  = NavLoteria.IsChecked  == true ? Visibility.Visible : Visibility.Collapsed;
            ViewDecimos.Visibility  = NavDecimos.IsChecked  == true ? Visibility.Visible : Visibility.Collapsed;
            ViewQuiniela.Visibility = NavQuiniela.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
            ViewSorteos.Visibility  = NavSorteos.IsChecked  == true ? Visibility.Visible : Visibility.Collapsed;
        }

        // ── Debug ─────────────────────────────────────────────────────────────

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
