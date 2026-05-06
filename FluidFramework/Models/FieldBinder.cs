using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;

namespace FluidFramework.Models
{
    /// <summary>
    /// Class used to bind from a property to a data column.
    /// </summary>
    /// <typeparam name="T">
    /// The CLR type of the column. For value-type columns that may contain DBNull,
    /// pass the nullable form (e.g. <c>Guid?</c>, <c>DateTime?</c>, <c>int?</c>).
    /// Reference types (e.g. <c>string</c>) can be passed directly.
    /// </typeparam>
    public class FieldBinder<T> : INotifyPropertyChanged
    {
        /// <summary>
        /// Event raised when the value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Notify the change of value.
        /// </summary>
        protected void NotifyPropertyChanged(string info)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
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
        public FieldBinder() { }

        /// <summary>
        /// Constructor
        /// </summary>
        public FieldBinder(DataRow row, string field)
        {
            SourceRow = row;
            SourceField = field;
        }

        /// <summary>
        /// Gets the value.
        /// </summary>
        protected virtual T GetValue()
        {
            return (T)SourceRow[SourceField];
        }

        /// <summary>
        /// Sets the value and raise a notification.
        /// </summary>
        protected virtual void SetValue(object value)
        {
            SourceRow[SourceField] = value ?? DBNull.Value;
            NotifyPropertyChanged(nameof(Field));
        }

        /// <summary>
        /// The value
        /// </summary>
        public T Field
        {
            get
            {
                if (SourceRow.IsNull(SourceField)) return default(T);
                return GetValue();
            }

            set
            {
                if (value == null)
                {
                    if (!SourceRow.IsNull(SourceField)) SetValue(null);
                }
                else if (SourceRow.IsNull(SourceField) ||
                         !EqualityComparer<T>.Default.Equals(GetValue(), value))
                {
                    SetValue(value);
                }
            }
        }
    }
}
