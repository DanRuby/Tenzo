using System.Collections.Generic;
using System.Linq;
using OxyPlot;

namespace tEngine.DataModel
{
    /// <summary>
    /// Инкапсуляция данных для графиков
    /// </summary>
    public class Graph
    {
        private List<DataPoint> dataPoints;

        /// <summary>
        /// Количество точек  
        /// </summary>
        public int Count => dataPoints.Count;

        /// <summary>
        /// Точки
        /// </summary>
        public List<DataPoint> DataPoints
        {
            get => dataPoints;
            set
            {
                dataPoints = value;
                if (dataPoints != null)
                {
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

        /// <summary>
        /// Имеются ли данные
        /// </summary>
        public bool HasData => DataPoints != null && Count > 0;
        
        /// <summary>
        /// Максимум на графике 
        /// </summary>
        public double Max { get; private set; }

        /// <summary>
        /// Среднее значение
        /// </summary>
        public double Mean { get; private set; }

        /// <summary>
        /// Минимум на графике
        /// </summary>
        public double Min { get; private set; }

        public Graph() => DataPoints = new List<DataPoint>();

        /// <summary>
        /// Очистить данные
        /// </summary>
        public void Clear() => DataPoints = new List<DataPoint>();
    }
}