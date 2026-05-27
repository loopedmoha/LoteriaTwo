using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using LoteriaTwo.Models;
using LoteriaTwo.Services;

namespace LoteriaTwo.Views
{
    public partial class QuinielaView : UserControl
    {
        private readonly TextBox[] _localBoxes      = new TextBox[15];
        private readonly TextBox[] _visitanteBoxes  = new TextBox[15];
        private readonly TextBox[] _resultadoBoxes  = new TextBox[15];
        private readonly TextBlock[] _signoBlocks   = new TextBlock[15];
        private readonly CheckBox[] _jugadoChecks   = new CheckBox[15];

        public QuinielaView()
        {
            InitializeComponent();
            BuildGrid();
            LiveDataService.Instancia.Registrar(TipoElemento.Quiniela,
                () => $"Jornada: {TxtJornada.Text}  Fecha: {TxtFecha.Text}");
            RegistrarFormState();
        }

        private void RegistrarFormState()
        {
            FormStateService.Instancia.RegistrarSeccion("Quiniela",
                leer: () =>
                {
                    var d = new Dictionary<string, string>
                    {
                        ["Fecha"]           = TxtFecha.Text,
                        ["Jornada"]         = TxtJornada.Text,
                        ["AcertantesPleno"] = TxtAcertantesPleno.Text,
                        ["BotePleno"]       = TxtBotePleno.Text,
                    };
                    for (int i = 0; i < 15; i++)
                    {
                        d[$"Local{i}"]     = _localBoxes[i].Text;
                        d[$"Visitante{i}"] = _visitanteBoxes[i].Text;
                        d[$"Resultado{i}"] = _resultadoBoxes[i].Text;
                        d[$"Jugado{i}"]    = (_jugadoChecks[i].IsChecked == true).ToString();
                    }
                    return d;
                },
                escribir: d =>
                {
                    TxtFecha.Text           = d.Gv("Fecha");
                    TxtJornada.Text         = d.Gv("Jornada");
                    TxtAcertantesPleno.Text = d.Gv("AcertantesPleno");
                    TxtBotePleno.Text       = d.Gv("BotePleno");
                    for (int i = 0; i < 15; i++)
                    {
                        _localBoxes[i].Text        = d.Gv($"Local{i}");
                        _visitanteBoxes[i].Text    = d.Gv($"Visitante{i}");
                        _resultadoBoxes[i].Text    = d.Gv($"Resultado{i}");
                        _jugadoChecks[i].IsChecked = d.Gv($"Jugado{i}") == "True";
                    }
                });
        }

        // ── Grid programático ────────────────────────────────────────────────

