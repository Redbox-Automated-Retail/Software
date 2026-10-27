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
			object obj = frameworkElement?.FindResource(styleName);
			if (obj != null)
			{
				result = obj as Style;
			}
		}
		return result;
	}

	public static Brush GetBrush(FrameworkElement frameworkElement, string brushName)
	{
		Brush result = null;
		object obj = frameworkElement?.FindResource(brushName);
		if (obj != null)
		{
			result = obj as Brush;
		}
		return result;
	}
}
