using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model;
using Redbox.Rental.Model.KioskClientService;
using Redbox.Rental.Services.Session;
using Redbox.Rental.UI.Controls;
using Redbox.Rental.UI.Models;
using Redbox.Rental.UI.Properties;
using Redbox.Rental.UI.Views;

namespace Redbox.Rental.Services.Controllers;

public class MessagePopupViewController : BaseViewController
{
	private enum ModalType
	{
		Informational,
		Dialog,
		Error
	}

	private class ModalParameters
	{
		public ModalType ModalType { get; set; }

		public string TitleText { get; set; }

		public string MessageText { get; set; }

		public string Button1Text { get; set; }

		public string Button2Text { get; set; }

		public int? TimeOut { get; set; }

		public Action Button1Action { get; set; }

		public Action Button2Action { get; set; }

		public Action TimeOutAction { get; set; }

		public string ActorTag { get; set; }

		public UserControl ContentUserControl { get; set; }

		public List<string> AnalyticsContextList { get; set; }

		public string ViewName { get; set; }

		public ModalParameters(ModalType modalType)
		{
			ModalType = modalType;
		}
	}

	private static void ShowModal(ModalParameters modalParameters)
	{
		LogHelper.Instance.Log("Showing Modal " + modalParameters?.TitleText);
		ShowMessagePopupView(new ShowMessagePopupViewParameter(modalParameters?.TitleText, (!string.IsNullOrWhiteSpace(modalParameters?.TitleText)) ? ((modalParameters != null && modalParameters.ModalType == ModalType.Error) ? "error_modal_title_title_style" : "modal_title_title_style") : ((modalParameters != null && modalParameters.ModalType == ModalType.Error) ? "error_modal_no_title_title_style" : "modal_no_title_title_style"), modalParameters?.MessageText, string.IsNullOrWhiteSpace(modalParameters?.MessageText) ? "modal_no_message_message_style" : "modal_message_message_style", modalParameters?.Button1Text, "button_text_large_style", (!string.IsNullOrWhiteSpace(modalParameters?.Button2Text)) ? "modal_two_buttons_button1_style" : "modal_one_button_button1_style", modalParameters?.Button2Text, "button_text_large_style", "modal_two_buttons_button2_style", "modal_button_grid_style", "modal_button_inner_grid_style", null, (modalParameters?.ContentUserControl != null) ? "modal_content_user_control_message_dock_panel_style" : "modal_no_content_user_control_message_dock_panel_style", (modalParameters != null && modalParameters.TimeOut.HasValue) ? modalParameters.TimeOut.Value : GetDefaultMessageTimeOut(), modalParameters?.Button1Action, modalParameters?.Button2Action, modalParameters?.TimeOutAction, modalParameters?.ActorTag, modalParameters?.ContentUserControl, "Transparent", new Thickness(0.0, 0.0, 0.0, 0.0), modalParameters?.AnalyticsContextList, modalParameters?.ViewName, Visibility.Visible, isAutoSized: true));
	}

	public static void ShowMessagePopupViewVendError(Action timeOutAction, Action continueAction)
	{
		LogHelper.Instance.Log("Showing Vend Error message popup");
		ShowCriticalErrorView(new ShowMessagePopupViewParameter(Resources.vend_error_title, "error_modal_title_title_style", Resources.vend_error_message, "modal_message_message_style", Resources.continue_button, "button_text_large_style", "modal_one_button_button1_style", null, "button_text_large_style", "modal_two_buttons_button2_style", "modal_button_grid_style", "modal_button_inner_grid_style", null, "modal_no_content_user_control_message_dock_panel_style", GetDefaultMessageTimeOut(), continueAction, null, timeOutAction, "ViewVendError", null, "Transparent", new Thickness(0.0, 0.0, 0.0, 0.0), null, null, Visibility.Visible, isAutoSized: true));
	}

	public static void ShowRecommendationOnPickupPrompt(Action timeOutAction, Action pickupAction, Action continueBrowsingAction)
	{
		LogHelper.Instance.Log("Showing Recommendation On Pickup prompt");
		ShowModal(new ModalParameters(ModalType.Dialog)
		{
			TitleText = Resources.reco_need_more_time,
			Button1Text = Resources.pickup.ToUpper(),
			Button2Text = Resources.keep_browsing_1.ToUpper() + " " + Resources.keep_browsing_2.ToUpper(),
			TimeOut = GetDefaultMessageTimeOut(),
			Button1Action = pickupAction,
			Button2Action = continueBrowsingAction,
			TimeOutAction = timeOutAction,
			ActorTag = "RecommendationOnPickupPrompt"
		});
	}

	public static void ShowMyPointsError(Action continueAction, Action timeOutAction)
	{
		LogHelper.Instance.Log("Showing My Points Error");
		ShowModal(new ModalParameters(ModalType.Informational)
		{
			TitleText = Resources.my_points_error_title,
			MessageText = Resources.my_points_error_message,
			Button1Text = Resources.my_points_error_btn_text.ToUpper(),
			TimeOut = GetDefaultMessageTimeOut(30000, "long"),
			Button1Action = continueAction,
			TimeOutAction = timeOutAction,
			ActorTag = "MyPointsError"
		});
	}

	public static void ShowNotEnoughPointsError(Action continueAction, Action timeOutAction)
	{
		LogHelper.Instance.Log("Showing Not Enough Points Error");
		ShowModal(new ModalParameters(ModalType.Informational)
		{
			TitleText = Resources.sorry,
			MessageText = Resources.not_enough_points,
			Button1Text = Resources.okay.ToUpper(),
			TimeOut = GetDefaultMessageTimeOut(30000, "long"),
			Button1Action = continueAction,
			TimeOutAction = timeOutAction,
			ActorTag = "NotEnoughPointsError"
		});
	}

	public static void ShowRelatedTitlesNotFoundMessage(Action continueAction, Action timeOutAction)
	{
		LogHelper.Instance.Log("Showing Simple Message: {0}", Resources.related_movies_popup_header);
		ShowModal(new ModalParameters(ModalType.Informational)
		{
			TitleText = Resources.related_movies_popup_header,
			MessageText = Resources.related_movies_not_found_message,
			Button1Text = Resources.got_it_button.ToUpper(),
			TimeOut = GetDefaultMessageTimeOut(30000, "long"),
			Button1Action = continueAction,
			TimeOutAction = timeOutAction,
			ActorTag = "RelatedTitlesNotFoundMessage"
		});
	}

	public static void ShowAuthErrorMessage(string cultureSpecificTitle, string cultureSpecificMessage, string cultureSpecificButtonText, Action continueAction, Action timeOutAction, List<string> analyticsContexts = null)
	{
		ShowModal(new ModalParameters(ModalType.Error)
		{
			TitleText = cultureSpecificTitle,
			MessageText = cultureSpecificMessage,
			Button1Text = cultureSpecificButtonText,
			Button1Action = continueAction,
			TimeOutAction = timeOutAction,
			AnalyticsContextList = analyticsContexts
		});
	}

	public static void ShowLogOutWarning(Action timeOutAction, Action backAction, Action continueAction)
	{
		LogHelper.Instance.Log("Showing Log out warning message");
		ShowModal(new ModalParameters(ModalType.Dialog)
		{
			TitleText = Resources.sign_out_dialog_title,
			MessageText = Resources.sign_out_dialog_message,
			Button1Text = Resources.back_button.ToUpper(),
			Button2Text = Resources.sign_out_button,
			TimeOutAction = timeOutAction,
			Button1Action = backAction,
			Button2Action = continueAction,
			ViewName = "SignOutWarningMessage",
			TimeOut = GetDefaultMessageTimeOut(30000, "long")
		});
	}

	public static void ShowPerksCreateAccountError(Action timeOutAction, Action continueAction)
	{
		LogHelper.Instance.Log("Showing Perks Create Account Error");
		ShowModal(new ModalParameters(ModalType.Informational)
		{
			MessageText = Resources.perks_sign_up_cannot_create_account_message,
			Button1Text = Resources.okay.ToUpper(),
			TimeOut = GetDefaultMessageTimeOut(30000, "long"),
			Button1Action = continueAction,
			TimeOutAction = timeOutAction,
			ViewName = "PerksCreateAccountError"
		});
	}

	public static void ShowInventoryError(Action timeOutAction, Action continueAction)
	{
		LogHelper.Instance.Log("Showing Inventory Error");
		ShowModal(new ModalParameters(ModalType.Informational)
		{
			MessageText = Resources.inventory_error,
			Button1Text = Resources.okay.ToUpper(),
			TimeOut = GetDefaultMessageTimeOut(30000, "long"),
			Button1Action = continueAction,
			TimeOutAction = timeOutAction,
			ViewName = "InventoryError"
		});
	}

	public static void ShowLoyaltyIsDownOnLoginMessage(Action timeOutAction, Action continueAction)
	{
		LogHelper.Instance.Log("Showing loyalty is down on login message");
		ShowModal(new ModalParameters(ModalType.Error)
		{
			TitleText = Resources.personalization_service_error_KLI0005,
			Button1Text = Resources.okay.ToUpper(),
			TimeOut = GetDefaultMessageTimeOut(30000, "long"),
			Button1Action = continueAction,
			TimeOutAction = timeOutAction,
			ActorTag = "LoyaltyIsDownMessage"
		});
	}

