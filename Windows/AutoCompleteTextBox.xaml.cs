using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using FILETIME = System.Runtime.InteropServices.ComTypes.FILETIME;

namespace Decomp.Windows
{
    public partial class AutoCompleteTextBox
    {
        // ReSharper disable InconsistentNaming
        private const int MAX_PATH = 260;

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        private struct WIN32_FIND_DATA
        {
            public readonly FileAttributes dwFileAttributes;
            private readonly FILETIME ftCreationTime;
            private readonly FILETIME ftLastAccessTime;
            private readonly FILETIME ftLastWriteTime;
            private readonly uint nFileSizeHigh;
            private readonly uint nFileSizeLow;
            private readonly uint dwReserved0;
            private readonly uint dwReserved1;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_PATH)]
            public readonly string cFileName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 14)]
            private readonly string cAlternateFileName;
        }

        [DllImport("Kernel32.dll", EntryPoint = "FindFirstFileW", CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = true)]
        private static extern IntPtr FindFirstFile(string lpFileName, out WIN32_FIND_DATA lpFindFileData);

        [DllImport("Kernel32.dll", EntryPoint = "FindNextFileW", CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool FindNextFile(IntPtr hFindFile, out WIN32_FIND_DATA lpFindFileData);

        [DllImport("Kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool FindClose(IntPtr hFindFile);

        private readonly IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);

        private const int WM_NCLBUTTONDOWN = 0x00A1;
        private const int WM_NCRBUTTONDOWN = 0x00A4;
        private const int WM_LBUTTONUP = 0x0202;
        private const int WM_RBUTTONUP = 0x0205;
        // ReSharper restore InconsistentNaming

        private IntPtr WndProc(IntPtr hWnd, int iMsg, IntPtr wParam, IntPtr lParam, ref bool bHandled)
        {
            switch (iMsg)
            {
                case WM_NCRBUTTONDOWN:
                case WM_NCLBUTTONDOWN:
                    Popup.IsOpen = false;
                    ItemsListBox.ItemsSource = null;
                    break;
                case WM_RBUTTONUP:
                case WM_LBUTTONUP:
                    Popup.IsOpen = false;
                    if (ItemsListBox.ItemsSource != null && ItemsListBox.SelectedIndex != -1) Text = ItemsListBox.SelectedItem.ToString();
                    break;
            }
            return IntPtr.Zero;
        }

        public IEnumerable<string> GetItems(string textPattern)
        {
            if (textPattern == null) throw new ArgumentNullException(nameof(textPattern));

            if (textPattern.Length < 2 || textPattern[1] != ':') yield break;
            var lastSlashPos = textPattern.LastIndexOf('\\');
            if (lastSlashPos == -1) yield break;
            var fileNamePatternLength = textPattern.Length - lastSlashPos - 1;
            var baseFolder = textPattern.Substring(0, lastSlashPos + 1);
            var hFind = FindFirstFile(textPattern + "*", out var fd);
            if (hFind == INVALID_HANDLE_VALUE) yield break;
            do
            {
                if (fd.cFileName[0] == '.') continue;
                if ((fd.dwFileAttributes & FileAttributes.Hidden) != 0) continue;
                if (fileNamePatternLength > fd.cFileName.Length) continue;
                yield return baseFolder + fd.cFileName;
            } while (FindNextFile(hFind, out fd));
            FindClose(hFind);
        }

        private bool _loaded;
        private bool _prevState;
        private ListBox ItemsListBox => Template.FindName("PART_ItemList", this) as ListBox;
        private Popup Popup => Template.FindName("PART_Popup", this) as Popup;
        private Grid Root => Template.FindName("root", this) as Grid;

        public AutoCompleteTextBox() => InitializeComponent();

        private Window GetParentWindow()
        {
            DependencyObject d = this;
            while (d != null && !(d is Window)) d = LogicalTreeHelper.GetParent(d);
            return d as Window;
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _loaded = true;

            KeyDown += AutoCompleteTextBoxKeyDown;
            KeyUp += AutoCompleteTextBoxKeyUp;
            PreviewKeyDown += AutoCompleteTextBoxPreviewKeyDown;
            ItemsListBox.PreviewMouseDown += ItemsListBoxPreviewMouseDown;
            ItemsListBox.KeyDown += ItemsListBoxKeyDown;
            Popup.CustomPopupPlacementCallback += Repositioning;

            var parentWindow = GetParentWindow();
            if (parentWindow == null) return;
            parentWindow.Deactivated += (s, e) => { _prevState = Popup.IsOpen; Popup.IsOpen = false; };
            parentWindow.Activated += (s, e) => Popup.IsOpen = _prevState;

            var source = PresentationSource.FromVisual(parentWindow) as HwndSource;
            source?.AddHook(WndProc);
        }

        private CustomPopupPlacement[] Repositioning(Size popupSize, Size targetSize, Point offset) => new[] { new CustomPopupPlacement(new Point(0.01 - offset.X, Root.ActualHeight - offset.Y), PopupPrimaryAxis.None) };

        protected override void OnTextInput(TextCompositionEventArgs e)
        {
            base.OnTextInput(e);
            if (!_loaded) return;
            try
            {
                var aVariants = GetItems(Text).ToList();
                ItemsListBox.ItemsSource = aVariants;
                Popup.IsOpen = ItemsListBox.Items.Count > 0;
            }
            catch
            {
                // ignored
            }
        }

        private void AutoCompleteTextBoxKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;
            Popup.IsOpen = false;
            UpdateSource();
        }

        private void AutoCompleteTextBoxKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape && Popup.IsOpen) Popup.IsOpen = false; 
        }

        private void AutoCompleteTextBoxPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (ItemsListBox.Items.Count <= 0 || e.OriginalSource is ListBoxItem) return;
            switch (e.Key)
            {
                case Key.Up:
                case Key.Down:
                case Key.Prior:
                case Key.Next:
                    ItemsListBox.Focus();
                    ItemsListBox.SelectedIndex = 0;
                    var lbi = (ListBoxItem)ItemsListBox.ItemContainerGenerator.ContainerFromIndex(ItemsListBox.SelectedIndex);
                    lbi.Focus();
                    e.Handled = true;
                    break;

            }
        }

        private void ItemsListBoxKeyDown(object sender, KeyEventArgs e)
        {
            if (!(e.OriginalSource is ListBoxItem)) return;
            var tb = (ListBoxItem)e.OriginalSource;

            e.Handled = true;
            switch (e.Key)
            {
                case Key.Enter:
                    Text = tb.Content as string;
                    UpdateSource();
                    break;
                case Key.Oem5:
                    Text = (tb.Content as string) + "\\";
                    break;
                default:
                    e.Handled = false;
                    break;
            }
            if (!e.Handled) return;

            Keyboard.Focus(this);
            Popup.IsOpen = false;
            if (Text != null) Select(Text.Length, 0);
        }

        private void ItemsListBoxPreviewMouseDown(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed) return;
            if (!(e.OriginalSource is TextBlock tb)) return;
            Text = tb.Text;
            Select(Text.Length, 0);
            UpdateSource();
            Popup.IsOpen = false;
            e.Handled = true;
        }

        private void UpdateSource()
        {
            if (GetBindingExpression(TextProperty) == null) return;
            var bindingExpression = GetBindingExpression(TextProperty);
            bindingExpression?.UpdateSource();
        }

#pragma warning disable CA1062 // Validate arguments of public methods
        protected override void OnDrop(DragEventArgs e)
        {
            base.OnDrop(e);
            var text = e.Data.GetData(DataFormats.FileDrop);
            var strings = (string[])text;
            if (strings != null) Text = $"{strings[0]}";
        }

        protected override void OnDragOver(DragEventArgs e)
        {
            base.OnDragOver(e);
            e.Effects = DragDropEffects.All;
            e.Handled = true;
        }

        protected override void OnDragEnter(DragEventArgs e)
        {
            base.OnDragEnter(e);
            e.Effects = DragDropEffects.All;
            e.Handled = true;
        }
#pragma warning restore CA1062 // Validate arguments of public methods
    }
}
