using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Runtime.Remoting.Contexts;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Annotations;
using System.Windows.Converters;
using MathNet.Numerics.IntegralTransforms;
using MathNet.Numerics.Statistics;
using Microsoft.Win32.SafeHandles;
using OxyPlot;
using tEngine.Helpers;

namespace tEngine.DataModel {
    public enum Hands {
        Left,
        Right
    }


    [DataContract]
    public class TData {
        private static int mNowTaskCount = 0;
        private readonly Mutex mMutex = new Mutex();
        private Hand[] mHands = new Hand[2] {new Hand(), new Hand()};
        private List<HAND_DATA> mHandsData = new List<HAND_DATA>() {new HAND_DATA(), new HAND_DATA()};
        private double mTime;

        [DataMember]
        public int BeginPoint {
            get { return mHands[0].BeginPoint; }
            set {
                mHands[0].BeginPoint = value;
                mHands[1].BeginPoint = value;
                ClearData();
            }
        }

        /// <summary>
        /// Количество выделенных измерений доступных для анализа
        /// </summary>
        public int Count {
            get { return mHandsData[0].Constant.Count; }
        }

        /// <summary>
        /// Количество имеющихся изерений
        /// </summary>
        public int CountBase {
            get { return mHands[0].Const.Count; }
        }

        [DataMember]
        public int EndPoint {
            get { return mHands[0].EndPoint; }
            set {
                mHands[0].EndPoint = value;
                mHands[1].EndPoint = value;
                ClearData();
            }
        }

        public bool IsBaseData {
            get {
                return mHandsData[0].IsConstant && mHandsData[1].IsConstant &&
                       mHandsData[0].IsTremor && mHandsData[1].IsTremor;
            }
        }

        public bool IsCorrelation {
            get { return mHandsData[0].IsCorrelation && mHandsData[1].IsCorrelation; }
        }

        public bool IsSomeData {
            get { return !(mHands[0].Const.IsNullOrEmpty() || mHands[1].Const.IsNullOrEmpty()); }
        }

        public bool IsSpectrum {
            get { return mHandsData[0].IsSpectrum && mHandsData[1].IsSpectrum; }
        }

        public HAND_DATA Left {
            get { return mHandsData[0]; }
        }

        public HAND_DATA Right {
            get { return mHandsData[1]; }
        }

        /// <summary>
        /// Время в секундах
        /// </summary>
        [DataMember]
        public double Time {
            get { return mTime; }
            set { mTime = value; }
        }

        public TData() {
            Time = AppSettings.GetValue( "DataTime", 30.0 );
        }

        public void AddHands( Hand left, Hand right ) {
            mHands[0] += left;
            mHands[1] += right;
        }

        /// <summary>
        /// Добавляет в очередь поток базовой обработки. 
        /// Запуск через TData.StartCalc(). Финальная функция через TData.AddTaskFinal()
        /// </summary>
        /// <returns></returns>
        public Guid BaseAnalys( Action<double, double> percentCallBack, Action<TData, bool> finalCallBack ) {
            var id = Guid.NewGuid();
            var cts = new CancellationTokenSource();

            // посылает значения (предполагается процент выполнения на каждую руку)
            var percentAction = new Action<double, double>( ( h1, h2 ) => {
                if( percentCallBack != null ) {
                    percentCallBack( h1, h2 );
                }
            } );
            // Заполнение Const и Tremor на основе Hands и Time
            var taska = new Task( ( param ) => {
                mMutex.WaitOne();
                ClearData();

                var result = Async_BaseAnalys( (CancellationToken) param, percentAction );
                if( result == false )
                    ClearData();
                if( finalCallBack != null )
                    finalCallBack( this, result );
                RemoveTaskQueue( id );
                mMutex.ReleaseMutex();
            }, cts.Token, cts.Token );

            AddTaskQueue( id, taska, cts );

            return id;
        }

        public void Clear() {
            lock( mLock ) {
                for( int i = 0; i < 2; i++ ) {
                    mHands[i].Clear();
                    mHandsData[i].Clear();
                }
            }
        }

        /// <summary>
        /// Чистит только то что можно пересчитать
        /// </summary>
        public void ClearData() {
            lock( mLock ) {
                for( int i = 0; i < 2; i++ ) {
                    mHandsData[i].Clear();
                }
            }
        }

