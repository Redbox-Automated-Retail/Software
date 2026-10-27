namespace Redbox.Rental.Model.Reservation;

public interface IReservationRequestResultItem
{
	string Barcode { get; set; }

	int ProductId { get; set; }

	int ItemId { get; set; }
}
