using System.Threading.Tasks;
using KioskClientService.API.Services.ProxyApi;

namespace KioskClientService.API.Services.Kiosk;

public interface IKioskService
{
	Task<NearbyKiosksResponse> GetNearbyKiosks(NearbyKiosksRequest request);
}
