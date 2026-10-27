using System;
using System.Linq;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model;
using Redbox.Rental.Model.Analytics;
using Redbox.Rental.Model.KioskClientService;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Model.Session;
using Redbox.Rental.Model.ShoppingCart;
using Redbox.Rental.Services.Promotion;
using Redbox.Rental.Services.Session;
using Redbox.Rental.Services.ViewDefinitions;
using Redbox.Rental.UI.Models;
using Redbox.Rental.UI.Properties;

namespace Redbox.Rental.Services.Controllers;

public class ShoppingCartViewFunctions
{
	private static IBrowseState BrowseState => Redbox.Rental.Services.Session.Session.Instance.BrowseState;

	private static IRentalShoppingCartService RentalShoppingCartService => ServiceLocator.Instance.GetService<IRentalShoppingCartService>();

	private static IAnalyticsService AnalyticsService => ServiceLocator.Instance.GetService<IAnalyticsService>();

	private static IKioskClientService KioskClientService => ServiceLocator.Instance.GetService<IKioskClientService>();

	public static void ViewShoppingCart()
	{
		BrowseState.IsShoppingCartFullMessagesOpen = false;
		IRentalShoppingCart rentalShoppingCart = RentalShoppingCartService?.CurrentRentalShoppingCart;
		IPromotionService instance = PromotionService.Instance;
		if (instance != null && instance.NeedToShowReturnVisitIneligibleCartWarning)
		{
			BrowseState.ReturnVisitIneligibleCartWarningShown = true;
			MessagePopupViewController.ShowPromoCodeReturnVisitIneligibleCartWarning(Resources.check_out_button_text, delegate
			{
				OrchestrationService.ViewGoBackSafe("ShowPromoCodeReturnVisitIneligibleCartWarning time out", popOnly: false);
				OrchestrationService.ViewShow("shopping_cart_view");
			}, delegate
			{
				OrchestrationService.ViewGoBackSafe("ShowPromoCodeReturnVisitIneligibleCartWarning back", popOnly: false);
			}, delegate
			{
				OrchestrationService.ViewGoBackSafe("ShowPromoCodeReturnVisitIneligibleCartWarning continue", popOnly: false);
				OrchestrationService.ViewShow("shopping_cart_view");
			});
		}
		else if ((KioskClientService?.Campaign?.EnableInCart ?? true) && (PromotionService.Instance.NeedToShowFreeInCartMessage || PromotionService.Instance.NeedToShowRentTwoInCartMessage))
		{
			BrowseState.ItemAddedFromView = null;
			ShowInCartOfferMessage();
		}
		else if (rentalShoppingCart != null && rentalShoppingCart.UpsellInfo.GetDialogNeedsToBeShown(TitleType.DVD))
		{
			OrchestrationService.ViewShow("upsell_view", new ShowUpsellViewParameter
			{
				TitleType = TitleType.DVD
			});
		}
		else if (rentalShoppingCart != null && rentalShoppingCart.UpsellInfo.GetDialogNeedsToBeShown(TitleType.Bluray))
		{
			OrchestrationService.ViewShow("upsell_view", new ShowUpsellViewParameter
			{
				TitleType = TitleType.Bluray
			});
		}
		else
		{
			OrchestrationService.ViewShow("shopping_cart_view");
		}
	}

