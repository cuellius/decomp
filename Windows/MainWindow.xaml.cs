using System;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using Decomp.Core;
using Microsoft.Win32;

namespace Decomp.Windows
{
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            this.AttachCustomSystemMenu();

            Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo(1033);
            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(1033);
            
            CurrentLanguageLabel.Content = GetCurrentLanguage() == "English" ? "Eng" : "Рус";

            Loaded += (s, e) => InitUserInterface();
            Closing += (s, e) => SaveUserInterface();

            Closing += (s, e) =>
            {
                _aboutWindow?.Close();
                _helpWindow?.Close();
                if (Decompiler.Alive) Decompiler.StopDecompilation();
                ErrorWindow.CloseCurrentErrorWindow();
            };
            Decompiler.Window = this;
        }

        public void Print(string message)
        {
            LogTextBox.Dispatcher.Invoke(() => LogTextBox.Text += message);
        }

        public void Print(string message, params object[] objects)
        {
            if (objects == null)
                Print(message);
            else
                LogTextBox.Dispatcher.Invoke(() => LogTextBox.Text += String.Format(message, objects));
        }

        private void DecompileButtonClick(object sender, RoutedEventArgs e)
        {
            if (Decompiler.Alive)
            {
                DecompileButton.Content = Application.GetResource("LocalizationDecompile");
                StatusTextBlock.Text = "";
                Decompiler.StopDecompilation();
            }
            else
            {
                LogTextBox.Text = "";
                if (SourcePathTextBox.Text.Trim() == "") SourcePathTextBox.Text = Application.StartupPath;
                if (OutputPathTextBox.Text.Trim() == "") OutputPathTextBox.Text = Application.StartupPath;

                DecompileButton.Content = Application.GetResource("LocalizationStop");
                StatusTextBlock.Text = Application.GetResource("LocalizationDecompilation");
                Decompiler.StartDecompilation();
            }
        }

        private void CloseButtonClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void BrowseSoursePathButtonClick(object sender, RoutedEventArgs e)
        {
            var folderBrowseDialog = new OpenFileOrFolderDialog
            {
                Title = Application.GetResource("LocalizationChooseInput"),
                ShowNewFolderButton = false,
                Path = SourcePathTextBox.Text,
                AcceptFiles = true
            };

            if (folderBrowseDialog.ShowDialog() == true)
            {
                SourcePathTextBox.SetText(folderBrowseDialog.Path);
            }
        }

        private void BrowseOutputPathButtonClick(object sender, RoutedEventArgs e)
        {
            var folderBrowseDialog = new OpenFileOrFolderDialog
            {
                Title = Application.GetResource("LocalizationChooseOutput"),
                AcceptFiles = false,
                ShowNewFolderButton = true,
                Path = OutputPathTextBox.Text
            };

            if (folderBrowseDialog.ShowDialog() == true)
            {
                OutputPathTextBox.Text = folderBrowseDialog.Path;
            }
        }

        private Window _aboutWindow;
        private void AboutButtonClick(object sender, RoutedEventArgs e)
        {
            if (_aboutWindow != null)
                _aboutWindow.Focus();
            else
            {
                _aboutWindow = new AboutWindow();
                _aboutWindow.Closed += (s, t) => _aboutWindow = null;
                _aboutWindow.Show();
            }
        }

        protected void InitUserInterface()
        {
            var key = Registry.CurrentUser.OpenSubKey("Software\\WMD");

            string source = "", output = "";

            if (Application.CommandLineArgs.Length > 0 && Directory.Exists(Application.CommandLineArgs[0]))
            {
                source = Application.CommandLineArgs[0];
                output = Application.CommandLineArgs[0];
            }
            else if (Application.CommandLineArgs.Length > 0 && File.Exists(Application.CommandLineArgs[0]))
            {
                source = Application.CommandLineArgs[0];
                output = Path.GetDirectoryName(Application.CommandLineArgs[0]);
            }
            else
            {
                if (key == null)
                    Registry.CurrentUser.OpenSubKey("Software", true)?.CreateSubKey("WMD");
                else
                {
                    source = key.GetValue("LastSource") as string;
                    output = key.GetValue("LastOutput") as string;
                }
            }

            if (source == null) source = "";
            if (output == null) output = "";

            if (source.Length > 0)
                if (source[source.Length - 1] == '\\')
                    source = source.Remove(source.Length - 1, 1);
            if (output.Length > 0)
                if (output[output.Length - 1] == '\\')
                    output = output.Remove(output.Length - 1, 1);

            SourcePathTextBox.Text = source;
            OutputPathTextBox.Text = output;
            
            OpenAfterCompleteCheckBox.IsChecked = key != null && ((int?)key.GetValue("OpenAfterComplete") ?? 1) != 0;
            ModeComboBox.SelectedIndex = (int?)key?.GetValue("Mode") ?? 2;

            DecompileShadersCheckBox.IsChecked = key != null && ((int?)key.GetValue("DecompileShaders") ?? 0) != 0;
            GenerateIdFilesCheckBox.IsChecked = key != null && ((int?)key.GetValue("MakeID") ?? 0) != 0;
        }

        protected void SaveUserInterface()
        {
            using (var key = Registry.CurrentUser.OpenSubKey("Software\\WMD", true))
            {
                key?.SetValue("LastSource", File.Exists(SourcePathTextBox.Text) ? SourcePathTextBox.Text : SourcePathTextBox.Text + "\\");
                key?.SetValue("LastOutput", OutputPathTextBox.Text + "\\");
                key?.SetValue("OpenAfterComplete", OpenAfterCompleteCheckBox.IsChecked(), RegistryValueKind.DWord);
                key?.SetValue("Mode", ModeComboBox.SelectedIndex, RegistryValueKind.DWord);
                key?.SetValue("DecompileShaders", DecompileShadersCheckBox.IsChecked == true ? 1 : 0, RegistryValueKind.DWord);
                key?.SetValue("MakeID", GenerateIdFilesCheckBox.IsChecked == true ? 1 : 0, RegistryValueKind.DWord);
            }
        }

        private Window _helpWindow;
        private void HelpCommandExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (_helpWindow != null)
                _helpWindow.Focus();
            else
            {
                _helpWindow = new HelpWindow();
                _helpWindow.Closed += (s, t) => _helpWindow = null;
                _helpWindow.Show();
            }
        }

        private void HelpCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private static string GetCurrentLanguage()
        {
            var key = Registry.CurrentUser.OpenSubKey("Software\\WMD", true);
            if (key == null)
            {
                var t = Registry.CurrentUser.OpenSubKey("Software", true)?.CreateSubKey("WMD");
                t?.SetValue("Language", "English");
                return "English";
            }
            var language = key.GetValue("Language") as string;
            if (language == "Russian" || language == "English") return language;
            key.SetValue("Language", "English");
            return "English";
        }

        private void LanguageButtonClick(object sender, RoutedEventArgs e)
        {
            var curLang = GetCurrentLanguage();
            var newLang = curLang == "Russian" ? "English" : "Russian";

            var key = Registry.CurrentUser.OpenSubKey("Software\\WMD", true);
            key?.SetValue("Language", newLang);

            CurrentLanguageLabel.Content = newLang == "English" ? "Eng" : "Рус";
            Application.Language = newLang;
        }
    }
}
