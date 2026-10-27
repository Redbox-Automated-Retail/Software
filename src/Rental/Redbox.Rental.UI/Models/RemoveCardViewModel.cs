using System;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;

namespace Redbox.Rental.UI.Models;

public class RemoveCardViewModel
{
	public Func<ISpeechControl> OnGetSpeechControl;

	public string TitleText { get; set; }

	public Action ContinueAction { get; set; }

	public int Timeout { get; set; }

	public bool CardHasBeenRemoved { get; set; }

	public Action<RemoveCardViewModel> StartCardRemovedYetCheck { get; set; }

	public Action<bool> ProcessCardRemovedCheckResult { get; set; }

	public Action CardNeverRemoved { get; set; }

	public Action StopTimeOutTimerAction { get; set; }
}