        public int DataLength() {
            return mHands[0].GetLength();
        }

        public IList<DataPoint> GetConst( Hands hand ) {
            var index = (hand == Hands.Left) ? 0 : 1;
            return mHandsData[index].Constant.Data;
        }

        public IEnumerable<short> GetConstBase( Hands hand ) {
            var index = (hand == Hands.Left) ? 0 : 1;
            return mHands[index].Const;
        }

        public IList<DataPoint> GetCorrelation( Hands hand ) {
            var index = (hand == Hands.Left) ? 0 : 1;
            return mHandsData[index].Correlation.Data;
        }

        public IList<DataPoint> GetSpectrum( Hands hand ) {
            var index = (hand == Hands.Left) ? 0 : 1;
            return mHandsData[index].Spectrum.Data;
        }

        /// <summary>
        /// Возвращает спектр на заданном диапазоне частот (из имеющихся)
        /// </summary>
        /// <param name="hand"></param>
        /// <param name="minimum">Нижняя граница, Гц</param>
        /// <param name="maximum">Верхняя граница, Гц</param>
        /// <returns></returns>
        public IList<DataPoint> GetSpectrumByHz( Hands hand, double minimum, double maximum ) {
            var start = -1;
            var end = -1;
            var index = (hand == Hands.Left) ? 0 : 1;
            if( mHandsData[index].Spectrum.Data == null ) return null;

            if( double.IsNaN( minimum ) == false ) {
                var sortValues = mHandsData[index].Spectrum.Data.OrderBy( dp => dp.X );
                var goodValues = sortValues.Where( dp => dp.X >= minimum );
                var first = goodValues.Any() ? goodValues.First() : sortValues.FirstOrDefault();


                Debug.Assert( first.IsDefined() );
                start = mHandsData[index].Spectrum.Data.IndexOf( first );
            }
            if( double.IsNaN( maximum ) == false ) {
                var sortValues = mHandsData[index].Spectrum.Data.OrderBy( dp => dp.X );
                var goodValues = sortValues.Where( dp => dp.X >= maximum );
                var last = goodValues.Any() ? goodValues.Last() : sortValues.LastOrDefault();

                Debug.Assert( last.IsDefined() );
                end = mHandsData[index].Spectrum.Data.IndexOf( last );
            }
            return GetSpectrum( hand ).Where( ( dp, i ) => i > start && i < end ).ToList();
        }

        public IList<DataPoint> GetTremor( Hands hand ) {
            var index = (hand == Hands.Left) ? 0 : 1;
            return mHandsData[index].Tremor.Data;
        }

        /// <summary>
        /// Возвращает амплитуду тремора
        /// </summary>
        /// <param name="hand"></param>
        /// <returns></returns>
        public double GetTremorAmplitude( Hands hand ) {
            var index = (hand == Hands.Left) ? 0 : 1;
            var interval = mHandsData[index].Tremor.Max - mHandsData[index].Tremor.Min;
            return Math.Abs( interval );
        }

        /// <summary>
        /// возвращает просто массив без учета времени
        /// </summary>
        /// <param name="hand"></param>
        /// <returns></returns>
        public IEnumerable<short> GetTremorBase( Hands hand ) {
            var index = (hand == Hands.Left) ? 0 : 1;
            return mHands[index].Tremor;
        }

        /// <summary>
        /// Возвращает процент тремора от средней постоянной
        /// </summary>
        /// <param name="hand"></param>
        /// <returns></returns>
        public double GetTremorPercent( Hands hand ) {
            var index = (hand == Hands.Left) ? 0 : 1;
            var tremor = GetTremorAmplitude( hand );
            var force = mHandsData[index].Constant.Mean;
            return tremor*100.0/force;
        }

