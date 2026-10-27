using System.Net;
using KioskClientService.API.App;

namespace KioskClientService.API.Services.Ads;

public class ExpireResponse : ApiBaseResponse, IMessageScrub
{
	public int? ResponseTime { get; set; }

	public ExpireResponse()
		: base(HttpStatusCode.OK)
	{
	}

	public override object Scrub()
	{
		return this.ToJsonIndented();
	}
}
