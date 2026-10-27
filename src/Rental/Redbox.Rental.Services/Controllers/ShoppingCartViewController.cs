using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.KioskServices;
using Redbox.Rental.Model;
using Redbox.Rental.Model.Analytics;
using Redbox.Rental.Model.DataService;
using Redbox.Rental.Model.KioskClientService;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Model.Loyalty;
using Redbox.Rental.Model.Personalization;
using Redbox.Rental.Model.Pricing;
using Redbox.Rental.Model.Promotion;
using Redbox.Rental.Model.Session;
using Redbox.Rental.Model.ShoppingCart;
using Redbox.Rental.Services.Configuration;
using Redbox.Rental.Services.KioskProduct;
using Redbox.Rental.Services.Loyalty;
using Redbox.Rental.Services.Session;
using Redbox.Rental.UI;
using Redbox.Rental.UI.Models;
using Redbox.Rental.UI.Properties;
using Redbox.Rental.UI.Views;

namespace Redbox.Rental.Services.Controllers;

public class ShoppingCartViewController : BaseLoyaltyController
{
	private static readonly object _syncObject = new object();

	private static bool HasActiveRedboxPlusSubscription => (Redbox.Rental.Services.Session.Session.Instance?.PersonalizationSession)?.RedboxPlusSession?.HasActiveSubscription == true;

	private static IBrowseState BrowseState => Redbox.Rental.Services.Session.Session.Instance.BrowseState;

	private static IIdleTimerService IdleTimerService => ServiceLocator.Instance.GetService<IIdleTimerService>();

	private static IPricingService PricingService => ServiceLocator.Instance.GetService<IPricingService>();

	private static IKioskClientService KioskClientService => ServiceLocator.Instance.GetService<IKioskClientService>();

	public static void ConfigureShoppingCartView()
	{
		FrameworkElement viewUserControl = BaseViewController.GetViewUserControl();
		if (viewUserControl != null)
		{
			if (BrowseState != null)
			{
				BrowseState.ItemAddedFromView = null;
			}
			OrchestrationService.ResetApplicationIdleTimerTimeout();
			ShoppingCartViewModel shoppingCartViewModel = new ShoppingCartViewModel();
			if (BaseViewController.RentalShoppingCartService?.CurrentRentalShoppingCart != null)
			{
				BaseViewController.RentalShoppingCartService.RemoveMultiDiscDiscsFromRentalShoppingCart(BaseViewController.RentalShoppingCartService.CurrentRentalShoppingCart.ShoppingSessionId);
				BaseViewController.RentalShoppingCartService.RemovePromoOfferFromCart(BaseViewController.RentalShoppingCartService.CurrentRentalShoppingCart.ShoppingSessionId);
			}
			if (PersonalizationFunctions.IsLoyaltySignedIn && BaseViewController.RentalShoppingCartService?.CurrentRentalShoppingCart?.TitleItems?.Any() == true)
			{
				BaseLoyaltyController.GetEstimateAccruedPoints(ProcessEstimateAccruedPointsResult);
				BaseLoyaltyController.GetEstimateRedemptionPoints(ProcessEstimateRedemptionPointsResult);
			}
			lock (_syncObject)
			{
				ConfigureTextAndVisibility(shoppingCartViewModel);
				ConfigureCommands(shoppingCartViewModel);
				ConfigureShoppingCartDisplay(shoppingCartViewModel);
				ConfigureTTS(shoppingCartViewModel);
			}
			viewUserControl.DataContext = shoppingCartViewModel;
			CommandManager.InvalidateRequerySuggested();
			(IdleTimerService?.GetIdleTimer("idle_timer"))?.Start();
		}
	}

	private static void ProcessEstimateAccruedPointsResult(IEstimatePointsAccruedResult estimatePointsAccruedResult)
	{
		lock (_syncObject)
		{
			ShoppingCartViewModel shoppingCartViewModelFromView = GetShoppingCartViewModelFromView();
			if (shoppingCartViewModelFromView == null)
			{
				return;
			}
			if (estimatePointsAccruedResult != null && estimatePointsAccruedResult.Success)
			{
				IList<IRentalShoppingCartTitleItem> source = BaseViewController.RentalShoppingCartService?.CurrentRentalShoppingCart.TitleItems;
				foreach (IProductEstimatedPointsAccrued productEstimatedPointsAccrued in estimatePointsAccruedResult.ProductEstimatedPointsAccruedList)
				{
					IRentalShoppingCartTitleItem rentalShoppingCartItem = source.FirstOrDefault((IRentalShoppingCartTitleItem x) => x.ProductId == productEstimatedPointsAccrued.ProductId);
					if (rentalShoppingCartItem != null)
					{
						if (rentalShoppingCartItem.LoyaltyPointsRecord == null)
						{
							rentalShoppingCartItem.LoyaltyPointsRecord = new LoyaltyPointsRecord(rentalShoppingCartItem);
							rentalShoppingCartItem.LoyaltyPointsRecord.Clone(productEstimatedPointsAccrued.LoyaltyPointsRecord);
						}
						else
						{
							rentalShoppingCartItem.LoyaltyPointsRecord.PointsToBeEarned = productEstimatedPointsAccrued.LoyaltyPointsRecord.PointsToBeEarned;
						}
						DisplayCheckoutProductModel displayCheckoutProductModel = shoppingCartViewModelFromView.DisplayProductModels.FirstOrDefault((DisplayCheckoutProductModel x) => x.Data == rentalShoppingCartItem);
						if (displayCheckoutProductModel != null)
						{
							PopulateDisplayCheckoutProductModel(displayCheckoutProductModel, shoppingCartViewModelFromView);
						}
					}
				}
			}
			else
			{
				Redbox.Rental.Services.Session.Session.Instance?.PersonalizationSession?.LogOutLoyaltySession();
				BaseViewController.RentalShoppingCartService?.CurrentRentalShoppingCart.ClearAllLoyaltyPointsRecords();
			}
			UpdateLoyaltyStatus(shoppingCartViewModelFromView, BaseViewController.RentalShoppingCartService?.CurrentRentalShoppingCart);
		}
	}

	private static void ProcessEstimateRedemptionPointsResult(IEstimateRedemptionPointsResult estimateRedemptionPointsResult)
	{
		lock (_syncObject)
		{
			ShoppingCartViewModel shoppingCartViewModelFromView = GetShoppingCartViewModelFromView();
			if (shoppingCartViewModelFromView == null)
			{
				return;
			}
			if (estimateRedemptionPointsResult != null && estimateRedemptionPointsResult.Success)
			{
				IList<IRentalShoppingCartTitleItem> source = BaseViewController.RentalShoppingCartService?.CurrentRentalShoppingCart.TitleItems;
				foreach (IProductEstimatedRedemptionPoints productEstimatedRedemptionPoints in estimateRedemptionPointsResult.ProductEstimatedRedemptionPointsList)
				{
					IRentalShoppingCartTitleItem rentalShoppingCartItem = source.FirstOrDefault((IRentalShoppingCartTitleItem x) => x.ProductId == productEstimatedRedemptionPoints.ProductId);
					if (rentalShoppingCartItem != null)
					{
						if (rentalShoppingCartItem.LoyaltyPointsRecord == null)
						{
							rentalShoppingCartItem.LoyaltyPointsRecord = new LoyaltyPointsRecord(rentalShoppingCartItem);
							rentalShoppingCartItem.LoyaltyPointsRecord.Clone(productEstimatedRedemptionPoints.LoyaltyPointsRecord);
						}
						else
						{
							rentalShoppingCartItem.LoyaltyPointsRecord.PointsToRedeem = productEstimatedRedemptionPoints.LoyaltyPointsRecord.PointsToRedeem;
						}
						DisplayCheckoutProductModel displayCheckoutProductModel = shoppingCartViewModelFromView.DisplayProductModels.FirstOrDefault((DisplayCheckoutProductModel x) => x.Data == rentalShoppingCartItem);
						if (displayCheckoutProductModel != null)
						{
							PopulateDisplayCheckoutProductModel(displayCheckoutProductModel, shoppingCartViewModelFromView);
						}
					}
				}
			}
			else
			{
				Redbox.Rental.Services.Session.Session.Instance?.PersonalizationSession?.LogOutLoyaltySession();
				BaseViewController.RentalShoppingCartService?.CurrentRentalShoppingCart.ClearAllLoyaltyPointsRecords();
			}
			UpdateLoyaltyStatus(shoppingCartViewModelFromView, BaseViewController.RentalShoppingCartService?.CurrentRentalShoppingCart);
		}
	}

