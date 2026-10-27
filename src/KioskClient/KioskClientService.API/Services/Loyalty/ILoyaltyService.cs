using System.Threading.Tasks;
using KioskClientService.API.App;
using KioskClientService.API.Services.ProxyApi;

namespace KioskClientService.API.Services.Loyalty;

public interface ILoyaltyService
{
	Task<APIResponse<ServerLoyaltyResponse>> EstimateAccrualAsync(CartPayload payload);

	Task<APIResponse<ServerLoyaltyResponse>> EstimateRedemptionAsync(CartPayload payload);

	Task<UpdateAccountResponse> UpdateAccountAsync(UpdateAccountPayload payload);
}
