using System.Windows.Input;

namespace Redbox.Rental.UI.Views;

public static class RedboxPlusLeadGenerationOfferViewUserControlCommands
{
	public static readonly RoutedCommand NoButtonCommand = new RoutedCommand();

	public static readonly RoutedCommand EmailButtonCommand = new RoutedCommand();

	public static readonly RoutedCommand QRCodeButtonCommand = new RoutedCommand();
}
