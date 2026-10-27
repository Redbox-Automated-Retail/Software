namespace Redbox.Rental.Model.ShoppingCart;

public interface IUpsellItem
{
	long OriginalProductId { get; set; }

	long UpsellProductId { get; set; }

	UpsellItemOfferResponse OfferResponse { get; set; }

	long ProductGroupId { get; set; }

	UpsellItemType TypeId { get; set; }

	bool MatchesOrginalOrUpsellProductId(long productId);
}
