namespace KioskClientService.API.Services.Transaction;

public class ResvAuthRequest : AuthorizeRequest
{
	public string ReferenceNumber { get; set; }
}
