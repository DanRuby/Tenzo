using System;
using tEngine.MVVM;
using tEngine.TActual.DataModel;

namespace TenzoActualGUI.ViewModel
{
    /// <summary>
    /// оперирует объектом Msm
    /// </summary>
    public class MsmInfoVM : Observed<MsmInfoVM>
    {
        public string Comment
        {
            get
            {
                return Msm == null ? "" : Msm.Comment;
            }
            set
            {
                if (Msm == null) return;
                Msm.Comment = value;
                NotifyPropertyChanged(m => m.Comment);
            }
        }

        public Measurement Msm { get; set; }

        public string Title
        {
            get
            {
                return Msm == null ? "" : Msm.Title;
            }
            set
            {
                if (Msm == null) return;
                Msm.Title = value;
                NotifyPropertyChanged(m => m.Title);
            }
        }

        public string FIO
        {
            get
            {
                return Msm == null ? "" : Msm.FIO;
            }
            set
            {
                if (Msm == null) return;
                Msm.FIO = value;
                NotifyPropertyChanged(m => m.FIO);
            }
        }

        public string Theme
        {
            get
            {
                return Msm == null ? "" : Msm.Theme;
            }
            set
            {
                if (Msm == null) return;
                Msm.Theme = value;
                NotifyPropertyChanged(m => m.Theme);
            }
        }

        public DateTime CreateDate
        {
            get
            {
                return Msm == null ? DateTime.Now : Msm.CreateTime;
            }
            set
            {
                if (Msm == null) return;
                Msm.CreateTime = value;
                NotifyPropertyChanged(m => m.CreateDate);
            }
        }
    }
}