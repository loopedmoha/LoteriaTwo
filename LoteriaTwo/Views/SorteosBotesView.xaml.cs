using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace LoteriaTwo.Views
{
    public partial class SorteosBotesView : UserControl
    {
        // Configuración de cada juego en PREMIADOS: nombre, bolas principales, bolas extra
        private static readonly (string Nombre, int Bolas, int Extras)[] JuegosPremiados =
        {
            ("BONOLOTO",       6, 1),  // 6 números + 1 complementario
            ("EUROMILLONES M", 5, 2),  // 5 números + 2 estrellas
            ("PRIMITIVA",      6, 2),  // 6 números + complementario + reintegro
            ("EL GORDO",       5, 1),  // 5 números + 1 clave
            ("LOTOTURF",       7, 0),  // 7 números
            ("EURODREAMS",     6, 1),  // 6 números + 1 dream
        };

        private readonly RadioButton[] _rdbPremiado    = new RadioButton[6];
        private readonly CheckBox[]    _chkBotePremiado = new CheckBox[6];
        private readonly TextBox[][]   _txtNumeros      = new TextBox[6][];
        private readonly TextBox[][]   _txtOtros        = new TextBox[6][];
        private readonly TextBox[]     _txtFechaPremiado = new TextBox[6];

        public SorteosBotesView()
        {
            InitializeComponent();
            BuildPremiados();
        }

        // ── Construcción programática de filas PREMIADOS ─────────────────────

        private void BuildPremiados()
        {
            var primary   = (Brush)Application.Current.Resources["BrushTextPrimary"];
            var secondary = (Brush)Application.Current.Resources["BrushTextSecondary"];

            GridPremiados.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(30) });   // radio
            GridPremiados.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(155) });  // nombre
            GridPremiados.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(60) });   // BOTE
            GridPremiados.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(255) });  // números
            GridPremiados.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(105) });  // otros
            GridPremiados.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(115) }); // fecha

            // Fila de cabeceras
            GridPremiados.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            PlaceP(MakeHeader("NUMEROS PREMIADOS", primary), 0, 3);
            PlaceP(MakeHeader("OTROS",             primary), 0, 4);
            PlaceP(MakeHeader("FECHA",             primary), 0, 5);

            // Filas de juegos
            for (int i = 0; i < JuegosPremiados.Length; i++)
            {
                var (nombre, bolas, extras) = JuegosPremiados[i];
                GridPremiados.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
                int row = i + 1;
                int idx = i;

                // Radio button de selección
                _rdbPremiado[i] = new RadioButton
                {
                    GroupName = "PremiadoGame",
                    Margin = new Thickness(0, 2, 4, 2),
                    VerticalAlignment = VerticalAlignment.Center,
                    Foreground = primary
                };
                PlaceP(_rdbPremiado[i], row, 0);

                // Nombre del juego
                PlaceP(new TextBlock
                {
                    Text = nombre,
                    FontWeight = FontWeights.SemiBold,
                    Foreground = primary,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(0, 2, 8, 2)
                }, row, 1);

                // Checkbox BOTE
                _chkBotePremiado[i] = new CheckBox
                {
                    Content = "BOTE",
                    Foreground = primary,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(0, 2, 4, 2),
                    FontSize = 10
                };
                PlaceP(_chkBotePremiado[i], row, 2);

                // Cajas de números premiados
                var spNums = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center };
                _txtNumeros[i] = new TextBox[bolas];
                for (int j = 0; j < bolas; j++)
                {
                    var tb = new TextBox
                    {
                        Width = 33,
                        MaxLength = 2,
                        HorizontalContentAlignment = HorizontalAlignment.Center,
                        Margin = new Thickness(0, 2, 3, 2)
                    };
                    _txtNumeros[i][j] = tb;
                    spNums.Children.Add(tb);
                }
                PlaceP(spNums, row, 3);

                // Cajas de números extra (estrellas, complementario, etc.)
                var spOtros = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center };
                _txtOtros[i] = new TextBox[extras];
                for (int j = 0; j < extras; j++)
                {
                    var tb = new TextBox
                    {
                        Width = 44,
                        MaxLength = 2,
                        HorizontalContentAlignment = HorizontalAlignment.Center,
                        Margin = new Thickness(0, 2, 3, 2)
                    };
                    _txtOtros[i][j] = tb;
                    spOtros.Children.Add(tb);
                }
                PlaceP(spOtros, row, 4);

                // Fecha
                _txtFechaPremiado[i] = new TextBox { Margin = new Thickness(0, 2, 0, 2), VerticalAlignment = VerticalAlignment.Center, Width = 110 };
                PlaceP(_txtFechaPremiado[i], row, 5);
            }
        }

        private void PlaceP(UIElement el, int row, int col)
        {
            Grid.SetRow(el, row);
            Grid.SetColumn(el, col);
            GridPremiados.Children.Add(el);
        }

        private static TextBlock MakeHeader(string text, Brush foreground) =>
            new TextBlock
            {
                Text = text,
                FontWeight = FontWeights.Bold,
                FontSize = 10,
                Foreground = foreground,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 4)
            };

        // ── Handlers — LOGOS ─────────────────────────────────────────────────

        private void LogoEntra_Click(object sender, RoutedEventArgs e) { }
        private void LogoSale_Click(object sender, RoutedEventArgs e) { }
        private void LogoEncadena_Click(object sender, RoutedEventArgs e) { }
        private void LogoP_Click(object sender, RoutedEventArgs e) { }

        // ── Handlers — BOTES ─────────────────────────────────────────────────

        private void BoteEntra_Click(object sender, RoutedEventArgs e) { }
        private void BoteSale_Click(object sender, RoutedEventArgs e) { }
        private void BoteEncadena_Click(object sender, RoutedEventArgs e) { }
        private void BoteP_Click(object sender, RoutedEventArgs e) { }

        // ── Handlers — PREMIADOS ─────────────────────────────────────────────

        private void PremiadosP_Click(object sender, RoutedEventArgs e) { }
        private void PremiadosEntra_Click(object sender, RoutedEventArgs e) { }
        private void PremiadosSale_Click(object sender, RoutedEventArgs e) { }
        private void PremiadosEncadena_Click(object sender, RoutedEventArgs e) { }
        private void PremiadosOrdenar_Click(object sender, RoutedEventArgs e) { }

        // ── Handlers — FALDONES ──────────────────────────────────────────────

        private void FaldonEntra_Click(object sender, RoutedEventArgs e) { }
        private void FaldonSale_Click(object sender, RoutedEventArgs e) { }
        private void FaldonCorregir_Click(object sender, RoutedEventArgs e) { }

        // ── Handlers — EL MILLÓN / JOKER ─────────────────────────────────────

        private void MillonP_Click(object sender, RoutedEventArgs e) { }
        private void MillonEntra_Click(object sender, RoutedEventArgs e) { }
        private void MillonSale_Click(object sender, RoutedEventArgs e) { }
        private void JokerEntra_Click(object sender, RoutedEventArgs e) { }
        private void JokerSale_Click(object sender, RoutedEventArgs e) { }

        // ── Handlers — EUROMILLONES ──────────────────────────────────────────

        private void EuroMillonEntra_Click(object sender, RoutedEventArgs e) { }
        private void EuroMillonSale_Click(object sender, RoutedEventArgs e) { }
        private void EuroMillonP_Click(object sender, RoutedEventArgs e) { }

        // ── Handlers — EURODREAMS ────────────────────────────────────────────

        private void EurodreamsLunes_Click(object sender, RoutedEventArgs e) { }
        private void EurodreamsJueves_Click(object sender, RoutedEventArgs e) { }
        private void EurodreamsEntra_Click(object sender, RoutedEventArgs e) { }
        private void EurodreamsSale_Click(object sender, RoutedEventArgs e) { }
        private void EurodreamsP_Click(object sender, RoutedEventArgs e) { }
        private void EurodreamsPremiado_Click(object sender, RoutedEventArgs e) { }
        private void EurodreamsProximo_Click(object sender, RoutedEventArgs e) { }
    }
}
