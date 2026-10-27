using System.Windows.Input;

namespace Redbox.Rental.UI.Views;

public static class FormatSelectionPopupViewUserControlCommands
{
	public static readonly RoutedCommand FormatCommand = new RoutedCommand();

	public static readonly RoutedCommand PurchaseCommand = new RoutedCommand();

	public static readonly RoutedCommand CancelCommand = new RoutedCommand();

	public static readonly RoutedCommand DigitalCodeInfoCommand = new RoutedCommand();
}
