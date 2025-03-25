using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace ChatApplication.Converters;

public class BoolToMessageColorConverter : IValueConverter
{
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
                // if the message is from the current user, use a green color
                // if the message is from the other user, use a differet color
                if (value != null)
                {
                        return (bool)value
                                ? new SolidColorBrush(Color.Parse("#2E8B57"))
                                : new SolidColorBrush(Color.Parse("#36454F"));
                }

                throw new Exception("Color is null");
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
                throw new NotImplementedException();
        }
}

public class BoolToAlignmentConverter : IValueConverter
{
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
                // if the message is from the current user, align it to the right
                // if the message is from the other user, align it to the left
                if (value != null)
                {
                        return (bool)value
                                ? Avalonia.Layout.HorizontalAlignment.Right
                                : Avalonia.Layout.HorizontalAlignment.Left;
                }

                throw new Exception("Alignment is null");
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
                throw new NotImplementedException();
        }
}