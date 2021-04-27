using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using OxyPlot;

namespace tEngine.Helpers
{
    /// <summary>
    /// Расширения LINQ
    /// </summary>
    public static class LINQExtention
    {
        /// <summary>
        /// прореживание
        /// </summary>
        /// <param name="length">длинна результата</param>
        /// <returns></returns>
        public static IList<T> GetPartExactly<T>(this IEnumerable<T> enumerable, uint length)
        {
            IList<T> collection = enumerable as IList<T> ?? enumerable.ToList();
            if (collection.IsNullOrEmpty())
                return collection;

            // сколько можно отдать
            int hasValues = collection.Count();
            // сколько требуют
            double requireValues = length == 0 ? 1 : length;

            IList<T> result = collection;
            if (requireValues <= hasValues && requireValues >= hasValues / 2.0)
            {
                // требуют меньше чем есть, но больше половины -> исключаем каждый k-ый
                int k = (int)Math.Floor(hasValues / (hasValues - requireValues));
                result = collection.Where((o, i) => i % k != 0).ToList();
            }
            else if (requireValues < hasValues / 2.0)
            {
                //требуют меньше половины -> берем каждый k-ый
                int k = (int)Math.Floor(hasValues / requireValues);
                result = collection.Where((o, i) => i % k == 0).ToList();
            }
            else if (requireValues >= hasValues)
            {
                //требуют больше чем есть
                result = collection;
            }
            return result;
        }

        public static IList<DataPoint> Normalized(this IList<DataPoint> list)
        {
            if (list.IsNullOrEmpty())
                return new List<DataPoint>();
            double max = list.Max(dp => Math.Abs(dp.Y));
            return list.Select(dp => new DataPoint(dp.X, dp.Y / max)).ToList();
        }


        /// <summary>
        /// прореживание
        /// </summary>
        /// <param name="resolution">процент длины результата от исходной коллекции</param>
        /// <returns></returns>
        public static IList<T> GetPartPercent<T>(this IEnumerable<T> enumerable, double resolution)
        {
            Debug.Assert(resolution >= 0);
            IList<T> collection = enumerable as IList<T> ?? enumerable.ToList();
            if (collection.IsNullOrEmpty())
                return collection;

            int hasValues = collection.Count();
            double length = hasValues * resolution / 100.0;

            return collection.GetPartExactly((uint)length);
        }


        public static bool IsNullOrEmpty<T>(this IEnumerable<T> enumerable)
        {
            if (enumerable == null)
            {
                return true;
            }
            ICollection<T> collection = enumerable as ICollection<T>;
            if (collection != null)
            {
                return collection.Count < 1;
            }
            return !enumerable.Any();
        }
    }
}