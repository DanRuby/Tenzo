using System;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using OxyPlot;
using tEngine.Helpers;
using tEngine.TActual.DataModel;
using tEngine.TMeter.DataModel;
using TenzoMeterGUI.ViewModel;
using TMSingleMeasurement.View;
using Msm = tEngine.TMeter.DataModel.Msm;

namespace TenzoMeterGUI.View {
    /// <summary>
    /// Interaction logic for UserWorkSpace.xaml
    /// </summary>
    public partial class UserWorkSpace : Window {
        private UserWorkSpaceVM mDataContext;

        public Guid ID {
            get { return mDataContext.User.ID; }
        }

        public UserWorkSpace() {
            InitializeComponent();
            WindowManager.UpdateWindowPos( this.GetType().Name, this );
            mDataContext = new UserWorkSpaceVM() {Parent = this};
            DataContext = mDataContext;
        }

        public void OpenMsm( Msm msm ) {
            mDataContext.OpenMsm( msm );
        }

        public void CopyUserInfo(User user) {
            Cloner.CopyAllProperties( mDataContext.User, user );
            mDataContext.UpdateAllProperties();
        }
        public void SetUser( User user ) {
            mDataContext.User = user;
            return;
            // полное копирование User
            mDataContext.User = new User(user);
        }

        public void UpdateAllProperties() {
            mDataContext.UpdateAllProperties();
        }

        private void PlotViewEx2_OnLoaded( object sender, RoutedEventArgs e ) {
            PlotViewEx2.Clear();
            var data = Enumerable.Range( 0, 100 ).Select( i => new DataPoint( i, Math.Sqrt( i ) ) ).ToList();
            PlotViewEx2.AddLineSeries( data, color: null, thickness: 2 );
            PlotViewEx2.ReDraw();
        }

        private void Window_OnClosing( object sender, CancelEventArgs e ) {
            if( mDataContext != null ) {
                mDataContext.PreClosed();
            }
            WindowManager.SaveWindowPos( this.GetType().Name, this );
            GC.Collect();
        }
    }
}