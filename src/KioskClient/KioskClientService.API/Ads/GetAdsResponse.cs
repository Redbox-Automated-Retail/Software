using System.Collections.Generic;
using System.Net;
using KioskClientService.API.App;

namespace KioskClientService.API.Ads;

public class GetAdsResponse : ApiBaseResponse, IMessageScrub
{
	public List<GetAdResponse> GetAdResponses { get; set; }

	public GetAdsResponse()
		: base(HttpStatusCode.OK)
	{
		GetAdResponses = new List<GetAdResponse>();
	}

	public override object Scrub()
	{
		return this.ToJsonIndented();
	}
}
