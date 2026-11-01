using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using DeviceService.ComponentModel;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.TrackData;
using Redbox.KioskServices.Proxy.ComponentModel;
using Redbox.KioskServices.Proxy.ComponentModel.Loyalty;
using Redbox.Rental.Model;
using Redbox.Rental.Model.Pricing;
using Redbox.Rental.Model.Session;
using Redbox.Rental.Model.ShoppingCart;

namespace Redbox.KioskServices.Proxy.Request;

public class ProxyMessageHelper
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
			KioskId = ServiceLocator.Instance.GetService<Redbox.KioskServices.Proxy.ComponentModel.IStoreServices>().StoreNumberInteger,
			CustomerProfileNumber = customerProfileNumber,
			ShoppingCart = CreateShoppingCart(sessionId)
		};
	}

	public static ShoppingCart CreateShoppingCart(Guid id)
	{
		IRentalShoppingCart obj = ServiceLocator.Instance.GetService<IRentalShoppingCartService>()?.GetRentalShoppingCart(id);
		return CreateShoppingCart(obj, obj.Promotion);
	}

	public static ShoppingCart CreateShoppingCart(IRentalShoppingCart rentalShoppingCart, IPromotion promo)
	{
		ShoppingCart shoppingCart = new ShoppingCart();
		if (rentalShoppingCart?.TitleItems?.Any() ?? false)
		{
			(rentalShoppingCart?.TitleItems?.Where((IRentalShoppingCartTitleItem x) => x?.LoyaltyPointsRecord?.ToBeRedeemed ?? false))?.ForEach(delegate(IRentalShoppingCartTitleItem x)
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
			foreach (IGrouping<RentalShoppingCartItemAction, IRentalShoppingCartTitleItem> item in from x in rentalShoppingCart.TitleItems
				group x by x.Action)
			{
				LineItemGroupType groupType = ((item.Key == RentalShoppingCartItemAction.rental) ? LineItemGroupType.Rental : LineItemGroupType.Purchase);
				List<LineItem> list = new List<LineItem>();
				foreach (IRentalShoppingCartTitleItem item2 in item)
				{
					IUpsellItem upsellItemByRentalShoppingCartItem = rentalShoppingCart.UpsellInfo.GetUpsellItemByRentalShoppingCartItem(item2);
					BlurayUpsell blurayUpsell = null;
					if (upsellItemByRentalShoppingCartItem != null)
					{
						blurayUpsell = new BlurayUpsell
						{
							ProductGroupId = (int)upsellItemByRentalShoppingCartItem.ProductGroupId,
							TitleId = item2.ProductId,
							Offer = upsellItemByRentalShoppingCartItem.OfferResponse.ToString(),
							TypeId = (int)upsellItemByRentalShoppingCartItem.TypeId
						};
					}
					IPricingRecord pricingRecord = item2?.PricingRecord;
					list.Add(new LineItem
					{
						Barcode = item2.Barcode,
						BlurayUpsell = blurayUpsell,
						SourceType = item2.SourceType,
						MultiDisc = item2.MultiDisc,
						ProductFamily = item2.TitleFamily.ToString(),
						ProductId = item2.ProductId,
						ProductType = item2.TitleType.GetDisplayName(),
						OfferCode = item2.OfferCode,
						DiscNumber = item2.DiscNumber,
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
							PriceSetId = (pricingRecord?.PriceSetId ?? 0),
							ProductPriceId = (int)(pricingRecord?.ProductPricingId ?? 0),
							Purchase = (pricingRecord?.Purchase ?? 0m)
						},
						VendStatus = ConvertToVendStatus(item2.Status),
						TivoQueryId = item2.TivoQueryId,
						PersonalizationTitleTags = item2.PersonalizationTitleTags
					});
				}
				ISubTotalGroup subTotalGroup = rentalShoppingCart.Totals.SubTotalGroups.FirstOrDefault((ISubTotalGroup y) => item.Key == y.Action);
				shoppingCart.Groups.Add(new LineItemGroup
				{
					GroupType = groupType,
					Items = list,
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

	internal static string CalcUtcOffset(DateTime now)
	{
		TimeSpan utcOffset = TimeZone.CurrentTimeZone.GetUtcOffset(now);
		return string.Format("{0}:{1}", utcOffset.TotalHours.ToString("00"), utcOffset.Minutes.ToString("00"));
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
