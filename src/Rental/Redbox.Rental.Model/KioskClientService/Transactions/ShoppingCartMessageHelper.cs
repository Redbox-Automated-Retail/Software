using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using DeviceService.ComponentModel;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.TrackData;
using Redbox.Rental.Model.Pricing;
using Redbox.Rental.Model.Session;
using Redbox.Rental.Model.ShoppingCart;

namespace Redbox.Rental.Model.KioskClientService.Transactions;

public class ShoppingCartMessageHelper
{
	public static CartMessage CreateCartMessage(string customerProfileNumber, string messageType)
	{
		IRentalShoppingCartService rentalShoppingCartService = ServiceLocator.Instance.GetService<IRentalShoppingCartService>();
		if (rentalShoppingCartService?.CurrentRentalShoppingCart != null)
		{
			return CreateCartMessage(rentalShoppingCartService.CurrentRentalShoppingCart.ShoppingSessionId, customerProfileNumber, messageType);
		}
		return null;
	}

	public static CartMessage CreateCartMessage(Guid sessionId, string customerProfileNumber, string messageType)
	{
		return new CartMessage
		{
			MessageType = messageType,
			KioskId = ServiceLocator.Instance.GetService<IStoreServices>().StoreNumberInteger,
			CustomerProfileNumber = customerProfileNumber,
			ShoppingCart = CreateShoppingCart(sessionId, titleItemsOnly: true)
		};
	}

	public static MessageShoppingCart CreateShoppingCart(Guid sessionId, bool titleItemsOnly = false)
	{
		IRentalShoppingCart rentalShoppingCart = ServiceLocator.Instance.GetService<IRentalShoppingCartService>()?.GetRentalShoppingCart(sessionId);
		return CreateShoppingCart(rentalShoppingCart, rentalShoppingCart.Promotion, titleItemsOnly);
	}

