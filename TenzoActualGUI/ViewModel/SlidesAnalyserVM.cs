using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using tEngine.DataModel;
using tEngine.MVVM;
using tEngine.TActual.DataModel;

namespace TenzoActualGUI.ViewModel {
    /// <summary>
    /// оперирует объектом PlayList
    /// </summary>
    public class SlidesAnalyserVM : Observed<SlidesAnalyserVM> {
        private List<Guid> mSpectrumTasks = new List<Guid>();
        public Command CMDCancelAnalys { get; private set; }
        public Command CMDSpectrumAnalys { get; private set; }
        public Command CMDUpdate { get; private set; }

        public bool IsBusyNow {
            get { return Parent.DataContext is MsmMasterVM && ((MsmMasterVM) Parent.DataContext).IsBusyNow; }
            set {
                if( Parent.DataContext is MsmMasterVM )
                    ((MsmMasterVM) Parent.DataContext).IsBusyNow = value;
            }
        }

        public PlayList PlayList { get; set; }

        public ObservableCollection<Slide> Slides {
            get { return new ObservableCollection<Slide>( PlayList.Slides ); }
        }

        public SlidesAnalyserVM() {
            PlayList = new PlayList();

            CMDUpdate = new Command( Update );
            CMDCancelAnalys = new Command( CancelAnalys );
            CMDSpectrumAnalys = new Command( SpectrumAnalys );
        }

        private void CancelAnalys() {
            //mSpectrumTasks.ForEach( guid => {
            //    var cts = TData.GetCancellationTokenSource( guid );
            //    if( cts != null )
            //        cts.Cancel();
            //} );
            //mSpectrumTasks.Clear();
            TData.CancelCalc();
        }

        private void SpectrumAnalys() {
            //foreach ( var slide in PlayList.Slides ) {
            //    if ( slide.IsBaseReady ) {
            //        var guid = slide.Data.SpectrumAnalys( null, ( data, b ) => Update() );
            //        mSpectrumTasks.Add( guid );
            //    }
            //}
            //mSpectrumTasks.Add( TData.AddFinal( () => {
            //    PlayList.RareCalculate();
            //    Update();
            //} ) );

            foreach( var slide in PlayList.Slides ) {
                if( slide.IsBaseReady ) {
                    slide.Data.SpectrumAnalys( null, ( data, b ) => Update(), corr: true );
                }
            }
            var cts = new CancellationTokenSource();
            TData.AddTaskFinal( new Task( ( param ) => {
                PlayList.RareCalculate();
                Parent.Dispatcher.BeginInvoke( DispatcherPriority.Normal, new Action( () => {
                    IsBusyNow = false;
                    if( Parent.DataContext is MsmMasterVM )
                        ((MsmMasterVM) (Parent.DataContext)).UpdateAllProperties();
                    UpdateAllProperties();
                } ) );
            }, cts.Token, cts.Token ), cts );
            IsBusyNow = true;
            TData.StartCalc();

            PlayList.IsNotSaveChanges = true;
        }

        private void Update() {
            NotifyPropertyChanged( m => m.Slides );
        }
    }
}