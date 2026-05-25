using System;
using System.Windows;
using Forms = System.Windows.Forms;

namespace StreamLighter
{
    public partial class MainWindow : Window
    {
        private OverlayWindow? overlay;
        private Forms.Screen[] screens = Array.Empty<Forms.Screen>();

        public MainWindow()
        {
            InitializeComponent();
            LoadMonitors();
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
