using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;
using OxyPlot;
using OxyPlot.Wpf;
using tEngine.DataModel;
using tEngine.Helpers;
using tEngine.MVVM;
using tEngine.PlotCreator;
using tEngine.TActual.DataModel;
using TenzoActualGUI.View;

namespace TenzoActualGUI.ViewModel {
    /// <summary>
    /// оперирует объектом PlayList
    /// </summary>
    public class SlidesResultVM : Observed<SlidesResultVM> {
        private const int MINIMUM_POINTS = 100;
        private Dictionary<object, PlotView> mGraphics = new Dictionary<object, PlotView>();
        private int mHzLower;
        private int mHzUpper;
        private bool mIsResize;
        private PlotModel mPMSpectrum_Left;
        private PlotModel mPMSpectrum_Right;
        private PlotModel mPMTremor_Left;
        private PlotModel mPMTremor_Right;
        private int mResolution;
        private Slide mSelectedItem;
        private int mSelectedTabIndex;
        private Slide mSlideToShow;

        public Collection<Series> AllLeftSeries {
            get { throw new NotImplementedException(); }
        }

        public Command CMDLVItemClick { get; private set; }
        public Command CMDOpenResultWindow { get; private set; }
        public Command CMDOxyLoad { get; private set; }
        public Command CMDOxyUnload { get; private set; }
        public Command CMDResetGraphAxes { get; private set; }
        public Command CMDSetShow { get; private set; }
        public Command CMDShowOnlyOne { get; private set; }
        public Command CMDUpdateGraphFull { get; private set; }

        public int HzLower {
            get { return mHzLower; }
            set {
                mHzLower = value;
                if( mHzLower >= MaximumHz )
                    mHzLower -= 1;
                NotifyPropertyChanged( m => m.HzLower );
                AppSettings.SetValue( "HzLower", mHzLower );
            }
        }

        public int HzUpper {
            get { return mHzUpper; }
            set {
                mHzUpper = value;
                if( mHzUpper <= MinimumHz )
                    mHzUpper += 1;
                NotifyPropertyChanged( m => m.HzUpper );
                AppSettings.SetValue( "HzUpper", mHzUpper );
            }
        }

        public bool IsNotResize {
            get { return !IsResize; }
            set { IsResize = !value; }
        }

        public bool IsResize {
            get { return mIsResize; }
            set {
                mIsResize = value;
                if( mIsResize == false )
                    UpdateGraphFull();
            }
        }

        public bool? IsShowAll {
            get {
                var show = PlayList.Slides.Count( slide => slide.IsShow == true );
                var hide = PlayList.Slides.Count - show;
                if( hide == 0 )
                    return true;
                if( show == 0 )
                    return false;
                return null;
            }
            set {
                foreach( var slide in PlayList.Slides ) {
                    slide.IsShow = value ?? false;
                }
                NotifyPropertyChanged( m => m.Slides );
            }
        }

        public int MaximumHz {
            get { return 500; }
        }

        public int MinimumHz {
            get { return 0; }
        }

        public PlayList PlayList { get; set; }

        public PlotModel PMSpectrum_Left {
            get {
                if( IsResize ) return null;
                if( mPMSpectrum_Left != null ) return mPMSpectrum_Left;
                mPMSpectrum_Left = new PlotModel();
                mPMSpectrum_Left.CreateSpectrumAllIn( "Ћева€ рука, спектральна€ характеристика" );

                mPMSpectrum_Left.Series.Clear();
                foreach( var slide in PlayList.Slides ) {
                    if( slide.IsShow )
                        mPMSpectrum_Left.Series.SetLine(
                            slide.Data.GetSpectrumByHz( Hands.Left, HzLower, HzUpper ).GetPartPercent( Resolution ) );
                }
                mPMSpectrum_Left.AutoScale();
                mPMSpectrum_Left.InvalidatePlot( true );
                return mPMSpectrum_Left;
            }
        }

        public PlotModel PMSpectrum_Right {
            get {
                if( IsResize ) return null;
                if( mPMSpectrum_Right != null ) return mPMSpectrum_Right;
                mPMSpectrum_Right = new PlotModel();
                mPMSpectrum_Right.CreateSpectrumAllIn( "ѕрава€ рука, спектральна€ характеристика" );

                mPMSpectrum_Right.Series.Clear();
                foreach( var slide in PlayList.Slides ) {
                    if( slide.IsShow )
                        mPMSpectrum_Right.Series.SetLine(
                            slide.Data.GetSpectrumByHz( Hands.Right, HzLower, HzUpper ).GetPartPercent( Resolution ) );
                }
                mPMSpectrum_Right.AutoScale();
                mPMSpectrum_Right.InvalidatePlot( true );
                return mPMSpectrum_Right;
            }
        }

