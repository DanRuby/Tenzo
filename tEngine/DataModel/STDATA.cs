using System.Collections.Generic;
using System.Linq;
using OxyPlot;

namespace tEngine.DataModel
{
    public class STDATA
    {
        private List<DataPoint> mData;
        public int Count { get; set; }

        public List<DataPoint> Data
        {
            get { return mData; }
            set
            {
                mData = value;
                if (mData != null)
                {
                    Count = mData.Count;
                    if (Count > 0)
                    {
                        Mean = mData.Average(point => point.Y);
                        Min = mData.Min(point => point.Y);
                        Max = mData.Max(point => point.Y);
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

        public bool HasData
        {
            get { return Data != null && Count > 0; }
        }

        public double Max { get; private set; }
        public double Mean { get; private set; }
        public double Min { get; private set; }

        public STDATA()
        {
            Data = new List<DataPoint>();
        }

        public void Clear()
        {
            Data = new List<DataPoint>();
        }
    }
}