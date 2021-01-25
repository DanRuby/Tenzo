using System;
using System.Collections.Generic;

namespace tEngine.TMeter.DataModel.IData {
    public interface IUser<T> {
        /// <summary>
        /// День рождения
        /// </summary>
        DateTime BirthDate { get; set; }

        /// <summary>
        /// Комментарии врача
        /// </summary>
        string Comment { get; set; }

        /// <summary>
        /// Абсолютный путь к файлу пациента
        /// </summary>
        string FilePath { get; set; }

        /// <summary>
        /// Отчество
        /// </summary>
        string FName { get; set; }

        /// <summary>
        /// Уникальный идентификатор
        /// </summary>
        Guid ID { get; set; }

        /// <summary>
        /// Список проведенных измерений 
        /// </summary>
        List<Measurement> Msms { get; set; }

        /// <summary>
        /// Имя
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Фамилия
        /// </summary>
        string SName { get; set; }

        /// <summary>
        /// Добавить измерение
        /// </summary>
        /// <param name="msm"></param>
        void AddMsm( Measurement msm );

        /// <summary>
        /// Получить измерение по номеру
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        Measurement GetMsm( int index );

        /// <summary>
        /// Получить измерение по ID
        /// </summary>
        /// <param name="msmId"></param>
        /// <returns></returns>
        Measurement GetMsm( Guid msmId );

        /// <summary>
        /// Количество измерений
        /// </summary>
        /// <returns></returns>
        int GetMsmCount();

        /// <summary>
        /// Получить коллекцию измерений
        /// </summary>
        /// <returns></returns>
        IEnumerable<Measurement> GetMsms();

        /// <summary>
        /// Открыть файл с пользователем
        /// </summary>
        /// <returns></returns>
        bool Open( string filePath, out T user );

        /// <summary>
        /// Удаоить измерение
        /// </summary>
        /// <param name="msm"></param>
        void RemoveMsm( Measurement msm );

        /// <summary>
        /// Удалить измерение
        /// </summary>
        /// <param name="msm"></param>
        void RemoveMsm( Guid msmId );

        /// <summary>
        /// Сохранить по умочанию
        /// </summary>
        /// <returns>успех</returns>
        bool Save();

        /// <summary>
        /// Сохранить по новому пути
        /// </summary>
        /// <param name="filePath">путь к файлу</param>
        /// <returns>успех</returns>
        bool Save( string filePath );

        /// <summary>
        /// Инициалы
        /// </summary>
        /// <returns></returns>
        string UserShort();
    }
}