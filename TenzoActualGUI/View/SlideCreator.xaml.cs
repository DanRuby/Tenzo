using System.ComponentModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using tEngine.Helpers;
using tEngine.TActual.DataModel;
using TenzoActualGUI.ViewModel;
using MessageBox = System.Windows.MessageBox;

namespace TenzoActualGUI.View {
    /// <summary>
    /// Interaction logic for SlideCreator.xaml
    /// </summary>
    public partial class SlideCreator : Window {
        private SlideCreatorVM mDataContext;

        public PlayList PlayList {
            get { return mDataContext.PlayList; }
            set { mDataContext.PlayList = value; }
        }

        public SlideCreator() {
            InitializeComponent();
            WindowManager.UpdateWindowPos( this.GetType().Name, this );
            mDataContext = new SlideCreatorVM() {Parent = this};
            DataContext = mDataContext;
        }

        private void Window_OnClosing( object sender, CancelEventArgs e ) {
            if ( mDataContext != null ) {
                try {
                    DialogResult = mDataContext.DialogResult;
                } catch { /*если окно не диалог - вылетит исключение, ну и пусть*/ }
            }
            WindowManager.SaveWindowPos( this.GetType().Name, this );
        }
    }
}