using System.Windows.Input;

namespace Redbox.Rental.UI.Views;

public static class Commands
{
	public static readonly RoutedCommand Yes = new RoutedCommand();

	public static readonly RoutedCommand No = new RoutedCommand();
}
