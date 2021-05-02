namespace tEngine.Recorder
{
    /// <summary>
    /// Счетчики устройства по пакетам
    /// </summary>
    public class DeviceCounters
    {
        /// <summary>
        /// Попытки подключения
        /// </summary>
        public int Connections { get; set; }

        /// <summary>
        /// Принятые к обработке пакеты
        /// </summary>
        public int FullPack { get; internal set; }

        /// <summary>
        /// Битые пакеты
        /// </summary>
        public int InvalidPack { get; internal set; }

        /// <summary>
        /// Утеряно пакетов
        /// </summary>
        public int LostPack { get; internal set; }

        /// <summary>
        /// Всего пакетов в секунду, включая повторяющиеся
        /// </summary>
        public PacketsPerSecondCounter PPS { get; internal set; }

        /// <summary>
        /// Потеряно пакетов
        /// </summary>
        public int RepeatPack { get; internal set; }

        /// <summary>
        /// Получено пакетов
        /// </summary>
        public int TotalPack { get; internal set; }

        /// <summary>
        /// Принятые к запись пакеты в секунду
        /// </summary>
        public PacketsPerSecondCounter ValidPPS { get; internal set; }

        public DeviceCounters()
        {
            Clear();
        }

        public void Clear()
        {
            FullPack = 0;
            InvalidPack = 0;
            LostPack = 0;
            PPS = new PacketsPerSecondCounter();
            RepeatPack = 0;
            TotalPack = 0;
            ValidPPS = new PacketsPerSecondCounter();
        }
    }
}