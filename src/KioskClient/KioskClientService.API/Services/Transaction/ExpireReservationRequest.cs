namespace KioskClientService.API.Services.Transaction;

public class ExpireReservationRequest : MessageBase
{
	public long ReferenceNumber { get; set; }
}
