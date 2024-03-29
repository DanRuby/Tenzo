﻿using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using tEngine.MVVM;

namespace tEngine.Markers
{
    /// <summary>
    /// Interaction logic for UIMarkers.xaml
    /// </summary>
    public partial class UIMarkers : UserControl
    {
        private MArea mArea = new MArea();

        public int Maximum => mArea.Maximum;

        public int Minimum => mArea.Minimum;

        public UIMarkers()
        {
            InitializeComponent();
            Init();
        }

        public void RedrawEverything(int left, int right)
        {
            mArea.RedrawEverything(left, right);
        }

        public void DrawMarkers(int left, int right)
        {
            mArea.RedrawMarkers(left, right);
        }

        public void Init() { }

        public void UpdateArea()
        {
            double width = Bd.ActualWidth;
            double height = Bd.ActualHeight;

            width = width <= 1 ? 1 : width;
            height = height <= 1 ? 1 : height;

            if (width > 0 && height > 0)
            {
                if (!Designer.IsDesignMode)
                    mArea.UpdateSettings();
                WriteableBitmap wb = new WriteableBitmap((int)width, (int)height, 96d, 96d, PixelFormats.Bgr24, null);
                mArea.UpdateArea(wb);
                AreaContainer.Source = wb;
            }
        }

        private void Area_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateArea();
        }
    }
}