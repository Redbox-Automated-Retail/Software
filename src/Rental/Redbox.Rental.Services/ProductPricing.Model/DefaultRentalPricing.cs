using System;

namespace ProductPricing.Model;

public class DefaultRentalPricing : IRentalPricing, IPricing
{
	public long PricingId { get; set; }

	public long ProductPriceId { get; set; }

	public DateTime StartDate { get; set; }

	public DateTime? EndDate { get; set; }

	public short? Attributes { get; set; }

	public byte? DaysOfWeek { get; set; }

	public TimeSpan? StartTime { get; set; }

	public TimeSpan? EndTime { get; set; }

	public PriceLevelType PriceLevelType { get; set; }

	public string PriceLevelTypeValue { get; set; }

	public PriceLocationType PriceLocationType { get; set; }

	public string PriceLocationTypeValue { get; set; }

	public int LocationRank { get; set; }

	public decimal InitialNight { get; set; }

	public int InitialDays { get; set; }

	public decimal ExtraNight { get; set; }

	public int NonReturnDays { get; set; }

	public decimal NonReturnPrice { get; set; }

	public decimal ExpirePrice { get; set; }

	public DefaultRentalPricing Clone()
	{
		return (DefaultRentalPricing)MemberwiseClone();
	}
}
