using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using tEngine.Helpers;
using LinearAxis = OxyPlot.Axes.LinearAxis;
using LineSeries = OxyPlot.Series.LineSeries;
using RectangleAnnotation = OxyPlot.Annotations.RectangleAnnotation;
using Series = OxyPlot.Series.Series;

namespace tEngine.PlotCreator
{
    public static class PlotExtension {
        private static string mSelectionTag = "SelectionMode";

        public static void AddLineSeries(this IList<LineSeries> ls, IList<DataPoint> data, string title = "", int thickness = 1,
            Color? color = null ) {
            if( data == null || data.Count <= 0 ) return;
            var series = new OxyPlot.Series.LineSeries {
                Smooth = false,
                Title = title,
                StrokeThickness = thickness,
                LineStyle = LineStyle.Solid,
                MinimumSegmentLength = 10,
                MarkerSize = 3,
                MarkerStroke = OxyColors.ForestGreen,
                //MarkerType = MarkerType.Plus
            };
            series.Points.AddRange( data );
            if( color != null ) series.Color = color.Value.GetColorOxy();
            ls.Add( series);
        }

        public static void SynchScale(params PlotModel[] models) {
            var minX = double.MaxValue;
            var minY = double.MaxValue;
            var maxX = double.MinValue;
            var maxY = double.MinValue;
            foreach( var model in models ) {
                // todo убрать индексы
                var axesY = model.Axes[0];
                var axesX = model.Axes[1];

                if( axesX.Minimum < minX )
                    minX = axesX.Minimum;
                if( axesX.Maximum > maxX )
                    maxX = axesX.Maximum;

                if( axesY.Minimum < minY )
                    minY = axesY.Minimum;
                if( axesY.Maximum > maxY )
                    maxY = axesY.Maximum;
            }
            foreach( var model in models ) {
                var axesY = model.Axes[0];
                var axesX = model.Axes[1];

                axesX.Minimum = minX;
                axesX.Maximum = maxX;

                axesY.Minimum = minY;
                axesY.Maximum = maxY;
            }


        }
        public static void AddSelector( this PlotModel model, Action<bool, double, double> selectAction = null,
            Color? selectorColor = null ) {
            
            if( model.Annotations.Any( an => (an.Tag != null) && an.Tag.Equals( mSelectionTag ) ) )
                return;

            var color = selectorColor ?? Colors.SkyBlue;
            var oxyColor = OxyColor.FromRgb( color.R, color.G, color.B );
            var range = new RectangleAnnotation {
                Fill = OxyColor.FromAColor( 120, oxyColor ),
                MinimumX = 0,
                MaximumX = 0,
                Tag = mSelectionTag
            };

            var startx = double.NaN;

            model.Annotations.Clear();
            model.Annotations.Add( range );
            model.MouseDown += ( s, e ) => {
                if( e.ChangedButton == OxyMouseButton.Left ) {
                    startx = range.InverseTransform( e.Position ).X;
                    range.MinimumX = startx;
                    range.MaximumX = startx;
                    range.Text = "";

                    model.InvalidatePlot( true );
                    e.Handled = true;
                }
            };
            model.MouseMove += ( s, e ) => {
                if( !double.IsNaN( startx ) ) {
                    var x = range.InverseTransform( e.Position ).X;
                    range.MinimumX = Math.Min( x, startx );
                    range.MaximumX = Math.Max( x, startx );
                    if( selectAction != null )
                        selectAction( false, range.MinimumX, range.MaximumX );
                    model.InvalidatePlot( true );
                    e.Handled = true;
                }
            };

            model.MouseUp += ( s, e ) => {
                var x = range.InverseTransform( e.Position ).X;
                if( x == startx )
                    range.Text = "";
                if( selectAction != null )
                    selectAction( true, range.MinimumX, range.MaximumX );
                startx = double.NaN;
            };
        }

        public static void AutoScale( this PlotModel pm ) {
            foreach( var axis in pm.Axes ) {
                pm.AutoScale( axis );
            }
        }

