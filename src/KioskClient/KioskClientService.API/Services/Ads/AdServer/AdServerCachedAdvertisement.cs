using System;

namespace KioskClientService.API.Services.Ads.AdServer;

public class AdServerCachedAdvertisement
{
	public Ad Ad { get; set; }

	public DateTime RequestedTime { get; set; }

	public string AssetPath { get; set; }

	public bool Requested { get; set; }
}
