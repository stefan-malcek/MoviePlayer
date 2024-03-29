﻿using System;
using System.Globalization;
using System.Windows.Data;

namespace MoviePlayer.Converters
{
    public class ImageBoxSizeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {

            var size = int.Parse(value?.ToString() ?? "0");
            return size / 4;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
