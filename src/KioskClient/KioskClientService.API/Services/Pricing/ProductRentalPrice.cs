using Newtonsoft.Json;

namespace KioskClientService.API.Services.Pricing;

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

	public bool IsMultiNightPrice => InitialDays > 1;

	public bool IsDeal
	{
		get
		{
			if (IsPriceTest)
			{
				return false;
			}
			return InitialNight < DefaultInitialNight;
		}
	}

	public bool IsPriceTest => HasAttribute(PricingAttributes.PriceTest);

	[JsonIgnore]
	public bool IsValid
	{
		get
		{
			if (InitialNight <= 0m || InitialDays < 1 || ExtraNight <= 0m || NonReturnDays <= 0 || NonReturnPrice <= 0m)
			{
				return false;
			}
			return true;
		}
	}

	private bool HasAttribute(PricingAttributes pa)
	{
		if (!Attributes.HasValue)
		{
			return false;
		}
		return ((PricingAttributes)Attributes.Value).HasFlag(pa);
	}
}
