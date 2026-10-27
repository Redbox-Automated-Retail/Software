using System.Collections.Generic;
using System.Threading.Tasks;
using KioskClientService.API.App;

namespace KioskClientService.API.Vistar;

public interface IVistarAPIClient
{
	Task<APIResponse<VistarGetAssetsResponse>> GetAssets(long kioskId, List<VistarDisplayArea> displayArea);

	Task<APIResponse<VistarGetAdResponse>> GetAd(long kioskId, List<VistarDisplayArea> displayArea);

	Task<APIResponse<VistarProofOfPlayResponse>> GetProofOfPlay(VistarProofOfPlayRequest request);

	Task<APIResponse<VistarExpirationResponse>> GetExpiration(VistarExpirationRequest request);
}
