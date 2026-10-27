using System;
using System.Windows;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model.Personalization;
using Redbox.Rental.Model.ShoppingCart;
using Redbox.Rental.Services.Configuration;
using Redbox.Rental.Services.Session;
using Redbox.Rental.UI;
using Redbox.Rental.UI.ControllersLogic;
using Redbox.Rental.UI.Models;
using Redbox.Rental.UI.Properties;
using Redbox.Rental.UI.Views;

namespace Redbox.Rental.Services.Controllers;

public class ApplyPromoViewController : BaseViewController
{
	public static void ConfigureApplyPromoView(ApplyPromoInfo info)
	{
		BaseViewController.StopIdleTimer();
		FrameworkElement viewUserControl = BaseViewController.GetViewUserControl();
		if (viewUserControl != null)
		{
			ApplyPromoViewModel applyPromoViewModel = GetViewModel();
			if (applyPromoViewModel == null)
			{
				applyPromoViewModel = new ApplyPromoViewModel();
			}
			OrchestrationService.SetCurrentUICulture();
			applyPromoViewModel.CancelButtonText = Resources.perks_cancel_btn_text;
			applyPromoViewModel.EnterPromoButtonText = Resources.apply_promo_enter_promo_button_text;
			applyPromoViewModel.EnterPromoMessage = Resources.apply_promo_enter_promo_message;
			applyPromoViewModel.TitleText = Resources.apply_promo_title;
			applyPromoViewModel.TopMessage = Resources.apply_promo_message_text;
			PromoListLogic promoListLogic = (applyPromoViewModel.PromoLogic = new PromoListLogic(applyPromoViewModel));
			applyPromoViewModel.CancelCommand = DynamicRouting.RegisterRoutedCommand(new DynamicRoutedCommand(), CancelCommand_Execute);
			applyPromoViewModel.EnterPromoCommand = DynamicRouting.RegisterRoutedCommand(new DynamicRoutedCommand(), (Action)delegate
			{
				EnterPromoCommand_Execute(info.ApplyPromoCodeAction);
			}, (Func<bool>)null);
			applyPromoViewModel.NextPagePressedCommand = DynamicRouting.RegisterRoutedCommand(new DynamicRoutedCommand(), promoListLogic.AddNextPagePressedAnalytics);
			applyPromoViewModel.PreviousPagePressedCommand = DynamicRouting.RegisterRoutedCommand(new DynamicRoutedCommand(), promoListLogic.AddPreviousPagePressedAnalytics);
			applyPromoViewModel.PageNumberPressedCommand = DynamicRouting.RegisterRoutedCommand(new DynamicRoutedCommand(), promoListLogic.AddPageNumberPressedAnalytics);
			applyPromoViewModel.AddOrRemovePromoCommand = DynamicRouting.RegisterRoutedCommand(new DynamicRoutedCommand(), delegate(StoredPromoCodeModel obj)
			{
				AddOrRemovePromoCommand_Execute(obj, info.ApplyPromoCodeAction);
			});
			applyPromoViewModel.OnLoadedCommand = DynamicRouting.RegisterRoutedCommand(new DynamicRoutedCommand(), (Action)delegate
			{
			}, (Func<bool>)null);
			UpdateOfferList(applyPromoViewModel);
			applyPromoViewModel.PromosPerPage = 3;
			applyPromoViewModel.CurrentPageNumber = PromoListLogic.FindPageOfSelectedPromo(applyPromoViewModel);
			viewUserControl.DataContext = applyPromoViewModel;
			ConfigureTTS(applyPromoViewModel);
		}
		BaseViewController.StartIdleTimer();
	}

	private static void CancelCommand_Execute()
	{
		BaseViewController.ProcessWPFHit();
		BaseViewController.AnalyticsService?.AddButtonPressEvent("ApplyPromoCancel");
		OrchestrationService.ViewGoBackSafe("ApplyPromoView cancel", popOnly: false);
	}

	internal static void LeaveApplyPromoView()
	{
	}

	private static void UpdateOfferList(ApplyPromoViewModel viewModel)
	{
		IPersonalizationSession personalizationSession = Redbox.Rental.Services.Session.Session.Instance?.PersonalizationSession;
		string activePromoCode = string.Empty;
		if (BaseViewController.RentalShoppingCartService?.CurrentRentalShoppingCart != null)
		{
			activePromoCode = BaseViewController.RentalShoppingCartService.CurrentRentalShoppingCart.Promotion?.Name;
		}
		PromoListLogic.CreateOfferListItemModels(viewModel, personalizationSession, activePromoCode, 'X', includeOffers: false, Redbox.Rental.Services.Configuration.Configuration.Instance.KioskSession.MarketingGeneral.EnableRedboxPlusRedemption);
	}

	private static void AddOrRemovePromoCommand_Execute(StoredPromoCodeModel model, Action<string> applyPromoAction)
	{
		BaseViewController.ProcessWPFHit();
		model.Logic.AddRemovePromoButtonPressedAnalytics(model);
		if (!model.IsAdded)
		{
			if (model != null && model.IsRedboxPlusPromo)
			{
				AddRedboxPlusFreeOneNightRentalDiscount();
			}
			else
			{
				ValidateStoredPromoWillNotConflictWithRedboxPlusBenefit(model, applyPromoAction);
			}
			return;
		}
		if (model != null && model.IsRedboxPlusPromo)
		{
			ClearRedboxPlusDiscount(model);
			return;
		}
		if (BaseViewController.RentalShoppingCartService?.CurrentRentalShoppingCart != null)
		{
			BaseViewController.RentalShoppingCartService?.CurrentRentalShoppingCart.ClearPromoCode();
			BaseViewController.RentalShoppingCartService?.CurrentRentalShoppingCart.ClearLoyaltyPointsToBeEarned();
		}
		OrchestrationService.ViewGoBackSafe("ApplyPromoView AddOrRemovePromoSelected", popOnly: false);
	}

