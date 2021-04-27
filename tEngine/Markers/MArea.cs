using System;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Newtonsoft.Json;
using tEngine.Helpers;

namespace tEngine.Markers
{
    /// <summary>
    /// Инкапсулирует маркеры и предоставляет методы для их рисования
    /// </summary>
    [DataContract]
    public class MArea
    {
        private WriteableBitmap mBackground;
        private WriteableBitmap mDest;
        private int mLastLeft = 0;
        private int mLastRight = 0;
        private WriteableBitmap mSource;

        [DataMember]
        public Color Color { get; set; }

        [DataMember]
        public int Grid { get; set; }

        [DataMember]
        public Color GridColor { get; set; }

        [DataMember(Name = "M1")]
        public Marker MarkerLeftHand { get; set; }

        [DataMember(Name = "M2")]
        public MarkerWithHole MarkerRightHand { get; set; }

        [DataMember]
        public int Maximum { get; set; }

        [DataMember]
        public int Minimum { get; set; }

        [DataMember]
        public bool ShowAxis { get; set; }

        [DataMember]
        public bool ShowGrid { get; set; }

        [DataMember(Name = "ShowM1")]
        public bool ShowMarkerLeft { get; set; }

        [DataMember(Name = "ShowM2")]
        public bool ShowMarkerRight { get; set; }

        public MArea()
        {
            Init();
        }

        public MArea(WriteableBitmap area)
        {
            Init();
            UpdateArea(area);
        }

        public void RedrawEverything(int left, int right)
        {
            DrawBackground();
            CopyBackground();
            DrawMarkers(left, right);
            CopyMarkers();
        }

        public void RedrawMarkers(int left, int right)
        {
            DrawMarkers(left, right);
            CopyMarkers();
        }

        public void Init()
        {
            ShowGrid = true;
            ShowAxis = true;
            ShowMarkerLeft = true;
            ShowMarkerRight = true;
            Minimum = -10000;
            Maximum = 35000;

            MarkerRightHand = new MarkerWithHole();
            MarkerLeftHand = new Marker() 
            {
                Color = Colors.Blue,
                Width = 60,
                Height = 6
            };

            GridColor = Colors.LightGray;
            Grid = 3000;
            Color = Colors.Azure;
        }

        /// <summary>
        /// Сохранить параметры
        /// </summary>
        public void SaveSettings()
        {
            JsonSerializerSettings settings = new JsonSerializerSettings() { ContractResolver = new JSONContractResolver() };
            string json = JsonConvert.SerializeObject(this, settings);
            FileIO.WriteString(AppSettings.Constants.MarkersSettings, json);
        }

        /// <summary>
        /// Обновить битмап на новую
        /// </summary>
        /// <param name="area">Новая битмап</param>
        public void UpdateArea(WriteableBitmap area)
        {
            if (area == null)
                return;
            mSource = new WriteableBitmap(area.Clone());
            mBackground = new WriteableBitmap(area.Clone());
            mDest = area;
            RedrawEverything(mLastLeft, mLastRight);
        }

        /// <summary>
        /// Обновить параметры
        /// </summary>
        public void UpdateSettings()
        {
            MArea set;
            string json;
            bool result = FileIO.ReadString(AppSettings.Constants.MarkersSettings, out json);
            try
            {
                if (result)
                {
                    set = JsonConvert.DeserializeObject<MArea>(json);
                }
                else
                    set = new MArea();
            }
            catch
            {
                set = new MArea();
            }

            Color = set.Color;
            Grid = set.Grid;
            Maximum = set.Maximum;
            Minimum = set.Minimum;
            ShowAxis = set.ShowAxis;
            ShowGrid = set.ShowGrid;
            MarkerLeftHand = set.MarkerLeftHand;
            MarkerRightHand = set.MarkerRightHand;
            GridColor = set.GridColor;
            ShowMarkerLeft = set.ShowMarkerLeft;
            ShowMarkerRight = set.ShowMarkerRight;

            MarkerRightHand.Hole = MarkerLeftHand.Height;

            MarkerLeftHand.UpdateSource();
            MarkerRightHand.UpdateSource();
        }

        /// <summary>
        /// Скопировать фон
        /// </summary>
        private void CopyBackground()
        {
            Rect rect = new Rect(0, 0, mSource.PixelWidth, mSource.PixelHeight);
            Drawer.CopyPart(mBackground, rect, mDest, rect);
        }

        /// <summary>
        /// Скопировать маркеры
        /// </summary>
        private void CopyMarkers()
        {
            Rect rect = new Rect();
            rect.Width = MarkerLeftHand.Width > MarkerRightHand.Width ? MarkerLeftHand.Width : MarkerRightHand.Width;
            rect.X = (mDest.PixelWidth - rect.Width) / 2;
            rect.Y = 0;
            rect.Height = mDest.PixelHeight;

            Drawer.CopyPart(mSource, rect, mDest, rect);
        }

        /// <summary>
        /// Нарисовать фон
        /// </summary>
        private void DrawBackground()
        {
            Rect rect = new Rect(0, 0, mSource.PixelWidth, mSource.PixelHeight);
            Drawer.DrawRectangle(mBackground, rect, Color);
            if (ShowAxis)
            {
                int mid = RelToAbsY(0);
                Drawer.DrawRectangle(mBackground, new Rect(0, mid - 1, mSource.PixelWidth, 2), GridColor);
            }
            if (ShowGrid)
            {
                int first = (int)(Math.Truncate(1.0 * Minimum / Grid) * Grid);
                for (int i = first; i < Maximum; i += Grid)
                {
                    int y = RelToAbsY(i);
                    Drawer.DrawRectangle(mBackground, new Rect(0, RelToAbsY(i), mSource.PixelWidth, 1), GridColor);
                }
            }

            Drawer.CopyPart(mBackground, rect, mSource, rect);
        }

        /// <summary>
        /// Нарисовать маркеры
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        private void DrawMarkers(int left, int right)
        {
            int y1 = RelToAbsY(left);
            int y2 = RelToAbsY(right);
            int y1Last = RelToAbsY(mLastLeft);
            int y2Last = RelToAbsY(mLastRight);

            HideMarker(mSource, y1Last, MarkerLeftHand.Width + 10, MarkerLeftHand.Height + 10);
            HideMarker(mSource, y2Last, MarkerRightHand.Width + 10, MarkerRightHand.Height * 2 + (MarkerRightHand.Hole ?? 0) + 10);

            if (ShowMarkerLeft)
                MarkerLeftHand.Draw(mSource, y1);
            if (ShowMarkerRight)
                MarkerRightHand.Draw(mSource, y2);

            mLastLeft = left;
            mLastRight = right;
        }

        /// <summary>
        /// Зарисовать маркеры
        /// </summary>
        /// <param name="bitmap"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        private void HideMarker(WriteableBitmap bitmap, int y, int width, int height)
        {
            Rect rect = new Rect();
            rect.Y = y - height / 2.0;
            rect.X = (bitmap.PixelWidth - width) / 2.0;
            rect.Width = width;
            rect.Height = height;

            Drawer.CopyPart(mBackground, rect, bitmap, rect);
        }

        /// <summary>
        /// Получить положительную координату относительно размеров зоны и ее параметров  
        /// </summary>
        /// <param name="y"></param>
        /// <returns></returns>
        private int RelToAbsY(int y)
        {
            if (Minimum == Maximum || mDest == null)
                return 0;
            return (int)(mDest.PixelHeight * (1.0 * y - Maximum) / (Minimum - Maximum));
        }
    }
}