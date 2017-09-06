using System.Windows;
using System.Windows.Controls;
using Decomp.Core;

namespace Decomp.Windows
{
    /// <summary>
    /// Interaction logic for HelpWindow.xaml
    /// </summary>
    public partial class HelpWindow
    {
        private readonly SimpleTrie<string> _helpArticles;
        private Button _prevButton;

        private Button CurrentButton
        {
            set
            {
                if(_prevButton != null) _prevButton.FontWeight = FontWeights.Normal;
                value.FontWeight = FontWeights.Bold;
                _prevButton = value;
            }
        }

        public HelpWindow()
        {
            InitializeComponent();
            this.AttachCustomSystemMenu();
            _prevButton = null;

            _helpArticles = new SimpleTrie<string>
            {
                ["General"] = Application.GetResource("LocalizationGeneralDescription"),
                ["Using"] = Application.GetResource("LocalizationUsingDescription"),
                ["Differences"] = Application.GetResource("LocalizationDifferencesDescription"),
                ["Compilation"] = Application.GetResource("LocalizationCompilationDescription"),
                ["WarningMessage"] = Application.GetResource("LocalizationWarningMessageDescription")
            };
            GeneralButtonClick(GeneralButton, null);
        }

        private void OkButtonClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void UseButtonClick(object sender, RoutedEventArgs e)
        {
            CurrentButton = (Button)sender;
            HelpTextBlock.SetHtml(_helpArticles["Using"]);
        }

        private void DifferencesButtonClick(object sender, RoutedEventArgs e)
        {
            CurrentButton = (Button)sender;
            HelpTextBlock.SetHtml(_helpArticles["Differences"]);
        }

        private void CompilationButtonClick(object sender, RoutedEventArgs e)
        {
            CurrentButton = (Button)sender;
            HelpTextBlock.SetHtml(_helpArticles["Compilation"]);
        }

        private void WarningMessageButtonClick(object sender, RoutedEventArgs e)
        {
            CurrentButton = (Button)sender;
            HelpTextBlock.SetHtml(_helpArticles["WarningMessage"]);
        }

        private void GeneralButtonClick(object sender, RoutedEventArgs e)
        {
            CurrentButton = (Button)sender;
            HelpTextBlock.SetHtml(_helpArticles["General"]);
        }
    }
}
