namespace tEngine.DataModel
{
    /// <summary>
    /// Инкапсуляция графиков каждой руки
    /// </summary>
    public class HandGraphs
    {
        /// <summary>
        /// Произвольная составляющая
        /// </summary>
        public Graph Constant { get; set; }

        /// <summary>
        /// Автокорреляция
        /// </summary>
        public Graph AutoCorrelation { get; set; }

        /// <summary>
        /// Спектральная характеристика
        /// </summary>
        public Graph Spectrum { get; set; }

        /// <summary>
        /// Тремор
        /// </summary>
        public Graph Tremor { get; set; }

        /// <summary>
        /// Имеются данные произвольной составляющей
        /// </summary>
        public bool HasConstant => Constant.HasData;

        /// <summary>
        /// Имеются данные автокорреляции
        /// </summary>
        public bool HasAutoCorrelation => AutoCorrelation.HasData;

        /// <summary>
        /// Имеются данные спектрального анализа
        /// </summary>
        public bool HasSpectrum => Spectrum.HasData;

        /// <summary>
        /// Имеются данные тремора
        /// </summary>
        public bool HasTremor => Tremor.HasData;

        public HandGraphs()
        {
            Constant = new Graph();
            AutoCorrelation = new Graph();
            Spectrum = new Graph();
            Tremor = new Graph();
        }

        /// <summary>
        /// Очистить данные
        /// </summary>
        public void Clear()
        {
            Constant.Clear();
            AutoCorrelation.Clear();
            Spectrum.Clear();
            Tremor.Clear();
        }
    }
}