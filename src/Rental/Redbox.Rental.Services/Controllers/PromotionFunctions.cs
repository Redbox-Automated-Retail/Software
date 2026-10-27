using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.KioskServices;
using Redbox.Rental.Model;
using Redbox.Rental.Model.Analytics;
using Redbox.Rental.Model.KioskClientService.Promotion;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Model.Personalization;
using Redbox.Rental.Model.Promotion;
using Redbox.Rental.Model.ShoppingCart;
using Redbox.Rental.Services.Promotion;
using Redbox.Rental.Services.Session;
using Redbox.Rental.UI.Models;
using Redbox.Rental.UI.Properties;

namespace Redbox.Rental.Services.Controllers;

public class PromotionFunctions
{
	public static void ValidatePromoCode(string promoCode, Action<IPromoCodeValidationResult> doneCallback, Action<IPromoCodeValidationResult> afterErrorMessageCallback)
	{
		OrchestrationService.ViewShow("general_message_view", new GeneralMessageViewModel
		{
			DisableIdleTimerOnEnter = true,
			EnableIdleTimerOnLeave = true,
			MessageText = Resources.promo_code_validating_message
		});
		if (promoCode == PromotionService.Instance?.GetInCartPromoCode())
		{
			LogHelper.Instance.Log("User manualy entered the InCartPromoCode - clearing!");
			promoCode = null;
		}
		PromotionService.Instance?.ValidatePromoCodeOnServer(promoCode, delegate(IPromoCodeValidationResult result)
		{
			ProcessPromoCodeValidationResult(result, doneCallback, afterErrorMessageCallback);
		});
	}

