using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Redbox.Controls;

public abstract class ReadOnlyConverter : IValueConverter
{
	public abstract object Convert(object value);

	public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
	{
		return Convert(value);
	}

	public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
	{
		return DependencyProperty.UnsetValue;
	}
}
