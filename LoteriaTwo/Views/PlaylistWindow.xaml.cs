using System.Windows;

namespace LoteriaTwo.Views
{
    public partial class PlaylistWindow : Window
    {
        public PlaylistWindow()
        {
            InitializeComponent();
        }

        public void RecargarUI() => ThePanel.RecargarUI();
    }
}
