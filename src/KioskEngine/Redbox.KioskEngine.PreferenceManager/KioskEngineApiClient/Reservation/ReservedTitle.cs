using Redbox.Rental.Model.Reservation;
using Redbox.Rental.Model.ShoppingCart;

namespace Redbox.KioskEngine.PreferenceManager.KioskEngineApiClient.Reservation;

public class ReservedTitle
{
	public string Barcode { get; set; }

	public long TitleId { get; set; }

	public decimal Price { get; set; }

	public decimal Discount { get; set; }

	public decimal DiscountedPrice { get; set; }

	public RentalShoppingCartItemAction Action { get; set; }

	public ReservationDiscountType DiscountType { get; set; }

	public decimal LoyaltyPoints { get; set; }
}
