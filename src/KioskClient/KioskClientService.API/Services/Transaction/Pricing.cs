namespace KioskClientService.API.Services.Transaction;

public class Pricing
{
	public decimal InitialNight { get; set; }

	public decimal ExtraNight { get; set; }

	public decimal NonReturn { get; set; }

	public decimal Purchase { get; set; }

	public decimal Expiration { get; set; }

	public int NonReturnDays { get; set; }

	public int PriceSetId { get; set; }

	public int? ProductPriceId { get; set; }

	public int? InitialDays { get; set; }

	public decimal? DefaultInitialNight { get; set; }

	public decimal? DefaultExtraNight { get; set; }
}
