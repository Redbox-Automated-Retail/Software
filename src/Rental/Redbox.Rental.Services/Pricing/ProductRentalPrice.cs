namespace Redbox.Rental.Services.Pricing;

public class ProductRentalPrice
{
	public decimal InitialNight { get; set; }

	public int InitialDays { get; set; }

	public decimal ExtraNight { get; set; }

	public int NonReturnDays { get; set; }

	public decimal NonReturnPrice { get; set; }

	public decimal ExpirePrice { get; set; }

	public decimal DefaultInitialNight { get; set; }

	public decimal DefaultExtraNight { get; set; }

	public short? Attributes { get; set; }

	public bool IsMultiNightPrice { get; set; }

	public bool IsDeal { get; set; }

	public bool IsPriceTest { get; set; }
}
