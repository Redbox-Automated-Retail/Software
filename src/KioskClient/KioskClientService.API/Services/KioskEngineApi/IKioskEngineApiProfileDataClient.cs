using System.Threading.Tasks;
using KioskClientService.API.App;

namespace KioskClientService.API.Services.KioskEngineApi;

public interface IKioskEngineApiProfileDataClient
{
	Task<APIResponse<GetKioskProductIdsResponse>> GetKioskProductIds();
}
