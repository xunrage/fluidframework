using System;
using System.ComponentModel;
using System.Data;

namespace FluidFramework.Models
{
    /// <summary>
    /// Class used to bind a Guid from a property to a data column.
    /// </summary>
    public class GuidPipe : INotifyPropertyChanged
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
        public GuidPipe() { }

        /// <summary>
        /// Constructor
        /// </summary>
        public GuidPipe(DataRow row, string field)
        {
            SourceRow = row;
            SourceField = field;
        }

        /// <summary>
        /// Gets the value.
        /// </summary>
        protected virtual Guid GetValue()
        {
            return (Guid)SourceRow[SourceField];
        }

        /// <summary>
        /// Sets the value and raise a notification.
        /// </summary>
        protected virtual void SetValue(Guid value)
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
        public Guid? Field
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
                        if ((Guid)SourceRow[SourceField] != value.Value)
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
