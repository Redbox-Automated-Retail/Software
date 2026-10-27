using Redbox.Rental.Model.KioskProduct;

namespace Redbox.Rental.Model.Reservation;

public class ReservableTitle
{
	public long ProductId { get; set; }

	public string LongName { get; set; }

	public TitleType TitleType { get; set; }

	public TitleFamily TitleFamily { get; set; }

	public int BarcodeCount { get; set; }

	public decimal OneNightRentalPrice { get; set; }

	public decimal PurchasePrice { get; set; }
}
