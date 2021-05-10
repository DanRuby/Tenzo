using System.Runtime.Serialization;
using System.Windows.Media;

namespace tEngine.PlotCreator
{
    [DataContract]
    public class PlotAxes
    {
        [DataMember]
        public bool AutoGrid { get; set; }

        [DataMember]
        public bool AutoScale { get; set; }

        /// <summary>
        /// Количество знаков после запятой
        /// </summary>
        [DataMember]
        public int DecimalCount { get; set; }

        /// <summary>
        /// После какого порядка рисовать степень (по модулю, одинакого и в +, и в -)
        /// </summary>
        [DataMember]
        public uint ExponentCount { get; set; }

        /// <summary>
        /// Шаг сетки
        /// </summary>
        [DataMember]
        public double Grid { get; set; }

        [DataMember]
        public Color GridColor { get; set; }

        [DataMember]
        public bool IsAxisVisible { get; set; }

        [DataMember]
        public bool IsPanEnabled { get; set; }

        [DataMember]
        public bool IsZoomEnabled { get; set; }

        [DataMember]
        public bool LogScale { get; set; }

        [DataMember]
        public double Maximum { get; set; }

        [DataMember]
        public double Minimum { get; set; }

        [DataMember]
        public double NumbersFontSize { get; set; }

        [DataMember]
        public bool ShowGrid { get; set; }

        [DataMember]
        public bool ShowNumbers { get; set; }

        [DataMember]
        public bool ShowTitle { get; set; }

        [DataMember]
        public string Title { get; set; }

        public PlotAxes() => SetDefault();

        public void SetDefault()
        {
            LogScale = false;
            ShowGrid = false;
            GridColor = Colors.LightGray;
            ShowNumbers = true;
            AutoScale = true;
            AutoGrid = true;
            Minimum = 0;
            Maximum = 10;
            Grid = 2;
            DecimalCount = 2;
            ExponentCount = 3;
            NumbersFontSize = 12; 
            Title = "";
            ShowTitle = true;
            IsPanEnabled = true;
            IsZoomEnabled = true;
        }
    }
}