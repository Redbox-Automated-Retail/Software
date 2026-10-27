namespace Redbox.Rental.Services.KioskClientService.Transactions;

public class ExpireReservationRequest : MessageBase
{
	public long ReferenceNumber { get; set; }
}
