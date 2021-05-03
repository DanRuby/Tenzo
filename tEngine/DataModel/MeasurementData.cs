using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using MathNet.Numerics.IntegralTransforms;
using OxyPlot;
using tEngine.Helpers;

namespace tEngine.DataModel
{
    public enum Hands
    {
        Left=0,
        Right=1
    }

    /// <summary>
    /// Инкапсуляция данных измерения
    /// </summary>
    [DataContract]
    public class MeasurementData
    {
        private readonly Mutex mMutex = new Mutex();
        private HandRawData[] mHands = new HandRawData[2] { new HandRawData(), new HandRawData() };
        private List<HandGraphs> mHandsData = new List<HandGraphs>() { new HandGraphs(), new HandGraphs() };
        private double mTime;

        /// <summary>
        /// Коэффициент корреляции тремора
        /// </summary>
        [DataMember]
        public float CorrelationCoef { get; private set; }

        [DataMember]
        public int BeginPoint
        {
            get => mHands[(int)Hands.Left].BeginPoint;
            set
            {
                mHands[(int)Hands.Left].BeginPoint = value;
                mHands[(int)Hands.Right].BeginPoint = value;
                ClearData();
            }
        }

        /// <summary>
        /// Количество выделенных измерений доступных для анализа
        /// </summary>
        public int Count => mHandsData[(int)Hands.Left].Constant.Count;

        /// <summary>
        /// Количество имеющихся изерений
        /// </summary>
        public int CountBase => mHands[(int)Hands.Left].Constant.Count;

        [DataMember]
        public int EndPoint
        {
            get => mHands[(int)Hands.Left].EndPoint;
            set
            {
                mHands[(int)Hands.Left].EndPoint = value;
                mHands[(int)Hands.Right].EndPoint = value;
                ClearData();
            }
        }

        public bool HasBaseData => mHandsData[(int)Hands.Left].HasConstant && mHandsData[(int)Hands.Right].HasConstant &&
                       mHandsData[(int)Hands.Left].HasTremor && mHandsData[(int)Hands.Right].HasTremor;

        public bool HasCorrelation => mHandsData[(int)Hands.Left].HasAutoCorrelation && mHandsData[(int)Hands.Right].HasAutoCorrelation;

        public bool HasSomeData => !(mHands[(int)Hands.Left].Constant.IsNullOrEmpty() || mHands[(int)Hands.Right].Constant.IsNullOrEmpty());

        public bool HasSpectrum => mHandsData[(int)Hands.Left].HasSpectrum && mHandsData[(int)Hands.Right].HasSpectrum;

        public HandGraphs Left => mHandsData[(int)Hands.Left];

        public HandGraphs Right => mHandsData[(int)Hands.Right];

        /// <summary>
        /// Время в секундах
        /// </summary>
        [DataMember]
        public double Time
        {
            get => mTime;
            set => mTime = value;
        }

        public MeasurementData() => Time = AppSettings.GetValue("DataTime", 30.0);

        public void AddHands(HandRawData left, HandRawData right)
        {
            mHands[(int)Hands.Left] += left;
            mHands[(int)Hands.Right] += right;
        }

        /// <summary>
        /// Добавляет в очередь поток базовой обработки. 
        /// Запуск через StartCalc(). Финальная функция через AddTaskFinal()
        /// </summary>
        /// <returns></returns>
        public Guid BaseAnalys(Action<double, double> percentCallBack, Action<MeasurementData, bool> finalCallBack)
        {
            Guid id = Guid.NewGuid();
            CancellationTokenSource cts = new CancellationTokenSource();

            // посылает значения (предполагается процент выполнения на каждую руку)
            Action<double, double> percentAction = new Action<double, double>((h1, h2) =>
            {
                if (percentCallBack != null)
                {
                    percentCallBack(h1, h2);
                }
            });
            // Заполнение Const и Tremor на основе Hands и Time
            Task taska = new Task((param) =>
            {
                mMutex.WaitOne();
                ClearData();

                bool result = Async_BaseAnalys((CancellationToken)param, percentAction);
                if (result == false)
                    ClearData();
                else CalculateTremorCorrelationCoef();

                if (finalCallBack != null)
                    finalCallBack(this, result);
                RemoveTaskQueue(id);
                mMutex.ReleaseMutex();
            }, cts.Token, cts.Token);

            AddTaskQueue(id, taska, cts);

            return id;
        }

