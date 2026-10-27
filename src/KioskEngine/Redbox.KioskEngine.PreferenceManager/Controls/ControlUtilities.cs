using System.Windows;

namespace Redbox.KioskEngine.PreferenceManager.Controls;

public class ControlUtilities
{
	public static Window GetParentWindow(FrameworkElement element)
	{
		Window window = null;
		FrameworkElement frameworkElement = element;
		while (window == null && frameworkElement?.Parent != null)
		{
			frameworkElement = frameworkElement.Parent as FrameworkElement;
			if (frameworkElement is Window)
			{
				window = frameworkElement as Window;
			}
		}
		return window;
	}
}
