using System;
using System.Collections.Generic;
using tEngine.DataModel;

namespace tEngine.TMeter.DataModel.IData {
    public interface IRsch<T> {
        /// <summary>
        /// Комментарий врача, описание
        /// </summary>
        string Comment { get; set; }

        /// <summary>
        /// Время создания
        /// </summary>
        DateTime CreateTime { get; set; }

        /// <summary>
        /// Абсолютный путь к файлу
        /// </summary>
        string FilePath { get; set; }

        /// <summary>
        /// Уникальный идентификатор
        /// </summary>
        Guid ID { get; set; }

        /// <summary>
        /// Перечисление ID включенных измерений
        /// </summary>
        List<Guid> MsmsGuids { get; set; }

        /// <summary>
        /// Название 
        /// </summary>
        string Title { get; set; }

        /// <summary>
        /// Список фалов пользователей
        /// </summary>
        List<string> UsersPaths { get; set; }

        /// <summary>
        /// Добавить новое измерение
        /// </summary>
        /// <param name="user"></param>
        /// <param name="msmId"></param>
        void AddMsm( User user, Guid msmId );

        /// <summary>
        /// Получить одно измерение
        /// </summary>
        /// <returns></returns>
        Msm GetMsm( Guid msmId );

        /// <summary>
        /// Количество измерений
        /// </summary>
        /// <returns></returns>
        int GetMsmCount();

        /// <summary>
        /// Получить набор измерений
        /// </summary>
        /// <returns></returns>
        IEnumerable<Msm> GetMsms();

        /// <summary>
        /// Получить набор измерений
        /// </summary>
        /// <returns></returns>
        IEnumerable<User> GetUsers();

        /// <summary>
        /// Количество пациентов
        /// </summary>
        /// <returns></returns>
        int GetUsersCount();

        /// <summary>
        /// Открыть
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        bool Open( string filePath, out T rsch );

        /// <summary>
        /// Удалить измерение из набора
        /// </summary>
        /// <param name="msmId"></param>
        void RemoveMsm( Guid msmId );

        /// <summary>
        /// Удалить измерение из набора
        /// </summary>
        /// <param name="msm"></param>
        void RemoveMsm( Msm msm );

        /// <summary>
        /// Сохранить в файл по умолчанию
        /// </summary>
        bool Save();

        /// <summary>
        /// Сохранить в новый файл
        /// </summary>
        /// <param name="filePath"></param>
        bool Save( string filePath );
    }
}