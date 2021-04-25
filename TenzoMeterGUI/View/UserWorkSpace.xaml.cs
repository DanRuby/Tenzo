using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using OxyPlot;
using tEngine.Helpers;
using tEngine.TMeter.DataModel;
using Measurement = tEngine.TMeter.DataModel.Measurement;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Threading;
using tEngine.DataModel;
using tEngine.Markers;
using tEngine.MVVM;
using tEngine.UControls;
using MessageBox = System.Windows.MessageBox;

namespace TenzoMeterGUI.View
{
    /// <summary>
    /// Interaction logic for UserWorkSpace.xaml
    /// </summary>
    public partial class UserWorkSpace : Window
    {
        private UserWorkSpaceVM mDataContext;

        public Guid ID => mDataContext.User.ID;

        public UserWorkSpace()
        {
            InitializeComponent();
            WindowManager.UpdateWindowPos(GetType().Name, this);
            mDataContext = new UserWorkSpaceVM() { Parent = this };
            DataContext = mDataContext;
        }

        public void OpenMsm(Measurement msm) => mDataContext.OpenMsm(msm);

        public void CopyUserInfo(User user)
        {
            Cloner.CopyAllProperties(mDataContext.User, user);
            mDataContext.UpdateAllProperties();
        }
        public void SetUser(User user) => mDataContext.User = user;

        public void UpdateAllProperties() => mDataContext.UpdateAllProperties();

        private void PlotViewEx2_OnLoaded(object sender, RoutedEventArgs e)
        {
            PlotViewEx2.Clear();
            List<DataPoint> data = Enumerable.Range(0, 100).Select(i => new DataPoint(i, Math.Sqrt(i))).ToList();
            PlotViewEx2.AddLineSeries(data, color: null, thickness: 2);
            PlotViewEx2.ReDraw();
        }

        private void Window_OnClosing(object sender, CancelEventArgs e)
        {
            if (mDataContext != null)
            {
                mDataContext.PreClosed();
                if (mDataContext.IsNotSaveChanges)
                {
                    MessageBoxResult answer = MessageBox.Show(string.Format("Имеются несохраненные изменения в: {0}. Сохранить?",
                       mDataContext.User.UserLong()),"Предупреждение",
                       MessageBoxButton.YesNoCancel, MessageBoxImage.Warning, MessageBoxResult.Cancel);

                    if (answer == MessageBoxResult.Cancel)
                        e.Cancel = true;
                    else mDataContext.CMDExit.DoExecute(answer);
                }    
            }
            WindowManager.SaveWindowPos(GetType().Name, this);
            GC.Collect();
        }
    }

    public class UserWorkSpaceVM : Observed<UserWorkSpaceVM>
    {
        private const int MINIMUM_POINTS = 100;
        private readonly Guid mNewMsmWindowID = Guid.NewGuid();
        private readonly Guid mResultWindowID = Guid.NewGuid();
        private ObservableCollection<DataToShow> mDataToShowList;
        private Dictionary<object, PlotViewEx> mGraphics = new Dictionary<object, PlotViewEx>();
        private bool mIsBusy = false;
        private Measurement mSelectedMsm;
        private int mSelectedMsmIndex;
        private User mUser;
        public Command CMDCreateTextFile { get; private set; }
        public Command CMDEditSelectedMsm { get; private set; }
        public Command CMDExit { get; private set; }
        public Command CMDExportCSV { get; private set; }
        public Command CMDFocusChanged { get; private set; }
        public Command CMDNewMsm { get; private set; }
        public Command CMDOxyLoad { get; private set; }
        public Command CMDOxyPanelLoaded { get; private set; }
        public Command CMDOxyUnload { get; private set; }
        public Command CMDRemoveSelectedMsm { get; private set; }
        public Command CMDResetMsmList { get; private set; }
        public Command CMDResultShow { get; private set; }
        public Command CMDSaveOpenUser { get; private set; }
        public Command CMDShowMarkers { get; private set; }
        public Command CMDShowMarkersSettings { get; private set; }
        public Command CMDSpectrumCalc { get; private set; }
        public Command CMDUpdateOxy { get; private set; }

