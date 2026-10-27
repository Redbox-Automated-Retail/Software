using System.Collections.Generic;
using Redbox.Rental.Model;
using Redbox.Rental.Model.Promotion;
using Redbox.Rental.Model.ShoppingCart;

namespace Redbox.Rental.Services.Promotion;

public class PromoCodeValidationInfo : IPromoCodeValidationInfo
{
	public bool IsCartValid { get; set; }

	public bool IsCartInvalidBecauseOfMultiNightPricing { get; set; }

	public bool IsCartInvalidBecauseOfFlashDeal { get; set; }

	public bool IsCartInvalidBecauseOfDiscountRestriction { get; set; }

	public decimal Price { get; set; }

	public decimal Percent { get; set; }

	public string Title { get; set; }

	public string Message { get; set; }

	public IList<IPromotionItem> PromotionItems { get; set; }

	public IList<IRentalShoppingCartTitleItem> QualifyingItems { get; set; }

	public PromoCodeValidationInfo()
	{
		PromotionItems = new List<IPromotionItem>();
		QualifyingItems = new List<IRentalShoppingCartTitleItem>();
	}
}
