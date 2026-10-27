using System;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;
using Redbox.KioskEngine.Environment.TextToSpeech;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.Services.Controllers;

internal class PromoConfirmationViewTextToSpeech
{
	internal static ISpeechControl GetSpeechControl(Func<PromoConfirmationViewModel> getViewModelFunction)
	{
		ISpeechControl result = new SpeechControl();
		result.Name = "tts_confirm_promo_view";
		AddSpeechPartInstructions(result, getViewModelFunction);
		result.SpeechParts.ForEach(delegate(ISpeechPart x)
		{
			x.SpeechControl = result;
		});
		return result;
	}

	private static void AddSpeechPartInstructions(ISpeechControl result, Func<PromoConfirmationViewModel> getViewModelFunction)
	{
		PromoConfirmationViewModel model = getViewModelFunction();
		ISpeechPart speechPart = new SpeechPart
		{
			Sequence = 1,
			Name = "dialog",
			Loop = true,
			StartPause = 500,
			EndPause = 500
		};
		speechPart.Refresh = delegate
		{
			speechPart.Clear();
			speechPart.Events.Add(new MapKeyPressToAction
			{
				KeyCode = "0",
				Action = delegate
				{
					model.CancelCommand.Execute(null);
				}
			});
			speechPart.Events.Add(new MapKeyPressToAction
			{
				KeyCode = "CANCEL",
				Action = delegate
				{
					model.CancelCommand.Execute(null);
				}
			});
			speechPart.Events.Add(new MapKeyPressToAction
			{
				KeyCode = "ENTER",
				Action = delegate
				{
					model.ApplyCommand.Execute(model.PromoCode);
				}
			});
			speechPart.Events.Add(new MapKeyPressToAction
			{
				KeyCode = "1",
				Action = delegate
				{
					model.ApplyCommand.Execute(model.PromoCode);
				}
			});
			speechPart.Texts.Add(new TextPart
			{
				Text = "You have selected the Promo ending with " + StoredPromoListTTSFunctions.GetMaskedPromoCodeEndCharacters(model.MaskedPromoCode)
			});
			speechPart.Texts.Add(new TextPart
			{
				Text = model.DescriptionText
			});
			speechPart.Texts.Add(new TextPart
			{
				Text = "To return to the previous screen without applying the promo code press 0 or cancel."
			});
			speechPart.Texts.Add(new TextPart
			{
				Text = "To apply the promo code press 1 or enter."
			});
			speechPart.Texts.Add(new TextPart
			{
				Text = model.LegalText
			});
		};
		result.SpeechParts.Add(speechPart);
	}
}
