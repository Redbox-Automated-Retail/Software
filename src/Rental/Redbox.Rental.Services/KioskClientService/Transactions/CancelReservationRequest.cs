namespace Redbox.Rental.Services.KioskClientService.Transactions;

public class CancelReservationRequest : MessageBase
{
	public long ReferenceNumber { get; set; }

	public string ReasonCode { get; set; }
}