	public static void ShowInCartOfferMessage()
	{
		IRentalShoppingCart rentalShoppingCart = RentalShoppingCartService?.CurrentRentalShoppingCart;
		if (rentalShoppingCart != null)
		{
			rentalShoppingCart.InCartPromoOfferMade = true;
		}
		MessagePopupViewController.ShowInCartOfferMessage(delegate
		{
			if (PromotionService.Instance.IsFreeInCartOfferRunning)
			{
				PromotionService.Instance.SetFreeInCartPromo(rentalShoppingCart);
				rentalShoppingCart?.ValidateAndUpdatePromoCode();
				GetViewService()?.GoBackSafe("ShowInCartOfferMessage Timed out.", popOnly: false);
			}
			else if (PromotionService.Instance.IsRentTwoInCartOfferRunning && PromotionService.Instance?.ValidateRentTwoInCartPromo()?.QualifyingItems?.Count() == 1)
			{
				rentalShoppingCart.InCartPromoOfferCancel = true;
				rentalShoppingCart.InCartTitleMarketing = false;
				GetViewService()?.GoBackSafe("ShowInCartOfferMessage Timed out.", popOnly: false);
			}
			else if (PromotionService.Instance.IsRentTwoInCartOfferRunning && (PromotionService.Instance?.ValidateRentTwoInCartPromo()?.QualifyingItems?.Count()).GetValueOrDefault() > 1)
			{
				rentalShoppingCart.InCartPromoOfferCancel = false;
				PromotionService.Instance.SetRentTwoInCartPromo(rentalShoppingCart);
				GetViewService()?.GoBackSafe("ShowInCartOfferMessage Timed out.", popOnly: false);
			}
			else
			{
				GetViewService()?.GoBackSafe("ShowInCartOfferMessage Timed out.", popOnly: true);
				rentalShoppingCart.InCartPromoOfferCancel = true;
				rentalShoppingCart.InCartTitleMarketing = false;
				CheckoutFunctions.BeginCheckout();
			}
		}, delegate
		{
			AnalyticsService?.AddButtonPressEvent("ShoppingCartViewFunctionsInCartPromoDecline");
			if (PromotionService.Instance.IsRentTwoInCartOfferRunning && PromotionService.Instance?.ValidateRentTwoInCartPromo()?.QualifyingItems?.Count() == 1)
			{
				rentalShoppingCart.InCartPromoOfferCancel = true;
				rentalShoppingCart.InCartTitleMarketing = false;
				GetViewService()?.GoBackSafe("in_cart_offer_message_view:no_button", popOnly: false);
			}
			else
			{
				GetViewService()?.GoBackSafe("in_cart_offer_message_view:no_button.", popOnly: true);
				rentalShoppingCart.InCartPromoOfferCancel = true;
				rentalShoppingCart.InCartTitleMarketing = false;
				CheckoutFunctions.BeginCheckout();
			}
		}, delegate
		{
			AnalyticsService?.AddButtonPressEvent("ShoppingCartViewFunctionsInCartPromoAccept");
			if (PromotionService.Instance.IsFreeInCartOfferRunning)
			{
				PromotionService.Instance.SetFreeInCartPromo(rentalShoppingCart);
				rentalShoppingCart?.ValidateAndUpdatePromoCode();
				GetViewService()?.GoBackSafe("ShowInCartOfferMessage yes button.", popOnly: false);
			}
			else if (PromotionService.Instance.IsRentTwoInCartOfferRunning && (PromotionService.Instance?.ValidateRentTwoInCartPromo()?.QualifyingItems?.Count()).GetValueOrDefault() > 1)
			{
				rentalShoppingCart.InCartPromoOfferCancel = false;
				PromotionService.Instance.SetRentTwoInCartPromo(rentalShoppingCart);
				GetViewService()?.GoBackSafe("ShowInCartOfferMessage yes button.", popOnly: false);
			}
			else
			{
				IPromotionRule inCartPromoActionRule = PromotionService.Instance.GetInCartPromoActionRule();
				if (inCartPromoActionRule != null && (inCartPromoActionRule.OfferAction == PromotionRule.Constants.OfferActionBrowseFilter || inCartPromoActionRule.OfferAction == PromotionRule.Constants.OfferActionTitleMarketing))
				{
					BrowseState.TitleFamily = inCartPromoActionRule.TitleFamily;
					BrowseState.Genre = null;
					BrowseState.Rating = null;
					BrowseState.Sort = null;
					BrowseState.BrowseSelectedSort = BrowseSelectedSort.Newest;
					BrowseState.PageNumber = 1;
					BrowseState.TitleType = inCartPromoActionRule.TitleType;
					BrowseState.BrowseMode = BrowseViewFilter.All;
					BrowseState.BrowsePriceRangeFilter = null;
					BrowseState.BrowseStudio = null;
					if (inCartPromoActionRule.OfferAction == PromotionRule.Constants.OfferActionTitleMarketing)
					{
						rentalShoppingCart.InCartTitleMarketing = true;
					}
				}
				if (PromotionService.Instance.IsFirstNightFreeItemInCartOfferRunning)
				{
					PromotionService.Instance.SetPromoToFirstNightFreeItemInCart(rentalShoppingCart);
				}
				else
				{
					PromotionService.Instance.SetPromoToFixedPriceInCart(rentalShoppingCart, 0m);
				}
				GetViewService()?.UnwindTo("browse_view", discard: true, "in-cart-offer-message-view:yes_button");
			}
		});
	}

