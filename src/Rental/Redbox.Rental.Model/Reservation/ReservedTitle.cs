using Redbox.Rental.Model.ShoppingCart;

namespace Redbox.Rental.Model.Reservation;

public class ReservedTitle : IReservedTitle
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
