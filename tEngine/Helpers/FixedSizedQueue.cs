using System.Collections.Concurrent;

namespace tEngine.Helpers
{
    /// <summary>
    /// Очередь фиксированного размера с которой могут работать несколько потоков 
    /// Используется в классе PerSeconds
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class FixedSizedQueue<T> : ConcurrentQueue<T>
    {
        public uint Limit { get; set; }

        /// <summary>
        /// Adds an object to the end of the System.Collections.Concurrent.ConcurrentQueue<T>.
        /// </summary>
        /// <param name="item">item: The object to add to the end of the System.Collections.Concurrent.ConcurrentQueue<T>. The value can be a null reference (Nothing in Visual Basic) for reference types.</param>
        public new void Enqueue(T item)
        {
            base.Enqueue(item);
            lock (this)
            {
                T overflow;
                while (Count > Limit && TryDequeue(out overflow)) ;
            }
        }
    }
}