	private static void ConfigureShoppingCartDisplay(ShoppingCartViewModel shoppingCartViewModel)
	{
		shoppingCartViewModel.ADASpacerVisibility = ((!BaseViewController.IsADAMode) ? Visibility.Collapsed : Visibility.Visible);
		int addMovieButtonVisibility;
		if (!BaseViewController.IsADAMode)
		{
			IRentalShoppingCartService rentalShoppingCartService = BaseViewController.RentalShoppingCartService;
			if (rentalShoppingCartService != null && !rentalShoppingCartService.CurrentRentalShoppingCart.IsFull)
			{
				addMovieButtonVisibility = 0;
				goto IL_0035;
			}
		}
		addMovieButtonVisibility = 2;
		goto IL_0035;
		IL_0035:
		shoppingCartViewModel.AddMovieButtonVisibility = (Visibility)addMovieButtonVisibility;
		shoppingCartViewModel.DisplayProductModels = CreateDisplayProductModelsFromCartItems(shoppingCartViewModel);
	}

	private static List<DisplayCheckoutProductModel> CreateDisplayProductModelsFromCartItems(ShoppingCartViewModel shoppingCartViewModel)
	{
		List<DisplayCheckoutProductModel> list = new List<DisplayCheckoutProductModel>();
		IRentalShoppingCart rentalShoppingCart = BaseViewController.RentalShoppingCartService?.CurrentRentalShoppingCart;
		IDataService service = ServiceLocator.Instance.GetService<IDataService>();
		if (rentalShoppingCart != null && service != null)
		{
			foreach (IBaseRentalShoppingCartItem item in rentalShoppingCart.Items)
			{
				IKioskProduct kioskProduct = null;
				IRentalShoppingCartTitleItem rentalShoppingCartTitleItem = item as IRentalShoppingCartTitleItem;
				IRentalShoppingCartSubscriptionItem rentalShoppingCartSubscriptionItem = item as IRentalShoppingCartSubscriptionItem;
				if (rentalShoppingCartTitleItem != null)
				{
					service.GetProduct(rentalShoppingCartTitleItem.ProductId, excludeTitleRollup: true, out kioskProduct);
				}
				else if (rentalShoppingCartSubscriptionItem != null)
				{
					kioskProduct = rentalShoppingCartSubscriptionItem.SubscriptionProduct;
				}
				if (kioskProduct != null)
				{
					DisplayCheckoutProductModel displayCheckoutProductModel = new DisplayCheckoutProductModel
					{
						Name = kioskProduct.ProductName,
						Data = item,
						Image = OrchestrationService.GetProductBitmapImage(kioskProduct)
					};
					PopulateDisplayCheckoutProductModel(displayCheckoutProductModel, shoppingCartViewModel);
					list.Add(displayCheckoutProductModel);
				}
			}
		}
		return list;
	}

	private static void PopulateDisplayCheckoutProductModel(DisplayCheckoutProductModel displayCheckoutProductModel, ShoppingCartViewModel shoppingCartViewModel)
	{
		IRentalShoppingCartTitleItem rentalShoppingCartTitleItem = displayCheckoutProductModel?.Data as IRentalShoppingCartTitleItem;
		IRentalShoppingCartSubscriptionItem rentalShoppingCartSubscriptionItem = displayCheckoutProductModel?.Data as IRentalShoppingCartSubscriptionItem;
		if (rentalShoppingCartTitleItem != null)
		{
			displayCheckoutProductModel.ConfigureForCheckout();
			IDataService service = ServiceLocator.Instance.GetService<IDataService>();
			IKioskProduct kioskProduct = null;
			service.GetProduct(rentalShoppingCartTitleItem.ProductId, excludeTitleRollup: false, out kioskProduct);
			ITitleProduct titleProduct = kioskProduct as ITitleProduct;
			displayCheckoutProductModel.ProductTitleText = BrowseFunctions.GetProductDisplayName(titleProduct);
			MDVProduct mDVProduct = titleProduct as MDVProduct;
			displayCheckoutProductModel.MDVFieldVisibility = ((mDVProduct == null) ? Visibility.Collapsed : Visibility.Visible);
			displayCheckoutProductModel.MDVNumberOfDiscsText = ((mDVProduct != null) ? $"({mDVProduct.TitleProducts.Count} {Resources.multi_disc_vend_discs_text_all_caps})**" : null);
			displayCheckoutProductModel.MDVDiscVendText = ((mDVProduct != null) ? $"{mDVProduct.TitleProducts.Count} {Resources.disc_vend}" : null);
			TitleTypeProperty value = null;
			TitleTypeProperties.Properties.TryGetValue(titleProduct.TitleType, out value);
			string productTypeText = $" - {value?.GetName(Resources.ResourceManager)}";
			displayCheckoutProductModel.ProductTypeText = productTypeText;
			string text = null;
			if (rentalShoppingCartTitleItem.Action == RentalShoppingCartItemAction.purchase)
			{
				text = Resources.cart_purchase;
			}
			else
			{
				string arg = string.Format(Resources.night_rental, rentalShoppingCartTitleItem?.PricingRecord?.InitialDays ?? 1);
				string arg2 = $"{Resources.extra_night} {rentalShoppingCartTitleItem?.PricingRecord.ExtraNight:C}";
				text = $"{arg} | {arg2}";
			}
			string arg3 = null;
			string productDescriptionLine3Text = null;
			if (PersonalizationFunctions.IsLoyaltySignedIn)
			{
				if (rentalShoppingCartTitleItem.LoyaltyPointsRecord != null)
				{
					if (!rentalShoppingCartTitleItem.LoyaltyPointsRecord.ToBeRedeemed)
					{
						if (rentalShoppingCartTitleItem.LoyaltyPointsRecord.PointsToBeEarned.HasValue)
						{
							if (rentalShoppingCartTitleItem.LoyaltyPointsRecord.PointsToBeEarned != 0)
							{
								arg3 = string.Format(Resources.loyalty_earning_points, rentalShoppingCartTitleItem.LoyaltyPointsRecord.PointsToBeEarned);
							}
						}
						else
						{
							arg3 = Resources.loyalty_retrieving_points;
						}
					}
					else
					{
						productDescriptionLine3Text = string.Format(Resources.loyalty_redeeming_points, rentalShoppingCartTitleItem.LoyaltyPointsRecord.PointsToRedeem);
					}
				}
				else
				{
					arg3 = Resources.loyalty_retrieving_points;
				}
			}
			displayCheckoutProductModel.ProductDescriptionLine2Text = $"{text}{arg3}";
			displayCheckoutProductModel.ProductDescriptionLine3Text = productDescriptionLine3Text;
			displayCheckoutProductModel.ProductDescriptionLine3TextVisibility = (string.IsNullOrEmpty(displayCheckoutProductModel.ProductDescriptionLine3Text) ? Visibility.Collapsed : Visibility.Visible);
			displayCheckoutProductModel.ProductCartPriceText = $"{rentalShoppingCartTitleItem.Price:C}";
			displayCheckoutProductModel.RemoveButtonText = Resources.shopping_cart_remove_button_text;
			displayCheckoutProductModel.TopCancelButtonVisibility = (BaseViewController.IsADAMode ? Visibility.Collapsed : Visibility.Visible);
			displayCheckoutProductModel.BottomCancelButtonVisibility = ((!BaseViewController.IsADAMode) ? Visibility.Collapsed : Visibility.Visible);
			displayCheckoutProductModel.RedboxPlusMovieInfoVisibility = ((!Redbox.Rental.Services.Configuration.Configuration.Instance.KioskSession.MarketingGeneral.EnableRedboxPlusRedemption || !HasActiveRedboxPlusSubscription || titleProduct == null || !titleProduct.IsRedboxPlusEligible) ? Visibility.Collapsed : Visibility.Visible);
			displayCheckoutProductModel.RedboxPlusMovieInfoButtonText = Resources.redbox_plus_movie;
			displayCheckoutProductModel.RedboxPlusInfoCommand = shoppingCartViewModel?.RedboxPlusInfoCommand;
		}
		else if (rentalShoppingCartSubscriptionItem != null)
		{
			ISubscriptionProduct subscriptionProduct = rentalShoppingCartSubscriptionItem.SubscriptionProduct;
			IRedboxPlusSubscriptionProduct redboxPlusSubscriptionProduct = ((subscriptionProduct != null && subscriptionProduct.SubscriptionType == SubscriptionType.RedboxPlus) ? (rentalShoppingCartSubscriptionItem.SubscriptionProduct as IRedboxPlusSubscriptionProduct) : null);
			if (redboxPlusSubscriptionProduct != null)
			{
				displayCheckoutProductModel.ConfigureForCheckout();
				displayCheckoutProductModel.ProductTitleText = Resources.shopping_cart_redbox_plus_text;
				displayCheckoutProductModel.ProductDescriptionLine2Text = redboxPlusSubscriptionProduct.MyBagProductDescription;
				displayCheckoutProductModel.ProductCartPriceText = $"{rentalShoppingCartSubscriptionItem.Price:C}";
				displayCheckoutProductModel.RemoveButtonText = Resources.shopping_cart_remove_button_text;
				displayCheckoutProductModel.DetailsButtonText = Resources.shopping_cart_details_button_text;
				displayCheckoutProductModel.TopCancelButtonVisibility = Visibility.Visible;
				displayCheckoutProductModel.DetailsButtonVisibility = Visibility.Visible;
			}
		}
	}

