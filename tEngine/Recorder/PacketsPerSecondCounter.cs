using System;
using System.Linq;
using tEngine.Helpers;

namespace tEngine.Recorder
{
    /// <summary>
    /// Счетчик пакетов в секунду
    /// </summary>
    public class PacketsPerSecondCounter
    {
        private object mLock = new object();
        private FixedSizedQueue<DateTime> mQueue = new FixedSizedQueue<DateTime>();

        public PacketsPerSecondCounter()
        {
            mQueue.Limit = 2;
            mQueue.Enqueue(DateTime.Now);
            mQueue.Enqueue(DateTime.Now);
        }

        /// <summary>
        /// Величина в секунду
        /// </summary>
        /// <returns></returns>
        public double GetPs()
        {
            lock (mLock)
            {
                double timeElapsed = (mQueue.Last() - mQueue.First()).TotalMilliseconds;
                // лимит корректируется во времени
                if (mQueue.Count == mQueue.Limit)
                {
                    if (timeElapsed < 500)
                        mQueue.Limit++;
                    if (timeElapsed > 2000)
                        mQueue.Limit--;
                }
                return (mQueue.Count / timeElapsed) * 1000;
            }
        }

        /// <summary>
        /// Увеличивает счетчик
        /// </summary>
        public void Increment()
        {
            lock (mLock)
            {
                mQueue.Enqueue(DateTime.Now);
            }
        }
    }
}