        /// <summary>
        /// Расчитывает коэффициент кореляции тремора
        /// </summary>
        private void CalculateTremorCorrelationCoef()
        {
            float leftAverage = (float)Left.Tremor.Mean;
            float rightAverage = (float)Right.Tremor.Mean;

            float cov = 0f;
            float leftDeviation = 0f;
            float rightDeviation = 0f;
            for(int i=0;i<Left.Tremor.Count;i++)
            {
                float leftDiff = (float)(Left.Tremor.DataPoints[i].Y - leftAverage);
                leftDeviation += leftDiff * leftDiff;
                
                float rightDiff= (float)(Right.Tremor.DataPoints[i].Y - rightAverage);
                rightDeviation += rightDiff * rightDiff;

                cov += leftDiff*rightDiff;
            }
            leftDeviation = (float)Math.Sqrt(leftDeviation); 
            rightDeviation = (float)Math.Sqrt(rightDeviation);
            CorrelationCoef = cov / (leftDeviation * rightDeviation);   
        }

        public void Clear()
        {
            lock (mLock)
            {
                for (int i = 0; i < 2; i++)
                {
                    mHands[i].Clear();
                    mHandsData[i].Clear();
                }
            }
        }

        /// <summary>
        /// Чистит только то что можно пересчитать
        /// </summary>
        public void ClearData()
        {
            lock (mLock)
            {
                for (int i = 0; i < 2; i++)
                {
                    mHandsData[i].Clear();
                }
            }
        }

        public IList<DataPoint> GetConst(Hands hand) => mHandsData[(int)hand].Constant.DataPoints;

        public IEnumerable<short> GetConstBase(Hands hand) => mHands[(int)hand].Constant;

        public IList<DataPoint> GetCorrelation(Hands hand) => mHandsData[(int)hand].AutoCorrelation.DataPoints;

        public IList<DataPoint> GetSpectrum(Hands hand) => mHandsData[(int)hand].Spectrum.DataPoints;

        public IList<DataPoint> GetTremor(Hands hand) => mHandsData[(int)hand].Tremor.DataPoints;

        /// <summary>
        /// Возвращает амплитуду тремора
        /// </summary>
        /// <param name="hand"></param>
        /// <returns></returns>
        public double GetTremorAmplitude(Hands hand)
        {
            double interval = mHandsData[(int)hand].Tremor.Max - mHandsData[(int)hand].Tremor.Min;
            return Math.Abs(interval);
        }

        /// <summary>
        /// возвращает просто массив без учета времени
        /// </summary>
        /// <param name="hand"></param>
        /// <returns></returns>
        public IEnumerable<short> GetTremorBase(Hands hand) => mHands[(int)hand].Tremor;

      

        public Guid SpectrumAnalys(Action<double, double> percentCallBack, Action<MeasurementData, bool> finalCallBack,
            bool corr = false)
        {
            Guid id = Guid.NewGuid();
            CancellationTokenSource cts = new CancellationTokenSource();

            Action<double, double> percentAction = new Action<double, double>((h1, h2) =>
            {
                if (percentCallBack != null)
                {
                    percentCallBack(h1, h2);
                }
            });

            // Заполнение 
            Task taska = new Task((param) =>
            {
                mMutex.WaitOne();
                lock (mLock)
                {
                    mHandsData.ForEach(hd => hd.Spectrum.Clear());
                    mHandsData.ForEach(hd => hd.AutoCorrelation.Clear());
                }

                bool result = Async_SpectrumAnalys((CancellationToken)param, percentAction, corr);
                if (result == false)
                {
                    lock (mLock)
                    {
                        mHandsData.ForEach(hd => hd.Spectrum.Clear());
                        mHandsData.ForEach(hd => hd.AutoCorrelation.Clear());
                    }
                } else CalculateTremorCorrelationCoef();
                if (finalCallBack != null)
                    finalCallBack(this, result);
                RemoveTaskQueue(id);
                mMutex.ReleaseMutex();
            }, cts.Token, cts.Token);

            AddTaskQueue(id, taska, cts);
            return id;
        }

