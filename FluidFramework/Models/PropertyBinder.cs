using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace FluidFramework.Models
{
    /// <summary>
    /// Class used to bind a typed property of an object via getter/setter delegates.
    /// </summary>
    public class PropertyBinder<T> : INotifyPropertyChanged
    {
        private readonly Func<T> _get;
        private readonly Action<T> _set;

        /// <summary>
        /// Event raised when the value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Constructor
        /// </summary>
        public PropertyBinder(Func<T> getter, Action<T> setter)
        {
            _get = getter ?? throw new ArgumentNullException(nameof(getter));
            _set = setter ?? throw new ArgumentNullException(nameof(setter));
        }

        /// <summary>
        /// The value
        /// </summary>
        public T Field
        {
            get => _get();
            set
            {
                if (!EqualityComparer<T>.Default.Equals(_get(), value))
                {
                    _set(value);
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Field)));
                }
            }
        }
    }
}
