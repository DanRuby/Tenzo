using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using tEngine.DataModel;
using tEngine.Helpers;
using tEngine.TMeter.DataModel;

namespace tEngine.TActual.DataModel {
    [DataContract]
    public class PlayList {
        public enum MoveDirection {
            Up,
            Down
        }

        public enum SortBy {
            Index,
            RareFactor,
        }

        public const string FOLDER_KEY = "LastPlayListFolder";
        private bool mIsNotSaveChanges;
        public string FilePath { get; set; }

        public bool IsNotSaveChanges {
            get { return mIsNotSaveChanges; }
            set { mIsNotSaveChanges = value; }
        }

        public ReadOnlyCollection<Slide> Slides { get; set; }

        public PlayList( PlayList playList ) {
            var list = new List<Slide>();
            list.AddRange( playList.Slides.Select( slide => new Slide( slide ) ) );

            this.Slides = new ReadOnlyCollection<Slide>( list );
            this.ID = playList.ID;
            this.CreateTime = playList.CreateTime;
            this.SecondsToSlide = playList.SecondsToSlide;
            this.Title = playList.Title;
        }

        public PlayList() {
            ID = Guid.NewGuid();
            SecondsToSlide = 5;
            CreateTime = DateTime.Now;
            Slides = new ReadOnlyCollection<Slide>( new List<Slide>() );
            FilePath = DefaultPath( this );
            IsNotSaveChanges = false;
        }

        public void AddRangeSlide( IEnumerable<Slide> slides ) {
            var index = Slides.Count() + 1;
            var list = Slides.ToList();
            list.AddRange( slides.Select( slide => {
                slide.Index = index;
                return slide;
            } ) );
            Slides = new ReadOnlyCollection<Slide>( list );
            Sort();
            IsNotSaveChanges = true;
        }

        public void AddSlide( Slide slide ) {
            slide.Index = Slides.Count() + 1;
            var list = Slides.ToList();
            list.Add( slide );
            Slides = new ReadOnlyCollection<Slide>( list );
            Sort();
            IsNotSaveChanges = true;
        }

        public void MoveSlide( Slide slide, MoveDirection dir ) {
            var list = Slides.ToList();
            var indexOld = list.IndexOf( slide );
            var indexNew = dir == MoveDirection.Up ? indexOld - 1 : indexOld + 1;
            if( Math.Min( indexOld, indexNew ) >= 0 && Math.Max( indexOld, indexNew ) < list.Count ) {
                list[indexOld].Index = indexNew;
                list[indexNew].Index = indexOld;
            }
            Sort();
            IsNotSaveChanges = true;
        }

        public static bool Open( string filePath, out PlayList playList ) {
            try {
                string json;
                var result = FileIO.ReadText( filePath, out json );
                playList = JsonConvert.DeserializeObject<PlayList>( json );
                playList.FilePath = filePath;
                var folder = new FileInfo( playList.FilePath ).Directory;
                if( folder != null )
                    AppSettings.SetValue( PlayList.FOLDER_KEY, folder.FullName );
                if( playList.Slides == null ) playList.Slides = new ReadOnlyCollection<Slide>( new List<Slide>() );
                foreach( var slide in playList.Slides ) {
                    slide.UriLoad();
                }
                playList.Sort();

                playList.IsNotSaveChanges = false;
                return result;
            } catch( Exception ex ) {
                playList = new PlayList();
                return false;
            }
        }

        private double GetCorrelation(IEnumerable<double> data1, IEnumerable<double> data2) {
            if( data1 == null || !data1.Any() || data2 == null || !data2.Any() )
                return 0;
            var size = data1.Count();
            if( data2.Count() < size ) {
                size = data2.Count();
            }
            return MathNet.Numerics.Statistics.Correlation.Pearson( data1.Take( size ), data2.Take( size ) );
        }

        private void GetCorrelationMatrix(ref double[,,] matrix, double [][][] dataArray, int count) {
            for( int i = 0; i < count; i++ ) {
                var tremor1 = dataArray[i][0];
                var spectrum1 = dataArray[i][1];
                var corr1 = dataArray[i][2];
                for( int j = i; j < count; j++ ) {

                    var tremor2 = dataArray[j][0];
                    var spectrum2 = dataArray[j][1];
                    var corr2 = dataArray[j][2];

                    matrix[i, j, 0] = (GetCorrelation( tremor1, tremor2 ) + 1) / 2;
                    matrix[i, j, 1] = (GetCorrelation( spectrum1, spectrum2 ) + 1) / 2;
                    matrix[i, j, 2] = (GetCorrelation( corr1, corr2 ) + 1) / 2;
                    matrix[j, i, 0] = matrix[i, j, 0];
                    matrix[j, i, 1] = matrix[i, j, 1];
                    matrix[j, i, 2] = matrix[i, j, 2];
                }
            }
        }

