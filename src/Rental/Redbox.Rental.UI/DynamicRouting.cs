using System;

namespace Redbox.Rental.UI;

public class DynamicRouting
{
	private static DynamicRoutedCommand InitRoutedCommand(DynamicRoutedCommand routeCommand, dynamic execute, dynamic canExecute)
	{
		if (routeCommand == null)
		{
			throw new NullReferenceException("The routeCommand can not be null");
		}
		routeCommand.InitDynamicExecutes(execute, canExecute);
		return routeCommand;
	}

	public static DynamicRoutedCommand RegisterRoutedCommand(DynamicRoutedCommand routeCommand, Action execute, Func<bool> canExecute = null)
	{
		return InitRoutedCommand(routeCommand, execute, canExecute);
	}

	public static DynamicRoutedCommand RegisterRoutedCommand(DynamicRoutedCommand routeCommand, Action<int> execute, Predicate<int> canExecute = null)
	{
		return InitRoutedCommand(routeCommand, execute, canExecute);
	}

	public static DynamicRoutedCommand RegisterRoutedCommand(DynamicRoutedCommand routeCommand, Action<bool> execute, Predicate<bool> canExecute = null)
	{
		return InitRoutedCommand(routeCommand, execute, canExecute);
	}

	public static DynamicRoutedCommand RegisterRoutedCommand(DynamicRoutedCommand routeCommand, Action<double> execute, Predicate<double> canExecute = null)
	{
		return InitRoutedCommand(routeCommand, execute, canExecute);
	}

	public static DynamicRoutedCommand RegisterRoutedCommand(DynamicRoutedCommand routeCommand, Action<string> execute, Predicate<string> canExecute = null)
	{
		return InitRoutedCommand(routeCommand, execute, canExecute);
	}

	public static DynamicRoutedCommand RegisterRoutedCommand(DynamicRoutedCommand routeCommand, Action<object> execute, Predicate<object> canExecute = null)
	{
		return InitRoutedCommand(routeCommand, execute, canExecute);
	}

	public static DynamicRoutedCommand RegisterRoutedCommand<T>(DynamicRoutedCommand routeCommand, Action<T> execute, Predicate<object> canExecute = null)
	{
		return InitRoutedCommand(routeCommand, execute, canExecute);
	}
}
