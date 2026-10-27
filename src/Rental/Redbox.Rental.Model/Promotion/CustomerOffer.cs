using System;
using System.Collections.Generic;
using Redbox.Rental.Model.KioskProduct;

namespace Redbox.Rental.Model.Promotion;

public class CustomerOffer
{
	public long OfferId { get; set; }

	public decimal Amount { get; set; }

	public OfferDiscountType DiscountType { get; set; }

	public string PromotionCode { get; set; }

	public DateTime? ExpirationDate { get; set; }

	public bool IncludeTitles { get; set; }

	public List<long> TitleIds { get; set; }

	public bool IncludeFormats { get; set; }

	public List<int> FormatIds { get; set; }

	public TitleFamily? TitleFamily { get; set; }

	public List<ITitleProduct> ApplicableProducts { get; set; }

	public TimeSpan? IncludeOlderThan { get; set; }

	public OfferScopeType OfferScopeType { get; set; }

	public CustomerSegmentType CustomerSegmentType { get; set; }

	public long? StoredOfferId { get; set; }

	public List<OfferProduct> OfferProducts { get; set; }

	public string OfferDiscountCode { get; set; }

	public bool RequiresProductInCart
	{
		get
		{
			if (DiscountType != OfferDiscountType.Free)
			{
				return DiscountType == OfferDiscountType.AmountOff;
			}
			return true;
		}
	}

	public ITitleProduct TargetProductInCart { get; set; }

	public long CampaignId { get; set; }

	public DateTime? RedeemedDateTime { get; set; }

	public ActionCode ActionCode { get; set; }

	public LegalTextType LegalTextType
	{
		get
		{
			switch (CustomerSegmentType)
			{
			case CustomerSegmentType.PersonalizedMultiNightPrice:
				return LegalTextType.Multinight;
			case CustomerSegmentType.BasketStretch:
				if (ActionCode == ActionCode.Purchase)
				{
					return LegalTextType.Purchase;
				}
				return LegalTextType.RentalShort;
			case CustomerSegmentType.Frac:
				return LegalTextType.Expires;
			default:
				return LegalTextType.Standard;
			}
		}
	}

	public bool ShouldAdjustPrice(IConfiguration configuration)
	{
		if (DiscountType != OfferDiscountType.MultiNightPrice || configuration.KioskSession.MarketingGeneral.UseDiscountsForPmnpOffers)
		{
			if (ActionCode == ActionCode.Purchase)
			{
				return string.IsNullOrEmpty(PromotionCode);
			}
			return false;
		}
		return true;
	}
}
