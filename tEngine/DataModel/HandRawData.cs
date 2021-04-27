using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using tEngine.Helpers;

namespace tEngine.DataModel
{
    /// <summary>
    /// Список значений на каждую руку
    /// </summary>
    [DataContract]
    public class HandRawData
    {
        private const int MaxLength = 50 * 1024 * 1024; // 50 мБ Д: по факту будет хранится 200 МБ инфы потому что шорт= 2 Б и 2 списка

        /// <summary>
        /// Точка начала выбранного диапазона
        /// </summary>
        [DataMember]
        public int BeginPoint { get; set; }

        ///<summary>
        ///Произвольная составляющая
        ///</summary>
        [DataMember]
        public List<short> Constant { get; set; }

        /// <summary>
        /// Точка конца выбранного диапазона
        /// </summary>
        [DataMember]
        public int EndPoint { get; set; }

        /// <summary>
        /// Тремор
        /// </summary>
        [DataMember]
        public List<short> Tremor { get; set; }

        public HandRawData()
        {
            Constant = new List<short>();
            Tremor = new List<short>();
            ResetPoints();
        }

        public void Clear()
        {
            Constant.Clear();
            Tremor.Clear();
            ResetPoints();
        }

        public bool LoadFromArray(byte[] array)
        {
            byte[][] data = BytesPacker.UnpackBytes(array);
            if (data.Length != 2 || data[0].Length != data[1].Length)
                return false;
            Constant = data[0].GetCollectionInt16().ToList();
            Tremor = data[1].GetCollectionInt16().ToList();
            return true;
        }

        public static HandRawData operator +(HandRawData a, HandRawData b)
        {
            if (a == null)
                a = new HandRawData();
            if (b == null)
                b = new HandRawData();
            HandRawData result = new HandRawData();
            result.Constant.AddRange(a.Constant);
            result.Constant.AddRange(b.Constant);

            result.Tremor.AddRange(a.Tremor);
            result.Tremor.AddRange(b.Tremor);

            int notFit = result.Constant.Count - MaxLength;
            if (notFit > 0)
                result.Constant.RemoveRange(0, notFit);
            result.ResetPoints();
            return result;
        }

        public byte[] ToByteArray() => BytesPacker.PackBytes(Constant.ToByteArray(), Tremor.ToByteArray());

        private void ResetPoints()
        {
            BeginPoint = 0;
            EndPoint = Constant.Count - 1;
        }

        #region No References
        /// <summary>
        /// Д: Возвращает последние значения начиная с позиции count 
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        public HandRawData GetHand(int count)
        {
            if (count > Constant.Count)
                count = Constant.Count;
            int first = Constant.Count - count;

            HandRawData result = new HandRawData();
            result.Constant = Constant.GetRange(first, count);
            result.Tremor = Tremor.GetRange(first, count);

            return result;
        }
        #endregion
    }
}