        public UserWorkSpaceVM()
        {
            CMDExportCSV = new Command(CMDExportCSV_Func);
            CMDResultShow = new Command(CMDResultShow_Func);
            CMDOxyPanelLoaded = new Command(CMDOxyPanelLoaded_Func);
            CMDCreateTextFile = new Command(CMDCreateTextFile_Func);
            CMDNewMsm = new Command(CMDNewMsm_Func);
            CMDEditSelectedMsm = new Command(CMDEditSelectedMsm_Func);
            CMDResetMsmList = new Command(CMDResetMsmList_Func);
            CMDRemoveSelectedMsm = new Command(CMDRemoveSelectedMsm_Func);
            CMDSaveOpenUser = new Command(CMDSaveOpenUser_Func);
            CMDExit = new Command(CMDExit_Func);
            CMDShowMarkers = new Command(CMDShowMarkers_Func);
            CMDShowMarkersSettings = new Command(CMDShowMarkersSettings_Func);
            CMDFocusChanged = new Command(CMDFocusChanged_Func);
            CMDSpectrumCalc = new Command(CMDSpectrumCalc_Func);
            CMDOxyLoad = new Command(CMDOxyLoad_Func);
            CMDOxyUnload = new Command(CMDOxyUnload_Func);
            CMDUpdateOxy = new Command(CMDUpdateOxy_Func);

            if (IsDesignMode)
            {
                User = User.GetTestUser();
            }
        }

        public ObservableCollection<DataToShow> DataToShowList
        {
            get
            {
                if (mDataToShowList == null && IsMsm)
                {
                    mDataToShowList = new ObservableCollection<DataToShow>() {
                        new DataToShow( "Минимальное", "Минимальное прилагаемое усилие, без учета тремора",
                            SelectedMsm.Data.Left.Constant.Min, SelectedMsm.Data.Right.Constant.Min ),
                        new DataToShow( "Среднее", "Среднее прилагаемое усилие",
                            SelectedMsm.Data.Left.Constant.Mean, SelectedMsm.Data.Right.Constant.Mean ),
                        new DataToShow( "Максимальное", "Максимальное прилагаемое усилие, без учета тремора",
                            SelectedMsm.Data.Left.Constant.Max, SelectedMsm.Data.Right.Constant.Max ),
                        new DataToShow( "Тремор", "Диапазон присутствующего тремора",
                            SelectedMsm.Data.GetTremorAmplitude( Hands.Left ),
                            SelectedMsm.Data.GetTremorAmplitude( Hands.Right ) ),
                        //new DataToShow( "Тремор, %", "Доля тремора относительно среднего усилия",
                        //    SelectedMsm.Data.GetTremorPercent( Hands.Left ),
                        //    SelectedMsm.Data.GetTremorPercent( Hands.Right ) ),
                    };
                }
                return mDataToShowList;
            }
        }

        // для отладки
        public int IndexInList => Msms.IndexOf(SelectedMsm);

        public bool IsBusy
        {
            get => mIsBusy;
            set
            {
                mIsBusy = value;
                NotifyPropertyChanged(m => m.IsBusy);
            }
        }

        public new bool IsDesignMode => true;

        public bool IsMarkersShow => Markers.WindowNotNull;

        public bool IsMsm => SelectedMsm != null;

        public bool IsNotSaveChanges
        {
            get => User.IsNotSaveChanges;
            set
            {
                User.IsNotSaveChanges = value;
                NotifyPropertyChanged(m => m.IsNotSaveChanges);
                NotifyPropertyChanged(m => m.WindowTitle);
            }
        }

        public ObservableCollection<Measurement> Msms => User.Msms;

        public bool SelectByIndex { get; set; }

        public Measurement SelectedMsm
        {
            get => mSelectedMsm;
            set
            {
                mSelectedMsm = value;
                SelectByIndex = false;
                UpdateSelectedMsm();

                NotifyPropertyChanged(m => m.SelectedMsm);
                NotifyPropertyChanged(m => m.IsMsm);

                if (mSelectedMsm != null)
                {
                    mDataToShowList = null;
                    NotifyPropertyChanged(m => m.DataToShowList);
                }

                NotifyPropertyChanged(m => m.IndexInList);
                CMDUpdateOxy_Func();
            }
        }

        public int SelectedMsmIndex
        {
            get => mSelectedMsmIndex;
            set
            {
                mSelectedMsmIndex = value;
                SelectByIndex = true;
                UpdateSelectedMsm();
            }
        }

        public User User
        {
            get => mUser;
            set
            {
                mUser = value;
                Debug.Assert(mUser != null);


                IsNotSaveChanges = false;
                SelectedMsm = Msms.FirstOrDefault();
                UpdateAllProperties();
            }
        }