        public static void AutoScale( this PlotModel pm, Axis axis ) {

            try {
                if( pm.Series.Any() == false ) return;
                var maxY = pm.Series.Where( series => series.IsVisible ).Select( series => {
                    var lineSeries = series as LineSeries;
                    var pt = lineSeries.GetPoints();
                    if( pt.Any() ) {
                        return pt.Max( dp => dp.Y );
                    }
                    return double.NaN;
                } ).Max();

                var minY = pm.Series.Where( series => series.IsVisible ).Select( series => {
                    var lineSeries = series as LineSeries;
                    var pt = lineSeries.GetPoints();
                    if( pt.Any() ) {
                        return pt.Min( dp => dp.Y );
                    }
                    return double.NaN;
                } ).Min();

                var maxX = pm.Series.Where( series => series.IsVisible ).Select( series => {
                    var lineSeries = series as LineSeries;
                    var pt = lineSeries.GetPoints();
                    if( pt.Any() ) {
                        return pt.Max( dp => dp.X );
                    }
                    return double.NaN;
                } ).Max();

                var minX = pm.Series.Where( series => series.IsVisible ).Select( series => {
                    var lineSeries = series as LineSeries;
                    var pt = lineSeries.GetPoints();
                    if( pt.Any() ) {
                        return pt.Min( dp => dp.X );
                    }
                    return double.NaN;
                } ).Min();

                // масштабирование с захватом нуля
                //if( minY > 0 ) {
                //    if( axis.Position == AxisPosition.Left || axis.Position == AxisPosition.Right )
                //        axis.Minimum = 0;
                //} else if( maxY < 0 ) {
                //    if( axis.Position == AxisPosition.Left || axis.Position == AxisPosition.Right )
                //        axis.Maximum = 0;
                //}
                //if( minX > 0 ) {
                //    if( axis.Position == AxisPosition.Top || axis.Position == AxisPosition.Bottom )
                //        axis.Minimum = 0;
                //} else if( maxX < 0 ) {
                //    if( axis.Position == AxisPosition.Top || axis.Position == AxisPosition.Bottom )
                //        axis.Maximum = 0;
                //}

                if( axis.Position == AxisPosition.Top || axis.Position == AxisPosition.Bottom ) {
                    axis.Minimum = minX;
                    axis.Maximum = maxX;
                }
                if( axis.Position == AxisPosition.Left || axis.Position == AxisPosition.Right ) {
                    axis.Minimum = minY;
                    axis.Maximum = maxY;
                }
                axis.Reset();
            } catch( Exception  ) {
                return;
            }
        }

        public static R CopySimilarObj<R, T>( T obj )
            where T : new()
            where R : new() {
            var result = new R();
            var srcProp = obj.GetType().GetProperties();
            var destProp = result.GetType().GetProperties();
            foreach( var piSrc in srcProp ) {
                if( piSrc.CanRead && piSrc.CanWrite ) {
                    var piDest = destProp.FirstOrDefault( pi => pi.Name.Equals( piSrc.Name ) );
                    if( piDest != null ) {
                        if( piDest.CanWrite ) {
                            if( piDest.PropertyType == piSrc.PropertyType ) {
                                piDest.SetValue( result, piSrc.GetValue( obj, null ), null );
                            } else if( piDest.PropertyType == typeof( Color ) && piSrc.PropertyType == typeof( OxyColor ) ) {
                                var color = ((OxyColor) piSrc.GetValue( obj, null )).GetColorMedia();
                                piDest.SetValue( result, color, null );
                            } else if( piDest.PropertyType == typeof( OxyColor ) &&
                                       piSrc.PropertyType == typeof( Color ) ) {
                                var color = ((Color) piSrc.GetValue( obj, null )).GetColorOxy();
                                piDest.SetValue( result, color, null );
                            }
                        }
                    }
                }
            }
            return result;
        }

        public static void CreateModel_1( this PlotModel model, string title = "" ) {
            model.Axes.Add( new LinearAxis() {
                Position = AxisPosition.Left,
                IsZoomEnabled = false,
                IsPanEnabled = false
            } );
            model.Axes.Add( new LinearAxis() {
                Position = AxisPosition.Bottom,
                IsZoomEnabled = true,
                IsPanEnabled = true
            } );
        }

