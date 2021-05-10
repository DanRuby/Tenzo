using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using tEngine.Helpers;
using tEngine.MVVM;
using UIElement = System.Windows.UIElement;

namespace tEngine.PlotCreator
{
    public enum EAxesStyle
    {
        Boxed,
        Cross,
        None
    }

    public enum ETitlePos
    {
        Top,
        Bottom
    }

    /// <summary>
    /// Interaction logic for PlotSettings.xaml
    /// </summary>
    public partial class PlotSettings : Window
    {
        private PlotSettingsVM mDataContext;

        public Action<PlotSet> AcceptSettingsAction
        {
            get => mDataContext.AcceptSettingsAction;
            set => mDataContext.AcceptSettingsAction = value;
        }

        public PlotSet PlotSet
        {
            get => mDataContext.PlotSet;
            set => mDataContext.PlotSet = value;
        }

        public PlotSettings()
        {
            InitializeComponent();
            WindowManager.UpdateWindowPos(GetType().Name, this);
            mDataContext = new PlotSettingsVM() { Parent = this };
            DataContext = mDataContext;
        }

        public void SetModel(PlotModelEx plotModel)
        {
            Debug.Assert(plotModel != null);
            mDataContext.PlotModel = plotModel;
            mDataContext.UpdateAllProperties();
        }

        private void ColorSelect_OnClick(object sender, RoutedEventArgs e) => mDataContext.CMDColorSelect.DoExecute(sender as Button);

        private void SimpleTextBox_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                UIElement element = sender as UIElement;
                if (element != null)
                {
                    element.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                }
            }
        }

        private void Window_OnClosing(object sender, CancelEventArgs e)
        {
            if (mDataContext != null)
            {
                try
                {
                    DialogResult = mDataContext.DialogResult;
                }
                catch (Exception ex)
                {
                    Debug.Assert(false, ex.Message);
                }
            }
            WindowManager.SaveWindowPos(GetType().Name, this);
        }
    }


    public class PlotSettingsVM : Observed<PlotSettingsVM>
    {

        private PlotModelEx mPlotModel;
        private PlotSet mPlotSet;
        public Action<PlotSet> AcceptSettingsAction { get; set; }
        public Command CMDAcceptButton { get; private set; }
        public Command CMDCancelButton { get; private set; }
        public Command CMDColorSelect { get; private set; }
        public Command CMDOkButton { get; private set; }

        public PlotSettingsVM()
        {
            CMDColorSelect = new Command(CMDColorSelect_Func);
            CMDOkButton = new Command(CMDOkButton_Func);
            CMDAcceptButton = new Command(CMDAcceptButton_Func);
            CMDCancelButton = new Command(CMDCancelButton_Func);

            PlotSet = PlotSet ?? new PlotSet();

            UpdateAllProperties();
        }

        public PlotModelEx PlotModel
        {
            get => mPlotModel;
            set
            {
                mPlotModel = value;
                PlotSet = new PlotSet(mPlotModel);
                NotifyPropertyChanged(m => m.PlotModel);
            }
        }

        public PlotSet PlotSet
        {
            get => mPlotSet;
            set
            {
                mPlotSet = value;
                NotifyPropertyChanged(m => m.PlotSet);
            }
        }



        private void CMDAcceptButton_Func() => AcceptSettingsAction?.Invoke(PlotSet);

        /// <summary>
        /// Принимает саму кнопку
        /// </summary>
        /// <param name="param"></param>
        private void CMDColorSelect_Func(object param)
        {
            Button bt = param as Button;
            System.Windows.Forms.ColorDialog cd = new System.Windows.Forms.ColorDialog()
            {
                Color = ((SolidColorBrush)bt.Background).Color.GetColorDrawing()
            };
            if (cd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Color newColor = cd.Color.GetColorMedia();
                bt.Background = new SolidColorBrush(newColor);
            }
        }

        private void CMDCancelButton_Func() => EndDialog(false);
        private void CMDOkButton_Func() => EndDialog(true);
    }
}