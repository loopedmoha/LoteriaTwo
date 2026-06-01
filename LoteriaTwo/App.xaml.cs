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
            ShutdownMode = ShutdownMode.OnExplicitShutdown;

            var selector = new ModoSelectorWindow();
            if (selector.ShowDialog() != true)
            {
                Shutdown();
                return;
            }

            var config = AppConfig.Load();
            var modo   = selector.ModoSeleccionado;

            BrainstormConnection[] connections = modo == ModoEstudio.Prado
                ? [new BrainstormConnection(config.PradoIP)]
                : [new BrainstormConnection(config.TorreIP1),
                   new BrainstormConnection(config.TorreIP2)];

            BrainstormService.Instancia.Inicializar(connections, config.BrainstormDB);
            RemoteShareService.Instancia.Inicializar(config.FotosShare, config.FotosUser, config.FotosPassword);

            var mainWindow = new MainWindow(config, modo, connections);
            MainWindow = mainWindow;
            ShutdownMode = ShutdownMode.OnMainWindowClose;

            var splash = new SplashWindow();
            splash.Show();

            bool connected = true;
            foreach (var conn in connections)
                connected &= await conn.ConnectAsync();

            splash.Close();
            mainWindow.Show();

            if (!connected)
            {
                MessageBox.Show(
                    "Error al conectar a uno o más IPF.",
                    "Error de conexión",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
        }
    }
}
