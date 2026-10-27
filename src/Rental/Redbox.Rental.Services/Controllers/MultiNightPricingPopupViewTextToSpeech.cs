using System;
using System.Windows;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;
using Redbox.KioskEngine.Environment.TextToSpeech;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.Services.Controllers;

public class MultiNightPricingPopupViewTextToSpeech
{
	private class TTSConstants
	{
		public const string Instructions = "Instructions";
	}

	public static ISpeechControl GetSpeechControl(Func<MultiNightPricingPopupViewModel> getMultiNightPricingPopupViewModelFunction)
	{
		ISpeechControl result = new SpeechControl();
		result.Name = "tts_multi_night_pricing_popup_view";
		AddSpeechPartInstructions(result, getMultiNightPricingPopupViewModelFunction);
		result.SpeechParts.ForEach(delegate(ISpeechPart x)
		{
			x.SpeechControl = result;
		});
		return result;
	}

	private static void AddSpeechPartInstructions(ISpeechControl speechControl, Func<MultiNightPricingPopupViewModel> getMultiNightPricingPopupViewModelFunction)
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
			MultiNightPricingPopupViewModel multiNightPricingPopupViewModel = getMultiNightPricingPopupViewModelFunction();
			if (multiNightPricingPopupViewModel != null)
			{
				speechPart.Events.Add(new MapKeyPressToAction
				{
					KeyCode = "CANCEL",
					Action = delegate
					{
						multiNightPricingPopupViewModel.ProcessOnCancelButtonClicked();
					}
				});
				speechPart.Events.Add(new MapKeyPressToAction
				{
					KeyCode = "1",
					Action = delegate
					{
						multiNightPricingPopupViewModel.ProcessOnOneNightButtonClicked();
					}
				});
				speechPart.Events.Add(new MapKeyPressToAction
				{
					KeyCode = "2",
					Action = delegate
					{
						multiNightPricingPopupViewModel.ProcessOnMultiNightButtonClicked();
					}
				});
				speechPart.Events.Add(new MapKeyPressToAction
				{
					KeyCode = "3",
					Action = delegate
					{
						multiNightPricingPopupViewModel.ProcessOnBuyButtonClicked();
					}
				});
				speechPart.Texts.Add(new TextPart
				{
					Text = multiNightPricingPopupViewModel.HeaderText
				});
				speechPart.Texts.Add(new TextPart
				{
					Text = multiNightPricingPopupViewModel.DescriptionText
				});
				if (multiNightPricingPopupViewModel.OneNightButtonVisibility == Visibility.Visible)
				{
					speechPart.Texts.Add(new TextPart
					{
						Text = multiNightPricingPopupViewModel.OneNightButtonText1 + " FOR " + multiNightPricingPopupViewModel.OneNightButtonText2 + ", press 1."
					});
				}
				if (multiNightPricingPopupViewModel.MultiNightButtonVisibility == Visibility.Visible)
				{
					speechPart.Texts.Add(new TextPart
					{
						Text = multiNightPricingPopupViewModel.MultiNightButtonText1 + " FOR " + multiNightPricingPopupViewModel.MultiNightButtonText2 + ", press 2."
					});
				}
				if (multiNightPricingPopupViewModel.BuyButtonVisibility == Visibility.Visible)
				{
					speechPart.Texts.Add(new TextPart
					{
						Text = multiNightPricingPopupViewModel.BuyButtonText1 + " FOR " + multiNightPricingPopupViewModel.BuyButtonText2 + ", press 3."
					});
				}
				speechPart.Texts.Add(new TextPart
				{
					Text = "To cancel, press cancel."
				});
				speechPart.Texts.Add(new TextPart
				{
					Text = multiNightPricingPopupViewModel.DisclaimerText
				});
			}
		};
		speechControl.SpeechParts.Add(speechPart);
	}
}
