using System.Collections.Generic;
using System.Threading.Tasks;
using KioskClientService.API.App;

namespace KioskClientService.API.Services.KioskEngineApi;

public interface IKioskEngineApiShoppingCartClient
{
	Task<APIResponse<GetLoyaltyTestShoppingCartItemsResponse>> GetLoyaltyTestShoppingCartItemsResponse();

	Task<List<LoyaltyTestShoppingCartItem>> GetLoyaltyTestShoppingCartItems();
}
