using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using Redbox.Rental.UI;

namespace Redbox.ControlPanel.UI.Views;

public class ViewControl : UserControl
{
	protected bool NeedKeyboardFocus { get; set; }

	public ViewControl()
	{
		base.DataContextChanged += DataContextHasChanged;
	}

	protected TModel GetModel<TModel>()
	{
		return (TModel)base.DataContext;
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
			if (NeedKeyboardFocus)
			{
				base.Focusable = true;
				Keyboard.Focus(this);
			}
		};
	}

	protected void InitKeyboardFocus(KeyEventHandler keyboardAction)
	{
		InitVisibilityHasChanged();
		if (keyboardAction != null)
		{
			base.KeyDown += keyboardAction;
			NeedKeyboardFocus = true;
		}
	}

	protected void SetFocus(UIElement element)
	{
		Action method = delegate
		{
			element.Focus();
			Keyboard.Focus(element);
		};
		base.Dispatcher.BeginInvoke(method, DispatcherPriority.SystemIdle);
	}

	protected static void InvokeRoutedCommand(DynamicRoutedCommand routeCommand, object parameter = null)
	{
		if (routeCommand != null && routeCommand.CanExecute(parameter))
		{
			routeCommand.Execute(parameter);
		}
	}

	protected static void InvokeRoutedCommand(RoutedCommand routeCommand, IInputElement target, object parameter = null)
	{
		if (routeCommand != null && routeCommand.CanExecute(parameter, target))
		{
			routeCommand.Execute(parameter, target);
		}
	}
}
