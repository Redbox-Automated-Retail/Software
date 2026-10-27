using System.Windows.Input;

namespace Redbox.Rental.UI.Views;

public static class PhoneAndPinViewUserControlCommands
{
	public static readonly RoutedCommand PhoneDisplayCommand = new RoutedCommand();

	public static readonly RoutedCommand PinDisplayCommand = new RoutedCommand();

	public static readonly RoutedCommand PinConfirmDisplayCommand = new RoutedCommand();

	public static readonly RoutedCommand StartCommand = new RoutedCommand();

	public static readonly RoutedCommand SignInCommand = new RoutedCommand();

	public static readonly RoutedCommand TermsButtonCommand = new RoutedCommand();
}
