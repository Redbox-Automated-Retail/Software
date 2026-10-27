using System;
using System.Collections.Generic;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Model.Loyalty;
using Redbox.Rental.Model.Pricing;

namespace Redbox.Rental.Model.ShoppingCart;

public interface IRentalShoppingCartTitleItem : IBaseRentalShoppingCartItem
{
	long ProductId { get; }

	long TitleRollupProductGroupId { get; }

	string TivoQueryId { get; set; }

	List<string> PersonalizationTitleTags { get; set; }

	TitleType TitleType { get; }

	Ratings Rating { get; }

	DateTime MerchandiseDate { get; }

	List<Genres> Genres { get; }

	TitleFamily TitleFamily { get; }

	ItemSourceType SourceType { get; set; }

	bool MultiDisc { get; }

	int? DiscNumber { get; }

	string OfferCode { get; set; }

	decimal PurchasePrice { get; }

	string SortName { get; }

	LineItemStatus Status { get; set; }

	string Barcode { get; set; }

	IPricingRecord PricingRecord { get; set; }

	ILoyaltyPointsRecord LoyaltyPointsRecord { get; set; }

	ITitleProduct TitleProduct { get; }

	bool HasDeal { get; }

	int RentalPosition { get; set; }

	bool IsRedboxPlusFreeOneNightRental { get; set; }

	new string ToString();
}
