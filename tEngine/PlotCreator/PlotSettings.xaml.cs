using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using OxyPlot;
using OxyPlot.Axes;
using tEngine.Helpers;
using tEngine.MVVM;
using UIElement = System.Windows.UIElement;

namespace tEngine.PlotCreator {
    public enum EAxesStyle {
        Boxed,
        Cross,
        None
    }

    public enum ETitlePos {
        Top,
        Bottom
    }

    [DataContract]
    public class PlotSet {
        [DataMember]
        public double AxesFontSize { get; set; }

        [DataMember]
        public PlotAxes AxesOX { get; set; }

        [DataMember]
        public PlotAxes AxesOY { get; set; }

        [DataMember]
        public EAxesStyle AxesStyle { get; set; }

        [DataMember]
        public Color BackColor { get; set; }

        [DataMember]
        public bool ShowTitle { get; set; }

        [DataMember]
        public string Title { get; set; }

        [DataMember]
        public double TitleFontSize { get; set; }

        [DataMember]
        public ETitlePos TitlePos { get; set; }

        public PlotSet() {
            Init( null );
        }

        public PlotSet( PlotModelEx pm ) {
            Init( pm );
        }

        public void CopyScale( PlotModel pm ) {
            var axes1 = pm.Axes.Count > 0 ? pm.Axes[0] : null;
            var axes2 = pm.Axes.Count > 1 ? pm.Axes[1] : null;
            Debug.Assert( axes1 != null && axes2 != null );

            CopyScale( AxesOY, axes1 );
            CopyScale( AxesOX, axes2 );
        }

        public void CopyScale( PlotAxes dest, Axis source ) {
            dest.Minimum = source.ActualMinimum;
            dest.Maximum = source.ActualMaximum;
        }

        public void LoadFromModel( PlotModelEx pm ) {
            BackColor = pm.Background.GetColorMedia();
            Title = pm.Title;
            TitleFontSize = (int) pm.TitleFontSize; // todo проверить размеры шрифтов
            ShowTitle = true;
            //TitlePos = 

            var axes1 = pm.Axes.Count > 0 ? pm.Axes[0] : null;
            var axes2 = pm.Axes.Count > 1 ? pm.Axes[1] : null;
            if( axes1 == null || axes2 == null ) return;
            Debug.Assert( axes1 != null && axes2 != null );

            if( (axes1.IsAxisVisible || axes2.IsAxisVisible) == false )
                AxesStyle = EAxesStyle.None;
            else
                AxesStyle = (axes1.PositionAtZeroCrossing || axes2.PositionAtZeroCrossing)
                    ? EAxesStyle.Cross
                    : EAxesStyle.Boxed;


            AxesFontSize = (int) axes1.FontSize;

            Cloner.CopyAllProperties( AxesOY, axes1 );
            Cloner.CopyAllProperties( AxesOX, axes2 );
            CopyScale( AxesOY, axes1 );
            CopyScale( AxesOX, axes2 );
        }

        public void SetDefault() {
            AxesOX.SetDefault();
            AxesOY.SetDefault();
            AxesStyle = EAxesStyle.Boxed;
            BackColor = Colors.White;
            Title = "";
            TitleFontSize = 12; // todo проверить размеры шрифтов
            ShowTitle = true;
            TitlePos = ETitlePos.Top;
            AxesFontSize = 12; // проверить 
        }

        private void Init( PlotModelEx pm ) {
            AxesOX = new PlotAxes();
            AxesOY = new PlotAxes();
            if( pm == null ) {
                SetDefault();
            } else {
                LoadFromModel( pm );
            }
        }

        #region Byte <=> Object

        public byte[] ToByteArray() {
            return BytesPacker.JSONObj( this );
        }

        public bool LoadFromArray( byte[] array ) {
            var obj = BytesPacker.LoadJSONObj<PlotSet>( array );
            Cloner.CopyAllProperties( this, obj );
            return true;
        }

        #endregion
    }

    [DataContract]
    public class PlotAxes {
        [DataMember]
        public bool AutoGrid { get; set; }

        [DataMember]
        public bool AutoScale { get; set; }

        /// <summary>
        /// Количество знаков после запятой
        /// </summary>
        [DataMember]
        public int DecimalCount { get; set; }

        /// <summary>
        /// После какого порядка рисовать степень (по модулю, одинакого и в +, и в -)
        /// </summary>
        [DataMember]
        public uint ExponentCount { get; set; }

        /// <summary>
        /// Шаг сетки
        /// </summary>
        [DataMember]
        public double Grid { get; set; }

        [DataMember]
        public Color GridColor { get; set; }

        [DataMember]
        public bool IsAxisVisible { get; set; }

        [DataMember]
        public bool IsPanEnabled { get; set; }

        [DataMember]
        public bool IsZoomEnabled { get; set; }

        [DataMember]
        public bool LogScale { get; set; }

