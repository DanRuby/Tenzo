using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace tEngine.UControls
{
    /// <summary>
    /// Interaction logic for WaitBagel.xaml
    /// </summary>
    public partial class WaitBagel : UserControl
    {
        public static readonly DependencyProperty ShowProperty = DependencyProperty.Register(
            "Show", typeof(bool), typeof(WaitBagel), new PropertyMetadata(false, (obj, args) =>
            {
                (obj as WaitBagel).GetBindingExpression(UserControl.VisibilityProperty).UpdateTarget();
            }));

        public bool Show
        {
            get => (bool)GetValue(ShowProperty);
            set => SetValue(ShowProperty, value);
        }

        public Visibility IsShow => Show ? Visibility.Visible : Visibility.Collapsed;

        public WaitBagel()
        {
            InitializeComponent();


            // проверка на design mode
            if ((LicenseManager.UsageMode == LicenseUsageMode.Designtime) == false)
            {
                GifImage.GifSource = @"/gif/load1.gif";
                GifImage.AutoStart = true;
            }
        }
    }
}
