using Redbox.Rental.Model.ShoppingCart;

namespace Redbox.Rental.Services.ShoppingCart;

public class UpsellItem : IUpsellItem
{
	public long OriginalProductId { get; set; }

	public long UpsellProductId { get; set; }

	public UpsellItemOfferResponse OfferResponse { get; set; }

	public long ProductGroupId { get; set; }

	public UpsellItemType TypeId { get; set; }

	public bool MatchesOrginalOrUpsellProductId(long productId)
	{
		if (OriginalProductId != productId)
		{
			return UpsellProductId == productId;
		}
		return true;
	}
}
