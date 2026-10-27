using System;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;
using Redbox.KioskEngine.Environment.TextToSpeech;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.Services.Controllers;

public class MessagePopupViewTextToSpeech
{
	private class TTSConstants
	{
		public const string Instructions = "Instructions";
	}

	public static ISpeechControl GetSpeechControl(Func<MessagePopupViewModel> getMessagePopupViewModelFunction)
	{
		ISpeechControl result = new SpeechControl();
		result.Name = "tts_message_popup_view";
		AddSpeechPartInstructions(result, getMessagePopupViewModelFunction);
		result.SpeechParts.ForEach(delegate(ISpeechPart x)
		{
			x.SpeechControl = result;
		});
		return result;
	}

	private static void AddSpeechPartInstructions(ISpeechControl speechControl, Func<MessagePopupViewModel> getMessagePopupViewModelFunction)
	{
		ISpeechPart speechPart = new SpeechPart
		{
			Name = "Instructions",
			Sequence = 1,
			KeySequenceDelay = 0,
			Loop = false,
			StartPause = 500,
			EndPause = 5000
		};
		speechPart.Refresh = delegate
		{
			speechPart.Clear();
			MessagePopupViewModel viewModel = getMessagePopupViewModelFunction();
			if (viewModel != null)
			{
				speechPart.Events.Add(new MapKeyPressToAction
				{
					KeyCode = "CANCEL",
					Action = delegate
					{
						viewModel.OnButton1Clicked();
					}
				});
				speechPart.Events.Add(new MapKeyPressToAction
				{
					KeyCode = "ENTER",
					Action = delegate
					{
						viewModel.OnButton2Clicked();
					}
				});
				speechPart.Texts.Add(new TextPart
				{
					Text = viewModel.Title
				});
				speechPart.Texts.Add(new TextPart
				{
					Text = viewModel.Message
				});
				speechPart.Texts.Add(new TextPart
				{
					Text = "For " + viewModel.Button1Text + ", press the CANCEL key."
				});
				speechPart.Texts.Add(new TextPart
				{
					Text = "For " + viewModel.Button2Text + ", press the ENTER key."
				});
			}
		};
		speechControl.SpeechParts.Add(speechPart);
	}
}
