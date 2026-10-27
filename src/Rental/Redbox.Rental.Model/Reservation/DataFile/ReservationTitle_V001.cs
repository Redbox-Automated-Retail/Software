namespace Redbox.Rental.Model.Reservation.DataFile;

public class ReservationTitle_V001
{
	public string Barcode { get; set; }

	public long TitleId { get; set; }

	public decimal DiscountedPrice { get; set; }

	public int ItemId { get; set; }

	public decimal Discount { get; set; }

	public ReservationType_V001 ReservationType { get; set; }

	public decimal Price { get; set; }

	public decimal LoyaltyPoints { get; set; }

	public ReservationDiscountType DiscountType { get; set; }
}
