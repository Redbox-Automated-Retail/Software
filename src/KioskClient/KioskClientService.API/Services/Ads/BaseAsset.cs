using System.IO;
using KioskClientService.API.Ads;

namespace KioskClientService.API.Services.Ads;

public abstract class BaseAsset
{
	public string AssetId { get; set; }

	public string AssetUrl { get; set; }

	public string MimeType { get; set; }

	public string GetAssetFileName()
	{
		return Path.ChangeExtension(AssetId, "json");
	}

	public string GetAssetCreativeFileName()
	{
		string ext = MediaTypeNames.GetExt(MimeType);
		return Path.ChangeExtension(AssetId, ext);
	}
}