        public static PlotModel CreateModelByView( this OxyPlot.Wpf.PlotView pv ) {
            var model = new PlotModel();
            model = CopySimilarObj<PlotModel, OxyPlot.Wpf.PlotView>( pv );
            foreach( var axes in pv.Axes ) {
                if( axes.GetType() == typeof( OxyPlot.Wpf.LinearAxis ) ) {
                    var newAxis = CopySimilarObj<LinearAxis, OxyPlot.Wpf.LinearAxis>( axes as OxyPlot.Wpf.LinearAxis );
                    model.Axes.Add( newAxis );
                }
            }
            return model;
        }

        public static void CreateSpectrumAllIn( this PlotModel model, string title = "" ) {
            model.TitleFontSize = 10;
            if( !string.IsNullOrEmpty( title ) ) model.Title = title;
            model.Axes.Add( new LinearAxis() {
                Position = AxisPosition.Left,
                IsZoomEnabled = false,
                IsPanEnabled = false
            } );
            model.Axes.Add( new LinearAxis() {
                Position = AxisPosition.Bottom,
                IsZoomEnabled = true,
                IsPanEnabled = true
            } );
        }

        public static System.Drawing.Color GetColorDrawing( this OxyColor color ) {
            return System.Drawing.Color.FromArgb( color.A, color.R, color.G, color.B );
        }

        public static System.Drawing.Color GetColorDrawing( this Color color ) {
            return System.Drawing.Color.FromArgb( color.A, color.R, color.G, color.B );
        }

        public static Color GetColorMedia( this OxyColor oxyColor ) {
            return Color.FromArgb( oxyColor.A, oxyColor.R, oxyColor.G, oxyColor.B );
        }

        public static Color GetColorMedia( this System.Drawing.Color color ) {
            return Color.FromArgb( color.A, color.R, color.G, color.B );
        }

        public static OxyColor GetColorOxy( this Color color ) {
            return OxyColor.FromArgb( color.A, color.R, color.G, color.B );
        }

        public static OxyColor GetColorOxy( this System.Drawing.Color color ) {
            return OxyColor.FromArgb( color.A, color.R, color.G, color.B );
        }

        public static IEnumerable<DataPoint> GetPoints( this Series series ) {
            var lineSeries = series as DataPointSeries;
            if( lineSeries == null ) return null;
            var points1 = lineSeries.Points;
            if( points1.IsNullOrEmpty() ) {
                var points2 = lineSeries.ItemsSource.Cast<DataPoint>();
                if( points2.IsNullOrEmpty() == false ) {
                    return points2;
                }
            }
            return points1;
        }

        public static void SetLine( this ElementCollection<Series> seriesCollection, IList<DataPoint> data,
            string title = "" ) {
            if( seriesCollection == null )
                return;
            if( data != null && data.Count > 0 ) {
                var series = SetLineSeries( data, title );
                seriesCollection.Add( series );
            }
        }

        public static LineSeries SetLineSeries( IList<DataPoint> data, string title = "", int thickness = 1,
            Color? color = null ) {
            if( data != null && data.Count > 0 ) {
                var series = new LineSeries {
                    Smooth = false,
                    Title = title,
                    StrokeThickness = thickness,
                    LineStyle = LineStyle.Solid,
                    MinimumSegmentLength = 10,
                    MarkerSize = 3,
                    MarkerStroke = OxyColors.ForestGreen,
                    //MarkerType = MarkerType.Plus
                };
                series.Points.AddRange( data );
                if( color != null ) series.Color = color.Value.GetColorOxy();


                return series;
            }
            return null;
        }

        public static void SetSelection( this PlotModel model, double min, double max ) {
            var range = model.Annotations.First( an => (an.Tag != null) && an.Tag.Equals( mSelectionTag ) )
                as OxyPlot.Annotations.RectangleAnnotation;
            if( range == null )
                return;
            range.MinimumX = min;
            range.MaximumX = max;
        }
    }
}