using System.Collections.Generic;

namespace KioskClientService.API.Services.Transaction;

public static class GetCustomerOffersResponseExtensions
{
	public static ConvertedGetCustomerOffersResponse ConvertExpirationDates(this GetCustomerOffersResponse response)
	{
		ConvertedGetCustomerOffersResponse convertedGetCustomerOffersResponse = new ConvertedGetCustomerOffersResponse
		{
			Success = response.Success,
			StatusCode = response.StatusCode,
			AccountNumber = response.AccountNumber,
			CustomerProfileNumber = response.CustomerProfileNumber,
			EmailAddress = response.EmailAddress
		};
		convertedGetCustomerOffersResponse.StoredOffers = new List<ConvertedCustomerOffer>();
		foreach (CustomerOffer storedOffer in response.StoredOffers)
		{
			convertedGetCustomerOffersResponse.StoredOffers.Add(ConvertCustomerOffer(storedOffer));
		}
		convertedGetCustomerOffersResponse.NewOffers = new List<ConvertedCustomerOffer>();
		foreach (CustomerOffer newOffer in response.NewOffers)
		{
			convertedGetCustomerOffersResponse.NewOffers.Add(ConvertCustomerOffer(newOffer));
		}
		return convertedGetCustomerOffersResponse;
	}

	private static ConvertedCustomerOffer ConvertCustomerOffer(CustomerOffer offer)
	{
		return new ConvertedCustomerOffer
		{
			OfferId = offer.OfferId,
			ExpirationDate = offer.ExpirationDate?.DateTime,
			PromotionCode = offer.PromotionCode,
			IncludeTitleIds = offer.IncludeTitleIds,
			ExcludeTitleIds = offer.ExcludeTitleIds,
			ExcludeIfWithinRedboxReleaseDate = offer.ExcludeIfWithinRedboxReleaseDate,
			DiscountAmount = offer.DiscountAmount,
			DiscountType = offer.DiscountType,
			ProductType = offer.ProductType,
			IncludeFormatIds = offer.IncludeFormatIds,
			ExcludeFormatIds = offer.ExcludeFormatIds,
			OfferScopeType = offer.OfferScopeType,
			CustomerSegmentType = offer.CustomerSegmentType,
			StoredOfferId = offer.StoredOfferId,
			OfferProducts = offer.OfferProducts,
			OfferDiscountCode = offer.OfferDiscountCode,
			CampaignId = offer.CampaignId,
			RedeemedDateTime = offer.RedeemedDateTime,
			ActionCode = offer.ActionCode
		};
	}
}
