using System;
using System.Collections.Generic;
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
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using tEngine.DataModel;
using tEngine.Helpers;
using tEngine.PlotCreator;

namespace tEngine.UControls
{
    /// <summary>
    /// Interaction logic for SelectablePlot.xaml
    /// </summary>
    public partial class SelectablePlot : UserControl
    {

        private bool mNeedInvalidate = false;

        public int BeginPoint
        {
            get => (int)GetValue(BeginPointProperty);
            set => SetValue(BeginPointProperty, value);
        }

        public MeasurementData Data
        {
            get => (MeasurementData)GetValue(DataProperty);
            set => SetValue(DataProperty, value);
        }

        public int EndPoint
        {
            get => (int)GetValue(EndPointProperty);
            set => SetValue(EndPointProperty, value);
        }

        public List<DataPoint> LeftHandConst
        {
            get
            {
                double w = root.ActualWidth;
                List<DataPoint> data =
                    Data.GetConstBase(Hands.Left)
                        .Select((s, i) => new DataPoint(i, s))
                        .GetPartExactly((uint)(w * 2))
                        .ToList();
                return data;
            }
        }

        public List<DataPoint> LeftHandTremor
        {
            get
            {
                double w = root.ActualWidth;
                List<DataPoint> data =
                    Data.GetTremorBase(Hands.Left)
                        .Select((s, i) => new DataPoint(i, s))
                        .GetPartExactly((uint)(w * 2))
                        .ToList();
                return data;
            }
        }

        public int Maximum
        {
            get => (int)GetValue(MaximumProperty);
            set => SetValue(MaximumProperty, value);
        }

        public List<DataPoint> RightHandConst
        {
            get
            {
                double w = root.ActualWidth;
                return
                    Data.GetConstBase(Hands.Right)
                        .Select((s, i) => new DataPoint(i, s))
                        .GetPartExactly((uint)(w * 2))
                        .ToList();
            }
        }

        public List<DataPoint> RightHandTremor
        {
            get
            {
                double w = root.ActualWidth;
                return
                    Data.GetTremorBase(Hands.Right)
                        .Select((s, i) => new DataPoint(i, s))
                        .GetPartExactly((uint)(w * 2))
                        .ToList();
            }
        }

        public SelectablePlot()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Создание новых моделей на основе PlotView
        /// </summary>
        private void Init()
        {
            UpdateData();

            constPlot.Model = null;
            tremorPlot.Model = null;

            PlotModelEx pmeConst = new PlotModelEx(constPlot);
            PlotModelEx pmeTremor = new PlotModelEx(tremorPlot);

            pmeConst.AddSelector(SelectAction);
            pmeTremor.AddSelector(SelectAction);

            constPlot.Model = pmeConst;
            tremorPlot.Model = pmeTremor;

            SelectBoth();
            ResetAll();
        }

        /// <summary>
        /// Перерисовка графиков
        /// </summary>
        private void InvalidatePlots()
        {
            if (constPlot.Model == null || tremorPlot.Model == null) return;
            constPlot.Model.InvalidatePlot(true);
            tremorPlot.Model.InvalidatePlot(true);
        }

        /// <summary>
        /// Сброс масштабирования
        /// Обновление слайдера
        /// </summary>
        private void ResetAll()
        {
            if (constPlot.Model == null || tremorPlot.Model == null) return;

            Slider.GetBindingExpression(RangeSlider.MaximumProperty).UpdateTarget();
            Slider.GetBindingExpression(RangeSlider.LowerValueProperty).UpdateTarget();
            Slider.GetBindingExpression(RangeSlider.UpperValueProperty).UpdateTarget();

            constPlot.Model.Axes.ToList().ForEach(axis =>
            {
                axis.Minimum = double.NaN;
                axis.Maximum = double.NaN;
                axis.Reset();
            });
            tremorPlot.Model.Axes.ToList().ForEach(axis =>
            {
                axis.Minimum = double.NaN;
                axis.Maximum = double.NaN;
                axis.Reset();
            });

            InvalidatePlots();
        }

        private void SelectablePlot_OnLoaded(object sender, RoutedEventArgs e)
        {
            UpdateData();
            Init();
            ResetAll();
        }

        private void SelectablePlot_OnSizeChanged(object sender, SizeChangedEventArgs e) { }

        private void SelectAction(bool wasUp, double min, double max)
        {
            if (wasUp && true)
            {
                //BeginPoint = (int) (Maximum*min/100);
                //EndPoint = (int) (Maximum*max/100);
                BeginPoint = (int)min;
                EndPoint = (int)max;
            }
        }

        /// <summary>
        /// Обновление значений выбора диапазона на моделях графиков
        /// </summary>
        private void SelectBoth()
        {
            if (constPlot.Model != null && tremorPlot.Model != null)
            {
                //var min = BeginPoint*100.0/Maximum;
                //var max = EndPoint*100.0/Maximum;
                int min = BeginPoint;
                int max = EndPoint;
                constPlot.Model.SetSelection(min, max);
                tremorPlot.Model.SetSelection(min, max);
            }
        }

        /// <summary>
        /// Обновление информации для графиков
        /// </summary>
        private void UpdateData()
        {
            constPlot.Series.ToList()
                .ForEach(
                    series => { series.GetBindingExpression(ItemsControl.ItemsSourceProperty).UpdateTarget(); });
            tremorPlot.Series.ToList()
                .ForEach(
                    series => { series.GetBindingExpression(ItemsControl.ItemsSourceProperty).UpdateTarget(); });
        }

        public static readonly DependencyProperty MaximumProperty = DependencyProperty.Register(
            "Maximum", typeof(int), typeof(SelectablePlot), new PropertyMetadata(default(int), (obj, args) =>
            {
                SelectablePlot plot = obj as SelectablePlot;
                if (plot.mNeedInvalidate)
                {
                    plot.SelectBoth();
                    plot.InvalidatePlots();
                }
            }));

        public static readonly DependencyProperty BeginPointProperty = DependencyProperty.Register(
            "BeginPoint", typeof(int), typeof(SelectablePlot),
            new PropertyMetadata(int.MinValue, (obj, args) =>
            {
                SelectablePlot plot = obj as SelectablePlot;
                if (plot.mNeedInvalidate)
                {
                    plot.SelectBoth();
                    plot.InvalidatePlots();
                }
            }));

        public static readonly DependencyProperty DataProperty = DependencyProperty.Register(
            "Data", typeof(MeasurementData), typeof(SelectablePlot), new PropertyMetadata(new MeasurementData(), (obj, args) =>
            {
                SelectablePlot plot = obj as SelectablePlot;
                if (args.NewValue == null)
                {
                    plot.Data = new MeasurementData();
                    return;
                }
                plot.mNeedInvalidate = false;
                plot.BeginPoint = plot.Data.BeginPoint;
                plot.EndPoint = plot.Data.EndPoint;
                plot.Init();

                plot.mNeedInvalidate = true;
                plot.SelectBoth();
                plot.InvalidatePlots();
            }));

        public static readonly DependencyProperty EndPointProperty = DependencyProperty.Register(
            "EndPoint", typeof(int), typeof(SelectablePlot), new PropertyMetadata(int.MaxValue, (obj, args) =>
            {
                SelectablePlot plot = obj as SelectablePlot;
                if (plot.EndPoint == -1)
                {
                    plot.EndPoint = plot.Maximum;
                    return;
                }
                if (plot.mNeedInvalidate)
                {
                    plot.SelectBoth();
                    plot.InvalidatePlots();
                }
            }));
    }
}