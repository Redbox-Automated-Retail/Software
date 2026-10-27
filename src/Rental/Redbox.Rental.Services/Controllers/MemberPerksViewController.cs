using System;
using System.Collections.Generic;
using System.Windows;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model.KioskClientService.Subscriptions;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Model.Personalization;
using Redbox.Rental.Model.Session;
using Redbox.Rental.Services.Configuration;
using Redbox.Rental.Services.Session;
using Redbox.Rental.UI;
using Redbox.Rental.UI.ControllersLogic;
using Redbox.Rental.UI.Models;
using Redbox.Rental.UI.Properties;
using Redbox.Rental.UI.Views;

namespace Redbox.Rental.Services.Controllers;

public class MemberPerksViewController : BaseViewController
{
	private static bool EnableRedboxPlusRedemption => Redbox.Rental.Services.Configuration.Configuration.Instance.KioskSession.MarketingGeneral.EnableRedboxPlusRedemption;

	private static IRedboxPlusSession RedboxPlusSession => (Redbox.Rental.Services.Session.Session.Instance?.PersonalizationSession)?.RedboxPlusSession ?? null;

	private static MemberPerksLogic GetPerksLogic()
	{
		return BaseViewController.GetViewModel<MemberPerksModel>()?.Logic;
	}

	public static void ConfigureView(PerksMemberInfo perksInfo)
	{
		FrameworkElement viewUserControl = BaseViewController.GetViewUserControl();
		if (viewUserControl == null)
		{
			return;
		}
		BaseViewController.StopIdleTimer();
		BaseViewController.SetCurrentUICulture();
		MemberPerksModel Model;
		if (perksInfo.PerksModel != null && perksInfo.PerksLogic != null)
		{
			Model = perksInfo.PerksModel;
			MemberPerksLogic perksLogic = perksInfo.PerksLogic;
		}
		else
		{
			Model = new MemberPerksModel
			{
				HeaderText = Resources.perks_header_text,
				PerksButtonText = Resources.perks_perks_btn_text,
				PromosButtonText = Resources.perks_promos_btn_text,
				CancelButtonText = Resources.perks_cancel_btn_text.ToUpper(),
				EmptyPromoListMessage = Resources.perks_promo_empty_list_text,
				PromosPerPage = 6,
				IsAdaMode = BaseViewController.ApplicationState.IsADAAccessible
			};
			switch (perksInfo.TierName?.ToLower())
			{
			default:
				Model.MemberTier = TierType.Member;
				break;
			case "star":
				Model.MemberTier = TierType.Star;
				break;
			case "superstar":
				Model.MemberTier = TierType.Superstar;
				break;
			case "legend":
				Model.MemberTier = TierType.Legend;
				break;
			}
			MemberPerksLogic perksLogic = new MemberPerksLogic(Model, perksInfo, new PromoListLogic(Model));
			Model.Logic = perksLogic;
			Model.InfoButtonCommand = DynamicRouting.RegisterRoutedCommand(new DynamicRoutedCommand(), delegate(PerksPopupInfo info)
			{
				MoreInfoPopupCommand_Execute(info, perksInfo);
			});
			Model.InfoButtonText = Resources.perks_info_button_text;
			Model.HowPointsWorkButtonCommand = DynamicRouting.RegisterRoutedCommand(new DynamicRoutedCommand(), delegate(PerksPopupInfo info)
			{
				HowPointsWorkPopupCommand_Execute(info, perksInfo);
			});
			Model.HowPointsWorkButtonText = Resources.perks_how_points_work_button_text;
			Model.AddOrRemovePromoCommand = DynamicRouting.RegisterRoutedCommand(new DynamicRoutedCommand(), delegate(StoredPromoCodeModel obj)
			{
				AddOrRemovePromoCommand_Execute(obj, Model);
			});
			Model.DetailsCommand = DynamicRouting.RegisterRoutedCommand(new DynamicRoutedCommand(), delegate(PerksOfferListItemModel obj)
			{
				DetailsCommand_Execute(obj, Model);
			});
			UpdateStoredPromos(Model);
			SetRedboxPlusInfo(Model);
			perksInfo.PerksModel = Model;
			perksInfo.PerksLogic = perksLogic;
		}
		viewUserControl.DataContext = Model;
		ConfigureTTS(Model);
		BaseViewController.StartIdleTimer();
	}

