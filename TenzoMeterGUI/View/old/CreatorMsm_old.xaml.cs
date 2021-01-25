using System.Windows;
using System.Windows.Input;
using tEngine.TMeter.DataModel;
using TenzoMeterGUI.ViewModel;

namespace TenzoMeterGUI.View.old {
    /// <summary>
    /// Interaction logic for MsmCreator.xaml
    /// </summary>
    public partial class CreatorMsm_old : Window {
        public Msm Result { get; set; }

        public CreatorMsm_old() {
            InitializeComponent();
            Init( "" );
        }

        public void Init( string title ) {
            DataContext = new CreatorMsmVM( title ) {
                Close = this.Close,
                FixResult = ( result ) => { this.Result = result; }
            };
        }

        private void TextBox_KeyDown( object sender, KeyEventArgs e ) {
            if( e.Key == Key.Enter ) {
                var element = sender as UIElement;
                if( element != null ) {
                    element.MoveFocus( new TraversalRequest( FocusNavigationDirection.Next ) );
                }
            }
        }
    }
}