using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using OxyPlot;
using tEngine.DataModel;
using tEngine.Helpers;
using tEngine.MVVM;
using tEngine.PlotCreator;
using tEngine.TMeter.DataModel;
using tEngine.UControls;
using Button = System.Windows.Controls.Button;
using CheckBox = System.Windows.Controls.CheckBox;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using UIElement = System.Windows.UIElement;

namespace TenzoMeterGUI.View
{
    /// <summary>
    ///     Interaction logic for ResultWindow.xaml
    /// </summary>
    public partial class ResultWindow : Window
    {
        private readonly ResultWindowVM mDataContext;

        public ResultWindow()
        {
            InitializeComponent();
            WindowManager.UpdateWindowPos(GetType().Name, this);
            mDataContext = new ResultWindowVM { Parent = this };
            DataContext = mDataContext;
        }

        public void SetMsmCollection(IList<Measurement> msms)
        {
            mDataContext.SetMsmCollection(msms);
        }

        private void ColorSelect_OnClick(object sender, RoutedEventArgs e)
        {
            mDataContext.CMDColorSelect.DoExecute((sender as Button));
        }

        private void OxyListItem_OnLoaded(object sender, RoutedEventArgs e)
        {
            mDataContext.CMDOxyLoad.DoExecute(sender);
        }

        private void OxyListItem_OnUnloaded(object sender, RoutedEventArgs e)
        {
            mDataContext.CMDOxyUnload.DoExecute(sender);
        }

