namespace Redbox.Rental.Model.Reservation;

public class ReservationRequestTitle
{
	public int ItemID { get; set; }

	public int TitleID { get; set; }

	public ReservationTitleType ReservationType { get; set; }

	public decimal Price { get; set; }

	public decimal Discount { get; set; }

	public ReservationDiscountType DiscountType { get; set; }

	public decimal DiscountedPrice { get; set; }

	public int NumberOfCredits { get; set; }

	public decimal LoyaltyPoints { get; set; }
}
