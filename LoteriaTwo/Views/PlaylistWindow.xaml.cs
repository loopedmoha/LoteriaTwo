using System.Windows;
using System.Windows.Controls;
using LoteriaTwo.Services;

namespace LoteriaTwo.Views
{
    public partial class PlaylistWindow : Window
    {
        public PlaylistWindow()
        {
            InitializeComponent();
        }

        public void RecargarUI() => ThePanel.RecargarUI();

        private void GuardarPlaylist_Click(object sender, RoutedEventArgs e)
        {
            try { PlaylistService.Instancia.Guardar(); }
            catch { MessageBox.Show("Error al guardar la playlist.", "Error", MessageBoxButton.OK, MessageBoxImage.Error); }
        }

        private void CargarPlaylist_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (PlaylistService.Instancia.Cargar())
                    RecargarUI();
                else
                    MessageBox.Show("No se encontró el fichero Playlist.json.", "Cargar playlist",
                        MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch { MessageBox.Show("Error al cargar la playlist.", "Error", MessageBoxButton.OK, MessageBoxImage.Error); }
        }

        private void LimpiarPlaylist_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("¿Limpiar la playlist activa?", "Confirmar",
                    MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes) return;
            var activa = PlaylistService.Instancia.Activa;
            activa.Logos.Clear();
            activa.Elementos.Clear();
        }
    }
}
