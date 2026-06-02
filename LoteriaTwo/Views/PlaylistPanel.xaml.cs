using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using LoteriaTwo.Models;
using LoteriaTwo.Services;

namespace LoteriaTwo.Views
{
    public partial class PlaylistPanel : UserControl
    {
        public PlaylistPanel()
        {
            InitializeComponent();
            Loaded += (_, _) => BindListas();
        }

        // ── Binding ───────────────────────────────────────────────────────────

        private void BindListas()
        {
            if (LstLogos is null || LstElementos is null) return;

            if (LstLogos.ItemsSource is ObservableCollection<PlaylistItem> prevL)
                prevL.CollectionChanged -= OnCollectionChanged;
            if (LstElementos.ItemsSource is ObservableCollection<PlaylistItem> prevE)
                prevE.CollectionChanged -= OnCollectionChanged;

            var activa = PlaylistService.Instancia.Activa;
            LstLogos.ItemsSource     = activa.Logos;
            LstElementos.ItemsSource = activa.Elementos;

            activa.Logos.CollectionChanged     += OnCollectionChanged;
            activa.Elementos.CollectionChanged += OnCollectionChanged;

            ActualizarContadores();
        }

        private void OnCollectionChanged(object? sender,
            System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
            => ActualizarContadores();

        private void ActualizarContadores()
        {
            if (TxtHeaderLogos is null || TxtHeaderElementos is null) return;
            var activa = PlaylistService.Instancia.Activa;
            TxtHeaderLogos.Text     = $"LOGOS ({activa.Logos.Count})";
            TxtHeaderElementos.Text = $"ELEMENTOS ({activa.Elementos.Count})";
        }

        // ── Selector de playlist ──────────────────────────────────────────────

        private void PlSel_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is not RadioButton rb || !int.TryParse(rb.Tag?.ToString(), out int idx)) return;
            PlaylistService.Instancia.IndicePlaylistActiva = idx;
            BindListas();
        }

        // ── Reordenación ──────────────────────────────────────────────────────

        private static void MoverArriba(ListBox lst)
        {
            if (lst.ItemsSource is not ObservableCollection<PlaylistItem> col) return;
            int i = lst.SelectedIndex;
            if (i <= 0) return;
            col.Move(i, i - 1);
            lst.SelectedIndex = i - 1;
        }

        private static void MoverAbajo(ListBox lst)
        {
            if (lst.ItemsSource is not ObservableCollection<PlaylistItem> col) return;
            int i = lst.SelectedIndex;
            if (i < 0 || i >= col.Count - 1) return;
            col.Move(i, i + 1);
            lst.SelectedIndex = i + 1;
        }

        private void LogoUp_Click(object sender, RoutedEventArgs e)   => MoverArriba(LstLogos);
        private void LogoDown_Click(object sender, RoutedEventArgs e) => MoverAbajo(LstLogos);
        private void ElemUp_Click(object sender, RoutedEventArgs e)   => MoverArriba(LstElementos);
        private void ElemDown_Click(object sender, RoutedEventArgs e) => MoverAbajo(LstElementos);

        // ── Acciones LOGOS ────────────────────────────────────────────────────

        private void EntraLogo_Click(object sender, RoutedEventArgs e)
            => BrainstormService.Instancia.EntraLogos();

        private void SaleLogo_Click(object sender, RoutedEventArgs e)
            => BrainstormService.Instancia.SaleLogos();

        // ── Acciones ELEMENTOS ────────────────────────────────────────────────

        private bool _colasActivas = false;

