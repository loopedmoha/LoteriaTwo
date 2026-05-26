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
        {
            if (LstLogos.SelectedItem is not PlaylistItem item) return;
            var el = ElementoRepository.Instancia.GetByTipo(item.Tipo)
                  ?? new Elemento { Tipo = item.Tipo };
            BrainstormService.Instancia.Entra(el);
        }
        private void SaleLogo_Click(object sender, RoutedEventArgs e)
        {
            if (LstLogos.SelectedItem is not PlaylistItem item) return;
            BrainstormService.Instancia.Sale(new Elemento { Tipo = item.Tipo });
        }

        // ── Acciones ELEMENTOS ────────────────────────────────────────────────

        private void ElemSale_Click(object sender, RoutedEventArgs e)
        {
            if (LstElementos.ItemsSource is not ObservableCollection<PlaylistItem> col) return;
            int i = LstElementos.SelectedIndex;
            if (i < 0) return;
            col.RemoveAt(i);
            if (col.Count > 0)
                LstElementos.SelectedIndex = Math.Min(i, col.Count - 1);
        }

        private void ElemSig_Click(object sender, RoutedEventArgs e)
        {
            if (LstElementos.ItemsSource is not ObservableCollection<PlaylistItem> col) return;

            int current = LstElementos.SelectedIndex < 0 ? -1 : LstElementos.SelectedIndex;
            int next = current + 1;
            int prev = current - 1;

            if (prev >= 0 && prev < col.Count)
            {
                Debug.WriteLine($"[Playlist] SALE  [{prev + 1}/{col.Count}] {col[prev].Nombre}");
                BrainstormService.Instancia.Sale(new Elemento { Tipo = col[prev].Tipo });
            }

            if (current >= 0 && current < col.Count)
            {
                var item     = col[current];
                var snapshot = LiveDataService.Instancia.GetSnapshot(item.Tipo);
                var datos    = snapshot.Length > 0 ? $"  |  {snapshot}" : string.Empty;
                Debug.WriteLine($"[Playlist] ENTRA [{current + 1}/{col.Count}] {item.Nombre}{datos}");

                var el = ElementoRepository.Instancia.GetByTipo(item.Tipo);
                if (el is not null)
                    BrainstormService.Instancia.Entra(el);
            }

            if (next < col.Count)
            {
                LstElementos.SelectedIndex = next;
                Debug.WriteLine($"[Playlist] Siguiente → [{next + 1}/{col.Count}] {col[next].Nombre}");
            }
            else
            {
                Debug.WriteLine($"[Playlist] Siguiente → fin de lista ({col.Count} elementos)");
            }
        }

        // ── Guardar / Cargar ──────────────────────────────────────────────────

        private void Guardar_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new Microsoft.Win32.SaveFileDialog
            {
                Title      = "Guardar playlist",
                Filter     = "Playlist JSON|*.json|Todos los archivos|*.*",
                DefaultExt = ".json",
                FileName   = $"playlist_{DateTime.Now:yyyy-MM-dd}"
            };
            if (dlg.ShowDialog() != true) return;
            try
            {
                PlaylistService.Instancia.Guardar(dlg.FileName);
                LogService.Instancia.Registrar(LogNivel.Accion, "Playlist",
                    $"Guardada → {System.IO.Path.GetFileName(dlg.FileName)}");
            }
            catch
            {
                MessageBox.Show("Error al guardar la playlist.", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Cargar_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new Microsoft.Win32.OpenFileDialog
            {
                Title      = "Cargar playlist",
                Filter     = "Playlist JSON|*.json|Todos los archivos|*.*",
                DefaultExt = ".json"
            };
            if (dlg.ShowDialog() != true) return;
            try
            {
                if (PlaylistService.Instancia.Cargar(dlg.FileName))
                {
                    BindListas();
                    LogService.Instancia.Registrar(LogNivel.Accion, "Playlist",
                        $"Cargada ← {System.IO.Path.GetFileName(dlg.FileName)}");
                }
                else
                    MessageBox.Show("Formato de playlist no válido.", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch
            {
                MessageBox.Show("Error al cargar la playlist.", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
