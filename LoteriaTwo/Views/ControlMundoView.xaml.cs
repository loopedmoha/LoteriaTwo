using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using LoteriaTwo.Config;
using LoteriaTwo.Services;

namespace LoteriaTwo.Views
{
    public partial class ControlMundoView : UserControl
    {
        private bool _syncingSlider = false;

        public ControlMundoView()
        {
            InitializeComponent();
            SliderHora.AddHandler(
                Thumb.DragCompletedEvent,
                new DragCompletedEventHandler(SliderHora_DragCompleted));
            Loaded += (_, _) => PopularNiveles();
        }

        // ── Selector de nivel ─────────────────────────────────────────────────

        private void PopularNiveles()
        {
            PnlNiveles.Children.Clear();
            var niveles = SceneController.Instancia.Config?.UnrealSettings.ListaNiveles;
            if (niveles is null || niveles.Count == 0) return;

            bool primero = true;
            foreach (var nivel in niveles)
            {
                var display = NivelANombreAmigable(nivel.Nivel);
                var rb = new RadioButton
                {
                    Content = display,
                    Tag = nivel.Nivel,
                    IsChecked = primero,
                    GroupName = "NivelUnreal",
                    FontSize = 13,
                    Margin = new Thickness(0, 0, 16, 0),
                    VerticalAlignment = VerticalAlignment.Center,
                };
                rb.Checked += NivelUnreal_Checked;
                PnlNiveles.Children.Add(rb);
                primero = false;
            }
        }

        private void NivelUnreal_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton rb)
                SceneController.Instancia.NivelActivo = rb.Tag?.ToString();
        }

        private static string NivelANombreAmigable(string nivel) => nivel switch
        {
            "Loterias_v2" => "Loterías",
            "Quiniela_v2" => "Quiniela",
            _ => nivel.Replace("_v2", string.Empty).Replace("_", " "),
        };

        // ── Slider hora (solo actualiza la etiqueta) ──────────────────────────

        private void SliderHora_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (TxtHoraExacta is null) return;
            int total = (int)SliderHora.Value;
            TxtHoraExacta.Text = $"{total / 60:D2}:{total % 60:D2}";
        }

        private void SliderHora_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            // Solo actualiza la etiqueta; el envío requiere pulsar APLICAR HORA.
        }

        // ── Combos (solo actualizan la UI, no envían a Unreal) ────────────────

        private void CmbHoraDia_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SliderHora is null || _syncingSlider) return;
            if (CmbHoraDia.SelectedItem is not ComboBoxItem item) return;

            var presetName = item.Content?.ToString() ?? string.Empty;
            int minutes;
            var horasCfg = SceneController.Instancia.Config?.UnrealSettings.HorasPredefinidas;
            if (horasCfg?.TryGetValue(presetName, out var horaStr) == true)
                minutes = HoraToMinutos(horaStr);
            else
                minutes = presetName switch
                {
                    "Amanecer"      => 7  * 60,
                    "Mañana"        => 10 * 60,
                    "Mediodía"      => 14 * 60,
                    "Tarde"         => 19 * 60,
                    "Puesta de sol" => 21 * 60 + 38,
                    "Noche"         => 22 * 60,
                    _               => 12 * 60,
                };

            _syncingSlider = true;
            SliderHora.Value = minutes;
            _syncingSlider = false;
        }

        private void CmbClima_SelectionChanged(object sender, SelectionChangedEventArgs e) { }

        private void CmbFaseLunar_SelectionChanged(object sender, SelectionChangedEventArgs e) { }

        // ── Botones APLICAR ───────────────────────────────────────────────────

        private void AplicarClima_Click(object sender, RoutedEventArgs e)
        {
            if (CmbClima.SelectedItem is not ComboBoxItem item) return;
            var clima = item.Content?.ToString();
            if (!string.IsNullOrEmpty(clima))
                SceneController.Instancia.CambiarClima(clima);
        }

        private void AplicarHora_Click(object sender, RoutedEventArgs e)
        {
            int totalMinutos = (int)SliderHora.Value;
            int horas   = totalMinutos / 60;
            int minutos = totalMinutos % 60;
            SceneController.Instancia.CambiarHora(horas * 100 + minutos);
        }

        private void AplicarFaseLunar_Click(object sender, RoutedEventArgs e)
        {
            if (CmbFaseLunar.SelectedItem is not ComboBoxItem item) return;
            var fase = item.Content?.ToString();
            if (!string.IsNullOrEmpty(fase))
                SceneController.Instancia.CambiarFaseLunar(fase);
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private static int HoraToMinutos(string horaStr)
        {
            var parts = horaStr.Split(':');
            if (parts.Length == 2
                && int.TryParse(parts[0], out int h)
                && int.TryParse(parts[1], out int m))
                return h * 60 + m;
            return 720;
        }

    }
}
