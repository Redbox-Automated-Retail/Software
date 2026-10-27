namespace ProductPricing.Model;

public interface IRentalPricing : IPricing
{
	short? Attributes { get; set; }

	PriceLevelType PriceLevelType { get; set; }

	string PriceLevelTypeValue { get; set; }

	decimal InitialNight { get; set; }

	int InitialDays { get; set; }
}