        private void BuildGrid()
        {
            var primary   = (Brush)Application.Current.Resources["BrushTextPrimary"];
            var secondary = (Brush)Application.Current.Resources["BrushTextSecondary"];

            GridPartidos.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(28) });
            GridPartidos.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            GridPartidos.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            GridPartidos.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(85) });
            GridPartidos.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(32) });
            GridPartidos.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            // Fila de cabeceras
            GridPartidos.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            AddHeaderCell("Equipo Local",      0, 1, primary);
            AddHeaderCell("Equipo Visitante",  0, 2, primary);
            AddHeaderCell("RESULTADO",         0, 3, primary);

            // 15 filas de partidos
            for (int i = 0; i < 15; i++)
            {
                GridPartidos.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                int gridRow = i + 1;
                int idx = i;
                double topM = (i == 14) ? 10.0 : 2.0; // separación visual antes del partido 15

                // Número
                var numTb = new TextBlock
                {
                    Text = (i + 1).ToString(),
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    Margin = new Thickness(0, topM, 6, 2),
                    Foreground = secondary,
                    FontWeight = (i == 14) ? FontWeights.Bold : FontWeights.Normal
                };
                Place(numTb, gridRow, 0);

                // Equipo local
                _localBoxes[i] = new TextBox { Margin = new Thickness(0, topM, 4, 2) };
                Place(_localBoxes[i], gridRow, 1);

                // Equipo visitante
                _visitanteBoxes[i] = new TextBox { Margin = new Thickness(0, topM, 4, 2) };
                Place(_visitanteBoxes[i], gridRow, 2);

                // Resultado
                _resultadoBoxes[i] = new TextBox
                {
                    Margin = new Thickness(0, topM, 4, 2),
                    MaxLength = 6,
                    HorizontalContentAlignment = HorizontalAlignment.Center
                };
                _resultadoBoxes[i].TextChanged += (s, e) => ActualizarSigno(idx);
                Place(_resultadoBoxes[i], gridRow, 3);

                // Signo (calculado automáticamente)
                _signoBlocks[i] = new TextBlock
                {
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    FontWeight = FontWeights.Bold,
                    FontSize = 14,
                    Margin = new Thickness(2, topM, 2, 2),
                    Foreground = secondary
                };
                Place(_signoBlocks[i], gridRow, 4);

                // Jugado
                _jugadoChecks[i] = new CheckBox
                {
                    Content = "Jugado",
                    IsChecked = true,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(4, topM, 0, 2),
                    FontSize = 10,
                    Foreground = primary
                };
                Place(_jugadoChecks[i], gridRow, 5);
            }
        }

        private void Place(UIElement el, int row, int col)
        {
            Grid.SetRow(el, row);
            Grid.SetColumn(el, col);
            GridPartidos.Children.Add(el);
        }

        private void AddHeaderCell(string text, int row, int col, Brush foreground)
        {
            var tb = new TextBlock
            {
                Text = text,
                FontWeight = FontWeights.Bold,
                FontSize = 11,
                Foreground = foreground,
                Margin = new Thickness(0, 2, 8, 5)
            };
            Place(tb, row, col);
        }

        // ── Cálculo de signo ─────────────────────────────────────────────────

        private void ActualizarSigno(int idx)
        {
            string signo = Partido.Signo(_resultadoBoxes[idx].Text, idx == 14);
            _signoBlocks[idx].Text = signo;
            _signoBlocks[idx].Foreground = signo switch
            {
                "1" => new SolidColorBrush(Color.FromRgb(0x16, 0xA3, 0x4A)), // verde
                "X" => new SolidColorBrush(Color.FromRgb(0x25, 0x63, 0xEB)), // azul
                "2" => new SolidColorBrush(Color.FromRgb(0xDC, 0x26, 0x26)), // rojo
                "0" => new SolidColorBrush(Color.FromRgb(0x7C, 0x3A, 0xED)), // morado
                "M" => new SolidColorBrush(Color.FromRgb(0xD9, 0x77, 0x06)), // ámbar
                _   => (Brush)Application.Current.Resources["BrushTextSecondary"]
            };
        }

        // ── Modelo ───────────────────────────────────────────────────────────

        public Quiniela GetQuiniela()
        {
            var q = new Quiniela
            {
                Fecha            = TxtFecha.Text,
                Jornada          = TxtJornada.Text,
                AcertantesPleno  = TxtAcertantesPleno.Text,
                BotePleno        = TxtBotePleno.Text
            };
            for (int i = 0; i < 15; i++)
            {
                q.Partidos[i].EquipoLocal      = _localBoxes[i].Text;
                q.Partidos[i].EquipoVisitante  = _visitanteBoxes[i].Text;
                q.Partidos[i].Resultado        = _resultadoBoxes[i].Text;
                q.Partidos[i].Jugado           = _jugadoChecks[i].IsChecked == true;
            }
            return q;
        }

        // ── Handlers ─────────────────────────────────────────────────────────

        private void CargarQuiniela_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new Microsoft.Win32.OpenFileDialog
            {
                Title = "Cargar quiniela",
                Filter = "Quiniela JSON|*.json|Todos los archivos|*.*",
                DefaultExt = ".json"
            };
            if (dlg.ShowDialog() != true) return;

            try
            {
                var json = File.ReadAllText(dlg.FileName);
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var q = JsonSerializer.Deserialize<Quiniela>(json, options);
                if (q != null) CargarDesdeQuiniela(q);
            }
            catch
            {
                MessageBox.Show("Error al cargar el archivo de quiniela.",
                                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CargarDesdeQuiniela(Quiniela q)
        {
            TxtFecha.Text           = q.Fecha;
            TxtJornada.Text         = q.Jornada;
            TxtAcertantesPleno.Text = q.AcertantesPleno;
            TxtBotePleno.Text       = q.BotePleno;

            for (int i = 0; i < 15 && i < q.Partidos.Length; i++)
            {
                var p = q.Partidos[i];
                _localBoxes[i].Text         = p.EquipoLocal;
                _visitanteBoxes[i].Text     = p.EquipoVisitante;
                _resultadoBoxes[i].Text     = p.Resultado;   // dispara ActualizarSigno vía TextChanged
                _jugadoChecks[i].IsChecked  = p.Jugado;
            }
        }
        public Elemento? UltimoElemento { get; private set; }

        private void Previsualizar(Elemento el)
        {
            ElementoRepository.Instancia.Add(el);
            UltimoElemento = el;
            LogService.Instancia.Registrar(LogNivel.Accion, el.Tipo.ToString(),
                "P → " + el.ToLogString());
            PlaylistService.Instancia.AgregarElemento(el);
            PlaylistService.Instancia.AgregarLogo("Quiniela", el.Tipo);
        }

        private void ChkModoQuiniela_Checked(object sender, RoutedEventArgs e)
        {
            BrainstormService.Instancia.ModoQuiniela = true;
            BrainstormService.Instancia.Enviar(BrainstormService.Instancia.CambiarFondo("Rojo"));
        }
        private void ChkModoQuiniela_Unchecked(object sender, RoutedEventArgs e)
        {
            BrainstormService.Instancia.ModoQuiniela = false;
            BrainstormService.Instancia.Enviar(BrainstormService.Instancia.CambiarFondo("Azul"));
        }

        private void EntraQuiniela_Click(object sender, RoutedEventArgs e)
        {
            var el = new Elemento { Tipo = TipoElemento.Quiniela };
            el.DatosQuiniela = GetQuiniela();
            BrainstormService.Instancia.Entra(el);
        }
        private void SaleQuiniela_Click(object sender, RoutedEventArgs e)
            => BrainstormService.Instancia.Sale(new Elemento { Tipo = TipoElemento.Quiniela });
        private void PQuiniela_Click(object sender, RoutedEventArgs e)
        {
            var el = new Elemento { Tipo = TipoElemento.Quiniela };
            el.DatosQuiniela = GetQuiniela();
            Previsualizar(el);
        }
        private void EntraPleno15_Click(object sender, RoutedEventArgs e)
        {
            var el = new Elemento { Tipo = TipoElemento.Pleno15 };
            el.DatosQuiniela = GetQuiniela();
            el["AcertantesPleno"] = TxtAcertantesPleno.Text;
            el["BotePleno"]       = TxtBotePleno.Text;
            BrainstormService.Instancia.Entra(el);
        }
        private void SalePleno15_Click(object sender, RoutedEventArgs e)
            => BrainstormService.Instancia.Sale(new Elemento { Tipo = TipoElemento.Pleno15 });
        private void PPleno15_Click(object sender, RoutedEventArgs e)
        {
            var el = new Elemento { Tipo = TipoElemento.Pleno15 };
            el["AcertantesPleno"] = TxtAcertantesPleno.Text;
            el["BotePleno"]       = TxtBotePleno.Text;
            Previsualizar(el);
        }
    }
}
