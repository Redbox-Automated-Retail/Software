using System;
using System.Collections.ObjectModel;
using System.Windows;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.UI;
using Redbox.Rental.UI.ControllersLogic;
using Redbox.Rental.UI.Models;
using Redbox.Rental.UI.Properties;
using Redbox.Rental.UI.Views;

namespace Redbox.Rental.Services.Controllers;

public class MemberPerksPopupViewController : BaseViewController
{
	public static void ConfigureView(PerksPopupInfo perksInfo)
	{
		MemberPerksPopupViewModel memberPerksPopupViewModel = null;
		FrameworkElement viewUserControl = BaseViewController.GetViewUserControl();
		if (viewUserControl != null)
		{
			BaseViewController.StopIdleTimer();
			BaseViewController.SetCurrentUICulture();
			memberPerksPopupViewModel = BaseViewController.GetViewModel<MemberPerksPopupViewModel>();
			if (memberPerksPopupViewModel == null)
			{
				memberPerksPopupViewModel = new MemberPerksPopupViewModel();
			}
			PerksTiers perksTiers = new PerksTiers
			{
				new PerksTier
				{
					Title = Resources.perks_tier_title0,
					RangeLimit = 0u
				},
				new PerksTier
				{
					Title = Resources.perks_tier_title1,
					RangeLimit = 10u
				},
				new PerksTier
				{
					Title = Resources.perks_tier_title2,
					RangeLimit = 20u
				},
				new PerksTier
				{
					Title = Resources.perks_tier_title3,
					RangeLimit = 50u
				}
			};
			switch (perksInfo.TierName?.ToLower())
			{
			default:
				memberPerksPopupViewModel.MemberTier = TierType.Member;
				memberPerksPopupViewModel.Title = Resources.perks_popup_level0_title;
				memberPerksPopupViewModel.BulletPoints = new ObservableCollection<string>
				{
					Resources.perks_level0_message1,
					Resources.perks_level0_message2,
					Resources.perks_level0_message3,
					Resources.perks_level0_message4
				};
				break;
			case "star":
				memberPerksPopupViewModel.MemberTier = TierType.Star;
				memberPerksPopupViewModel.Title = string.Format(Resources.perks_popup_title, Resources.perks_tier_title1);
				memberPerksPopupViewModel.BulletPoints = new ObservableCollection<string>
				{
					Resources.perks_level1_message1,
					Resources.perks_level1_message2,
					Resources.perks_level1_message3,
					Resources.perks_level1_message4
				};
				break;
			case "superstar":
				memberPerksPopupViewModel.MemberTier = TierType.Superstar;
				memberPerksPopupViewModel.Title = string.Format(Resources.perks_popup_title, Resources.perks_tier_title2);
				memberPerksPopupViewModel.BulletPoints = new ObservableCollection<string>
				{
					Resources.perks_level2_message1,
					Resources.perks_level2_message2,
					Resources.perks_level2_message3,
					Resources.perks_level2_message4
				};
				break;
			case "legend":
				memberPerksPopupViewModel.MemberTier = TierType.Legend;
				memberPerksPopupViewModel.Title = string.Format(Resources.perks_popup_title, Resources.perks_tier_title3);
				memberPerksPopupViewModel.BulletPoints = new ObservableCollection<string>
				{
					Resources.perks_level3_message1,
					Resources.perks_level3_message2,
					Resources.perks_level3_message3,
					Resources.perks_level3_message4
				};
				break;
			}
			int memberTier = (int)memberPerksPopupViewModel.MemberTier;
			int tierIndex = GetTierIndex(perksInfo.YearRentals);
			bool num = tierIndex < memberTier;
			tierIndex = (num ? memberTier : tierIndex);
			int nextLevelIndex = GetNextLevelIndex(tierIndex, perksInfo.YearRentals);
			if (num)
			{
				uint num2 = perksTiers[tierIndex].RangeLimit - perksInfo.YearRentals;
				memberPerksPopupViewModel.NextLevelRentals = string.Format(Resources.perks_maintain_message, perksTiers[tierIndex].Title, num2, DateTime.Now.Year);
			}
			else if (memberPerksPopupViewModel.MemberTier == TierType.Legend)
			{
				memberPerksPopupViewModel.NextLevelRentals = Resources.perks_snapshot_on_top;
			}
			else
			{
				uint num3 = perksTiers[nextLevelIndex].RangeLimit - perksInfo.YearRentals;
				memberPerksPopupViewModel.NextLevelRentals = string.Format(Resources.perks_next_level, num3, DateTime.Now.Year, perksTiers[nextLevelIndex].Title);
			}
			memberPerksPopupViewModel.CancelButtonCommand = DynamicRouting.RegisterRoutedCommand(new DynamicRoutedCommand(), (Action)delegate
			{
				OrchestrationService.ViewGoBackSafe("Member Perks popup Back", popOnly: false);
			}, (Func<bool>)null);
			memberPerksPopupViewModel.CancelButtonText = Resources.back_button;
			viewUserControl.DataContext = memberPerksPopupViewModel;
			ConfigureTTS(memberPerksPopupViewModel);
			BaseViewController.StartIdleTimer();
		}
	}

	private static int GetNextLevelIndex(int index, uint yearRentals)
	{
		return Math.Min(index + 1, 3);
	}

	private static int GetTierIndex(uint rentals)
	{
		switch (rentals)
		{
		case 0u:
		case 1u:
		case 2u:
		case 3u:
		case 4u:
		case 5u:
		case 6u:
		case 7u:
		case 8u:
		case 9u:
			return 0;
		case 10u:
		case 11u:
		case 12u:
		case 13u:
		case 14u:
		case 15u:
		case 16u:
		case 17u:
		case 18u:
		case 19u:
			return 1;
		case 20u:
		case 21u:
		case 22u:
		case 23u:
		case 24u:
		case 25u:
		case 26u:
		case 27u:
		case 28u:
		case 29u:
		case 30u:
		case 31u:
		case 32u:
		case 33u:
		case 34u:
		case 35u:
		case 36u:
		case 37u:
		case 38u:
		case 39u:
		case 40u:
		case 41u:
		case 42u:
		case 43u:
		case 44u:
		case 45u:
		case 46u:
		case 47u:
		case 48u:
		case 49u:
			return 2;
		default:
			return 3;
		}
	}

	public static void ClosingView(PerksPopupInfo perksInfo)
	{
	}

	private static void ConfigureTTS(MemberPerksPopupViewModel PerksModel)
	{
		ITextToSpeechService service = ServiceLocator.Instance.GetService<ITextToSpeechService>();
		FrameworkElement viewUserControl = BaseViewController.GetViewUserControl();
		if (service != null && service.TTSEnabled && viewUserControl != null)
		{
			PerksModel.ProcessGetSpeechControl = () => MemberPerksPopupTTSController.GetSpeechControl(BaseViewController.GetViewModel<MemberPerksPopupViewModel>);
		}
	}
}
