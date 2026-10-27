using System;

namespace KioskClientService.API.Services.ProxyApi;

public class KioskLoyalty
{
	public bool LoyaltySystemOnline { get; set; }

	public bool IsEnrolled { get; set; }

	public int PointBalance { get; set; }

	public string CurrentTier { get; set; }

	public int CurrentTierCounter { get; set; }

	public int PointsExpiring { get; set; }

	public DateTime? PointsExpirationDate { get; set; }
}
