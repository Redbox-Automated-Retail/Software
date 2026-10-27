namespace ProductPricing.Model;

public class DefaultRentalPricingDTO
{
	public long ProductPriceId { get; set; }

	public decimal InitialNight { get; set; }

	public decimal ExtraNight { get; set; }

	public int NonReturnDays { get; set; }

	public decimal NonReturnPrice { get; set; }

	public decimal ExpirePrice { get; set; }

	public static DefaultRentalPricingDTO FromDefaultRentalPricing(DefaultRentalPricing drp)
	{
		return new DefaultRentalPricingDTO
		{
			ProductPriceId = drp.ProductPriceId,
			InitialNight = drp.InitialNight,
			ExtraNight = drp.ExtraNight,
			NonReturnDays = drp.NonReturnDays,
			NonReturnPrice = drp.NonReturnPrice,
			ExpirePrice = drp.ExpirePrice
		};
	}
}
