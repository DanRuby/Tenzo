using System;
using System.Collections;
using System.Diagnostics;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using tEngine.Helpers;

namespace tEngine.PlotCreator
{
    public class PlotModelEx : PlotModel
    {
        public Guid ID => mID;
        private Guid mID;

        public PlotModelEx()
        {
            Init();
            InvalidatePlot(true);
        }

        public PlotModelEx(OxyPlot.Wpf.PlotView plotView)
        {
            if (plotView != null)
            {

                // определение источника, 
                // если уже есть модель - она в приоритете
                object source = plotView;
                IEnumerable sAxes = plotView.Axes;
                IEnumerable sSeries = plotView.Series;

                if (plotView.Model != null)
                {
                    source = plotView.Model;
                    sAxes = plotView.Model.Axes;
                    sSeries = plotView.Model.Series;
                }


                Cloner.CopyAllFields(this, source);
                Cloner.CopyAllProperties(this, source);

                // копирование всех осей
                Axis nAxis = null;
                foreach (object axis in sAxes)
                {
                    Debug.Assert(axis != null);
                    if (axis.GetType().Name.Equals("LinearAxis"))
                    {
                        nAxis = new LinearAxis();
                    }
                    else
                    {
                        throw new NotImplementedException();
                        // добавить if на нужный тип
                    }
                    Cloner.CopyAllFields(nAxis, axis);
                    Cloner.CopyAllProperties(nAxis, axis);
                    Axes.Add(nAxis);
                }

                // копирование всех рядов
                Series nSeries = null;
                foreach (object series in sSeries)
                {
                    Debug.Assert(series != null);
                    if (series.GetType().Name.Equals("LineSeries"))
                    {
                        nSeries = new LineSeries();
                    }
                    else if (series.GetType().Name.Equals("AreaSeries"))
                    {
                        nSeries = new AreaSeries();
                    }
                    else
                    {
                        throw new NotImplementedException();
                        // добавить if на нужный тип
                    }
                    Cloner.CopyAllFields(nSeries, series);
                    Cloner.CopyAllProperties(nSeries, series);

                    nSeries.IsVisible = true;

                    Series.Add(nSeries);
                }

                //...
                // добавить foreach на нужную коллекцию

            }
            Init();
            InvalidatePlot(true);
        }

        /// <summary>
        /// Обнуляет график
        /// </summary>
        public void ResetModel()
        {
            ResetAllAxes();
            InvalidatePlot(true);
        }

        /// <summary>
        /// Применить настройки
        /// </summary>
        /// <param name="ps"></param>
        public void AcceptSettings(PlotSet ps)
        {
            foreach (Axis axise in Axes)
            {
                axise.Minimum = double.NaN;
                axise.Maximum = double.NaN;
            }
            ResetAllAxes();

            Axis axes1 = CreateAxis(ps.AxesOY);
            Axis axes2 = CreateAxis(ps.AxesOX);

            axes1.Position = AxisPosition.Left;
            axes2.Position = AxisPosition.Bottom;
            switch (ps.AxesStyle)
            {
                case EAxesStyle.Boxed:
                    SetAxesPositionAtCrossing(axes1, axes2, false);
                    SetAxesVisibility(axes1, axes2,true);
                    break;
                case EAxesStyle.Cross:
                    SetAxesPositionAtCrossing(axes1, axes2, true);
                    SetAxesVisibility(axes1, axes2, true);
                    break;
                case EAxesStyle.None:
                    SetAxesVisibility(axes1, axes2, false);
                    break;
            }
            axes1.TitleFontSize = ps.AxesFontSize;
            axes2.TitleFontSize = ps.AxesFontSize;


            PlotAreaBackground = ps.BackColor.GetColorOxy();
            Title = ps.ShowTitle ? ps.Title : "";
            TitleFontSize = ps.TitleFontSize;

            Axes.Clear();
            Axes.Add(axes1);
            Axes.Add(axes2);

            if (ps.AxesOY.AutoScale)
            {
                this.AutoScale(axes1);
            }
            if (ps.AxesOX.AutoScale)
            {
                this.AutoScale(axes2);
            }

            InvalidatePlot(true);
        }

        /// <summary>
        /// Инициализация нового ID и настроек
        /// </summary>
        private void Init()
        {
            mID = Guid.NewGuid();
            AcceptModelSettings();
        }

        /// <summary>
        ///  Применить настройки c текущей модели
        /// </summary>
        private void AcceptModelSettings()
        {
            PlotSet ps = new PlotSet(this);
            AcceptSettings(ps);
        }

        /// <summary>
        /// Создает ось с предустановленными настройками
        /// </summary>
        /// <param name="pa"></param>
        /// <returns></returns>
        private Axis CreateAxis(PlotAxes pa)
        {
            Axis axes;
            if (pa.LogScale)
            {
                axes = new LogarithmicAxis();
            }
            else
            {
                axes = new LinearAxis();
            }
            if (pa.ShowGrid)
            {
                System.Windows.Media.Color color = pa.GridColor;
                color.A = 200;
                axes.MajorGridlineColor = color.GetColorOxy();
                if (pa.AutoGrid)
                    axes.MajorGridlineStyle = LineStyle.Solid;
            }
            axes.Title = pa.ShowTitle ? pa.Title : "";
            axes.FontSize = pa.ShowNumbers ? pa.NumbersFontSize : 0.1d;
            axes.LabelFormatter = d => GetNumber(pa.DecimalCount, pa.ExponentCount, d);
            if (pa.AutoScale)
            {
                this.AutoScale();
            }
            else
            {
                axes.Minimum = pa.Minimum;
                axes.Maximum = pa.Maximum;
            }

            Cloner.CopyAllProperties(axes, pa);
            return axes;
        }

        private static void SetAxesVisibility(Axis axes1, Axis axes2, bool visibility)
        {
            axes2.IsAxisVisible = visibility;
            axes1.IsAxisVisible = visibility;
        }

        private static void SetAxesPositionAtCrossing(Axis axes1, Axis axes2, bool value)
        {
            axes2.PositionAtZeroCrossing = value;
            axes1.PositionAtZeroCrossing = value;
        }

        /// <summary>
        /// возвращает строку {0:Fx}E±n
        /// </summary>
        /// <param name="dec">Количество знаков после запятой</param>
        /// <param name="exp">После какого порядка рисовать степень</param>
        /// <returns></returns>
        private string GetNumber(int dec, uint exp, double value)
        {
            int order = GetOrder(value);
            int E = 0;
            string str = "{" + $"0:F{dec}" + "}";
            if (Math.Abs(order) > exp)
            {
                E = (int)(Math.Abs(order) - exp);
                E *= Math.Sign(order);
                str = $"{str}e{(E >= 0 ? "+" : "-")}{Math.Abs(E)}";
            }
            str = string.Format(str, value / Math.Pow(10, E));
            return str;
        }

        /// <summary>
        /// Возвращает порядок числа
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private int GetOrder(double value)
        {
            if (value == 0) return 0;
            double tmp = Math.Abs(value);
            int result = 0;
            while (Math.Log10(tmp) < 1)
            {
                tmp *= 10;
                result--;
            }
            return (int)(Math.Log10(tmp)) + result;
        }
    }
}