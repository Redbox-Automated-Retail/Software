using System.Net;
using KioskClientService.API.App;

namespace KioskClientService.API.Services.Planograms;

public class SyncPlanogramsResponse : ApiBaseResponse
{
	public SyncPlanogramsResponse()
		: base(HttpStatusCode.OK)
	{
	}
}
