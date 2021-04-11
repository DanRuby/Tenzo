namespace tEngine.DataModel
{
    public class HAND_DATA
    {
        public STDATA Constant { get; set; }
        public STDATA Correlation { get; set; }

        public bool HasConstant
        {
            get { return Constant.HasData; }
        }

        public bool HasCorrelation
        {
            get { return Correlation.HasData; }
        }

        public bool HasSpectrum
        {
            get { return Spectrum.HasData; }
        }

        public bool HasTremor
        {
            get { return Tremor.HasData; }
        }

        public STDATA Spectrum { get; set; }
        public STDATA Tremor { get; set; }

        public HAND_DATA()
        {
            Constant = new STDATA();
            Correlation = new STDATA();
            Spectrum = new STDATA();
            Tremor = new STDATA();
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