	private static bool IsCurrentViewValid(bool logError = true)
	{
		return OrchestrationService.IsCurrentViewValid("shopping_cart_view");
	}

	private static void ConfigureCommands(ShoppingCartViewModel shoppingCartViewModel)
	{
		shoppingCartViewModel.OnBackButtonClicked += ShoppingCartViewModel_OnBackButtonClicked;
		shoppingCartViewModel.OnAddMovieButtonClicked += ShoppingCartViewModel_OnAddMovieButtonClicked;
		shoppingCartViewModel.OnSignInButtonClicked += ShoppingCartViewModel_OnSignInButtonClicked;
		shoppingCartViewModel.OnUsePointsButtonClicked += ShoppingCartViewModel_OnUsePointsButtonClicked;
		shoppingCartViewModel.OnPointsBalanceInfoButtonClicked += ShoppingCartViewModel_OnPointsBalanceInfoButtonClicked;
		shoppingCartViewModel.OnLoaded += ShoppingCartViewModel_OnLoaded;
		shoppingCartViewModel.OnTermsAndPrivacyButtonClicked += delegate
		{
			if (IsCurrentViewValid())
			{
				BaseViewController.AnalyticsService?.AddButtonPressEvent("ShoppingCartTermsAndPrivacy");
				HelpViewController.ShowTermsAndConditions();
			}
		};
		if (((Redbox.Rental.Services.Session.Session.Instance?.PersonalizationSession)?.StoredPromoCodes?.Count).GetValueOrDefault() == 0)
		{
			shoppingCartViewModel.OnPromoCodeButtonClicked += delegate
			{
				BaseViewController.AnalyticsService?.AddButtonPressEvent("ShoppingCartAddPromos");
				OrchestrationService.ViewShow("keyboard_promo_view", new KeyboardInfo
				{
					MaxLength = 31,
					KeyboardText = "",
					IsValidEntry = ValidationServices.IsValidPromoCode,
					CancelAction = delegate
					{
						OrchestrationService.ViewGoBack("go back", popOnly: false);
					},
					ContinueAction = ValidatePromoCode
				});
			};
		}
		else
		{
			shoppingCartViewModel.OnPromoCodeButtonClicked += delegate
			{
				BaseViewController.AnalyticsService?.AddButtonPressEvent("ShoppingCartAddPromos");
				OrchestrationService.ViewShow("apply_promo_view", new ApplyPromoInfo
				{
					ApplyPromoCodeAction = ValidatePromoCode
				});
			};
		}
		shoppingCartViewModel.OnUpdateBagButtonClicked += delegate
		{
			BaseViewController.AnalyticsService?.AddButtonPressEvent("ShoppingCartUpdateBag");
			OrchestrationService.ViewShow("shopping_cart_update_ada_view");
		};
		shoppingCartViewModel.OnCancelBrowseItemModel += RemoveItemFromShoppingCart;
		shoppingCartViewModel.OnPayButtonClicked += ShoppingCartViewModel_OnPayButtonClicked;
		shoppingCartViewModel.OnViewOfferDetailsButtonClicked += ShoppingCartViewModel_OnViewOfferDetailsButtonClicked;
		shoppingCartViewModel.RedboxPlusInfoCommand = DynamicRouting.RegisterRoutedCommand(new DynamicRoutedCommand(), (Action)delegate
		{
			BaseViewController.ProcessWPFHit();
			BaseViewController.AnalyticsService?.AddButtonPressEvent("ShoppingCartRedboxPlusInfo");
			OrchestrationService.ViewShow("redbox_plus_info_popup_view");
		}, (Func<bool>)null);
		shoppingCartViewModel.OnScanRedboxPlusQRCodeButtonClicked += ShoppingCartViewModel_OnScanRedboxPlusQRCodeButtonClicked;
		shoppingCartViewModel.OnViewRedboxPlusOfferButtonClicked += ShoppingCartViewModel_OnViewRedboxPlusOfferButtonClicked;
	}

	private static void ShoppingCartViewModel_OnViewRedboxPlusOfferButtonClicked()
	{
		BaseViewController.ProcessWPFHit();
		BaseViewController.GetViewModel<ShoppingCartViewModel>();
		BaseViewController.AnalyticsService?.AddButtonPressEvent("ShoppingCartViewRedboxPlusOffer");
		OrchestrationService.ViewShow("redbox_plus_offer_selection_view");
	}

	private static void ShoppingCartViewModel_OnScanRedboxPlusQRCodeButtonClicked()
	{
		BaseViewController.ProcessWPFHit();
		BaseViewController.AnalyticsService?.AddButtonPressEvent("ShoppingCartScanRedboxPlusQRCode");
		RedboxPlusFunctions.ShowRedboxPlusQRCode();
	}

