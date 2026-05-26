using System.Windows;
using LoteriaTwo.Config;
using LoteriaTwo.Services;
using LoteriaTwo.Views;

namespace LoteriaTwo
{
    public partial class App : Application
    {
        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            ShutdownMode = ShutdownMode.OnMainWindowClose;

            var config = AppConfig.Load();
            var connection = new BrainstormConnection(config.BrainstormIP);
            BrainstormService.Instancia.Inicializar(connection, config.BrainstormDB);

            // Registrar el MainWindow ANTES del splash para que WPF
            // no cierre la app cuando el splash se cierre.
            var mainWindow = new MainWindow(config, connection);
            MainWindow = mainWindow;

            var splash = new SplashWindow();
            splash.Show();

            bool connected = await connection.ConnectAsync();

            splash.Close();
            mainWindow.Show();

            if (!connected)
            {
                MessageBox.Show(
                    $"Error al conectar a IPF ({config.BrainstormIP})",
                    "Error de conexión",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
        }
    }
}
