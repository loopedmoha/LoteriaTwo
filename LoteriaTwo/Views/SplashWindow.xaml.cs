using System.Windows;

namespace LoteriaTwo.Views
{
    public partial class SplashWindow : Window
    {
        public SplashWindow()
        {
            InitializeComponent();
        }

        public void SetStatus(string text) => TxtStatus.Text = text;
    }
}
