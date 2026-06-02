using System.Collections.Generic;
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
            SceneController.Instancia.Inicializar(UnrealConfig.Load(modo));

            var mainWindow = new MainWindow(config, modo, connections);
            MainWindow = mainWindow;
            ShutdownMode = ShutdownMode.OnMainWindowClose;

            var splash = new SplashWindow();
            splash.Show();

            // ── Paso 1: IPF ──────────────────────────────────────────────────
            splash.SetStatus("Conectando con Brainstorm IPF…");
            bool ipfOk = true;
            foreach (var conn in connections)
                ipfOk &= await conn.ConnectAsync();

            // ── Paso 2: Unreal Engine ────────────────────────────────────────
            splash.SetStatus("Comprobando conexión a Unreal Engine…");
            var (unrealOk, unrealTotal) = await SceneController.Instancia.PingAllAsync();

            splash.Close();
            mainWindow.Show();

            // ── Advertencias ─────────────────────────────────────────────────
            var avisos = new List<string>();
            if (!ipfOk)
                avisos.Add("No se pudo conectar a uno o más IPF (Brainstorm).");
            if (unrealTotal > 0 && unrealOk < unrealTotal)
                avisos.Add($"Unreal Engine: {unrealTotal - unrealOk} de {unrealTotal} IPs sin respuesta.");

            if (avisos.Count > 0)
                MessageBox.Show(
                    string.Join("\n\n", avisos),
                    "Advertencias de conexión",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
        }
    }
}
