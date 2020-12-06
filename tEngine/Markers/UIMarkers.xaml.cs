using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using tEngine.MVVM;

namespace tEngine.Markers {
    /// <summary>
    /// Interaction logic for UIMarkers.xaml
    /// </summary>
    public partial class UIMarkers : UserControl {
        private MArea mArea = new MArea();

        public int Maximum {
            get { return mArea.Maximum; }
        }

        public int Minimum {
            get { return mArea.Minimum; }
        }

        public UIMarkers() {
            InitializeComponent();
            Init();
        }

        public void DrawAll( int left, int right ) {
            mArea.DrawAll( left, right );
        }

        public void DrawPart( int left, int right ) {
            mArea.DrawPart( left, right );
        }

        public void Init() {}

        public void UpdateArea() {
            var width = Bd.ActualWidth;
            var height = Bd.ActualHeight;

            width = width <= 1 ? 1 : width;
            height = height <= 1 ? 1 : height;

            if( width > 0 && height > 0 ) {
                if( !Designer.IsDesignMode )
                    mArea.UpdateSettings();
                var wb = new WriteableBitmap( (int) width, (int) height, 96d, 96d, PixelFormats.Bgr24, null );
                mArea.UpdateArea( wb );
                AreaContainer.Source = wb;
            }
        }

        private void Area_OnSizeChanged( object sender, SizeChangedEventArgs e ) {
            UpdateArea();
        }
    }
}