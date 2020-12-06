using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using tEngine.MVVM;

namespace tEngine.UControls {
    /// <summary>
    /// Interaction logic for WaitBagel.xaml
    /// </summary>
    public partial class WaitBagel : UserControl {
        public static readonly DependencyProperty ShowProperty = DependencyProperty.Register(
            "Show", typeof( bool ), typeof( WaitBagel ), new PropertyMetadata( false , ( obj, args ) => {
                (obj as WaitBagel).GetBindingExpression( UserControl.VisibilityProperty ).UpdateTarget();
            }) );

        public bool Show {
            get { return (bool) GetValue( ShowProperty ); }
            set { SetValue( ShowProperty, value ); }
        }

        public Visibility IsShow {
            get { return Show ? Visibility.Visible : Visibility.Collapsed; }
        }

        public WaitBagel() {
            InitializeComponent();
            

            // проверка на design mode
            if( (LicenseManager.UsageMode == LicenseUsageMode.Designtime) == false ) {
                GifImage.GifSource = @"/gif/load1.gif";
                GifImage.AutoStart = true;
            }
        }
    }
}
