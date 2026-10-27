using System;
using System.Collections.Generic;
using KioskClientService.API.Services.Pricing;

namespace KioskClientService.API.Services.ProxyApi;

public class RentalPricingByPrice
{
	public DateTime StartDate { get; set; }

	public DateTime? EndDate { get; set; }

	public short? Attributes { get; set; }

	public List<PriceLevel> PriceLevel { get; set; }

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
