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
        private Guid mID;

        public PlotModelEx()
        {
            Init();
            InvalidatePlot(true);
        }

        private void Init()
        {
            mID = Guid.NewGuid();
            AcceptModelSettings();
            //var ps = new PlotSet( this );
            //AcceptSettings( ps );
        }

        public PlotModelEx(OxyPlot.Wpf.PlotView pv)
        {
            if (pv != null)
            {

                // определение источника, 
                // если уже есть модель - она в приоритете
                object source = pv;
                IEnumerable sAxes = pv.Axes;
                IEnumerable sSeries = pv.Series;

                if (pv.Model != null)
                {
                    source = pv.Model;
                    sAxes = pv.Model.Axes;
                    sSeries = pv.Model.Series;
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

        // Применить настройки c текущей модели
        public void AcceptModelSettings()
        {
            PlotSet ps = new PlotSet(this);
            AcceptSettings(ps);
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
                    axes1.PositionAtZeroCrossing = false;
                    axes2.PositionAtZeroCrossing = false;
                    axes1.IsAxisVisible = true;
                    axes2.IsAxisVisible = true;

                    break;
                case EAxesStyle.Cross:
                    axes1.PositionAtZeroCrossing = true;
                    axes2.PositionAtZeroCrossing = true;
                    axes1.IsAxisVisible = true;
                    axes2.IsAxisVisible = true;

                    break;
                case EAxesStyle.None:
                    axes1.IsAxisVisible = false;
                    axes2.IsAxisVisible = false;

                    break;
            }
            axes1.TitleFontSize = ps.AxesFontSize;
            axes2.TitleFontSize = ps.AxesFontSize;


            PlotAreaBackground = ps.BackColor.GetColorOxy();
            Title = ps.ShowTitle ? ps.Title : "";
            TitleFontSize = ps.TitleFontSize;
            // TitlePos = ETitlePos.Top; 

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

        public Guid ID
        {
            get { return mID; }
        }

        public void ResetModel()
        {
            ScaleAxes();
            InvalidatePlot(true);
        }

        public void ScaleAxes()
        {
            ResetAllAxes();
        }

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
                //axes.ExtraGridlines = //min, max -> grid          
                if (pa.AutoGrid)
                {
                    axes.MajorGridlineStyle = LineStyle.Solid;
                }
            }
            axes.Title = pa.ShowTitle ? pa.Title : "";
            axes.FontSize = pa.ShowNumbers ? pa.NumbersFontSize : 0.1d;
            axes.LabelFormatter = d => GetNumber(pa.DecimalCount, pa.ExponentCount, d);
            //AutoScale = true;

            //DecimalCount = 2;
            //ExponentCount = 3;
            if (pa.AutoScale)
            {
                this.AutoScale();
                //this.AutoScale(axes);
            }
            else
            {
                axes.Minimum = pa.Minimum;
                axes.Maximum = pa.Maximum;
            }

            Cloner.CopyAllProperties(axes, pa);
            return axes;
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
            string str = "{" + string.Format("0:F{0}", dec) + "}";
            if (Math.Abs(order) > exp)
            {
                E = (int)(Math.Abs(order) - exp);
                E *= Math.Sign(order);
                str = string.Format("{0}e{2}{1}", str, Math.Abs(E), E >= 0 ? "+" : "-");
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