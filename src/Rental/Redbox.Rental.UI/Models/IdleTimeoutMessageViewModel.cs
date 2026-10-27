using System;
using System.Windows;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;

namespace Redbox.Rental.UI.Models;

public class IdleTimeoutMessageViewModel : DependencyObject
{
	public Action OnButtonNoClicked;

	public Action OnButtonYesClicked;

	public Func<ISpeechControl> OnGetSpeechControl;

	public static readonly DependencyProperty ButtonNoTextProperty = DependencyProperty.Register("ButtonNoText", typeof(string), typeof(IdleTimeoutMessageViewModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty ButtonYesTextProperty = DependencyProperty.Register("ButtonYesText", typeof(string), typeof(IdleTimeoutMessageViewModel), new FrameworkPropertyMetadata(null));

	public static readonly DependencyProperty MessageTextProperty = DependencyProperty.Register("MessageText", typeof(string), typeof(IdleTimeoutMessageViewModel), new FrameworkPropertyMetadata(null));

	public int Timeout { get; set; }

	public string ButtonNoText
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(ButtonNoTextProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(ButtonNoTextProperty, value);
			});
		}
	}

	public string ButtonYesText
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(ButtonYesTextProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(ButtonYesTextProperty, value);
			});
		}
	}

	public string MessageText
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(MessageTextProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(MessageTextProperty, value);
			});
		}
	}

	public void ProcessOnButtonNoClicked()
	{
		OnButtonNoClicked?.Invoke();
	}

	public void ProcessOnButtonYesClicked()
	{
		OnButtonYesClicked?.Invoke();
	}

	public ISpeechControl ProcessGetSpeechControl()
	{
		ISpeechControl result = null;
		if (OnGetSpeechControl != null)
		{
			result = OnGetSpeechControl();
		}
		return result;
	}
}
