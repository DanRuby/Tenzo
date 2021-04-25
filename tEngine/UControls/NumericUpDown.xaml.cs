using System;
using System.Collections.Generic;
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
using OxyPlot.Wpf;
using tEngine.MVVM;
using tEngine.PlotCreator;
using UIElement = System.Windows.UIElement;

namespace tEngine.UControls
{
    /// <summary>
    /// Interaction logic for PlotViewEx.xaml
    /// </summary>
    public partial class NumericUpDown : UserControl
    {
        public static readonly DependencyProperty MaximumProperty = DependencyProperty.Register(
            "Maximum", typeof(int), typeof(NumericUpDown), new PropertyMetadata(100));

        public static readonly DependencyProperty MinimumProperty = DependencyProperty.Register(
            "Minimum", typeof(int), typeof(NumericUpDown), new PropertyMetadata(0));

        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
            "Value", typeof(int), typeof(NumericUpDown), new PropertyMetadata(0, (obj, args) =>
            {
                NumericUpDown numeric = (obj as NumericUpDown);
                if (numeric == null) return;
                if (numeric.IsLoaded)
                {
                    if ((int)args.NewValue > numeric.Maximum)
                    {
                        numeric.Value = numeric.Maximum;
                    }
                    else if ((int)args.NewValue < numeric.Minimum)
                    {
                        numeric.Value = numeric.Minimum;
                    }
                }
            }));

        public Command CMDDown { get; private set; }
        public Command CMDUp { get; private set; }

        public int Maximum
        {
            get => (int)GetValue(MaximumProperty);
            set => SetValue(MaximumProperty, value);
        }

        public int Minimum
        {
            get => (int)GetValue(MinimumProperty);
            set => SetValue(MinimumProperty, value);
        }


        public int Value
        {
            get => (int)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        public NumericUpDown()
        {
            CMDUp = new Command(CMDUp_Func);
            CMDDown = new Command(CMDDown_Func);

            InitializeComponent();
        }

        private void CMDDown_Func()
        {
            if (Value > Minimum)
                Value--;
        }

        private void CMDUp_Func()
        {
            if (Value < Maximum)
                Value++;
        }

        private void NumericUpDown_OnLoaded(object sender, RoutedEventArgs e)
        {
            if (Value > Maximum)
            {
                Value = Maximum;
            }
            else if (Value < Minimum)
            {
                Value = Minimum;
            }
        }
    }
}