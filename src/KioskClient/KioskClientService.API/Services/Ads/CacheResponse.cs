using System.Net;
using KioskClientService.API.App;
using Newtonsoft.Json;

namespace KioskClientService.API.Services.Ads;

public class CacheResponse : ApiBaseResponse, IMessageScrub
{
	public int? ResponseTime { get; set; }

	public int AssetCacheCount { get; set; }

	public CacheResponse()
		: base(HttpStatusCode.OK)
	{
	}

	public override object Scrub()
	{
		return JsonConvert.SerializeObject(this);
	}
}
