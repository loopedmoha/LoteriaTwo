using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using LoteriaTwo.Config;
using LoteriaTwo.Services;

namespace LoteriaTwo
{
    public partial class MainWindow : Window
    {
        private AppConfig _config = new();
        private BrainstormConnection? _connection;

        public MainWindow(AppConfig config, BrainstormConnection connection)
        {
            InitializeComponent();
            _config = config;
            _connection = connection;
            _connection.StateChanged += state => Dispatcher.BeginInvoke(() => ActualizarIndicador(state));
            ActualizarIndicador(_connection.State);
        }

        private void ActualizarIndicador(ConnectionState state)
        {
            bool ok = state == ConnectionState.Connected;
            EllipseConexion.Fill = new SolidColorBrush(ok
                ? Color.FromRgb(0x16, 0xA3, 0x4A)
                : Color.FromRgb(0xDC, 0x26, 0x26));
            TxtConexion.Text = ok ? $"IPF conectado ({_config.BrainstormIP})" : "Sin conexión IPF";
        }

        private async void ReconectarIPF_Click(object sender, RoutedEventArgs e)
        {
            if (_connection is null) return;
            bool ok = await _connection.ConnectAsync();
            if (!ok)
                MessageBox.Show($"Error al conectar a IPF ({_config.BrainstormIP})",
                                "Error de conexión", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        // GENERAL
        private void SaleUltimo_Click(object sender, RoutedEventArgs e) { }
        private void LimpiarFormulario_Click(object sender, RoutedEventArgs e) { }
        private void Guardar_Click(object sender, RoutedEventArgs e) { }
private void EntrarFondo_Click(object sender, RoutedEventArgs e) { }
        private void SaleFondo_Click(object sender, RoutedEventArgs e) { }
        private void AbrirPlaylist_Click(object sender, RoutedEventArgs e) { }
        private void Reset_Click(object sender, RoutedEventArgs e) { }
    }
}
