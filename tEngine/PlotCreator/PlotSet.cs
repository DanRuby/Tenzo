using System.Diagnostics;
using System.Runtime.Serialization;
using System.Windows.Media;
using OxyPlot;
using OxyPlot.Axes;
using tEngine.Helpers;

namespace tEngine.PlotCreator
{
    [DataContract]
    public class PlotSet
    {
        [DataMember]
        public double AxesFontSize { get; set; }

        [DataMember]
        public PlotAxes AxesOX { get; set; }

        [DataMember]
        public PlotAxes AxesOY { get; set; }

        [DataMember]
        public EAxesStyle AxesStyle { get; set; }

        [DataMember]
        public Color BackColor { get; set; }

        [DataMember]
        public bool ShowTitle { get; set; }

        [DataMember]
        public string Title { get; set; }

        [DataMember]
        public double TitleFontSize { get; set; }

        [DataMember]
        public ETitlePos TitlePos { get; set; }

        public PlotSet() => Init(null);

        public PlotSet(PlotModelEx pm) => Init(pm);

        public void CopyScale(PlotModel pm)
        {
            Axis axes1 = pm.Axes.Count > 0 ? pm.Axes[0] : null;
            Axis axes2 = pm.Axes.Count > 1 ? pm.Axes[1] : null;
            Debug.Assert(axes1 != null && axes2 != null);

            CopyScale(AxesOY, axes1);
            CopyScale(AxesOX, axes2);
        }

        public void CopyScale(PlotAxes dest, Axis source)
        {
            dest.Minimum = source.ActualMinimum;
            dest.Maximum = source.ActualMaximum;
        }

        public void LoadFromModel(PlotModelEx pm)
        {
            BackColor = pm.Background.GetColorMedia();
            Title = pm.Title;
            TitleFontSize = (int)pm.TitleFontSize;
            ShowTitle = true;
            

            Axis axes1 = pm.Axes.Count > 0 ? pm.Axes[0] : null;
            Axis axes2 = pm.Axes.Count > 1 ? pm.Axes[1] : null;
            if (axes1 == null || axes2 == null)
                return;
            Debug.Assert(axes1 != null && axes2 != null);

            if ((axes1.IsAxisVisible || axes2.IsAxisVisible) == false)
                AxesStyle = EAxesStyle.None;
            else
                AxesStyle = (axes1.PositionAtZeroCrossing || axes2.PositionAtZeroCrossing)
                    ? EAxesStyle.Cross
                    : EAxesStyle.Boxed;


            AxesFontSize = (int)axes1.FontSize;

            Cloner.CopyAllProperties(AxesOY, axes1);
            Cloner.CopyAllProperties(AxesOX, axes2);
            CopyScale(AxesOY, axes1);
            CopyScale(AxesOX, axes2);
        }

        public void SetDefault()
        {
            AxesOX.SetDefault();
            AxesOY.SetDefault();
            AxesStyle = EAxesStyle.Boxed;
            BackColor = Colors.White;
            Title = "";
            TitleFontSize = 12; 
            ShowTitle = true;
            TitlePos = ETitlePos.Top;
            AxesFontSize = 12;  
        }

        private void Init(PlotModelEx pm)
        {
            AxesOX = new PlotAxes();
            AxesOY = new PlotAxes();
            if (pm == null)
                SetDefault();
            else
                LoadFromModel(pm);
        }

        #region Byte <=> Object

        public byte[] ToByteArray()
        {
            return BytesPacker.JSONObj(this);
        }

        public bool LoadFromArray(byte[] array)
        {
            PlotSet obj = BytesPacker.LoadJSONObj<PlotSet>(array);
            Cloner.CopyAllProperties(this, obj);
            return true;
        }

        #endregion
    }
}