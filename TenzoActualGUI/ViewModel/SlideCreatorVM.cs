using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using tEngine.Helpers;
using tEngine.MVVM;
using tEngine.TActual;
using tEngine.TActual.DataModel;

namespace TenzoActualGUI.ViewModel {
    /// <summary>
    /// отвечает за подготовку набора картинок
    /// </summary>
    public class SlideCreatorVM : Observed<SlideCreatorVM> {
        private PlayList mPlayList;
        private Slide mSelectedSlide;
        private Slide mSelectedSlideEssential;
        private Slide mSelectedSlideInessential;
        private ReadOnlyObservableCollection<Slide> mSlides;
        public Command CMDAddImage { get; private set; }
        public Command CMDImageClick { get; private set; }
        public Command CMDMoveSelected { get; private set; }
        public Command CMDOpenPlayList { get; private set; }
        public Command CMDRemoveSlide { get; private set; }
        public Command CMDSavePlayList { get; private set; }
        public Command CMDSetSelect { get; private set; }
        public Command CMDToggleGrade { get; private set; }

        public bool IsBusyNow {
            get { return Parent.DataContext is MsmMasterVM && ((MsmMasterVM) Parent.DataContext).IsBusyNow; }
            set {
                if( Parent.DataContext is MsmMasterVM )
                    ((MsmMasterVM) Parent.DataContext).IsBusyNow = value;
            }
        }

        public bool IsImportant {
            get {
                if( SelectedSlide == null ) return false;
                return SelectedSlide.Grade == Slide.SlideGrade.Essential;
            }
            set {
                if( SelectedSlide != null ) {
                    SelectedSlide.Grade = value ? Slide.SlideGrade.Essential : Slide.SlideGrade.Inessential;
                    PlayList.IsNotSaveChanges = true;
                }

                NotifyPropertyChanged( m => m.Slides );
                NotifyPropertyChanged( m => m.SlidesEssential );
                NotifyPropertyChanged( m => m.SlidesInessential );
                NotifyPropertyChanged( m => m.IsImportant );
                NotifyPropertyChanged( m => m.SelectedSlide );
            }
        }

        public PlayList PlayList {
            get { return mPlayList; }
            set {
                mPlayList = value;
                SelectedSlide = mPlayList.Slides.FirstOrDefault() ?? new Slide();
            }
        }

        public Slide SelectedSlide {
            get { return mSelectedSlide; }
            set {
                mSelectedSlide = value;
                if( mSelectedSlide != null ) {
                    if( mSelectedSlide.Grade == Slide.SlideGrade.Essential ) {
                        mSelectedSlideEssential = mSelectedSlide;
                    } else {
                        mSelectedSlideInessential = mSelectedSlide;
                    }
                }
                NotifyPropertyChanged( m => m.SelectedSlideInessential );
                NotifyPropertyChanged( m => m.SelectedSlideEssential );
                NotifyPropertyChanged( m => m.SelectedSlide );
                NotifyPropertyChanged( m => m.IsImportant );
            }
        }

        public Slide SelectedSlideEssential {
            get { return mSelectedSlideEssential; }
            set {
                mSelectedSlideEssential = value;
                if( mSelectedSlideEssential != null ) mSelectedSlide = mSelectedSlideEssential;

                NotifyPropertyChanged( m => m.SelectedSlideInessential );
                NotifyPropertyChanged( m => m.SelectedSlideEssential );
                NotifyPropertyChanged( m => m.SelectedSlide );
                NotifyPropertyChanged( m => m.IsImportant );
                
            }
        }

        public int SelectedSlideIndex { get; set; }

        public Slide SelectedSlideInessential {
            get { return mSelectedSlideInessential; }
            set {
                mSelectedSlideInessential = value;
                if( mSelectedSlideInessential != null ) mSelectedSlide = mSelectedSlideInessential;
                NotifyPropertyChanged( m => m.SelectedSlideInessential );
                NotifyPropertyChanged( m => m.SelectedSlideEssential );
                NotifyPropertyChanged( m => m.SelectedSlide );
                NotifyPropertyChanged( m => m.IsImportant );
            }
        }