	public static MessageShoppingCart CreateShoppingCart(IRentalShoppingCart rentalShoppingCart, IPromotion promo, bool titleItemsOnly = false)
	{
		MessageShoppingCart shoppingCart = new MessageShoppingCart();
		if (titleItemsOnly ? (rentalShoppingCart?.TitleItems?.Any() == true) : (rentalShoppingCart?.Items?.Any() == true))
		{
			(rentalShoppingCart?.TitleItems?.Where((IRentalShoppingCartTitleItem x) => x?.LoyaltyPointsRecord?.ToBeRedeemed == true))?.ForEach(delegate(IRentalShoppingCartTitleItem x)
			{
				shoppingCart.Discounts.Add(new Discount
				{
					ProductId = x.ProductId,
					DiscountType = DiscountType.Loyalty,
					RedemptionPoints = x.LoyaltyPointsRecord.PointsToRedeem,
					Amount = (x.PricingRecord?.InitialNight ?? 0m)
				});
			});
			if (rentalShoppingCart.Promotion != null)
			{
				promo?.PromotionItems?.ForEach(delegate(IPromotionItem x)
				{
					shoppingCart.Discounts.Add(new Discount
					{
						Amount = x.PromotionAmount,
						DiscountType = (DiscountType)rentalShoppingCart.Promotion.DiscountType,
						PromotionCode = promo.Name,
						PromotionCodeValue = promo.OriginalPrice,
						PromotionIntentCode = promo.Intent,
						ProductId = x.RentalShoppingCartItem.ProductId,
						ApplyOnlyToProduct = (promo.IsValidatedPostVend ? new bool?(promo.IsProductSpecific) : ((bool?)null))
					});
				});
			}
			foreach (IGrouping<RentalShoppingCartItemAction, IBaseRentalShoppingCartItem> item in from x in rentalShoppingCart.Items
				group x by x.Action)
			{
				List<LineItem> lineItems = new List<LineItem>();
				LineItemGroupType groupType = ((item.Key == RentalShoppingCartItemAction.subscription) ? LineItemGroupType.Subscription : ((item.Key == RentalShoppingCartItemAction.rental) ? LineItemGroupType.Rental : LineItemGroupType.Purchase));
				if (item.Key == RentalShoppingCartItemAction.subscription)
				{
					if (titleItemsOnly)
					{
						continue;
					}
					foreach (IRentalShoppingCartSubscriptionItem subscriptionItem in item)
					{
						lineItems.Add(new LineItem
						{
							SubscriptionId = subscriptionItem.SubscriptionProduct.SubscriptionId,
							TempPassword = subscriptionItem.TempPassword
						});
					}
					shoppingCart.Groups.Add(new LineItemGroup
					{
						GroupType = groupType,
						Items = lineItems
					});
					continue;
				}
				foreach (IRentalShoppingCartTitleItem titleItem in item)
				{
					IUpsellItem upsellItemByRentalShoppingCartItem = rentalShoppingCart.UpsellInfo.GetUpsellItemByRentalShoppingCartItem(titleItem);
					BlurayUpsell blurayUpsell = null;
					if (upsellItemByRentalShoppingCartItem != null)
					{
						blurayUpsell = new BlurayUpsell
						{
							ProductGroupId = (int)upsellItemByRentalShoppingCartItem.ProductGroupId,
							TitleId = titleItem.ProductId,
							Offer = upsellItemByRentalShoppingCartItem.OfferResponse.ToString(),
							TypeId = (int)upsellItemByRentalShoppingCartItem.TypeId
						};
					}
					IPricingRecord pricingRecord = titleItem?.PricingRecord;
					lineItems.Add(new LineItem
					{
						Barcode = titleItem.Barcode,
						BlurayUpsell = blurayUpsell,
						SourceType = titleItem.SourceType,
						ProductFamily = titleItem.TitleFamily.ToString(),
						ProductId = titleItem.ProductId,
						ProductType = titleItem.TitleType.GetDisplayName(),
						ProductName = titleItem.TitleProduct.ProductName,
						OfferCode = titleItem.OfferCode,
						MultiDisc = titleItem.MultiDisc,
						DiscNumber = titleItem.DiscNumber,
						Pricing = new Pricing
						{
							DefaultExtraNight = pricingRecord?.DefaultExtraNightPrice,
							DefaultInitialNight = pricingRecord?.DefaultInitialNightPrice,
							Expiration = (pricingRecord?.ExpirationPrice ?? 0m),
							ExtraNight = (pricingRecord?.ExtraNight ?? 0m),
							InitialDays = pricingRecord?.InitialDays,
							InitialNight = (pricingRecord?.InitialNight ?? 0m),
							NonReturn = (pricingRecord?.NonReturn ?? 0m),
							NonReturnDays = (int)(pricingRecord?.NonReturnDays ?? 0m),
							PriceSetId = (pricingRecord?.PriceSetId).GetValueOrDefault(),
							ProductPriceId = (int)(pricingRecord?.ProductPricingId).GetValueOrDefault(),
							Purchase = (pricingRecord?.Purchase ?? 0m)
						},
						VendStatus = ConvertToVendStatus(titleItem.Status),
						TivoQueryId = titleItem.TivoQueryId,
						PersonalizationTitleTags = titleItem.PersonalizationTitleTags
					});
				}
				ISubTotalGroup subTotalGroup = rentalShoppingCart.Totals.SubTotalGroups.FirstOrDefault((ISubTotalGroup y) => item.Key == y.Action);
				shoppingCart.Groups.Add(new LineItemGroup
				{
					GroupType = groupType,
					Items = lineItems,
					Totals = new Totals
					{
						DiscountedSubtotal = subTotalGroup.SubTotal,
						Subtotal = subTotalGroup.Amount,
						GrandTotal = subTotalGroup.GrantTotal,
						TaxAmount = subTotalGroup.TaxAmount,
						TaxRate = subTotalGroup.TaxRate * 100m
					}
				});
			}
		}
		if (rentalShoppingCart.ServiceFee != null && shoppingCart.ServiceFee != null)
		{
			shoppingCart.ServiceFee.DefaultAmount = rentalShoppingCart.ServiceFee.DefaultAmount;
			shoppingCart.ServiceFee.ActualAmount = rentalShoppingCart.ServiceFee.ActualAmount;
			shoppingCart.ServiceFee.TaxRate = rentalShoppingCart.ServiceFee.TaxRate;
			shoppingCart.ServiceFee.TaxAmount = rentalShoppingCart.ServiceFee.TaxAmount;
		}
		return shoppingCart;
	}

