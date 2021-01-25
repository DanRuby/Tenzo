namespace tEngine.Recorder
{
    /// <summary>
    /// Д: Набор комманд для обмена с прибором
    /// </summary>
    public class Commands
    {
        public class FromDevice
        {
            /// <summary>
            /// Пришла информация о ацп
            /// </summary>
            public const byte ADCCHECK = 0x02;

            /// <summary>
            /// Пришла информация в пакете
            /// </summary>
            public const byte DATA = 0x01;
        }

        public class ToDevice
        {
            /// <summary>
            /// Установка нового идентификатора измерений (новая картинка)
            /// </summary>
            public const byte NEW_REQUEST = 0x71;

            /// <summary>
            /// Команда перезагрузки/перекалибровки
            /// </summary>
            public const byte RESTART = 0x72;

            /// <summary>
            /// Установить режим работы AdcCheck
            /// </summary>
            public const byte SM_ADCCHECK = 0x73;

            /// <summary>
            /// Установить режим работы Normal
            /// </summary>
            public const byte SM_NORMAL = 0x74;
        }
    }
}