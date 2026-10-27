using System;
using System.Globalization;
using System.Windows.Data;

namespace Redbox.Controls;

public class GreaterOrEqualConverter : IMultiValueConverter
{
	public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
	{
		if (values != null && values.Length == 2 && values[0] is int && values[1] is int && (int)values[0] >= (int)values[1])
		{
			return true;
		}
		return false;
	}

	public object[] ConvertBack(object values, Type[] targetType, object parameter, CultureInfo culture)
	{
		return null;
	}
}
