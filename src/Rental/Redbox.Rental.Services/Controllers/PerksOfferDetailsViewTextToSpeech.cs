using System;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;
using Redbox.KioskEngine.Environment.TextToSpeech;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.Services.Controllers;

public class PerksOfferDetailsViewTextToSpeech
{
	private class TTSConstants
	{
		public const string Instructions = "Instructions";
	}

	public static ISpeechControl GetSpeechControl(Func<PerksOfferDetailsViewModel> getPerksOfferDetailsViewModelFunction)
	{
		ISpeechControl result = new SpeechControl();
		result.Name = "tts_perks_offer_details_view";
		AddSpeechPartInstructions(result, getPerksOfferDetailsViewModelFunction);
		result.SpeechParts.ForEach(delegate(ISpeechPart x)
		{
			x.SpeechControl = result;
		});
		return result;
	}

	private static void AddSpeechPartInstructions(ISpeechControl speechControl, Func<PerksOfferDetailsViewModel> getPerksOfferDetailsViewModelFunction)
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
			PerksOfferDetailsViewModel perksOfferDetailsViewModel = getPerksOfferDetailsViewModelFunction();
			if (perksOfferDetailsViewModel != null)
			{
				speechPart.Events.Add(new MapKeyPressToAction
				{
					KeyCode = "CANCEL",
					Action = delegate
					{
						perksOfferDetailsViewModel.ProcessOnCancelButtonClicked();
					}
				});
				speechPart.Texts.Add(new TextPart
				{
					Text = $"{perksOfferDetailsViewModel.OfferValueText} {perksOfferDetailsViewModel.OfferUnitsText}"
				});
				if (!string.IsNullOrEmpty(perksOfferDetailsViewModel.ProgressText))
				{
					speechPart.Texts.Add(new TextPart
					{
						Text = perksOfferDetailsViewModel.ProgressText
					});
				}
				speechPart.Texts.Add(new TextPart
				{
					Text = perksOfferDetailsViewModel.NameText
				});
				speechPart.Texts.Add(new TextPart
				{
					Text = perksOfferDetailsViewModel.DescriptionText
				});
				if (!string.IsNullOrEmpty(perksOfferDetailsViewModel.DateRangeText))
				{
					speechPart.Texts.Add(new TextPart
					{
						Text = perksOfferDetailsViewModel.DateRangeText
					});
				}
				speechPart.Texts.Add(new TextPart
				{
					Text = perksOfferDetailsViewModel.LegalInformationText
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
