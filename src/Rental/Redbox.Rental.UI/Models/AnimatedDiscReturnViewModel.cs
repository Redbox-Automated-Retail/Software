using System;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;

namespace Redbox.Rental.UI.Models;

public class AnimatedDiscReturnViewModel : BaseModel<AnimatedDiscReturnViewModel>
{
	public Action OnGotItButtonClicked;

	public Action OnTimeout;

	public string GotItButtonText { get; set; }

	public string Title { get; set; }

	public string Message { get; set; }

	public string TutorialPart1 { get; set; }

	public string TutorialPart2 { get; set; }

	public string TutorialPart3 { get; set; }

	public Func<ISpeechControl> OnGetSpeechControl { get; set; }

	public void ProcessOnGotItButtonClicked()
	{
		OnGotItButtonClicked?.Invoke();
	}

	public void ProcessOnTimeout()
	{
		OnTimeout?.Invoke();
	}
}