	public static void RemoveItemFromShoppingCart(DisplayProductModel displayProductModel, object parameter)
	{
		IRentalShoppingCartTitleItem rentalShoppingCartTitleItem = displayProductModel?.Data as IRentalShoppingCartTitleItem;
		IRentalShoppingCartSubscriptionItem rentalShoppingCartSubscriptionItem = displayProductModel?.Data as IRentalShoppingCartSubscriptionItem;
		IRentalShoppingCart rentalShoppingCart = RentalShoppingCartService?.CurrentRentalShoppingCart;
		if (rentalShoppingCart != null && rentalShoppingCartTitleItem != null)
		{
			IRentalShoppingCartTitleItem deleteRentalShoppingCartItem = rentalShoppingCart.TitleItems.FirstOrDefault((IRentalShoppingCartTitleItem x) => x.ProductId == rentalShoppingCartTitleItem.ProductId);
			bool flag = true;
			if (rentalShoppingCart.InCartTitleMarketing)
			{
				LogHelper.Instance.Log("Creating temp RentalShoppingCart to retest in cart offer conditions");
				IRentalShoppingCart rentalShoppingCart2 = CloneRentalShoppingCart(rentalShoppingCart, deleteRentalShoppingCartItem);
				bool isCartValid = PromotionService.Instance.ValidateRegularInCartPromo(rentalShoppingCart2).IsCartValid;
				if (rentalShoppingCart2 != null)
				{
					RentalShoppingCartService.RemoveRentalShoppingCart(rentalShoppingCart2.ShoppingSessionId);
				}
				if (!isCartValid)
				{
					flag = false;
					MessagePopupViewController.ShowInCartCancelWarning(delegate
					{
						LogHelper.Instance.Log("Title marketing remove item warning popup timed out");
						RemoveItemFromShoppingCart(rentalShoppingCart, deleteRentalShoppingCartItem);
						ClearInCart(rentalShoppingCart);
						GetViewService()?.UnwindToSafe("shopping_cart_view", discard: true, "ShowInCartCancelWarning Timed out.");
					}, delegate
					{
						LogHelper.Instance.Log("User cancelled remove item with back");
						AnalyticsService?.AddButtonPressEvent("ShoppingCartViewFunctionsInCartRemoveItemCancel");
						GetViewService()?.UnwindToSafe("shopping_cart_view", discard: true, "ShowInCartCancelWarning go back.");
					}, delegate
					{
						LogHelper.Instance.Log("User confirmed remove item warning popup with continue");
						AnalyticsService?.AddButtonPressEvent("ShoppingCartViewFunctionsInCartRemoveItemConfirm");
						RemoveItemFromShoppingCart(rentalShoppingCart, deleteRentalShoppingCartItem);
						ClearInCart(rentalShoppingCart);
						GetViewService()?.UnwindToSafe("shopping_cart_view", discard: true, "ShowInCartCancelWarning continue.");
					});
				}
			}
			if (flag && deleteRentalShoppingCartItem != null)
			{
				RemoveItemFromShoppingCart(rentalShoppingCart, deleteRentalShoppingCartItem);
				GetViewService()?.Show();
			}
		}
		else if (rentalShoppingCart != null && rentalShoppingCartSubscriptionItem != null)
		{
			IRentalShoppingCartSubscriptionItem rentalShoppingCartSubscriptionItem2 = rentalShoppingCart.SubscriptionItems.FirstOrDefault((IRentalShoppingCartSubscriptionItem x) => x.SubscriptionProduct.CompareTo(rentalShoppingCartSubscriptionItem.SubscriptionProduct) == 0);
			if (rentalShoppingCartSubscriptionItem2 != null)
			{
				RemoveItemFromShoppingCart(rentalShoppingCart, rentalShoppingCartSubscriptionItem2);
				GetViewService()?.Show();
			}
		}
	}

	private static IRentalShoppingCart CloneRentalShoppingCart(IRentalShoppingCart rentalShoppingCartToClone, IRentalShoppingCartTitleItem excludeRentalShoppingCartItem)
	{
		IRentalShoppingCart rentalShoppingCart = RentalShoppingCartService.CreateNew(Guid.NewGuid());
		foreach (IRentalShoppingCartTitleItem item in rentalShoppingCartToClone.TitleItems.Where((IRentalShoppingCartTitleItem x) => excludeRentalShoppingCartItem == null || x.ProductId != (excludeRentalShoppingCartItem?.ProductId ?? 0)))
		{
			IRentalShoppingCartTitleItem rentalShoppingCartTitleItem = RentalShoppingCartService.CreateRentalShoppingCartItemFromProductId(item.ProductId);
			if (rentalShoppingCartTitleItem != null)
			{
				rentalShoppingCartTitleItem.SourceType = item.SourceType;
				rentalShoppingCartTitleItem.Action = item.Action;
				if (rentalShoppingCartTitleItem.SourceType == ItemSourceType.Reservation)
				{
					rentalShoppingCartTitleItem.Barcode = item.Barcode;
				}
				rentalShoppingCartTitleItem.Quantity = 1m;
				rentalShoppingCartTitleItem.Status = LineItemStatus.Pending;
				rentalShoppingCart.AddItem(rentalShoppingCartTitleItem);
			}
			else
			{
				LogHelper.Instance.Log("Error in RentalShoppingCartFunctions.AddItem. Unable to find product with id {0}", item.ProductId);
			}
		}
		return rentalShoppingCart;
	}

	private static void ClearInCart(IRentalShoppingCart rentalShoppingCart)
	{
		rentalShoppingCart.ClearPromoCode();
		rentalShoppingCart.InCartPromoOfferMade = true;
		rentalShoppingCart.InCartPromoOfferCancel = true;
		rentalShoppingCart.InCartTitleMarketing = false;
	}

	private static void RemoveItemFromShoppingCart(IRentalShoppingCart rentalShoppingCart, IBaseRentalShoppingCartItem deleteRentalShoppingCartItem)
	{
		AnalyticsService?.AddButtonPressEvent("ShoppingCartViewFunctionsRemoveItem")?.AddData(ProductData.ToProductData(deleteRentalShoppingCartItem));
		BrowseFunctions.PromptToRemoveItem(deleteRentalShoppingCartItem, userRemoved: true);
	}

	private static IViewService GetViewService()
	{
		return ServiceLocator.Instance.GetService<IViewService>();
	}
}
