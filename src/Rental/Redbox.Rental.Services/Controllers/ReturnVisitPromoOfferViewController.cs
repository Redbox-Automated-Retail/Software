using System;
using System.Windows;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.KioskServices;
using Redbox.Rental.Model;
using Redbox.Rental.Model.KioskClientService.Transactions;
using Redbox.Rental.Model.Session;
using Redbox.Rental.Model.ShoppingCart;
using Redbox.Rental.Services.Promotion;
using Redbox.Rental.Services.Session;
using Redbox.Rental.UI.Models;
using Redbox.Rental.UI.Properties;
using Redbox.Rental.UI.Views;

namespace Redbox.Rental.Services.Controllers;

public class ReturnVisitPromoOfferViewController : BaseViewController
{
	private static string _promoCode;

	private static ReturnPromoType _promoType;

	private static decimal? _amount;

	private static Action _closeAction;

	public static void ConfigureReturnVisitPromoOfferView(ReturnVisitPromoOfferViewParameters returnVisitPromoOfferViewParameters)
	{
		BaseViewController.StopIdleTimer();
		CreateAndStartTimer();
		FrameworkElement viewUserControl = BaseViewController.GetViewUserControl();
		if (viewUserControl != null)
		{
			ReturnVisitPromoOfferViewModel returnVisitPromoOfferViewModel = new ReturnVisitPromoOfferViewModel();
			_promoType = returnVisitPromoOfferViewParameters.PromoType;
			returnVisitPromoOfferViewModel.TitleText = Resources.return_visit_promo_offer_title_text;
			returnVisitPromoOfferViewModel.AmountText = ((returnVisitPromoOfferViewParameters.PromoType == ReturnPromoType.FirstNightFree) ? Resources.return_visit_promo_offer_free_amount_text : string.Format(Resources.return_visit_promo_offer_amount_text, returnVisitPromoOfferViewParameters.Amount));
			_amount = returnVisitPromoOfferViewParameters.Amount;
			returnVisitPromoOfferViewModel.MessageText = ((returnVisitPromoOfferViewParameters.PromoType == ReturnPromoType.FirstNightFree) ? Resources.return_visit_promo_offer_free_message_text : Resources.return_visit_promo_offer_message_text);
			returnVisitPromoOfferViewModel.EligibleText = ((returnVisitPromoOfferViewParameters.PromoType == ReturnPromoType.FirstNightFree) ? Resources.return_visit_promo_offer_free_eligible_text : string.Empty);
			string text = string.Format(Resources.return_visit_promo_offer_promo_code_text, returnVisitPromoOfferViewParameters.PromoCode);
			returnVisitPromoOfferViewModel.PromoCodeText = string.Empty;
			_promoCode = returnVisitPromoOfferViewParameters.PromoCode;
			string text2 = string.Format(Resources.return_visit_promo_offer_expiration_text, returnVisitPromoOfferViewParameters.ExpirationDate);
			returnVisitPromoOfferViewModel.ExpirationText = string.Format(text + " - " + text2);
			returnVisitPromoOfferViewModel.LegalText = ((returnVisitPromoOfferViewParameters.PromoType == ReturnPromoType.FirstNightFree) ? Resources.return_visit_promo_offer_first_night_free_legal_text : Resources.return_visit_promo_offer_legal_text);
			returnVisitPromoOfferViewModel.CloseButtonText = Resources.close_button.ToUpper();
			returnVisitPromoOfferViewModel.UseNowButtonText = Resources.use_now_button_text.ToUpper();
			_closeAction = returnVisitPromoOfferViewParameters.CloseAction;
			returnVisitPromoOfferViewModel.OnCloseButtonClicked = (Action)Delegate.Combine(returnVisitPromoOfferViewModel.OnCloseButtonClicked, new Action(ReturnVisitPromoOfferViewModel_OnCloseButtonClicked));
			returnVisitPromoOfferViewModel.OnUseNowButtonClicked = (Action)Delegate.Combine(returnVisitPromoOfferViewModel.OnUseNowButtonClicked, new Action(ReturnVisitPromoOfferViewModel_OnUseNowButtonClicked));
			viewUserControl.DataContext = returnVisitPromoOfferViewModel;
		}
	}