	private static void ShoppingCartViewModel_OnLoaded()
	{
		IRentalShoppingCart rentalShoppingCart = BaseViewController.RentalShoppingCartService?.CurrentRentalShoppingCart;
		IPersonalizationSession personalizationSession = Redbox.Rental.Services.Session.Session.Instance?.PersonalizationSession;
		if (!string.IsNullOrEmpty(personalizationSession?.SelectedPromo) && personalizationSession.SelectedPromo != rentalShoppingCart?.Promotion?.Name)
		{
			LogHelper.Instance.Log("Validating promotion " + personalizationSession.SelectedPromo + " selected from My Perks screen.");
			ValidatePromoCode(personalizationSession.SelectedPromo);
		}
	}

	private static void ValidatePromoCode(string promoCode)
	{
		LogHelper.Instance.Log("calling show promo callback");
		Action<IPromoCodeValidationResult> doneCallback = delegate
		{
			BaseViewController.GetViewService().UnwindToSafe("shopping_cart_view", discard: false, "ShoppingCart:get_promo_code (on_complete)");
		};
		Action<IPromoCodeValidationResult> afterErrorMessageCallback = delegate
		{
			IViewService viewService = BaseViewController.GetViewService();
			if (viewService != null && viewService.Stack.Any((IViewFrameInstance vfi) => vfi.ViewFrame?.ViewName == "keyboard_promo_view"))
			{
				viewService?.UnwindToSafe("keyboard_promo_view", discard: false, "ShoppingCart:get_promo_code (on_error)");
			}
			else
			{
				viewService?.UnwindToSafe("shopping_cart_view", discard: false, "ShoppingCart:get_promo_code (on_error)");
			}
		};
		PromotionFunctions.ValidatePromoCode(promoCode, doneCallback, afterErrorMessageCallback);
	}

	private static void ShoppingCartViewModel_OnUsePointsButtonClicked()
	{
		BaseViewController.ProcessWPFHit();
		BaseViewController.AnalyticsService?.AddButtonPressEvent("ShoppingCartUsePoints");
		if (!PersonalizationFunctions.IsLoyaltySignedIn)
		{
			MessagePopupViewController.ShowLoyaltyIsDownMessage(delegate
			{
				OrchestrationService.ViewGoBackSafe("Loyalty is down message popup timeout", popOnly: false);
			}, delegate
			{
				OrchestrationService.ViewGoBackSafe("Loyalty is down message popup continue", popOnly: false);
			});
			return;
		}
		if ((Redbox.Rental.Services.Session.Session.Instance?.PersonalizationSession?.LoyaltySession?.PointsBalance).GetValueOrDefault() == 0)
		{
			PersonalizationFunctions.ShowNotEnoughPointsError();
			return;
		}
		bool flag = false;
		IRentalShoppingCart rentalShoppingCart = BaseViewController.RentalShoppingCartService?.CurrentRentalShoppingCart;
		if (rentalShoppingCart != null)
		{
			IList<IRentalShoppingCartTitleItem> redeemableItems = rentalShoppingCart.RedeemableItems;
			IList<IRentalShoppingCartTitleItem> toBeRedeemedItems = rentalShoppingCart.ToBeRedeemedItems;
			if (redeemableItems.Count == 1 && toBeRedeemedItems.Count == 0)
			{
				IRentalShoppingCartTitleItem rentalShoppingCartTitleItem = redeemableItems.First();
				if ((Redbox.Rental.Services.Session.Session.Instance?.PersonalizationSession?.LoyaltySession?.PointsBalance).GetValueOrDefault() >= rentalShoppingCartTitleItem.LoyaltyPointsRecord.PointsToRedeem.Value)
				{
					rentalShoppingCartTitleItem.LoyaltyPointsRecord.ToBeRedeemed = true;
					flag = true;
				}
			}
			else if (toBeRedeemedItems.Count == 1 && redeemableItems.Count == 0)
			{
				toBeRedeemedItems.First().LoyaltyPointsRecord.ToBeRedeemed = false;
				flag = true;
			}
		}
		IViewService viewService = BaseViewController.GetViewService();
		if ((rentalShoppingCart != null && viewService != null) & flag)
		{
			rentalShoppingCart.ValidateAndUpdatePromoCode();
			rentalShoppingCart.UpdateServiceFee();
			viewService.Show();
		}
		else
		{
			PersonalizationFunctions.ShowUseMemberPoints();
		}
	}

	private static void ShoppingCartViewModel_OnPointsBalanceInfoButtonClicked()
	{
		BaseViewController.ProcessWPFHit();
		BaseViewController.AnalyticsService?.AddButtonPressEvent("ShoppingCartPointsBalanceInfo");
		OrchestrationService.ViewShow("perks_signup_info_popup_view");
	}

	private static void ShoppingCartViewModel_OnSignInButtonClicked()
	{
		BaseViewController.ProcessWPFHit();
		BaseViewController.AnalyticsService?.AddButtonPressEvent("ShoppingCartSignIn");
		PersonalizationFunctions.ShowRegularSignInSignUpDialog();
	}

	private static void RemoveItemFromShoppingCart(DisplayProductModel displayProductModel, object parameter)
	{
		BaseViewController.ProcessWPFHit();
		if (displayProductModel?.Data is IBaseRentalShoppingCartItem rentalShoppingCartItem)
		{
			BaseViewController.AnalyticsService?.AddButtonPressEvent("ShoppingCartDelete")?.AddData(ProductData.ToProductData(rentalShoppingCartItem));
			ShoppingCartViewFunctions.RemoveItemFromShoppingCart(displayProductModel, parameter);
		}
	}

