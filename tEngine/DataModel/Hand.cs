using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using tEngine.Helpers;

namespace tEngine.DataModel {
    /// <summary>
    /// список значение на каждую руку
    /// </summary>
    [DataContract]
    public class Hand {
        private const int MaxLength = 50*1024*1024; // 50 мБ

        /// <summary>
        /// точка начала выбранного диапазона
        /// </summary>
        [DataMember]
        public int BeginPoint { get; set; }

        [DataMember]
        public List<short> Const { get; set; }

        /// <summary>
        /// точка конца выбранного диапазона
        /// </summary>
        [DataMember]
        public int EndPoint { get; set; }

        [DataMember]
        public List<short> Tremor { get; set; }

        public Hand() {
            Const = new List<short>();
            Tremor = new List<short>();
            ResetPoints();
        }

        public void Clear() {
            Const.Clear();
            Tremor.Clear();
            ResetPoints();
        }

        private void ResetPoints() {
            BeginPoint = 0;
            EndPoint = Const.Count - 1;
        }

        /// <summary>
        /// Возвращает count последних значений
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        public Hand GetHand( int count ) {
            if( count > Const.Count )
                count = Const.Count;
            var first = Const.Count - count;

            var result = new Hand();
            result.Const = Const.GetRange( first, count );
            result.Tremor = Tremor.GetRange( first, count );

            return result;
        }

        public int GetLength() {
            if( Const != null )
                return Const.Count;
            return 0;
        }

        public bool LoadFromArray( byte[] array ) {
            var data = BytesPacker.UnpackBytes( array );
            if( data.Length != 2 || data[0].Length != data[1].Length )
                return false;
            Const = data[0].GetCollectionInt16().ToList();
            Tremor = data[1].GetCollectionInt16().ToList();
            return true;
        }

        //TODO заменить на функцию void Add(Hand)
        public static Hand operator +( Hand a, Hand b ) {
            if( a == null )
                a = new Hand();
            if( b == null )
                b = new Hand();
            var result = new Hand();
            result.Const.AddRange( a.Const );
            result.Const.AddRange( b.Const );

            result.Tremor.AddRange( a.Tremor );
            result.Tremor.AddRange( b.Tremor );

            var notFit = result.Const.Count - MaxLength;
            if( notFit > 0 )
                result.Const.RemoveRange( 0, notFit );
            result.ResetPoints();
            return result;
        }

        public byte[] ToByteArray() {
            return BytesPacker.PackBytes( Const.ToByteArray(), Tremor.ToByteArray() );
        }
    }
}