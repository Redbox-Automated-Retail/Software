using Redbox.Rental.Model.Loyalty;
using Redbox.Rental.Model.ShoppingCart;

namespace Redbox.Rental.Services.Loyalty;

public class LoyaltyPointsRecord : ILoyaltyPointsRecord
{
	private IRentalShoppingCartTitleItem _rentalShoppingCartItem;

	public int? PointsToBeEarned { get; set; }

	public int? PointsToRedeem { get; set; }

	public bool ToBeRedeemed { get; set; }

	public bool CanRedeem
	{
		get
		{
			bool result = false;
			if (PointsToRedeem > 0 && _rentalShoppingCartItem != null && _rentalShoppingCartItem.Action == RentalShoppingCartItemAction.rental)
			{
				result = true;
			}
			return result;
		}
	}

	public LoyaltyPointsRecord(IRentalShoppingCartTitleItem rentalShoppingCartItem)
	{
		_rentalShoppingCartItem = rentalShoppingCartItem;
	}

	public void Clone(ILoyaltyPointsRecord source)
	{
		if (source != null)
		{
			PointsToBeEarned = source.PointsToBeEarned;
			PointsToRedeem = source.PointsToRedeem;
			ToBeRedeemed = source.ToBeRedeemed;
		}
	}
}
