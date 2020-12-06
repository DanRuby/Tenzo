using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using tEngine.DataModel;
using tEngine.MVVM;
using tEngine.UControls;

namespace TenzoMeterGUI.View {
    /// <summary>
    ///     Interaction logic for plottest.xaml
    /// </summary>
    public partial class plottest : Window {
        public plottest() {
            InitializeComponent();
        }

        private void PlotViewEx2_OnLoaded( object sender, RoutedEventArgs e ) {
            PlotViewEx2.Clear();
            var data = Enumerable.Range( 0, 100 ).Select( i => new DataPoint( i, Math.Sqrt( i ) ) ).ToList();
            PlotViewEx2.AddLineSeries( data, color: null, thickness: 2 );
            PlotViewEx2.ReDraw();
        }
    }

    public class plottestVM : Observed<plottestVM> {
        private TData mData = new TData();
        private ObservableCollection<DataPoint> mMeasurements;
        private bool mShowPlot;
        public Command CMDButton { get; private set; }

        public TData Data {
            get { return mData; }
            set {
                mData = value;
                NotifyPropertyChanged( m => m.Data );
            }
        }

        public ObservableCollection<DataPoint> Measurements {
            get { return mMeasurements; }
            set {
                mMeasurements = value;
                NotifyPropertyChanged( m => m.Measurements );
            }
        }

        public bool ShowPlot {
            get { return mShowPlot; }
            set {
                mShowPlot = value;
                NotifyPropertyChanged( m => m.ShowPlot );
            }
        }

        public plottestVM() {
            CMDButton = new Command( CMDButton_Func );

            mMeasurements = new ObservableCollection<DataPoint>();
            const int N = 500;

            for( var i = 0; i < N; i++ ) {
                Measurements.Add( new DataPoint( i - 100, i - 100 ) );
            }
            ShowPlot = true;

        }

        private void CMDButton_Func( object param ) {

            
            // тестовая TData
            var sz = 4000;
            var hand1 = new Hand();         
            var hand2 = new Hand();

            var cs1 = Enumerable.Range( 0, sz ).Select( v => (short) (100*Math.Sin( v*Math.PI/180 )) ).ToList();
            var tr1 = Enumerable.Range( 0, sz ).Select( v => (short) (10*Math.Sin( v*Math.PI/180 )) ).ToList();
            var cs2 = Enumerable.Range( 0, sz ).Select( v => (short) (100*Math.Sin( v*Math.PI/180 + Math.PI/2 )) ).ToList();
            var tr2 = Enumerable.Range( 0, sz ).Select( v => (short) (10*Math.Sin( v*Math.PI/180 + Math.PI/2 )) ).ToList();

            hand1.Const = cs1;
            hand1.Tremor = tr1;
            hand2.Const = cs2;
            hand2.Tremor = tr2;
            //hand1.Const = Enumerable.Range( 0, sz ).Select( v => (short) v ).ToList();
            //hand1.Tremor = Enumerable.Range( 0, sz ).Select( v => (short) (v/2) ).ToList();
            //hand2.Const = Enumerable.Range( 0, sz ).Select( v => (short) (100 - v) ).ToList();
            //hand2.Tremor = Enumerable.Range( 0, sz ).Select( v => (short) ((100 - v)/2) ).ToList();
            var dt = new TData();
            dt.AddHands( hand1, hand2 );
            Data = dt;

            //var start = Measurements.Count;
            //for( var i = 0; i < 100; i++ ) {
            //    Measurements.Add( new DataPoint( start + i, i ) );
            //    NotifyPropertyChanged( m => m.Measurements );
            //}
            //(param as PlotViewEx).Invalidate();
        }
    }
}