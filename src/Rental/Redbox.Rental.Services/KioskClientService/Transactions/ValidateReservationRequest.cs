namespace Redbox.Rental.Services.KioskClientService.Transactions;

public class ValidateReservationRequest : MessageBase
{
	public long ReferenceNumber { get; set; }
}
