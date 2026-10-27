using System.Collections.Generic;
using KioskClientService.API.Services.Ads.AdServer;
using KioskClientService.API.Vistar;

namespace KioskClientService.API.Services.Ads;

public static class AssetExtensions
{
	public static Asset ToAsset(this VistarAsset va)
	{
		return new Asset
		{
			AssetId = va.asset_id,
			CreativeId = va.creative_id,
			OrderId = va.order_id,
			CampaignId = va.campaign_id,
			AssetUrl = va.asset_url,
			Width = va.width,
			Height = va.height,
			MimeType = va.mime_type,
			LengthInSeconds = va.length_in_seconds,
			LengthInMilliseconds = va.length_in_milliseconds,
			CreativeCategory = va.creative_category,
			Advertiser = va.advertiser,
			CreativeName = va.creative_name
		};
	}

	public static AdServerAsset ToAdServerAsset(this Ad ad)
	{
		return new AdServerAsset
		{
			AssetId = ad.AssetId,
			AssetUrl = ad.AssetUrl,
			MimeType = ad.MimeType
		};
	}

	public static List<Asset> ToAssets(this List<VistarAsset> valist)
	{
		List<Asset> list = new List<Asset>();
		valist.ForEach(delegate(VistarAsset x)
		{
			list.Add(x.ToAsset());
		});
		return list;
	}
}
