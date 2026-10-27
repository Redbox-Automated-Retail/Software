using System;

namespace ProductPricing.Model;

public interface IPricing
{
	long PricingId { get; set; }

	long ProductPriceId { get; set; }

	DateTime StartDate { get; set; }

	DateTime? EndDate { get; set; }

	int LocationRank { get; set; }

	byte? DaysOfWeek { get; set; }

	TimeSpan? StartTime { get; set; }

	TimeSpan? EndTime { get; set; }
}
