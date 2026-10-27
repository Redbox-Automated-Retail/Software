using System;
using KioskClientService.API.Ads;

namespace KioskClientService.API.Services.Ads.AdServer;

public class Ad
{
	public string Id { get; set; } = Guid.NewGuid().ToString();

	public string AssetId { get; set; }

	public AdProviderName Provider { get; set; }

	public bool Expirable { get; set; }

	public string AssetUrl { get; set; }

	public AdLocation Location { get; set; }

	public string MimeType { get; set; }
}
