using System;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using tEngine.DataModel;
using tEngine.Helpers;

namespace tEngine.TMeter.DataModel
{
    [DataContract]
    public class Measurement
    {
        private MeasurementData mData = new MeasurementData();

        public MeasurementData Data => mData;

        /// <summary>
        /// Длительность измерения, сек
        /// </summary>
        public double MsmTime
        {
            get => mData.Time;
            set => mData.Time = value;
        }

        public User Owner { get; set; }

        public Measurement() => Init();

        public Measurement(Measurement msm)
        {
            Init();
            // полное копирование Msm
            //Д: пизда какое медленное копирование
            LoadFromArray(msm.ToByteArray());
        }

        public void AddData(HandRawData left, HandRawData right) => mData.AddHands(left, right);

        public void Copy(Measurement msm)
        {
            Cloner.CopyAllProperties(this, msm);

            ID = msm.ID;
            Owner = msm.Owner;
            SetData(msm.Data);
        }

        public static Measurement GetTestMsm(User owner = null, string title = "Измерение")
        {
            Measurement testMsm = new Measurement()
            {
                Title = title,
                Comment = "Тестовое измерение",
                CreateTime = DateTime.Now,
                MsmTime = 5
            };

            int garmonics = 10;

            int length = (int)Math.Pow(10, 1);
            System.Collections.Generic.List<short> dataTremor = Enumerable.Range(0, length).Select(i =>
            {
                double result = 0.0;
                for (int f = 1; f < garmonics + 1; f++)
                    result += 20 * Math.Sin((f * i * Math.PI / 180.0) * 5.0);
                return (short)result;
            }).ToList();
            System.Collections.Generic.List<short> dataConst =
                Enumerable.Range(0, length)
                    .Select(i => (short)(200 + dataTremor[i]))
                    .ToList();
            testMsm.AddData(
                new HandRawData() { Constant = dataConst, Tremor = dataTremor },
                new HandRawData()
                {
                    Constant = dataConst.Select(s => (short)(s + 10)).ToList(),
                    Tremor = dataTremor.Select(s => (short)(s + 10)).ToList()
                });

            testMsm.Owner=owner;
            return testMsm;
        }

        public void Msm2CSV(string filePath)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Time;Hz;Delta;" + "LConstant;LTremor;LSpectrum;LCorrelation;" +
                           "RConstant;RTremor;RSpectrum;RCorrelation");

            int length = Data.Count;

            double[] Time = Data.GetConst(Hands.Left).Select(dp => dp.X).ToArray();

            double[] LConstant = Data.GetConst(Hands.Left).Select(dp => dp.Y).ToArray();
            double[] LTremor = Data.GetTremor(Hands.Left).Select(dp => dp.Y).ToArray();
            double[] RConstant = Data.GetConst(Hands.Right).Select(dp => dp.Y).ToArray();
            double[] RTremor = Data.GetTremor(Hands.Right).Select(dp => dp.Y).ToArray();

            double[] Hz = Data.GetSpectrum(Hands.Left).Select(dp => dp.X).ToArray();
            double[] LSpectrum = Data.GetSpectrum(Hands.Left).Select(dp => dp.Y).ToArray();
            double[] RSpectrum = Data.GetSpectrum(Hands.Right).Select(dp => dp.Y).ToArray();

            double[] Delta = Data.GetCorrelation(Hands.Left).Select(dp => dp.X).ToArray();
            double[] LCorrelation = Data.GetCorrelation(Hands.Left).Select(dp => dp.Y).ToArray();
            double[] RCorrelation = Data.GetCorrelation(Hands.Right).Select(dp => dp.Y).ToArray();

            for (int i = 0; i < length; i++)
            {
                if (i < length / 2)
                {
                    sb.AppendLine($"{Time[i]};{Hz[i]};{Delta[i]};{LConstant[i]};{LTremor[i]};{LSpectrum[i]};{LCorrelation[i]};" +
                        $"{RConstant[i]};{RTremor[i]};{RSpectrum[i]};{RCorrelation[i]}");
                }
                else
                {
                    sb.AppendLine($"{Time[i]};{"\"\""};{Delta[i]};{LConstant[i]};{LTremor[i]};{"\"\""};{LCorrelation[i]};" +
                        $"{RConstant[i]};{RTremor[i]};{"\"\""};{RCorrelation[i]}");
                }
            }
            FileIO.WriteString(filePath, sb.ToString());
        }

        public void SetData(MeasurementData data)
        {
            if (data != null)
                mData = data;
            else throw new NullReferenceException("Параметр data не должен быть null");
        }

        private void Init() => ID = Guid.NewGuid();

        #region JSON

        [DataMember]
        public string Title { get; set; }

        [DataMember]
        public Guid ID { get; protected set; }

        [DataMember]
        public string Comment { get; set; }

        [DataMember]
        public DateTime CreateTime { get; set; }

        #endregion

        #region Byte <=> Object

        public byte[] ToByteArray()
        {
            byte[] obj = BytesPacker.JSONObj(this);
            byte[] data = mData.ToByteArray();
            return BytesPacker.PackBytes(obj, data);
        }

        public bool LoadFromArray(byte[] array)
        {
            byte[][] objData = BytesPacker.UnpackBytes(array);
            if (objData.Length != 2)
                return false;
            Measurement obj = BytesPacker.LoadJSONObj<Measurement>(objData[0]);
            Title = obj.Title;
            ID = obj.ID;
            Comment = obj.Comment;
            CreateTime = obj.CreateTime;

            bool result = mData.LoadFromArray(objData[1]);
            return result;
        }

        #endregion
    }
}