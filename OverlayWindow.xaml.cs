using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Forms = System.Windows.Forms;

namespace StreamLighter
{
    public partial class OverlayWindow : Window
    {
        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_TRANSPARENT = 0x00000020;
        private const int WS_EX_TOOLWINDOW = 0x00000080;

        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        private EllipseGeometry? holeGeometry;
        private double holeRadius = 120; // radius of transparent area around cursor

        private bool fillTop = true;
        private bool fillBottom = true;
        private bool fillLeft = true;
        private bool fillRight = true;
        private double fillThickness = 200;

        public OverlayWindow()
        {
            InitializeComponent();
            CompositionTarget.Rendering += CompositionTarget_Rendering;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // If size/position were already set by caller (e.g. MainWindow.ApplySelectedMonitorToOverlay), do not override them.
            bool sizeUnset = double.IsNaN(Width) || double.IsNaN(Height) || Width <= 0 || Height <= 0;
            bool posUnset = double.IsNaN(Left) || double.IsNaN(Top);

            if (sizeUnset || posUnset)
            {
                // Restrict overlay to the selected monitor bounds (default: primary)
                var screen = Forms.Screen.PrimaryScreen;
                var bounds = screen.Bounds;
                Left = bounds.X;
                Top = bounds.Y;
                Width = bounds.Width;
                Height = bounds.Height;
            }

            // Initialize hole geometry and mask regardless of whether we set bounds here
            holeGeometry = new EllipseGeometry(new System.Windows.Point(-1000, -1000), holeRadius, holeRadius);
            UpdateMaskPath();
        }


        private System.Windows.Point lastHoleCenter = new System.Windows.Point(-1000, -1000);

        private void CompositionTarget_Rendering(object? sender, EventArgs e)
        {
            // Poll global cursor position and show hole only when it's over the lit frame area
            var cursor = Forms.Cursor.Position;
            double localX = cursor.X - Left;
            double localY = cursor.Y - Top;

            bool onFrame = IsPointOnFrame(localX, localY);

            if (holeGeometry == null) return;

            if (onFrame)
            {
                var newCenter = new System.Windows.Point(localX, localY);
                if (newCenter != lastHoleCenter)
                {
                    holeGeometry.Center = newCenter;
                    lastHoleCenter = newCenter;
                    UpdateMaskPath();
                }
            }
            else
            {
                // hide hole
                if (lastHoleCenter.X != -1000 || lastHoleCenter.Y != -1000)
                {
                    holeGeometry.Center = new System.Windows.Point(-1000, -1000);
                    lastHoleCenter = new System.Windows.Point(-1000, -1000);
                    UpdateMaskPath();
                }
            }
        }

        private bool IsPointOnFrame(double x, double y)
        {
            if (x < 0 || y < 0 || x > Width || y > Height) return false;

            double leftInset = fillLeft ? fillThickness : 0;
            double topInset = fillTop ? fillThickness : 0;
            double rightInset = fillRight ? fillThickness : 0;
            double bottomInset = fillBottom ? fillThickness : 0;

            double innerX = leftInset;
            double innerY = topInset;
            double innerW = Math.Max(0, Width - leftInset - rightInset);
            double innerH = Math.Max(0, Height - topInset - bottomInset);

            // On frame if inside outer rect and NOT inside inner rect (when inner exists)
            if (innerW <= 0 || innerH <= 0)
            {
                // frame fills entire screen
                return true;
            }

            if (x >= innerX && x <= innerX + innerW && y >= innerY && y <= innerY + innerH)
                return false;

            return true;
        }

        

        private void UpdateMaskPath()
        {
            if (MaskPath == null || holeGeometry == null) return;
            var outer = new RectangleGeometry(new System.Windows.Rect(0, 0, Width, Height));

            double leftInset = fillLeft ? fillThickness : 0;
            double topInset = fillTop ? fillThickness : 0;
            double rightInset = fillRight ? fillThickness : 0;
            double bottomInset = fillBottom ? fillThickness : 0;

            double innerX = leftInset;
            double innerY = topInset;
            double innerW = Math.Max(0, Width - leftInset - rightInset);
            double innerH = Math.Max(0, Height - topInset - bottomInset);

            Geometry frame;
            if (innerW <= 0 || innerH <= 0)
            {
                // if inner rect collapses, just fill outer
                frame = outer;
            }
            else
            {
                var inner = new RectangleGeometry(new System.Windows.Rect(innerX, innerY, innerW, innerH));
                frame = Geometry.Combine(outer, inner, GeometryCombineMode.Exclude, null);
            }

            var combined = Geometry.Combine(frame, holeGeometry, GeometryCombineMode.Exclude, null);
            MaskPath.Data = combined;
        }

        public void UpdateFill(bool top, bool bottom, bool left, bool right, double thickness)
        {
            fillTop = top;
            fillBottom = bottom;
            fillLeft = left;
            fillRight = right;
            fillThickness = thickness;
            UpdateMaskPath();
        }

        private void Window_SourceInitialized(object sender, EventArgs e)
        {
            var hwnd = new WindowInteropHelper(this).Handle;
            var exStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
            SetWindowLong(hwnd, GWL_EXSTYLE, exStyle | WS_EX_TRANSPARENT | WS_EX_TOOLWINDOW);
        }
    }
}