        public List<Slide> Slides {
            get { return new List<Slide>( (PlayList ?? new PlayList()).Slides ); }
        }

        public List<Slide> SlidesEssential {
            get {
                return
                    new List<Slide>(
                        ((PlayList ?? new PlayList()).Slides).Where( slide => slide.Grade == Slide.SlideGrade.Essential ) );
            }
        }

        public List<Slide> SlidesInessential {
            get {
                return
                    new List<Slide>(
                        ((PlayList ?? new PlayList()).Slides).Where(
                            slide => slide.Grade == Slide.SlideGrade.Inessential ) );
            }
        }

         
        public Command CMDMixSlides { get; private set; }

        private void CMDMixSlides_Func() {    
            if( PlayList == null ) return;
            PlayList.MixSlides();
            NotifyPropertyChanged( m => m.Slides );
            NotifyPropertyChanged( m => m.SlidesEssential );
            NotifyPropertyChanged( m => m.SlidesInessential );
        }
        public SlideCreatorVM() {
            try {
                CMDMixSlides = new Command( CMDMixSlides_Func );
                CMDRemoveSlide = new Command( CMDRemoveSlide_Func );
                CMDAddImage = new Command( CMDAddImage_Func );
                CMDImageClick = new Command( CMDImageClick_Func );
                CMDSavePlayList = new Command( CMDSavePlayList_Func );
                CMDMoveSelected = new Command( CMDMoveSelected_Func );
                CMDOpenPlayList = new Command( CMDOpenPlayList_Func );
                CMDToggleGrade = new Command( CMDToggleGrade_Func );
                CMDSetSelect = new Command( CMDSetSelect_Func );

                PlayList = new PlayList();

                SelectedSlideIndex = 0;
                UpdateAllProperties();
            } catch( Exception ex ) {
                Debug.Assert( false, ex.Message );
            }
        }
        

        public static string OpenDialog() {
            var ofd = new System.Windows.Forms.OpenFileDialog();
            ofd.Filter = string.Format( "*{0}|*{0}", Constants.PlayListExt );
            ofd.RestoreDirectory = true;
            var initPath = AppSettings.GetValue( PlayList.FOLDER_KEY, Constants.AppDataFolder );
            ofd.InitialDirectory = initPath + @"\";
            if( ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK ) {
                return ofd.FileName;
            }
            return null;
        }

        private void CMDAddImage_Func() {
            if( PlayList == null ) return;
            var filter = "";
            var codecs = ImageCodecInfo.GetImageEncoders().Select( info => info.FilenameExtension );
            var extensions = string.Join( ";", codecs.ToArray() );
            filter = "Image files | " + extensions;
            filter = String.Format( "{0}|{1} ({2})|{2}", filter, "All Files", "*.*" );

            var ofd = new System.Windows.Forms.OpenFileDialog {
                RestoreDirectory = true,
                Filter = filter,
                Multiselect = true
            };
            var initPath = AppSettings.GetValue( Slide.FOLDER_KEY, Constants.AppDataFolder );
            ofd.InitialDirectory = initPath + @"\";
            if( ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK ) {
                IsBusyNow = true;
                Task.Factory.StartNew( () => {
                    try {
                        foreach( var fileName in ofd.FileNames ) {
                            if( new FileInfo( fileName ).Exists ) {
                                var newSlide = new Slide();
                                newSlide.UriLoad( new Uri( fileName ) );
                                PlayList.AddSlide( newSlide );
                                AppSettings.SetValue( Slide.FOLDER_KEY, fileName.CutFileName() );
                            }
                        }
                    } catch( Exception ex ) {
                        Debug.Assert( false, ex.ToString() );
                    }
                } ).ContinueWith( task => {
                    Parent.Dispatcher.BeginInvoke( DispatcherPriority.Normal,
                        new Action( () => {
                            try {
                                SelectedSlide = Slides.FirstOrDefault();
                                NotifyPropertyChanged( m => m.Slides );
                                NotifyPropertyChanged( m => m.SlidesEssential );
                                NotifyPropertyChanged( m => m.SlidesInessential );
                                NotifyPropertyChanged( m => m.SelectedSlide );
                                IsBusyNow = false;
                            } catch( Exception ex ) {
                                Debug.Assert( false, ex.ToString() );
                            }
                        } ) );
                } );
            }
        }

