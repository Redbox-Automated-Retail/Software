namespace KioskClientService.API.Services.Transaction;

public class RedeemedOffer
{
	public long OfferId { get; set; }

	public long? CampaignId { get; set; }

	public long? StoredOfferId { get; set; }
}
