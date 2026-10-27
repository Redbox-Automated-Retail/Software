using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;
using Redbox.KioskEngine.Environment.TextToSpeech;
using Redbox.Rental.UI.Controls;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.Views;

public class ViewUserControl : TextToSpeechUserControl, IWPFActor
{
	protected string SpeechViewName { get; set; }

	protected string SimpleSpeechPart => "SimpleSpeechPart";

	protected bool NeedKeyboardFocus { get; set; }

	public ViewUserControl()
	{
		base.DataContextChanged += DataContextHasChanged;
	}

	protected void ApplyTheme<T>(UIElement mainControl) where T : UIElement
	{
		ITheme theme = ServiceLocator.Instance.GetService<IThemeService>()?.CurrentTheme;
		IEnumerable<T> enumerable = RentalUserControl.FindLogicalChildren<T>(mainControl);
		if (enumerable == null)
		{
			return;
		}
		foreach (T item in enumerable)
		{
			theme?.SetStyle(item as FrameworkElement);
		}
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

	protected virtual void InitSpeechPart(ISpeechPart speechPart)
	{
	}

	protected virtual ISpeechControl CreateSimpleSpeechControl()
	{
		SpeechControl obj = new SpeechControl
		{
			Name = $"tts_{SpeechViewName}"
		};
		ISpeechPart speechPart = new SpeechPart
		{
			Sequence = 1,
			Name = SimpleSpeechPart,
			StartPause = 100
		};
		speechPart.Refresh = delegate
		{
			speechPart.Clear();
			InitSpeechPart(speechPart);
		};
		((ISpeechControl)obj).SpeechParts.Add(speechPart);
		return obj;
	}

	public override ISpeechControl GetSpeechControl()
	{
		if (SpeechViewName != null)
		{
			return CreateSimpleSpeechControl();
		}
		return (base.DataContext is IBaseModel { ProcessGetSpeechControl: not null } baseModel) ? baseModel.ProcessGetSpeechControl() : null;
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
