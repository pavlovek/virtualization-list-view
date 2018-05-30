using System;
using System.ComponentModel;

namespace VirtualizationListViewControl.Collection
{
    /// <summary>
    /// Data item wrapper
    /// </summary>
    /// <typeparam name="T">Data item object</typeparam>
    public class DataWrapper<T> : INotifyPropertyChanged, ICloneable
    {
        private readonly int _index;
        private T _data;

        /// <summary>
        /// Item data
        /// </summary>
        public T Data
        {
            get { return _data; }
            internal set
            {
                _data = value;
                OnPropertyChanged("Data");
            }
        }

        /// <summary>
        /// Item index in list
        /// </summary>
        public int Index
        {
            get { return _index; }
        }

        /// <summary>
        /// Indicate when item data loading
        /// </summary>
        public bool IsLoading
        {
            get { return Data == null; }
        }

        /// <summary>
        /// Indicate when item now in view state
        /// </summary>
        public bool IsInUse
        {
            get { return PropertyChanged != null; }
        }


        /// <summary>
        /// Default constructor
        /// </summary>
        public DataWrapper()
        {
            Data = default(T);
        } 

        /// <summary>
        /// Constructor with item index
        /// </summary>
        /// <param name="index">Item index in list</param>
        public DataWrapper(int index)
            : this()
        {
            _index = index;
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            var e = new PropertyChangedEventArgs(propertyName);
            var h = PropertyChanged;
            if (h != null)
                h(this, e);
        }

        #endregion

        public object Clone()
        {
            DataWrapper<T> clone = new DataWrapper<T>(Index);
            if (Data != null)
                clone.Data = (T) ((ICloneable) Data).Clone();
            return clone;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((DataWrapper<T>) obj);
        }

        protected bool Equals(DataWrapper<T> other)
        {
            return _index == other._index;
        }

        public override int GetHashCode()
        {
            return _index;
        }
    }
}
