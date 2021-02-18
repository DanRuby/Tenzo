using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using tEngine.Helpers;
using tEngine.TActual.DataModel;
using TenzoActualGUI.ViewModel;

// ReSharper disable InconsistentNaming

namespace TenzoActualGUI.View
{
    /// <summary>
    /// Interaction logic for MsmMaster.xaml
    /// </summary>
    public partial class MsmMaster : Window
    {
        private readonly MsmMasterVM mDataContext;

        private readonly DispatcherTimer mResizeTimer = new DispatcherTimer
        {
            Interval = new TimeSpan(0, 0, 0, 0, 500),
            IsEnabled = false
        };

        public bool LoadWithOpen = false;

        public MsmMaster()
        {
            AppSettings.Init(AppSettings.Project.Actual);

            InitializeComponent();
            mResizeTimer.Tick += ResizeTimerOnTick;
            WindowManager.UpdateWindowPos(GetType().Name, this);
            mDataContext = new MsmMasterVM(this);
            DataContext = mDataContext;
        }

        public void SetMsm(Measurement msm)
        {
            mDataContext.SetMsm(msm);
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
            CheckBox cb = sender as CheckBox;
            if (cb != null)
            {
                cb.IsChecked = !cb.IsChecked;
            }
        }

        private void MsmMaster_OnLoaded(object sender, RoutedEventArgs e)
        {
            if (LoadWithOpen)
            {
                if (mDataContext.OpenMsmW() == false)
                    Close();
            }
        }

        private void ResizeTimerOnTick(object sender, EventArgs eventArgs)
        {
            mResizeTimer.IsEnabled = false;
            mDataContext.DCSlidesResult.IsResize = false;
            mDataContext.DCSlidesResult.UpdateAllProperties();
        }

        private void Window_OnClosing(object sender, CancelEventArgs e)
        {
            if (mDataContext != null)
            {
                if (mDataContext.PreClosed() != true)
                {
                    e.Cancel = true;
                    return;
                }
                try
                {
                    DialogResult = mDataContext.DialogResult;
                }
                catch
                {
                    /*если окно не диалог - вылетит исключение, ну и пусть*/
                }
            }
            WindowManager.SaveWindowPos(GetType().Name, this);
        }

        private void Window_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            mResizeTimer.IsEnabled = true;
            mResizeTimer.Stop();
            mResizeTimer.Start();
            mDataContext.DCSlidesResult.IsResize = true;
            mDataContext.DCSlidesResult.UpdateAllProperties();
        }
    }
}