	private static void MoreInfoPopupCommand_Execute(PerksPopupInfo info, PerksMemberInfo perksInfo)
	{
		OrchestrationService.ViewShow("member_perks_popup_view", new PerksPopupInfo
		{
			Points = perksInfo.Points,
			YearRentals = perksInfo.YearRentals,
			TierName = perksInfo.TierName
		});
	}

	private static void HowPointsWorkPopupCommand_Execute(PerksPopupInfo info, PerksMemberInfo perksInfo)
	{
		OrchestrationService.ViewShow("perks_signup_info_popup_view");
	}

	private static void UpdateStoredPromos(MemberPerksModel model)
	{
		IPersonalizationSession personalizationSession = Redbox.Rental.Services.Session.Session.Instance?.PersonalizationSession;
		string activePromoCode = string.Empty;
		if (BaseViewController.RentalShoppingCartService?.CurrentRentalShoppingCart != null)
		{
			activePromoCode = BaseViewController.RentalShoppingCartService.CurrentRentalShoppingCart.Promotion?.Name;
		}
		List<IPerksOfferListItem> list = PromoListLogic.CreateOfferListItemModels(model, personalizationSession, activePromoCode, '*', includeOffers: true, includeRedboxPlus: false);
		model.IsPromoCodeListVisible = list.Count > 0;
		model.IsEmptyPromoListMessageVisible = list.Count == 0;
		model.PromosPerPage = 6;
		model.CurrentPageNumber = PromoListLogic.FindPageOfSelectedPromo(model);
	}

	private static void AddOrRemovePromoCommand_Execute(StoredPromoCodeModel promoCodeModel, MemberPerksModel model)
	{
		promoCodeModel.Logic.AddRemovePromoButtonPressedAnalytics(promoCodeModel);
		BaseViewController.ProcessWPFHit();
		model.ShowPromoListMessage = false;
		if (!promoCodeModel.IsAdded)
		{
			ValidateStoredPromoWillNotConflictWithRedboxPlusBenefit(promoCodeModel, model);
		}
		else if (BaseViewController.RentalShoppingCartService?.CurrentRentalShoppingCart != null)
		{
			BaseViewController.RentalShoppingCartService?.CurrentRentalShoppingCart.ClearPromoCode();
			BaseViewController.RentalShoppingCartService?.CurrentRentalShoppingCart.ClearLoyaltyPointsToBeEarned();
			promoCodeModel.IsAdded = false;
			model.ShowPromoListMessage = true;
			model.PromoListMessage = string.Format(Resources.perks_promo_remove_message, promoCodeModel.MaskedPromoCode);
		}
	}

	private static void ValidateStoredPromoWillNotConflictWithRedboxPlusBenefit(StoredPromoCodeModel promoCodeModel, MemberPerksModel model)
	{
		RedboxPlusFunctions.ValidatePromoWillNotConflictWithRedboxPlusBenefit(delegate
		{
			OrchestrationService.ViewGoBackSafe("MemberPerksView DeclinedStoredPromo because of Redbox+ conflict.", popOnly: false);
		}, delegate
		{
			OrchestrationService.ViewGoBackSafe("MemberPerksView StoredPromoSelected", popOnly: false);
			ConfirmPromo(promoCodeModel, model);
		}, delegate
		{
			ConfirmPromo(promoCodeModel, model);
		});
	}

	private static void ConfirmPromo(StoredPromoCodeModel promoCodeModel, MemberPerksModel model)
	{
		PromoConfirmationInfo promoConfirmationInfo = new PromoConfirmationInfo();
		promoConfirmationInfo.promoCodeModel = promoCodeModel;
		promoConfirmationInfo.ApplyPromoCodeAction = delegate
		{
			if (((BaseViewController.RentalShoppingCartService?.CurrentRentalShoppingCart)?.Items?.Count).GetValueOrDefault() > 0)
			{
				ValidatePromoCode(promoCodeModel, model);
			}
			else
			{
				BaseViewController.RentalShoppingCartService?.CurrentRentalShoppingCart?.ClearPromoCode();
				SetSelectedPromo(promoCodeModel, model);
			}
		};
		promoConfirmationInfo.CancelAction = delegate
		{
		};
		OrchestrationService.ViewShow("promo_confirmation_view", promoConfirmationInfo);
	}

	private static void DetailsCommand_Execute(PerksOfferListItemModel perksOfferModel, MemberPerksModel model)
	{
		if (perksOfferModel.OfferStatus == "Active")
		{
			OrchestrationService.ViewShow("perks_offer_details_view", perksOfferModel);
		}
	}

