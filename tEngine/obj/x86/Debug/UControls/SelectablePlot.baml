   M S B A M L   `   `   `  ����E  AtEngine, Version=1.0.17.616, Culture=neutral, PublicKeyToken=null5clr-namespace:tEngine.UControlstEngine.UControls  5      &     tEngine.UControls.SelectablePlot   5      \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5      R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      W SOxyPlot.Wpf, Version=2014.1.546.0, Culture=neutral, PublicKeyToken=75e952ba404cdbb0 oxyhttp://oxyplot.org/wpf  5      /	uControlsclr-namespace:tEngine.UControls  5      -   5          >�Name$
  root��5
      .��5      �  � 	Resources 5   
   ��%    5       � RowDefinitions 5   
   �� 5      $��* �6   �� 5      $��* �6   �� 5      $
��Auto �6   .+�5   
   � $��2��6   .+�5      �� .+�5      #   tEngine.UControls.RangeSlider  -   5          Name$ Slider��5   (      Minimum$ 0Q�5         Maximum 5      ��  �� ElementName$
 root�� �� Path$ Maximum� �� Mode$ OneWay=�	   
LowerValue	 5      �� $
 root��$ 
BeginPoint�$ TwoWay=�
   
UpperValue
 5      �� $
 root��$ EndPoint�$ TwoWay=�5      5         OxyPlot.Wpf.PlotView  5!   
   -   5          Name$ 	constPlot��5!   $   $��0��6      Axes 5"        OxyPlot.Wpf.LinearAxis  5#        OxyPlot.Wpf.Axis   Position$
 Left=�6!      IsZoomEnabled .     IsPanEnabled .     IsAxisVisible .  6\     5$      $ Bottom=�6!    .   .   .  6^      Series. 5&        OxyPlot.Wpf.LineSeries     StrokeThickness$ 3Q�6Z   ��6   �� $
 root��$ LeftHandConst�6Z     5'      $ 3Q�6[   ��6   �� $
 root��$ RightHandConst�6[   5(        5)   
   -   5        $ 
tremorPlot��5)   $   $��1��6    5*        5+      $
 Left=�6!    .   .   .  6\     5,      $ Bottom=�6!    .   .   .  6^   . 5.        $ 3Q�6[   ��6   �� $
 root��$ LeftHandTremor�6[     5/      $ 3Q�6\   ��6   �� $
 root��$ RightHandTremor�6\   50      51      52      