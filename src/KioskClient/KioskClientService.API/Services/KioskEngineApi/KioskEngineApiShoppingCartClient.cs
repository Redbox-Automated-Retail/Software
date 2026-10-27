using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using KioskClientService.API.App;

namespace KioskClientService.API.Services.KioskEngineApi;

public class KioskEngineApiShoppingCartClient : KioskEngineApiBaseClient, IKioskEngineApiShoppingCartClient
{
	private readonly IHttpService _httpService;

	private static string _apiPath = "api/shoppingcart";

	public KioskEngineApiShoppingCartClient(IHttpService httpService)
	{
		_httpService = httpService;
	}

	public async Task<APIResponse<GetLoyaltyTestShoppingCartItemsResponse>> GetLoyaltyTestShoppingCartItemsResponse()
	{
		return await _httpService.SendRequest<GetLoyaltyTestShoppingCartItemsResponse>(KioskEngineApiBaseClient.KioskEngineApiBaseUrl, _apiPath + "/loyaltytestshoppingcartitems", null, HttpMethod.Get);
	}

	public async Task<List<LoyaltyTestShoppingCartItem>> GetLoyaltyTestShoppingCartItems()
	{
		List<LoyaltyTestShoppingCartItem> result = new List<LoyaltyTestShoppingCartItem>();
		APIResponse<GetLoyaltyTestShoppingCartItemsResponse> aPIResponse = await GetLoyaltyTestShoppingCartItemsResponse();
		if (aPIResponse.StatusCode == HttpStatusCode.OK)
		{
			result = aPIResponse.Response.Content;
		}
		return result;
	}
}
