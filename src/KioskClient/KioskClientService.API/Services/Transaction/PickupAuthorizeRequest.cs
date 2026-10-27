namespace KioskClientService.API.Services.Transaction;

public class PickupAuthorizeRequest : AuthorizeRequest
{
	public long ReferenceNumber { get; set; }

	public bool CartWasModified { get; set; }
}