	private static void ValidatePromoCode(StoredPromoCodeModel promoCodeModel, MemberPerksModel model)
	{
		LogHelper.Instance.Log("calling ValidatePromoCode from MemberPerksView.");
		IViewService viewService = ServiceLocator.Instance.GetService<IViewService>();
		PromotionFunctions.ValidatePromoCode(doneCallback: delegate
		{
			viewService.UnwindToSafe("member_perks_view", discard: false, "MemberPerksView:get_promo_code (on_complete)");
			SetSelectedPromo(promoCodeModel, model);
		}, afterErrorMessageCallback: delegate
		{
			viewService.UnwindToSafe("member_perks_view", discard: false, "MemberPerksView:get_promo_code (on_error)");
			if ((BaseViewController.RentalShoppingCartService?.CurrentRentalShoppingCart).Promotion == null)
			{
				ClearSelectedPromo(model);
			}
		}, promoCode: promoCodeModel.PromoCode);
	}

	private static void SetSelectedPromo(StoredPromoCodeModel promoCodeModel, MemberPerksModel model)
	{
		ClearSelectedPromo(model);
		promoCodeModel.IsAdded = true;
		model.ShowPromoListMessage = true;
		model.PromoListMessage = string.Format(Resources.perks_promo_add_message, promoCodeModel.MaskedPromoCode);
		ServiceLocator.Instance.GetService<IPersonalizationService>()?.SetSelectedPromo(promoCodeModel.PromoCode);
	}

	private static void ClearSelectedPromo(MemberPerksModel model)
	{
		foreach (IPerksOfferListItem storedPromoCode in model.StoredPromoCodes)
		{
			if (storedPromoCode is StoredPromoCodeModel)
			{
				((StoredPromoCodeModel)storedPromoCode).IsAdded = false;
			}
		}
		ServiceLocator.Instance.GetService<IPersonalizationService>()?.ClearSelectedPromo();
	}

	public static void ClosingView(PerksMemberInfo perksInfo)
	{
	}

	private static void ConfigureTTS(MemberPerksModel PerksModel)
	{
		ITextToSpeechService service = ServiceLocator.Instance.GetService<ITextToSpeechService>();
		FrameworkElement viewUserControl = BaseViewController.GetViewUserControl();
		if (service != null && service.TTSEnabled && viewUserControl != null)
		{
			PerksModel.ProcessGetSpeechControl = () => BaseTTSController<MemberPerksTTSController, MemberPerksLogic>.GetSpeechControl(GetPerksLogic);
		}
	}

	private static void SeeRedboxPlusMoviesButtonCommand_Execute()
	{
		BaseViewController.ProcessWPFHit();
		BaseViewController.AnalyticsService?.AddButtonPressEvent("MemberPerksViewSeeRedboxPlusMovies");
		Redbox.Rental.Services.Session.Session.Instance.BrowseState.BrowseMode = BrowseViewFilter.RedboxPlus;
		Redbox.Rental.Services.Session.Session.Instance.BrowseState.BrowseSelectedSort = BrowseSelectedSort.Newest;
		Redbox.Rental.Services.Session.Session.Instance.BrowseState.TitleType = TitleType.All;
		Redbox.Rental.Services.Session.Session.Instance.BrowseState.PageNumber = 1;
		OrchestrationService.ViewShow("browse_view", BrowseFunctions.GetBrowseViewparameters());
		BrowseFunctions.ShowRedboxPlusBrowseWarnings();
	}

	private static void TermsButtonCommand_Execute()
	{
		BaseViewController.ProcessWPFHit();
		BaseViewController.AnalyticsService?.AddButtonPressEvent("MemberPerksViewTerms");
		HelpViewController.ShowRedboxPlus();
	}

