namespace Redbox.Rental.Services.KioskClientService.Transactions;

public class GCAuthResult : AuthorizeResult
{
	public GiftCardSummary GiftCardSummary { get; set; }

	public GCAuthResult()
	{
		GiftCardSummary = new GiftCardSummary();
	}
}