        public bool LoadFromArray( byte[] array ) {
            var handsData = BytesPacker.UnpackBytes( array );

            if( handsData.Length >= 2 ) {
                var h1 = mHands[0].LoadFromArray( handsData[0] );
                var h2 = mHands[1].LoadFromArray( handsData[1] );
                if( (h1 || h2) == false )
                    return false;
            }

            if( handsData.Length >= 8 ) {
                var const0 = handsData[2].GetCollectionDataPoint();
                var const1 = handsData[3].GetCollectionDataPoint();
                var tremor0 = handsData[4].GetCollectionDataPoint();
                var tremor1 = handsData[5].GetCollectionDataPoint();
                var spectrum0 = handsData[6].GetCollectionDataPoint();
                var spectrum1 = handsData[7].GetCollectionDataPoint();

                if( const0 != null && const1 != null &&
                    tremor0 != null && tremor1 != null ) {
                    mHandsData[0].Constant.Data = const0.ToList();
                    mHandsData[1].Constant.Data = const1.ToList();
                    mHandsData[0].Tremor.Data = tremor0.ToList();
                    mHandsData[1].Tremor.Data = tremor1.ToList();
                    if( spectrum0 != null && spectrum1 != null ) {
                        mHandsData[0].Spectrum.Data = spectrum0.ToList();
                        mHandsData[1].Spectrum.Data = spectrum1.ToList();
                    }
                }
            }
            if( handsData.Length >= 9 ) {
                var obj = BytesPacker.LoadJSONObj<TData>( handsData[8] );
                this.Time = obj.Time;
            }

            if( handsData.Length >= 11 ) {
                var corr0 = handsData[9].GetCollectionDataPoint();
                var corr1 = handsData[10].GetCollectionDataPoint();
                if( corr0 != null && corr1 != null ) {
                    mHandsData[0].Correlation.Data = corr0.ToList();
                    mHandsData[1].Correlation.Data = corr1.ToList();
                }
            }
            return true;
        }

        public Guid SpectrumAnalys( Action<double, double> percentCallBack, Action<TData, bool> finalCallBack,
            bool corr = false ) {
            var id = Guid.NewGuid();
            var cts = new CancellationTokenSource();

            var percentAction = new Action<double, double>( ( h1, h2 ) => {
                if( percentCallBack != null ) {
                    percentCallBack( h1, h2 );
                }
            } );

            // Заполнение 
            var taska = new Task( ( param ) => {
                mMutex.WaitOne();
                lock( mLock ) {
                    mHandsData.ForEach( hd => hd.Spectrum.Clear() );
                    mHandsData.ForEach( hd => hd.Correlation.Clear() );
                }

                var result = Async_SpectrumAnalys( (CancellationToken) param, percentAction, corr );
                if( result == false ) {
                    lock( mLock ) {
                        mHandsData.ForEach( hd => hd.Spectrum.Clear() );
                        mHandsData.ForEach( hd => hd.Correlation.Clear() );
                    }
                }
                if( finalCallBack != null )
                    finalCallBack( this, result );
                RemoveTaskQueue( id );
                mMutex.ReleaseMutex();
            }, cts.Token, cts.Token );

            AddTaskQueue( id, taska, cts );
            return id;
        }

        public byte[] ToByteArray() {
            var hand1 = mHands[0].ToByteArray();
            var hand2 = mHands[1].ToByteArray();
            var const1 = mHandsData[0].Constant.Data.ToByteArray();
            var const2 = mHandsData[1].Constant.Data.ToByteArray();
            var tremor1 = mHandsData[0].Tremor.Data.ToByteArray();
            var tremor2 = mHandsData[1].Tremor.Data.ToByteArray();
            var spectrum1 = mHandsData[0].Spectrum.Data.ToByteArray();
            var spectrum2 = mHandsData[1].Spectrum.Data.ToByteArray();
            var corr1 = mHandsData[0].Correlation.Data.ToByteArray();
            var corr2 = mHandsData[1].Correlation.Data.ToByteArray();
            var json = BytesPacker.JSONObj( this );
            var handsData = BytesPacker.PackBytes( hand1, hand2, const1, const2, tremor1, tremor2, spectrum1, spectrum2,
                json, corr1, corr2 );

            return handsData;
        }

