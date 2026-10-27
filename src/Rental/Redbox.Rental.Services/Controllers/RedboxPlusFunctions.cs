using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Imaging;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel.KioskServices;
using Redbox.Rental.Model;
using Redbox.Rental.Model.Analytics;
using Redbox.Rental.Model.DataService;
using Redbox.Rental.Model.KioskClientService;
using Redbox.Rental.Model.KioskClientService.Subscriptions;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Model.Personalization;
using Redbox.Rental.Model.ShoppingCart;
using Redbox.Rental.Services.Configuration;
using Redbox.Rental.Services.Promotion;
using Redbox.Rental.Services.Session;
using Redbox.Rental.Services.ShoppingCart;
using Redbox.Rental.UI.Properties;

namespace Redbox.Rental.Services.Controllers;

public class RedboxPlusFunctions
{
	public static bool HasActiveSubscription => RedboxPlusSession?.HasActiveSubscription ?? false;

	public static bool CanShowRedboxPlusRedemption
	{
		get
		{
			if (Redbox.Rental.Services.Configuration.Configuration.Instance.KioskSession.MarketingGeneral.EnableRedboxPlusRedemption)
			{
				return HasActiveSubscription;
			}
			return false;
		}
	}

	public static RedboxPlusSubscriptionTier? RedboxPlusSubscriptionTier => Redbox.Rental.Services.Session.Session.Instance?.PersonalizationSession?.RedboxPlusSession?.RedboxPlusPlanInfo?.SubscriptionTier;

	public static bool IsFreeOneNightRentalAvailable => Redbox.Rental.Services.Session.Session.Instance?.PersonalizationSession?.StoredPromoCodes?.GetRedboxPlusPromos()?.Any() == true;

	public static int NumberOfFreeOneNightRentalsAvailable => (Redbox.Rental.Services.Session.Session.Instance?.PersonalizationSession?.StoredPromoCodes?.GetRedboxPlusPromos()?.Count()).GetValueOrDefault();

	private static IRedboxPlusSession RedboxPlusSession => Redbox.Rental.Services.Session.Session.Instance?.PersonalizationSession?.RedboxPlusSession;

	public static bool CanAddRedboxPlusFreeOneNightRental
	{
		get
		{
			bool result = false;
			IRentalShoppingCart rentalShoppingCart = ServiceLocator.Instance.GetService<IRentalShoppingCartService>()?.CurrentRentalShoppingCart;
			if (rentalShoppingCart != null)
			{
				if (HasActiveSubscription && IsFreeOneNightRentalAvailable)
				{
					if (rentalShoppingCart.IsFull)
					{
						return rentalShoppingCart.HasRedboxPlusFreeOneNightRentalSelected;
					}
					return true;
				}
				return false;
			}
			return result;
		}
	}

	public static IRentalShoppingCartTitleItem GetBestFreeOneNightRentalEligibleItem()
	{
		IRentalShoppingCartTitleItem result = null;
		List<IRentalShoppingCartTitleItem> eligibleFreeOneNightRentalItems = GetEligibleFreeOneNightRentalItems();
		if (eligibleFreeOneNightRentalItems != null && eligibleFreeOneNightRentalItems.Count > 0)
		{
			List<IRentalShoppingCartTitleItem> itemsToSelectFrom = eligibleFreeOneNightRentalItems;
			List<IRentalShoppingCartTitleItem> list = eligibleFreeOneNightRentalItems.Where((IRentalShoppingCartTitleItem x) => x?.LoyaltyPointsRecord?.ToBeRedeemed != true && !(x.PricingRecord?.IsMultiNightPrice ?? false))?.ToList();
			if (list != null && list.Count > 0)
			{
				itemsToSelectFrom = list;
			}
			result = itemsToSelectFrom.FirstOrDefault((IRentalShoppingCartTitleItem x) => x.Price == itemsToSelectFrom.Max((IRentalShoppingCartTitleItem y) => y.Price));
		}
		return result;
	}