        public PlotModel PMTremor_Left {
            get {
                if( IsResize ) return null;
                if( mPMTremor_Left != null ) return mPMTremor_Left;
                mPMTremor_Left = new PlotModel();
                mPMTremor_Left.CreateSpectrumAllIn( "Ћева€ рука, тремор" );

                mPMTremor_Left.Series.Clear();
                foreach( var slide in PlayList.Slides ) {
                    if( slide.IsShow )
                        mPMTremor_Left.Series.SetLine(
                            slide.Data.GetTremor( Hands.Left ).GetPartPercent( Resolution ) );
                }
                mPMTremor_Left.AutoScale();
                mPMTremor_Left.InvalidatePlot( true );
                return mPMTremor_Left;
            }
        }

        public PlotModel PMTremor_Right {
            get {
                if( IsResize ) return null;
                if( mPMTremor_Right != null ) return mPMTremor_Right;
                mPMTremor_Right = new PlotModel();
                mPMTremor_Right.CreateSpectrumAllIn( "ѕрава€ рука, тремор" );
                mPMTremor_Right.Series.Clear();
                foreach( var slide in PlayList.Slides ) {
                    if( slide.IsShow )
                        mPMTremor_Right.Series.SetLine(
                            slide.Data.GetTremor( Hands.Right ).GetPartPercent( Resolution ) );
                }
                mPMTremor_Right.AutoScale();
                mPMTremor_Right.InvalidatePlot( true );
                return mPMTremor_Right;
            }
        }

        public bool ReadyToShow {
            get { return SlideToShow != null; }
        }

        public int Resolution {
            get { return mResolution; }
            set {
                mResolution = value;
                mResolution = mResolution < 0 ? 0 : mResolution;
                mResolution = mResolution > 100 ? 100 : mResolution;
                NotifyPropertyChanged( m => m.Resolution );
                AppSettings.SetValue( "Resolution", mResolution );
            }
        }

        public Slide SelectedItem {
            get { return mSelectedItem; }
            set {
                mSelectedItem = value;
                NotifyPropertyChanged( m => m.SelectedItem );
            }
        }

        public TabItem SelectedTab { get; set; }

        public int SelectedTabIndex {
            get { return mSelectedTabIndex; }
            set {
                mSelectedTabIndex = value;
                NotifyPropertyChanged( m => m.SelectedTabIndex );
            }
        }

        public ObservableCollection<Slide> Slides {
            get { return new ObservableCollection<Slide>( PlayList.Slides ); }
        }

        public Slide SlideToShow {
            get { return mSlideToShow; }
            set {
                mSlideToShow = value;
                NotifyPropertyChanged( m => m.SlideToShow );
            }
        }

        public SlidesResultVM() {
            IsResize = false;

            CMDOpenResultWindow = new Command( CMDOpenResultWindow_Func );
            CMDUpdateGraphFull = new Command( UpdateGraphFull );
            CMDSetShow = new Command( SetShow );
            CMDResetGraphAxes = new Command( ResetGraphAxes );
            CMDLVItemClick = new Command( LVItemClick );
            CMDOxyLoad = new Command( OxyLoad );
            CMDOxyUnload = new Command( OxyUnload );
            CMDShowOnlyOne = new Command( ShowOnlyOne );


            PlayList = new PlayList();
            HzLower = AppSettings.GetValue( "HzLower", MinimumHz );
            HzUpper = AppSettings.GetValue( "HzUpper", MaximumHz );
            Resolution = AppSettings.GetValue( "Resolution", Pixels2R( 100 ) );
        }

        /// <summary>
        /// сколько процентов от общего числа будут составл€ть "pixels" точек
        /// (относительно всех возможных графиков)
        /// </summary>
        /// <param name="pixels"></param>
        /// <returns></returns>
        public int Pixels2R( int pixels ) {
            if( PlayList.Slides.Any() == false )
                return 100;
            var max = PlayList.Slides.Max( slide => slide.Data.DataLength() );
            var min = (int) (max/10.0 > MINIMUM_POINTS ? MINIMUM_POINTS : max/10.0);
            var k = (max - min)/(100 - 1);
            var b = min - k*1.0;

            var result = (pixels - b)/k;
            return (int) result;
        }

