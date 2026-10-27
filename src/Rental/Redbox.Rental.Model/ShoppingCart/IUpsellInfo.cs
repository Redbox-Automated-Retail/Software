using System.Collections.Generic;
using Redbox.Rental.Model.KioskProduct;

namespace Redbox.Rental.Model.ShoppingCart;

public interface IUpsellInfo
{
	IList<IUpsellItem> Items { get; }

	bool GetDoNotShowAgain(TitleType titleType);

	void SetDoNotShowAgain(TitleType titleType, bool value);

	bool GetDialogNeedsToBeShown(TitleType titleType);

	void AddMissingUpsellItems(TitleType titleType);

	void RejectUndecidedUpsellItems();

	void RemoveUndecidedUpsellItems();

	void SetUpsellOffer(long originalProductId, UpsellItemOfferResponse offerResponse);

	IUpsellItem GetUpsellItemByRentalShoppingCartItem(IRentalShoppingCartTitleItem rentalShoppingCartItem);
}
