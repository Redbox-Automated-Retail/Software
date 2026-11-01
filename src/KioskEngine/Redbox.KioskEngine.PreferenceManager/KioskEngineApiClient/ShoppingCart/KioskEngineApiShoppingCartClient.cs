using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Redbox.Core;
using Redbox.Rental.Model.Loyalty;
using Redbox.Rental.Services.App;

namespace Redbox.KioskEngine.PreferenceManager.KioskEngineApiClient.ShoppingCart;

public class KioskEngineApiShoppingCartClient : KioskEngineApiBaseClient
{
	private static string _apiPath = "api/shoppingcart";

	public static async Task<APIResponse<GetLoyaltyTestShoppingCartItemsResponse>> GetLoyaltyTestShoppingCartItemsResponse()
	{
		IHttpService httpService = ServiceLocator.Instance.GetService<IHttpService>();
		HttpRequestMessage request = httpService.GenerateRequest(KioskEngineApiBaseClient.KioskEngineApiBaseUrl, _apiPath + "/loyaltytestshoppingcartitems", null, HttpMethod.Get);
		return await httpService.SendRequestAsync<GetLoyaltyTestShoppingCartItemsResponse>(request, null, "GetLoyaltyTestShoppingCartItemsResponse", "c:\\build\\Redbox.KioskEngine.PreferenceManager\\KioskEngineApiClient\\ShoppingCart\\KioskEngineApiShoppingCartClient.cs");
	}

	public static async Task<List<LoyaltyTestShoppingCartItem>> GetLoyaltyTestShoppingCartItems()
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
