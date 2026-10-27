using System.Windows.Input;

namespace Redbox.Rental.UI.Controls;

public static class PagingButtonsControlCommands
{
	public static readonly RoutedCommand LeftArrowPressedCommand = new RoutedCommand();

	public static readonly RoutedCommand RightArrowPressedCommand = new RoutedCommand();

	public static readonly RoutedCommand NumberPressedCommand = new RoutedCommand();
}
