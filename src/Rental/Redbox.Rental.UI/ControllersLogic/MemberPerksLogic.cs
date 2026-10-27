using System;
using System.Collections.Generic;
using System.Windows;
using Redbox.Core;
using Redbox.Rental.Model.ShoppingCart;
using Redbox.Rental.UI.Models;
using Redbox.Rental.UI.Properties;
using Redbox.Rental.UI.Views;

namespace Redbox.Rental.UI.ControllersLogic;

public class MemberPerksLogic : BaseLogic
{
	private const string ImagePath = "pack://Application:,,,/Redbox.Rental.UI;component/Assets/Images/";

	private const string perks_hero = "perks-star-white-icon.png";

	private const string perks_superhero = "perks-superstar-white-icon.png";

	private const string perks_legend = "perks-legend-white-icon.png";

	public bool Instructions { get; set; }

	public Action CloseAction { get; set; }

	public MemberPerksModel Model { get; private set; }

	public PromoListLogic PromoLogic { get; private set; }

	private static List<string> ImageNames => new List<string> { null, "perks-star-white-icon.png", "perks-superstar-white-icon.png", "perks-legend-white-icon.png" };

	protected static IRentalShoppingCartService RentalShoppingCartService => ServiceLocator.Instance.GetService<IRentalShoppingCartService>();

	public bool IsRedboxPlus => Model.RedboxPlusButtonVisibility == Visibility.Visible;

	public bool IsPerks => Model.PerksButtonVisibility == Visibility.Visible;

	public bool IsPromo => Model.PromoButtonVisibility == Visibility.Visible;

	protected void PopulatePerks(PerksMemberInfo memberInfo)
	{
		string[] obj = new string[4]
		{
			Resources.perks_tier_title0,
			Resources.perks_tier_title1,
			Resources.perks_tier_title2,
			Resources.perks_tier_title3
		};
		double rentals = memberInfo.YearRentals;
		Model.SnapshotPoints = $"{memberInfo.Points:n0}";
		Model.SnapshotPointsLabel = Resources.perks_snapshot_points;
		Model.SnapshotExpire = ((memberInfo.ExpirePoints != 0) ? string.Format(Resources.perks_snapshot_expire, memberInfo.ExpirePoints, memberInfo.ExpireDate.Value.ToString("MM/dd/yyyy")) : "");
		int memberTier = (int)Model.MemberTier;
		int perksTierIndex = GetPerksTierIndex(rentals);
		perksTierIndex = ((perksTierIndex < memberTier) ? memberTier : perksTierIndex);
		SetSimpleNextLevel(perksTierIndex, rentals);
		string perksLevel = obj[perksTierIndex];
		bool flag = false;
		switch (perksTierIndex)
		{
		default:
			flag = true;
			Model.MemberTier = TierType.Member;
			Model.PerksLevelMessage = Resources.perks_level0_message;
			break;
		case 1:
			Model.MemberTier = TierType.Star;
			Model.PerksLevelMessage = Resources.perks_level1_message;
			break;
		case 2:
			Model.MemberTier = TierType.Superstar;
			Model.PerksLevelMessage = Resources.perks_level2_message;
			break;
		case 3:
			Model.MemberTier = TierType.Legend;
			Model.PerksLevelMessage = Resources.perks_level3_message;
			break;
		}
		Model.PerksLevel = perksLevel;
		Model.PerksImagePath = (flag ? null : ("pack://Application:,,,/Redbox.Rental.UI;component/Assets/Images/" + ImageNames[perksTierIndex]));
		Model.EarnedRewardsMessage = GetEarnedRewardsMessage(memberInfo.Points);
		Model.PerksMarketing = Resources.perks_marketing;
		Model.PerksImageVisibility = (flag ? Visibility.Collapsed : Visibility.Visible);
		Model.PerksTitleVisibility = ((!flag) ? Visibility.Collapsed : Visibility.Visible);
		Model.PromoTitle = Resources.perks_promo_title;
	}

