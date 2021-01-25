using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Windows;
using System.Windows.Media.Imaging;
using Newtonsoft.Json;
using tEngine.DataModel;
using tEngine.Helpers;

namespace tEngine.TActual.DataModel
{
    [DataContract]
    public class Slide {
        public enum SlideGrade {
            Essential, // важный
            Inessential // нет
        }

        public TData Data {
            get { return mData; }
        }

        public BitmapImage ImageBig { get; set; }
        public BitmapImage ImageMedium { get; set; }
        public BitmapImage ImageSmall { get; set; }

        public bool IsBaseReady {
            get { return mData.IsBaseData; }
        }

        public bool IsSpectrumReady {
            get { return mData.IsSpectrum; }
        }

        public Slide( Slide slide ) {
            this.Id = slide.Id;
            this.RareFactor_Left = slide.RareFactor_Left;
            this.RareFactor_Right = slide.RareFactor_Right;
            this.Index = slide.Index;
            this.Grade = slide.Grade;
            this.Name = slide.Name;
            this.IsShow = slide.IsShow;
            this.FileUri = slide.FileUri;
            this.mData = slide.Data;
            this.ImageBig = slide.ImageBig.CloneCurrentValue();
            this.ImageMedium = slide.ImageMedium.CloneCurrentValue();
            this.ImageSmall = slide.ImageSmall.CloneCurrentValue();
        }

        public Slide() {
            Id = Guid.NewGuid();
            Grade = SlideGrade.Inessential;
            RareFactor_Left = new double[3]; // todo убрать константы
            RareFactor_Right = new double[3]; 
            IsShow = true;
        }

        public byte[] ToArray() {
            var json = JsonConvert.SerializeObject( this,
                new JsonSerializerSettings {ContractResolver = new JSONContractResolver()} );
            var bt1 = Encoding.Unicode.GetBytes( json );
            return bt1;
        }

        public bool UriLoad() => UriLoad(FileUri);

        public bool UriLoad( Uri uri ) {
            try {
                // todo при кириллице может возникнуть переполнение пути, из-за змены символов
                ImageSmall = ImageHelper.Uri2BI( uri, ImageGrade.Small );
                ImageMedium = ImageHelper.Uri2BI( uri, ImageGrade.Medium );
                ImageBig = ImageHelper.Uri2BI( uri, ImageGrade.Big );
                //ImageTrue = ImageHelper.Uri2BI( uri );

                Id = Guid.NewGuid();
                Name = new FileInfo( uri.OriginalString ).Name;
                FileUri = uri;
                //ImageByte = ImageHelper.BI2Array( ImageTrue );
                return true;
            } catch( Exception ex ) {
                Debug.Assert( false, ex.ToString() );
                return false;
            }
        }

        private struct ImageGrade {
            public static readonly Size Big;
            public static readonly Size Medium;
            public static readonly Size Small;

            static ImageGrade() {
                Small = new Size( 100, 100 );
                Medium = new Size( 500, 500 );
                Big = new Size( SystemParameters.FullPrimaryScreenWidth, SystemParameters.FullPrimaryScreenHeight );
            }
        }

        #region Byte <=> Object

        public byte[] ToByteArray() {
            var obj = BytesPacker.JSONObj( this );
            var data = mData.ToByteArray();
            return BytesPacker.PackBytes( obj, data );
        }

        public bool LoadFromArray( byte[] array ) {
            var objData = BytesPacker.UnpackBytes( array );
            if( objData.Length != 2 ) return false;

            var obj = BytesPacker.LoadJSONObj<Slide>( objData[0] );
            this.Id = obj.Id;
            this.RareFactor_Left = obj.RareFactor_Left;
            this.RareFactor_Right = obj.RareFactor_Right;
            this.Index = obj.Index;
            this.Grade = obj.Grade;
            this.Name = obj.Name;
            this.IsShow = obj.IsShow;
            this.FileUri = obj.FileUri;

            var result = mData.LoadFromArray( objData[1] );
            return result;
        }

        #endregion

        
        public double RareFactor_Left_Summary {
            get { return RareFactor_Left.Average(); }
        }        
        public double RareFactor_Right_Summary {
            get { return RareFactor_Left.Average(); }
        }

        #region JSON

        [DataMember]
        public Uri FileUri { get; set; }

        [DataMember( Name = "ID" )]
        public Guid Id { get; private set; }

        private TData mData = new TData();
        public const string FOLDER_KEY = "LastNewImageFolder";

        //TODO: Переделать коэффициент необычности по человечески
        /// <summary>
        /// [тремор, спектр, корреляция]
        /// </summary>
        [DataMember( Name = "RF_Left" )]
        public double[] RareFactor_Left { get; set; }
        /// <summary>
        /// [тремор, спектр, корреляция]
        /// </summary>
        [DataMember( Name = "RF_Right" )]
        public double[] RareFactor_Right { get; set; }

        [DataMember]
        public int Index { get; set; }

        [DataMember]
        public SlideGrade Grade { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public bool IsShow { get; set; }

        #endregion
    }
}