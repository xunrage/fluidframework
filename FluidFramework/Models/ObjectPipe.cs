using System;
using System.ComponentModel;
using System.Data;

namespace FluidFramework.Models
{
    /// <summary>
    /// Class used to bind from a property to a data column.
    /// </summary>
    public class ObjectPipe : INotifyPropertyChanged
    {
        /// <summary>
        /// Event raised when the value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Notify the change of value.
        /// </summary>
        protected void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        /// <summary>
        /// The source row
        /// </summary>
        public DataRow SourceRow { get; set; }

        /// <summary>
        /// The property
        /// </summary>
        public string SourceField { get; set; }

        /// <summary>
        /// Constructor without parameters
        /// </summary>
        public ObjectPipe() { }

        /// <summary>
        /// Constructor
        /// </summary>
        public ObjectPipe(DataRow row, string field)
        {
            SourceRow = row;
            SourceField = field;
        }

        /// <summary>
        /// Gets the value.
        /// </summary>
        protected virtual object GetValue()
        {
            return SourceRow[SourceField];
        }

        /// <summary>
        /// Sets the value and raise a notification.
        /// </summary>
        protected virtual void SetValue(object value)
        {
            SourceRow[SourceField] = value;
            NotifyPropertyChanged("Field");
        }

        /// <summary>
        /// The value
        /// </summary>
        public object Field
        {
            get
            {
                if (SourceRow.IsNull(SourceField)) return null;
                return GetValue();
            }

            set
            {
                if (value != null)
                {
                    if (SourceRow.IsNull(SourceField))
                    {
                        SetValue(value);
                    }
                    else
                    {
                        if (!SourceRow[SourceField].Equals(value))
                        {
                            SetValue(value);
                        }
                    }
                }
                else
                {
                    if (!SourceRow.IsNull(SourceField))
                    {
                        SetValue(DBNull.Value);
                    }
                }
            }
        }
    }
}
