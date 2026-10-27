using System;
using System.Windows;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;

namespace Redbox.Rental.UI.Models;

public class BaseModel<T> : DependencyObject, IBaseModel
{
	public Func<ISpeechControl> ProcessGetSpeechControl { get; set; }

	protected static DependencyProperty CreateDependencyProperty(string propertyName, Type propertyType, object defaultValue = null, bool affectsRender = true)
	{
		return DependencyProperty.Register(propertyName, propertyType, typeof(T), new FrameworkPropertyMetadata(defaultValue)
		{
			AffectsRender = affectsRender
		});
	}
}
