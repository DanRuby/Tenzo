using System.Collections.Concurrent;

namespace tEngine.Helpers
{
    /// <summary>
    /// Очередь фиксированного размера с которой могут работать несколько потоков 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class FixedSizedQueue<T> : ConcurrentQueue<T>
    {
        /// <summary>
        /// Максимальный размер очереди
        /// </summary>
        public uint Limit { get; set; }

        /// <summary>
        /// Добавляет элемент в очередь
        /// </summary>
        /// <param name="item">Элемент для добавления. Может быть null, если тип T поддерживает nullable</param>
        public new void Enqueue(T item)
        {
            base.Enqueue(item);
            lock (this)
            {
                while (Count > Limit && TryDequeue(out T overflow)) ;
            }
        }
    }
}