        private void PlotsPanel_OnLoaded(object sender, RoutedEventArgs e)
        {
            mDataContext.CMDOnLoad.DoExecute(null);
        }

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
                catch (Exception)
                {
                    // Debug.Assert( false, ex.Message );
                }
            }
            WindowManager.SaveWindowPos(GetType().Name, this);
        }
    }

    public enum EShowHand
    {
        Both,
        Left,
        Right
    }

    public enum EShowMode
    {
        Const,
        Tremor,
        Spectrum,
        Correlation
    }

    public class ResultWindowVM : Observed<ResultWindowVM>
    {
        private readonly Dictionary<object, PlotViewEx> mGraphics = new Dictionary<object, PlotViewEx>();
        private readonly List<Measurement> mMsms = new List<Measurement>();
        private readonly bool mNeedReDraw = true;
        private readonly PlotSetResult[] mSettings = new PlotSetResult[Enum.GetNames(typeof(EShowMode)).Length];
        private bool mIsBusy;
        private ObservableCollection<MsmIndex> mMsmsIndex;
        private EShowHand mShowHand;
        private EShowMode mShowMode;

        public bool AutoScale
        {
            get { return mSettings[(int)ShowMode].AutoScale; }
            set
            {
                mSettings[(int)ShowMode].AutoScale = value;
                NotifyPropertyChanged(m => m.AutoScale);
            }
        }

        public Command CMDButton { get; private set; }
        public Command CMDColorSelect { get; private set; }
        public Command CMDCreatePdf { get; private set; }
        public Command CMDOnLoad { get; private set; }
        public Command CMDOptimizeScale { get; private set; }
        public Command CMDOxyLoad { get; private set; }
        public Command CMDOxyUnload { get; private set; }
        public Command CMDResetScale { get; private set; }
        public Command CMDSetShow { get; private set; }

        public bool IsBusy
        {
            get { return mIsBusy; }
            set
            {
                mIsBusy = value;
                NotifyPropertyChanged(m => m.IsBusy);
            }
        }

        public Visibility LeftShow
        {
            get
            {
                return (ShowHand == EShowHand.Left || ShowHand == EShowHand.Both)
                    ? Visibility.Visible
                    : Visibility.Collapsed;
            }
        }

        public ObservableCollection<MsmIndex> Msms
        {
            get
            {
                if (mMsmsIndex == null)
                    mMsmsIndex =
                        new ObservableCollection<MsmIndex>(mMsms.Select((msm, i) => new MsmIndex(msm, i + 1)));
                return mMsmsIndex;
            }
        }

        public ObservableCollection<MsmIndex> MsmsToDraw
        {
            get { return new ObservableCollection<MsmIndex>(Msms.Where(msm => msm.IsShow)); }
        }

        public bool Normalize
        {
            get { return mSettings[(int)ShowMode].Normalize; }
            set
            {
                mSettings[(int)ShowMode].Normalize = value;
                NotifyPropertyChanged(m => m.Normalize);
            }
        }

        public PlotSetResult PlotSet
        {
            get { return mSettings[(int)ShowMode]; }
            set { mSettings[(int)ShowMode] = value; }
        }

        public string PlotType
        {
            get
            {
                switch (ShowMode)
                {
                    case EShowMode.Const:
                        return "Произвольное усилие";
                    case EShowMode.Tremor:
                        return "Тремор";
                    case EShowMode.Spectrum:
                        return "Спектральная характеристика";
                    case EShowMode.Correlation:
                        return "АКФ";
                }
                return "";
            }
        }

        public Visibility RightShow
        {
            get
            {
                return (ShowHand == EShowHand.Right || ShowHand == EShowHand.Both)
                    ? Visibility.Visible
                    : Visibility.Collapsed;
            }
        }

        public EShowHand ShowHand
        {
            get { return mShowHand; }
            set
            {
                mShowHand = value;
                // OxyDraw();
                NotifyPropertyChanged(m => m.ShowHand);
            }
        }

        public EShowMode ShowMode
        {
            get { return mShowMode; }
            set
            {
                mShowMode = value;
                //UpdateAllProperties();
                NotifyPropertyChanged(m => m.ShowMode);
                NotifyPropertyChanged(m => m.PlotSet);
                NotifyPropertyChanged(m => m.AutoScale);
                NotifyPropertyChanged(m => m.Normalize);
                //if( mNeedReDraw ) {
                //    OxyDraw();
                //}
            }
        }

        public ResultWindowVM()
        {
            for (int i = 0; i < Enum.GetNames(typeof(EShowMode)).Length; i++)
            {
                mSettings[i] = new PlotSetResult();
                mSettings[i].AxesOX.AutoScale = false;
                mSettings[i].AxesOX.DecimalCount = 1;

                mSettings[i].AxesOY.AutoScale = false;
                mSettings[i].AxesOX.DecimalCount = 1;

                mSettings[i].AutoScale = true;
                mSettings[i].Normalize = false;
            }

            CMDCreatePdf = new Command(CMDCreatePdf_Func);
            CMDSetShow = new Command(CMDSetShow_Func);
            CMDOnLoad = new Command(CMDOnLoad_Func);
            CMDOptimizeScale = new Command(CMDOptimizeScale_Func);
            CMDResetScale = new Command(CMDResetScale_Func);
            CMDColorSelect = new Command(CMDColorSelect_Func);
            CMDOxyLoad = new Command(CMDOxyLoad_Func);
            CMDOxyUnload = new Command(CMDOxyUnLoad_Func);
            CMDButton = new Command(CMDButton_Func);

            if (IsDesignMode)
            {
                SetMsmCollection(new[]
                {Measurement.GetTestMsm( title: "тест1" ), Measurement.GetTestMsm( title: "тест2" ), Measurement.GetTestMsm( title: "тест3" )});
                mMsms.ForEach(msm => msm.Data.BaseAnalys(null, null));
                TData.StartCalc();
            }
        }

        public void CreatePDF()
        {
            if (Msms.IsNullOrEmpty()) return;
            User user = Msms.First().GetOwner();

            PdfSave wnd = WindowManager.NewWindow<PdfSave>();
            wnd.SetPrintData(user, MsmsToDraw.Cast<Measurement>().ToList(), mSettings.Select(psr =>
            {
                PlotSetResult result = new PlotSetResult(psr);
                return result;
            }).ToList());
            wnd.ShowDialog();
        }

        public void SetMsmCollection(IList<Measurement> msms)
        {
            mMsms.AddRange(msms);
            NotifyPropertyChanged(m => m.Msms);
        }

        private async void CMDButton_Func() => OxyDraw();

        /// <summary>
        ///     Принимает саму кнопку
        /// </summary>
        /// <param name="param"></param>
        private void CMDColorSelect_Func(object param)
        {
            Button bt = param as Button;
            ColorDialog cd = new ColorDialog
            {
                Color = ((SolidColorBrush)bt.Background).Color.GetColorDrawing()
            };
            if (cd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Color newColor = cd.Color.GetColorMedia();
                bt.Background = new SolidColorBrush(newColor);
            }
        }

        private void CMDCreatePdf_Func()
        {
            CreatePDF();
        }

        /// <summary>
        ///     запускается при загрузке всех графиков
        /// </summary>
        private void CMDOnLoad_Func()
        {
            NotifyPropertyChanged(m => m.MsmsToDraw);
            if (mNeedReDraw)
            {
                OxyDraw();
            }
        }

        /// <summary>
        ///     Установка одного масштаба на все графики.
        /// </summary>
        private async void CMDOptimizeScale_Func()
        {
            IsBusy = true;
            await ReSetPlots();
            await Task.Factory.StartNew(() =>
            {
                List<PlotViewEx> plots = mGraphics.Select(kv => kv.Value).ToList();
                plots.ForEach(pve =>
                {
                    //pve.PlotModel.AcceptSettings( PlotSet );
                    pve.PlotModel.AutoScale();
                });
                PlotExtension.SynchScale(plots.Select(pve => (PlotModel)pve.PlotModel).ToArray());
                // todo убрать индексы
                OxyPlot.Axes.Axis axesY = plots.First().PlotModel.Axes[0];
                OxyPlot.Axes.Axis axesX = plots.First().PlotModel.Axes[1];
                PlotSet.AxesOX.Minimum = axesX.Minimum;
                PlotSet.AxesOX.Maximum = axesX.Maximum;
                PlotSet.AxesOY.Minimum = axesY.Minimum;
                PlotSet.AxesOY.Maximum = axesY.Maximum;

                NotifyPropertyChanged(m => m.PlotSet);
                NotifyPropertyChanged(m => m.PlotSet.AxesOX);
                NotifyPropertyChanged(m => m.PlotSet.AxesOY);
            });
            foreach (KeyValuePair<object, PlotViewEx> kv in mGraphics)
            {
                kv.Value.ReDraw(false);
            }
            IsBusy = false;
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

        private void CMDOxyUnLoad_Func(object param)
        {
            if (param is PlotViewEx == false) return;
            PlotViewEx pve = (PlotViewEx)param;
            if (pve.Tag != null)
                if (mGraphics.ContainsKey(pve.Tag))
                {
                    mGraphics.Remove(pve.Tag);
                }
        }

        /// <summary>
        ///     Сброс масштабов всех графиков
        /// </summary>
        private async void CMDResetScale_Func()
        {
            IsBusy = true;
            await ReSetPlots();
            await Task.Factory.StartNew(() =>
            {
                List<PlotViewEx> plots = mGraphics.Select(kv => kv.Value).ToList();
                for (int i = 0; i < plots.Count; i += 2)
                {
                    PlotViewEx left = plots[i];
                    PlotViewEx right = plots[i + 1];
                    //left.PlotModel.AcceptSettings( PlotSet );
                    //right.PlotModel.AcceptSettings( PlotSet );

                    left.PlotModel.AutoScale();
                    right.PlotModel.AutoScale();
                    PlotExtension.SynchScale(left.PlotModel, right.PlotModel);
                }
            });
            foreach (KeyValuePair<object, PlotViewEx> kv in mGraphics)
            {
                kv.Value.ReDraw(false);
            }
            IsBusy = false;
        }

        private void CMDSetShow_Func(object param)
        {
            if (param is CheckBox)
            {
                CheckBox cb = (CheckBox)param;
                cb.IsChecked = !cb.IsChecked;
            }
            NotifyPropertyChanged(m => m.Msms);
        }

        private async void OxyDraw()
        {
            IsBusy = true;
            await ReSetPlots();
            if (PlotSet.AutoScale)
            {
                await Task.Factory.StartNew(() =>
                {
                    List<PlotViewEx> plots = mGraphics.Select(kv => kv.Value).ToList();
                    plots.ForEach(pve =>
                    {
                        //pve.PlotModel.AcceptSettings( PlotSet );
                        pve.PlotModel.AutoScale();
                    });
                    PlotExtension.SynchScale(plots.Select(pve => (PlotModel)pve.PlotModel).ToArray());

                    //// todo убрать индексы
                    //var axesY = plots.First().PlotModel.Axes[0];
                    //var axesX = plots.First().PlotModel.Axes[1];
                    //PlotSet.AxesOX.Minimum = axesX.Minimum;
                    //PlotSet.AxesOX.Maximum = axesX.Maximum;
                    //PlotSet.AxesOY.Minimum = axesY.Minimum;
                    //PlotSet.AxesOY.Maximum = axesY.Maximum;
                });
            }

            foreach (KeyValuePair<object, PlotViewEx> kv in mGraphics)
            {
                kv.Value.ReDraw(false);
            }
            NotifyPropertyChanged(m => m.PlotType);
            IsBusy = false;
        }

        private Task ReSetPlots()
        {
            return Task.Factory.StartNew(() =>
            {
                if (mGraphics.Count < 1) return;
                foreach (KeyValuePair<object, PlotViewEx> kv in mGraphics)
                {
                    // получение графика(pve), измерения(msm), руки(hand) и информации для отображения(data)
                    PlotViewEx pve = kv.Value;
                    string tag = kv.Key.ToString();
                    Hands hand = tag.StartsWith("left_") ? Hands.Left : Hands.Right;
                    string idString = tag.Substring(hand == Hands.Left ? "left_".Length : "right_".Length);
                    Guid id;
                    //Debug.Assert( Guid.TryParse( idString, out id ) );
                    Guid.TryParse(idString, out id);
                    MsmIndex msm = Msms.First(m => m.ID.Equals(id));

                    IList<DataPoint> data = new List<DataPoint>();

                    switch (ShowMode)
                    {
                        case EShowMode.Const:
                            data = msm.Data.GetConst(hand);
                            break;
                        case EShowMode.Tremor:
                            data = msm.Data.GetTremor(hand);
                            break;
                        case EShowMode.Spectrum:
                            data = msm.Data.GetSpectrum(hand);
                            break;
                        case EShowMode.Correlation:
                            data = msm.Data.GetCorrelation(hand);
                            break;
                    }
                    pve.Clear();
                    if (PlotSet.Normalize)
                    {
                        data = data.Normalized();
                    }
                    if (data.IsNullOrEmpty())
                        data = new[] { new DataPoint(0, 0) };
                    pve.AddLineSeries(data, thickness: 2);

                    //// правая рука без подписей по ОУ
                    //if( hand == Hands.Right ) {
                    //    //PlotSet.AxesOY.ShowNumbers = false;
                    //} else {
                    //    PlotSet.AxesOY.ShowNumbers = true;
                    //}
                    //// по ОХ подписи только на нижнем графике
                    //if( i++ < mGraphics.Count - 2 ) {
                    //    //PlotSet.AxesOX.ShowNumbers = false;
                    //} else {
                    //    PlotSet.AxesOX.ShowNumbers = true;
                    //}
                    pve.PlotModel.AcceptSettings(PlotSet);
                }
            });
        }

        public class PlotSetResult : PlotSet
        {
            public bool AutoScale { get; set; }
            public bool Normalize { get; set; }

            public PlotSetResult(PlotSet ps)
            {
                Cloner.CopyAllProperties(this, ps);
            }

            public PlotSetResult()
            {
                AutoScale = true;
                Normalize = false;
            }
        }

        public class MsmIndex : Measurement
        {
            public int Index { get; set; }
            public bool IsShow { get; set; }

            public MsmIndex(Measurement msm, int index)
            {
                Copy(msm);

                Index = index;
                IsShow = false;
            }
        }
    }
}