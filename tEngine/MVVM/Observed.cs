using System;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Windows;
using System.Xml.Serialization;

namespace tEngine.MVVM
{
    public static class Designer
    {
        [XmlIgnore]
        //Д: XmlIgnore должно хватать + варнинг на нижний аттрибут
        //[field: NonSerialized]
        public static bool IsDesignMode
        {
            get { return (LicenseManager.UsageMode == LicenseUsageMode.Designtime); }
        }
    }

    [Serializable]
    public class Observed<TModel> : INotifyPropertyChanged
    {
        [XmlIgnore]
        // [field: NonSerialized]
        public bool IsDesignMode
        {
            get { return (LicenseManager.UsageMode == LicenseUsageMode.Designtime); }
        }

        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        public void UpdateAllProperties()
        {
            this.GetType()
                .GetProperties()
                .Where(info => info.CanRead)
                .ToList()
                .ForEach(info => { NotifyPropertyChanged(info.Name); });
        }

        protected virtual void NotifyPropertyChanged<TResult>(Expression<Func<TModel, TResult>> property)
        {
            string propertyName = ((MemberExpression)property.Body).Member.Name;
            NotifyPropertyChanged(propertyName);
        }

        protected virtual void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(
                    this,
                    new PropertyChangedEventArgs(propertyName)
                    );
            }
        }

        #region mustbe

        public Window Parent { get; set; }
        public bool? DialogResult = null;

        protected void EndDialog(bool? dialogResult = null)
        {
            DialogResult = dialogResult;
            if (Parent != null)
                Parent.Close();
        }

        #endregion
    }
}