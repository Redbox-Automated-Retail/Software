using System;
using KioskClientService.API.Services.Pricing;
using Newtonsoft.Json;

namespace KioskClientService.API.Services.ProxyApi;

public class RentalPricing
{
	[JsonIgnore]
	public long PricingId { get; set; }

	[JsonIgnore]
	public long ProductPriceId { get; set; }

	public DateTime StartDate { get; set; }

	public DateTime? EndDate { get; set; }

	public short? Attributes { get; set; }

	public PriceLevelType PriceLevelType { get; set; }

	public string PriceLevelTypeValue { get; set; }

	public int LocationRank { get; set; }

	public decimal InitialNight { get; set; }

	public int InitialDays { get; set; }

	public decimal ExtraNight { get; set; }

	public int NonReturnDays { get; set; }

	public decimal NonReturnPrice { get; set; }

	public decimal ExpirePrice { get; set; }

	internal ProductRentalPrice ToProductRentalPrice()
	{
		return new ProductRentalPrice
		{
			InitialNight = InitialNight,
			ExtraNight = ExtraNight,
			InitialDays = InitialDays,
			ExpirePrice = ExpirePrice,
			NonReturnDays = NonReturnDays,
			NonReturnPrice = NonReturnPrice,
			Attributes = Attributes,
			DefaultInitialNight = InitialNight,
			DefaultExtraNight = ExtraNight
		};
	}
}
