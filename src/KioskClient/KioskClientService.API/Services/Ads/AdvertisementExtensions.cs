using System.Collections.Generic;
using KioskClientService.API.Vistar;

namespace KioskClientService.API.Services.Ads;

public static class AdvertisementExtensions
{
	public static Advertisement ToAdvertisement(this VistarAdvertisement va)
	{
		Advertisement advertisement = new Advertisement
		{
			Id = va.id,
			ProofOfPlayUrl = va.proof_of_play_url,
			ExpirationUrl = va.expiration_url,
			LeaseExpiry = va.lease_expiry,
			DisplayAreaId = va.display_area_id,
			AssetId = va.asset_id,
			AssetUrl = va.asset_url,
			Width = va.width,
			Height = va.height,
			MimeType = va.mime_type,
			LengthInSeconds = va.length_in_seconds,
			LengthInMilliseconds = va.length_in_milliseconds,
			DisplayTime = va.display_time,
			CreativeId = va.creative_id,
			Advertiser = va.advertiser,
			DealId = va.deal_id,
			OrderId = va.order_id,
			CreativeCategory = va.creative_category
		};
		if (va.creative_companion != null)
		{
			advertisement.CreativeCompanion = va.creative_companion.ToCreativeCompanion();
		}
		return advertisement;
	}

	public static List<Advertisement> ToAdvertisements(this List<VistarAdvertisement> valist)
	{
		List<Advertisement> list = new List<Advertisement>();
		valist.ForEach(delegate(VistarAdvertisement x)
		{
			list.Add(x.ToAdvertisement());
		});
		return list;
	}

	public static Asset ToAsset(this Advertisement ad)
	{
		return new Asset
		{
			AssetId = ad.AssetId,
			AssetUrl = ad.AssetUrl,
			Width = ad.Width,
			Height = ad.Height,
			MimeType = ad.MimeType,
			LengthInSeconds = (ad.LengthInSeconds ?? 2),
			LengthInMilliseconds = (ad.LengthInMilliseconds ?? 2000),
			CreativeId = ad.CreativeId,
			Advertiser = ad.Advertiser,
			OrderId = ad.OrderId,
			CreativeCategory = ad.CreativeCategory,
			CampaignId = string.Empty,
			CreativeName = string.Empty
		};
	}

	public static List<CreativeCompanionKVP> ToCreativeCompanion(this List<VistarCreativeCompanionKVP> vistarCreativeCompanion)
	{
		List<CreativeCompanionKVP> creativeCompanion = new List<CreativeCompanionKVP>();
		vistarCreativeCompanion.ForEach(delegate(VistarCreativeCompanionKVP c)
		{
			creativeCompanion.Add(new CreativeCompanionKVP
			{
				Name = c.name,
				Value = c.value
			});
		});
		return creativeCompanion;
	}
}
