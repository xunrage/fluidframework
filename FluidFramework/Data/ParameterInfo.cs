using System;

namespace FluidFramework.Data
{
    /// <summary>
    /// Paramer class used to query data.
    /// </summary>
    public class ParameterInfo
    {
        /// <summary>
        /// Parameter name
        /// </summary>
        public string Parameter { get; set; }

        /// <summary>
        /// Parameter value
        /// </summary>
        public object Value { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        public ParameterInfo(){}

        /// <summary>
        /// Constructor with field values.
        /// </summary>
        public ParameterInfo(string parameter, object value)
        {
            Parameter = parameter;
            Value = value;
        }

        /// <summary>
        /// Helper method to create a parameter that has DBNull value if the value is empty and optionally to be converted to the given type.
        /// </summary>
        public static ParameterInfo CreateWithDBNull(string parameter, object value, Type convertType = null)
        {
            ParameterInfo p = new ParameterInfo {Parameter = parameter};
            if (value == null)
            {
                p.Value = DBNull.Value;
                return p;
            }

            Func<object, Type, object> xrConvert = (v, t) =>
            {
                if (t == null)
                {
                    return v;
                }
                try
                {
                    return Convert.ChangeType(v, t);
                }
                catch
                {
                    return DBNull.Value;
                }
            };

            if (value is String)
            {
                p.Value = ((String) value) == String.Empty ? DBNull.Value : xrConvert(value, convertType);
            }
            else if (value is Guid)
            {
                p.Value = ((Guid)value) == Guid.Empty ? DBNull.Value : xrConvert(value, convertType);
            }
            else if (value is DateTime)
            {
                p.Value = ((DateTime)value) == default(DateTime) ? DBNull.Value : xrConvert(value, convertType);
            }
            else
            {
                p.Value = xrConvert(value, convertType);
            }
            return p;
        }
    }
}