	public static void ValidatePromoWillNotConflictWithRedboxPlusBenefit(Action cancelAction, Action continueInSpiteOfConflictAction, Action noConflictContinue)
	{
		IRentalShoppingCart obj = ServiceLocator.Instance.GetService<IRentalShoppingCartService>()?.CurrentRentalShoppingCart;
		if (obj != null && obj.HasRedboxPlusFreeOneNightRentalSelected)
		{
			MessagePopupViewController.ShowSwapRedboxPlusBenefitForPromoMessage(cancelAction, cancelAction, delegate
			{
				LogHelper.Instance.Log("Removing Redbox+ benefit(s) to allow use of promo.");
				ClearFreeOneNightRental();
				continueInSpiteOfConflictAction?.Invoke();
			});
		}
		else
		{
			noConflictContinue?.Invoke();
		}
	}

	public static void ValidateAndSetFreeOneNightRental(IRentalShoppingCartTitleItem bestFreeOneNightRentalEligibleItem, bool showConfirmationMessage, string afterSuccessUnwindToViewName)
	{
		IRentalShoppingCart rentalShoppingCart = ServiceLocator.Instance.GetService<IRentalShoppingCartService>()?.CurrentRentalShoppingCart;
		if (bestFreeOneNightRentalEligibleItem?.PricingRecord?.IsMultiNightPrice == true)
		{
			string contextMessage = "Failed to add Redbox+ Free 1 night rental discount.  Eligible item is using mulit-night pricing.";
			LogHelper.Instance.Log(contextMessage);
			Action action = delegate
			{
				OrchestrationService.UnwindToSafe(afterSuccessUnwindToViewName, discard: true, contextMessage);
			};
			MessagePopupViewController.ShowRedboxPlusMultiNightPriceErrorMessage(action, action);
			return;
		}
		if (bestFreeOneNightRentalEligibleItem?.LoyaltyPointsRecord?.ToBeRedeemed == true)
		{
			string contextMessage2 = "Failed to add Redbox+ Free 1 night rental discount.  Eligible item already being redeemed with perks points.";
			LogHelper.Instance.Log(contextMessage2);
			Action action2 = delegate
			{
				OrchestrationService.UnwindToSafe(afterSuccessUnwindToViewName, discard: true, contextMessage2);
			};
			MessagePopupViewController.ShowRedboxPlusPerksPointErrorMessage(action2, action2, action2);
			return;
		}
		if (rentalShoppingCart?.Promotion != null && !rentalShoppingCart.Promotion.IsRedboxPlusFreeOneNightRental)
		{
			string contextMessage3 = "ApplyPromoView failed to add Redbox+ Free 1 night rental discount.  Eligible item already being redeemed with promo.";
			LogHelper.Instance.Log(contextMessage3);
			Action continueAction = delegate
			{
				SwapPromoForRedboxPlusFreeOneNightRental(bestFreeOneNightRentalEligibleItem);
				if (!string.IsNullOrEmpty(afterSuccessUnwindToViewName))
				{
					OrchestrationService.UnwindToSafe(afterSuccessUnwindToViewName, discard: true, contextMessage3);
				}
				else
				{
					OrchestrationService.ViewGoBackSafe(contextMessage3, popOnly: false);
				}
			};
			Action action3 = delegate
			{
				OrchestrationService.ViewGoBackSafe(contextMessage3, popOnly: false);
			};
			MessagePopupViewController.ShowSwapPromoForRedboxPlusBenefitMessage(action3, action3, continueAction);
			return;
		}
		SetFreeOneNightRental(bestFreeOneNightRentalEligibleItem);
		if (!showConfirmationMessage)
		{
			return;
		}
		Action continueAction2 = delegate
		{
			if (!string.IsNullOrEmpty(afterSuccessUnwindToViewName))
			{
				OrchestrationService.UnwindToSafe(afterSuccessUnwindToViewName, discard: true, "ApplyPromoView added Redbox+ Free 1 night rental");
			}
		};
		OrchestrationService.SetCurrentUICulture();
		MessagePopupViewController.ShowRedboxPlusFreeOneNightRentalApplied(continueAction2);
	}

