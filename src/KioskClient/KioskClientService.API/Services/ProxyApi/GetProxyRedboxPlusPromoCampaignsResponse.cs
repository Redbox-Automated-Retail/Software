using System.Collections.Generic;
using KioskClientService.API.App;

namespace KioskClientService.API.Services.ProxyApi;

public class GetProxyRedboxPlusPromoCampaignsResponse : BaseResponse, IMessageScrub
{
	public List<RedboxPlusPromoCampaign> RedboxPlusPromoCampaigns { get; } = new List<RedboxPlusPromoCampaign>();

	public GetProxyRedboxPlusPromoCampaignsResponse()
		: base(200)
	{
		base.Success = true;
	}
}
