namespace KioskClientService.API.Services.Transaction;

public class GCAuthResponse : AuthorizeResponse
{
	public GiftCardSummary GiftCardSummary { get; set; }

	public GCAuthResponse()
	{
		GiftCardSummary = new GiftCardSummary();
	}
}
