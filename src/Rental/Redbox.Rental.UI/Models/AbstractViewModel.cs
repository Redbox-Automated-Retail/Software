using System;
using System.Windows;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;

namespace Redbox.Rental.UI.Models;

public abstract class AbstractViewModel : DependencyObject, IProcessTextToSpeech
{
	public Func<ISpeechControl> OnGetSpeechControl;

	public ISpeechControl ProcessGetSpeechControl()
	{
		return OnGetSpeechControl?.Invoke();
	}

	public static DependencyProperty CreateDependencyProperty<T>(string name, Type returnType, object defaultValue = null)
	{
		return DependencyProperty.Register(name, returnType, typeof(T), new FrameworkPropertyMetadata(defaultValue));
	}
}
