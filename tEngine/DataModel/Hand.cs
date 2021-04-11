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
    public class Hand
    {
        private const int MaxLength = 50 * 1024 * 1024; // 50 мБ Д: по факту будет хранится 200 МБ инфы потому что шорт= 2 Б и 2 списка

        /// <summary>
        /// Точка начала выбранного диапазона
        /// </summary>
        [DataMember]
        public int BeginPoint { get; set; }

        ///<summary>
        ///Д:Список не треморных значений?
        ///</summary>
        [DataMember]
        public List<short> Const { get; set; }

        /// <summary>
        /// Точка конца выбранного диапазона
        /// </summary>
        [DataMember]
        public int EndPoint { get; set; }

        /// <summary>
        /// Д: Список треморных значений?
        /// </summary>
        [DataMember]
        public List<short> Tremor { get; set; }

        public Hand()
        {
            Const = new List<short>();
            Tremor = new List<short>();
            ResetPoints();
        }

        public void Clear()
        {
            Const.Clear();
            Tremor.Clear();
            ResetPoints();
        }

        public int GetLength()
        {
            if (Const != null)
                return Const.Count;
            return 0;
        }

        public bool LoadFromArray(byte[] array)
        {
            byte[][] data = BytesPacker.UnpackBytes(array);
            if (data.Length != 2 || data[0].Length != data[1].Length)
                return false;
            Const = data[0].GetCollectionInt16().ToList();
            Tremor = data[1].GetCollectionInt16().ToList();
            return true;
        }

        public static Hand operator +(Hand a, Hand b)
        {
            if (a == null)
                a = new Hand();
            if (b == null)
                b = new Hand();
            Hand result = new Hand();
            result.Const.AddRange(a.Const);
            result.Const.AddRange(b.Const);

            result.Tremor.AddRange(a.Tremor);
            result.Tremor.AddRange(b.Tremor);

            int notFit = result.Const.Count - MaxLength;
            if (notFit > 0)
                result.Const.RemoveRange(0, notFit);
            result.ResetPoints();
            return result;
        }

        public byte[] ToByteArray() => BytesPacker.PackBytes(Const.ToByteArray(), Tremor.ToByteArray());

        private void ResetPoints()
        {
            BeginPoint = 0;
            EndPoint = Const.Count - 1;
        }

        #region No References
        /// <summary>
        /// Д: Возвращает последние значения начиная с позиции count 
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        public Hand GetHand(int count)
        {
            if (count > Const.Count)
                count = Const.Count;
            int first = Const.Count - count;

            Hand result = new Hand();
            result.Const = Const.GetRange(first, count);
            result.Tremor = Tremor.GetRange(first, count);

            return result;
        }
        #endregion
    }
}