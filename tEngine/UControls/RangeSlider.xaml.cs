﻿using System;
using System.Windows;
using System.Windows.Controls;

namespace tEngine.UControls {
    /// <summary>
    /// Interaction logic for RangeSlider.xaml
    /// </summary>
    public partial class RangeSlider : UserControl {
        public static readonly DependencyProperty LowerValueProperty =
            DependencyProperty.Register( "LowerValue", typeof( double ), typeof( RangeSlider ),
                new UIPropertyMetadata( 10.0 ) );

        public static readonly DependencyProperty MaximumProperty =
            DependencyProperty.Register( "Maximum", typeof( double ), typeof( RangeSlider ),
                new UIPropertyMetadata( 100.0 ) );

        public static readonly DependencyProperty MinimumProperty =
            DependencyProperty.Register( "Minimum", typeof( double ), typeof( RangeSlider ),
                new UIPropertyMetadata( 0.0 ) );

        public static readonly DependencyProperty UpperValueProperty =
            DependencyProperty.Register( "UpperValue", typeof( double ), typeof( RangeSlider ),
                new UIPropertyMetadata( 20.0 ) );

        public double LowerValue {
            get { return (double) GetValue( LowerValueProperty ); }
            set { SetValue( LowerValueProperty, value ); }
        }

        public double Maximum {
            get { return (double) GetValue( MaximumProperty ); }
            set { SetValue( MaximumProperty, value ); }
        }

        public double Minimum {
            get { return (double) GetValue( MinimumProperty ); }
            set { SetValue( MinimumProperty, value ); }
        }

        public double UpperValue {
            get { return (double) GetValue( UpperValueProperty ); }
            set { SetValue( UpperValueProperty, value ); }
        }

        public RangeSlider() {
            InitializeComponent();

            this.Loaded += Slider_Loaded;
        }

        private void LowerSlider_ValueChanged( object sender, RoutedPropertyChangedEventArgs<double> e ) {
            UpperSlider.Value = Math.Max( UpperSlider.Value, LowerSlider.Value );
        }

        private void Slider_Loaded( object sender, RoutedEventArgs e ) {
            LowerSlider.ValueChanged += LowerSlider_ValueChanged;
            UpperSlider.ValueChanged += UpperSlider_ValueChanged;
        }

        private void UpperSlider_ValueChanged( object sender, RoutedPropertyChangedEventArgs<double> e ) {
            LowerSlider.Value = Math.Min( UpperSlider.Value, LowerSlider.Value );
        }
    }
}