	public static void ShowMissingRedboxPlusRentalItemMessage(Action timeOutAction, Action backAction, Action continueAction)
	{
		LogHelper.Instance.Log("Showing missing redbox+ rental item message");
		ShowModal(new ModalParameters(ModalType.Error)
		{
			TitleText = Resources.redbox_plus_missing_item_error_title,
			MessageText = Resources.redbox_plus_missing_item_error_message,
			Button1Text = Resources.okay.ToUpper(),
			TimeOutAction = timeOutAction,
			Button1Action = continueAction
		});
	}

	public static void ShowRedboxPlusCartFullMessage(Action timeOutAction, Action continueAction)
	{
		LogHelper.Instance.Log("Showing redbox+ cart full message");
		ShowModal(new ModalParameters(ModalType.Error)
		{
			TitleText = Resources.redbox_plus_cart_full_message_title,
			MessageText = Resources.redbox_plus_cart_full_message_body,
			Button1Text = Resources.okay.ToUpper(),
			TimeOutAction = timeOutAction,
			Button1Action = continueAction
		});
	}

	public static void ShowRedboxPlusFreeOneNightRentalAlreadyUsedTier1Message(Action timeOutAction, Action continueAction)
	{
		LogHelper.Instance.Log("Showing redbox+ monthly free 1-night rental already used tier1 message");
		ShowModal(new ModalParameters(ModalType.Error)
		{
			TitleText = Resources.redbox_plus_already_used_tier1_header,
			MessageText = Resources.redbox_plus_already_used_tier1_message,
			Button1Text = Resources.redbox_plus_already_used_close_button_text,
			TimeOutAction = timeOutAction,
			Button1Action = continueAction
		});
	}

	public static void ShowRedboxPlusFreeOneNightRentalAlreadyUsedTier2Message(Action timeOutAction, Action continueAction)
	{
		LogHelper.Instance.Log("Showing redbox+ monthly free 1-night rental already used tier2 message");
		ShowModal(new ModalParameters(ModalType.Error)
		{
			TitleText = Resources.redbox_plus_already_used_tier2_header,
			MessageText = Resources.redbox_plus_already_used_tier2_message,
			Button1Text = Resources.redbox_plus_already_used_close_button_text,
			TimeOutAction = timeOutAction,
			Button1Action = continueAction
		});
	}

	public static void ShowRedboxPlusFreeOneNightRentalApplied(Action continueAction)
	{
		LogHelper.Instance.Log("Showing redbox+ monthly free 1-night rental appllied message");
		ShowModal(new ModalParameters(ModalType.Informational)
		{
			TitleText = Resources.redbox_plus_added_free_1_night_rental_title,
			Button1Text = Resources.okay.ToUpper(),
			Button1Action = continueAction,
			TimeOutAction = continueAction
		});
	}

	public static void ShowRedboxPlusPerksPointErrorMessage(Action timeOutAction, Action backAction, Action continueAction)
	{
		LogHelper.Instance.Log("Showing redbox+ cart full message");
		ShowModal(new ModalParameters(ModalType.Error)
		{
			TitleText = Resources.redbox_plus_perks_points_redemption_error_title,
			MessageText = Resources.redbox_plus_perks_points_redemption_error_message,
			Button1Text = Resources.okay.ToUpper(),
			TimeOutAction = timeOutAction,
			Button1Action = continueAction
		});
	}

	public static void ShowRedboxPlusMultiNightPriceErrorMessage(Action timeOutAction, Action continueAction)
	{
		LogHelper.Instance.Log("Showing redbox+ multi-night price error message");
		ShowModal(new ModalParameters(ModalType.Error)
		{
			TitleText = Resources.redbox_plus_multi_night_error_title,
			MessageText = Resources.redbox_plus_multi_night_error_message,
			Button1Text = Resources.okay.ToUpper(),
			TimeOutAction = timeOutAction,
			Button1Action = continueAction
		});
	}

	public static void ShowRedboxPlusGenericErrorMessage(Action continueAction)
	{
		ShowModal(new ModalParameters(ModalType.Error)
		{
			TitleText = Resources.redbox_plus_signup_generic_error_title,
			MessageText = Resources.redbox_plus_signup_generic_error_message,
			Button1Text = Resources.okay.ToUpper(),
			Button1Action = continueAction,
			TimeOutAction = continueAction,
			AnalyticsContextList = new List<string> { "Redbox+ signup generic error" }
		});
	}

	public static void ShowRedboxPlusExistingSubscriptionErrorMessage(Action continueAction)
	{
		ShowModal(new ModalParameters(ModalType.Error)
		{
			TitleText = Resources.redbox_plus_signup_existing_subscription_error_title,
			MessageText = Resources.redbox_plus_signup_existing_subscription_error_message,
			Button1Text = Resources.okay.ToUpper(),
			Button1Action = continueAction,
			TimeOutAction = continueAction,
			AnalyticsContextList = new List<string> { "Redbox+ signup existing subscription error" }
		});
	}

	public static void ShowRedboxPlusExistingAccountErrorMessage(Action continueAction)
	{
		ShowModal(new ModalParameters(ModalType.Error)
		{
			TitleText = Resources.redbox_plus_signup_existing_account_error_title,
			MessageText = Resources.redbox_plus_signup_existing_account_error_message,
			Button1Text = Resources.okay.ToUpper(),
			Button1Action = continueAction,
			TimeOutAction = continueAction,
			AnalyticsContextList = new List<string> { "Redbox+ signup error existing Redbox Account" }
		});
	}

	public static void ShowOfferTimeoutMessage(Action timeOutAction, Action backAction, Action continueAction)
	{
		LogHelper.Instance.Log("Showing offer dialog timeout message");
		ShowModal(new ModalParameters(ModalType.Informational)
		{
			TitleText = Resources.offer_timeout_title,
			MessageText = Resources.offer_timeout_message,
			Button1Text = Resources.no_thanks_button_text.ToUpper(),
			Button2Text = Resources.yes_button_mixed_case.ToUpper(),
			TimeOut = GetDefaultMessageTimeOut(30000, "short"),
			Button1Action = backAction,
			Button2Action = continueAction,
			TimeOutAction = timeOutAction,
			ActorTag = "OfferTimeoutMessage"
		});
	}

	public static void ShowSwapPromoForRedboxPlusBenefitMessage(Action timeOutAction, Action backAction, Action continueAction)
	{
		LogHelper.Instance.Log("Showing swap promo for Redbox+ free one night rental message");
		ShowModal(new ModalParameters(ModalType.Dialog)
		{
			TitleText = Resources.swap_promo_with_redbox_plus_benefit,
			Button2Text = Resources.yes_button,
			Button1Text = Resources.no_button,
			Button1Action = backAction,
			TimeOutAction = timeOutAction,
			Button2Action = continueAction,
			TimeOut = GetDefaultMessageTimeOut(30000, "short"),
			ViewName = "SwapPromoForRedboxPlusFreeOneNightRental"
		});
	}

	public static void ShowSwapRedboxPlusBenefitForPromoMessage(Action timeOutAction, Action backAction, Action continueAction)
	{
		LogHelper.Instance.Log("Showing swap Redbox+ benefit for promo message");
		ShowModal(new ModalParameters(ModalType.Dialog)
		{
			TitleText = Resources.swap_redbox_plus_benefit_with_promo,
			Button2Text = Resources.yes_button,
			Button1Text = Resources.no_button,
			Button1Action = backAction,
			TimeOutAction = timeOutAction,
			Button2Action = continueAction,
			TimeOut = GetDefaultMessageTimeOut(30000, "short"),
			ViewName = "SwapRedboxPlusBenefitForPromo"
		});
	}

	public static void ShowRedboxPlusAuthorizeErrorMessage(Action continueAction)
	{
		ShowModal(new ModalParameters(ModalType.Error)
		{
			TitleText = Resources.auth_error_redbox_plus_auth_declined_title,
			MessageText = Resources.auth_error_redbox_plus_auth_declined_message,
			Button1Text = Resources.okay.ToUpper(),
			Button1Action = continueAction,
			TimeOutAction = continueAction,
			AnalyticsContextList = new List<string> { "Redbox+ signup authorize error" }
		});
	}

	public static void ShowLoyaltyIsDownMessage(Action timeOutAction, Action continueAction)
	{
		LogHelper.Instance.Log("Showing loyalty is down message");
		ShowModal(new ModalParameters(ModalType.Error)
		{
			TitleText = Resources.loyalty_down_dialog_title,
			MessageText = Resources.loyalty_down_dialog_text,
			Button1Text = Resources.okay.ToUpper(),
			TimeOut = GetDefaultMessageTimeOut(30000, "short"),
			Button1Action = continueAction,
			TimeOutAction = timeOutAction,
			ActorTag = "LoyaltyIsDownMessage"
		});
	}

