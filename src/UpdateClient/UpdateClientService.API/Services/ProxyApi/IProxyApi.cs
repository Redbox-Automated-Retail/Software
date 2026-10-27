using System.Threading.Tasks;
using Redbox.NetCore.Middleware.Http;

namespace UpdateClientService.API.Services.ProxyApi;

public interface IProxyApi
{
	Task<APIResponse<ApiBaseResponse>> RecordKioskPingFailure(KioskPingFailure kioskPingFailure);
}
