using System.Windows.Input;

namespace Redbox.Rental.UI.Controls;

public static class KeypadUserControlCommands
{
	public static readonly RoutedCommand NumberCommand = new RoutedCommand();

	public static readonly RoutedCommand ClearAllCommand = new RoutedCommand();

	public static readonly RoutedCommand BackCommand = new RoutedCommand();
}
