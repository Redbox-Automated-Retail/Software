using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;
using Redbox.KioskEngine.Environment.TextToSpeech;
using Redbox.Rental.UI.ControllersLogic;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.Services.Controllers;

public class MemberPerksTTSController : BaseTTSController<MemberPerksTTSController, MemberPerksLogic>
{
	private class TTSConstants
	{
		public const string PerksMenu = "PerksMenu";

		public const string PerksMember = "PerksMember";

		public const string PerksPromo = "PerksPromo";
	}

	public static void AddSpeechParts(ISpeechControl speechControl, MemberPerksLogic logic)
	{
		AddSpeechPartForPerks(speechControl, logic);
		AddJumpToCurrentPerksSection(speechControl, logic);
		AddSpeechPartPerksMember(speechControl, logic);
		AddSpeechPartPerksPromo(speechControl, logic);
	}

	private static void AddBaseEvents(ISpeechPart speechPart, MemberPerksLogic logic)
	{
		speechPart.Events.Add(new MapKeyPressToAction
		{
			KeyCode = "0",
			Action = delegate
			{
				logic.CancelButtonCommand_Execute();
			}
		});
		speechPart.Events.Add(new MapKeyPressToAction
		{
			KeyCode = "ENTER",
			Action = delegate
			{
				logic.CancelButtonCommand_Execute();
			}
		});
		if (logic.IsPerks)
		{
			speechPart.Events.Add(new MapKeyPressToAction
			{
				KeyCode = "1",
				Action = delegate
				{
					logic.PerksButtonCommand_Execute();
				},
				Function = "TTSSkipToSpeechPart",
				Value = "PerksMember"
			});
		}
		if (logic.IsPromo)
		{
			speechPart.Events.Add(new MapKeyPressToAction
			{
				KeyCode = "2",
				Action = delegate
				{
					logic.PromoButtonCommand_Execute();
				},
				Function = "TTSSkipToSpeechPart",
				Value = "PerksPromo"
			});
		}
		if (logic.IsPerks)
		{
			speechPart.Events.Add(new MapKeyPressToAction
			{
				KeyCode = "3",
				Action = delegate
				{
					logic.Model.InfoButtonCommand?.Execute(null);
				}
			});
		}
		if (logic.IsPerks)
		{
			speechPart.Events.Add(new MapKeyPressToAction
			{
				KeyCode = "4",
				Action = delegate
				{
					logic.Model.HowPointsWorkButtonCommand?.Execute(null);
				}
			});
		}
	}

	private static void AddJumpToCurrentPerksSection(ISpeechControl speechControl, MemberPerksLogic logic)
	{
		ISpeechPart speechPart = new SpeechPart
		{
			Sequence = 1,
			Name = "PerksMenu",
			StartPause = 500
		};
		speechPart.Refresh = delegate
		{
			string text = (logic.Model.PerksSelected ? "PerksMember" : "PerksPromo");
			speechPart.RegularExpression = new RegularExpression
			{
				Expression = "Jump",
				Value = "Jump",
				Success = text,
				Failure = text
			};
		};
		speechControl.SpeechParts.Add(speechPart);
	}

	private static void AddSpeechPartForPerks(ISpeechControl speechControl, MemberPerksLogic logic)
	{
		ISpeechPart speechPart = new SpeechPart
		{
			Sequence = 0,
			Name = "PerksMenu",
			StartPause = 500
		};
		speechPart.Refresh = delegate
		{
			speechPart.Clear();
			speechPart.Texts.Add(new TextPart
			{
				Text = "Welcome to My Redbox"
			});
			speechPart.Texts.Add(new TextPart
			{
				Text = "To exit My Redbox, and go back to the previous screen, press 0 or Enter."
			});
			AddBaseEvents(speechPart, logic);
			if (logic.IsPerks)
			{
				speechPart.Texts.Add(new TextPart
				{
					Text = "To access Perks member information, press 1."
				});
			}
			if (logic.IsPromo)
			{
				speechPart.Texts.Add(new TextPart
				{
					Text = "To access Perks offer information, press 2."
				});
			}
			if (logic.IsPerks)
			{
				speechPart.Texts.Add(new TextPart
				{
					Text = "To get more information about your perks level, press 3."
				});
			}
			if (logic.IsPerks)
			{
				speechPart.Texts.Add(new TextPart
				{
					Text = "To get more information about how points work, press 4."
				});
			}
		};
		speechControl.SpeechParts.Add(speechPart);
	}

	private static void AddSpeechPartPerksMember(ISpeechControl speechControl, MemberPerksLogic logic)
	{
		MemberPerksModel Model = logic.Model;
		string perksTitle = $"You are a {Model.PerksLevel}";
		string perksBullet = string.Format(Model.PerksLevelMessage);
		ISpeechPart speechPart = new SpeechPart
		{
			Sequence = 1,
			Name = "PerksMember",
			StartPause = 500
		};
		speechPart.Refresh = delegate
		{
			speechPart.Clear();
			speechPart.Texts.Add(new TextPart
			{
				Text = "You are viewing your perks."
			});
			speechPart.Texts.Add(new TextPart
			{
				Text = perksTitle
			});
			speechPart.Texts.Add(new TextPart
			{
				Text = perksBullet
			});
			AddBaseEvents(speechPart, logic);
			speechPart.Texts.Add(new TextPart
			{
				Text = $"You have {Model.SnapshotPoints} points."
			});
			speechPart.Texts.Add(new TextPart
			{
				Text = Model.EarnedRewardsMessage
			});
			speechPart.Texts.Add(new TextPart
			{
				Text = (string.IsNullOrWhiteSpace(Model.SnapshotExpire) ? "" : Model.SnapshotExpire)
			});
		};
		speechControl.SpeechParts.Add(speechPart);
	}

	private static void AddSpeechPartPerksPromo(ISpeechControl speechControl, MemberPerksLogic logic)
	{
		ITextToSpeechService ttsService = ServiceLocator.Instance.GetService<ITextToSpeechService>();
		ISpeechPart speechPart = new SpeechPart
		{
			Sequence = 1,
			Name = "PerksPromo",
			StartPause = 500,
			EndPause = 5000
		};
		speechPart.Refresh = delegate
		{
			speechPart.Clear();
			speechPart.Texts.Add(new TextPart
			{
				Text = "You are viewing your offers."
			});
			AddBaseEvents(speechPart, logic);
			MemberPerksModel model = logic.Model;
			if (model.ShowPromoListMessage)
			{
				speechPart.Texts.Add(new TextPart
				{
					Text = model.PromoListMessage
				});
			}
			if (model.StoredPromoCodes.Count == 0)
			{
				speechPart.Texts.Add(new TextPart
				{
					Text = "Looking pretty empty in here. Keep an eye out for your next promo."
				});
			}
			else
			{
				StoredPromoListTTSFunctions.AddPromoListTTS(logic.PromoLogic, ttsService, speechPart, model);
			}
		};
		speechControl.SpeechParts.Add(speechPart);
	}
}
