using System;

namespace ProductPricing.Model;

public class PurchasePricing : IPurchasePricing, IPricing
{
	public long PricingId { get; set; }

	public long ProductPriceId { get; set; }

	public DateTime StartDate { get; set; }

	public DateTime? EndDate { get; set; }

	public long ProductId { get; set; }

	public PriceLocationType PriceLocationType { get; set; }

	public string PriceLocationTypeValue { get; set; }

	public int LocationRank { get; set; }

	public byte? DaysOfWeek { get; set; }

	public TimeSpan? StartTime { get; set; }

	public TimeSpan? EndTime { get; set; }

	public decimal PurchasePrice { get; set; }

	public PurchasePricing Clone()
	{
		return (PurchasePricing)MemberwiseClone();
	}
}
