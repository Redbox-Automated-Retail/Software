namespace KioskClientService.API.Services.Transaction;

public class BlurayUpsell
{
	public int ProductGroupId { get; set; }

	public string Offer { get; set; }

	public long TitleId { get; set; }

	public int? TypeId { get; set; }
}