	public static void ShowInCartOfferMessage(Action timeOutAction, Action backAction, Action continueAction)
	{
		LogHelper.Instance.Log("Showing in cart offer message");
		InCartOfferMessageText inCartOfferMessageText = PromotionService.Instance.GetInCartOfferMessageText();
		ShowModal(new ModalParameters(ModalType.Dialog)
		{
			TitleText = inCartOfferMessageText.Header,
			MessageText = inCartOfferMessageText.Message,
			Button1Text = ((PromotionService.Instance.IsFreeInCartOfferRunning || (PromotionService.Instance.IsRentTwoInCartOfferRunning && PromotionService.Instance.ValidateRentTwoInCartPromo().IsCartValid)) ? null : inCartOfferMessageText.NoButtonText),
			Button2Text = inCartOfferMessageText.YesButtonText,
			TimeOut = GetDefaultMessageTimeOut(30000, "long"),
			Button1Action = backAction,
			Button2Action = continueAction,
			TimeOutAction = timeOutAction,
			ActorTag = "InCartOfferMessage"
		});
	}

	public static void ShowGiftCardNotEnabledMessage(Action timeOutAction, Action continueAction)
	{
		LogHelper.Instance.Log("Showing gift card not enabled message");
		ShowModal(new ModalParameters(ModalType.Error)
		{
			TitleText = Resources.gc_not_available_title,
			MessageText = Resources.gc_not_available,
			Button1Text = Resources.continue_button,
			TimeOut = GetDefaultMessageTimeOut(30000, "long"),
			Button1Action = continueAction,
			TimeOutAction = timeOutAction,
			ActorTag = "GiftCardNotEnabledMessage"
		});
	}

	public static void ShowTitleLevelPromoCodeInvalidWarning(Action timeOutAction, Action backAction, Action continueAction)
	{
		LogHelper.Instance.Log("Showing promo code invalid warning");
		ShowModal(new ModalParameters(ModalType.Dialog)
		{
			TitleText = Resources.tsp_invalid_header2,
			MessageText = Resources.tsp_invalid_message,
			Button1Text = Resources.do_not_use_promo_button,
			Button2Text = Resources.use_promo_button,
			TimeOut = GetDefaultMessageTimeOut(),
			Button1Action = backAction,
			Button2Action = continueAction,
			TimeOutAction = timeOutAction,
			ActorTag = "PromoCodeInvalidWarning"
		});
	}

	public static void ShowTitleLevelPromoCodeInvalidWarning(Action timeOutAction, Action continueAction)
	{
		LogHelper.Instance.Log("Showing title level promo code invalid warning");
		ShowModal(new ModalParameters(ModalType.Informational)
		{
			TitleText = Resources.tsp_invalid_header2,
			MessageText = Resources.tsp_invalid_message,
			Button1Text = Resources.continue_button,
			TimeOut = GetDefaultMessageTimeOut(),
			Button1Action = continueAction,
			TimeOutAction = timeOutAction,
			ActorTag = "TitleLevelPromoCodeInvalidWarning",
			AnalyticsContextList = new List<string> { "titlelevelpromoWarning" }
		});
	}

	public static void ShowFirstNightFreePromoCodeInvalidWarning(Action timeOutAction, Action backAction, Action continueAction, string message)
	{
		LogHelper.Instance.Log("Showing first night free promo code invalid warning");
		ShowModal(new ModalParameters(ModalType.Informational)
		{
			TitleText = Resources.tsp_invalid_header,
			MessageText = message,
			Button1Text = Resources.do_not_use_promo_button,
			Button2Text = Resources.use_promo_button,
			TimeOut = GetDefaultMessageTimeOut(),
			Button1Action = backAction,
			Button2Action = continueAction,
			TimeOutAction = timeOutAction,
			ActorTag = "FirstNightFreePromoCodeInvalidWarning"
		});
	}

	public static void ShowFirstNightFreePromoCodeInvalidWarning(Action timeOutAction, Action continueAction, string message)
	{
		LogHelper.Instance.Log("Showing first night free promo code invalid warning");
		ShowModal(new ModalParameters(ModalType.Informational)
		{
			TitleText = Resources.tsp_invalid_header,
			MessageText = message,
			Button1Text = Resources.continue_button,
			TimeOut = GetDefaultMessageTimeOut(),
			Button1Action = continueAction,
			TimeOutAction = timeOutAction,
			ActorTag = "FirstNightFreePromoCodeInvalidWarning",
			AnalyticsContextList = new List<string> { "firstnightfreepromoWarning" }
		});
	}

	public static void ShowRxGxPromoCodeInvalidWarning(Action timeOutAction, Action backAction, Action continueAction, string message)
	{
		LogHelper.Instance.Log("Showing promo code invalid warning");
		ShowModal(new ModalParameters(ModalType.Dialog)
		{
			TitleText = Resources.oops,
			MessageText = message,
			Button1Text = Resources.do_not_use_promo_button,
			Button2Text = Resources.use_promo_button,
			TimeOut = GetDefaultMessageTimeOut(),
			Button1Action = backAction,
			Button2Action = continueAction,
			TimeOutAction = timeOutAction,
			ActorTag = "PromoCodeInvalidWarning",
			AnalyticsContextList = new List<string> { "rxgxWarning" }
		});
	}

	public static void ShowRxGxPromoCodeInvalidWarning(Action timeOutAction, Action continueAction, string message)
	{
		LogHelper.Instance.Log("Showing rxgx promo code invalid warning");
		ShowModal(new ModalParameters(ModalType.Informational)
		{
			TitleText = Resources.oops,
			MessageText = message,
			Button1Text = Resources.continue_button,
			TimeOut = GetDefaultMessageTimeOut(),
			Button1Action = continueAction,
			TimeOutAction = timeOutAction,
			ActorTag = "PromoCodeRxGxInvalidWarning",
			AnalyticsContextList = new List<string> { "rxgxWarning" }
		});
	}

	public static void ShowPromoCodeInvalidForCartItemsWarning(Action timeOutAction, Action continueAction, string title, string message)
	{
		LogHelper.Instance.Log("Showing promo code invalid for cart items warning");
		ShowModal(new ModalParameters(ModalType.Error)
		{
			TitleText = title,
			MessageText = message,
			Button1Text = Resources.continue_button,
			TimeOut = GetDefaultMessageTimeOut(),
			Button1Action = continueAction,
			TimeOutAction = timeOutAction,
			ActorTag = "PromoCodeInvalidForCartItemsWarning",
			AnalyticsContextList = new List<string> { "promoCodeInvalidForCartItemsWarning" }
		});
	}

	public static void ShowPromoCodeAlreadyUsedWarning(Action timeOutAction, Action continueAction)
	{
		LogHelper.Instance.Log("Showing promo code already used warning");
		ShowModal(new ModalParameters(ModalType.Informational)
		{
			TitleText = Resources.promo_code_already_used_message_title,
			MessageText = Resources.promo_code_already_used_message,
			Button1Text = Resources.okay_button,
			TimeOut = GetDefaultMessageTimeOut(),
			Button1Action = continueAction,
			TimeOutAction = timeOutAction,
			ActorTag = "PromoCodeAlreadyUsedWarning",
			AnalyticsContextList = new List<string> { "promoAlreadyUsed" }
		});
	}

	public static void ShowPromoCodeNotValidAtThisLocationWarning(Action timeOutAction, Action continueAction)
	{
		LogHelper.Instance.Log("Showing promo code not valid at this location warning");
		ShowModal(new ModalParameters(ModalType.Error)
		{
			TitleText = Resources.promo_code_not_valid_location_title,
			MessageText = Resources.promo_code_not_valid_location_message,
			Button1Text = Resources.okay_button,
			TimeOut = GetDefaultMessageTimeOut(),
			Button1Action = continueAction,
			TimeOutAction = timeOutAction,
			ActorTag = "PromoCodeNotValidAtThisLocationWarning",
			AnalyticsContextList = new List<string> { "promoInvalidLocation" }
		});
	}

	public static void ShowPromoCodeForOnDemandWarning(Action timeOutAction, Action continueAction)
	{
		LogHelper.Instance.Log("Showing promo code for on demand warning");
		ShowModal(new ModalParameters(ModalType.Error)
		{
			TitleText = Resources.promo_code_for_on_demand_title,
			MessageText = Resources.promo_code_for_on_demand_message,
			Button1Text = Resources.okay_button,
			TimeOut = GetDefaultMessageTimeOut(),
			Button1Action = continueAction,
			TimeOutAction = timeOutAction,
			ActorTag = "PromoCodeForOnDemandWarning",
			AnalyticsContextList = new List<string> { "promoForOnDemand" }
		});
	}

	public static void ShowPromoCodeValidationFailureWarning(Action timeOutAction, Action continueAction)
	{
		LogHelper.Instance.Log("Showing promo code validation failure warning");
		ShowModal(new ModalParameters(ModalType.Informational)
		{
			TitleText = Resources.promo_code_validation_failure_message_title,
			MessageText = Resources.promo_code_validation_failure_message,
			Button1Text = Resources.okay_button,
			TimeOut = GetDefaultMessageTimeOut(),
			Button1Action = continueAction,
			TimeOutAction = timeOutAction,
			ActorTag = "PromoCodeValidationFailureWarning",
			AnalyticsContextList = new List<string> { "promoCantBeUsed" }
		});
	}

