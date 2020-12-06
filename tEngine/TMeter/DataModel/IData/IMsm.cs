using System;
using tEngine.DataModel;

namespace tEngine.TMeter.DataModel.IData {
    public interface IMsm {
        /// <summary>
        /// Комментарий врача, описание
        /// </summary>
        string Comment { get; set; }

        /// <summary>
        /// Время создания
        /// </summary>
        DateTime CreateTime { get; set; }

        /// <summary>
        /// Возвращает содержащиеся данные
        /// </summary>
        TData Data { get; }

        /// <summary>
        /// Уникальный идентификатор
        /// </summary>
        Guid ID { get; set; }

        /// <summary>
        /// Время измерения
        /// </summary>
        double MsmTime { get; set; }

        /// <summary>
        /// Пациент владелец
        /// </summary>
        User Owner { get; set; }

        /// <summary>
        /// Название 
        /// </summary>
        string Title { get; set; }

        /// <summary>
        /// Добавление 
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        void AddData( Hand left, Hand right );

        /// <summary>
        /// Очистка данных
        /// </summary>
        void Clear();

        /// <summary>
        /// Возвращает количество записанных значений 
        /// </summary>
        /// <returns></returns>
        int DataLength();

        /// <summary>
        /// Возвращает пациента
        /// </summary>
        /// <returns></returns>
        User GetOwner();

        /// <summary>
        /// Установка свзи измерения с пациентом
        /// </summary>
        void UserAssociated( User owner );
    }
}