        private void CMDImageClick_Func( object param ) {
            var i = 0;
        }

        private void CMDMoveSelected_Func( object param ) {
            if( PlayList == null ) return;
            var msg = param as string;
            if( msg != null ) {
                if( msg.Equals( "up" ) || msg.Equals( "left" )) {
                    PlayList.MoveSlide( SelectedSlide, PlayList.MoveDirection.Up );
                } else if( msg.Equals( "down" )  || msg.Equals( "right" )) {
                    PlayList.MoveSlide( SelectedSlide, PlayList.MoveDirection.Down );
                }
            }
            NotifyPropertyChanged( m => m.Slides );
            NotifyPropertyChanged( m => m.SlidesEssential );
            NotifyPropertyChanged( m => m.SlidesInessential );
        }

        private void CMDOpenPlayList_Func() {
            var file = OpenDialog();
            PlayList playList = null;
            PlayList.Open( file, out playList );
            PlayList = playList;
        }

        private void CMDRemoveSlide_Func() {
            if( PlayList == null ) return;
            if( SelectedSlide != null ) {
                if(
                    MessageBox.Show( "Удалить картинку?", "Предупреждение", MessageBoxButton.YesNo,
                        MessageBoxImage.Asterisk ) != MessageBoxResult.Yes )
                    return;
                var index = PlayList.RemoveSlide( SelectedSlide );
                SelectedSlideIndex = index;

                NotifyPropertyChanged( m => m.Slides );
                NotifyPropertyChanged( m => m.SlidesEssential );
                NotifyPropertyChanged( m => m.SlidesInessential );
                NotifyPropertyChanged( m => m.SelectedSlideIndex );
            }
        }

        private void CMDSavePlayList_Func() {
            if( PlayList == null ) return;
            var sfd = new System.Windows.Forms.SaveFileDialog();
            var initPath = AppSettings.GetValue( "LastPlayListFolder", Constants.AppDataFolder );
            sfd.InitialDirectory = (initPath + @"\").CutFileName();
            sfd.Filter = string.Format( "*{0}|*{0}", Constants.PlayListExt );
            if( sfd.ShowDialog() == System.Windows.Forms.DialogResult.OK ) {
                PlayList.Save( sfd.FileName );
                AppSettings.SetValue( "LastPlayListFolder", sfd.FileName.CutFileName() );
            }
        }

        private void CMDSetSelect_Func( object param ) {
            if( param as string == "Essential" ) {
                if( SelectedSlideEssential != null )
                    SelectedSlide = SelectedSlideEssential;
            } else if( param as string == "Inessential" ) {
                if( SelectedSlideInessential != null )
                    SelectedSlide = SelectedSlideInessential;
            }
        }

        private void CMDToggleGrade_Func() {
            if( SelectedSlide == null ) return;
            var now = SelectedSlide.Grade == Slide.SlideGrade.Essential;
            SelectedSlide.Grade = !now ? Slide.SlideGrade.Essential : Slide.SlideGrade.Inessential;
            if( !now ) {
                SelectedSlideEssential = SelectedSlide;
            } else {
                SelectedSlideInessential = SelectedSlide;
            }

            PlayList.IsNotSaveChanges = true;

            NotifyPropertyChanged( m => m.Slides );
            NotifyPropertyChanged( m => m.SlidesEssential );
            NotifyPropertyChanged( m => m.SlidesInessential );
            NotifyPropertyChanged( m => m.IsImportant );
            NotifyPropertyChanged( m => m.SelectedSlide );
        }
    }
}