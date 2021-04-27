using System;
using System.Linq;
using tEngine.Helpers;

namespace tEngine.Recorder
{
    /// <summary>
    /// Счетчик пакетов в секунду
    /// </summary>
    public class PerSeconds
    {
        private object mLock = new object();
        private FixedSizedQueue<DateTime> mQueue = new FixedSizedQueue<DateTime>();

        public PerSeconds()
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
                double time = (mQueue.Last() - mQueue.First()).TotalMilliseconds;
                // лимит корректируется во времени
                if (mQueue.Count == mQueue.Limit)
                {
                    if (time < 500)
                        mQueue.Limit++;
                    if (time > 2000)
                        mQueue.Limit--;
                }
                return (mQueue.Count / time) * 1000;
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