	private static void ValidateStoredPromoWillNotConflictWithRedboxPlusBenefit(StoredPromoCodeModel model, Action<string> applyPromoAction)
	{
		RedboxPlusFunctions.ValidatePromoWillNotConflictWithRedboxPlusBenefit(delegate
		{
			OrchestrationService.ViewGoBackSafe("ApplyPromoView DeclinedStoredPromo because of Redbox+ conflict.", popOnly: false);
		}, delegate
		{
			OrchestrationService.ViewGoBackSafe("ApplyPromoView StoredPromoSelected", popOnly: false);
			ConfirmPromo(model, applyPromoAction);
		}, delegate
		{
			ConfirmPromo(model, applyPromoAction);
		});
	}

	private static void ConfirmPromo(StoredPromoCodeModel model, Action<string> applyPromoAction)
	{
		PromoConfirmationInfo promoConfirmationInfo = new PromoConfirmationInfo();
		promoConfirmationInfo.promoCodeModel = model;
		promoConfirmationInfo.ApplyPromoCodeAction = delegate(string p)
		{
			OrchestrationService.ViewGoBackSafe("ApplyPromoView AddOrRemovePromoSelected", popOnly: false);
			applyPromoAction(p);
		};
		promoConfirmationInfo.CancelAction = delegate
		{
		};
		OrchestrationService.ViewShow("promo_confirmation_view", promoConfirmationInfo);
	}

	private static void ClearRedboxPlusDiscount(StoredPromoCodeModel model)
	{
		bool flag = model?.IsRedboxPlusPromo ?? false;
		if (flag)
		{
			RedboxPlusFunctions.ClearFreeOneNightRental();
		}
		OrchestrationService.UnwindToSafe("shopping_cart_view", discard: true, flag ? "ApplyPromoView Removed Redbox+ Free 1 night rental discount" : "ApplyPromoView Removed Redbox+ purchase discount");
	}

	private static void AddRedboxPlusFreeOneNightRentalDiscount()
	{
		IRentalShoppingCartTitleItem bestFreeOneNightRentalEligibleItem = RedboxPlusFunctions.GetBestFreeOneNightRentalEligibleItem();
		if (bestFreeOneNightRentalEligibleItem == null)
		{
			string contextMessage = "ApplyPromoView failed to add Redbox+ Free 1 night rental discount.  No eligible items in cart.";
			LogHelper.Instance.Log(contextMessage);
			Action action = delegate
			{
				OrchestrationService.ViewGoBackSafe(contextMessage, popOnly: false);
			};
			MessagePopupViewController.ShowMissingRedboxPlusRentalItemMessage(action, action, action);
		}
		else
		{
			RedboxPlusFunctions.ValidateAndSetFreeOneNightRental(bestFreeOneNightRentalEligibleItem, showConfirmationMessage: true, "shopping_cart_view");
		}
	}

	private static void EnterPromoCommand_Execute(Action<string> applyPromoAction)
	{
		BaseViewController.ProcessWPFHit();
		BaseViewController.AnalyticsService?.AddButtonPressEvent("ApplyPromoEnterPromo");
		Action showEnterPromoCodeKeyboardScreen = delegate
		{
			OrchestrationService.ViewShow("keyboard_promo_view", new KeyboardInfo
			{
				MaxLength = 31,
				KeyboardText = "",
				IsValidEntry = ValidationServices.IsValidPromoCode,
				CancelAction = delegate
				{
					OrchestrationService.ViewGoBack("go back", popOnly: false);
				},
				ContinueAction = applyPromoAction
			});
		};
		RedboxPlusFunctions.ValidatePromoWillNotConflictWithRedboxPlusBenefit(delegate
		{
			OrchestrationService.ViewGoBackSafe("ApplyPromoView DeclinedEnterPromo because of Redbox+ conflict.", popOnly: false);
		}, delegate
		{
			OrchestrationService.ViewGoBackSafe("ApplyPromoView EnterPromoSelected", popOnly: false);
			showEnterPromoCodeKeyboardScreen();
		}, showEnterPromoCodeKeyboardScreen);
	}

	private static void ConfigureTTS(ApplyPromoViewModel viewModel)
	{
		ITextToSpeechService service = ServiceLocator.Instance.GetService<ITextToSpeechService>();
		if (service != null && service.TTSEnabled)
		{
			viewModel.OnGetSpeechControl = () => ApplyPromoViewTextToSpeech.GetSpeechControl(GetViewModel);
		}
	}

	private static ApplyPromoViewModel GetViewModel()
	{
		FrameworkElement viewUserControl = BaseViewController.GetViewUserControl();
		if (viewUserControl != null)
		{
			return viewUserControl.DataContext as ApplyPromoViewModel;
		}
		return null;
	}
}
