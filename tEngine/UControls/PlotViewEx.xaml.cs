using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
using System.Windows.Threading;
using OxyPlot;
using OxyPlot.Series;
using OxyPlot.Wpf;
using tEngine.Helpers;
using tEngine.PlotCreator;

namespace tEngine.UControls
{
    /// <summary>
    /// Interaction logic for PlotViewEx.xaml
    /// </summary>
    public partial class PlotViewEx : UserControl
    {
        private PlotModelEx mPlotModel;
        private PlotSet mPlotSet;
        private PlotView mPlotView;
        private DispatcherTimer mResizeDrawing = new DispatcherTimer();

        public ImageSource Bitmap
        {
            get
            {
                if (ImageSource == null)
                {
                    ImageSource = MVVM.Converters.PlotModelToBitmap.GetBitmapFromPM(PlotModel);
                }
                return ImageSource;
            }
        }

        public BitmapSource ImageSource { get; set; }

        public PlotModelEx PlotModel
        {
            get => mPlotModel;
            set => mPlotModel = value;
        }

        public PlotView PlotView
        {
            get => mPlotView;
            set
            {
                mPlotView = value;

                // обновление модели при загрузки контрола
                root.Loaded += RootOnLoaded;

                PlotContainer.Children.Clear();
                PlotContainer.Children.Add(mPlotView);
            }
        }

        public bool ShowPlot
        {
            get => (bool)GetValue(ShowPlotProperty);
            set => SetValue(ShowPlotProperty, value);
        }

        public static readonly DependencyProperty ShowMenuProperty = DependencyProperty.Register(
            "ShowMenu", typeof(Visibility), typeof(PlotViewEx), new PropertyMetadata(Visibility.Visible));

        public Visibility ShowMenu
        {
            get => (Visibility)GetValue(ShowMenuProperty);
            set => SetValue(ShowMenuProperty, value);
        }

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        private PlotSet PlotSet
        {
            get => mPlotSet;
            set
            {
                mPlotSet = value;
                if (PlotModel != null)
                {
                    PlotModel.AcceptSettings(mPlotSet);
                    if (ShowPlot == false)
                        UpdateImage();
                }
            }
        }

        public PlotViewEx()
        {
            InitializeComponent();


            PlotSet = new PlotSet();
            ShowPlot = true;


            // перерисовка при ресайзе
            mResizeDrawing.Interval = new TimeSpan(0, 0, 0, 0, 4);
            mResizeDrawing.Tick += (sender, args) =>
            {
                if (ShowPlot == false)
                    UpdateImage();
                mResizeDrawing.Stop();
            };

            PlotView = new PlotView();
        }


        public void AddLineSeries(IList<DataPoint> data, string title = "", int thickness = 1,
            Color? color = null)
        {
            if (data == null || data.Count <= 0) return;
            OxyPlot.Series.LineSeries series = new OxyPlot.Series.LineSeries
            {
                Smooth = false,
                Title = title,
                StrokeThickness = thickness,
                LineStyle = LineStyle.Solid,
                MinimumSegmentLength = 10,
                MarkerSize = 3,
                MarkerStroke = OxyColors.ForestGreen,
                //MarkerType = MarkerType.Plus
            };
            series.Points.AddRange(data);
            if (color != null) series.Color = color.Value.GetColorOxy();
            PlotModel.Series.Add(series);
        }

        public void Clear()
        {
            if (PlotModel != null)
            {
                PlotModel.Series.Clear();
            }
        }

        /// <summary>
        /// Установка модели на PlotView
        /// </summary>
        public void InitModel()
        {
            Debug.Assert(PlotView != null);

            PlotView.Model = null;

            PlotModel = new PlotModelEx(PlotView);
            //Title как свойство PlotViewEx
            if (Title.IsNullOrEmpty() == false)
            {
                PlotModel.Title = Title;
            }

            PlotView.Model = PlotModel;
        }

        public void ReDraw(bool autoScale = true)
        {
            if (PlotModel != null)
            {
                if (autoScale)
                    PlotModel.AutoScale();
                PlotModel.InvalidatePlot(true);
                UpdateImage();
            }
        }

        public void UpdateImage()
        {
            ImageSource = null;
            Image.GetBindingExpression(Image.SourceProperty).UpdateTarget();
        }

        private void ButtonReset_OnClick(object sender, RoutedEventArgs e)
        {
            PlotModel.ResetModel();
            UpdateImage();
        }

        private void ButtonSettings_OnClick(object sender, RoutedEventArgs e)
        {
            PlotSettings ps = new PlotSettings();
            ps.SetModel(PlotModel);
            PlotSet.AxesOX.AutoScale = false;
            PlotSet.AxesOY.AutoScale = false;
            PlotSet.CopyScale(PlotModel);
            if (Title.IsNullOrEmpty() == false)
            {
                PlotSet.Title = Title;
            }
            PlotSet.TitleFontSize = PlotModel.TitleFontSize;

            ps.PlotSet = PlotSet;
            ps.AcceptSettingsAction = set => { PlotSet = set; };
            if (ps.ShowDialog() == true)
            {
                //accept settings
                PlotSet = ps.PlotSet;
            }
        }

        private void PlotViewEx_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            mResizeDrawing.Stop();
            mResizeDrawing.Start();
        }

        private void RootOnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            InitModel();
            root.Loaded -= RootOnLoaded;
        }

        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
            "Title", typeof(string), typeof(PlotViewEx), new PropertyMetadata(default(string), (obj, args) =>
            {
                PlotViewEx pme = (obj as PlotViewEx);
                string title = args.NewValue as string;
                if (pme.PlotModel != null)
                    pme.PlotModel.Title = title;
                if (pme.PlotView != null)
                    pme.PlotView.Title = title;
            }));

        public static readonly DependencyProperty ShowPlotProperty = DependencyProperty.Register("ShowPlot",
            typeof(bool), typeof(PlotViewEx),
            new PropertyMetadata(default(bool),
                (obj, args) =>
                {
                    if ((bool?)args.NewValue == false)
                        (obj as PlotViewEx).UpdateImage();
                }));
    }
}