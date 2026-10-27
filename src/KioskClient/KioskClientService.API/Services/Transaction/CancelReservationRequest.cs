namespace KioskClientService.API.Services.Transaction;

public class CancelReservationRequest : MessageBase
{
	public long ReferenceNumber { get; set; }

	public string ReasonCode { get; set; }
}
