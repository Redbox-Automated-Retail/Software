using System.Collections.Generic;
using System.Net;
using KioskClientService.API.App;

namespace KioskClientService.API.Services.ProxyApi;

public class GetPlanogramsResponse : ApiBaseResponse, IMessageScrub
{
	public List<PlanogramInfo> Planograms { get; } = new List<PlanogramInfo>();

	public GetPlanogramsResponse()
		: base(HttpStatusCode.OK)
	{
	}
}
