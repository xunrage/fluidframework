using System;
using System.ComponentModel;
using System.Data;

namespace FluidFramework.Models
{
    /// <summary>
    /// Class used to bind a DateTime from a property to a data column.
    /// </summary>
    public class DateTimePipe : INotifyPropertyChanged
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
        public DateTimePipe() { }

        /// <summary>
        /// Constructor
        /// </summary>
        public DateTimePipe(DataRow row, string field)
        {
            SourceRow = row;
            SourceField = field;
        }

        /// <summary>
        /// Gets the value.
        /// </summary>
        protected virtual DateTime GetValue()
        {
            return (DateTime)SourceRow[SourceField];
        }

        /// <summary>
        /// Sets the value and raise a notification.
        /// </summary>
        protected virtual void SetValue(DateTime value)
        {
            SourceRow[SourceField] = value;
            NotifyPropertyChanged("Field");
        }

        /// <summary>
        /// Sets the null value and raise a notification.
        /// </summary>
        protected virtual void SetNullValue()
        {
            SourceRow[SourceField] = DBNull.Value;
            NotifyPropertyChanged("Field");
        }

        /// <summary>
        /// The value
        /// </summary>
        public DateTime? Field
        {
            get
            {
                if (SourceRow.IsNull(SourceField)) return null;
                return GetValue();
            }

            set
            {
                if (value.HasValue)
                {
                    if (SourceRow.IsNull(SourceField))
                    {
                        SetValue(value.Value);                        
                    }
                    else
                    {
                        if ((DateTime)SourceRow[SourceField] != value.Value)
                        {
                            SetValue(value.Value);
                        }
                    }
                }
                else
                {
                    if (!SourceRow.IsNull(SourceField))
                    {
                        SetNullValue();
                    }
                }
            }
        }
    }
}