        /// <summary>
        /// Формирует HAND_DATA на основе объектов Hand
        /// </summary>
        /// <param name="cancelTok"></param>
        /// <param name="percentAction"></param>
        /// <returns></returns>
        private bool Async_BaseAnalys(CancellationToken cancelTok, Action<double, double> percentAction)
        {
            if (cancelTok.IsCancellationRequested)
                return false;

            double[] percent = new double[] { 0, 0 };
            percentAction(percent[0], percent[1]);

            for (int i = 0; i < 2; i++)
            {
                IEnumerable<short> cd = mHands[i].Constant.Where((s, index) => (index >= BeginPoint && index <= EndPoint));
                IEnumerable<short> td = mHands[i].Tremor.Where((s, index) => (index >= BeginPoint && index <= EndPoint));
                List<short> constData = cd.Any() ? cd.ToList() : null;
                List<short> tremorData = td.Any() ? td.ToList() : null;

                // проверка на возможность расчетов
                if (constData == null || tremorData == null ||
                    constData.Count == 0 || tremorData.Count == 0)
                {
                    return false;
                }

                // временной шаг, считаем что все промежутки между отсчетами равны
                double dt = Time / mHands[i].Constant.Count;

                percent[i] = 10;
                percentAction(percent[0], percent[1]);

                mHandsData[i].Constant.DataPoints = constData.Select((s, index) => new DataPoint(index * dt, s)).ToList();
                if (cancelTok.IsCancellationRequested)
                    return false;

                percent[i] = 45;
                percentAction(percent[0], percent[1]);

                mHandsData[i].Tremor.DataPoints = tremorData.Select((s, index) => new DataPoint(index * dt, s)).ToList();
                if (cancelTok.IsCancellationRequested)
                    return false;

                percent[i] = 100;
                percentAction(percent[0], percent[1]);
            }
            return true;
        }

        private bool Async_SpectrumAnalys(CancellationToken cancelTok, Action<double, double> percentAction, bool corr)
        {
            if (cancelTok.IsCancellationRequested)
                return false;

            // Посылка начальных значений процентов
            double[] percent = new double[] { 0, 0 };
            percentAction(percent[0], percent[1]);

            for (int i = 0; i < 2; i++)
            {
                List<DataPoint> tremorData = mHandsData[i].Tremor.DataPoints;

                if (tremorData == null || tremorData.Count == 0)
                    return false;

                Complex[] samples = tremorData.Select(item => new Complex(item.Y, 0.0)).ToArray();

                Fourier.Forward(samples, FourierOptions.Matlab);
                int N = samples.Count();
                if (N > 2)
                {
                    mHandsData[i].Spectrum.DataPoints =
                        samples.Take((int)Math.Ceiling(N / 2.0))
                            .Select((cmpl, x) => new DataPoint(x / Time, cmpl.Magnitude * (x == 0 ? 1 : 2) / N))
                            .ToList();
                }
                if (cancelTok.IsCancellationRequested)
                    return false;
                if (corr)
                {
                    double dt = Time / mHandsData[i].Constant.Count;
                    // спектральная плотность
                    Complex[] den = samples.Select(complex => complex * complex).ToArray();
                    Fourier.Inverse(den, FourierOptions.Matlab);
                    // перенос второй части в отрицательую сторону
                    int secondCount = (int)Math.Ceiling(N / 2.0);
                    int firstCount = den.Count() - secondCount;
                    IEnumerable<DataPoint> second =
                        den.Take(secondCount).Select((complex, x) => new DataPoint(dt * x, complex.Magnitude));
                    IEnumerable<DataPoint> first =
                        den.Skip(secondCount)
                            .Select((complex, x) => new DataPoint(dt * (x - firstCount), complex.Magnitude));
                    mHandsData[i].AutoCorrelation.DataPoints = first.Concat(second).ToList();
                }

                percent[i] = 100;
                percentAction(percent[0], percent[1]);
            }
            return true;
        }

