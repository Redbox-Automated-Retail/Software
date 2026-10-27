using System.IO;

namespace KioskClientService.API.Services.Ads;

public class Asset : BaseAsset
{
	public string CreativeId { get; set; }

	public string OrderId { get; set; }

	public string CampaignId { get; set; }

	public int Width { get; set; }

	public int Height { get; set; }

	public int LengthInSeconds { get; set; }

	public int LengthInMilliseconds { get; set; }

	public string CreativeCategory { get; set; }

	public string Advertiser { get; set; }

	public string CreativeName { get; set; }

	public static string GetAssetFileName(string assetId)
	{
		return Path.ChangeExtension(assetId, "json");
	}

	public static string GetAssetIdFromFileName(string fileName)
	{
		return Path.GetFileNameWithoutExtension(fileName);
	}
}
