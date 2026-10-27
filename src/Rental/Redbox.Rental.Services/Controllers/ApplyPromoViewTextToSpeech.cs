using System;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;
using Redbox.KioskEngine.Environment.TextToSpeech;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.Services.Controllers;

public class ApplyPromoViewTextToSpeech
{
	public static ISpeechControl GetSpeechControl(Func<ApplyPromoViewModel> getViewModelFunction)
	{
		ISpeechControl result = new SpeechControl();
		result.Name = "tts_apply_promo_view";
		AddSpeechPartInstructions(result, getViewModelFunction);
		result.SpeechParts.ForEach(delegate(ISpeechPart x)
		{
			x.SpeechControl = result;
		});
		return result;
	}

	private static void AddSpeechPartInstructions(ISpeechControl result, Func<ApplyPromoViewModel> getViewModelFunction)
	{
		ApplyPromoViewModel model = getViewModelFunction();
		ITextToSpeechService ttsService = ServiceLocator.Instance.GetService<ITextToSpeechService>();
		ISpeechPart speechPart = new SpeechPart
		{
			Sequence = 1,
			Name = "ApplyPromo",
			Loop = false,
			StartPause = 500,
			EndPause = 500
		};
		speechPart.Refresh = delegate
		{
			speechPart.Clear();
			AddBaseEvents(model, speechPart);
			speechPart.Texts.Add(new TextPart
			{
				Text = "Apply a Promo"
			});
			speechPart.Texts.Add(new TextPart
			{
				Text = "You're a promo pro! Which one would you like to use now?"
			});
		};
		result.SpeechParts.Add(speechPart);
		ISpeechPart speechPart2 = new SpeechPart
		{
			Sequence = 2,
			Name = "PromoList",
			Loop = true,
			StartPause = 500,
			EndPause = 1000
		};
		speechPart2.Refresh = delegate
		{
			speechPart2.Clear();
			AddBaseEvents(model, speechPart2);
			StoredPromoListTTSFunctions.AddPromoListTTS(model.PromoLogic, ttsService, speechPart2, model);
			speechPart2.Texts.Add(new TextPart
			{
				Text = "Have another promo?  Press Enter to enter another promo code."
			});
			speechPart2.Texts.Add(new TextPart
			{
				Text = "To exit this screen, and go back to the previous screen, press 0 or Cancel."
			});
		};
		result.SpeechParts.Add(speechPart2);
	}

	private static void AddBaseEvents(ApplyPromoViewModel model, ISpeechPart speechPart)
	{
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
				model.EnterPromoCommand.Execute(null);
			}
		});
	}
}