	private static void ProcessPromoCodeValidationResult(IPromoCodeValidationResult promoCodeValidationResult, Action<IPromoCodeValidationResult> doneCallback, Action<IPromoCodeValidationResult> afterErrorMessageCallback)
	{
		Task.Factory.StartNew(delegate
		{
			Action doneAction = delegate
			{
				doneCallback(promoCodeValidationResult);
			};
			Action action = delegate
			{
				afterErrorMessageCallback(promoCodeValidationResult);
			};
			if (promoCodeValidationResult.Success)
			{
				IRentalShoppingCartService service = ServiceLocator.Instance.GetService<IRentalShoppingCartService>();
				IPromotion newPromotion = CreatePromotionFromPromoCodeValidationResult(promoCodeValidationResult);
				IPromoCodeValidationInfo promoCodeValidationInfo = SetAndValidateAndUpdatePromotion(newPromotion);
				if (promoCodeValidationResult.IntentCode == PromotionIntentCode.FirstNightGameGameRequired && !promoCodeValidationInfo.IsCartValid && !promoCodeValidationInfo.IsCartInvalidBecauseOfMultiNightPricing)
				{
					promoCodeValidationResult.Success = false;
					promoCodeValidationResult.Response = "GameRequired";
				}
				else if (!promoCodeValidationInfo.IsCartValid && (promoCodeValidationInfo.IsCartInvalidBecauseOfFlashDeal || promoCodeValidationInfo.IsCartInvalidBecauseOfMultiNightPricing))
				{
					MessagePopupViewController.ShowPromoCodeInvalidForCartItemsWarning(doneAction, doneAction, promoCodeValidationInfo.Title, promoCodeValidationInfo.Message);
				}
				else if (!promoCodeValidationInfo.IsCartValid && promoCodeValidationInfo.IsCartInvalidBecauseOfDiscountRestriction && !newPromotion.AllowFullDiscount && newPromotion.DefaultPromo != null)
				{
					IPromotion promotion = CreatePromotionFromDefaultPromo(newPromotion.DefaultPromo);
					if (promotion != null)
					{
						if (SetAndValidateAndUpdatePromotion(promotion).IsCartValid)
						{
							string arg = ((promotion.Intent == PromotionIntentCode.PercentOffCartTotal) ? $"{promotion.Percent:P0}" : $"{promotion.Price:C}");
							MessagePopupViewController.ShowPromoCodeDiscountRestrictionWarning(string.Format(Resources.promo_code_discount_restriction_warning_title, arg), string.Format(Resources.apply_promo_amount_off_button, arg), delegate
							{
								SetAndValidateAndUpdatePromotion(newPromotion);
								doneAction();
							}, delegate
							{
								SetAndValidateAndUpdatePromotion(newPromotion);
								doneAction();
							}, delegate
							{
								doneAction();
							});
							ServiceLocator.Instance.GetService<IAnalyticsService>()?.AddViewEvent(new DiscountRestrictionEvent(newPromotion.Name, promotion.Name));
						}
						else
						{
							SetAndValidateAndUpdatePromotion(newPromotion);
							MessagePopupViewController.ShowPromoCodeInvalidForCartItemsWarning(doneAction, doneAction, Resources.promo_code_error_invalid_cart_items_title, Resources.promo_code_error_invalid_cart_items_message);
						}
					}
					else
					{
						doneAction();
					}
				}
				else if (newPromotion.Intent == PromotionIntentCode.RxGx)
				{
					if (!promoCodeValidationInfo.IsCartValid)
					{
						MessagePopupViewController.ShowRxGxPromoCodeInvalidWarning(doneAction, doneAction, promoCodeValidationInfo.Title + Environment.NewLine + Environment.NewLine + promoCodeValidationInfo.Message);
					}
					else
					{
						doneAction();
					}
				}
				else if (newPromotion.Intent == PromotionIntentCode.FirstNightFreeItem && (newPromotion.CampaignTitles?.Titles?.Count).GetValueOrDefault() <= 0)
				{
					if (!promoCodeValidationInfo.IsCartValid)
					{
						if (newPromotion.ProductTypeId.HasValue)
						{
							string firstNightFreePromoCodeInvalidWarningMessage = GetFirstNightFreePromoCodeInvalidWarningMessage(newPromotion.ProductTypeId);
							MessagePopupViewController.ShowFirstNightFreePromoCodeInvalidWarning(doneAction, doneAction, firstNightFreePromoCodeInvalidWarningMessage);
						}
						else if ((newPromotion.FormatIds?.Count ?? 0) > 0)
						{
							string firstNightFreePromoCodeInvalidWarningMessage2 = GetFirstNightFreePromoCodeInvalidWarningMessage(newPromotion.FormatIds);
							MessagePopupViewController.ShowFirstNightFreePromoCodeInvalidWarning(doneAction, doneAction, firstNightFreePromoCodeInvalidWarningMessage2);
						}
						else
						{
							MessagePopupViewController.ShowTitleLevelPromoCodeInvalidWarning(doneAction, doneAction);
						}
					}
					else
					{
						doneAction();
					}
				}
				else if ((newPromotion.CampaignTitles?.Titles?.Count).GetValueOrDefault() > 0)
				{
					if (!promoCodeValidationInfo.IsCartValid)
					{
						MessagePopupViewController.ShowTitleLevelPromoCodeInvalidWarning(doneAction, doneAction);
					}
					else
					{
						doneAction();
					}
				}
				else if (newPromotion.ActionCode == PromotionActionCode.Rental && service.CurrentRentalShoppingCart.HasItemAction(RentalShoppingCartItemAction.purchase))
				{
					if (!promoCodeValidationInfo.IsCartValid)
					{
						MessagePopupViewController.ShowPromoCodeIneligibleCartPurchaseWarning(doneAction, doneAction);
					}
					else
					{
						doneAction();
					}
				}
				else if (newPromotion.ActionCode == PromotionActionCode.Purchase && service.CurrentRentalShoppingCart.HasItemAction(RentalShoppingCartItemAction.rental))
				{
					if (!promoCodeValidationInfo.IsCartValid)
					{
						MessagePopupViewController.ShowPromoCodeIneligibleCartRentalWarning(doneAction, doneAction);
					}
					else
					{
						doneAction();
					}
				}
				else if (!promoCodeValidationInfo.IsCartValid)
				{
					MessagePopupViewController.ShowPromoCodeIneligibleCartWarning(doneAction, doneAction);
				}
				else
				{
					doneAction();
				}
			}
			if (!promoCodeValidationResult.Success)
			{
				ServiceLocator.Instance.GetService<IPersonalizationService>()?.ClearSelectedPromo();
				if (promoCodeValidationResult.Response == PromotionValidationResponse.CodeAlreadyUsed.ToString())
				{
					MessagePopupViewController.ShowPromoCodeAlreadyUsedWarning(action, action);
				}
				else if (promoCodeValidationResult.Response == PromotionValidationResponse.CodeNotValidAtThisLocation.ToString())
				{
					MessagePopupViewController.ShowPromoCodeNotValidAtThisLocationWarning(action, action);
				}
				else if (promoCodeValidationResult.Response == PromotionValidationResponse.CodeForOnDemand.ToString())
				{
					MessagePopupViewController.ShowPromoCodeForOnDemandWarning(action, action);
				}
				else if (promoCodeValidationResult.Response == PromotionValidationResponse.Error.ToString() || promoCodeValidationResult.Response == "Server Busy" || promoCodeValidationResult.Response == "Communications Failure")
				{
					MessagePopupViewController.ShowPromoCodeValidationFailureWarning(action, action);
				}
				else if (promoCodeValidationResult.Response == "GameRequired")
				{
					MessagePopupViewController.ShowPromoCodeGameRequiredWarning(action, action);
				}
				else
				{
					MessagePopupViewController.ShowPromoCodeNotValidWarning(action, action);
				}
			}
		}, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.FromCurrentSynchronizationContext());
	}