	public static void SwapPromoForRedboxPlusFreeOneNightRental(IRentalShoppingCartTitleItem freeOneNightRentalShoppingCartItem)
	{
		IRentalShoppingCart rentalShoppingCart = ServiceLocator.Instance.GetService<IRentalShoppingCartService>()?.CurrentRentalShoppingCart;
		if (rentalShoppingCart != null && freeOneNightRentalShoppingCartItem?.TitleProduct?.IsRedboxPlusEligible == true)
		{
			LogHelper.Instance.Log($"Swapping cart promo for Redbox+ Free 1 night rental on product id {freeOneNightRentalShoppingCartItem?.ProductId}");
			rentalShoppingCart.ClearPromoCode();
			SetFreeOneNightRental(freeOneNightRentalShoppingCartItem);
		}
	}

	private static void SetFreeOneNightRental(IRentalShoppingCartTitleItem rentalShoppingCartItem)
	{
		ClearFreeOneNightRental();
		if (rentalShoppingCartItem == null)
		{
			return;
		}
		IPromotion promotion = CreateRedboxPlusPromotion(rentalShoppingCartItem);
		if (promotion != null)
		{
			IPromotionService service = ServiceLocator.Instance.GetService<IPromotionService>();
			rentalShoppingCartItem.IsRedboxPlusFreeOneNightRental = true;
			LogHelper.Instance.Log($"Setting Redbox+ Free 1 night rental on product id {rentalShoppingCartItem?.ProductId}");
			service?.SetPromotion(promotion);
			IRentalShoppingCart rentalShoppingCart = ServiceLocator.Instance.GetService<IRentalShoppingCartService>()?.CurrentRentalShoppingCart;
			if (rentalShoppingCart != null && rentalShoppingCart.Items.Any((IBaseRentalShoppingCartItem x) => x == rentalShoppingCartItem))
			{
				rentalShoppingCart.TriggerChangeNotification();
			}
		}
	}

	private static IPromotion CreateRedboxPlusPromotion(IRentalShoppingCartTitleItem rentalShoppingCartTitleItem)
	{
		IPromotion result = null;
		IStoredPromoCode storedPromoCode = Redbox.Rental.Services.Session.Session.Instance?.PersonalizationSession?.StoredPromoCodes?.GetRedboxPlusPromos()?.FirstOrDefault();
		if (storedPromoCode != null)
		{
			result = new Redbox.Rental.Services.Promotion.Promotion
			{
				ActionCode = PromotionActionCode.Rental,
				CampaignId = storedPromoCode.CampaignId,
				DiscountType = DiscountType.PromotionCode,
				Name = storedPromoCode.PromoCode,
				QualifyingItems = new List<IRentalShoppingCartTitleItem> { rentalShoppingCartTitleItem },
				PromotionItems = new List<IPromotionItem>
				{
					new PromotionItem
					{
						RentalShoppingCartItem = rentalShoppingCartTitleItem,
						PromotionAmount = rentalShoppingCartTitleItem.Price
					}
				},
				Price = rentalShoppingCartTitleItem.Price,
				Quantity = 1,
				Intent = PromotionIntentCode.FirstNightFreeItem
			};
		}
		else
		{
			LogHelper.Instance.Log("Error in RedboxPlusFunctions.CreateRedboxPlusPromotion:  Unable to create Redbox+ promotion from StoredPromos list.");
		}
		return result;
	}

