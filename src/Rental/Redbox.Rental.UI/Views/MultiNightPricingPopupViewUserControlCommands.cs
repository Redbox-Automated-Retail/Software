using System.Windows.Input;

namespace Redbox.Rental.UI.Views;

public static class MultiNightPricingPopupViewUserControlCommands
{
	public static readonly RoutedCommand OneNightCommand = new RoutedCommand();

	public static readonly RoutedCommand MultiNightCommand = new RoutedCommand();

	public static readonly RoutedCommand BuyCommand = new RoutedCommand();

	public static readonly RoutedCommand CancelCommand = new RoutedCommand();
}