	private static void SetRedboxPlusInfo(MemberPerksModel model)
	{
		bool flag = EnableRedboxPlusRedemption && (RedboxPlusSession?.HasActiveSubscription ?? false);
		RedboxPlusSubscriptionTier redboxPlusSubscriptionTier = RedboxPlusSession?.RedboxPlusPlanInfo?.SubscriptionTier ?? RedboxPlusSubscriptionTier.Tier1;
		bool valueOrDefault = RedboxPlusSession?.RedboxPlusPlanInfo?.IsEarnPointsFor1ExtraNightAvailable == true;
		IRedboxPlusSession redboxPlusSession = RedboxPlusSession;
		string text = ((redboxPlusSession != null && (redboxPlusSession.RedboxPlusPlanInfo?.SubscriptionEndDate).HasValue) ? RedboxPlusSession.RedboxPlusPlanInfo.SubscriptionEndDate.Value.ToShortDateString() : null);
		if (flag)
		{
			model.PerksSelected = false;
			model.RedboxPlusSelected = true;
			model.PromosSelected = false;
		}
		model.TabButtonsColumns = (flag ? 3 : 2);
		model.RedboxPlusButtonVisibility = ((!flag) ? Visibility.Collapsed : Visibility.Visible);
		model.RedboxPlusButtonText = Resources.perks_redbox_plus_btn_text;
		model.RedboxPlusPlanId1Visibility = ((redboxPlusSubscriptionTier != RedboxPlusSubscriptionTier.Tier1) ? Visibility.Collapsed : Visibility.Visible);
		model.RedboxPlusPlanId2Visibility = ((redboxPlusSubscriptionTier != RedboxPlusSubscriptionTier.Tier2) ? Visibility.Collapsed : Visibility.Visible);
		model.RedboxPlusFreeMonthlyText = ((redboxPlusSubscriptionTier == RedboxPlusSubscriptionTier.Tier2) ? Resources.redbox_plus_free_1night_text : Resources.redbox_plus_free_monthly_text);
		model.RedboxPlusFreeThisMonthText = ((redboxPlusSubscriptionTier == RedboxPlusSubscriptionTier.Tier2) ? string.Format(Resources.redbox_plus_this_year_text, 24 - RedboxPlusFunctions.NumberOfFreeOneNightRentalsAvailable) : string.Format(Resources.redbox_plus_this_month_text, RedboxPlusFunctions.IsFreeOneNightRentalAvailable ? Resources.redbox_plus_ready_to_use_text : Resources.redbox_plus_used_text));
		model.RedboxPlusSeeMoviesText = Resources.redbox_plus_see_movies_text;
		model.RedboxPlusChooseText = Resources.redbox_plus_choose_text;
		model.RedboxPlusExtendedText = Resources.redbox_plus_extended_text;
		model.RedboxPlusKeepText = Resources.redbox_plus_keep_text;
		model.RedboxPlusExtraNightText = Resources.redbox_plus_extra_night_text;
		model.RedboxPlusExtraThisMonthText = string.Format(Resources.redbox_plus_this_month_text, valueOrDefault ? Resources.redbox_plus_ready_to_use_text : Resources.redbox_plus_used_text);
		model.RedboxPlusNeedAnotherText = Resources.redbox_plus_need_another_text;
		model.RedboxPlusAwardedText = Resources.redbox_plus_awarded_text;
		if ((RedboxPlusSession?.RedboxPlusPlanInfo?.SubscriptionStatus).GetValueOrDefault() == SubscriptionPlanStatus.Active)
		{
			model.RedboxPlusPlanRenewsText = ((text != null) ? string.Format((redboxPlusSubscriptionTier == RedboxPlusSubscriptionTier.Tier2) ? Resources.redbox_plus_tier2_plan_renews_text : Resources.redbox_plus_tier1_plan_renews_text, text) : string.Empty);
		}
		else
		{
			model.RedboxPlusPlanRenewsText = ((text != null) ? string.Format((redboxPlusSubscriptionTier == RedboxPlusSubscriptionTier.Tier2) ? Resources.redbox_plus_tier2_plan_ends_text : Resources.redbox_plus_tier1_plan_ends_text, text) : string.Empty);
		}
		model.RedboxPlusLegalText = ((redboxPlusSubscriptionTier == RedboxPlusSubscriptionTier.Tier2) ? Resources.redbox_plus_legal_text : string.Empty);
		model.RedboxPlusTermsText = Resources.redbox_plus_terms_text;
		model.SeeRedboxPlusMoviesButtonCommand = DynamicRouting.RegisterRoutedCommand(new DynamicRoutedCommand(), (Action)delegate
		{
			SeeRedboxPlusMoviesButtonCommand_Execute();
		}, (Func<bool>)null);
		model.TermsButtonCommand = DynamicRouting.RegisterRoutedCommand(new DynamicRoutedCommand(), (Action)delegate
		{
			TermsButtonCommand_Execute();
		}, (Func<bool>)null);
	}
}