	public static void ShowPromoCodeGameRequiredWarning(Action timeOutAction, Action continueAction)
	{
		LogHelper.Instance.Log("Showing promo code game required warning");
		ShowModal(new ModalParameters(ModalType.Informational)
		{
			TitleText = Resources.promo_code_game_required_title,
			MessageText = Resources.promo_code_game_required_message,
			Button1Text = Resources.okay_button,
			TimeOut = GetDefaultMessageTimeOut(),
			Button1Action = continueAction,
			TimeOutAction = timeOutAction,
			ActorTag = "PromoCodeGameRequiredWarning",
			AnalyticsContextList = new List<string> { "promoCantBeUsed" }
		});
	}

	public static void ShowPromoCodeNotValidWarning(Action timeOutAction, Action continueAction)
	{
		LogHelper.Instance.Log("Showing promo code not valid warning");
		ShowModal(new ModalParameters(ModalType.Informational)
		{
			TitleText = Resources.promo_code_not_valid_message_title,
			MessageText = Resources.promo_code_not_valid_message,
			Button1Text = Resources.okay_button,
			TimeOut = GetDefaultMessageTimeOut(),
			Button1Action = continueAction,
			TimeOutAction = timeOutAction,
			ActorTag = "PromoCodeNotValidWarning",
			AnalyticsContextList = new List<string> { "promoNotValid" }
		});
	}

	public static void ShowPromoCodeDiscountRestrictionWarning(string title, string continueButtonText, Action timeOutAction, Action backAction, Action continueAction)
	{
		LogHelper.Instance.Log("Showing promo code discount restriction warning message");
		ShowModal(new ModalParameters(ModalType.Dialog)
		{
			TitleText = title,
			MessageText = Resources.promo_code_discount_restriction_warning_message,
			Button1Text = Resources.no_thanks_button.ToUpper(),
			Button2Text = continueButtonText,
			TimeOutAction = timeOutAction,
			Button1Action = backAction,
			Button2Action = continueAction,
			ViewName = "PromoCodeDiscountRestrictionWarningPopupView",
			AnalyticsContextList = new List<string> { "PromoCodeDiscountRestrictionWarningPopupView" }
		});
	}

	public static void ShowPromoCodeReturnVisitIneligibleCartWarning(string button2Text, Action timeOutAction, Action backAction, Action continueAction)
	{
		LogHelper.Instance.Log("Showing promo code return visit ineligible cart warning message");
		ShowModal(new ModalParameters(ModalType.Error)
		{
			TitleText = Resources.promo_code_return_visit_ineligible_cart_warning_title,
			MessageText = Resources.promo_code_return_visit_ineligible_cart_warning_message,
			Button1Text = Resources.add_movie_button_text.ToUpper(),
			Button2Text = button2Text,
			TimeOutAction = timeOutAction,
			Button1Action = backAction,
			Button2Action = continueAction,
			ViewName = "PromoCodeReturnVisitIneligibleCartWarningPopupView",
			AnalyticsContextList = new List<string> { "PromoCodeReturnVisitIneligibleCartWarningPopupView" }
		});
	}

	public static void ShowPromoCodeIneligibleCartWarning(Action timeOutAction, Action continueAction)
	{
		LogHelper.Instance.Log("Showing promo code ineligible cart warning message");
		ShowModal(new ModalParameters(ModalType.Error)
		{
			TitleText = Resources.promo_code_ineligible_cart_warning_title,
			MessageText = Resources.promo_code_ineligible_cart_warning_message,
			Button1Text = Resources.got_it_button.ToUpper(),
			TimeOutAction = timeOutAction,
			Button1Action = continueAction,
			ViewName = "PromoCodeIneligibleCartWarningPopupView",
			AnalyticsContextList = new List<string> { "PromoCodeIneligibleCartWarningPopupView" }
		});
	}

	public static void ShowPromoCodeIneligibleCartPurchaseWarning(Action timeOutAction, Action continueAction)
	{
		LogHelper.Instance.Log("Showing promo code ineligible cart purchase warning message");
		ShowModal(new ModalParameters(ModalType.Error)
		{
			TitleText = Resources.promo_code_ineligible_cart_purchase_warning_title,
			MessageText = Resources.promo_code_ineligible_cart_purchase_warning_message,
			Button1Text = Resources.continue_button,
			TimeOutAction = timeOutAction,
			Button1Action = continueAction,
			ViewName = "PromoCodeIneligibleCartPurchaseWarningPopupView",
			AnalyticsContextList = new List<string> { "PromoCodeIneligibleCartPurchaseWarningPopupView" }
		});
	}

	public static void ShowPromoCodeIneligibleCartRentalWarning(Action timeOutAction, Action continueAction)
	{
		LogHelper.Instance.Log("Showing promo code ineligible cart rental warning message");
		ShowModal(new ModalParameters(ModalType.Error)
		{
			TitleText = Resources.promo_code_ineligible_cart_rental_warning_title,
			MessageText = Resources.promo_code_ineligible_cart_rental_warning_message,
			Button1Text = Resources.continue_button,
			TimeOutAction = timeOutAction,
			Button1Action = continueAction,
			ViewName = "PromoCodeIneligibleCartRentalWarningPopupView",
			AnalyticsContextList = new List<string> { "PromoCodeIneligibleCartRentalWarningPopupView" }
		});
	}

	public static void ShowInCartCancelWarning(Action timeOutAction, Action backAction, Action continueAction)
	{
		LogHelper.Instance.Log("Showing in cart offer cancel warning message popup");
		ShowModal(new ModalParameters(ModalType.Dialog)
		{
			TitleText = Resources.in_cart_remove_item_warning_header,
			MessageText = Resources.in_cart_remove_item_warning_message,
			Button1Text = Resources.back_button.ToUpper(),
			Button2Text = Resources.continue_button,
			TimeOut = GetDefaultMessageTimeOut(),
			Button1Action = backAction,
			Button2Action = continueAction,
			TimeOutAction = timeOutAction,
			ActorTag = "InCartCancelWarning"
		});
	}

	public static void ShowSubPlatformWarning(Action timeOutAction, Action backAction, Action continueAction)
	{
		LogHelper.Instance.Log("Showing Sub Platform Warning message popup");
		IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
		ShowModal(new ModalParameters(ModalType.Dialog)
		{
			TitleText = service["Subplatform:Header"],
			MessageText = service["Subplatform:Message"],
			Button1Text = Resources.back_uppercase_button,
			Button2Text = Resources.okay_button,
			TimeOut = GetDefaultMessageTimeOut(),
			Button1Action = backAction,
			Button2Action = continueAction,
			TimeOutAction = timeOutAction,
			ActorTag = "SubPlatformWarning",
			AnalyticsContextList = new List<string> { "subplatformWarning" }
		});
	}

	public static void ShowADAPurchaseWarning(Action timeOutAction, Action backAction, Action continueAction)
	{
		LogHelper.Instance.Log("Showing ADA Purchase Warning message popup");
		IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
		ShowModal(new ModalParameters(ModalType.Dialog)
		{
			TitleText = service["ADAPurchaseWarning:Header"],
			Button1Text = Resources.back_uppercase_button,
			Button2Text = Resources.okay_button,
			TimeOut = GetDefaultMessageTimeOut(),
			Button1Action = backAction,
			Button2Action = continueAction,
			TimeOutAction = timeOutAction,
			ActorTag = "ADAPurchaseWarning"
		});
	}

	public static void ShowPurchaseWarning(Action timeOutAction, Action backAction, Action continueAction)
	{
		LogHelper.Instance.Log("Showing Purchase Warning message popup");
		IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
		ShowModal(new ModalParameters(ModalType.Dialog)
		{
			TitleText = service["PurchaseWarning:Header"],
			MessageText = service["PurchaseWarning:Message"],
			Button1Text = Resources.back_uppercase_button,
			Button2Text = Resources.okay_button,
			TimeOut = GetDefaultMessageTimeOut(),
			Button1Action = backAction,
			Button2Action = continueAction,
			TimeOutAction = timeOutAction,
			ActorTag = "PurchaseWarning",
			AnalyticsContextList = new List<string> { "purchaseWarning" }
		});
	}

	public static void ShowMatureWarning(Action timeOutAction, Action backAction, Action continueAction, List<string> analyticsContexts)
	{
		LogHelper.Instance.Log("Showing Mature Warning message popup");
		IMacroService service = ServiceLocator.Instance.GetService<IMacroService>();
		ShowModal(new ModalParameters(ModalType.Dialog)
		{
			TitleText = service["MatureWarning:Header"],
			MessageText = service["MatureWarning:Message"],
			Button1Text = Resources.back_uppercase_button,
			Button2Text = Resources.okay_button,
			TimeOut = GetDefaultMessageTimeOut(),
			Button1Action = backAction,
			Button2Action = continueAction,
			TimeOutAction = timeOutAction,
			ActorTag = "MatureWarning",
			AnalyticsContextList = analyticsContexts
		});
	}