        /// <summary>
        /// Формирует HAND_DATA на основе объектов Hand
        /// </summary>
        /// <param name="cancelTok"></param>
        /// <param name="percentAction"></param>
        /// <returns></returns>
        private bool Async_BaseAnalys( CancellationToken cancelTok, Action<double, double> percentAction ) {
            if( cancelTok.IsCancellationRequested ) return false;

            var percent = new double[] {0, 0};
            percentAction( percent[0], percent[1] );

            for( int i = 0; i < 2; i++ ) {
                var cd = mHands[i].Const.Where( ( s, index ) => (index >= BeginPoint && index <= EndPoint) );
                var td = mHands[i].Tremor.Where( ( s, index ) => (index >= BeginPoint && index <= EndPoint) );
                var constData = cd.Any() ? cd.ToList() : null;
                var tremorData = td.Any() ? td.ToList() : null;

                // проверка на возможность расчетов
                if( constData == null || tremorData == null ||
                    constData.Count == 0 || tremorData.Count == 0 ) {
                    return false;
                }

                // временной шаг, считаем что все промежутки между отсчетами равны
                var dt = Time/mHands[i].GetLength();

                percent[i] = 10;
                percentAction( percent[0], percent[1] );

                mHandsData[i].Constant.Data =
                    constData.Select( ( s, index ) => new DataPoint( index*dt, s ) ).ToList();
                if( cancelTok.IsCancellationRequested ) return false;

                percent[i] = 45;
                percentAction( percent[0], percent[1] );

                mHandsData[i].Tremor.Data =
                    tremorData.Select( ( s, index ) => new DataPoint( index*dt, s ) ).ToList();
                if( cancelTok.IsCancellationRequested ) return false;

                percent[i] = 100;
                percentAction( percent[0], percent[1] );
            }
            return true;
        }

        private bool Async_SpectrumAnalys( CancellationToken cancelTok, Action<double, double> percentAction, bool corr ) {
            if( cancelTok.IsCancellationRequested ) return false;
            // Посылка начальных значений процентов
            var percent = new double[] {0, 0};
            percentAction( percent[0], percent[1] );

            for( int i = 0; i < 2; i++ ) {
                var tremorData = mHandsData[i].Tremor.Data;

                if( tremorData == null || tremorData.Count == 0 ) return false;

                var samples = tremorData.Select( item => new Complex( item.Y, 0.0 ) ).ToArray();

                Fourier.Forward( samples, FourierOptions.Matlab );
                var N = samples.Count();
                if( N > 2 ) {
                    mHandsData[i].Spectrum.Data =
                        samples.Take( (int) Math.Ceiling( N/2.0 ) )
                            .Select( ( cmpl, x ) => new DataPoint( x/Time, cmpl.Magnitude*(x == 0 ? 1 : 2)/N ) )
                            .ToList();
                }
                if( cancelTok.IsCancellationRequested ) return false;
                if( corr ) {
                    var dt = Time/mHandsData[i].Constant.Count;
                    // спектральная плотность
                    var den = samples.Select( complex => complex*complex ).ToArray();
                    Fourier.Inverse( den, FourierOptions.Matlab );
                    // перенос второй части в отрицательую сторону
                    var secondCount = (int) Math.Ceiling( N/2.0 );
                    var firstCount = den.Count() - secondCount;
                    var second =
                        den.Take( secondCount ).Select( ( complex, x ) => new DataPoint( dt*x, complex.Magnitude ) );
                    var first =
                        den.Skip( secondCount )
                            .Select( ( complex, x ) => new DataPoint( dt*(x - firstCount), complex.Magnitude ) );
                    mHandsData[i].Correlation.Data = first.Concat( second ).ToList();
                }

                percent[i] = 100;
                percentAction( percent[0], percent[1] );
            }
            return true;
        }

        //private void CalculateBaseParam() {
        //    if( mConstant[0] == null || mConstant[0].Count == 0 ||
        //        mConstant[1] == null || mConstant[1].Count == 0 ||
        //        mTremor[0] == null || mTremor[0].Count == 0 ||
        //        mTremor[1] == null || mTremor[1].Count == 0 ) {
        //        mConstant = new List<DataPoint>[2];
        //        mTremor = new List<DataPoint>[2];
        //        mBaseParameters.Clear();
        //        return;
        //    }

        //    lock( mLock ) {
        //        mBaseParameters["mean_" + Hands.Left] = mConstant[0].Average( point => point.Y );
        //        mBaseParameters["mean_" + Hands.Right] = mConstant[1].Average( point => point.Y );

        //        mBaseParameters["min_" + Hands.Left] = mConstant[0].Min( point => point.Y );
        //        mBaseParameters["min_" + Hands.Right] = mConstant[1].Min( point => point.Y );

        //        mBaseParameters["max_" + Hands.Left] = mConstant[0].Min( point => point.Y );
        //        mBaseParameters["max_" + Hands.Right] = mConstant[1].Max( point => point.Y );

