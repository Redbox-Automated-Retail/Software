namespace Redbox.Rental.Model.Reservation.DataFile;

public class ReservationPricingRecord_V001
{
	public string TitleFamily { get; set; }

	public string TitleType { get; set; }

	public decimal InitialNight { get; set; }

	public decimal ExtraNight { get; set; }

	public decimal ExpirationPrice { get; set; }

	public decimal NonReturn { get; set; }

	public decimal NonReturnDays { get; set; }
}
