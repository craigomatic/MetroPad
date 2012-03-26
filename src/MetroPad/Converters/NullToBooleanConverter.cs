using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace MetroPad.Converters
{
    public class NullToBooleanConverter : IValueConverter
    {
        public bool ReturnsTrueOnNull { get; set; }

        public NullToBooleanConverter()
        {
            this.ReturnsTrueOnNull = true;
        }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (this.ReturnsTrueOnNull)
            {
                return value == null ? true : false;
            }
            else
            {
                return value != null ? true : false;
            }
        }
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
