using System.Collections.Generic;
using Redbox.KioskEngine.ComponentModel.KioskServices;
using Redbox.Rental.Model.Analytics;
using Redbox.Rental.Model.ShoppingCart;

namespace Redbox.Rental.Model;

public interface IPromotion
{
	DiscountType DiscountType { get; set; }

	string Name { get; set; }

	int CampaignId { get; set; }

	decimal OriginalPrice { get; set; }

	decimal Price { get; set; }

	int Quantity { get; set; }

	PromotionIntentCode Intent { get; set; }

	decimal OriginalPercent { get; set; }

	decimal Percent { get; set; }

	byte RentQuantity { get; set; }

	PromotionRentFormat? RentFormat { get; set; }

	byte GetQuantity { get; set; }

	PromotionGetFormat? GetFormat { get; set; }

	ICampaignTitles CampaignTitles { get; set; }

	int? ProductTypeId { get; set; }

	List<int> FormatIds { get; set; }

	IList<IRentalShoppingCartTitleItem> QualifyingItems { get; set; }

	IList<IPromotionItem> PromotionItems { get; set; }

	bool IsProductSpecific { get; }

	bool IsValidatedPostVend { get; set; }

	bool IsExternallyValidated { get; }

	PromotionActionCode ActionCode { get; set; }

	RentalShoppingCartItemAction CartItemAction { get; }

	bool IsRedboxPlusFreeOneNightRental { get; }

	bool AllowFullDiscount { get; set; }

	IDefaultPromo DefaultPromo { get; set; }

	bool IsReturnVisit { get; set; }
}