	private static IPromotion CreatePromotionFromPromoCodeValidationResult(IPromoCodeValidationResult promoCodeValidationResult)
	{
		decimal amount = promoCodeValidationResult.Amount;
		decimal num = ((promoCodeValidationResult.IntentCode == PromotionIntentCode.PercentOffCartTotal) ? amount : 0m);
		IPromotion promotion = new Redbox.Rental.Services.Promotion.Promotion
		{
			Name = promoCodeValidationResult.PromoCode,
			OriginalPrice = amount,
			Price = amount,
			Quantity = 1,
			Intent = promoCodeValidationResult.IntentCode,
			OriginalPercent = num,
			Percent = num,
			RentQuantity = (byte)promoCodeValidationResult.RentQuantity,
			RentFormat = promoCodeValidationResult.RentFormat,
			GetQuantity = (byte)promoCodeValidationResult.GetQuantity,
			GetFormat = promoCodeValidationResult.GetFormat,
			CampaignTitles = promoCodeValidationResult.CampaignTitles,
			ProductTypeId = promoCodeValidationResult.ProductTypeId,
			FormatIds = promoCodeValidationResult.FormatIds,
			ActionCode = promoCodeValidationResult.ActionCode,
			AllowFullDiscount = (promoCodeValidationResult.AllowFullDiscount ?? true)
		};
		if (promoCodeValidationResult.DefaultPromo != null)
		{
			promotion.DefaultPromo = new Redbox.Rental.Services.Promotion.DefaultPromo
			{
				PromoCode = promoCodeValidationResult.DefaultPromo.PromoCode,
				PromotionIntentCode = promoCodeValidationResult.DefaultPromo.PromotionIntentCode,
				Amount = promoCodeValidationResult.DefaultPromo.Amount
			};
		}
		IStoredPromoCode storedPromoCode = Redbox.Rental.Services.Session.Session.Instance?.PersonalizationSession?.StoredPromoCodes?.FirstOrDefault((IStoredPromoCode x) => x.PromoCode == promoCodeValidationResult.PromoCode);
		if (storedPromoCode != null)
		{
			promotion.CampaignId = storedPromoCode.CampaignId;
		}
		return promotion;
	}

	public static IPromotion CreatePromotionFromPromotionCodeValidationResponse(string promoCode, PromotionCodeValidationResponse promotionCodeValidationResponse)
	{
		if (!string.IsNullOrEmpty(promoCode) && promotionCodeValidationResponse != null && Enum.TryParse<PromotionIntentCode>(promotionCodeValidationResponse.PromotionIntentCode, out var result))
		{
			decimal amount = promotionCodeValidationResponse.Amount;
			decimal num = ((result == PromotionIntentCode.PercentOffCartTotal) ? amount : 0m);
			PromotionRentFormat result2;
			PromotionGetFormat result3;
			PromotionActionCode result4;
			return new Redbox.Rental.Services.Promotion.Promotion
			{
				Name = promoCode,
				OriginalPrice = amount,
				Price = amount,
				Quantity = 1,
				Intent = result,
				OriginalPercent = num,
				Percent = num,
				RentQuantity = (byte)(promotionCodeValidationResponse.RentQty.HasValue ? promotionCodeValidationResponse.RentQty.Value : 0),
				RentFormat = (Enum.TryParse<PromotionRentFormat>(promotionCodeValidationResponse.RentFormat, out result2) ? new PromotionRentFormat?(result2) : ((PromotionRentFormat?)null)),
				GetQuantity = (byte)(promotionCodeValidationResponse.GetQty.HasValue ? promotionCodeValidationResponse.GetQty.Value : 0),
				GetFormat = (Enum.TryParse<PromotionGetFormat>(promotionCodeValidationResponse.GetFormat, out result3) ? new PromotionGetFormat?(result3) : ((PromotionGetFormat?)null)),
				CampaignTitles = ((promotionCodeValidationResponse.CampaignTitles != null) ? new CampaignTitles
				{
					Include = promotionCodeValidationResponse.CampaignTitles.Include,
					Titles = promotionCodeValidationResponse.CampaignTitles.Titles
				} : null),
				ProductTypeId = promotionCodeValidationResponse.ProductTypeId,
				FormatIds = promotionCodeValidationResponse.FormatIds,
				ActionCode = ((!Enum.TryParse<PromotionActionCode>(promotionCodeValidationResponse.ActionType, out result4)) ? PromotionActionCode.Rental : result4),
				AllowFullDiscount = (promotionCodeValidationResponse.AllowFullDiscount ?? true),
				DefaultPromo = ((promotionCodeValidationResponse.DefaultPromo != null) ? new Redbox.Rental.Services.Promotion.DefaultPromo
				{
					PromoCode = promotionCodeValidationResponse.DefaultPromo.PromoCode,
					PromotionIntentCode = promotionCodeValidationResponse.DefaultPromo.PromotionIntentCode,
					Amount = promotionCodeValidationResponse.DefaultPromo.Amount
				} : null),
				CampaignId = 0
			};
		}
		return null;
	}

