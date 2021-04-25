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
            WindowManager.UpdateWindowPos(GetType().Name, this);
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
            WindowManager.SaveWindowPos(GetType().Name, this);
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
            get => mArea.Color;
            set => mArea.Color = value;
        }

        public int Grid
        {
            get => mArea.Grid;
            set
            {
                if (value > 0)
                    mArea.Grid = value;
                else mArea.Grid = 1;
            }
        }

        public Color GridColor
        {
            get => mArea.GridColor;
            set => mArea.GridColor = value;
        }

        public Color M1Color
        {
            get => mArea.MarkerLeftHand.Color;
            set => mArea.MarkerLeftHand.Color = value;
        }

        public int M1Height
        {
            get => mArea.MarkerLeftHand.Height;
            set
            {
                if (value > 0)
                    mArea.MarkerLeftHand.Height = value;
                else mArea.MarkerLeftHand.Height = 1;
            }
        }

        public int M1Width
        {
            get => mArea.MarkerLeftHand.Width;
            set
            {
                if (value > 0)
                    mArea.MarkerLeftHand.Width = value;
                else mArea.MarkerLeftHand.Width = 1;
            }
        }

        public Color M2Color
        {
            get => mArea.MarkerRightHand.Color;
            set => mArea.MarkerRightHand.Color = value;
        }

        public int M2Height
        {
            get => mArea.MarkerRightHand.Height;
            set
            {
                if (value > 0)
                    mArea.MarkerRightHand.Height = value;
                else mArea.MarkerRightHand.Height = 1;
            }
        }

        public int M2Width
        {
            get => mArea.MarkerRightHand.Width;
            set
            {
                if (value > 0)
                    mArea.MarkerRightHand.Width = value;
                else mArea.MarkerRightHand.Width = 1;
            }
        }

        public int Maximum
        {
            get => mArea.Maximum;
            set => mArea.Maximum = value;
        }

        public int Minimum
        {
            get => mArea.Minimum;
            set => mArea.Minimum = value;
        }

        public bool ShowAxis
        {
            get => mArea.ShowAxis;
            set => mArea.ShowAxis = value;
        }

        public bool ShowGrid
        {
            get => mArea.ShowGrid;
            set => mArea.ShowGrid = value;
        }

        public bool ShowMarker1
        {
            get => mArea.ShowMarkerLeft;
            set => mArea.ShowMarkerLeft = value;
        }

        public bool ShowMarker2
        {
            get => mArea.ShowMarkerRight;
            set => mArea.ShowMarkerRight = value;
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