using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.Views;

[ValueConversion(typeof(PhoneAndPinViewModel.DisplayType), typeof(bool))]
public class DisplayTypeToSelectedConverter : IValueConverter
{
	public object Convert(object value, Type t, object parameter, CultureInfo culture)
	{
		return (PhoneAndPinViewModel.DisplayType)value == (PhoneAndPinViewModel.DisplayType)Enum.Parse(typeof(PhoneAndPinViewModel.DisplayType), (string)parameter);
	}

	public object ConvertBack(object value, Type t, object parameter, CultureInfo culture)
	{
		if ((bool)value)
		{
			return parameter;
		}
		return DependencyProperty.UnsetValue;
	}
}
