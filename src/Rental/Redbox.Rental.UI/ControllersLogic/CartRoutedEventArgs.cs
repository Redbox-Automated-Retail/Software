using System.Windows;

namespace Redbox.Rental.UI.ControllersLogic;

public class CartRoutedEventArgs : RoutedEventArgs
{
	public string ButtonName { get; set; }

	public dynamic Parameter { get; set; }

	public CartRoutedEventArgs(RoutedEvent routedEvent)
	{
		base.RoutedEvent = routedEvent;
	}
}