	private static void ShoppingCartViewModel_OnPayButtonClicked()
	{
		BaseViewController.AnalyticsService?.AddButtonPressEvent("ShoppingCartPayNow");
		IRentalShoppingCart rentalShoppingCart = BaseViewController.RentalShoppingCartService?.CurrentRentalShoppingCart;
		if (rentalShoppingCart == null)
		{
			return;
		}
		IPromotion promotion = rentalShoppingCart.Promotion;
		if (promotion != null && promotion.IsReturnVisit)
		{
			IPromotionService instance = PromotionService.Instance;
			if (instance == null || !instance.NeedToShowReturnVisitIneligibleCartWarning)
			{
				CheckoutFunctions.BeginCheckout();
				return;
			}
			BrowseState.ReturnVisitIneligibleCartWarningShown = true;
			MessagePopupViewController.ShowPromoCodeReturnVisitIneligibleCartWarning(Resources.pay, delegate
			{
				OrchestrationService.ViewGoBackSafe("ShowPromoCodeReturnVisitIneligibleCartWarning time out", popOnly: false);
				CheckoutFunctions.BeginCheckout();
			}, delegate
			{
				OrchestrationService.ViewGoBackSafe("ShowPromoCodeReturnVisitIneligibleCartWarning back", popOnly: false);
				BrowseProducts(BrowseState.TitleFamily, BrowseState.Genre, BrowseState.Rating, BrowseState.Sort);
			}, delegate
			{
				OrchestrationService.ViewGoBackSafe("ShowPromoCodeReturnVisitIneligibleCartWarning continue", popOnly: false);
				CheckoutFunctions.BeginCheckout();
			});
		}
		else if (promotion != null && promotion.Intent == PromotionIntentCode.RxGx)
		{
			IPromoCodeValidationInfo promoCodeValidationInfo = rentalShoppingCart.ValidateAndUpdatePromoCode();
			if (promoCodeValidationInfo != null && promoCodeValidationInfo.IsCartValid)
			{
				CheckoutFunctions.BeginCheckout();
				return;
			}
			MessagePopupViewController.ShowRxGxPromoCodeInvalidWarning(delegate
			{
				BaseViewController.GetViewService()?.GoBackSafe("Show RxGx Promo Code Invalid Warning popup timed out", popOnly: false);
			}, delegate
			{
				BaseViewController.AnalyticsService?.AddButtonPressEvent("ShoppingCartRemovePromo");
				rentalShoppingCart.ClearPromoCode();
				BaseViewController.GetViewService()?.GoBackSafe("Show RxGx Promo Code Invalid Warning popup: don't use promo", popOnly: false);
				ShowInCartOfferOrProceedCheckout(rentalShoppingCart);
			}, delegate
			{
				BaseViewController.AnalyticsService?.AddButtonPressEvent("ShoppingCartKeepPromo");
				BaseViewController.GetViewService()?.GoBackSafe("Show RxGx Promo Code Invalid Warning popup: use promo", popOnly: false);
				BrowseState.SetDefaultBrowseStateSet();
				OrchestrationService.ViewShow("browse_view", BrowseFunctions.GetBrowseViewparameters());
			}, promoCodeValidationInfo.Title + Environment.NewLine + Environment.NewLine + promoCodeValidationInfo.Message);
		}
		else if (promotion != null && promotion.Intent == PromotionIntentCode.FirstNightFreeItem && (promotion.CampaignTitles?.Titles?.Count).GetValueOrDefault() <= 0)
		{
			IPromoCodeValidationInfo promoCodeValidationInfo2 = rentalShoppingCart.ValidateAndUpdatePromoCode();
			if (promoCodeValidationInfo2 != null && promoCodeValidationInfo2.IsCartValid)
			{
				CheckoutFunctions.BeginCheckout();
				return;
			}
			string empty = string.Empty;
			MessagePopupViewController.ShowFirstNightFreePromoCodeInvalidWarning(message: promotion.ProductTypeId.HasValue ? PromotionFunctions.GetFirstNightFreePromoCodeInvalidWarningMessage(promotion.ProductTypeId) : (((promotion.FormatIds?.Count ?? 0) <= 0) ? string.Empty : PromotionFunctions.GetFirstNightFreePromoCodeInvalidWarningMessage(promotion.FormatIds)), timeOutAction: delegate
			{
				BaseViewController.GetViewService()?.GoBackSafe("Show First Night Free Promo Code Invalid Warning popup timed out", popOnly: false);
			}, backAction: delegate
			{
				BaseViewController.AnalyticsService?.AddButtonPressEvent("ShoppingCartRemovePromo");
				rentalShoppingCart.ClearPromoCode();
				BaseViewController.GetViewService()?.GoBackSafe("Show First Night Free Promo Code Invalid Warning popup: don't use promo", popOnly: false);
				ShowInCartOfferOrProceedCheckout(rentalShoppingCart);
			}, continueAction: delegate
			{
				BaseViewController.AnalyticsService?.AddButtonPressEvent("ShoppingCartKeepPromo");
				BaseViewController.GetViewService()?.GoBackSafe("Show First Night Free Promo Code Invalid Warning popup: use promo", popOnly: false);
				BrowseState.SetDefaultBrowseStateSet();
				OrchestrationService.ViewShow("browse_view", BrowseFunctions.GetBrowseViewparameters());
			});
		}
		else if (promotion != null && (promotion.CampaignTitles?.Titles?.Count).GetValueOrDefault() > 0)
		{
			IPromoCodeValidationInfo promoCodeValidationInfo3 = rentalShoppingCart.ValidateAndUpdatePromoCode();
			if (promoCodeValidationInfo3 != null && promoCodeValidationInfo3.IsCartValid)
			{
				CheckoutFunctions.BeginCheckout();
				return;
			}
			MessagePopupViewController.ShowTitleLevelPromoCodeInvalidWarning(delegate
			{
				BaseViewController.GetViewService()?.GoBackSafe("Show Title Level Promo Code Invalid Warning popup timed out", popOnly: false);
			}, delegate
			{
				BaseViewController.AnalyticsService?.AddButtonPressEvent("ShoppingCartRemovePromo");
				rentalShoppingCart.ClearPromoCode();
				BaseViewController.GetViewService()?.GoBackSafe("Show Title Level Promo Code Invalid Warning popup: don't use promo", popOnly: false);
				ShowInCartOfferOrProceedCheckout(rentalShoppingCart);
			}, delegate
			{
				BaseViewController.AnalyticsService?.AddButtonPressEvent("ShoppingCartKeepPromo");
				BaseViewController.GetViewService()?.GoBackSafe("Show Title Level Promo Code Invalid Warning popup: use promo", popOnly: false);
				BrowseState.SetDefaultBrowseStateSet();
				OrchestrationService.ViewShow("browse_view", BrowseFunctions.GetBrowseViewparameters());
			});
		}
		else
		{
			ShowInCartOfferOrProceedCheckout(rentalShoppingCart);
		}
	}

	private static void ShowInCartOfferOrProceedCheckout(IRentalShoppingCart rentalShoppingCart)
	{
		if (rentalShoppingCart != null && !rentalShoppingCart.IsFull && (KioskClientService?.Campaign?.EnableInCart ?? true) && !PromotionService.Instance.IsFreeInCartOfferRunning && !PromotionService.Instance.IsRentTwoInCartOfferRunning && PromotionService.Instance.CartQualifiesForInCartOffer(rentalShoppingCart) && !rentalShoppingCart.InCartPromoOfferMade)
		{
			ShoppingCartViewFunctions.ShowInCartOfferMessage();
		}
		else
		{
			CheckoutFunctions.BeginCheckout();
		}
	}

	private static void ShoppingCartViewModel_OnBackButtonClicked()
	{
		BaseViewController.AnalyticsService?.AddButtonPressEvent("ShoppingCartBack");
		BrowseProducts(BrowseState.TitleFamily, BrowseState.Genre, BrowseState.Rating, BrowseState.Sort);
	}

	private static void ShoppingCartViewModel_OnAddMovieButtonClicked()
	{
		BaseViewController.AnalyticsService?.AddButtonPressEvent("ShoppingCartAddMovie");
		if (BaseViewController.RentalShoppingCartService?.CurrentRentalShoppingCart != null)
		{
			BaseViewController.RentalShoppingCartService.CurrentRentalShoppingCart.InCartTitleMarketing = false;
		}
		BrowseProducts(TitleFamily.Movies, null, null, null);
	}

	private static void ShoppingCartViewModel_OnViewOfferDetailsButtonClicked(DisplayProductModel displayProductModel)
	{
		BaseViewController.ProcessWPFHit();
		IRentalShoppingCartSubscriptionItem rentalShoppingCartSubscriptionItem = displayProductModel?.Data as IRentalShoppingCartSubscriptionItem;
		if (rentalShoppingCartSubscriptionItem?.SubscriptionProduct != null)
		{
			ISubscriptionProduct subscriptionProduct = rentalShoppingCartSubscriptionItem.SubscriptionProduct;
			BaseViewController.AnalyticsService?.AddButtonPressEvent("ShoppingCartViewOfferDetails").AddData(ProductData.ToProductData(subscriptionProduct));
			if (subscriptionProduct is IRedboxPlusSubscriptionProduct parameter)
			{
				OrchestrationService.ViewShow("redbox_plus_offer_detail_view", parameter);
			}
			else
			{
				LogHelper.Instance.Log("Unable to find subscription product.");
			}
		}
	}

	private static void BrowseProducts(TitleFamily titleFamily, Genres? genre, Ratings? rating, BrowseSort? sort)
	{
		BrowseState.TitleFamily = titleFamily;
		BrowseState.TitleType = TitleType.All;
		BrowseState.BrowseSelectedSort = ((titleFamily == TitleFamily.Games) ? BrowseSelectedSort.ForSale : BrowseSelectedSort.Newest);
		BrowseState.Genre = genre;
		BrowseState.Rating = rating;
		BrowseState.Sort = sort;
		BrowseState.BrowseMode = ((titleFamily == TitleFamily.Games) ? BrowseViewFilter.ForSale : BrowseViewFilter.All);
		BrowseState.PageNumber = 1;
		BrowseState.BrowsePriceRangeFilter = null;
		BrowseState.BrowseStudio = null;
		OrchestrationService.ViewShow("browse_view", BrowseFunctions.GetBrowseViewparameters());
	}

