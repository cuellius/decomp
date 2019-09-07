using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Decomp.Windows
{
    public static class WindowCustomSystemMenu
    {
        // ReSharper disable once InconsistentNaming
        private static readonly Dictionary<IntPtr, Window> _dictionary = new Dictionary<IntPtr, Window>();

        public static void AttachCustomSystemMenu(this Window w)
        {
            w.Loaded += (s, e) => 
            {
                var hwnd = new WindowInteropHelper(w).Handle;
                _dictionary[hwnd] = w;
                var hwndSource = HwndSource.FromHwnd(hwnd);
                hwndSource?.AddHook(WindowProc);
            };
        }

        // ReSharper disable InconsistentNaming
        private const int WM_SYSCOMMAND = 0x112;
        private const int SC_KEYMENU = 0xF100;
        private const int SC_SIZE = 0xF000;
        private const int SC_MOVE = 0xF010;
        // ReSharper restore InconsistentNaming

        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint uMsg, IntPtr wParam, IntPtr lParam);
        
        private static IntPtr WindowProc(IntPtr hwnd, int iMsg, IntPtr wParam, IntPtr lParam, ref bool bHandled)
        {
            var w = _dictionary[hwnd];
            if (iMsg == WM_SYSCOMMAND && wParam == (IntPtr)SC_KEYMENU)
            {
                if (!(System.Windows.Application.Current.Resources["SystemMenu"] is ContextMenu menu)) return IntPtr.Zero;
                ((MenuItem)menu.Items[0]).Click += (s, e) => w.WindowState = WindowState.Normal;
                ((MenuItem)menu.Items[0]).IsEnabled = w.WindowState != WindowState.Normal;
                ((Path)((Canvas)((MenuItem)menu.Items[0]).Icon).Children[0]).Stroke = ((MenuItem)menu.Items[0]).IsEnabled ? Brushes.Black : Brushes.Gray;
                ((MenuItem)menu.Items[1]).Click += (s, e) => SendMessage(hwnd, WM_SYSCOMMAND, (IntPtr)SC_MOVE, IntPtr.Zero);
                ((MenuItem)menu.Items[1]).IsEnabled = w.WindowState != WindowState.Maximized;
                ((MenuItem)menu.Items[2]).Click += (s, e) => SendMessage(hwnd, WM_SYSCOMMAND, (IntPtr)SC_SIZE, IntPtr.Zero);
                ((MenuItem)menu.Items[2]).IsEnabled = w.WindowState != WindowState.Maximized;
                ((MenuItem)menu.Items[3]).Click += (s, e) => w.WindowState = WindowState.Minimized;
                ((MenuItem)menu.Items[3]).IsEnabled = w.WindowState != WindowState.Minimized;
                ((MenuItem)menu.Items[4]).Click += (s, e) => w.WindowState = WindowState.Maximized;
                ((MenuItem)menu.Items[4]).IsEnabled = w.WindowState != WindowState.Maximized && w.ResizeMode != ResizeMode.NoResize && w.ResizeMode != ResizeMode.CanMinimize;
                ((MenuItem)menu.Items[6]).Click += (s, e) => w.Close();
                menu.Placement = PlacementMode.Relative;
                menu.PlacementTarget = w;
                menu.HorizontalOffset = 7;
                menu.VerticalOffset = 32;
                menu.StaysOpen = true;
                new Thread(() =>
                {
                    Thread.Sleep(10);
                    menu.Dispatcher?.Invoke(() => menu.IsOpen = true);
                }).Start();
                //menu.

                bHandled = true;
            }

            return IntPtr.Zero;
        }
    }
}
