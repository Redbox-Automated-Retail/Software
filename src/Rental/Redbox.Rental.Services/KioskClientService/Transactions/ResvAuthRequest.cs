namespace Redbox.Rental.Services.KioskClientService.Transactions;

public class ResvAuthRequest : AuthorizeRequest
{
	public string ReferenceNumber { get; set; }
}
