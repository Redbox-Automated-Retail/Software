using System.Net;
using KioskClientService.API.App;

namespace KioskClientService.API.Services.Ads;

public class ProofOfPlayResponse : ApiBaseResponse, IMessageScrub
{
	public int? ResponseTime { get; set; }

	public ProofOfPlayResponse()
		: base(HttpStatusCode.OK)
	{
	}

	public override object Scrub()
	{
		return this.ToJsonIndented();
	}
}
