using System.Collections.Generic;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Model.ShoppingCart;

namespace Redbox.Rental.Model;

public interface IPromotionRule
{
	string SatisfyCondition { get; set; }

	string OfferCondition { get; set; }

	RentalShoppingCartItemAction? Action { get; set; }

	RentalShoppingCartItemSequence? Sequence { get; set; }

	TitleType TitleType { get; set; }

	TitleFamily TitleFamily { get; set; }

	List<long> IncludeProducts { get; set; }

	List<long> ExcludeProducts { get; set; }

	List<TitleType> ProductTypes { get; set; }

	int MinimumQuantity { get; set; }

	int MaximumQuantity { get; set; }

	bool? AskOnlyOnce { get; set; }

	string RedemptionRule { get; set; }

	string RedemptionAmount { get; set; }

	string RedemptionCode { get; set; }

	string OfferLanguage { get; set; }

	string OfferMessageVersion { get; set; }

	string OfferAction { get; set; }

	string InCartPromoHeaderMessage { get; set; }

	string InCartPromoBodyMessage { get; set; }

	string InCartPromoYesButton { get; set; }

	string InCartPromoNoButton { get; set; }
}
