using System.Collections.Generic;
using KioskClientService.API.App;
using Newtonsoft.Json;

namespace KioskClientService.API.Services.Products.MoreLikeThis;

public class GetMoreLikeThisResponse : BaseResponse, IMessageScrub
{
	public List<long> MoreLikeThis { get; set; }

	public GetMoreLikeThisResponse()
		: base(200)
	{
		base.Success = true;
	}

	public override object Scrub()
	{
		return JsonConvert.SerializeObject(this);
	}
}
