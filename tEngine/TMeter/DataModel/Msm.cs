using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Windows.Forms;
using tEngine.DataModel;
using tEngine.Helpers;
using tEngine.TMeter.DataModel.IData;

namespace tEngine.TMeter.DataModel {
    [DataContract]
    public class Msm {
        private TData mData = new TData();

        public TData Data {
            get { return mData; }
        }

        /// <summary>
        /// Длительность измерения, сек
        /// </summary>
        public double MsmTime {
            get { return mData.Time; }
            set { mData.Time = value; }
        }

        private User Owner { get; set; }

        public Msm() {
            Init();
        }

        public Msm( Msm msm ) {
            Init();
            // полное копирование Msm
            LoadFromArray( msm.ToByteArray() );
            return;
            // todo проверить все ли копируется
            var pinfo = msm.GetType().GetProperties();
            pinfo.ToList().ForEach( info => {
                if( info.CanRead && info.CanWrite ) {
                    info.SetValue( this, info.GetValue( msm, null ), null );
                }
            } );
            this.mData = msm.mData;
        }

        public void AddData( Hand left, Hand right ) {
            mData.AddHands( left, right );
        }

        public void Clear() {
            mData.Clear();
        }

        public void Copy( Msm msm ) {
            Cloner.CopyAllProperties( this, msm );

            ID = msm.ID;
            Owner = msm.Owner;
            SetData( msm.Data );
        }

        public int DataLength() {
            return mData.DataLength();
        }

        public User GetOwner() {
            return Owner;
        }

        public static Msm GetTestMsm( User owner = null, string title = "Измерение" ) {
            var testMsm = new Msm() {
                Title = title,
                Comment = "Тестовое измерение",
                CreateTime = DateTime.Now,
                MsmTime = 5
            };


            var garmonics = 10;

            var length = (int) Math.Pow( 10, 1 );
            var dataTremor = Enumerable.Range( 0, length ).Select( i => {
                var result = 0.0;
                for( int f = 1; f < garmonics + 1; f++ )
                    result += 20*Math.Sin( (f*i*Math.PI/180.0)*5.0 );
                return (short) result;
            } ).ToList();
            var dataConst =
                Enumerable.Range( 0, length )
                    .Select( i => (short) (200 + dataTremor[i]) )
                    .ToList();
            testMsm.AddData(
                new Hand() {Const = dataConst, Tremor = dataTremor},
                new Hand() {
                    Const = dataConst.Select( s => (short) (s + 10) ).ToList(),
                    Tremor = dataTremor.Select( s => (short) (s + 10) ).ToList()
                } );

            testMsm.UserAssociated( owner );
            return testMsm;
        }

        public void Msm2CSV( string filePath ) {
            var sb = new StringBuilder();
            sb.AppendLine( "Time;Hz;Delta;" + "LConstant;LTremor;LSpectrum;LCorrelation;" +
                           "RConstant;RTremor;RSpectrum;RCorrelation" );

            var length = Data.Count;

            var Time = Data.GetConst( Hands.Left ).Select( dp => dp.X ).ToArray();

            var LConstant = Data.GetConst( Hands.Left ).Select( dp => dp.Y ).ToArray();
            var LTremor = Data.GetTremor( Hands.Left ).Select( dp => dp.Y ).ToArray();
            var RConstant = Data.GetConst( Hands.Right ).Select( dp => dp.Y ).ToArray();
            var RTremor = Data.GetTremor( Hands.Right ).Select( dp => dp.Y ).ToArray();

            var Hz = Data.GetSpectrum( Hands.Left ).Select( dp => dp.X ).ToArray();
            var LSpectrum = Data.GetSpectrum( Hands.Left ).Select( dp => dp.Y ).ToArray();
            var RSpectrum = Data.GetSpectrum( Hands.Right ).Select( dp => dp.Y ).ToArray();

            var Delta = Data.GetCorrelation( Hands.Left ).Select( dp => dp.X ).ToArray();
            var LCorrelation = Data.GetCorrelation( Hands.Left ).Select( dp => dp.Y ).ToArray();
            var RCorrelation = Data.GetCorrelation( Hands.Right ).Select( dp => dp.Y ).ToArray();


            for( int i = 0; i < length; i++ ) {
                if( i < length/2 ) {
                    sb.AppendLine( string.Format( "{0};{1};{2};{3};{4};{5};{6};{7};{8};{9};{10}",
                        Time[i],
                        Hz[i],
                        Delta[i],
                        LConstant[i],
                        LTremor[i],
                        LSpectrum[i],
                        LCorrelation[i],
                        RConstant[i],
                        RTremor[i],
                        RSpectrum[i],
                        RCorrelation[i]
                        ) );
                } else {
                    sb.AppendLine( string.Format( "{0};{1};{2};{3};{4};{5};{6};{7};{8};{9};{10}",
                        Time[i],
                        "\"\"",
                        Delta[i],
                        LConstant[i],
                        LTremor[i],
                        "\"\"",
                        LCorrelation[i],
                        RConstant[i],
                        RTremor[i],
                        "\"\"",
                        RCorrelation[i]
                        ) );
                }
            }
            FileIO.WriteText( filePath, sb.ToString() );
        }

        public void SetData( TData data ) {
            Debug.Assert( data != null );
            mData = data;
        }

        public void UserAssociated( User owner ) {
            Owner = owner;
        }

        private void Init() {
            ID = Guid.NewGuid();
        }

        #region JSON

        [DataMember]
        public string Title { get; set; }

        [DataMember]
        public Guid ID { get; protected set; }

        [DataMember]
        public string Comment { get; set; }

        /// <summary>
        /// Время и дата записи
        /// </summary>
        [DataMember]
        public DateTime CreateTime { get; set; }

        #endregion

        #region Byte <=> Object

        public byte[] ToByteArray() {
            var obj = BytesPacker.JSONObj( this );
            var data = mData.ToByteArray();
            return BytesPacker.PackBytes( obj, data );
        }

        public bool LoadFromArray( byte[] array ) {
            var objData = BytesPacker.UnpackBytes( array );
            if( objData.Length != 2 ) return false;
            var obj = BytesPacker.LoadJSONObj<Msm>( objData[0] );
            this.Title = obj.Title;
            this.ID = obj.ID;
            this.Comment = obj.Comment;
            this.CreateTime = obj.CreateTime;

            var result = mData.LoadFromArray( objData[1] );
            return result;
        }

        #endregion
    }
}