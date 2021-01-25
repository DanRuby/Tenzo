using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Threading;
using tEngine.Helpers;
using tEngine.MVVM;
using tEngine.TActual;
using tEngine.TActual.DataModel;
using MessageBox = System.Windows.MessageBox;

namespace TenzoActualGUI.ViewModel
{
    public class MsmMasterVM : Observed<MsmMasterVM>
    {
        private MasterState mCurrentState = 0;
        private bool mIsBusyNow = false;
        private Task<Measurement> mTaskOpenFile;
        public Command CMDExit { get; private set; }
        public Command CMDNewMsm { get; private set; }
        public Command CMDNextStep { get; private set; }
        public Command CMDOpenMsm { get; private set; }
        public Command CMDOpenTab { get; private set; }
        public Command CMDPrevStep { get; private set; }
        public Command CMDSaveMsm { get; private set; }
        public Command CMDSaveMsmAs { get; private set; }
        public MsmInfoVM DCInfo { get; private set; }
        public MsmGuideVM DCMsmGuide { get; private set; }
        public SlideCreatorVM DCSlides { get; private set; }
        public SlidesAnalyserVM DCSlidesAnalyser { get; private set; }
        public SlidesResultVM DCSlidesResult { get; private set; }

        public bool IsBusyNow
        {
            get { return mIsBusyNow; }
            set
            {
                mIsBusyNow = value;
                NotifyPropertyChanged(m => m.IsBusyNow);
            }
        }

        public bool IsDebugConfig
        {
#if DEBUG
            get { return true; }
#else
            get { return false; }
#endif
        }

        public bool IsDebugMode
        {
            get { return AppSettings.GetValue("IsDebug", true); }
            set { AppSettings.SetValue("IsDebug", value); }
        }

        public bool IsNotFirst
        {
            get { return (CurrentState != 0); }
        }

        public bool IsNotLast
        {
            get
            {
                int max = Enum.GetValues(typeof(MasterState)).Cast<MasterState>().Distinct().Count() - 1;
                return ((int)CurrentState != max);
            }
        }

        public bool IsNotSaveChanges
        {
            get { return Msm.PlayList.IsNotSaveChanges; }
            set
            {
                Msm.PlayList.IsNotSaveChanges = value;
                NotifyPropertyChanged(m => m.WindowTitle);
            }
        }

        public int TabSelect
        {
            get { return (int)mCurrentState; }
            set { SelectTab(value); }
        }

        public string WindowTitle
        {
            get { return Msm.FileName + Constants.MSM_EXT + (IsNotSaveChanges ? "*" : ""); }
        }

        private MasterState CurrentState
        {
            get { return mCurrentState; }
            set
            {
                mCurrentState = value;
                NotifyPropertyChanged(m => m.TabSelect);
            }
        }

        public Measurement Msm { get; set; }

        public MsmMasterVM()
        {
            Init(null);
        }

        public MsmMasterVM(Window parent)
        {
            Init(parent);
        }

        public bool IDM
        {
            get { return IsDesignMode; }
        }

        public string Location
        {
            get { return System.Reflection.Assembly.GetExecutingAssembly().Location; }
        }
        public bool PreClosed()
        {
            if (IsNotSaveChanges)
            {
                MessageBoxResult result = MessageBox.Show("Имеются несохраненные изменения, сохранить?", "Предупреждение",
                    MessageBoxButton.YesNoCancel, MessageBoxImage.Asterisk);
                if (result == MessageBoxResult.Yes)
                    SaveMsm();
                if (result == MessageBoxResult.Cancel)
                    return false;
            }

            DCMsmGuide.PreClosed();
            return true;
        }

        public void SetMsm(Measurement msm)
        {
            Msm = msm;
            DCInfo.Msm = Msm;
            DCSlides.PlayList = Msm.PlayList;
            DCMsmGuide.Msm = Msm;
            DCSlidesAnalyser.PlayList = Msm.PlayList;
            DCSlidesResult.PlayList = Msm.PlayList;

            SelectTab(0);
            UpdateAllProperties();
        }

        public void TestMode(int? maxCount = null)
        {
            Measurement msm = Measurement.CreateTestMsm(maxCount);
            SetMsm(msm);
        }

        private void Exit()
        {
            Parent.Close();
        }

        private void Init(Window parent)
        {
            CMDNextStep = new Command(NextStep);
            CMDPrevStep = new Command(PrevStep);
            CMDOpenTab = new Command(OpenTab);

            CMDNewMsm = new Command(NewMsm);
            CMDOpenMsm = new Command(OpenMsm);
            CMDSaveMsm = new Command(SaveMsm);
            CMDSaveMsmAs = new Command(SaveMsmAs);
            CMDExit = new Command(Exit);

            Parent = parent;
            DCInfo = new MsmInfoVM() { Parent = parent };
            DCMsmGuide = new MsmGuideVM() { Parent = parent };

            DCSlides = new SlideCreatorVM() { Parent = parent };
            DCSlidesAnalyser = new SlidesAnalyserVM() { Parent = parent };
            DCSlidesResult = new SlidesResultVM() { Parent = parent };

            Measurement msm = new Measurement();
            if (IsDesignMode)
            {
                msm = Measurement.CreateTestMsm(5);
            }
            SetMsm(msm);
        }

