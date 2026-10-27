namespace Redbox.Rental.Model.Reservation;

public class ReservationRequestResultItem : IReservationRequestResultItem
{
	public string Barcode { get; set; }

	public int ProductId { get; set; }

	public int ItemId { get; set; }
}
