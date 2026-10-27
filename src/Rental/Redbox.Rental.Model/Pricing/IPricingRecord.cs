using System.Collections.Generic;
using Redbox.Rental.Model.KioskProduct;

namespace Redbox.Rental.Model.Pricing;

public interface IPricingRecord
{
	TitleFamily TitleFamily { get; set; }

	TitleType TitleType { get; set; }

	decimal InitialNight { get; set; }

	decimal ExtraNight { get; set; }

	decimal ExpirationPrice { get; set; }

	decimal NonReturn { get; set; }

	decimal NonReturnDays { get; set; }

	decimal PromoValue { get; set; }

	decimal Purchase { get; set; }

	long? ProductPricingId { get; set; }

	int? InitialDays { get; set; }

	decimal? DefaultInitialNightPrice { get; set; }

	decimal? DefaultExtraNightPrice { get; set; }

	int? PriceSetId { get; set; }

	bool IsMultiNightPrice { get; }

	bool IsDeal { get; }

	IPricingRecord Clone();

	bool IsEqual(IPricingRecord pricingRecord);

	Dictionary<string, object> ToDictionary();
}