        //        mBaseParameters["interval_min_" + Hands.Left] = mTremor[0].Min( point => point.Y );
        //        mBaseParameters["interval_min_" + Hands.Right] = mTremor[1].Min( point => point.Y );

        //        mBaseParameters["interval_max_" + Hands.Left] = mTremor[0].Max( point => point.Y );
        //        mBaseParameters["interval_max_" + Hands.Right] = mTremor[1].Max( point => point.Y );


        //        // параметры для сравнения
        //        mBaseParameters["tremor_mean1" + Hands.Left] = Param1( mTremor[0], DataMode.First );
        //        mBaseParameters["tremor_mean1" + Hands.Right] = Param1( mTremor[1], DataMode.First );

        //        var df_left1 = Param2( mTremor[0], DataMode.First );
        //        var df_rigth1 = Param2( mTremor[1], DataMode.First );
        //        mBaseParameters["tremor_d1+" + Hands.Left] = df_left1[0];
        //        mBaseParameters["tremor_d1+" + Hands.Right] = df_rigth1[0];
        //        mBaseParameters["tremor_d1-" + Hands.Left] = df_left1[1];
        //        mBaseParameters["tremor_d1-" + Hands.Right] = df_rigth1[1];

        //        mBaseParameters["tremor_mean2" + Hands.Left] = Param1( mTremor[0], DataMode.Second );
        //        mBaseParameters["tremor_mean2" + Hands.Right] = Param1( mTremor[1], DataMode.Second );

        //        var df_left2 = Param2( mTremor[0], DataMode.Second );
        //        var df_rigth2 = Param2( mTremor[1], DataMode.Second );
        //        mBaseParameters["tremor_d2+" + Hands.Left] = df_left2[0];
        //        mBaseParameters["tremor_d2+" + Hands.Right] = df_rigth2[0];
        //        mBaseParameters["tremor_d2-" + Hands.Left] = df_left2[1];
        //        mBaseParameters["tremor_d2-" + Hands.Right] = df_rigth2[1];
        //    }
        //}

        //private void CalculateSpectrumParam() {
        //    if( mSpectrum[0] == null || mSpectrum[0].Count == 0 ||
        //        mSpectrum[1] == null || mSpectrum[1].Count == 0 ) {
        //        mSpectrum = new List<DataPoint>[2];
        //        mSpectrumParameters.Clear();
        //        return;
        //    }
        //    lock( mLock ) {
        //        // параметры для сравнения
        //        mSpectrumParameters["spectrum_mean1" + Hands.Left] = Param1( mSpectrum[0], DataMode.First );
        //        mSpectrumParameters["spectrum_mean1" + Hands.Right] = Param1( mSpectrum[1], DataMode.First );

        //        var df_left1 = Param2( mSpectrum[0], DataMode.First );
        //        var df_rigth1 = Param2( mSpectrum[1], DataMode.First );
        //        mSpectrumParameters["spectrum_d1+" + Hands.Left] = df_left1[0];
        //        mSpectrumParameters["spectrum_d1+" + Hands.Right] = df_rigth1[0];
        //        mSpectrumParameters["spectrum_d1-" + Hands.Left] = df_left1[1];
        //        mSpectrumParameters["spectrum_d1-" + Hands.Right] = df_rigth1[1];

        //        mSpectrumParameters["spectrum_mean2" + Hands.Left] = Param1( mSpectrum[0], DataMode.Second );
        //        mSpectrumParameters["spectrum_mean2" + Hands.Right] = Param1( mSpectrum[1], DataMode.Second );

        //        var df_left2 = Param2( mSpectrum[0], DataMode.Second );
        //        var df_rigth2 = Param2( mSpectrum[1], DataMode.Second );
        //        mSpectrumParameters["spectrum_d2+" + Hands.Left] = df_left2[0];
        //        mSpectrumParameters["spectrum_d2+" + Hands.Right] = df_rigth2[0];
        //        mSpectrumParameters["spectrum_d2-" + Hands.Left] = df_left2[1];
        //        mSpectrumParameters["spectrum_d2-" + Hands.Right] = df_rigth2[1];
        //    }
        //}

