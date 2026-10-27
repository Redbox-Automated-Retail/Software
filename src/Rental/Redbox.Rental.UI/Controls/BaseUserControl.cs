using System;
using System.Windows;
using System.Windows.Input;

namespace Redbox.Rental.UI.Controls;

public class BaseUserControl : RentalUserControl
{
	public BaseUserControl()
	{
		base.DataContextChanged += DataContextHasChanged;
	}

	private void DataContextHasChanged(object sender, DependencyPropertyChangedEventArgs e)
	{
		DataContextHasChanged();
	}

	protected virtual void DataContextHasChanged()
	{
	}

	protected virtual void VisibleHasChanged()
	{
	}

	protected void InitVisibilityHasChanged()
	{
		base.IsVisibleChanged += delegate
		{
			VisibleHasChanged();
		};
	}

	protected static DependencyProperty CreateDependencyProperty<TOwner>(string propertyName, Type propertyType, object defaultValue = null)
	{
		return DependencyProperty.Register(propertyName, propertyType, typeof(TOwner), new FrameworkPropertyMetadata(defaultValue)
		{
			AffectsRender = true
		});
	}

	protected static void InvokeRoutedCommand(DynamicRoutedCommand routeCommand, object parameter = null)
	{
		if (routeCommand.CanExecute(parameter))
		{
			routeCommand.Execute(parameter);
		}
	}

	protected static void InvokeRoutedCommand(RoutedCommand routeCommand, IInputElement target, object parameter = null)
	{
		if (routeCommand.CanExecute(parameter, target))
		{
			routeCommand.Execute(parameter, target);
		}
	}
}
