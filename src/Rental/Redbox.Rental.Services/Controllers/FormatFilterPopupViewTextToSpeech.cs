using System;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;
using Redbox.KioskEngine.Environment.TextToSpeech;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.Services.Controllers;

public class FormatFilterPopupViewTextToSpeech
{
	private class TTSConstants
	{
		public const string Instructions = "Instructions";
	}

	public static ISpeechControl GetSpeechControl(Func<FormatSelectionPopupViewModel> getFormatFilterPopupViewModelFunction)
	{
		ISpeechControl result = new SpeechControl();
		result.Name = "tts_format_filter_popup_view";
		AddSpeechPartInstructions(result, getFormatFilterPopupViewModelFunction);
		result.SpeechParts.ForEach(delegate(ISpeechPart x)
		{
			x.SpeechControl = result;
		});
		return result;
	}

	private static void AddSpeechPartInstructions(ISpeechControl speechControl, Func<FormatSelectionPopupViewModel> getFormatFilterPopupViewModelFunction)
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
			FormatSelectionPopupViewModel viewModel = getFormatFilterPopupViewModelFunction();
			if (viewModel != null)
			{
				speechPart.Events.Add(new MapKeyPressToAction
				{
					KeyCode = "CANCEL",
					Action = delegate
					{
						viewModel.ProcessCancelButtonClicked();
					}
				});
				speechPart.Texts.Add(new TextPart
				{
					Text = "pick your format"
				});
				int num = 1;
				foreach (FormatButtonData button in viewModel.ButtonData)
				{
					speechPart.Events.Add(new MapKeyPressToAction
					{
						KeyCode = num.ToString(),
						Action = delegate
						{
							viewModel.ProcessFormatButtonClicked(button.Format);
						}
					});
					speechPart.Texts.Add(new TextPart
					{
						Text = "For " + button.Text + ", press " + num + "."
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
