using System.Threading.Tasks;
using KioskClientService.API.App;

namespace KioskClientService.API.Services.Analytics.KioskDataService;

public interface IKioskDataServiceClient
{
	Task<APIResponse<AnalyticsResponse>> PostKioskClientEvent(KioskClientEventRequest request);
}