	public static void Show4KAlert(Action timeOutAction, Action backAction, Action continueAction)
	{
		LogHelper.Instance.Log("Showing 4K UHD Alert message popup");
		ShowModal(new ModalParameters(ModalType.Dialog)
		{
			TitleText = Resources.four_k_alert_title,
			MessageText = Resources.four_k_alert,
			Button1Text = Resources.cancel_button.ToUpper(),
			Button2Text = Resources.got_it_button.ToUpper(),
			TimeOut = GetDefaultMessageTimeOut(),
			Button1Action = backAction,
			Button2Action = continueAction,
			TimeOutAction = timeOutAction,
			ActorTag = "4KUHDAlert",
			AnalyticsContextList = new List<string> { "4KDialog" },
			ViewName = "4K_warning_popup_view"
		});
	}

	public static void ShowItemInCartQuitMessagePopupView(Action timeOutAction, Action backAction, Action continueAction)
	{
		ShowModal(new ModalParameters(ModalType.Dialog)
		{
			TitleText = Resources.item_in_cart_quit_popup_message,
			Button1Text = Resources.keep_browsing_button.ToUpper(),
			Button2Text = Resources.quit_confirm_button.ToUpper(),
			TimeOutAction = timeOutAction,
			Button1Action = backAction,
			Button2Action = continueAction,
			ViewName = "ItemInCartQuitMessagePopupView",
			AnalyticsContextList = new List<string> { "ItemInCartQuitMessagePopupView" }
		});
	}

	public static void ShowSignedInEmptyCartQuitMessagePopupView(Action timeOutAction, Action backAction, Action continueAction)
	{
		ShowModal(new ModalParameters(ModalType.Dialog)
		{
			TitleText = Resources.signed_in_empty_cart_quit_popup_message,
			Button1Text = Resources.keep_browsing_button.ToUpper(),
			Button2Text = Resources.quit_confirm_button.ToUpper(),
			TimeOutAction = timeOutAction,
			Button1Action = backAction,
			Button2Action = continueAction,
			ViewName = "SignedInEmptyCartQuitMessagePopupView",
			AnalyticsContextList = new List<string> { "SignedInEmptyCartQuitMessagePopupView" }
		});
	}

	public static void ShowSignedInItemInCartQuitMessagePopupView(Action timeOutAction, Action backAction, Action continueAction)
	{
		ShowModal(new ModalParameters(ModalType.Dialog)
		{
			TitleText = Resources.signed_in_item_in_cart_quit_popup_message,
			Button1Text = Resources.keep_browsing_button.ToUpper(),
			Button2Text = Resources.quit_confirm_button.ToUpper(),
			TimeOutAction = timeOutAction,
			Button1Action = backAction,
			Button2Action = continueAction,
			ViewName = "SignedInItemInCartQuitMessagePopupView",
			AnalyticsContextList = new List<string> { "SignedInItemInCartQuitMessagePopupView" }
		});
	}

	public static void ShowRemoveSubscriptionWarning(Action timeOutAction, Action backAction, Action continueAction)
	{
		ShowModal(new ModalParameters(ModalType.Dialog)
		{
			TitleText = Resources.subscription_removal_title,
			MessageText = Resources.subscription_removal_message,
			Button1Text = Resources.cancel_button.ToUpper(),
			Button2Text = Resources.remove_button,
			TimeOutAction = timeOutAction,
			Button1Action = backAction,
			Button2Action = continueAction,
			ViewName = "remove_subscription_popup_view",
			AnalyticsContextList = new List<string> { "RemoveSubscription" }
		});
	}

	public static void ShowMultiDiscAlert(string title, string message, Action timeOutAction, Action continueAction)
	{
		LogHelper.Instance.Log("Showing Multi Disc Alert message popup");
		ShowModal(new ModalParameters(ModalType.Informational)
		{
			TitleText = title,
			MessageText = message,
			Button1Text = Resources.multi_disc_warning_button_text.ToUpper(),
			TimeOut = GetDefaultMessageTimeOut(),
			Button1Action = continueAction,
			TimeOutAction = timeOutAction,
			ActorTag = "MultiDiscAlert",
			AnalyticsContextList = new List<string> { "MultiDiscWarningDialog" },
			ViewName = "multi_disc_warning_popup_view"
		});
	}

	public static void ShowDigitalCodeAlert(Action backAction, Action okAction, Action timeOutAction)
	{
		LogHelper.Instance.Log("Showing Digital Code Alert message popup");
		ShowModal(new ModalParameters(ModalType.Dialog)
		{
			TitleText = Resources.digital_code_alert_title,
			MessageText = Resources.digital_code_alert_message,
			Button1Text = Resources.back_uppercase_button,
			Button2Text = Resources.okay_button,
			TimeOut = GetDefaultMessageTimeOut(),
			Button1Action = backAction,
			Button2Action = okAction,
			TimeOutAction = timeOutAction,
			ActorTag = "DigitalCodeAlert",
			AnalyticsContextList = new List<string> { "digitalcodeincart_message_view" }
		});
	}

	public static void ShowMessagePopupViewReturnSuccess(Action timeOutAction)
	{
		LogHelper.Instance.Log("Showing Return Success message popup");
		ShowModal(new ModalParameters(ModalType.Error)
		{
			TitleText = Resources.title_thanks,
			MessageText = Resources.inventory_updated_message,
			TimeOut = GetDefaultMessageTimeOut(5000, "short"),
			TimeOutAction = timeOutAction,
			ActorTag = "ViewReturnSuccess"
		});
	}

	public static void ShowMessagePopupViewReturnSuccessWithBrowse(Action timeOutAction, Action backAction, Action continueAction)
	{
		LogHelper.Instance.Log("Showing Return Success with Browse message popup");
		ShowModal(new ModalParameters(ModalType.Dialog)
		{
			TitleText = Resources.return_success_with_browse_popup_title,
			MessageText = Resources.return_success_with_browse_popup_message,
			Button1Text = Resources.close_button.ToUpper(),
			Button2Text = Resources.see_new_movies_button_text.ToUpper(),
			TimeOutAction = timeOutAction,
			Button1Action = backAction,
			Button2Action = continueAction,
			ViewName = "ReturnSuccessWithBrowseMessagePopupView",
			AnalyticsContextList = new List<string> { "ReturnSuccessWithBrowseMessagePopupView" }
		});
	}

	public static void ShowMessageCartFull(Action timeoutAction, Action continueAction)
	{
		LogHelper.Instance.Log("Showing Cart Full message popup");
		ShowModal(new ModalParameters(ModalType.Informational)
		{
			TitleText = Resources.cart_full_header,
			MessageText = Resources.cart_full_body,
			Button1Text = Resources.continue_button,
			TimeOut = OrchestrationService.GetIdleTimerTimeout(),
			Button1Action = continueAction,
			TimeOutAction = timeoutAction,
			ActorTag = "CartFull"
		});
	}

	public static void ShowMessagePopupViewHardwareError(Action timeOutAction, Action continueAction)
	{
		LogHelper.Instance.Log("Showing Hardware Error message popup");
		ShowModal(new ModalParameters(ModalType.Error)
		{
			MessageText = Resources.hardware_error_message,
			Button1Text = Resources.got_it_button.ToUpper(),
			TimeOut = GetDefaultMessageTimeOut(),
			Button1Action = continueAction,
			TimeOutAction = timeOutAction,
			ActorTag = "ViewHardwareError",
			AnalyticsContextList = new List<string> { "hardwareError" }
		});
	}

	public static void ShowMessagePopupViewReturnBackwardInserted(Action timeOutAction, Action continueAction)
	{
		LogHelper.Instance.Log("Showing Return Backward Inserted message popup");
		ShowModal(new ModalParameters(ModalType.Error)
		{
			TitleText = Resources.backward_insert_error_title,
			MessageText = Resources.backward_insert_error,
			Button1Text = Resources.got_it_button.ToUpper(),
			TimeOut = GetDefaultMessageTimeOut(),
			Button1Action = continueAction,
			TimeOutAction = timeOutAction,
			ActorTag = "ViewReturnBackwardInserted",
			AnalyticsContextList = new List<string> { "returnBackwardInserted" }
		});
	}

	public static void ShowMessagePopupViewReturnUnrecognizedBarcode(Action timeOutAction, Action continueAction)
	{
		LogHelper.Instance.Log("Showing Return Unrecognized Barcode message popup");
		ShowModal(new ModalParameters(ModalType.Error)
		{
			TitleText = Resources.oops,
			MessageText = Resources.unrecognized_barcode_returned_message,
			Button1Text = Resources.got_it_button.ToUpper(),
			TimeOut = GetDefaultMessageTimeOut(),
			Button1Action = continueAction,
			TimeOutAction = timeOutAction,
			ActorTag = "ViewReturnUnrecognizedBarcode",
			AnalyticsContextList = new List<string> { "returnUnrecognizedBarcode" }
		});
	}

