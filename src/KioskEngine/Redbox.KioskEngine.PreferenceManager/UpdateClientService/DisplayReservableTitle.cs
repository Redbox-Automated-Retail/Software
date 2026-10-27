using Redbox.Rental.Model.Reservation;

namespace Redbox.KioskEngine.PreferenceManager.UpdateClientService;

public class DisplayReservableTitle
{
	public ReservableTitle ReservableTitle { get; set; }

	public int ProductId => (int)ReservableTitle.ProductId;

	public override string ToString()
	{
		return $"Product Id: {ReservableTitle.ProductId} - {ReservableTitle.LongName}; {ReservableTitle.TitleType}";
	}
}
