using System;

namespace KioskClientService.API.Services.ProxyApi;

public class AcceptedOffer
{
	public string Code { get; set; }

	public DateTime? StartDate { get; set; }

	public DateTime? EndDate { get; set; }

	public string Name { get; set; }

	public string Status { get; set; }

	public string Description { get; set; }

	public string Partner { get; set; }

	public DateTime? OptedInDate { get; set; }

	public string TimeToComplete { get; set; }

	public DateTime? CompleteDate { get; set; }

	public int? ShowInBannerPriority { get; set; }

	public string LegalInformation { get; set; }

	public string SocialMediaDescription { get; set; }

	public string ProgressTrackerCode { get; set; }

	public int? CurrentValue { get; set; }

	public int? MaxValue { get; set; }

	public int? RemainderValue { get; set; }

	public string EndValueText { get; set; }
}
