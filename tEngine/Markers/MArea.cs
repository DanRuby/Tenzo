﻿using System;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Newtonsoft.Json;
using tEngine.Helpers;

namespace tEngine.Markers
{
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
        public Marker1 Marker1 { get; set; }

        [DataMember(Name = "M2")]
        public Marker2 Marker2 { get; set; }

        [DataMember]
        public int Maximum { get; set; }

        [DataMember]
        public int Minimum { get; set; }

        [DataMember]
        public bool ShowAxis { get; set; }

        [DataMember]
        public bool ShowGrid { get; set; }

        [DataMember(Name = "ShowM1")]
        public bool ShowMarker1 { get; set; }

        [DataMember(Name = "ShowM2")]
        public bool ShowMarker2 { get; set; }

        public MArea()
        {
            Init();
        }

        public MArea(WriteableBitmap area)
        {
            Init();
            UpdateArea(area);
        }

        public void DrawAll(int left, int right)
        {
            DrawBackground();
            CopyBacground();
            DrawMarkers(left, right);
            CopyMarkers();
        }

        public void DrawPart(int left, int right)
        {
            DrawMarkers(left, right);
            CopyMarkers();
        }

        public void Init()
        {
            ShowGrid = true;
            ShowAxis = true;
            ShowMarker1 = true;
            ShowMarker2 = true;
            Minimum = -10000;
            Maximum = 35000;
            Marker2 = new Marker2();
            Marker1 = new Marker1();
            GridColor = Colors.LightGray;
            Grid = 3000;
            Color = Colors.Azure;
        }

        public void SaveSettings()
        {
            JsonSerializerSettings settings = new JsonSerializerSettings() { ContractResolver = new JSONContractResolver() };
            string json = JsonConvert.SerializeObject(this, settings);
            FileIO.WriteText(AppSettings.Constants.MarkersSettings, json);
        }

        public void UpdateArea(WriteableBitmap area)
        {
            if (area == null) return;
            mSource = new WriteableBitmap(area.Clone());
            mBackground = new WriteableBitmap(area.Clone());
            mDest = area;
            DrawAll(mLastLeft, mLastRight);
        }

        public void UpdateSettings()
        {
            MArea set;
            string json;
            bool result = FileIO.ReadText(AppSettings.Constants.MarkersSettings, out json);
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
            Marker1 = set.Marker1;
            Marker2 = set.Marker2;
            GridColor = set.GridColor;
            ShowMarker1 = set.ShowMarker1;
            ShowMarker2 = set.ShowMarker2;

            Marker2.Hole = Marker1.Height;

            Marker1.UpdateSource();
            Marker2.UpdateSource();
        }

        private void CopyBacground()
        {
            Rect rect = new Rect(0, 0, mSource.PixelWidth, mSource.PixelHeight);
            Drawer.CopyPart(mBackground, rect, mDest, rect);
        }

        private void CopyMarkers()
        {
            Rect rect = new Rect();
            rect.Width = Marker1.Width > Marker2.Width ? Marker1.Width : Marker2.Width;
            rect.X = (mDest.PixelWidth - rect.Width) / 2;
            rect.Y = 0;
            rect.Height = mDest.PixelHeight;

            Drawer.CopyPart(mSource, rect, mDest, rect);
        }

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

        private void DrawMarkers(int left, int right)
        {
            int y1 = RelToAbsY(left);
            int y2 = RelToAbsY(right);
            int y1Last = RelToAbsY(mLastLeft);
            int y2Last = RelToAbsY(mLastRight);

            HideMarker(mSource, y1Last, Marker1.Width + 10, Marker1.Height + 10);
            HideMarker(mSource, y2Last, Marker2.Width + 10, Marker2.Height * 2 + (Marker2.Hole ?? 0) + 10);

            if (ShowMarker1)
                Marker1.Draw(mSource, y1);
            if (ShowMarker2)
                Marker2.Draw(mSource, y2);

            mLastLeft = left;
            mLastRight = right;
        }

        private void HideMarker(WriteableBitmap bitmap, int y, int width, int height)
        {
            Rect rect = new Rect();
            rect.Y = y - height / 2.0;
            rect.X = (bitmap.PixelWidth - width) / 2.0;
            rect.Width = width;
            rect.Height = height;

            Drawer.CopyPart(mBackground, rect, bitmap, rect);
        }

        private int RelToAbsY(int y)
        {
            if (Minimum == Maximum || mDest == null)
                return 0;
            return (int)(mDest.PixelHeight * (1.0 * y - Maximum) / (Minimum - Maximum));
        }
    }
}