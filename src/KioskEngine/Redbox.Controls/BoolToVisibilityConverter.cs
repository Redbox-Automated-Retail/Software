using System.Windows;
using System.Windows.Data;

namespace Redbox.Controls;

[ValueConversion(typeof(bool), typeof(Visibility))]
public class BoolToVisibilityConverter : ReadOnlyConverter
{
	public override object Convert(object value)
	{
		if (value is bool && (bool)value)
		{
			return Visibility.Visible;
		}
		return Visibility.Collapsed;
	}
}
