using System;
using System.Globalization;
using System.Windows.Data;

namespace RtStorage.Views.Converters
{
    [ValueConversion(typeof(long), typeof(string))]
    public class DataSizeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            long size = (long)value;
            string sizeStr;

            if (size > 1024*1024*1024)
            {
                sizeStr = string.Format("{0:F2}GB", size/(1024.0*1024.0*1024.0));

            }
            else if (size > 1024*1024)
            {
                sizeStr = string.Format("{0:F2}MB", size/(1024.0*1024.0));

            }
            else if (size > 1024)
            {
                sizeStr = string.Format("{0:F2}KB", size/(1024.0));
            }
            else
            {
                sizeStr = string.Format("{0}B", size);

            }
            return sizeStr;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
