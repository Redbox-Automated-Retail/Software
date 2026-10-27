using System;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;
using Redbox.KioskEngine.Environment.TextToSpeech;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.Services.Controllers;

public class MemberPerksPopupTTSController
{
	public static ISpeechControl GetSpeechControl(Func<MemberPerksPopupViewModel> getviewModelFunction)
	{
		ISpeechControl result = new SpeechControl();
		result.Name = "tts_member_perks_popup_view";
		AddSpeechPart(result, getviewModelFunction);
		result.SpeechParts.ForEach(delegate(ISpeechPart x)
		{
			x.SpeechControl = result;
		});
		return result;
	}

	private static void AddSpeechPart(ISpeechControl speechControl, Func<MemberPerksPopupViewModel> getViewModelFunction)
	{
		ISpeechPart speechPart = new SpeechPart
		{
			Name = "Info",
			Sequence = 1,
			KeySequenceDelay = 0,
			Loop = true,
			StartPause = 500,
			EndPause = 5000
		};
		speechPart.Refresh = delegate
		{
			speechPart.Clear();
			MemberPerksPopupViewModel viewModel = getViewModelFunction();
			if (viewModel != null)
			{
				speechPart.Events.Add(new MapKeyPressToAction
				{
					KeyCode = "CANCEL",
					Action = delegate
					{
						viewModel.CancelButtonCommand?.Execute(null);
					}
				});
				speechPart.Texts.Add(new TextPart
				{
					Text = viewModel.Title
				});
				foreach (string bulletPoint in viewModel.BulletPoints)
				{
					speechPart.Texts.Add(new TextPart
					{
						Text = bulletPoint
					});
				}
				speechPart.Texts.Add(new TextPart
				{
					Text = viewModel.NextLevelRentals
				});
				speechPart.Texts.Add(new TextPart
				{
					Text = "To go back, press cancel."
				});
			}
		};
		speechControl.SpeechParts.Add(speechPart);
	}
}
