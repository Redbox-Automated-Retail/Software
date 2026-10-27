using System;
using KioskClientService.API.Services.Ads;

namespace KioskClientService.API.Ads;

public class CachedAdvertisement
{
	public Advertisement Advertisement { get; set; }

	public AdLocation AdLocation { get; set; }

	public DateTime RequestedTime { get; set; }

	public string AssetPath { get; set; }

	public bool Requested { get; set; }
}
