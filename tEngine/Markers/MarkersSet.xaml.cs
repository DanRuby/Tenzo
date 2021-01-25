using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using tEngine.Helpers;
using tEngine.MVVM;

namespace tEngine.Markers
{
    /// <summary>
    /// Interaction logic for MarkersSet.xaml
    /// </summary>
    public partial class MarkersSet : Window
    {
        private MarkersSetVM mDataContext;

        public MarkersSet()
        {
            InitializeComponent();
            WindowManager.UpdateWindowPos(this.GetType().Name, this);
            mDataContext = new MarkersSetVM() { Parent = this };
            DataContext = mDataContext;
        }

        private void Window_OnClosing(object sender, CancelEventArgs e)
        {
            if (mDataContext != null)
            {
                try
                {
                    DialogResult = mDataContext.DialogResult;
                }
                catch
                {
                    /*если окно не диалог - вылетит исключение, ну и пусть*/
                }
            }
            WindowManager.SaveWindowPos(this.GetType().Name, this);
        }
    }


    public class MarkersSetVM : Observed<MarkersSetVM>
    {
        private MArea mArea = new MArea();
        public Command CMDColorSelect { get; private set; }
        public Command CMDDefaultSet { get; private set; }
        public Command CMDSaveSettings { get; private set; }

        public Color Color
        {
            get { return mArea.Color; }
            set { mArea.Color = value; }
        }

        public int Grid
        {
            get { return mArea.Grid; }
            set { mArea.Grid = value; }
        }

        public Color GridColor
        {
            get { return mArea.GridColor; }
            set { mArea.GridColor = value; }
        }

        public Color M1Color
        {
            get { return mArea.Marker1.Color; }
            set { mArea.Marker1.Color = value; }
        }

        public int M1Height
        {
            get { return mArea.Marker1.Height; }
            set { mArea.Marker1.Height = value; }
        }

        public int M1Width
        {
            get { return mArea.Marker1.Width; }
            set { mArea.Marker1.Width = value; }
        }

        public Color M2Color
        {
            get { return mArea.Marker2.Color; }
            set { mArea.Marker2.Color = value; }
        }

        public int M2Height
        {
            get { return mArea.Marker2.Height; }
            set { mArea.Marker2.Height = value; }
        }

        public int M2Width
        {
            get { return mArea.Marker2.Width; }
            set { mArea.Marker2.Width = value; }
        }

        public int Maximum
        {
            get { return mArea.Maximum; }
            set { mArea.Maximum = value; }
        }

        public int Minimum
        {
            get { return mArea.Minimum; }
            set { mArea.Minimum = value; }
        }

        public bool ShowAxis
        {
            get { return mArea.ShowAxis; }
            set { mArea.ShowAxis = value; }
        }

        public bool ShowGrid
        {
            get { return mArea.ShowGrid; }
            set { mArea.ShowGrid = value; }
        }

        public bool ShowMarker1
        {
            get { return mArea.ShowMarker1; }
            set { mArea.ShowMarker1 = value; }
        }

        public bool ShowMarker2
        {
            get { return mArea.ShowMarker2; }
            set { mArea.ShowMarker2 = value; }
        }

        public MarkersSetVM()
        {
            CMDDefaultSet = new Command(DefaultSet);
            CMDSaveSettings = new Command(SaveSettings);
            CMDColorSelect = new Command(ColorSelect);

            mArea.UpdateSettings();
            UpdateAllProperties();
        }

        private void ColorSelect(object param)
        {
            string color = param as string ?? "";
            if (color.Equals("M1"))
            {
                M1Color = SelectColor(M1Color);
            }
            else if (color.Equals("M2"))
            {
                M2Color = SelectColor(M2Color);
            }
            else if (color.Equals("Grid"))
            {
                GridColor = SelectColor(GridColor);
            }
            else if (color.Equals("BackColor"))
            {
                Color = SelectColor(Color);
            }
            UpdateAllProperties();
        }

        private void DefaultSet()
        {
            mArea.Init();
            UpdateAllProperties();
        }

        private void SaveSettings(object param)
        {
            bool save;
            save = Boolean.TryParse(param as string, out save) && save;
            if (save == true)
            {
                mArea.SaveSettings();
                EndDialog(true);
            }
            else
            {
                EndDialog(false);
            }
        }

        private Color SelectColor(Color color)
        {
            ColorDialog cd = new ColorDialog();
            cd.Color = System.Drawing.Color.FromArgb(color.A, color.R, color.G, color.B);
            DialogResult dr = cd.ShowDialog();
            if (dr == System.Windows.Forms.DialogResult.OK)
            {
                System.Drawing.Color result = cd.Color;

                return Color.FromArgb(result.A, result.R, result.G, result.B);
            }
            return color;
        }
    }
}