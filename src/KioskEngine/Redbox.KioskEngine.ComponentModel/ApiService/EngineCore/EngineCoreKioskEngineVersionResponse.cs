using System.Net;

namespace Redbox.KioskEngine.ComponentModel.ApiService.EngineCore;

public class EngineCoreKioskEngineVersionResponse : ApiServiceBaseResponse<KioskEngineVersions>
{
	public EngineCoreKioskEngineVersionResponse()
	{
		base.StatusCode = HttpStatusCode.OK;
		base.Success = true;
		base.Content = new KioskEngineVersions();
	}
}
