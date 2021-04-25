using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace TenzoMeterGUI.Helpers
{
    public class CustomGrid : Grid
    {
        protected override void OnRender(DrawingContext dc)
        {
            if (ShowCustomGridLines)
            {
                foreach (RowDefinition rowDefinition in RowDefinitions)
                {
                    dc.DrawLine(new Pen(GridLineBrush, GridLineThickness), new Point(0, rowDefinition.Offset),
                        new Point(ActualWidth, rowDefinition.Offset));
                }

                foreach (ColumnDefinition columnDefinition in ColumnDefinitions)
                {
                    dc.DrawLine(new Pen(GridLineBrush, GridLineThickness), new Point(columnDefinition.Offset, 0),
                        new Point(columnDefinition.Offset, ActualHeight));
                }
                dc.DrawRectangle(Brushes.Transparent, new Pen(GridLineBrush, GridLineThickness),
                    new Rect(0, 0, ActualWidth, ActualHeight));
            }
            base.OnRender(dc);
        }

        static CustomGrid()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CustomGrid),
                new FrameworkPropertyMetadata(typeof(CustomGrid)));
        }

        #region Properties

        public bool ShowCustomGridLines
        {
            get => (bool)GetValue(ShowCustomGridLinesProperty);
            set => SetValue(ShowCustomGridLinesProperty, value);
        }

        public static readonly DependencyProperty ShowCustomGridLinesProperty =
            DependencyProperty.Register("ShowCustomGridLines", typeof(bool), typeof(CustomGrid),
                new UIPropertyMetadata(false));

        public Brush GridLineBrush
        {
            get => (Brush)GetValue(GridLineBrushProperty);
            set => SetValue(GridLineBrushProperty, value);
        }

        public static readonly DependencyProperty GridLineBrushProperty =
            DependencyProperty.Register("GridLineBrush", typeof(Brush), typeof(CustomGrid),
                new UIPropertyMetadata(Brushes.Black));

        public double GridLineThickness
        {
            get => (double)GetValue(GridLineThicknessProperty);
            set => SetValue(GridLineThicknessProperty, value);
        }

        public static readonly DependencyProperty GridLineThicknessProperty =
            DependencyProperty.Register("GridLineThickness", typeof(double), typeof(CustomGrid),
                new UIPropertyMetadata(1.0));

        #endregion
    }
}