using Redbox.Rental.Model.ShoppingCart;

namespace Redbox.Rental.Model.Reservation;

public interface IReservedTitle
{
	string Barcode { get; set; }

	long TitleId { get; set; }

	decimal Price { get; set; }

	decimal Discount { get; set; }

	decimal DiscountedPrice { get; set; }

	RentalShoppingCartItemAction Action { get; set; }

	ReservationDiscountType DiscountType { get; set; }

	decimal LoyaltyPoints { get; set; }
}