        public string WindowTitle => User.UserLong() + (IsNotSaveChanges ? "*" : "");



        public void OpenMsm(Measurement msm)
        {
            if (msm == null) return;
            ResetList(msm);
            IsNotSaveChanges = false;
            //throw new NotImplementedException();
        }

        public void PreClosed()
        {
            MeasurementData.CancelCalc();
        }

        public int R2Pixels(int percent)
        {
            MeasurementData data = SelectedMsm.Data;

            int max = data.Count;
            int min = (int)(max / 10.0 > MINIMUM_POINTS ? MINIMUM_POINTS : max / 10.0);
            int k = (max - min) / (100 - 1);
            double b = min - k * 1.0;

            double result = percent * k + b;
            return (int)result;
        }

        private void CMDCreateTextFile_Func()
        {
            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.DefaultExt = ".txt";
                sfd.Filter = "*.txt|*.txt";
                sfd.InitialDirectory = Directory.GetCurrentDirectory();
                sfd.FileName = "text_adc";
                if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    double[] adc1 = SelectedMsm.Data.GetConst(Hands.Left).Select(dp => dp.Y).ToArray();
                    double[] adc2 = SelectedMsm.Data.GetTremor(Hands.Left).Select(dp => dp.Y).ToArray();
                    double[] adc3 = SelectedMsm.Data.GetConst(Hands.Right).Select(dp => dp.Y).ToArray();
                    double[] adc4 = SelectedMsm.Data.GetTremor(Hands.Right).Select(dp => dp.Y).ToArray();
                    StringBuilder sb = new StringBuilder();
                    sb.Append("ацп1\tацп2\tацп3\tацп4" + Environment.NewLine);
                    for (int i = 0; i < adc1.Count(); i++)
                    {
                        //sb.Append( string.Format( "{0}\t{1}\t{2}\t{3}", adc1[i],adc2[i],adc3[i],adc4[i]) + Environment.NewLine);
                        sb.Append(adc1[i] + "\t" + adc2[i] + "\t" + adc3[i] + "\t" + adc4[i] + Environment.NewLine);
                    }
                    using (StreamWriter sw = new StreamWriter(sfd.FileName))
                    {
                        sw.Write(sb);
                    }
                    System.Windows.Forms.MessageBox.Show("Файл записан!" + Environment.NewLine + sfd.FileName);
                }
            }
        }

        private void CMDEditSelectedMsm_Func()
        {
            DisableBaseButtons(true);
            MsmCreator wnd = new MsmCreator();
            wnd.SetMsm(SelectedMsm);
            wnd.Closing += MsmCreator_OnClosing;
            wnd.Topmost = true;
            wnd.Show();
        }

        private async void CMDExit_Func(object param)
        {
                MessageBoxResult answer = (MessageBoxResult)param;
                if (answer == MessageBoxResult.Yes)
                {
                    CMDSaveOpenUser_Func();
                }
                else 
                {
                    IsBusy = true;
                    await Task.Factory.StartNew(() => { User.Restore(); });
                User.NotifyMeasurmentChanged();
                    IsBusy = false;
                }
        }

            private void CMDExportCSV_Func()
        {
            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.DefaultExt = "*.csv";
                sfd.Filter = "*.csv|*.csv";
                if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    try
                    {
                        SelectedMsm.Msm2CSV(sfd.FileName);
                        MessageBox.Show("Измерение записано в файл " + sfd.FileName, "Сообщение", MessageBoxButton.OK);
                    }
                    catch (Exception)
                    {
                        MessageBox.Show("Ошибка записи файла", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void CMDFocusChanged_Func()
        {
            NotifyPropertyChanged(m => m.IsMarkersShow);
        }

        private void CMDNewMsm_Func()
        {
            DisableBaseButtons(true);

            MsmCreator wnd = WindowManager.GetWindow<MsmCreator>(mNewMsmWindowID) ??
                      WindowManager.NewWindow<MsmCreator>(mNewMsmWindowID);


            wnd.Closing += MsmCreator_OnClosing;
            wnd.Topmost = true;
            wnd.Show();

            //if( wnd.ShowDialog() == true ) {
            //    if( wnd.Result != null ) {
            //        var msm = wnd.Result;
            //        User.AddMsm( msm );
            //        ResetList( msm );
            //    }
            //}
        }

        private void CMDOxyLoad_Func(object param)
        {
            if (param is PlotViewEx == false) return;
            PlotViewEx pve = (PlotViewEx)param;
            if (mGraphics.ContainsKey(pve.Tag))
            {
                mGraphics.Remove(pve.Tag);
            }
            mGraphics.Add(pve.Tag, pve);
        }

        private void CMDOxyPanelLoaded_Func()
        {
            // данная функция необходима только при первой загрузке
            CMDOxyPanelLoaded.CanExecute = false;
            CMDUpdateOxy_Func();
        }

        private void CMDOxyUnload_Func(object param)
        {
            if (param is PlotViewEx == false) return;
            PlotViewEx pve = (PlotViewEx)param;
            if (pve.Tag != null)
                if (mGraphics.ContainsKey(pve.Tag))
                {
                    mGraphics.Remove(pve.Tag);
                }
        }

        private void CMDRemoveSelectedMsm_Func()
        {
            Debug.Assert(SelectedMsm != null);
            if (IsMsm)
            {
                MessageBoxResult answer =
                    MessageBox.Show(
                        string.Format(
                            "Измерение \"{0}\" будет удалено вместе со всей измеренной информацией . Удалить измерение?",
                            SelectedMsm.Title), "Предупреждение",
                        MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No);
                if (answer == MessageBoxResult.Yes)
                {
                    int index = SelectedMsmIndex;
                    User.RemoveMsm(SelectedMsm);
                    ResetList(index);
                    IsNotSaveChanges = true;
                }
            }
        }

        private void CMDResetMsmList_Func()
        {
            ResetList(SelectedMsm);
        }

        private void CMDResultShow_Func()
        {
            ResultWindow wnd = WindowManager.GetWindow<ResultWindow>(mResultWindowID);
            if (wnd == null)
            {
                wnd = WindowManager.NewWindow<ResultWindow>(mResultWindowID);
                wnd.SetMsmCollection(User.Msms);
            }
            wnd.Show();
        }

        private void CMDSaveOpenUser_Func()
        {
            User.SaveDefaultPath();
            IsNotSaveChanges = false;
        }

        private void CMDShowMarkers_Func()
        {
            if (!Markers.WindowNotNull)
            {
                Markers wnd = Markers.GetWindow();
                wnd.Closed += (sender, args) => { NotifyPropertyChanged(m => m.IsMarkersShow); };
                wnd.Show();
            }
            else
            {
                Markers.CloseWindow();
            }
        }

        private void CMDShowMarkersSettings_Func()
        {
            MarkersSet ms = new MarkersSet();
            if (ms.ShowDialog() == true)
            {
                bool needClose = !Markers.WindowNotNull;

                Markers wnd = Markers.GetWindow();
                wnd.UpdateSettings();
                wnd.ReDraw();
                if (needClose) wnd.FreeWindow();
            }
        }

        private void CMDSpectrumCalc_Func()
        {
            Measurement first = User.Msms.First(msm => SelectedMsm.ID.Equals(msm.ID));
            if (first == null) 
                return;
            IsBusy = true;
            first.Data.SpectrumAnalys(null, (data, b) =>
            {
                Parent.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                {
                    //MessageBox.Show( "Анализ завершен!" );
                    ResetList(first);
                    CMDUpdateOxy_Func();
                    IsBusy = false;
                    IsNotSaveChanges = true;
                }));
            }, true);
            MeasurementData.StartCalc();
        }

        private Task CMDSpectrumCalc_Func_Async()
        {
            Measurement first = User.Msms.First(msm => SelectedMsm.ID.Equals(msm.ID));
            if (first == null) return null;
            IsBusy = true;
            Task task = Task.Factory.StartNew(() =>
            {
                bool exit = false;
                first.Data.SpectrumAnalys(null, (data, b) => { exit = true; }, true);
                MeasurementData.StartCalc();
                while (exit == false)
                {
                    Thread.Sleep(10);
                }
                Parent.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
                {
                    ResetList(first);
                    CMDUpdateOxy_Func();
                    IsBusy = false;
                }));
            });

            return task;
        }

        private void CMDUpdateOxy_Func()
        {
            List<PlotViewEx> list = mGraphics.Select(kv => kv.Value).ToList();
            foreach (PlotViewEx value in list)
            {
                OxyDraw(value);
            }
        }

        private void DisableBaseButtons(bool b)
        {
            CMDNewMsm.CanExecute = !b;
            CMDEditSelectedMsm.CanExecute = !b;
            CMDRemoveSelectedMsm.CanExecute = !b;
            CMDExit.CanExecute = !b;
        }

        private async void MsmCreator_OnClosing(object sender, CancelEventArgs cancelEventArgs)
        {
            IsBusy = true;
            MsmCreator wnd = sender as MsmCreator;
            if (wnd.NotDialogButResult == true)
            {
                if (wnd.Result != null)
                {
                    if (SelectedMsm != null && SelectedMsm.ID.Equals(wnd.Result.ID))
                    {
                        Cloner.CopyAllFields(SelectedMsm, wnd.Result);
                        Cloner.CopyAllProperties(SelectedMsm, wnd.Result);
                        SelectedMsm.SetData(wnd.Result.Data);
                    }
                    else
                    {
                        User.AddMsm(wnd.Result);
                    }
                    IsNotSaveChanges = true;
                    SelectedMsm = wnd.Result;
                    await CMDSpectrumCalc_Func_Async();

                    wnd.PostSave();
                    wnd.PostScript();
                }
            }
            DisableBaseButtons(false);
            IsBusy = false;
        }

        private async void OxyDraw(PlotViewEx pve)
        {
            string command = pve.Tag as string;
            if (command == null) return;

            await Task.Factory.StartNew(() =>
            {
                pve.Clear();

                MeasurementData data = (SelectedMsm != null) ? SelectedMsm.Data : new MeasurementData();

                if (command.Equals("all_tremor"))
                {
                    int thickness = 2;
                    Color? color = null;
                    pve.AddLineSeries(data.GetTremor(Hands.Left).GetPartPercent(100), color: color,
                        thickness: thickness);
                    pve.AddLineSeries(data.GetTremor(Hands.Right).GetPartPercent(100), color: color,
                        thickness: thickness);
                }
                else if (command.Equals("all_const"))
                {
                    int thickness = 2;
                    Color? color = null;
                    pve.AddLineSeries(data.GetConst(Hands.Left).GetPartPercent(100), color: color,
                        thickness: thickness);
                    pve.AddLineSeries(data.GetConst(Hands.Right).GetPartPercent(100), color: color,
                        thickness: thickness);
                }
                else if (command.Equals("all_spectrum"))
                {
                    int thickness = 2;
                    Color? color = null;
                    pve.AddLineSeries(data.GetSpectrum(Hands.Left).GetPartPercent(100), color: color,
                        thickness: thickness);
                    pve.AddLineSeries(data.GetSpectrum(Hands.Right).GetPartPercent(100), color: color,
                        thickness: thickness);
                }
                else if (command.Equals("all_corr"))
                {
                    int thickness = 2;
                    Color? color = null;
                    pve.AddLineSeries(data.GetCorrelation(Hands.Left).GetPartPercent(100), color: color,
                        thickness: thickness);
                    pve.AddLineSeries(data.GetCorrelation(Hands.Right).GetPartPercent(100), color: color,
                        thickness: thickness);
                }
            });
            pve.ReDraw();
        }

        private void ResetList(int selectIndex = 0)
        {
            UpdateAllProperties();

            int toSelect = selectIndex;
            if (selectIndex >= Msms.Count)
            {
                toSelect = (Msms.Count - 1);
            }
            SelectedMsmIndex = toSelect;
        }

        private void ResetList(Measurement selectItem = null)
        {
            UpdateAllProperties();

            if (selectItem == null)
            {
                SelectedMsm = Msms.FirstOrDefault();
            }
            else
            {
                SelectedMsm = Msms.FirstOrDefault(msm => msm.ID.Equals(selectItem.ID));
            }
        }

        private async void UpdateSelectedMsm()
        {
            IsBusy = true;
            await Task<object>.Factory.StartNew(() =>
            {
                NotifyPropertyChanged(m => m.SelectByIndex);
                NotifyPropertyChanged(m => m.SelectedMsmIndex);
                return null;
            });
            IsBusy = false;
        }

        public class DataToShow
        {
            public string Title { get; set; }
            public string ToolTip { get; set; }
            public double V1 { get; set; }
            public double V2 { get; set; }

            public DataToShow(string title, string toolTip, double v1, double v2)
            {
                Title = title;
                ToolTip = toolTip;
                V1 = v1;
                V2 = v2;
            }
        }
    }

}