namespace ProductPricing.Model;

public class RentalPricingDTO
{
	public long ProductPriceId { get; set; }

	public short? Attributes { get; set; }

	public decimal InitialNight { get; set; }

	public int InitialDays { get; set; }

	public decimal ExtraNight { get; set; }

	public int NonReturnDays { get; set; }

	public decimal NonReturnPrice { get; set; }

	public decimal ExpirePrice { get; set; }

	public decimal DefaultInitialNight { get; set; }

	public decimal DefaultExtraNight { get; set; }

	public static RentalPricingDTO FromRentalPricing(RentalPricing rp)
	{
		return new RentalPricingDTO
		{
			ProductPriceId = rp.ProductPriceId,
			InitialNight = rp.InitialNight,
			InitialDays = rp.InitialDays,
			ExtraNight = rp.ExtraNight,
			NonReturnDays = rp.NonReturnDays,
			NonReturnPrice = rp.NonReturnPrice,
			ExpirePrice = rp.ExpirePrice,
			DefaultInitialNight = rp.InitialNight,
			DefaultExtraNight = rp.ExtraNight,
			Attributes = rp.Attributes
		};
	}

	public static RentalPricingDTO CreateFromDefaultRentalPricing(DefaultRentalPricingDTO rp)
	{
		return new RentalPricingDTO
		{
			ProductPriceId = rp.ProductPriceId,
			InitialNight = rp.InitialNight,
			InitialDays = 1,
			ExtraNight = rp.ExtraNight,
			NonReturnDays = rp.NonReturnDays,
			NonReturnPrice = rp.NonReturnPrice,
			ExpirePrice = rp.ExpirePrice,
			DefaultInitialNight = rp.InitialNight,
			DefaultExtraNight = rp.ExtraNight
		};
	}

	public bool HasAttribute(PricingAttributes pa)
	{
		if (!Attributes.HasValue)
		{
			return false;
		}
		return ((PricingAttributes)Attributes.Value).HasFlag(pa);
	}
}
