using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using tEngine.MVVM;
using tEngine.TMeter.DataModel;

namespace TenzoMeterGUI.View.old {
    /// <summary>
    /// Interaction logic for RschManager.xaml
    /// </summary>
    public partial class RschManager : Window {
        private Dictionary<string, string> mColumns = new Dictionary<string, string>();
        private Dictionary<string, ListSortDirection> mColumnsDirection = new Dictionary<string, ListSortDirection>();
        public Rsch Result { get; set; }

        public RschManager() {
            InitializeComponent();

            var columnsTitles = new List<string>() {"№", "Название", "Дата", "Измерений"};
            var columns = new List<string>() {"Index", "Rsch.Title", "Rsch.CreateTime", "Rsch.Msms.Count"};
            for( int i = 0; i < columnsTitles.Count; i++ ) {
                mColumns.Add( columnsTitles[i], columns[i] );
                mColumnsDirection.Add( columnsTitles[i], ListSortDirection.Ascending );
            }

            Init( new User() );
        }

        public void Init( User user ) {
            DataContext = new RschManagerVM( user ) {
                Close = this.Close,
                SelectItem = this.SelectItem,
                FixResult = ( result ) => { this.Result = result; }
            };
        }

        public void SelectItem( int index ) {
            if( listView.Items.Count > index ) {
                var item = listView.Items[index];
                if( item != null ) {
                    listView.SelectedItem = item;
                }
            }
        }

        private void EventSetter_OnHandler( object sender, RoutedEventArgs e ) {
            var ch = sender as GridViewColumnHeader;
            if( ch == null )
                return;
            var content = ch.Content.ToString();
            if( mColumns.ContainsKey( content ) == false ||
                mColumnsDirection.ContainsKey( content ) == false )
                return;

            var key = mColumns[content];
            var cv = (CollectionView) CollectionViewSource.GetDefaultView( listView.ItemsSource );

            var direct = mColumnsDirection[content];
            if( direct == ListSortDirection.Ascending )
                mColumnsDirection[content] = ListSortDirection.Descending;
            else {
                mColumnsDirection[content] = ListSortDirection.Ascending;
            }

            cv.SortDescriptions.Clear();
            cv.SortDescriptions.Add( new SortDescription( key, direct ) );
        }

        private void Window_Closing( object sender, CancelEventArgs e ) {
            var dc = DataContext as RschManagerVM;
            if( dc != null )
                dc.SaveColumnsWidth();
        }

        private void Window_Loaded( object sender, RoutedEventArgs e ) {
            SelectItem( 0 );
        }
    }
    public class RschManagerVM : Observed<RschManagerVM> {
        public Action Close;
        public Action<Rsch> FixResult;
        public Action<int> SelectItem;
        private ObservableCollection<double> mColumnsWidth;
        private User mCurrentUser;
        private ObservableCollection<RschIndexes> mRsches;
        private RschIndexes mSelectedItem;
        public Command CMDCancel { get; private set; }
        public Command CMDNewRsch { get; private set; }
        public Command CMDSelect { get; private set; }

        public ObservableCollection<double> ColumnsWidth {
            get { return mColumnsWidth; }
            set {
                mColumnsWidth = value;
                NotifyPropertyChanged( m => m.ColumnsWidth );
            }
        }

        public ObservableCollection<RschIndexes> Rsches {
            get { return mRsches; }
            set {
                mRsches = value;
                NotifyPropertyChanged( m => m.Rsches );
            }
        }

        public RschIndexes SelectedItem {
            get { return mSelectedItem; }
            set {
                mSelectedItem = value;
                NotifyPropertyChanged( m => m.SelectedItem );
            }
        }

        public RschManagerVM() {
            Init( new User() );
        }

        public RschManagerVM( User user ) {
            Init( user );
        }

        public void Init( User user ) {
            mCurrentUser = user;
            //Rsches = new ObservableCollection<RschIndexes>( mCurrentUser.Rsches.Select( ( rsch, i ) => new RschIndexes( i + 1, rsch ) ) );
            // TODO check
            //ColumnsWidth = new ObservableCollection<double>( AppSettings.SColumnsWidth );

            CMDNewRsch = new Command( NewRsch );
            CMDCancel = new Command( Cancel );
            CMDSelect = new Command( Select );
            if ( SelectItem != null )
                SelectItem( 0 );
        }

        public void SaveColumnsWidth() {
            // TODO check
            //AppSettings.SColumnsWidth = new List<double>( ColumnsWidth );
        }

        private void Cancel() {
            EndDialog();
        }

        private void EndDialog( Rsch rsch = null ) {
            if ( FixResult != null )
                FixResult( rsch );
            if ( Close != null )
                Close();
        }

        private void NewRsch() {
            //var cr = WindowManager.NewWindow<RschCreator>();
            //var defTitle = string.Format( "Исследование №{0}", Rsches.Count + 1 );
            //cr.Init( defTitle );
            //cr.ShowDialog();
            //if ( cr.Result != null ) {
            //    mCurrentUser.Rsches.Add( cr.Result );
            //    Rsches.Add( new RschIndexes( Rsches.Count + 1, cr.Result ) );
            //}
        }

        private void Select() {
            if ( SelectedItem == null ) {
                MessageBox.Show( "Не выбрано исследование.", "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Exclamation );
                return;
            }
            EndDialog( SelectedItem.Rsch );
        }
    }

    public class RschIndexes {
        public int Index { get; set; }
        public Rsch Rsch { get; set; }

        public RschIndexes( int index, Rsch rsch ) {
            Index = index;
            Rsch = rsch;
        }
    }
}