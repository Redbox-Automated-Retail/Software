using System;

namespace KioskClientService.API.Services.ProxyApi;

public abstract class BaseDetails
{
	public long CampaignId { get; set; }

	public DateTime StartDate { get; set; }

	public DateTime? EndDate { get; set; }

	public CampaignType CampaignType { get; set; }

	public int Rank { get; set; }
}
