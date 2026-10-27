using System;

namespace ProductPricing.Model;

public class RentalPricing : IRentalPricing, IPricing
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

	public decimal DefaultInitialNight { get; set; }

	public decimal DefaultExtraNight { get; set; }

	public RentalPricing Clone()
	{
		return (RentalPricing)MemberwiseClone();
	}

	public void CopyFrom(DefaultRentalPricing drp)
	{
		PricingId = drp.PricingId;
		ProductPriceId = drp.ProductPriceId;
		StartDate = drp.StartDate;
		EndDate = drp.EndDate;
		Attributes = drp.Attributes;
		DaysOfWeek = drp.DaysOfWeek;
		StartTime = drp.StartTime;
		EndTime = drp.EndTime;
		PriceLevelType = drp.PriceLevelType;
		PriceLevelTypeValue = drp.PriceLevelTypeValue;
		PriceLocationType = drp.PriceLocationType;
		PriceLocationTypeValue = drp.PriceLocationTypeValue;
		LocationRank = drp.LocationRank;
		InitialNight = drp.InitialNight;
		InitialDays = drp.InitialDays;
		ExtraNight = drp.ExtraNight;
		NonReturnDays = drp.NonReturnDays;
		NonReturnPrice = drp.NonReturnPrice;
		ExpirePrice = drp.ExpirePrice;
		DefaultInitialNight = drp.InitialNight;
		DefaultExtraNight = drp.ExtraNight;
	}
}