	public static void ShowMessagePopupViewCardNotRecognized(Action timeOutAction, Action continueAction)
	{
		LogHelper.Instance.Log("Showing Card Not Recognized message popup");
		ShowModal(new ModalParameters(ModalType.Error)
		{
			TitleText = Resources.invalid_card_title,
			MessageText = Resources.invalid_card_message,
			Button1Text = Resources.got_it_button.ToUpper(),
			TimeOut = GetDefaultMessageTimeOut(),
			Button1Action = continueAction,
			TimeOutAction = timeOutAction,
			ActorTag = "ViewCardNotRecognized",
			AnalyticsContextList = new List<string> { "cardNotRecognized" }
		});
	}

	public static void ShowMessagePopupViewCheckoutCardReaderTampered(Action timeOutAction, Action continueAction)
	{
		LogHelper.Instance.Log("Showing Tampered card reader message popup");
		ShowModal(new ModalParameters(ModalType.Error)
		{
			TitleText = Resources.card_reader_tampered_message_title,
			MessageText = Resources.card_reader_tampered_checkout_flow_message_body,
			Button1Text = Resources.got_it_button.ToUpper(),
			TimeOut = GetDefaultMessageTimeOut(),
			Button1Action = continueAction,
			TimeOutAction = timeOutAction,
			ActorTag = "ViewSwipedCardIsChipEnabled",
			AnalyticsContextList = new List<string> { "swipedCardIsChipEnabled" }
		});
	}

	public static void ShowMessagePopupViewReservationCardReaderTampered(Action timeOutAction, Action continueAction)
	{
		LogHelper.Instance.Log("Showing Tampered card reader message popup");
		ShowModal(new ModalParameters(ModalType.Error)
		{
			TitleText = Resources.card_reader_tampered_message_title,
			MessageText = Resources.card_reader_tampered_reservation_flow_message_body,
			Button1Text = Resources.got_it_button.ToUpper(),
			TimeOut = GetDefaultMessageTimeOut(),
			Button1Action = continueAction,
			TimeOutAction = timeOutAction,
			ActorTag = "ViewSwipedCardIsChipEnabled",
			AnalyticsContextList = new List<string> { "swipedCardIsChipEnabled" }
		});
	}

	public static void ShowMessagePopupViewSwipedCardIsChipEnabled(Action timeOutAction, Action continueAction)
	{
		LogHelper.Instance.Log("Showing Swiped Card is chip enabled message popup");
		ShowModal(new ModalParameters(ModalType.Error)
		{
			TitleText = Resources.swiped_card_is_chip_enabled_title,
			MessageText = Resources.swiped_card_is_chip_enabled_message,
			Button1Text = Resources.got_it_button.ToUpper(),
			TimeOut = GetDefaultMessageTimeOut(),
			Button1Action = continueAction,
			TimeOutAction = timeOutAction,
			ActorTag = "ViewSwipedCardIsChipEnabled",
			AnalyticsContextList = new List<string> { "swipedCardIsChipEnabled" }
		});
	}

	public static void ShowMessagePopupViewBadChipReadError(Action timeOutAction, Action continueAction)
	{
		LogHelper.Instance.Log("Showing bad chip read error message popup");
		ShowModal(new ModalParameters(ModalType.Error)
		{
			TitleText = Resources.bad_chip_read_title,
			MessageText = Resources.bad_chip_read_message,
			Button1Text = Resources.got_it_button.ToUpper(),
			TimeOut = GetDefaultMessageTimeOut(),
			Button1Action = continueAction,
			TimeOutAction = timeOutAction,
			ActorTag = "ViewBadChipReadError",
			AnalyticsContextList = new List<string> { "badChipReadError" }
		});
	}

	public static void ShowMessagePopupViewBadContactlessReadError(Action timeOutAction, Action continueAction)
	{
		LogHelper.Instance.Log("Showing bad contactless read error message popup");
		ShowModal(new ModalParameters(ModalType.Error)
		{
			TitleText = Resources.bad_contactless_read_title,
			MessageText = Resources.bad_contactless_read_message,
			Button1Text = Resources.got_it_button.ToUpper(),
			TimeOut = GetDefaultMessageTimeOut(),
			Button1Action = continueAction,
			TimeOutAction = timeOutAction,
			ActorTag = "ViewBadContactlessReadError",
			AnalyticsContextList = new List<string> { "badContactlessReadError" }
		});
	}

	public static void ShowMessagePopupViewReservationMobileWalletError(Action timeOutAction, Action continueAction)
	{
		LogHelper.Instance.Log("Showing Reservation Mobile Wallet error message popup");
		ShowModal(new ModalParameters(ModalType.Error)
		{
			TitleText = Resources.reservation_mobile_device_error_title,
			MessageText = Resources.reservation_mobile_device_error_message,
			Button1Text = Resources.got_it_button.ToUpper(),
			TimeOut = GetDefaultMessageTimeOut(),
			Button1Action = continueAction,
			TimeOutAction = timeOutAction,
			ActorTag = "ViewReservationMobileWalletError",
			AnalyticsContextList = new List<string> { "reservationMobileWalletError" }
		});
	}

	public static void ShowMessagePopupViewCardNotRecognizedTechnicalFallback(Action timeOutAction, Action continueAction)
	{
		LogHelper.Instance.Log("Showing Card Not Recognized Technical Fallback message popup");
		ShowModal(new ModalParameters(ModalType.Error)
		{
			TitleText = Resources.technical_fallback_title,
			MessageText = Resources.technical_fallback_message,
			Button1Text = Resources.got_it_button.ToUpper(),
			TimeOut = GetDefaultMessageTimeOut(30000, "long"),
			Button1Action = continueAction,
			TimeOutAction = timeOutAction,
			ActorTag = "ViewCardNotRecognizedTechnicalFallback",
			AnalyticsContextList = new List<string> { "cardNotRecognizedTechnicalFallback" }
		});
	}

	public static void ShowMessagePopupViewGoogleWorkaroundError(Action timeOutAction, Action continueAction)
	{
		LogHelper.Instance.Log("Showing Google Workaround error message popup");
		ShowModal(new ModalParameters(ModalType.Error)
		{
			TitleText = Resources.google_workaround_error_title,
			MessageText = Resources.google_workaround_error_message,
			Button1Text = Resources.okay.ToUpper(),
			TimeOut = GetDefaultMessageTimeOut(),
			Button1Action = continueAction,
			TimeOutAction = timeOutAction,
			ActorTag = "ViewGoogleWorkaroundError",
			AnalyticsContextList = new List<string> { "googleWorkaroundError" }
		});
	}

	public static void ShowMessagePopupViewGoogleNoPassFoundError(Action timeOutAction, Action continueAction)
	{
		LogHelper.Instance.Log("Showing Google No Pass Found error message popup");
		ShowModal(new ModalParameters(ModalType.Error)
		{
			TitleText = Resources.mobile_pass_login_google_no_pass_found_error_header,
			MessageText = Resources.mobile_pass_login_google_no_pass_found_error_message,
			Button1Text = Resources.got_it_button.ToUpper(),
			TimeOut = GetDefaultMessageTimeOut(30000, "long"),
			Button1Action = continueAction,
			TimeOutAction = timeOutAction,
			ActorTag = "ViewGoogleNoPassFoundError",
			ContentUserControl = new QRCodeDisplayUserControl
			{
				DataContext = new
				{
					QRCodeImage = ServiceLocator.Instance.GetService<IKioskClientService>()?.QRCodeUtilities?.GetQRCodeImage(Resources.mobile_pass_login_qr_code_url, returnDefaultOnError: true),
					MessageStyle = WPFFunctions.GetStyle(BaseViewController.GetViewUserControl(), "modal_qr_code_display_user_control_no_message_message_style"),
					MessageText = string.Empty
				}
			},
			AnalyticsContextList = new List<string> { "googleNoPassFoundError" }
		});
	}

	public static void ShowMessagePopupViewMobilePaymentCardNoPassFoundError(Action timeOutAction, Action continueAction)
	{
		LogHelper.Instance.Log("Showing Mobile Payment Card No Pass Found error message popup");
		ShowModal(new ModalParameters(ModalType.Error)
		{
			TitleText = Resources.mobile_payment_card_no_pass_found_error_title,
			MessageText = Resources.mobile_payment_card_no_pass_found_error_message,
			Button1Text = Resources.got_it_button.ToUpper(),
			TimeOut = GetDefaultMessageTimeOut(),
			Button1Action = continueAction,
			TimeOutAction = timeOutAction,
			ActorTag = "ViewMobilePaymentCardNoPassFoundError",
			AnalyticsContextList = new List<string> { "mobilePaymentCardNoPassFoundError" }
		});
	}

	public static void ShowPerksPassLoginErrorMessage(string title, string message, Action continueAction, Action timeOutAction)
	{
		LogHelper.Instance.Log("Showing Perks Pass login error message");
		ShowModal(new ModalParameters(ModalType.Error)
		{
			TitleText = title,
			MessageText = message,
			Button1Text = Resources.got_it_button.ToUpper(),
			Button1Action = continueAction,
			TimeOutAction = timeOutAction,
			AnalyticsContextList = new List<string> { "PerksPassLoginErrorMessage" }
		});
	}

