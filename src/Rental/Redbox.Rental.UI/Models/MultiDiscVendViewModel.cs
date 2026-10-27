using System;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;

namespace Redbox.Rental.UI.Models;

public class MultiDiscVendViewModel
{
	public Func<ISpeechControl> OnGetSpeechControl;

	public string HeaderText { get; set; }

	public string ProductListText { get; set; }

	public string GrabDiscsText { get; set; }

	public string NoteHeaderText { get; set; }

	public string NoteBodyText { get; set; }

	public string NumberOfDiscsText { get; set; }

	public string DiscVendText { get; set; }

	public string OkayButtonText { get; set; }

	public bool CallbackExecuted { get; set; }

	public event Action OkayButtonAction;

	public void ProcessOkayButtonClick()
	{
		OkayButtonAction?.Invoke();
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