        /// <summary>
        /// Возвращает часть коллекции в заданном диапазоне, с заданным разрешением
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        /// <param name="start">Первый элемент</param>
        /// <param name="end">Последний элемент</param>
        /// <param name="length">Сколько требуется элементов</param>
        /// <returns></returns>
        private IList<T> GetPartOfCollection<T>( IList<T> collection, uint? start, uint? end, uint? length ) {
            lock( mLock ) {
                if( collection.IsNullOrEmpty() ) return null;

                var startIndex = start ?? 0;
                var endIndex = end ?? (uint) (collection.Count() - 1);

                // сколько можно отдать
                var hasValues = Math.Abs( (double) endIndex - startIndex ) + 1;
                // сколько требуют
                var requireValues = length ?? hasValues;

                var result = collection;
                if( requireValues <= hasValues && requireValues >= hasValues/2 ) {
                    // требуют меньше чем есть, но больше половины -> исключаем каждый k-ый
                    var k = (int) Math.Floor( hasValues/(hasValues - requireValues) );
                    result = collection.Where( ( o, i ) => i >= startIndex && i <= endIndex && i%k != 0 ).ToList();
                } else if( requireValues < hasValues/2 ) {
                    //требуют меньше половины -> берем каждый k-ый
                    var k = (int) Math.Floor( hasValues/requireValues );
                    result = collection.Where( ( o, i ) => i >= startIndex && i <= endIndex && i%k == 0 ).ToList();
                } else if( requireValues >= hasValues ) {
                    //требуют больше чем есть
                    result = collection.Where( ( o, i ) => i >= startIndex && i <= endIndex ).ToList();
                }
                return result;
            }
        }

        private double Param1( IEnumerable<DataPoint> data, DataMode mode = DataMode.First ) {
            if( mode == DataMode.First ) return data.Average( dp => dp.Y );
            if( mode == DataMode.Second ) return data.Average( dp => dp.Y*dp.X );
            return data.Average( dp => dp.Y );
        }

        private double[] Param2( IEnumerable<DataPoint> data, DataMode mode = DataMode.First ) {
            var result = new[] {0d, 0d};
            var count = data.Count();
            if( mode == DataMode.First ) {
                for( int i = 0; i < count - 1; i++ ) {
                    var delta = data.ElementAt( i + 1 ).Y - data.ElementAt( i ).Y;
                    if( delta > 0 ) {
                        result[0] += delta;
                    } else {
                        result[1] += delta;
                    }
                }
            } else if( mode == DataMode.Second ) {
                for( int i = 0; i < count - 1; i++ ) {
                    var delta = data.ElementAt( i + 1 ).Y*data.ElementAt( i + 1 ).X -
                                data.ElementAt( i ).Y*data.ElementAt( i ).X;
                    if( delta > 0 ) {
                        result[0] += delta;
                    } else {
                        result[1] += delta;
                    }
                }
            }
            return result;
        }

        private enum DataMode {
            First,
            Second,
        }

        #region TaskPool

        #region Queue

        private const int INDEX_ID = 0;
        private const int INDEX_TASK = 1;
        private const int INDEX_CTS = 2;


        [NonSerialized]
        private static List<object[]> TaskPoolQueue = new List<object[]>();

        public static void AddTaskQueue( Guid id, Task taska, CancellationTokenSource cts ) {
            lock( mLock ) {
                if( TaskPoolQueue.Any() )
                    ((Task) TaskPoolQueue.Last()[INDEX_TASK]).ContinueWith( t => { taska.Start(); } );
                TaskPoolQueue.Add( new object[] {id, taska, cts} );
            }
        }


        public static Guid AddTaskFinal( Task taska, CancellationTokenSource cts ) {
            var id = Guid.NewGuid();
            taska.ContinueWith( task => { RemoveTaskQueue( id ); } );
            AddTaskQueue( id, taska, cts );
            return id;
        }

        private static Guid mCurrentPack = Guid.Empty;

        public static void StartCalc() {
            lock( mLock ) {
                if( TaskPoolQueue.Any() )
                    ((Task) TaskPoolQueue.First()[INDEX_TASK]).Start();
            }
        }


        public static void CancelCalc() {
            lock( mLock ) {
                foreach( var task in TaskPoolQueue ) {
                    ((CancellationTokenSource) task[INDEX_CTS]).Cancel();
                }
            }
        }

