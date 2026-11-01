using System.Windows;
using System.Windows.Media;

namespace Redbox.Rental.Services.Controllers;

public class WPFFunctions
{
	public static Style GetStyle(FrameworkElement frameworkElement, string styleName)
	{
		Style result = null;
		if (!string.IsNullOrEmpty(styleName))
		{
			object resource = frameworkElement?.FindResource(styleName);
			if (resource != null)
			{
				result = resource as Style;
			}
		}
		return result;
	}

	public static Brush GetBrush(FrameworkElement frameworkElement, string brushName)
	{
		Brush result = null;
		object resource = frameworkElement?.FindResource(brushName);
		if (resource != null)
		{
			result = resource as Brush;
		}
		return result;
	}
}