	private string GetEarnedRewardsMessage(uint points)
	{
		if (points < 1000)
		{
			return Resources.perks_earned_level0_message;
		}
		if (points < 1500)
		{
			return string.Format(Resources.perks_earned_level1_message, 1500 - points);
		}
		if (points < 3000)
		{
			return Resources.perks_earned_level2_message;
		}
		if (points < 4500)
		{
			return Resources.perks_earned_level3_message;
		}
		if (points < 6000)
		{
			return Resources.perks_earned_level4_message;
		}
		if (points < 7500)
		{
			return Resources.perks_earned_level5_message;
		}
		return Resources.perks_earned_level6_message;
	}

	private void SetSimpleNextLevel(int index, double rentals)
	{
		double num;
		do
		{
			num = index switch
			{
				1 => 20.0, 
				2 => 50.0, 
				3 => -1.0, 
				_ => 10.0, 
			};
		}
		while (num >= 0.0 && num - rentals < 0.0);
	}

	private int GetPerksTierIndex(double rentals)
	{
		if (rentals < 10.0)
		{
			return 0;
		}
		if (rentals < 20.0)
		{
			return 1;
		}
		if (rentals < 50.0)
		{
			return 2;
		}
		return 3;
	}

	public void RedboxPlusButtonCommand_Execute()
	{
		Model.RedboxPlusSelected = true;
		Model.PerksSelected = false;
		Model.PromosSelected = false;
		BaseLogic.HandleWPFHit();
		BaseLogic.AnalyticsService?.AddButtonPressEvent("MemberPerksLogicRedboxPlus");
	}

	public void PerksButtonCommand_Execute()
	{
		Model.RedboxPlusSelected = false;
		Model.PerksSelected = true;
		Model.PromosSelected = false;
		BaseLogic.HandleWPFHit();
		BaseLogic.AnalyticsService?.AddButtonPressEvent("MemberPerksLogicPerks");
	}

	public void PromoButtonCommand_Execute()
	{
		Model.RedboxPlusSelected = false;
		Model.PerksSelected = false;
		Model.PromosSelected = true;
		BaseLogic.HandleWPFHit();
		BaseLogic.AnalyticsService?.AddButtonPressEvent("MemberPerksLogicPromo");
	}

	public void CancelButtonCommand_Execute()
	{
		CloseAction?.Invoke();
		BaseLogic.HandleWPFHit();
		BaseLogic.AnalyticsService?.AddButtonPressEvent("MemberPerksLogicBack");
	}

	public void SelectedButtonCommand_Execute(string buttonValue)
	{
		BaseLogic.AnalyticsService?.AddButtonPressEvent("MemberPerksLogicButtonValue", buttonValue);
	}

	public MemberPerksLogic(MemberPerksModel perksModel, PerksMemberInfo memberInfo, PromoListLogic promoLogic)
	{
		Instructions = true;
		CloseAction = memberInfo.CancelAction;
		PromoLogic = promoLogic;
		Model = perksModel;
		Model.CancelButtonCommand = DynamicRouting.RegisterRoutedCommand(Commands.CancelButtonCommand, CancelButtonCommand_Execute);
		Model.SelectedButtonCommand = DynamicRouting.RegisterRoutedCommand(Commands.SelectedButtonCommand, SelectedButtonCommand_Execute);
		Model.NextPagePressedCommand = DynamicRouting.RegisterRoutedCommand(new DynamicRoutedCommand(), PromoLogic.AddNextPagePressedAnalytics);
		Model.PreviousPagePressedCommand = DynamicRouting.RegisterRoutedCommand(new DynamicRoutedCommand(), PromoLogic.AddPreviousPagePressedAnalytics);
		Model.PageNumberPressedCommand = DynamicRouting.RegisterRoutedCommand(new DynamicRoutedCommand(), PromoLogic.AddPageNumberPressedAnalytics);
		PopulatePerks(memberInfo);
	}
}