	private static void UpdateADARedboxPlusMovieInfo(ShoppingCartViewModel shoppingCartViewModel, IRentalShoppingCart rentalShoppingCart)
	{
		shoppingCartViewModel.RedboxPlusMovieInfoVisibility = ((!BaseViewController.IsADAMode || !Redbox.Rental.Services.Configuration.Configuration.Instance.KioskSession.MarketingGeneral.EnableRedboxPlusRedemption || !HasActiveRedboxPlusSubscription || rentalShoppingCart == null || !rentalShoppingCart.TitleItems.Any((IRentalShoppingCartTitleItem x) => x?.TitleProduct?.IsRedboxPlusEligible == true)) ? Visibility.Collapsed : Visibility.Visible);
		shoppingCartViewModel.RedboxPlusMovieInfoButtonText = Resources.redbox_plus_movie;
	}

	private static void ConfigureTextAndVisibility(ShoppingCartViewModel shoppingCartViewModel)
	{
		if (shoppingCartViewModel == null)
		{
			return;
		}
		OrchestrationService.SetCurrentUICulture();
		IPersonalizationSession personalizationSession = Redbox.Rental.Services.Session.Session.Instance?.PersonalizationSession;
		IRentalShoppingCart rentalShoppingCart = BaseViewController.RentalShoppingCartService?.CurrentRentalShoppingCart;
		shoppingCartViewModel.LabelTop = Resources.my_bag;
		shoppingCartViewModel.PriceHeaderText = Resources.summary;
		shoppingCartViewModel.BackButtonText = Resources.back_button.ToUpper();
		TimeSpan value = Redbox.Rental.Services.Session.Session.Instance?.RentalReturnTime ?? TimeSpan.FromHours(21.0);
		DateTime dateTime = DateTime.Today.Add(value);
		string arg = ((value.Seconds == 0) ? dateTime.ToString("h:mm tt") : dateTime.ToString("h:mm:ss tt"));
		shoppingCartViewModel.PassiveSaleNote = string.Format(Resources.passive_sale_note, arg);
		shoppingCartViewModel.MDVFootnoteVisibility = ((!rentalShoppingCart.TitleItems.Any((IRentalShoppingCartTitleItem x) => x.MultiDisc)) ? Visibility.Collapsed : Visibility.Visible);
		shoppingCartViewModel.MDVFootnote = (rentalShoppingCart.TitleItems.Any((IRentalShoppingCartTitleItem x) => x.MultiDisc) ? Resources.multi_disc_vend_footnote : null);
		int num = (int)((rentalShoppingCart?.DefaultServiceFee ?? 0m) * 100m);
		shoppingCartViewModel.ServiceFeeText = string.Format(Resources.service_fee_text, num);
		shoppingCartViewModel.ServiceFeeTextVisibility = ((num <= 0) ? Visibility.Collapsed : Visibility.Visible);
		shoppingCartViewModel.TermsAndPrivacyButtonText = Resources.terms_link_text;
		shoppingCartViewModel.PromoCodeButtonMargin = (PersonalizationFunctions.IsSignedIn ? new Thickness(0.0, 0.0, 0.0, 48.0) : new Thickness(0.0, 0.0, 0.0, 74.0));
		if (Redbox.Rental.Services.Configuration.Configuration.Instance.KioskSession.MarketingGeneral.EnableRedboxPlusRedemption && HasActiveRedboxPlusSubscription)
		{
			shoppingCartViewModel.PromoCodeButtonText = ((rentalShoppingCart?.Promotion != null) ? Resources.promo_code_button_edit_redbox_plus_text : Resources.promo_code_button_use_redbox_plus_text);
		}
		else
		{
			shoppingCartViewModel.PromoCodeButtonText = ((rentalShoppingCart?.Promotion != null) ? Resources.promo_code_button_edit_text : Resources.promo_code_button_apply_text);
			int num2 = (personalizationSession?.StoredPromoCodes?.Count).GetValueOrDefault() - (personalizationSession?.StoredPromoCodes?.GetRedboxPlusPromos()?.Count).GetValueOrDefault();
			if (num2 > 0)
			{
				shoppingCartViewModel.PromoCodeButtonText += $" ({num2})";
			}
		}
		shoppingCartViewModel.SignInIncentiveText = Resources.sign_in_incentive_text;
		shoppingCartViewModel.SignInIncentiveTextVisibility = ((!PersonalizationFunctions.IsSignInEnabled) ? Visibility.Collapsed : Visibility.Visible);
		shoppingCartViewModel.SignInButtonText1 = Resources.use_and_earn_points;
		shoppingCartViewModel.SignInButtonText2 = Resources.sign_in_button_text2.ToUpper();
		shoppingCartViewModel.SignInButtonVisibility = ((!PersonalizationFunctions.IsSignInEnabled) ? Visibility.Collapsed : Visibility.Visible);
		IRentalShoppingCartSubscriptionItem rentalShoppingCartSubscriptionItem = rentalShoppingCart.SubscriptionItems.FirstOrDefault((IRentalShoppingCartSubscriptionItem x) => x.SubscriptionProduct.SubscriptionType == SubscriptionType.RedboxPlus);
		bool flag = rentalShoppingCart?.Promotion?.IsReturnVisit == true && rentalShoppingCart != null && rentalShoppingCart.Promotion?.Intent == PromotionIntentCode.FirstNightFreeItem;
		bool flag2 = (rentalShoppingCart?.Totals?.DiscountAmountUsed).GetValueOrDefault() > 0m;
		if (flag && !flag2)
		{
			shoppingCartViewModel.PricingModel.DiscountLabelText = Resources.discount_label_return_visit_not_applied_text;
		}
		else if (rentalShoppingCart != null && rentalShoppingCart.HasItemsToBeRedeemed)
		{
			shoppingCartViewModel.PricingModel.DiscountLabelText = Resources.discount_label_points_text;
			if (rentalShoppingCart != null && rentalShoppingCart.HasRedboxPlusFreeOneNightRentalSelected)
			{
				shoppingCartViewModel.PricingModel.DiscountLabelText = Resources.discount_label_points_and_redbox_plus_benefit_text;
			}
			else if (rentalShoppingCart?.Promotion != null)
			{
				shoppingCartViewModel.PricingModel.DiscountLabelText = Resources.discount_label_points_and_promo_text;
			}
		}
		else if (rentalShoppingCart != null && rentalShoppingCart.HasRedboxPlusFreeOneNightRentalSelected)
		{
			shoppingCartViewModel.PricingModel.DiscountLabelText = Resources.discount_label_redbox_plus_benefit_text;
		}
		else if (rentalShoppingCart?.Promotion != null)
		{
			shoppingCartViewModel.PricingModel.DiscountLabelText = Resources.discount_label_promo_text;
		}
		else
		{
			shoppingCartViewModel.PricingModel.DiscountLabelText = Resources.discount_label;
		}
		shoppingCartViewModel.PricingModel.ServiceFeeLabelText = Resources.service_fee_label;
		shoppingCartViewModel.PricingModel.SubtotalLabelText = Resources.subtotal_label;
		shoppingCartViewModel.PricingModel.TaxLabelText = ((rentalShoppingCartSubscriptionItem != null) ? Resources.estimated_tax_label : Resources.tax_label);
		shoppingCartViewModel.PricingModel.TotalLabelText = Resources.total_label;
		shoppingCartViewModel.PointsEstimateIntroText = Resources.points_estimate_intro;
		shoppingCartViewModel.PointsEstimateEndText = Resources.points_estimate_end;
		shoppingCartViewModel.PricingModel.SubtotalValueText = $"{(rentalShoppingCart?.Totals?.SubTotal).GetValueOrDefault() + (rentalShoppingCart?.Totals?.DiscountAmountUsed).GetValueOrDefault():C}";
		string text = (ServiceLocator.Instance.GetService<IDataService>()?.GetStore())?.State;
		shoppingCartViewModel.PricingModel.TaxLineVisibility = ((text == "SC" || text == "MD") ? Visibility.Collapsed : Visibility.Visible);
		shoppingCartViewModel.PricingModel.TaxValueText = $"{rentalShoppingCart?.Totals?.TaxAmount:C}";
		shoppingCartViewModel.PricingModel.TotalValueText = $"{rentalShoppingCart?.Totals?.GrandTotal:f2}";
		shoppingCartViewModel.PromoCodeButtonEnabled = (rentalShoppingCart == null || !rentalShoppingCart.InCartPromoOfferMade || rentalShoppingCart == null || rentalShoppingCart.InCartPromoOfferCancel) && (rentalShoppingCart == null || rentalShoppingCart.TitleItems.Any((IRentalShoppingCartTitleItem x) => !x.PricingRecord.IsMultiNightPrice) || (rentalShoppingCart?.TitleItems.Count ?? 0) == 0);
		string text2 = $"{((!(((rentalShoppingCart?.Totals?.DiscountAmountUsed).GetValueOrDefault() != 0m) | flag)) ? ((decimal?)null) : rentalShoppingCart?.Totals?.DiscountAmountUsed):C}";
		if (!string.IsNullOrEmpty(text2))
		{
			text2 = $"-{text2}";
		}
		shoppingCartViewModel.PricingModel.DiscountValueText = text2;
		shoppingCartViewModel.PricingModel.DiscountLineVisibility = ((!(((rentalShoppingCart?.Totals?.DiscountAmountUsed).GetValueOrDefault() > 0m) | flag)) ? Visibility.Collapsed : Visibility.Visible);
		shoppingCartViewModel.PricingModel.ServiceFeeValueText = $"{rentalShoppingCart?.Totals?.ServiceFee:C}";
		shoppingCartViewModel.PricingModel.ServiceFeeLineVisibility = ((!((rentalShoppingCart?.Totals?.ServiceFee).GetValueOrDefault() > 0m)) ? Visibility.Collapsed : Visibility.Visible);
		shoppingCartViewModel.PayButtonEnabled = rentalShoppingCart != null && rentalShoppingCart.Count > 0;
		shoppingCartViewModel.PayButtonText1 = (PersonalizationFunctions.IsSignedIn ? null : Resources.shopping_cart_pay_button_text1);
		shoppingCartViewModel.PayButtonText1Visibility = (PersonalizationFunctions.IsSignedIn ? Visibility.Collapsed : Visibility.Visible);
		shoppingCartViewModel.PayButtonText2 = (PersonalizationFunctions.IsSignedIn ? Resources.shopping_cart_pay_button_text1.ToUpper() : Resources.shopping_cart_pay_button_text2.ToUpper());
		string text3 = ((rentalShoppingCartSubscriptionItem != null) ? (" " + Resources.shopping_cart_redbox_plus_accept_and_pay_text) : null);
		string text4 = ((!Redbox.Rental.Services.Configuration.Configuration.Instance.KioskSession.Browse.EnableSonySweepstakes || rentalShoppingCart == null || !rentalShoppingCart.TitleItems.Any((IRentalShoppingCartTitleItem x) => x.TitleProduct.Studio.ToLower() == "sony")) ? null : (string.IsNullOrEmpty(text3) ? (" " + Resources.sony_sweepstakes_legal_text) : (" " + Resources.sony_sweepstakes_abbreviated_legal_text)));
		shoppingCartViewModel.AcceptAndPayText = Resources.shopping_cart_accept_and_pay_text + text3 + text4;
		if (Redbox.Rental.Services.Configuration.Configuration.Instance.KioskSession.MarketingGeneral.EnableRedboxSweepstakes && (rentalShoppingCart?.TitleItems?.Count).GetValueOrDefault() > 0)
		{
			shoppingCartViewModel.AcceptAndPayText = Resources.redbox_sweepstakes_accept_and_pay_text ?? "";
			if (rentalShoppingCartSubscriptionItem != null)
			{
				shoppingCartViewModel.AcceptAndPayText = Resources.redbox_sweepstakes_and_redbox_plus_accept_and_pay_text ?? "";
			}
		}
		shoppingCartViewModel.AddMovieButtonText = Resources.add_movie_button_text.ToUpper();
		shoppingCartViewModel.UpdateBagButtonText = Resources.update_bag.ToUpper();
		shoppingCartViewModel.UpdateBagButtonVisibility = ((!BaseViewController.IsADAMode) ? Visibility.Collapsed : Visibility.Visible);
		UpdateADARedboxPlusMovieInfo(shoppingCartViewModel, rentalShoppingCart);
		UpdateLoyaltyStatus(shoppingCartViewModel, rentalShoppingCart);
		shoppingCartViewModel.RedboxPlusOfferHeaderText = Resources.redbox_plus_offer_my_bag_title;
		shoppingCartViewModel.RedboxPlusOfferText = Resources.redbox_plus_offer_my_bag_subtitle;
		shoppingCartViewModel.RedboxPlusOfferVisibility = ((!Redbox.Rental.Services.Configuration.Configuration.Instance.KioskSession.MarketingGeneral.EnableRedboxPlusOffer || BaseViewController.IsADAMode || BrowseState.SubscriptionRemoved || rentalShoppingCart == null || rentalShoppingCart.Count > 2 || (rentalShoppingCart?.SubscriptionItems?.Count).GetValueOrDefault() != 0 || (PersonalizationFunctions.IsSignedIn && !PersonalizationFunctions.CanAddRedboxPlusSubscription())) ? Visibility.Collapsed : Visibility.Visible);
		shoppingCartViewModel.OfferVisibility = ((shoppingCartViewModel.RedboxPlusOfferVisibility != Visibility.Visible) ? Visibility.Collapsed : Visibility.Visible);
		shoppingCartViewModel.OfferSeparatorLineVisibility = Visibility.Collapsed;
	}