        #region JSON
        public bool LoadFromArray(byte[] array)
        {
            byte[][] handsData = BytesPacker.UnpackBytes(array);

            if (handsData.Length >= 2)
            {
                bool h1 = mHands[(int)Hands.Left].LoadFromArray(handsData[(int)Hands.Left]);
                bool h2 = mHands[(int)Hands.Right].LoadFromArray(handsData[(int)Hands.Right]);
                if ((h1 || h2) == false)
                    return false;
            }

            if (handsData.Length >= 8)
            {
                IEnumerable<DataPoint> const0 = handsData[2].GetCollectionDataPoint();
                IEnumerable<DataPoint> const1 = handsData[3].GetCollectionDataPoint();
                IEnumerable<DataPoint> tremor0 = handsData[4].GetCollectionDataPoint();
                IEnumerable<DataPoint> tremor1 = handsData[5].GetCollectionDataPoint();
                IEnumerable<DataPoint> spectrum0 = handsData[6].GetCollectionDataPoint();
                IEnumerable<DataPoint> spectrum1 = handsData[7].GetCollectionDataPoint();

                if (const0 != null && const1 != null &&
                    tremor0 != null && tremor1 != null)
                {
                    mHandsData[(int)Hands.Left].Constant.DataPoints = const0.ToList();
                    mHandsData[(int)Hands.Right].Constant.DataPoints = const1.ToList();
                    mHandsData[(int)Hands.Left].Tremor.DataPoints = tremor0.ToList();
                    mHandsData[(int)Hands.Right].Tremor.DataPoints = tremor1.ToList();
                    if (spectrum0 != null && spectrum1 != null)
                    {
                        mHandsData[(int)Hands.Left].Spectrum.DataPoints = spectrum0.ToList();
                        mHandsData[(int)Hands.Right].Spectrum.DataPoints = spectrum1.ToList();
                    }
                }
            }
            if (handsData.Length >= 9)
            {
                MeasurementData obj = BytesPacker.LoadJSONObj<MeasurementData>(handsData[8]);
                Time = obj.Time;
                CorrelationCoef = obj.CorrelationCoef;
            }

            if (handsData.Length >= 11)
            {
                IEnumerable<DataPoint> corr0 = handsData[9].GetCollectionDataPoint();
                IEnumerable<DataPoint> corr1 = handsData[10].GetCollectionDataPoint();
                if (corr0 != null && corr1 != null)
                {
                    mHandsData[(int)Hands.Left].AutoCorrelation.DataPoints = corr0.ToList();
                    mHandsData[(int)Hands.Right].AutoCorrelation.DataPoints = corr1.ToList();
                }
            }
            return true;
        }

