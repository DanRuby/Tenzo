using System.Collections.Generic;
using System.Linq;
using OxyPlot;

namespace tEngine.DataModel
{
    public class Graph
    {
        private List<DataPoint> dataPoints;
        public int Count { get; set; }

        public List<DataPoint> DataPoints
        {
            get => dataPoints;
            set
            {
                dataPoints = value;
                if (dataPoints != null)
                {
                    Count = dataPoints.Count;
                    if (Count > 0)
                    {
                        Mean = dataPoints.Average(point => point.Y);
                        Min = dataPoints.Min(point => point.Y);
                        Max = dataPoints.Max(point => point.Y);
                    }
                    else
                    {
                        Mean = 0;
                        Min = 0;
                        Max = 0;
                    }
                }
            }
        }

        public bool HasData => DataPoints != null && Count > 0;

        public double Max { get; private set; }
        public double Mean { get; private set; }
        public double Min { get; private set; }

        public Graph()
        {
            DataPoints = new List<DataPoint>();
        }

        public void Clear()
        {
            DataPoints = new List<DataPoint>();
        }
    }
}