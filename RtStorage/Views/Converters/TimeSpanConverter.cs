using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace RtStorage.Views.Converters
{
    [ValueConversion(typeof(long), typeof(string))]
    public class TimeSpanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            long time = (long)value;

            
            return TimeSpan.FromMilliseconds(time).ToString(@"hh\:mm\:ss");
            
            // 書式はパラメータで渡せたほうがいいか？
            //return TimeSpan.FromMilliseconds(time).ToString(@"g");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
