using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media.Animation;
using System.Windows.Navigation;

namespace Decomp.Windows
{
    public partial class ErrorWindow
    {
        private static Window _window;
        public static void CloseCurrentErrorWindow()
        {
            if(_window == null) return;
            try { _window.Dispatcher?.Invoke(_window.Close); } catch { }
        }

        public ErrorWindow(Exception exception)
        {
            InitializeComponent();
            this.AttachCustomSystemMenu();
            //FatalErrorTextBlock.Text = (string)XamlServices.Parse("Fatal error in the application. Please report author of the program about error. <Hyperlink NavigateUri=\"http://rusmnb.ru/index.php?topic=17294.0\" RequestNavigate=\"BugRepostHyperlinkOnRequestNavigate\">http://rusmnb.ru/index.php?topic=17294.0</Hyperlink>");

            FatalErrorTextBlock.Inlines.Clear();
            FatalErrorTextBlock.Inlines.Add(Application.GetResource("LocalizationFatalError"));
            FatalErrorTextBlock.Inlines.Add(" ");
            var hyperLink = new Hyperlink { NavigateUri = new Uri("http://rusmnb.ru/index.php?topic=17294.0") };
            hyperLink.Inlines.Add("http://rusmnb.ru/index.php?topic=17294.0");
            hyperLink.RequestNavigate += BugRepostHyperlinkOnRequestNavigate;
            FatalErrorTextBlock.Inlines.Add(hyperLink);

            InformationTextBlock.Text = exception.ToString();
            SizeChanged += (s, e) =>
            {
                if (!e.WidthChanged) return;
                if (Math.Abs(ActualWidth - 433) > 1e-9) Width = 433;
            };
        }

        private void InfoOnExpanded(object sender, RoutedEventArgs e)
        {
            var animation = new DoubleAnimation
            {
                From = Height,
                By = InformationTextBlock.MinHeight,
                Duration = TimeSpan.FromSeconds(0.4)
            };
            BeginAnimation(HeightProperty, animation);
        }

        private void InfoOnCollapsed(object sender, RoutedEventArgs e)
        {
            var animation = new DoubleAnimation
            {
                From = Height,
                By = -InformationTextBlock.MinHeight,
                Duration = TimeSpan.FromSeconds(0.4)
            };
            BeginAnimation(HeightProperty, animation);
        }

        private void BugRepostHyperlinkOnRequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(e.Uri.ToString());
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            _window = null;
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            _window = this;
        }
    }
}