        public byte[] ToByteArray()
        {
            byte[] hand1 = mHands[(int)Hands.Left].ToByteArray();
            byte[] hand2 = mHands[(int)Hands.Right].ToByteArray();
            byte[] const1 = mHandsData[(int)Hands.Left].Constant.DataPoints.ToByteArray();
            byte[] const2 = mHandsData[(int)Hands.Right].Constant.DataPoints.ToByteArray();
            byte[] tremor1 = mHandsData[(int)Hands.Left].Tremor.DataPoints.ToByteArray();
            byte[] tremor2 = mHandsData[(int)Hands.Right].Tremor.DataPoints.ToByteArray();
            byte[] spectrum1 = mHandsData[(int)Hands.Left].Spectrum.DataPoints.ToByteArray();
            byte[] spectrum2 = mHandsData[(int)Hands.Right].Spectrum.DataPoints.ToByteArray();
            byte[] corr1 = mHandsData[(int)Hands.Left].AutoCorrelation.DataPoints.ToByteArray();
            byte[] corr2 = mHandsData[(int)Hands.Right].AutoCorrelation.DataPoints.ToByteArray();
            byte[] json = BytesPacker.JSONObj(this);
            byte[] handsData = BytesPacker.PackBytes(hand1, hand2, const1, const2, tremor1, tremor2, spectrum1, spectrum2,
                json, corr1, corr2);

            return handsData;
        }
        #endregion

        #region TaskPool

        private const int INDEX_ID = 0;
        private const int INDEX_TASK = 1;
        private const int INDEX_CTS = 2;

        [NonSerialized]
        private static List<object[]> TaskPoolQueue = new List<object[]>();

        public static void AddTaskQueue(Guid id, Task taska, CancellationTokenSource cts)
        {
            lock (mLock)
            {
                if (TaskPoolQueue.Any())
                    ((Task)TaskPoolQueue.Last()[INDEX_TASK]).ContinueWith(t => { taska.Start(); });
                TaskPoolQueue.Add(new object[] { id, taska, cts });
            }
        }


        public static Guid AddTaskFinal(Task taska, CancellationTokenSource cts)
        {
            Guid id = Guid.NewGuid();
            taska.ContinueWith(task => { RemoveTaskQueue(id); });
            AddTaskQueue(id, taska, cts);
            return id;
        }

        private static Guid mCurrentPack = Guid.Empty;

        public static void StartCalc()
        {
            lock (mLock)
            {
                if (TaskPoolQueue.Any())
                    ((Task)TaskPoolQueue.First()[INDEX_TASK]).Start();
            }
        }


        public static void CancelCalc()
        {
            lock (mLock)
            {
                foreach (object[] task in TaskPoolQueue)
                {
                    ((CancellationTokenSource)task[INDEX_CTS]).Cancel();
                }
            }
        }

        private static void RemoveTaskQueue(Guid id)
        {
            lock (mLock)
            {
                object[] first = TaskPoolQueue.First(objects => objects[INDEX_ID].Equals(id));
                if (first != null)
                {
                    TaskPoolQueue.Remove(first);
                }
            }
        }

        private static readonly object mLock = new object();

        #endregion

        #region No References
        /// <summary>
        /// Возвращает спектр на заданном диапазоне частот (из имеющихся)
        /// </summary>
        /// <param name="hand"></param>
        /// <param name="minimum">Нижняя граница, Гц</param>
        /// <param name="maximum">Верхняя граница, Гц</param>
        /// <returns></returns>
        public IList<DataPoint> GetSpectrumByHz(Hands hand, double minimum, double maximum)
        {
            int start = -1;
            int end = -1;
            int index = (hand == Hands.Left) ? 0 : 1;
            if (mHandsData[index].Spectrum.DataPoints == null)
                return null;

            if (double.IsNaN(minimum) == false)
            {
                IOrderedEnumerable<DataPoint> sortedValues = mHandsData[index].Spectrum.DataPoints.OrderBy(dp => dp.X);
                IEnumerable<DataPoint> goodValues = sortedValues.Where(dp => dp.X >= minimum);
                DataPoint first = goodValues.Any() ? goodValues.First() : sortedValues.FirstOrDefault();


                Debug.Assert(first.IsDefined());
                start = mHandsData[index].Spectrum.DataPoints.IndexOf(first);
            }
            if (double.IsNaN(maximum) == false)
            {
                IOrderedEnumerable<DataPoint> sortedValues = mHandsData[index].Spectrum.DataPoints.OrderBy(dp => dp.X);
                IEnumerable<DataPoint> goodValues = sortedValues.Where(dp => dp.X >= maximum);
                DataPoint last = goodValues.Any() ? goodValues.Last() : sortedValues.LastOrDefault();

                Debug.Assert(last.IsDefined());
                end = mHandsData[index].Spectrum.DataPoints.IndexOf(last);
            }
            return GetSpectrum(hand).Where((dp, i) => i > start && i < end).ToList();
        }


