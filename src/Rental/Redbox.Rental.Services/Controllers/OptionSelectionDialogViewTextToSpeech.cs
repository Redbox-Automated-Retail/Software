using System;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;
using Redbox.KioskEngine.Environment.TextToSpeech;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.Services.Controllers;

public class OptionSelectionDialogViewTextToSpeech
{
	private class TTSConstants
	{
		public const string Instructions = "Instructions";
	}

	public static ISpeechControl GetSpeechControl(Func<OptionSelectionDialogViewModel> getViewModelFunction)
	{
		ISpeechControl result = new SpeechControl();
		result.Name = "tts_option_selection_dialog_view";
		AddSpeechPartInstructions(result, getViewModelFunction);
		result.SpeechParts.ForEach(delegate(ISpeechPart x)
		{
			x.SpeechControl = result;
		});
		return result;
	}

	private static void AddSpeechPartInstructions(ISpeechControl speechControl, Func<OptionSelectionDialogViewModel> getViewModelFunction)
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
			OptionSelectionDialogViewModel viewModel = getViewModelFunction();
			if (viewModel != null)
			{
				speechPart.Events.Add(new MapKeyPressToAction
				{
					KeyCode = "CANCEL",
					Action = delegate
					{
						viewModel.CancelCommand.Execute(null);
					}
				});
				speechPart.Texts.Add(new TextPart
				{
					Text = viewModel.HeaderText
				});
				speechPart.Texts.Add(new TextPart
				{
					Text = viewModel.DescriptionText
				});
				int num = 1;
				foreach (ButtonData button in viewModel.ButtonData)
				{
					speechPart.Events.Add(new MapKeyPressToAction
					{
						KeyCode = num.ToString(),
						Action = delegate
						{
							button.Command.Execute(button.Tag);
						}
					});
					speechPart.Texts.Add(new TextPart
					{
						Text = "For " + (string.IsNullOrEmpty(button.TtsText) ? button.Text : button.TtsText) + ", press " + num + "."
					});
					num++;
				}
				speechPart.Texts.Add(new TextPart
				{
					Text = "To cancel, press cancel."
				});
			}
		};
		speechControl.SpeechParts.Add(speechPart);
	}
}