	public static void ShowPerksPassLoginDialogMessageAndQRCode(Action continueAction, Action timeOutAction)
	{
		LogHelper.Instance.Log("Showing Perks Pass login dialog message and QR code");
		ShowModal(new ModalParameters(ModalType.Informational)
		{
			TitleText = Resources.mobile_pass_login_get_pass_header,
			MessageText = Resources.mobile_pass_login_get_pass_message,
			Button1Text = Resources.got_it_button.ToUpper(),
			TimeOut = GetDefaultMessageTimeOut(30000, "long"),
			Button1Action = continueAction,
			TimeOutAction = timeOutAction,
			ActorTag = "PerksPassLoginDialogMessageAndQACode",
			ContentUserControl = new QRCodeDisplayUserControl
			{
				DataContext = new
				{
					QRCodeImage = ServiceLocator.Instance.GetService<IKioskClientService>()?.QRCodeUtilities?.GetQRCodeImage(Resources.mobile_pass_login_qr_code_url, returnDefaultOnError: true),
					MessageStyle = WPFFunctions.GetStyle(BaseViewController.GetViewUserControl(), "modal_qr_code_display_user_control_no_message_message_style"),
					MessageText = string.Empty
				}
			},
			AnalyticsContextList = new List<string> { "PerksPassLoginDialogMessageAndQRCode" }
		});
	}

	public static void ShowMessagePopupViewDigitalCode(string price, Action backAction, Action buyAction, Action timeOutAction)
	{
		LogHelper.Instance.Log("Showing Digital Code message popup");
		ShowModal(new ModalParameters(ModalType.Dialog)
		{
			TitleText = Resources.digital_code_popup_title,
			MessageText = Resources.digital_code_popup_message,
			Button1Text = Resources.back_uppercase_button,
			Button2Text = string.Format(Resources.digital_code_popup_buy_button, price),
			TimeOut = GetDefaultMessageTimeOut(),
			Button1Action = backAction,
			Button2Action = buyAction,
			TimeOutAction = timeOutAction,
			ActorTag = "ViewDigitalCode",
			ContentUserControl = new DigitalCodePopupUserControl
			{
				DataContext = new
				{
					MessageLine1 = Resources.digital_code_popup_message_line_1,
					MessageLine2 = Resources.digital_code_popup_message_line_2,
					MessageLine3 = Resources.digital_code_popup_message_line_3
				}
			},
			AnalyticsContextList = new List<string> { "whatdigitalcode_message_view" }
		});
	}

	public static void ShowQRCodeDialog(BitmapImage qrCodeImage, string url, Action backAction, Action timeOutAction)
	{
		LogHelper.Instance.Log("Showing QR Code message popup");
		ShowModal(new ModalParameters(ModalType.Informational)
		{
			TitleText = Resources.qr_code_dialog_title,
			Button1Text = Resources.done_button,
			TimeOut = GetDefaultMessageTimeOut(),
			Button1Action = backAction,
			TimeOutAction = timeOutAction,
			ActorTag = "ViewQRCodeDialog",
			ContentUserControl = new QRCodeDisplayUserControl
			{
				DataContext = new
				{
					QRCodeImage = qrCodeImage,
					MessageStyle = WPFFunctions.GetStyle(BaseViewController.GetViewUserControl(), "modal_qr_code_display_user_control_message_message_style"),
					MessageText = string.Format(Resources.qr_code_dialog_message, url)
				}
			},
			AnalyticsContextList = new List<string> { "qr_code_dialog_view" }
		});
	}

	public static void ShowForgotPasswordModal(Action timeOutAction, Action continueAction)
	{
		LogHelper.Instance.Log("Showing Forgot Password modal");
		ShowModal(new ModalParameters(ModalType.Informational)
		{
			TitleText = Resources.forgot_password_modal_title,
			MessageText = Resources.forgot_password_modal_message,
			Button1Text = Resources.done_button,
			TimeOutAction = timeOutAction,
			Button1Action = continueAction,
			ActorTag = "ForgotPasswordModal",
			ContentUserControl = new QRCodeDisplayUserControl
			{
				DataContext = new
				{
					QRCodeImage = ServiceLocator.Instance.GetService<IKioskClientService>()?.QRCodeUtilities?.GetQRCodeImage(Resources.forgot_password_modal_url, returnDefaultOnError: true),
					MessageStyle = WPFFunctions.GetStyle(BaseViewController.GetViewUserControl(), "modal_qr_code_display_user_control_message_message_style"),
					MessageText = string.Format(Resources.forgot_password_modal_qr_message, Resources.forgot_password_modal_url)
				}
			},
			AnalyticsContextList = new List<string> { "ForgotPasswordModal" }
		});
	}

	public static void ShowReserveOnlineRentDialog(Action timeOutAction, Action backAction, Action continueAction)
	{
		LogHelper.Instance.Log("Showing Reserve Online message popup");
		ShowModal(new ModalParameters(ModalType.Dialog)
		{
			TitleText = Resources.reserve_online_title,
			MessageText = Resources.reserve_online_text,
			Button1Text = Resources.back_button.ToUpper(),
			Button2Text = Resources.got_it_button.ToUpper(),
			TimeOut = GetDefaultMessageTimeOut(),
			Button1Action = backAction,
			Button2Action = continueAction,
			TimeOutAction = timeOutAction,
			ActorTag = "ReserveOnlineRentDialog",
			AnalyticsContextList = new List<string> { "ReserveOnlineRentDialog" }
		});
	}

	public static void ShowMachineFullWarning(Action timeOutAction, Action continueAction)
	{
		OrchestrationService.SetCurrentUICulture();
		LogHelper.Instance.Log("Showing machine full warning");
		ShowModal(new ModalParameters(ModalType.Error)
		{
			TitleText = Resources.machine_full_title,
			MessageText = Resources.machine_full_message,
			Button1Text = Resources.got_it_button.ToUpper(),
			TimeOut = GetDefaultMessageTimeOut(),
			Button1Action = continueAction,
			TimeOutAction = timeOutAction,
			ActorTag = "MachineFullWarning",
			AnalyticsContextList = new List<string> { "machineFullWarning" }
		});
	}

	public static void ShowBrowseMessagePopupLearnMorePopup(Action timeOutAction, Action continueAction)
	{
		LogHelper.Instance.Log("Showing Browse Message Popup Learn More Popup");
		ShowModal(new ModalParameters(ModalType.Informational)
		{
			TitleText = Resources.browse_message_popup_learn_more_popup_title,
			MessageText = Resources.browse_message_popup_learn_more_popup_message,
			Button1Text = Resources.close_button.ToUpper(),
			TimeOutAction = timeOutAction,
			Button1Action = continueAction,
			ViewName = "browse_message_popup_learn_more_popup_view",
			AnalyticsContextList = new List<string> { "browse_message_popup_learn_more_popup_view" }
		});
	}

	public static void ShowReservationNotFoundView(Action timeOutAction, Action continueAction)
	{
		LogHelper.Instance.Log("Showing ShowReservationNotFoundView");
		ShowModal(new ModalParameters(ModalType.Error)
		{
			TitleText = Resources.reservation_not_found_view_title_text,
			MessageText = Resources.reservation_not_found_view_message_text,
			Button1Text = Resources.got_it_button.ToUpper(),
			TimeOut = GetDefaultMessageTimeOut(),
			Button1Action = continueAction,
			TimeOutAction = timeOutAction,
			ActorTag = "ShowReservationNotFoundView",
			AnalyticsContextList = new List<string> { "ShowReservationNotFoundView" },
			ViewName = "ShowReservationNotFoundView"
		});
	}

	public static void ShowAuthorizeAtPickupOfflineMessage(Action timeOutAction, Action backAction, Action continueAction)
	{
		LogHelper.Instance.Log("Showing ShowAuthorizeAtPickupOfflineMessage");
		ShowModal(new ModalParameters(ModalType.Error)
		{
			TitleText = Resources.authorize_at_pickup_offline_header,
			MessageText = Resources.authorize_at_pickup_offline_message,
			Button1Text = Resources.close_button.ToUpper(),
			Button2Text = Resources.see_all_movies_button.ToUpper(),
			TimeOut = GetDefaultMessageTimeOut(),
			Button1Action = backAction,
			Button2Action = continueAction,
			TimeOutAction = timeOutAction,
			ActorTag = "ShowAuthorizeAtPickupOfflineMessage",
			AnalyticsContextList = new List<string> { "ShowAuthorizeAtPickupOfflineMessage" },
			ViewName = "ShowAuthorizeAtPickupOfflineMessage"
		});
	}

	public static void ShowDualInStockLearnMorePopup(Action timeOutAction, Action continueAction)
	{
		LogHelper.Instance.Log("Showing Dual In Stock Learn More Popup");
		ShowModal(new ModalParameters(ModalType.Informational)
		{
			TitleText = Resources.dual_in_stock_learn_more_popup_title,
			MessageText = Resources.dual_in_stock_learn_more_popup_message,
			Button1Text = Resources.close_button.ToUpper(),
			TimeOutAction = timeOutAction,
			Button1Action = continueAction,
			ViewName = "dual_in_stock_learn_more_popup_view",
			AnalyticsContextList = new List<string> { "dual_in_stock_learn_more_popup_view" }
		});
	}