	private static void UpdateLoyaltyStatus(ShoppingCartViewModel shoppingCartViewModel, IRentalShoppingCart rentalShoppingCart)
	{
		if (shoppingCartViewModel == null)
		{
			return;
		}
		shoppingCartViewModel.LoyaltySystemErrorText = Resources.error_points_down;
		shoppingCartViewModel.LoyaltySystemErrorTextVisibility = Visibility.Collapsed;
		shoppingCartViewModel.PointsEstimateText = string.Format(Resources.points_estimate_text, 0, string.Empty);
		shoppingCartViewModel.PointsEstimateVisibility = Visibility.Collapsed;
		shoppingCartViewModel.PointsBalanceText = string.Format(Resources.shopping_cart_points_balance_text, 0);
		shoppingCartViewModel.PointsBalanceTextVisibility = Visibility.Collapsed;
		shoppingCartViewModel.PointsBalanceInfoButtonVisibility = Visibility.Collapsed;
		shoppingCartViewModel.PointsBalanceStackPanelVisibility = Visibility.Collapsed;
		shoppingCartViewModel.PointsBalanceInsufficientMessageText = string.Format(Resources.shopping_cart_points_balance_insufficient_message_text, 1);
		shoppingCartViewModel.PointsBalanceInsufficientMessageTextAlignment = TextAlignment.Left;
		shoppingCartViewModel.PointsBalanceInsufficientMessageTextVisibility = Visibility.Collapsed;
		shoppingCartViewModel.PointsBalanceSufficientMessageText = string.Format(Resources.shopping_cart_points_balance_sufficient_message_text, 1);
		shoppingCartViewModel.PointsBalanceSufficientMessageStackPanelVisibility = Visibility.Collapsed;
		shoppingCartViewModel.UsePointsButtonText = Resources.use_points.ToUpper();
		shoppingCartViewModel.UsePointsButtonEnabled = false;
		shoppingCartViewModel.UsePointsButtonVisibility = Visibility.Collapsed;
		if (PersonalizationFunctions.IsSignedIn && PersonalizationFunctions.IsLoyaltyEnabled && PersonalizationFunctions.IsLoyaltySignInError)
		{
			shoppingCartViewModel.LoyaltySystemErrorTextVisibility = Visibility.Visible;
		}
		else
		{
			if (rentalShoppingCart == null || !PersonalizationFunctions.IsLoyaltySignedIn)
			{
				return;
			}
			int totalPointsToBeEarned = rentalShoppingCart.TotalPointsToBeEarned;
			string arg = ((totalPointsToBeEarned > 0) ? "+" : string.Empty);
			shoppingCartViewModel.PointsEstimateText = string.Format(Resources.points_estimate_text, totalPointsToBeEarned, arg);
			shoppingCartViewModel.PointsEstimateVisibility = Visibility.Visible;
			shoppingCartViewModel.UsePointsButtonVisibility = Visibility.Visible;
			int valueOrDefault = (Redbox.Rental.Services.Session.Session.Instance?.PersonalizationSession?.LoyaltySession?.PointsBalance).GetValueOrDefault();
			IList<IRentalShoppingCartTitleItem> redeemableItems = rentalShoppingCart.RedeemableItems;
			IList<IRentalShoppingCartTitleItem> toBeRedeemedItems = rentalShoppingCart.ToBeRedeemedItems;
			if (toBeRedeemedItems.Count > 0)
			{
				int num = toBeRedeemedItems.Sum((IRentalShoppingCartTitleItem titleItem) => titleItem.LoyaltyPointsRecord.PointsToRedeem.Value);
				int num2 = valueOrDefault - num;
				shoppingCartViewModel.PointsBalanceText = string.Format(Resources.shopping_cart_remaining_points_balance_text, num2);
				shoppingCartViewModel.PointsBalanceTextVisibility = Visibility.Visible;
				shoppingCartViewModel.PointsBalanceStackPanelVisibility = Visibility.Visible;
				if (toBeRedeemedItems.Count == 1 && redeemableItems.Count == 0)
				{
					shoppingCartViewModel.UsePointsButtonText = Resources.undo_points.ToUpper();
				}
				else
				{
					shoppingCartViewModel.UsePointsButtonText = Resources.edit_points.ToUpper();
				}
				shoppingCartViewModel.UsePointsButtonEnabled = true;
			}
			else
			{
				if (redeemableItems.Count <= 0)
				{
					return;
				}
				if (rentalShoppingCart.Items.Count == 1 && rentalShoppingCart.Promotion != null)
				{
					if (rentalShoppingCart.HasRedboxPlusFreeOneNightRentalSelected)
					{
						shoppingCartViewModel.PointsBalanceInsufficientMessageText = Resources.shopping_cart_points_balance_insufficient_redbox_plus_message_text;
					}
					else
					{
						shoppingCartViewModel.PointsBalanceInsufficientMessageText = Resources.shopping_cart_points_balance_insufficient_promo_message_text;
					}
					shoppingCartViewModel.PointsBalanceInsufficientMessageTextAlignment = TextAlignment.Center;
					shoppingCartViewModel.PointsBalanceInsufficientMessageTextVisibility = Visibility.Visible;
					return;
				}
				shoppingCartViewModel.PointsBalanceText = string.Format(Resources.shopping_cart_points_balance_text, valueOrDefault);
				shoppingCartViewModel.PointsBalanceTextVisibility = Visibility.Visible;
				shoppingCartViewModel.PointsBalanceStackPanelVisibility = Visibility.Visible;
				IRentalShoppingCartTitleItem rentalShoppingCartTitleItem = redeemableItems.OrderBy((IRentalShoppingCartTitleItem titleItem) => titleItem.LoyaltyPointsRecord.PointsToRedeem.Value).First();
				int num3 = rentalShoppingCartTitleItem.PricingRecord?.InitialDays ?? 1;
				int value = rentalShoppingCartTitleItem.LoyaltyPointsRecord.PointsToRedeem.Value;
				string displayDescription = rentalShoppingCartTitleItem.TitleType.GetDisplayDescription();
				bool displayUpdatedPerksPoints = Redbox.Rental.Services.Configuration.Configuration.Instance.KioskSession.MarketingGeneral.DisplayUpdatedPerksPoints;
				int num4 = ((!displayUpdatedPerksPoints) ? 1 : 0);
				int num5 = (displayUpdatedPerksPoints ? 1550 : 1000);
				if (valueOrDefault < value)
				{
					if (valueOrDefault >= num4)
					{
						shoppingCartViewModel.PointsBalanceInfoButtonVisibility = Visibility.Visible;
						shoppingCartViewModel.PointsBalanceInsufficientMessageTextVisibility = Visibility.Visible;
						if (valueOrDefault >= num5)
						{
							int num6 = value - valueOrDefault;
							shoppingCartViewModel.PointsBalanceInsufficientMessageText = (displayUpdatedPerksPoints ? string.Format(Resources.shopping_cart_points_balance_insufficient_message_title_type_text_updated, num6, num3) : string.Format(Resources.shopping_cart_points_balance_insufficient_message_title_type_text, num6, num3, displayDescription));
						}
						else
						{
							shoppingCartViewModel.PointsBalanceInsufficientMessageText = string.Format(Resources.shopping_cart_points_balance_insufficient_message_text, num3);
						}
					}
				}
				else
				{
					shoppingCartViewModel.PointsBalanceSufficientMessageStackPanelVisibility = Visibility.Visible;
					shoppingCartViewModel.UsePointsButtonEnabled = true;
					if (redeemableItems.Count == 1)
					{
						shoppingCartViewModel.PointsBalanceSufficientMessageText = (displayUpdatedPerksPoints ? string.Format(Resources.shopping_cart_points_balance_sufficient_message_title_type_text_updated, num3) : string.Format(Resources.shopping_cart_points_balance_sufficient_message_title_type_text, num3, displayDescription));
					}
					else
					{
						shoppingCartViewModel.PointsBalanceSufficientMessageText = string.Format(Resources.shopping_cart_points_balance_sufficient_message_text, num3);
					}
				}
			}
		}
	}

	private static void ConfigureTTS(ShoppingCartViewModel shoppingCartViewModel)
	{
		ITextToSpeechService service = ServiceLocator.Instance.GetService<ITextToSpeechService>();
		FrameworkElement viewUserControl = BaseViewController.GetViewUserControl();
		if (service != null && service.TTSEnabled && viewUserControl != null)
		{
			shoppingCartViewModel.OnGetSpeechControl = () => ShoppingCartViewTextToSpeech.GetSpeechControl(GetShoppingCartViewModelFromView);
		}
	}

	private static ShoppingCartViewModel GetShoppingCartViewModelFromView()
	{
		FrameworkElement shoppingCartViewUserControl = BaseViewController.GetViewUserControl();
		if (shoppingCartViewUserControl != null)
		{
			return shoppingCartViewUserControl.Dispatcher.Invoke(() => shoppingCartViewUserControl?.DataContext as ShoppingCartViewModel);
		}
		return null;
	}
}
