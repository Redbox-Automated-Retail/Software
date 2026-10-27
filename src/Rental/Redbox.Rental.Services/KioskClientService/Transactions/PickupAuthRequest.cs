namespace Redbox.Rental.Services.KioskClientService.Transactions;

public class PickupAuthRequest : AuthorizeRequest
{
	public long ReferenceNumber { get; set; }

	public bool CartWasModified { get; set; }
}