        public void RareCalculate() {
            var count = Slides.Count;
            var leftData = new double[count][][];
            var rightData = new double[count][][];
            for( int i = 0; i < count; i++ ) {
                var left = Slides[i].Data.Left;
                var right = Slides[i].Data.Right;

                leftData[i] = new double[3][];
                rightData[i] = new double[3][];

                leftData[i][0] = left.IsTremor ? left.Tremor.Data.Select( dp => dp.Y ).ToArray() : null;
                leftData[i][1] = left.IsSpectrum ? left.Spectrum.Data.Select( dp => dp.Y ).ToArray() : null;
                leftData[i][2] = left.IsCorrelation ? left.Spectrum.Data.Select( dp => dp.Y ).ToArray() : null;

                rightData[i][0] = right.IsTremor ? right.Tremor.Data.Select( dp => dp.Y ).ToArray() : null;
                rightData[i][1] = right.IsSpectrum ? right.Spectrum.Data.Select( dp => dp.Y ).ToArray() : null;
                rightData[i][2] = right.IsCorrelation ? right.Spectrum.Data.Select( dp => dp.Y ).ToArray() : null;
            }

            // левая рука
            var rareLeft = new double[count, count, 3];
            GetCorrelationMatrix( ref rareLeft, leftData, count );
                     
            // правая рука
            var rareRight= new double[count, count, 3];
            GetCorrelationMatrix( ref rareRight, rightData, count );

            for( int i = 0; i < count; i++ ) {
                var slideRareLeft = new double[3] {0, 0, 0};
                var slideRareRight = new double[3] {0, 0, 0};
                for( int j = 0; j < count; j++ ) {
                    for( int k = 0; k < 3; k++ ) {
                        slideRareLeft[k] += rareLeft[i, j, k];
                        slideRareRight[k] += rareRight[i, j, k];
                    }
                }
                Slides[i].RareFactor_Left = slideRareLeft.ToArray();
                Slides[i].RareFactor_Right = slideRareRight.ToArray();
            }
            IsNotSaveChanges = true;
        }

        public int RemoveSlide( Slide slide ) {
            var list = Slides.ToList();
            var index = list.IndexOf( slide );
            list.Remove( slide );
            Slides = new ReadOnlyCollection<Slide>( list );
            Sort();

            while( index >= list.Count )
                index--;
            IsNotSaveChanges = true;
            return index;
        }

        public bool Save() {
            var filepath = string.IsNullOrEmpty( FilePath ) ? DefaultPath( this ) : FilePath;
            return Save( filepath );
        }

        public bool Save( string filePath ) {
            try {
                var dinfo = new FileInfo( filePath ).Directory;
                if( dinfo != null ) {
                    if( !dinfo.Exists ) dinfo.Create();

                    var settings = new JsonSerializerSettings {ContractResolver = new JSONContractResolver()};
                    var json = JsonConvert.SerializeObject( this, settings );
                    FileIO.WriteText( filePath, json );

                    IsNotSaveChanges = false;
                }
            } catch( Exception ex ) {
                ex = ex;
            }
            return true;
        }
        public void MixSlides() {            
            var list = Slides.OrderBy( slide => Guid.NewGuid() ).ToList();
            for( int i = 0; i < list.Count; i++ ) {
                list[i].Index = i;
            }
            Sort();
            IsNotSaveChanges = true;
        }

        public void Sort( SortBy key = SortBy.Index ) {
            IEnumerable<Slide> list = new List<Slide>();
            if( key == SortBy.Index ) {
                list = Slides.OrderBy( slide => slide.Index );
            } else if( key == SortBy.RareFactor ) {
                //list = Slides.OrderBy( slide => slide.RareFactor_Left );
            }
            list = list.Select( ( slide, i ) => {
                slide.Index = i + 1;
                return slide;
            } );
            Slides = new ReadOnlyCollection<Slide>( list.ToList() );
            IsNotSaveChanges = true;
        }

        private string DefaultPath( PlayList playList ) {
            var cDirectory = AppSettings.GetValue( PlayList.FOLDER_KEY, Constants.AppDataFolder );
            var filepath = cDirectory.CorrectSlash();
            filepath += playList.Title + Constants.PlayListExt;
            return filepath;
        }

        #region JSON

        [DataMember]
        public DateTime CreateTime { get; set; }

        [DataMember]
        public Guid ID { get; private set; }

        [DataMember]
        public double SecondsToSlide { get; set; }

        [DataMember]
        public string Title { get; set; }

        #endregion

        #region Byte <=> Object

        public byte[] ToByteArray() {
            var obj = BytesPacker.JSONObj( this );
            var slidesData = new byte[Slides.Count][];
            for( int i = 0; i < Slides.Count; i++ ) {
                var slide = Slides[i];
                slidesData[i] = slide.ToByteArray();
            }
            var slides = BytesPacker.PackBytes( slidesData );

            return BytesPacker.PackBytes( obj, slides );
        }

        public bool LoadFromArray( byte[] array ) {
            var objData = BytesPacker.UnpackBytes( array );
            if( objData.Length != 2 ) return false;

            var obj = BytesPacker.LoadJSONObj<PlayList>( objData[0] );
            this.ID = obj.ID;
            this.CreateTime = obj.CreateTime;
            this.SecondsToSlide = obj.SecondsToSlide;
            this.Title = obj.Title;

            var slidesArray = BytesPacker.UnpackBytes( objData[1] );
            var result = true;
            foreach( byte[] bytes in slidesArray ) {
                var newSlide = new Slide();
                result = result && newSlide.LoadFromArray( bytes );
                this.AddSlide( newSlide );
            }
            IsNotSaveChanges = false;
            return result;
        }

        #endregion


    }
}