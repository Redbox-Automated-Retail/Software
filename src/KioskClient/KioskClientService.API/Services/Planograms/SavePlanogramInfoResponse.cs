using System.Net;
using KioskClientService.API.App;

namespace KioskClientService.API.Services.Planograms;

public class SavePlanogramInfoResponse : ApiBaseResponse
{
	public SavePlanogramInfoResponse()
		: base(HttpStatusCode.OK)
	{
	}
}
