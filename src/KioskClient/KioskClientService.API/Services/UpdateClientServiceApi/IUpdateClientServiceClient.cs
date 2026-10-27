using System.Threading.Tasks;
using KioskClientService.API.App;
using KioskClientService.API.Services.ProfileData;

namespace KioskClientService.API.Services.UpdateClientServiceApi;

public interface IUpdateClientServiceClient
{
	Task<APIResponse<GetRecordChangesResponse>> GetRecordChanges<T>(DataUpdateRequest request);
}
