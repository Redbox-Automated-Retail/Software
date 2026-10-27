using System;

namespace Redbox.KioskEngine.ComponentModel.KioskServices;

public class MarketingAdImpressionData
{
	public Guid CampaignId { get; set; }

	public string Name { get; set; }

	public DateTime StartDate { get; set; }

	public DateTime EndDate { get; set; }

	public TimeSpan? StartTime { get; set; }

	public TimeSpan? EndTime { get; set; }

	public int? DaysOfWeek { get; set; }

	public DateTime ImpressionStartDate { get; set; }

	public DateTime ImpressionEndDate { get; set; }

	public int AdLocation { get; set; }

	public int Count { get; set; }
}