        private void NewMsm()
        {
            if (IsNotSaveChanges)
            {
                MessageBoxResult result = MessageBox.Show("Имеются несохраненные изменения, сохранить?", "Предупреждение",
                    MessageBoxButton.YesNoCancel, MessageBoxImage.Asterisk);
                if (result == MessageBoxResult.Yes)
                    SaveMsm();
                if (result == MessageBoxResult.Cancel)
                    return;
            }
            Measurement msm = new Measurement();
            SetMsm(msm);
            SelectTab(0);
            UpdateAllProperties();
        }

        private void NextStep()
        {
            int state = (int)CurrentState + 1;
            SelectTab(state);
        }

        private void OpenMsm()
        {
            OpenMsmW();
        }

        //открытие с ожиданием
        public bool OpenMsmW()
        {
            if (IsNotSaveChanges)
            {
                MessageBoxResult result = MessageBox.Show("Имеются несохраненные изменения, сохранить?", "Предупреждение",
                    MessageBoxButton.YesNoCancel, MessageBoxImage.Asterisk);
                if (result == MessageBoxResult.Yes)
                    SaveMsm();
                if (result == MessageBoxResult.Cancel)
                    return false;
            }
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = string.Format("*{0}|*{0}", Constants.MSM_EXT);
            ofd.RestoreDirectory = true;
            string initPath = AppSettings.GetValue(Measurement.FOLDER_KEY, Constants.AppDataFolder);
            ofd.InitialDirectory = initPath + @"\";
            if (ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                IsBusyNow = true;
                bool result = false;
                Measurement msm;
                mTaskOpenFile = Task<Measurement>.Factory.StartNew(() =>
                {
                    result = Measurement.Open(ofd.FileName, out msm);
                    return msm;
                }).ContinueWith(task =>
                {
                    Parent.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                        new Action(() =>
                        {
                            if (result)
                            {
                                SetMsm(task.Result);
                                IsNotSaveChanges = false;
                                UpdateAllProperties();
                            }
                            else
                            {
                                MessageBox.Show("Не удалось открыть файл", "Ошибка", MessageBoxButton.OK,
                                    MessageBoxImage.Error);
                            }
                            IsBusyNow = false;
                        }));
                    return task.Result;
                });
            }
            else
                return false;

            return true;
        }

        private void OpenTab(object param)
        {
            int tab = 0;
            if (int.TryParse(param as string, out tab))
            {
                SelectTab(tab);
            }
        }

        private void PrevStep()
        {
            int state = (int)CurrentState - 1;
            SelectTab(state);
        }

        private void SaveMsm()
        {
            if (Msm.FilePath == null)
                SaveMsmAs();
            else
                Msm.Save();
            IsNotSaveChanges = false;
        }

        private void SaveMsmAs()
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = string.Format("*{0}|*{0}", Constants.MSM_EXT);
            sfd.RestoreDirectory = true;
            string initPath = AppSettings.GetValue(Measurement.FOLDER_KEY, Constants.AppDataFolder);
            sfd.InitialDirectory = initPath + @"\";
            sfd.FileName = Msm.FileName;
            if (sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string filepath = sfd.FileName;
                FileInfo finfo = new FileInfo(filepath);
                if (finfo.Exists)
                {
                    if (
                        MessageBox.Show("Файл с таким именем уже существует.\r\nЗаменить?", "Предупреждение",
                            MessageBoxButton.YesNo, MessageBoxImage.Error) != MessageBoxResult.Yes)
                    {
                        return;
                    }
                }

                IsBusyNow = true;
                Msm.Save(filepath);
                IsNotSaveChanges = false;
                IsBusyNow = false;
            }
        }

        private void SelectTab(int index)
        {
            CurrentState = (MasterState)index;
            // обновление кнопок вперед/назад
            NotifyPropertyChanged(m => m.IsNotFirst);
            NotifyPropertyChanged(m => m.IsNotLast);
            NotifyPropertyChanged(m => m.TabSelect);

            NotifyPropertyChanged(m => m.WindowTitle);

            // обновление соответствующей вкладки
            if (CurrentState == MasterState.Info)
                NotifyPropertyChanged(m => m.DCInfo);
            if (CurrentState == MasterState.ImageSelect)
                NotifyPropertyChanged(m => m.DCSlides);
            if (CurrentState == MasterState.Msm)
                NotifyPropertyChanged(m => m.DCMsmGuide);
            if (CurrentState == MasterState.Analysis)
                NotifyPropertyChanged(m => m.DCSlidesAnalyser);
            if (CurrentState == MasterState.Result)
                NotifyPropertyChanged(m => m.DCSlidesResult);
        }

        private enum MasterState
        {
            Info = 0,
            ImageSelect = 1,
            Msm = 2,
            Analysis = 2,
            Result = 3
        }
    }
}