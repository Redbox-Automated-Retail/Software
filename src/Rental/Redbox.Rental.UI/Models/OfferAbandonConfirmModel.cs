using System;
using System.Windows;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;

namespace Redbox.Rental.UI.Models;

public class OfferAbandonConfirmModel
{
	public Func<ISpeechControl> OnGetSpeechControl;

	public string TitleText { get; set; }

	public Visibility TitleVisibility { get; set; }

	public string MessageText { get; set; }

	public Visibility MessageVisibility { get; set; }

	public string SubMessageText { get; set; }

	public Visibility SubMessageTextVisibility { get; set; }

	public string OkButtonText { get; set; }

	public Visibility OkButtonVisibility { get; set; }

	public DynamicRoutedCommand OkButtonCommand { get; set; }

	public bool CallbackExecuted { get; set; }

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
