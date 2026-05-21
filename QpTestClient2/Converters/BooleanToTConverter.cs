using Avalonia.Data.Converters;
using System;

namespace QpTestClient.Converters
{
    public abstract class BooleanToTConverter<T> : IValueConverter
    {
        /// <summary>
        /// 值为null时，对应的返回值
        /// </summary>
        public T NullValue { get; set; }
        /// <summary>
        /// 值为true时，对应的返回值
        /// </summary>
        public T TrueValue { get; set; }
        /// <summary>
        /// 值为false时，对应的返回值
        /// </summary>
        public T FalseValue { get; set; }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null || !(value is Boolean))
                return NullValue;
            Boolean ret = (Boolean)value;
            if (ret)
                return TrueValue;
            else
                return FalseValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