	public static void ClearFreeOneNightRental()
	{
		IRentalShoppingCart rentalShoppingCart = ServiceLocator.Instance.GetService<IRentalShoppingCartService>()?.CurrentRentalShoppingCart;
		rentalShoppingCart?.TitleItems.ForEach(delegate(IRentalShoppingCartTitleItem x)
		{
			if (x != null && x.IsRedboxPlusFreeOneNightRental)
			{
				x.IsRedboxPlusFreeOneNightRental = false;
				LogHelper.Instance.Log($"Removing Redbox+ Free 1 night rental from product id {x?.ProductId}");
				rentalShoppingCart?.ClearPromoCode();
				rentalShoppingCart?.ClearLoyaltyPointsToBeEarned();
			}
		});
		ServiceLocator.Instance.GetService<IPromotionService>()?.ClearPromotion();
		rentalShoppingCart.ClearLoyaltyPointsToBeEarned();
	}

	private static List<IRentalShoppingCartTitleItem> GetEligibleFreeOneNightRentalItems()
	{
		IRentalShoppingCart obj = ServiceLocator.Instance.GetService<IRentalShoppingCartService>()?.CurrentRentalShoppingCart;
		IDataService dataService = ServiceLocator.Instance.GetService<IDataService>();
		return obj?.TitleItems?.Where(delegate(IRentalShoppingCartTitleItem x)
		{
			IKioskProduct kioskProduct = null;
			dataService?.GetTitleRollupProduct(x.TitleRollupProductGroupId, out kioskProduct);
			ITitleRollupProduct titleRollupProduct = kioskProduct as ITitleRollupProduct;
			if (x.Action == RentalShoppingCartItemAction.rental)
			{
				ITitleProduct titleProduct = x.TitleProduct;
				if (titleProduct == null || !titleProduct.IsRedboxPlusEligible)
				{
					return titleRollupProduct?.IsRedboxPlusEligible ?? false;
				}
				return true;
			}
			return false;
		})?.ToList();
	}

	public static bool IsTitleProductSelectedRedboxPlusFreeOneNightRental(ITitleProduct titleProduct)
	{
		bool result = false;
		IRentalShoppingCart rentalShoppingCart = ServiceLocator.Instance?.GetService<IRentalShoppingCartService>()?.CurrentRentalShoppingCart;
		if (rentalShoppingCart != null && titleProduct != null && !(titleProduct is ITitleRollupProduct))
		{
			result = rentalShoppingCart.GetTitleItemsByCriteria(new RentalShoppingCartFilterCriteria
			{
				IncludeProducts = new List<long> { titleProduct.ProductId },
				IncludeFlashDeals = true,
				IncludeLoyaltyRedeemedItems = true,
				IncludeMultiNightPricedItems = true,
				IncludeDiscountRestrictionItems = true
			}).FirstOrDefault()?.IsRedboxPlusFreeOneNightRental ?? false;
		}
		return result;
	}

	public static void ShowRedboxPlusQRCode()
	{
		string redbox_plus_qr_code_disiplay_url = Resources.redbox_plus_qr_code_disiplay_url;
		string redbox_plus_qr_code_url = Resources.redbox_plus_qr_code_url;
		if (!string.IsNullOrEmpty(redbox_plus_qr_code_disiplay_url) && !string.IsNullOrEmpty(redbox_plus_qr_code_url))
		{
			BitmapImage bitmapImage = ServiceLocator.Instance.GetService<IKioskClientService>()?.QRCodeUtilities?.GetQRCodeImage(redbox_plus_qr_code_url, returnDefaultOnError: true);
			if (bitmapImage != null)
			{
				Action action = delegate
				{
					OrchestrationService.ViewGoBackSafe("QR Code Done", popOnly: false);
				};
				MessagePopupViewController.ShowQRCodeDialog(bitmapImage, redbox_plus_qr_code_disiplay_url, action, action);
			}
			else
			{
				LogHelper.Instance.Log("Unable to get Redbox+ offer QR code image for url " + redbox_plus_qr_code_url);
			}
		}
		else
		{
			LogHelper.Instance.Log("Redbox+ offer QR code url is null");
		}
	}
}
