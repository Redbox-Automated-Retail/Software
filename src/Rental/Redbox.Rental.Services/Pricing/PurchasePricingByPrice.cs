using System;
using System.Collections.Generic;

namespace Redbox.Rental.Services.Pricing;

public class PurchasePricingByPrice
{
	public DateTime StartDate { get; set; }

	public DateTime? EndDate { get; set; }

	public decimal PurchasePrice { get; set; }

	public List<long> Titles { get; set; }

	internal ProductPurchasePrice ToProductPurchasePrice()
	{
		return new ProductPurchasePrice
		{
			PurchasePrice = PurchasePrice
		};
	}
}
