using System.Net;

namespace Redbox.KioskEngine.ComponentModel.ApiService.EngineCore;

public class GetStoreInfoResponse : ApiServiceBaseResponse<StoreInfo>
{
	public GetStoreInfoResponse()
	{
		base.StatusCode = HttpStatusCode.OK;
		base.Success = true;
		base.Content = new StoreInfo();
	}
}
