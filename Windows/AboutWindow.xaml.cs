using System.Windows;

namespace Decomp.Windows
{
    public partial class AboutWindow
    {
        public AboutWindow()
        {
            InitializeComponent();
            this.AttachCustomSystemMenu();
        }

        private void OkButtonClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
