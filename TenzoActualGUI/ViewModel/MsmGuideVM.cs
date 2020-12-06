using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using tEngine.DataModel;
using tEngine.Helpers;
using tEngine.Markers;
using tEngine.MVVM;
using tEngine.TActual.DataModel;
using TenzoActualGUI.View;

namespace TenzoActualGUI.ViewModel {
    /// <summary>
    /// отвечает за проведение измерения: запись и показ картинок
    /// оперирует объектом Msm
    /// </summary>
    public class MsmGuideVM : Observed<MsmGuideVM> {
        private bool mIsRun;
        private DispatcherTimer mTimerProgress;
        public Command CMDMarkersSettings { get; private set; }
        public Command CMDShowCancel { get; private set; }
        public Command CMDShowSlideWindow { get; private set; }
        public Command CMDStartMeasurement { get; private set; }
        public Slide CurrentSlide { get; set; }
        public int CurrentSlideIndex { get; private set; }
        public string CurrentTime { get; private set; }

        public bool IsPauseBeforeStart {
            get { return AppSettings.GetValue( "IsPauseBeforeStart", false ); }
            set {
                AppSettings.SetValue( "IsPauseBeforeStart", value );
                NotifyPropertyChanged( m => m.IsPauseBeforeStart );
            }
        }

        public bool IsPlayList {
            get { return PlayList != null; }
        }

        public bool IsRun {
            get { return mIsRun; }
            set {
                mIsRun = value;
                NotifyPropertyChanged( m => m.IsRun );
            }
        }

        public bool IsSlidesShow {
            get { return SlideShow.IsWindowCreate(); }
            set {
                if( value ) {
                    var wnd = SlideShow.GetWindow();
                    wnd.PreClosed = () => NotifyPropertyChanged( m => m.IsSlidesShow );
                    wnd.Show();
                } else {
                    SlideShow.CloseAll();
                }
                NotifyPropertyChanged( m => m.IsSlidesShow );
            }
        }

        public Msm Msm { get; set; }
        public double Progress { get; private set; }

        public double SecondsToSlide {
            get { return IsPlayList ? PlayList.SecondsToSlide : 0; }
            set {
                if( IsPlayList ) PlayList.SecondsToSlide = value;
                NotifyPropertyChanged( m => m.SecondsToSlide );
            }
        }

        public int SlideCount {
            get { return (IsPlayList) ? PlayList.Slides.Count : 0; }
        }

        private PlayList PlayList {
            get { return Msm == null ? null : Msm.PlayList; }
            set { if( Msm != null ) Msm.PlayList = value; }
        }

        public MsmGuideVM() {
            CMDShowSlideWindow = new Command( ShowSlideWindow );
            CMDStartMeasurement = new Command( StartMeasurement );
            CMDShowCancel = new Command( ShowCancel );
            CMDMarkersSettings = new Command( MarkersSettings );
        }

        public void PreClosed() {
            ShowCancel();
            SlideShow.CloseAll();
        }

        /// <summary>
        /// вызывается при смене слайда (не привязано к началу поступления данных для слайда)
        /// </summary>
        private void CurrentSlideCallBack( Guid guid ) {
            if( !IsPlayList ) return;
            CurrentSlide = PlayList.Slides.FirstOrDefault( slide => slide.Id.Equals( guid ) );
            NotifyPropertyChanged( m => m.CurrentSlide );

            CurrentSlideIndex++;
            NotifyPropertyChanged( m => m.CurrentSlideIndex );
        }

