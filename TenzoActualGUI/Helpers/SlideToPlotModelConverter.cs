using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using System.Windows.Media;
using OxyPlot;
using OxyPlot.Wpf;
using tEngine.DataModel;
using tEngine.Helpers;
using tEngine.MVVM.Converters;
using tEngine.PlotCreator;
using tEngine.TActual.DataModel;
using TenzoActualGUI.View;
using Series = OxyPlot.Series.Series;

namespace TenzoActualGUI.Helpers
{
    internal class SlideToPlotModelConverter : ConverterBaseMulti<SlideToPlotModelConverter> {
        public override object Convert( object[] values, Type targetType, object parameter, CultureInfo culture ) {
            if( values.Any( value => value == null ) ) {
                return Binding.DoNothing;
            }
            var pp = new PlotParams( values );

            var toDraw = GetSeries( pp );
            var plotModel = pp.PlotView.CreateModelByView();
            plotModel.Series.Clear();

            foreach( var series in toDraw ) {
                if( series != null ) plotModel.Series.Add( series );
            }

            plotModel.AutoScale();
            plotModel.InvalidatePlot( true );

            return plotModel;
        }

        private IEnumerable<Series> GetSeries( PlotParams pp ) {
            var toDraw = new List<Series>();
            Series lastSeries = null;
            foreach( var hand in new[] {Hands.Left, Hands.Right} ) {
                foreach( var slide in pp.AllSlides ) {
                    IList<DataPoint> data;
                    switch( pp.ShowMode ) {
                        case EShowMode.Tremor:
                            data = slide.Data.GetTremor( hand ).GetPartPercent( pp.Resolution );
                            break;
                        case EShowMode.Spectrum:
                            data =
                                slide.Data.GetSpectrum( hand )
                                    .Where( dp => dp.X > pp.HzLower && dp.X < pp.HzUpper )
                                    .GetPartPercent( 100 ); // спектр лучше выводить полностью
                            break;
                        case EShowMode.Correlation:
                            data = slide.Data.GetCorrelation( hand ).GetPartPercent( pp.Resolution );
                            break;
                        case EShowMode.Const:
                        default:
                            data = slide.Data.GetConst( hand ).GetPartPercent( pp.Resolution );
                            break;
                    }

                    // текущий слайд рисуем последним
                    if( slide.Id.Equals( pp.Slide.Id ) == true && hand == pp.Hand ) {
                        lastSeries = PlotExtension.SetLineSeries(
                            data,
                            color: Colors.Red,
                            thickness: 5 );
                    } else {
                        toDraw.Add( PlotExtension.SetLineSeries(
                            data,
                            color: Color.FromRgb( 227, 227, 227 ),
                            thickness: 3 ) );
                    }
                }
            }
            if( lastSeries != null ) toDraw.Add( lastSeries );
            return toDraw;
        }

        private class PlotParams {
            public IList<Slide> AllSlides { get; set; }
            public Hands Hand { get; set; }
            public double HzLower { get; set; }
            public double HzUpper { get; set; }
            public PlotView PlotView { get; set; }
            public double Resolution { get; set; }
            public EShowMode ShowMode { get; set; }
            public Slide Slide { get; set; }

            public PlotParams( object[] values ) {
                try {
                    Slide = (Slide) values[0];
                    PlotView = (PlotView) values[1];
                    ShowMode = (EShowMode) values[2];
                    Hand = (Hands) values[3];
                    AllSlides = (IList<Slide>) values[4];
                    Resolution = (double) values[5];
                    HzLower = (double) values[6];
                    HzUpper = (double) values[7];
                } catch( Exception  ) {
                    //if( new Observed<object>().IsDesignMode == false )
                    //    Debug.Assert( ex == null, ex.ToString() );
                    throw new Exception();
                }
            }
        }
    }
}