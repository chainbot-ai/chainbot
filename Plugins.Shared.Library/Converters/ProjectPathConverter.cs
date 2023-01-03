using Plugins.Shared.Library.Librarys;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Plugins.Shared.Library.Converters
{
    public class ProjectPathConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return DependencyProperty.UnsetValue;

            var filePath = (string)value;

            var retPath = "";
            if (!System.IO.Path.IsPathRooted(filePath))
            {
                if(parameter == null)
                {
                    retPath = System.IO.Path.Combine(SharedObject.Instance.ProjectPath, filePath);
                }
                else
                {
                    retPath = System.IO.Path.Combine(SharedObject.Instance.ProjectPath + @"\" + (string)parameter, filePath);
                }
                
            }
            else
            {
                retPath = filePath;
            }

            if(System.IO.File.Exists(retPath))
            {
                return Common.BitmapFromUri(new Uri(retPath));
            }
            else
            {
                return DependencyProperty.UnsetValue;
            }
            
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

       
    }
}