        /// <summary>
        /// Вызывается по завершении показа слайдов
        /// </summary>
        private void FinalCallBack( bool withError ) {
            if( IsRun == true ) {
                Parent.Dispatcher.BeginInvoke( DispatcherPriority.Normal, new Action( () => {
                    if( SlideShow.IsWindowCreate() ) {
                        var wnd = SlideShow.GetWindow();
                        //wnd.Dispatcher.BeginInvoke( new Action( wnd.Close ) );
                        if( wnd != null ) wnd.ShowCancel();
                    }
                } ) );
                IsRun = false;
                if( !withError && IsPlayList ) {
                    foreach( var slide in PlayList.Slides ) {
                        slide.Data.BaseAnalys( null, ( data, b ) => { UpdateAllProperties(); } );
                    }
                    var cts = new CancellationTokenSource();
                    TData.AddTaskFinal( new Task( ( param ) => {
                        Parent.Dispatcher.BeginInvoke( DispatcherPriority.Normal, new Action( () => {
                            if( Parent.DataContext is MsmMasterVM )
                                ((MsmMasterVM) (Parent.DataContext)).UpdateAllProperties();
                            UpdateAllProperties();
                        } ) );
                    }, cts.Token, cts.Token ), cts );
                    TData.StartCalc();
                }
                mTimerProgress.Stop();

                CurrentTime =
                    new TimeSpan( 0, 0, 0, 0, (int) (SlideCount*PlayList.SecondsToSlide*1000.0) ).ToString(
                        @"mm\:ss\.ff" );
                Progress = 100;
                NotifyPropertyChanged( m => m.Progress );

                var msgBoxSuccess = new Action( () => {
                    MessageBox.Show( "Измерения завершены!", "Информация", MessageBoxButton.OK,
                        MessageBoxImage.Information );
                } );

                var msgBoxError = new Action( () => {
                    MessageBox.Show( "Измерения были прерваны", "Ошибка", MessageBoxButton.OK,
                        MessageBoxImage.Error );
                } );

                Parent.Dispatcher.BeginInvoke( DispatcherPriority.Normal, withError ? msgBoxError : msgBoxSuccess );

                Parent.Dispatcher.BeginInvoke( DispatcherPriority.Normal,
                    new Action( () => {
                        if( Parent.DataContext is MsmMasterVM ) {
                            //((MsmMasterVM) (Parent.DataContext)).DCSlidesAnalyser.CMDSpectrumAnalys.DoExecute( null );
                            ((MsmMasterVM) (Parent.DataContext)).UpdateAllProperties();
                        }
                    } ) );
            }
        }

        private void MarkersSettings() {
            var ms = new MarkersSet();
            if( ms.ShowDialog() == true ) {
                SlideShow.GetWindow().UpdateSettings();
            }
        }

        private void ShowCancel() {
            FinalCallBack( true );
        }

        private void ShowSlideWindow() {
            IsSlidesShow = true;
        }

        private void StartMeasurement() {
            Debug.Assert( IsPlayList, "пропал плейлист" );
            if( IsRun == false ) {
                IsRun = true;

                foreach( var slide in Msm.PlayList.Slides ) {
                    slide.Data.ClearData();
                }
                if( IsPauseBeforeStart ) {
                    Msm.CreateTime = (DateTime.Now + new TimeSpan( 0, 0, 0, 10 ));
                } else {
                    Msm.CreateTime = DateTime.Now;
                }
                CurrentSlideIndex = 0;
                CurrentSlide = null;
                CurrentTime = "00:00.00";

                var wnd = SlideShow.GetWindow();
                wnd.PreClosed = () => {
                    NotifyPropertyChanged( m => m.IsSlidesShow );
                };
                wnd.Show();
                wnd.StartShow( PlayList, CurrentSlideCallBack, FinalCallBack, Msm.CreateTime );
                    

                mTimerProgress = new DispatcherTimer();
                mTimerProgress.Tick += TimerProgressOnTick;
                mTimerProgress.Interval = new TimeSpan( 0, 0, 0, 0, 1000/10 ); //скорость обновления
                mTimerProgress.Start();

                UpdateAllProperties();
                PlayList.IsNotSaveChanges = true;
            }
        }

        private void TimerProgressOnTick( object sender, EventArgs e ) {
            if( !IsPlayList ) return;
            double maxTime = SlideCount*PlayList.SecondsToSlide;
            var time = DateTime.Now - Msm.CreateTime;
            var seconds = time.TotalMilliseconds/1000.0;
            if( seconds > maxTime ) {
                time = new TimeSpan( 0, 0, 0, 0, (int) (maxTime*1000.0) );
            }

            CurrentTime = time.ToString( @"mm\:ss\.ff" );
            Progress = (100.0/maxTime)*seconds;
            NotifyPropertyChanged( m => m.Progress );
            NotifyPropertyChanged( m => m.CurrentTime );
        }
    }
}