        /// <summary>
        /// 0..100 => сколько требуетс€ точек 
        /// 100 - нарисовать все что есть
        /// 50 - нарисовать каждую вторую точку
        /// </summary>
        /// <param name="resolution">"разрешение"</param>
        /// <returns></returns>
        public int R2Pixels( int resolution ) {
            if( PlayList.Slides.Any() == false )
                return 100;
            // считать по всем графикам
            var max = PlayList.Slides.Max( slide => slide.Data.DataLength() );
            var min = (int) (max/10.0 > MINIMUM_POINTS ? MINIMUM_POINTS : max/10.0);
            var k = (max - min)/(100 - 1);
            var b = min - k*1.0;

            var result = resolution*k + b;
            return (int) result;
        }

        private void CMDOpenResultWindow_Func() {
            var wnd = new ResultWindow();
            wnd.SetMsm(new Measurement(){PlayList = PlayList});
            wnd.ShowDialog();
        }

        private void LVItemClick() {
            if( SelectedItem == null ) return;
            var first = PlayList.Slides.First( slide => slide.Id.Equals( SelectedItem.Id ) );
            if( first != null ) first.IsShow = !first.IsShow;

            NotifyPropertyChanged( m => m.Slides );
            NotifyPropertyChanged( m => m.IsShowAll );
        }

        private void OxyLoad( object param ) {
            if( param is PlotView == false ) return;
            if( SelectedTab.Tag == null ) return;
            var pv = (PlotView) param;
            if( SelectedTab != null && pv.Tag != null ) {
                if( !pv.Tag.ToString().StartsWith( SelectedTab.Tag.ToString() ) )
                    return;
            } else return;

            if( mGraphics.ContainsKey( pv.Tag ) ) {
                mGraphics.Remove( pv.Tag );
            }
            var model = new PlotModel();
            //model.CreateSpectrumAllIn( pv.Title );

            model = pv.CreateModelByView();
            model.Series.Clear();
            var toAdd = new List<OxyPlot.Series.Series>();
            var sortSlides = PlayList.Slides.OrderBy( slide => slide.IsShow );
            foreach( var slide in sortSlides ) {
                var thickness = slide.IsShow ? ((IsShowAll == true) ? 2 : 3) : 1;
                Color? color = null;
                if( !slide.IsShow )
                    color = Colors.LightGray;

                // лева€ рука
                if( pv.Tag.Equals( "left_tremor" ) ) {
                    toAdd.Add( PlotExtension.SetLineSeries(
                        slide.Data.GetTremor( Hands.Left ).GetPartPercent( Resolution ),
                        color: color,
                        thickness: thickness ) );
                } else if( pv.Tag.Equals( "left_spectrum" ) ) {
                    toAdd.Add( PlotExtension.SetLineSeries(
                        slide.Data.GetSpectrumByHz( Hands.Left, HzLower, HzUpper ).GetPartPercent( Resolution ),
                        color: color,
                        thickness: thickness ) );
                } else if( pv.Tag.Equals( "left_corr" ) ) {
                    toAdd.Add( PlotExtension.SetLineSeries(
                        slide.Data.GetCorrelation( Hands.Left ).GetPartPercent( Resolution ),
                        color: color,
                        thickness: thickness ) );
                }
                // права€ рука
                else if( pv.Tag.Equals( "right_tremor" ) ) {
                    toAdd.Add( PlotExtension.SetLineSeries(
                        slide.Data.GetTremor( Hands.Right ).GetPartPercent( Resolution ),
                        color: color,
                        thickness: thickness ) );
                } else if( pv.Tag.Equals( "right_spectrum" ) ) {
                    toAdd.Add( PlotExtension.SetLineSeries(
                        slide.Data.GetSpectrumByHz( Hands.Right, HzLower, HzUpper ).GetPartPercent( Resolution ),
                        color: color,
                        thickness: thickness ) );
                } else if( pv.Tag.Equals( "right_corr" ) ) {
                    toAdd.Add( PlotExtension.SetLineSeries(
                        slide.Data.GetCorrelation( Hands.Right ).GetPartPercent( Resolution ),
                        color: color,
                        thickness: thickness ) );
                }
                // обе руки
                else if( pv.Tag.Equals( "all_spectrum" ) ) {
                    toAdd.Add( PlotExtension.SetLineSeries(
                        slide.Data.GetSpectrumByHz( Hands.Left, HzLower, HzUpper ).GetPartPercent( Resolution ),
                        color: color,
                        thickness: thickness ) );
                    toAdd.Add( PlotExtension.SetLineSeries(
                        slide.Data.GetSpectrumByHz( Hands.Right, HzLower, HzUpper ).GetPartPercent( Resolution ),
                        color: color,
                        thickness: thickness ) );
                } else if( pv.Tag.Equals( "all_tremor" ) ) {
                    toAdd.Add( PlotExtension.SetLineSeries(
                        slide.Data.GetTremor( Hands.Left ).GetPartPercent( Resolution ),
                        color: color,
                        thickness: thickness ) );
                    toAdd.Add( PlotExtension.SetLineSeries(
                        slide.Data.GetTremor( Hands.Right ).GetPartPercent( Resolution ),
                        color: color,
                        thickness: thickness ) );
                } else if( pv.Tag.Equals( "all_corr" ) ) {
                    toAdd.Add( PlotExtension.SetLineSeries(
                        slide.Data.GetCorrelation( Hands.Left ).GetPartPercent( Resolution ),
                        color: color,
                        thickness: thickness ) );
                    toAdd.Add( PlotExtension.SetLineSeries(
                        slide.Data.GetCorrelation( Hands.Right ).GetPartPercent( Resolution ),
                        color: color,
                        thickness: thickness ) );
                }
                // выбранна€ рука
                else if( pv.Tag.ToString().StartsWith( SelectedTab.Tag.ToString() ) ) {
                    if( pv.Tag.ToString().StartsWith( slide.Id.ToString() ) ) {
                        // тремор
                        if( pv.Tag.ToString().EndsWith( "tremor" ) ) {
                            toAdd.Add( PlotExtension.SetLineSeries(
                                slide.Data.GetTremor( Hands.Left ).GetPartPercent( Resolution ) ) );
                            toAdd.Add( PlotExtension.SetLineSeries(
                                slide.Data.GetTremor( Hands.Right ).GetPartPercent( Resolution ) ) );
                        }
                        // спектр
                        else if( pv.Tag.ToString().EndsWith( "spectrum" ) ) {
                            toAdd.Add( PlotExtension.SetLineSeries(
                                slide.Data.GetSpectrumByHz( Hands.Left, HzLower, HzUpper ).GetPartPercent( Resolution ) ) );
                            toAdd.Add( PlotExtension.SetLineSeries(
                                slide.Data.GetSpectrumByHz( Hands.Right, HzLower, HzUpper ).GetPartPercent( Resolution ) ) );
                        }
                        // коррел€ци€
                        else if( pv.Tag.ToString().EndsWith( "corr" ) ) {
                            toAdd.Add( PlotExtension.SetLineSeries(
                                slide.Data.GetCorrelation( Hands.Left ).GetPartPercent( Resolution ) ) );
                            toAdd.Add( PlotExtension.SetLineSeries(
                                slide.Data.GetCorrelation( Hands.Right ).GetPartPercent( Resolution ) ) );
                        }
                    }
                }
            }
            toAdd.ForEach( series => { if( series != null ) model.Series.Add( series ); } );
            model.AutoScale();
            model.InvalidatePlot( true );
            pv.Model = model;
            mGraphics.Add( pv.Tag, pv );
        }

