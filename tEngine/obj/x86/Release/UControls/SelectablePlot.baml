   M S B A M L   `   `   `  ���� E  AtEngine, Version=1.0.17.616, Culture=neutral, PublicKeyToken=null5clr-namespace:tEngine.UControlstEngine.UControls  &     tEngine.UControls.SelectablePlot   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     @d2http://schemas.microsoft.com/expression/blend/2008     W SOxyPlot.Wpf, Version=2014.1.546.0, Culture=neutral, PublicKeyToken=75e952ba404cdbb0 oxyhttp://oxyplot.org/wpf  /	uControlsclr-namespace:tEngine.UControls  -     >�Name$
  root��.���  � 	Resources ��%     � RowDefinitions �� $��* ��� $��* ��� $
��Auto �.+�� $��2��.+��� .+�#   tEngine.UControls.RangeSlider  -     Name$ Slider��   Minimum$ 0Q�   Maximum ��  �� ElementName$
 root�� �� Path$ Maximum� �� Mode$ OneWay=�	   
LowerValue	 �� $
 root��$ 
BeginPoint�$ TwoWay=�
   
UpperValue
 �� $
 root��$ EndPoint�$ TwoWay=�  OxyPlot.Wpf.PlotView  -     Name$ 	constPlot��$��0��   Axes   OxyPlot.Wpf.LinearAxis    OxyPlot.Wpf.Axis   Position$
 Left=�   IsZoomEnabled .     IsPanEnabled .     IsAxisVisible .    $ Bottom=� .   .   .     Series.   OxyPlot.Wpf.LineSeries     StrokeThickness$ 3Q����� $
 root��$ LeftHandConst�  $ 3Q����� $
 root��$ RightHandConst�  -   $ 
tremorPlot��$��1��   $
 Left=� .   .   .    $ Bottom=� .   .   .  .   $ 3Q����� $
 root��$ LeftHandTremor�  $ 3Q����� $
 root��$ RightHandTremor