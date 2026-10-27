using System.Collections.Generic;

namespace KioskClientService.API.Services.Ads;

public class Advertisement
{
	public string Id { get; set; }

	public string ProofOfPlayUrl { get; set; }

	public string ExpirationUrl { get; set; }

	public long LeaseExpiry { get; set; }

	public string DisplayAreaId { get; set; }

	public string AssetId { get; set; }

	public string AssetUrl { get; set; }

	public int Width { get; set; }

	public int Height { get; set; }

	public string MimeType { get; set; }

	public int? LengthInSeconds { get; set; }

	public int? LengthInMilliseconds { get; set; }

	public long DisplayTime { get; set; }

	public string CreativeId { get; set; }

	public string Advertiser { get; set; }

	public string DealId { get; set; }

	public string OrderId { get; set; }

	public string CreativeCategory { get; set; }

	public List<CreativeCompanionKVP> CreativeCompanion { get; set; }
}