        private static void RemoveTaskQueue( Guid id ) {
            lock( mLock ) {
                var first = TaskPoolQueue.First( objects => objects[INDEX_ID].Equals( id ) );
                if( first != null ) {
                    TaskPoolQueue.Remove( first );
                }
            }
        }

        #endregion

        ///// <summary>
        ///// id -> [ Task, CancellationTokenSource ]
        ///// </summary>
        ///// 
        //[NonSerialized]
        //private static Dictionary<Guid, object[]> TaskPool = new Dictionary<Guid, object[]>();

        //[NonSerialized]
        //private static Dictionary<Guid, object[]> TaskPoolFinal = new Dictionary<Guid, object[]>();

        private static readonly object mLock = new object();


        //private static void AddTask( Guid id, Task taska, CancellationTokenSource cts, bool final = false ) {
        //    if( final ) {
        //        TaskPoolFinal.Add( id, new object[] {taska, cts} );
        //    } else {
        //        TaskPool.Add( id, new object[] {taska, cts} );
        //    }
        //}

        //private static void RemoveTask( Guid id ) {
        //    if( TaskPool.ContainsKey( id ) ) {
        //        TaskPool.Remove( id );
        //    } else if( TaskPoolFinal.ContainsKey( id ) ) {
        //        TaskPoolFinal.Remove( id );
        //    }
        //}

        //public static CancellationTokenSource GetCancellationTokenSource( Guid id ) {
        //    if( TaskPool.ContainsKey( id ) )
        //        return TaskPool[id][1] as CancellationTokenSource;
        //    else if( TaskPoolFinal.ContainsKey( id ) )
        //        return TaskPoolFinal[id][1] as CancellationTokenSource;
        //    return null;
        //}

        //public static Task GetTask( Guid id ) {
        //    if( TaskPool.ContainsKey( id ) )
        //        return TaskPool[id][0] as Task;
        //    else if( TaskPoolFinal.ContainsKey( id ) )
        //        return TaskPoolFinal[id][0] as Task;
        //    return null;
        //}

        //public static Guid AddFinal( Action action ) {
        //    var id = Guid.NewGuid();
        //    var cts = new CancellationTokenSource();
        //    var lastTaskID = TaskPool.Last().Key;

        //    var taska = Task.Factory.StartNew( ( param ) => {
        //        var cancelTok = (CancellationToken) param;
        //        while( !cancelTok.IsCancellationRequested ) {
        //            if( TaskPool.Count == 0 ) {
        //                if( action != null )
        //                    action();
        //                break;
        //            }
        //            Thread.Sleep( 20 );
        //        }
        //        RemoveTask( id );
        //    }, cts.Token, cts.Token );
        //    AddTask( id, taska, cts, true );
        //    return id;
        //}

        #endregion
    }

    public class HAND_DATA {
        public STDATA Constant { get; set; }
        public STDATA Correlation { get; set; }

        public bool IsConstant {
            get { return Constant.IsData; }
        }

        public bool IsCorrelation {
            get { return Correlation.IsData; }
        }

        public bool IsSpectrum {
            get { return Spectrum.IsData; }
        }

        public bool IsTremor {
            get { return Tremor.IsData; }
        }

        public STDATA Spectrum { get; set; }
        public STDATA Tremor { get; set; }

        public HAND_DATA() {
            Constant = new STDATA();
            Correlation = new STDATA();
            Spectrum = new STDATA();
            Tremor = new STDATA();
        }

        public void Clear() {
            Constant.Clear();
            Correlation.Clear();
            Spectrum.Clear();
            Tremor.Clear();
        }
    }

    public class STDATA {
        private List<DataPoint> mData;
        public int Count { get; set; }

        public List<DataPoint> Data {
            get { return mData; }
            set {
                mData = value;
                if( mData != null ) {
                    Count = mData.Count;
                    if( Count > 0 ) {
                        Mean = mData.Average( point => point.Y );
                        Min = mData.Min( point => point.Y );
                        Max = mData.Max( point => point.Y );
                    } else {
                        Mean = 0;
                        Min = 0;
                        Max = 0;
                    }
                }
            }
        }

        public bool IsData {
            get { return Data != null && Count > 0; }
        }

        public double Max { get; private set; }
        public double Mean { get; private set; }
        public double Min { get; private set; }

        public STDATA() {
            Data = new List<DataPoint>();
        }

        public void Clear() {
            Data = new List<DataPoint>();
        }
    }
}