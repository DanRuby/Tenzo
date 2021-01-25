using System.Windows;
using System.Windows.Input;

namespace TenzoMeterGUI.NewStyles
{
    partial class CommonStyles : ResourceDictionary {
        public CommonStyles() {
            InitializeComponent();
        }

        // переводит фокус по "Enter"
        private void FrameworkElement_KeyDown( object sender, KeyEventArgs e ) {
            if ( e.Key == Key.Enter ) {
                var element = sender as UIElement;
                if ( element != null ) {
                    element.MoveFocus( new TraversalRequest( FocusNavigationDirection.Next ) );
                }
            }
        }
    }
}
