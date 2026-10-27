using System.Collections.Generic;
using Redbox.Rental.Model.KioskProduct;

namespace Redbox.Rental.Model.ShoppingCart;

public interface IRentalShoppingCartFilterCriteria
{
	RentalShoppingCartItemAction? Action { get; set; }

	RentalShoppingCartItemSequence? Sequence { get; set; }

	TitleType TitleType { get; set; }

	TitleFamily TitleFamily { get; set; }

	List<long> IncludeProducts { get; set; }

	List<long> ExcludeProducts { get; set; }

	List<TitleType> ProductTypes { get; set; }

	int? ProductTypeId { get; set; }

	List<int> FormatIds { get; set; }

	bool IncludeMultiNightPricedItems { get; set; }

	bool IncludeFlashDeals { get; set; }

	bool IncludeLoyaltyRedeemedItems { get; set; }

	bool? IncludeRedboxPlusFreeOneNightRentalItems { get; set; }

	bool IncludeDiscountRestrictionItems { get; set; }
}