	public static void LeaveReturnVisitPromoOfferView()
	{
		StopAndRemoveTimer();
		BaseViewController.StartIdleTimer();
	}

	private static void CreateAndStartTimer()
	{
		ITimerService service = ServiceLocator.Instance.GetService<ITimerService>();
		int timeOut = OrchestrationService.TTSGetTimeout("long", 40000);
		(service?.CreateTimer("ReturnVisitPromoOfferViewTimer", timeOut, timeOut, delegate
		{
			BaseViewController.AnalyticsService?.AddTimeoutEvent(timeOut);
			_closeAction?.Invoke();
		}))?.Start();
	}

	private static void StopAndRemoveTimer()
	{
		ITimerService service = ServiceLocator.Instance.GetService<ITimerService>();
		(service?.GetTimer("ReturnVisitPromoOfferViewTimer"))?.Stop();
		service?.RemoveTimer("ReturnVisitPromoOfferViewTimer");
	}

	private static void ReturnVisitPromoOfferViewModel_OnCloseButtonClicked()
	{
		BaseViewController.AnalyticsService?.AddButtonPressEvent("ReturnVisitPromoOfferClose");
		_closeAction?.Invoke();
	}

	private static void ReturnVisitPromoOfferViewModel_OnUseNowButtonClicked()
	{
		BaseViewController.AnalyticsService?.AddButtonPressEvent("ReturnVisitPromoOfferUseNow");
		_closeAction?.Invoke();
		IPromotion promotion = CreatePromotionFromReturnVisitPromo(_promoCode, _promoType, _amount);
		if (promotion != null)
		{
			OrchestrationService.StartUserInteraction(SessionStartTriggerActionList.Instance.FindByName(SessionStartTriggerActionConstants.ShowBrowseView));
			Redbox.Rental.Services.Session.Session.Instance?.BrowseState?.SetDefaultBrowseStateSet();
			OrchestrationService.ViewShow("browse_view", BrowseFunctions.GetBrowseViewparameters());
			PromotionService.Instance?.SetPromotion(promotion);
			IRentalShoppingCartService service = ServiceLocator.Instance.GetService<IRentalShoppingCartService>();
			service?.CurrentRentalShoppingCart?.ValidateAndUpdatePromoCode();
			service?.CurrentRentalShoppingCart?.UpdateServiceFee();
			service?.CurrentRentalShoppingCart?.ClearLoyaltyPointsToBeEarned();
		}
	}

	private static IPromotion CreatePromotionFromReturnVisitPromo(string promoCode, ReturnPromoType promoType, decimal? amount)
	{
		if (!string.IsNullOrEmpty(promoCode))
		{
			if (promoType == ReturnPromoType.AmountOff)
			{
				decimal? num = amount;
				if ((num.GetValueOrDefault() > 0m) & num.HasValue)
				{
					goto IL_0036;
				}
			}
			if (promoType == ReturnPromoType.FirstNightFree)
			{
				goto IL_0036;
			}
		}
		return null;
		IL_0036:
		return new Redbox.Rental.Services.Promotion.Promotion
		{
			Name = promoCode,
			OriginalPrice = ((promoType == ReturnPromoType.FirstNightFree) ? 0m : amount.GetValueOrDefault()),
			Price = ((promoType == ReturnPromoType.FirstNightFree) ? 0m : amount.GetValueOrDefault()),
			Quantity = 1,
			Intent = ((promoType == ReturnPromoType.FirstNightFree) ? PromotionIntentCode.FirstNightFreeItem : PromotionIntentCode.FixedValue),
			OriginalPercent = 0m,
			Percent = 0m,
			RentQuantity = 0,
			RentFormat = null,
			GetQuantity = 0,
			GetFormat = null,
			CampaignTitles = null,
			ProductTypeId = null,
			FormatIds = null,
			ActionCode = PromotionActionCode.Rental,
			AllowFullDiscount = (promoType != ReturnPromoType.FirstNightFree),
			DefaultPromo = null,
			CampaignId = 0,
			IsReturnVisit = true
		};
	}
}
