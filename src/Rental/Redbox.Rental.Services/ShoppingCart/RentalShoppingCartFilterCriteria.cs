using System.Collections.Generic;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Model.ShoppingCart;

namespace Redbox.Rental.Services.ShoppingCart;

public class RentalShoppingCartFilterCriteria : IRentalShoppingCartFilterCriteria
{
	public RentalShoppingCartItemAction? Action { get; set; }

	public RentalShoppingCartItemSequence? Sequence { get; set; }

	public TitleType TitleType { get; set; }

	public TitleFamily TitleFamily { get; set; }

	public List<long> IncludeProducts { get; set; }

	public List<long> ExcludeProducts { get; set; }

	public List<TitleType> ProductTypes { get; set; }

	public int? ProductTypeId { get; set; }

	public List<int> FormatIds { get; set; }

	public bool IncludeMultiNightPricedItems { get; set; }

	public bool IncludeFlashDeals { get; set; }

	public bool IncludeLoyaltyRedeemedItems { get; set; }

	public bool? IncludeRedboxPlusFreeOneNightRentalItems { get; set; }

	public bool IncludeDiscountRestrictionItems { get; set; }
}
