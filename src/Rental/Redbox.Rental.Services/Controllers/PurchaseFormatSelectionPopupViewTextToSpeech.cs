using System;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;
using Redbox.KioskEngine.Environment.TextToSpeech;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.Services.Controllers;

public class PurchaseFormatSelectionPopupViewTextToSpeech
{
	private class TTSConstants
	{
		public const string Instructions = "Instructions";
	}

	public static ISpeechControl GetSpeechControl(Func<FormatSelectionPopupViewModel> getFormatSelectionPopupViewModelFunction)
	{
		ISpeechControl result = new SpeechControl();
		result.Name = "tts_format_selection_popup_view";
		AddSpeechPartInstructions(result, getFormatSelectionPopupViewModelFunction);
		result.SpeechParts.ForEach(delegate(ISpeechPart x)
		{
			x.SpeechControl = result;
		});
		return result;
	}

	private static void AddSpeechPartInstructions(ISpeechControl speechControl, Func<FormatSelectionPopupViewModel> getFormatSelectionPopupViewModelFunction)
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
			FormatSelectionPopupViewModel formatSelectionPopupViewModel = getFormatSelectionPopupViewModelFunction();
			if (formatSelectionPopupViewModel != null)
			{
				speechPart.Texts.Add(new TextPart
				{
					Text = formatSelectionPopupViewModel.HeaderText
				});
				speechPart.Texts.Add(new TextPart
				{
					Text = formatSelectionPopupViewModel.DescriptionText
				});
				int num = 1;
				foreach (FormatButtonData button in formatSelectionPopupViewModel.ButtonData)
				{
					speechPart.Events.Add(new MapKeyPressToAction
					{
						KeyCode = num.ToString(),
						Action = delegate
						{
							formatSelectionPopupViewModel.ProcessFormatButtonClicked(button.Format);
						}
					});
					speechPart.Texts.Add(new TextPart
					{
						Text = $"For {button.Text}, press {num.ToString()}."
					});
					num++;
				}
				speechPart.Events.Add(new MapKeyPressToAction
				{
					KeyCode = "CANCEL",
					Action = delegate
					{
						formatSelectionPopupViewModel.ProcessCancelButtonClicked();
					}
				});
				speechPart.Texts.Add(new TextPart
				{
					Text = "To cancel, press cancel."
				});
			}
		};
		speechControl.SpeechParts.Add(speechPart);
	}
}