	public static void HideMessagePopupView(bool popOnly)
	{
		IViewService service = ServiceLocator.Instance.GetService<IViewService>();
		if (service != null && service.Peek() == "message_popup_view")
		{
			MessagePopupViewStopTimer();
			service.GoBackSafe("messageviewmanager:hide()", popOnly);
		}
	}

	public static bool IsMessagePopupViewShowing()
	{
		IViewService service = ServiceLocator.Instance.GetService<IViewService>();
		if (service != null)
		{
			return service.Peek() == "message_popup_view";
		}
		return false;
	}

	public static void MessagePopupViewStopTimer()
	{
		IActor actor = ActorFunctions.GetActor("MessagePopupViewUserControl");
		if (actor != null && actor.WPFFrameworkElement is MessagePopupViewUserControl messagePopupViewUserControl)
		{
			messagePopupViewUserControl.StopTimer();
		}
	}

	public static void ReenableTimer()
	{
		((MessagePopupViewUserControl)BaseViewController.GetViewUserControl())?.ReenableTimer();
	}

	public static void ShowMessagePopupView(ShowMessagePopupViewParameter parameter)
	{
		OrchestrationService.ViewShow("message_popup_view", parameter);
		if (!string.IsNullOrWhiteSpace(parameter?.ActorTag))
		{
			BaseViewController.AnalyticsService?.AddEventProperty("ShowMessageActorTag", parameter.ActorTag);
		}
	}

	public static void ShowCriticalErrorView(ShowMessagePopupViewParameter parameter)
	{
		OrchestrationService.ShowCriticalErrorView("message_popup_view", parameter);
	}

	public static void LeaveMessagePopupView(IViewFrameInstance viewFrameInstance)
	{
		MessagePopupViewUserControl messagePopupViewUserControl = (MessagePopupViewUserControl)BaseViewController.GetViewUserControl(viewFrameInstance.Id);
		if (messagePopupViewUserControl != null && messagePopupViewUserControl.DataContext is MessagePopupViewModel messagePopupViewModel)
		{
			messagePopupViewModel.BackgroundOpacity = 0.0;
		}
	}

	public static void ConfigureMessagePopupView(ShowMessagePopupViewParameter parameters)
	{
		if (parameters.AnalyticsContextList != null)
		{
			foreach (string item in parameters.AnalyticsContextList.Where((string c) => !string.IsNullOrWhiteSpace(c)))
			{
				_ = item;
			}
		}
		Thickness thickness = default(Thickness);
		MessagePopupViewUserControl messagePopupViewUserControl = (MessagePopupViewUserControl)BaseViewController.GetViewUserControl();
		if (messagePopupViewUserControl != null)
		{
			MessagePopupViewModel messagePopupViewModel = (MessagePopupViewModel)(messagePopupViewUserControl.DataContext = new MessagePopupViewModel
			{
				Title = parameters.TitleText,
				Message = parameters.MessageText,
				BackgroundOpacity = 0.6,
				TitleStyle = WPFFunctions.GetStyle(messagePopupViewUserControl, parameters.TitleStyle ?? "message_popup_dialog_title_style"),
				MessageStyle = WPFFunctions.GetStyle(messagePopupViewUserControl, parameters.MessageStyle ?? "message_popup_dialog_mesg_style"),
				Button1Text = parameters.Button1Text,
				Button1TextStyle = WPFFunctions.GetStyle(messagePopupViewUserControl, parameters.Button1TextStyle ?? "button1_text_style"),
				Button1Style = WPFFunctions.GetStyle(messagePopupViewUserControl, parameters.Button1Style ?? "button1_style"),
				Button1Visibility = ((Redbox.Rental.Services.Session.Session.Instance.IsInZeroTouchMode || string.IsNullOrEmpty(parameters.Button1Text)) ? Visibility.Collapsed : Visibility.Visible),
				Button2Text = parameters.Button2Text,
				Button2TextStyle = WPFFunctions.GetStyle(messagePopupViewUserControl, parameters.Button2TextStyle ?? "button2_text_style"),
				Button2Style = WPFFunctions.GetStyle(messagePopupViewUserControl, parameters.Button2Style ?? "button2_style"),
				ButtonGridStyle = WPFFunctions.GetStyle(messagePopupViewUserControl, parameters.ButtonGridStyle ?? "message_popup_dialog_button_grid_style"),
				Button2Visibility = ((Redbox.Rental.Services.Session.Session.Instance.IsInZeroTouchMode || string.IsNullOrEmpty(parameters.Button2Text)) ? Visibility.Collapsed : Visibility.Visible),
				InnerGridStyle = WPFFunctions.GetStyle(messagePopupViewUserControl, parameters.InnerGridStyle ?? null),
				FloatingButtonGridStyle = WPFFunctions.GetStyle(messagePopupViewUserControl, parameters.FloatingButtonGridStyle ?? "message_popup_dialog_floating_button_grid_style"),
				MessageDockPanelStyle = WPFFunctions.GetStyle(messagePopupViewUserControl, parameters.MessageDockPanelStyle ?? "message_popup_dialog_message_dock_panel_style"),
				TimeOut = (Redbox.Rental.Services.Session.Session.Instance.IsInZeroTouchMode ? 10000 : parameters.Timeout),
				OnButton1Clicked = delegate
				{
					BaseViewController.AnalyticsService?.AddButtonPressEvent("MessagePopupButton1", parameters.Button1Text);
					parameters.Button1Action?.Invoke();
				},
				OnButton2Clicked = delegate
				{
					BaseViewController.AnalyticsService?.AddButtonPressEvent("MessagePopupButton2", parameters.Button2Text);
					parameters.Button2Action?.Invoke();
				},
				OnTimeOut = parameters.TimeOutAction,
				ContentUserControl = parameters.ContentUserControl,
				BorderBrushColor = (string.IsNullOrEmpty(parameters.BorderBrushColor) ? "#E42045" : parameters.BorderBrushColor),
				MainGridMargin = (parameters.MainGridMargin.Equals(thickness) ? new Thickness(128.0, 96.0, 128.0, 96.0) : parameters.MainGridMargin),
				FixedSizeGridVisibility = (parameters.IsAutoSized ? Visibility.Collapsed : Visibility.Visible),
				AutoSizedGridVisibility = ((!parameters.IsAutoSized) ? Visibility.Collapsed : Visibility.Visible),
				ButtonGridSeparatorLineVisibility = parameters.ButtonGridSeparatorLineVisibility
			});
			LogHelper.Instance.Log("Setting MessagePopupView: Title:{0} Message:{1}", parameters.TitleText, parameters.MessageText);
			ConfigureTTS(messagePopupViewModel);
			if (Redbox.Rental.Services.Session.Session.Instance.IsInZeroTouchMode)
			{
				ConfigureCountdownTimer(messagePopupViewModel);
				return;
			}
			messagePopupViewModel.CountdownTimerText = string.Empty;
			messagePopupViewModel.CountdownTimerGridVisibility = Visibility.Collapsed;
		}
	}

	private static void ConfigureTTS(MessagePopupViewModel viewModel)
	{
		ITextToSpeechService service = ServiceLocator.Instance.GetService<ITextToSpeechService>();
		if (service != null && service.TTSEnabled)
		{
			viewModel.OnGetSpeechControl = () => MessagePopupViewTextToSpeech.GetSpeechControl(GetViewModel);
		}
	}

	private static void ConfigureCountdownTimer(MessagePopupViewModel messagePopupViewModel)
	{
		if (messagePopupViewModel.TimeOut <= 0)
		{
			return;
		}
		int secondsRemaining = messagePopupViewModel.TimeOut / 1000;
		messagePopupViewModel.CountdownTimerText = secondsRemaining.ToString("0:00");
		messagePopupViewModel.CountdownTimerGridVisibility = Visibility.Visible;
		ITimerService timerService = ServiceLocator.Instance.GetService<ITimerService>();
		(timerService?.CreateTimer("MessagePopupCountdownTimer", 1000, 1000, delegate
		{
			secondsRemaining--;
			messagePopupViewModel.CountdownTimerText = secondsRemaining.ToString("0:00");
			if (secondsRemaining <= 1)
			{
				(timerService?.GetTimer("MessagePopupCountdownTimer"))?.Stop();
				timerService?.RemoveTimer("MessagePopupCountdownTimer");
			}
		}))?.Start();
	}

	private static MessagePopupViewModel GetViewModel()
	{
		FrameworkElement viewUserControl = BaseViewController.GetViewUserControl();
		if (viewUserControl != null)
		{
			return viewUserControl.DataContext as MessagePopupViewModel;
		}
		return null;
	}

	private static int GetDefaultMessageTimeOut()
	{
		return GetDefaultMessageTimeOut(15000, "medium");
	}

	private static int GetDefaultMessageTimeOut(int defaultTimeout, string timeoutType)
	{
		return OrchestrationService.TTSGetTimeout(timeoutType, defaultTimeout);
	}
}