	public static string CalcUtcOffset(DateTime now)
	{
		TimeSpan utcOffset = TimeZone.CurrentTimeZone.GetUtcOffset(now);
		return utcOffset.TotalHours.ToString("00") + ":" + utcOffset.Minutes.ToString("00");
	}

	private static VendStatus ConvertToVendStatus(LineItemStatus status)
	{
		switch (status)
		{
		case LineItemStatus.Pending:
		case LineItemStatus.InventoryError:
			return VendStatus.EmptyOrStuck;
		case LineItemStatus.CustomerDisabled:
			return VendStatus.CustomerDisabled;
		case LineItemStatus.ItemNotTaken:
			return VendStatus.DiskNotTaken;
		case LineItemStatus.HardwareError:
			return VendStatus.HardwareError;
		case LineItemStatus.InventoryCapacity:
			return VendStatus.KioskFull;
		case LineItemStatus.Fullfilled:
			return VendStatus.Vended;
		default:
			return VendStatus.NotVended;
		}
	}

	public static Dictionary<string, object> GetCreditCardData(IShoppingSession session, bool includeTrack2)
	{
		ISession session2 = ServiceLocator.Instance.GetService<IRentalSessionService>()?.GetCurrentSession();
		ITrackData trackData = session2?.TrackData;
		IUnencryptedTrackData unencryptedTrackData = trackData as IUnencryptedTrackData;
		IEncryptedTrackData encryptedTrackData = trackData as IEncryptedTrackData;
		IEMVTrackData iEMVTrackData = trackData as IEMVTrackData;
		Dictionary<string, object> dictionary = new Dictionary<string, object>
		{
			{
				"KeyId",
				unencryptedTrackData?.EncryptionCerificateKeyId
			},
			{
				"Number",
				unencryptedTrackData?.EncryptedAccountNumber
			},
			{
				"CardType",
				trackData?.CardType
			},
			{
				"CardId",
				trackData?.CardHashId
			},
			{
				"LastFour",
				encryptedTrackData?.LastFour ?? trackData?.LastFour
			},
			{
				"LastName",
				trackData?.LastName
			},
			{
				"FirstName",
				trackData?.FirstName
			},
			{
				"PostalCode",
				session?.ShoppingCart?.PostalCode
			},
			{
				"ExpirationYear",
				trackData?.ExpiryYear
			},
			{
				"ExpirationMonth",
				trackData?.ExpiryMonth
			},
			{
				"IsEncrypted",
				encryptedTrackData != null
			},
			{
				"BIN",
				trackData?.FirstSix
			},
			{
				"KSN",
				encryptedTrackData?.KSN
			},
			{
				"ICEncryptedData",
				encryptedTrackData?.ICEncData
			},
			{
				"ReaderSerialNumber",
				encryptedTrackData?.MfgSerialNumber
			},
			{
				"EmvEnabledTerminal",
				trackData?.EmvEnabled
			},
			{
				"EmvTags",
				iEMVTrackData?.Tags
			},
			{
				"FallbackType",
				(int?)((trackData == null || !trackData.IsInTechnicalFallback || trackData.CardSourceType != CardSourceType.Swipe || !trackData.CardHasChip) ? ((FallbackType?)null) : session2?.LastFallbackType)
			},
			{
				"CardSourceType",
				trackData?.CardSourceType ?? CardSourceType.Unknown
			},
			{
				"ContactlessEnabled",
				trackData?.ContactlessEnabledAndSupportsEmv
			}
		};
		if (includeTrack2)
		{
			dictionary["Track2"] = ExtractFromSecure(unencryptedTrackData?.EncryptedTrack2);
		}
		return dictionary;
	}

	private static string ExtractFromSecure(SecureString value)
	{
		try
		{
			return Marshal.PtrToStringBSTR(Marshal.SecureStringToBSTR(value));
		}
		catch
		{
			return string.Empty;
		}
	}
}
