using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace RecorderApp.Utility
{
    public class FilenameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var fn = (string)value;
            // find index of underscore
            int br = fn.IndexOf('_');

            // filename of vid segment
            string name = getName(fn, br);

            //br++;
            //int len = s.Length - br;
            string dt = getDate(fn, br);
            string dt_conv = dateTimeConvert(dt);

            return name + " " + dt_conv;
        }

        public string getName(string fullfn, int separator)
        {
            return fullfn.Substring(0, separator);
        }

        public string getDate(string fullfn, int separator)
        {
            // substring date part
            string date = fullfn.Substring(++separator, fullfn.Length - separator);
            //remove extension
            date = date.Remove(date.Length - 4, 4);
            return date;
        }
        public string dateTimeConvert(string dt)
        {
            try
            {

                DateTime d = DateTime.ParseExact(dt, "yyyyMMdd-HHmmss", CultureInfo.InvariantCulture);
                return d.ToString();
            } 
            catch 
            {
                return "";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