        private void OxyUnload( object param ) {
            if( param is PlotView == false ) return;
            var pv = (PlotView) param;
            if( pv.Tag != null )
                if( mGraphics.ContainsKey( pv.Tag ) ) {
                    mGraphics.Remove( pv.Tag );
                }
        }

        private void RemoveGraphic( object key ) {
            mGraphics[key].Series.Clear();
            mGraphics.Remove( key );
        }

        private void ResetGraphAxes() {
            mPMSpectrum_Left.ResetAllAxes();
            mPMSpectrum_Right.ResetAllAxes();
            mPMTremor_Left.ResetAllAxes();
            mPMTremor_Right.ResetAllAxes();
            UpdateAllProperties();
        }

        private void SetShow( object param ) {
            if( param is CheckBox ) {
                var cb = (CheckBox) param;
                cb.IsChecked = !cb.IsChecked;
            } else if( param is Slide ) {
                var slide = (Slide) param;
                var first = PlayList.Slides.First( sd => sd.Id.Equals( slide.Id ) );
                if( first != null ) first.IsShow = !first.IsShow;
            }
            var selected = SelectedItem;
            NotifyPropertyChanged( m => m.Slides );
            NotifyPropertyChanged( m => m.IsShowAll );
            SelectedItem = selected;
        }

        private void ShowOnlyOne() {
            SlideToShow = SelectedItem;
            // когда-нибудь придумать что-нибудь получше
            SelectedTabIndex = 3;
            UpdateGraphFull();
        }

        private void UpdateGraphFull() {
            //mPMSpectrum_Left = null;
            //mPMSpectrum_Right = null;
            //mPMTremor_Left = null;
            //mPMTremor_Right = null;
            var list = mGraphics.Select( kv => kv.Value ).ToList();
            foreach( var value in list ) {
                OxyLoad( value );
            }
        }
    }
}