        #region Закоменченные методы
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
        #endregion Закоменченные методы

        /// <summary>
        /// Возвращает процент тремора от средней постоянной
        /// </summary>
        /// <param name="hand"></param>
        /// <returns></returns>
        public double GetTremorPercent(Hands hand)
        {
            int index = (hand == Hands.Left) ? 0 : 1;
            double tremor = GetTremorAmplitude(hand);
            double force = mHandsData[index].Constant.Mean;
            return tremor * 100.0 / force;
        }

        /// <summary>
        /// Возвращает часть коллекции в заданном диапазоне, с заданным разрешением
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        /// <param name="start">Первый элемент</param>
        /// <param name="end">Последний элемент</param>
        /// <param name="length">Сколько требуется элементов</param>
        /// <returns></returns>
        private IList<T> GetPartOfCollection<T>(IList<T> collection, uint? start, uint? end, uint? length)
        {
            lock (mLock)
            {
                if (collection.IsNullOrEmpty()) return null;

                uint startIndex = start ?? 0;
                uint endIndex = end ?? (uint)(collection.Count() - 1);

                // сколько можно отдать
                double hasValues = Math.Abs((double)endIndex - startIndex) + 1;
                // сколько требуют
                double requireValues = length ?? hasValues;

                IList<T> result = collection;
                if (requireValues <= hasValues && requireValues >= hasValues / 2)
                {
                    // требуют меньше чем есть, но больше половины -> исключаем каждый k-ый
                    int k = (int)Math.Floor(hasValues / (hasValues - requireValues));
                    result = collection.Where((o, i) => i >= startIndex && i <= endIndex && i % k != 0).ToList();
                }
                else if (requireValues < hasValues / 2)
                {
                    //требуют меньше половины -> берем каждый k-ый
                    int k = (int)Math.Floor(hasValues / requireValues);
                    result = collection.Where((o, i) => i >= startIndex && i <= endIndex && i % k == 0).ToList();
                }
                else if (requireValues >= hasValues)
                {
                    //требуют больше чем есть
                    result = collection.Where((o, i) => i >= startIndex && i <= endIndex).ToList();
                }
                return result;
            }
        }

        private double Param1(IEnumerable<DataPoint> data, DataMode mode = DataMode.First)
        {
            if (mode == DataMode.First)
                return data.Average(dp => dp.Y);
            if (mode == DataMode.Second)
                return data.Average(dp => dp.Y * dp.X);
            return data.Average(dp => dp.Y);
        }

        private double[] Param2(IEnumerable<DataPoint> data, DataMode mode = DataMode.First)
        {
            double[] result = new[] { 0d, 0d };
            int count = data.Count();
            if (mode == DataMode.First)
            {
                for (int i = 0; i < count - 1; i++)
                {
                    double delta = data.ElementAt(i + 1).Y - data.ElementAt(i).Y;
                    if (delta > 0)
                    {
                        result[0] += delta;
                    }
                    else
                    {
                        result[1] += delta;
                    }
                }
            }
            else if (mode == DataMode.Second)
            {
                for (int i = 0; i < count - 1; i++)
                {
                    double delta = data.ElementAt(i + 1).Y * data.ElementAt(i + 1).X -
                                data.ElementAt(i).Y * data.ElementAt(i).X;
                    if (delta > 0)
                    {
                        result[0] += delta;
                    }
                    else
                    {
                        result[1] += delta;
                    }
                }
            }
            return result;
        }
        private enum DataMode
        {
            First,
            Second,
        }

        #endregion No References
    }
}