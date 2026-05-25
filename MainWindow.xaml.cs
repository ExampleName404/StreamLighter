using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Drawing;
using System.Drawing.Drawing2D;
using Forms = System.Windows.Forms;

namespace StreamLighter
{
    public partial class MainWindow : Window
    {
        private OverlayWindow? overlay;
        private Forms.Screen[] screens = Array.Empty<Forms.Screen>();
        private Forms.NotifyIcon? trayIcon;
        private System.Drawing.Icon? appIcon;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern bool DestroyIcon(IntPtr handle);

        public MainWindow()
        {
            InitializeComponent();
            LoadMonitors();
            InitializeTrayIcon();
            Closing += MainWindow_Closing;
        }

        private void InitializeTrayIcon()
        {
            trayIcon = new Forms.NotifyIcon();
            trayIcon.Text = "StreamLighter — by NotUnrealEngineer";
            // create a simple programmatic icon (circle) to use in tray and window
            try
            {
                using var bmp = new System.Drawing.Bitmap(64, 64);
                using (var g = System.Drawing.Graphics.FromImage(bmp))
                {
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    g.Clear(System.Drawing.Color.Transparent);
                    var rect = new System.Drawing.Rectangle(4, 4, 56, 56);
                    using var brush = new LinearGradientBrush(rect, System.Drawing.Color.FromArgb(255, 255, 200, 0), System.Drawing.Color.FromArgb(255, 255, 120, 0), 45f);
                    g.FillEllipse(brush, rect);
                    using var pen = new System.Drawing.Pen(System.Drawing.Color.FromArgb(200, 80, 40, 0), 3);
                    g.DrawEllipse(pen, rect);
                }
                var hIcon = bmp.GetHicon();
                appIcon = System.Drawing.Icon.FromHandle(hIcon);
                trayIcon.Icon = appIcon;

                // set WPF window icon from hIcon
                var bitmapSource = Imaging.CreateBitmapSourceFromHIcon(hIcon, Int32Rect.Empty, BitmapSizeOptions.FromWidthAndHeight(32, 32));
                this.Icon = bitmapSource;
            }
            catch
            {
                trayIcon.Icon = System.Drawing.SystemIcons.Application;
            }

            // keep icon always visible in tray
            trayIcon.Visible = true;

            var menu = new Forms.ContextMenuStrip();
            var openItem = new Forms.ToolStripMenuItem("Open");
            openItem.Click += (s, e) => ShowFromTray();
            var exitItem = new Forms.ToolStripMenuItem("Exit");
            exitItem.Click += (s, e) => ExitFromTray();
            menu.Items.Add(openItem);
            menu.Items.Add(exitItem);
            trayIcon.ContextMenuStrip = menu;

            trayIcon.DoubleClick += (s, e) => ShowFromTray();
        }

        private void LoadMonitors()
        {
            screens = Forms.Screen.AllScreens;
            ComboMonitors.Items.Clear();
            for (int i = 0; i < screens.Length; i++)
            {
                var s = screens[i];
                ComboMonitors.Items.Add($"{i}: {s.DeviceName} ({s.Bounds.Width}x{s.Bounds.Height})");
            }
            ComboMonitors.SelectedIndex = 0;
        }

        private void ToggleOverlay_Click(object sender, RoutedEventArgs e)
        {
            if (overlay == null)
            {
                overlay = new OverlayWindow();
                overlay.Opacity = SliderBrightness.Value;
                // position overlay on selected monitor
                ApplySelectedMonitorToOverlay();
                overlay.Show();
                ToggleOverlay.Content = "On";
                UpdateOverlayFillFromUI();
            }
            else
            {
                overlay.Close();
                overlay = null;
                ToggleOverlay.Content = "Off";
            }
        }

        private void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            // hide to tray instead of exiting
            e.Cancel = true;
            HideToTray();
        }

        private void HideToTray()
        {
            Hide();
            if (trayIcon != null)
            {
                trayIcon.Visible = true;
                // Do not show balloon tip when hiding to tray
            }
        }

        private void ShowFromTray()
        {
            Dispatcher.Invoke(() =>
            {
                Show();
                WindowState = WindowState.Normal;
                Activate();
            });
        }

        private void ExitFromTray()
        {
            // cleanup and shutdown
            if (trayIcon != null)
            {
                trayIcon.Visible = false;
                trayIcon.Dispose();
                trayIcon = null;
            }
            // dispose created icon
            if (appIcon != null)
            {
                var handle = appIcon.Handle;
                appIcon.Dispose();
                appIcon = null;
                DestroyIcon(handle);
            }
            Application.Current.Shutdown();
        }

        private void SliderBrightness_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (overlay != null)
            {
                overlay.Opacity = e.NewValue;
            }
        }

        private void FillOption_Changed(object sender, RoutedEventArgs e)
        {
            UpdateOverlayFillFromUI();
        }

        private void SliderThickness_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            UpdateOverlayFillFromUI();
        }

        private void UpdateOverlayFillFromUI()
        {
            if (overlay == null) return;
            var top = ChkTop.IsChecked == true;
            var bottom = ChkBottom.IsChecked == true;
            var left = ChkLeft.IsChecked == true;
            var right = ChkRight.IsChecked == true;
            var thickness = SliderThickness.Value;
            overlay.UpdateFill(top, bottom, left, right, thickness);
        }

        private void ComboMonitors_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (overlay != null)
            {
                ApplySelectedMonitorToOverlay();
            }
        }

        private void ApplySelectedMonitorToOverlay()
        {
            if (overlay == null) return;
            var idx = ComboMonitors.SelectedIndex;
            if (idx < 0 || idx >= screens.Length) idx = 0;
            var s = screens[idx];
            var b = s.Bounds;
            overlay.Left = b.X;
            overlay.Top = b.Y;
            overlay.Width = b.Width;
            overlay.Height = b.Height;
            overlay.Dispatcher.Invoke(() => overlay.UpdateLayout());
            UpdateOverlayFillFromUI();
        }
    }
}