        [DataMember]
        public double Maximum { get; set; }

        [DataMember]
        public double Minimum { get; set; }

        [DataMember]
        public double NumbersFontSize { get; set; }

        [DataMember]
        public bool ShowGrid { get; set; }

        [DataMember]
        public bool ShowNumbers { get; set; }

        [DataMember]
        public bool ShowTitle { get; set; }

        [DataMember]
        public string Title { get; set; }

        public PlotAxes() {
            SetDefault();
        }

        public void SetDefault() {
            LogScale = false;
            ShowGrid = false;
            GridColor = Colors.LightGray;
            ShowNumbers = true;
            AutoScale = true;
            AutoGrid = true;
            Minimum = 0;
            Maximum = 10;
            Grid = 2;
            DecimalCount = 2;
            ExponentCount = 3;
            NumbersFontSize = 12; // проверить
            Title = "";
            ShowTitle = true;
            IsPanEnabled = true;
            IsZoomEnabled = true;
        }
    }

    /// <summary>
    /// Interaction logic for PlotSettings.xaml
    /// </summary>
    public partial class PlotSettings : Window {
        private PlotSettingsVM mDataContext;

        public Action<PlotSet> AcceptSettingsAction {
            get { return mDataContext.AcceptSettingsAction; }
            set { mDataContext.AcceptSettingsAction = value; }
        }

        public PlotSet PlotSet {
            get { return mDataContext.PlotSet; }
            set { mDataContext.PlotSet = value; }
        }

        public PlotSettings() {
            InitializeComponent();
            WindowManager.UpdateWindowPos( this.GetType().Name, this );
            mDataContext = new PlotSettingsVM() {Parent = this};
            DataContext = mDataContext;
        }

        public void SetModel( PlotModelEx plotModel ) {
            Debug.Assert( plotModel != null );
            mDataContext.PlotModel = plotModel;
            mDataContext.UpdateAllProperties();
        }

        private void ColorSelect_OnClick( object sender, RoutedEventArgs e ) {
            mDataContext.CMDColorSelect.DoExecute( (sender as Button) );
        }

        private void SimpleTextBox_OnKeyDown( object sender, KeyEventArgs e ) {
            if( e.Key == Key.Enter ) {
                var element = sender as UIElement;
                if( element != null ) {
                    element.MoveFocus( new TraversalRequest( FocusNavigationDirection.Next ) );
                }
            }
        }

        private void Window_OnClosing( object sender, CancelEventArgs e ) {
            if( mDataContext != null ) {
                try {
                    DialogResult = mDataContext.DialogResult;
                } catch( Exception ex ) {
                    Debug.Assert( false, ex.Message );
                }
            }
            WindowManager.SaveWindowPos( this.GetType().Name, this );
        }
    }


    public class PlotSettingsVM : Observed<PlotSettingsVM> {
        private Color mBcColor;
        private PlotModelEx mPlotModel;
        private PlotSet mPlotSet;
        public Action<PlotSet> AcceptSettingsAction { get; set; }
        public Command CMDAcceptButton { get; private set; }
        public Command CMDCancelButton { get; private set; }
        public Command CMDColorSelect { get; private set; }
        public Command CMDOkButton { get; private set; }

        public PlotModelEx PlotModel {
            get { return mPlotModel; }
            set {
                mPlotModel = value;
                PlotSet = new PlotSet( mPlotModel );
                NotifyPropertyChanged( m => m.PlotModel );
            }
        }

        public PlotSet PlotSet {
            get { return mPlotSet; }
            set {
                mPlotSet = value;
                NotifyPropertyChanged( m => m.PlotSet );
            }
        }

        public PlotSettingsVM() {
            CMDColorSelect = new Command( CMDColorSelect_Func );
            CMDOkButton = new Command( CMDOkButton_Func );
            CMDAcceptButton = new Command( CMDAcceptButton_Func );
            CMDCancelButton = new Command( CMDCancelButton_Func );

            PlotSet = PlotSet ?? new PlotSet();

            UpdateAllProperties();
        }

        private void CMDAcceptButton_Func() {
            if( AcceptSettingsAction != null )
                AcceptSettingsAction( PlotSet );
        }

        private void CMDCancelButton_Func() {
            EndDialog( false );
        }

        /// <summary>
        /// Принимает саму кнопку
        /// </summary>
        /// <param name="param"></param>
        private void CMDColorSelect_Func( object param ) {
            var bt = param as Button;
            var cd = new System.Windows.Forms.ColorDialog() {
                Color = ((SolidColorBrush) bt.Background).Color.GetColorDrawing()
            };
            if( cd.ShowDialog() == System.Windows.Forms.DialogResult.OK ) {
                var newColor = cd.Color.GetColorMedia();
                bt.Background = new SolidColorBrush( newColor );
            }
        }

        private void CMDOkButton_Func() {
            EndDialog( true );
        }
    }
}