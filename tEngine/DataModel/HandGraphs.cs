namespace tEngine.DataModel
{
    public class HandGraphs
    {
        public Graph Constant { get; set; }
        public Graph Correlation { get; set; }
        public Graph Spectrum { get; set; }
        public Graph Tremor { get; set; }

        public bool HasConstant => Constant.HasData;
        public bool HasCorrelation => Correlation.HasData;
        public bool HasSpectrum => Spectrum.HasData;
        public bool HasTremor => Tremor.HasData;

        public HandGraphs()
        {
            Constant = new Graph();
            Correlation = new Graph();
            Spectrum = new Graph();
            Tremor = new Graph();
        }

        public void Clear()
        {
            Constant.Clear();
            Correlation.Clear();
            Spectrum.Clear();
            Tremor.Clear();
        }
    }
}