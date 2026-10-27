using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;

namespace Redbox.Rental.UI.Controls;

public class TextToSpeechUserControl : RentalUserControl, ITextToSpeechControl
{
	private Dictionary<string, RoutedCommand> m_routedCommands = new Dictionary<string, RoutedCommand>();

	protected Dictionary<string, RoutedCommand> RoutedCommands => m_routedCommands;

	public virtual ISpeechControl GetSpeechControl()
	{
		return null;
	}

	public virtual string ControlText(string controlName)
	{
		return null;
	}

	public virtual void ExecuteCommand(string commandName, string parameter)
	{
		RoutedCommand value = null;
		if (m_routedCommands.TryGetValue(commandName, out value))
		{
			value.Execute(parameter, this);
		}
	}

	public virtual bool IsControlEnabled(string controlName)
	{
		return GetUIElement(controlName)?.IsEnabled ?? false;
	}

	public virtual bool IsControlVisible(string controlName)
	{
		return GetUIElement(controlName)?.IsVisible ?? false;
	}

	public virtual void ToggleADALine()
	{
		UIElement uIElement = GetUIElement("adaLine");
		if (uIElement == null)
		{
			uIElement = new Line
			{
				X1 = 0.0,
				Y1 = 286.0,
				Height = 3.0,
				Width = 1024.0,
				Stroke = new SolidColorBrush
				{
					Color = Colors.GreenYellow
				},
				StrokeThickness = 3.0,
				Visibility = Visibility.Visible
			};
			AddChild(uIElement);
		}
		else
		{
			uIElement.Visibility = ((uIElement.Visibility == Visibility.Visible) ? Visibility.Hidden : Visibility.Visible);
		}
	}

	protected void RegisterRoutedCommand(string commandName, RoutedCommand command)
	{
		if (!m_routedCommands.Keys.Contains(commandName))
		{
			m_routedCommands.Add(commandName, command);
		}
	}

	private UIElement GetUIElement(string controlName)
	{
		UIElement result = null;
		object obj = FindName(controlName);
		if (obj != null && obj is UIElement uIElement)
		{
			result = uIElement;
		}
		return result;
	}
}