	private static IPromotion CreatePromotionFromDefaultPromo(IDefaultPromo defaultPromo)
	{
		if (defaultPromo != null && !string.IsNullOrEmpty(defaultPromo.PromoCode) && !string.IsNullOrEmpty(defaultPromo.PromotionIntentCode) && defaultPromo.Amount.HasValue && Enum.TryParse<PromotionIntentCode>(defaultPromo.PromotionIntentCode, out var result) && (result == PromotionIntentCode.FixedValue || result == PromotionIntentCode.PercentOffCartTotal))
		{
			decimal num = (decimal)defaultPromo.Amount.Value;
			decimal num2 = ((result == PromotionIntentCode.PercentOffCartTotal) ? num : 0m);
			return new Redbox.Rental.Services.Promotion.Promotion
			{
				Name = defaultPromo.PromoCode,
				OriginalPrice = num,
				Price = num,
				Quantity = 1,
				Intent = result,
				OriginalPercent = num2,
				Percent = num2,
				RentQuantity = 0,
				RentFormat = null,
				GetQuantity = 0,
				GetFormat = null,
				CampaignTitles = null,
				ProductTypeId = null,
				FormatIds = null,
				ActionCode = PromotionActionCode.Rental,
				AllowFullDiscount = false,
				DefaultPromo = null,
				CampaignId = 0
			};
		}
		return null;
	}

	private static IPromoCodeValidationInfo SetAndValidateAndUpdatePromotion(IPromotion promotion)
	{
		PromotionService.Instance?.SetPromotion(promotion);
		IRentalShoppingCartService service = ServiceLocator.Instance.GetService<IRentalShoppingCartService>();
		IPromoCodeValidationInfo result = service.CurrentRentalShoppingCart.ValidateAndUpdatePromoCode();
		service.CurrentRentalShoppingCart.UpdateServiceFee();
		service.CurrentRentalShoppingCart.ClearLoyaltyPointsToBeEarned();
		return result;
	}

	public static string GetFirstNightFreePromoCodeInvalidWarningMessage(int? productTypeId)
	{
		if (!productTypeId.HasValue)
		{
			return string.Empty;
		}
		string empty = string.Empty;
		return string.Format(arg0: (productTypeId == 1) ? Resources.promo_code_first_night_free_movie : ((productTypeId != 2) ? Resources.promo_code_first_night_free_item : Resources.promo_code_first_night_free_game), format: Resources.promo_code_first_night_free_message);
	}

	public static string GetFirstNightFreePromoCodeInvalidWarningMessage(List<int> formatIds)
	{
		if (formatIds == null || formatIds.Count == 0)
		{
			return string.Empty;
		}
		string text = string.Empty;
		if (formatIds.Count == 1)
		{
			text = ((TitleType)formatIds[0]).GetDisplayDescription();
		}
		else if (formatIds.Count == 2)
		{
			text = ((TitleType)formatIds[0]).GetDisplayDescription() + " " + Resources.promo_code_first_night_free_or + " " + ((TitleType)formatIds[1]).GetDisplayDescription();
		}
		else
		{
			for (int i = 0; i < formatIds.Count; i++)
			{
				text = ((i >= formatIds.Count - 2) ? ((i >= formatIds.Count - 1) ? (text + ((TitleType)formatIds[i]).GetDisplayDescription()) : (text + ((TitleType)formatIds[i]).GetDisplayDescription() + ", " + Resources.promo_code_first_night_free_or + " ")) : (text + ((TitleType)formatIds[i]).GetDisplayDescription() + ", "));
			}
		}
		return string.Format(Resources.promo_code_first_night_free_message, text);
	}
}