        private void ElemSale_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("[Playlist] SALE activo");
            if (_colasActivas)
            {
                _colasActivas = false;
                SceneController.Instancia.PantallaNDI();
            }
            else
            {
                BrainstormService.Instancia.SaleActivo();
            }
        }

        private bool _pantallaBajada = false;

        private void BtnPantalla_Click(object sender, RoutedEventArgs e)
        {
            _pantallaBajada     = !_pantallaBajada;
            BtnPantalla.Content = _pantallaBajada ? "Subir Pantalla" : "Bajar Pantalla";

            if (_pantallaBajada)
                SceneController.Instancia.BajarPantalla();
            else
                SceneController.Instancia.SubirPantalla();
        }

        private bool _actualizandoIndice;

        private void ElemSig_Click(object sender, RoutedEventArgs e)
        {
            if (LstElementos.ItemsSource is not ObservableCollection<PlaylistItem> col) return;

            int current = LstElementos.SelectedIndex;
            if (current < 0 || current >= col.Count) return;

            var item     = col[current];
            var snapshot = LiveDataService.Instancia.GetSnapshot(item.Tipo);
            var datos    = snapshot.Length > 0 ? $"  |  {snapshot}" : string.Empty;
            Debug.WriteLine($"[Playlist] ENTRA [{current + 1}/{col.Count}] {item.Nombre}{datos}");

            var el = ElementoRepository.Instancia.Get(item.ElementoId);
            if (el is not null)
            {
                bool esColas = el.Tipo == TipoElemento.Imagen && el["Foto"] == "Colas";

                if (_colasActivas && !esColas)
                    SceneController.Instancia.PantallaNDI();

                _colasActivas = esColas;

                if (esColas)
                    SceneController.Instancia.PantallaSDI();
                else
                    BrainstormService.Instancia.Entra(el);
            }

            var logoNombre = GetLogoEnIndice(current);
            if (logoNombre is not null)
            {
                Debug.WriteLine($"[Playlist] NEXT LOGO [{current + 1}] Logo{logoNombre}");
                BrainstormService.Instancia.NextLogo(logoNombre);
            }

            int next = current + 1;
            _actualizandoIndice = true;
            if (next < col.Count)
            {
                LstElementos.SelectedIndex = next;
                LstLogos.SelectedIndex     = next;
                Debug.WriteLine($"[Playlist] → [{next + 1}/{col.Count}] {col[next].Nombre}");
            }
            else
            {
                Debug.WriteLine($"[Playlist] → fin de lista ({col.Count} elementos)");
            }
            _actualizandoIndice = false;
        }

        private string? GetLogoEnIndice(int idx)
        {
            if (LstLogos.ItemsSource is not ObservableCollection<PlaylistItem> logos) return null;
            if (idx < 0 || idx >= logos.Count) return null;
            return logos[idx].Nombre;
        }

        private void LstElementos_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_actualizandoIndice) return;

            int idx = LstElementos.SelectedIndex;
            if (idx < 0) return;

            _actualizandoIndice = true;
            LstLogos.SelectedIndex = idx;
            _actualizandoIndice = false;

            var logoNombre = GetLogoEnIndice(idx);
            if (logoNombre is null) return;

            Debug.WriteLine($"[Playlist] SYNC LOGO [{idx + 1}] Logo{logoNombre}");
            BrainstormService.Instancia.SyncLogo(logoNombre);
        }

        // ── Eliminar / Limpiar ────────────────────────────────────────────────

        private static void EliminarSeleccionado(ListBox lst)
        {
            if (lst.ItemsSource is not ObservableCollection<PlaylistItem> col) return;
            int i = lst.SelectedIndex;
            if (i < 0 || i >= col.Count) return;
            col.RemoveAt(i);
            if (col.Count > 0)
                lst.SelectedIndex = Math.Min(i, col.Count - 1);
        }

        private void LogoEliminar_Click(object sender, RoutedEventArgs e)
            => EliminarSeleccionado(LstLogos);

        private void ElemEliminar_Click(object sender, RoutedEventArgs e)
            => EliminarSeleccionado(LstElementos);

        private void LimpiarPlaylist_Click(object sender, RoutedEventArgs e)
        {
            var activa = PlaylistService.Instancia.Activa;
            activa.Logos.Clear();
            activa.Elementos.Clear();
        }

        // ── Guardar / Cargar ──────────────────────────────────────────────────

        private void Guardar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PlaylistService.Instancia.Guardar();
                LogService.Instancia.Registrar(LogNivel.Accion, "Playlist", "Guardada → Playlist.json");
            }
            catch
            {
                MessageBox.Show("Error al guardar la playlist.", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Cargar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (PlaylistService.Instancia.Cargar())
                {
                    BindListas();
                    LogService.Instancia.Registrar(LogNivel.Accion, "Playlist", "Cargada ← Playlist.json");
                }
                else
                    MessageBox.Show("No se encontró el fichero Playlist.json.", "Cargar playlist",
                        MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch
            {
                MessageBox.Show("Error al cargar la playlist.", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
