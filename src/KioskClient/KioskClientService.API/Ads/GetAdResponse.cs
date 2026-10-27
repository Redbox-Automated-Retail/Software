using System.Net;
using KioskClientService.API.App;
using KioskClientService.API.Services.Ads;
using KioskClientService.API.Services.Ads.AdServer;

namespace KioskClientService.API.Ads;

public class GetAdResponse : ApiBaseResponse, IMessageScrub
{
	public string AdvertisementId { get; set; }

	public int Width { get; set; }

	public int Height { get; set; }

	public string MimeType { get; set; }

	public string AssetPath { get; set; }

	public AdLocation AdLocation { get; set; }

	public GetAdResponse()
		: base(HttpStatusCode.OK)
	{
	}

	public override object Scrub()
	{
		return this.ToJsonIndented();
	}

	public GetAdResponse CopyFrom(Advertisement ad)
	{
		AdvertisementId = ad.Id;
		Width = ad.Width;
		Height = ad.Height;
		MimeType = ad.MimeType;
		return this;
	}

	public GetAdResponse CopyFrom(CachedAdvertisement ca)
	{
		AdLocation = ca.AdLocation;
		AssetPath = ca.AssetPath;
		CopyFrom(ca.Advertisement);
		return this;
	}

	public GetAdResponse CopyFrom(AdServerCachedAdvertisement ca)
	{
		AdvertisementId = ca.Ad.Id;
		AdLocation = ca.Ad.Location;
		AssetPath = ca.AssetPath;
		return this;
	}
}
