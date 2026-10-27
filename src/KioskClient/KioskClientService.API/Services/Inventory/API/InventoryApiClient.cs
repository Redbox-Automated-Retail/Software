using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using KioskClientService.API.App;
using KioskClientService.API.Services.Configuration;
using KioskClientService.API.Services.Inventory.Models;
using Newtonsoft.Json;

namespace KioskClientService.API.Services.Inventory.API;

public class InventoryApiClient : IInventoryApiClient
{
	private readonly IKioskConfiguration _kioskConfiguration;

	private readonly IHttpService _httpService;

	private long _kioskId;

	private const string _getMerchandizingOrdersEndpoint = "api/merchandizingorder/{KioskId}/{OrderDate}";

	private const string _updateMerchandizingStatusEndpoint = "api/disc/updatemerchandizingstatus";

	private const string _orderTypeField = "orderType";

	private const string _getProductGroupOrderEndpoint = "api/productgroupsortorder/{productGroupSortOrderType}";

	private const string _discActionEndpoint = "api/disc/discaction";

	private const string _discFraudEndpoint = "api/disc/discfraud";

	private const string _syncEndpoint = "api/kiosk/sync";

	private const string _inventoryAuthHeader = "x-api-key";

	public InventoryApiClient(IStoreService store, IOptionsSnapshotKioskConfiguration optionsSnapshotKioskConfiguration, IHttpService httpService)
	{
		_kioskConfiguration = optionsSnapshotKioskConfiguration;
		_httpService = httpService;
		if (_kioskConfiguration.KioskClientDefault.InventoryApiTimeout != 0)
		{
			_httpService.Timeout = _kioskConfiguration.KioskClientDefault.InventoryApiTimeout;
		}
		_kioskId = store?.KioskId ?? 0;
	}

	private List<Header> GetHeaders(long? kioskId = null)
	{
		KioskCloudClientConfiguration kioskCloudClientConfiguration = new KioskCloudClientConfiguration
		{
			APIKey = _kioskConfiguration?.KioskClientDefault?.InventoryApiKey,
			AuthHeader = "x-api-key"
		};
		return kioskCloudClientConfiguration.GetKioskCloudHeaders(null, kioskId);
	}

	public async Task<APIResponse<MerchandizingOrdersResponse>> GetMerchandizingOrders(DateTime orderDate, MerchandizingOrderType? orderType)
	{
		string text = "api/merchandizingorder/{KioskId}/{OrderDate}".Replace("{KioskId}", _kioskId.ToString()).Replace("{OrderDate}", orderDate.ToString("yyyy-MM-dd"));
		if (orderType.HasValue)
		{
			text = text + "?orderType=" + (int)orderType.Value;
		}
		return await _httpService.SendRequest<MerchandizingOrdersResponse>(_kioskConfiguration.KioskClientDefault.InventoryApiUrl, text, null, HttpMethod.Get, GetHeaders(_kioskId), logResponseContent: false);
	}

	public async Task<APIResponse<MerchandStatusUpdateResponse>> UpdateMerchandizingStatuses(List<MerchandStatusUpdate> updates)
	{
		return await _httpService.SendRequest<MerchandStatusUpdateResponse>(_kioskConfiguration.KioskClientDefault.InventoryApiUrl, "api/disc/updatemerchandizingstatus", updates, HttpMethod.Post, GetHeaders(_kioskId));
	}

	public async Task<APIResponse<GetProductGroupSortOrder>> GetProductGroupOrder(int productGroupSortOrderType)
	{
		string endpoint = "api/productgroupsortorder/{productGroupSortOrderType}".Replace("{productGroupSortOrderType}", productGroupSortOrderType.ToString());
		return await _httpService.SendRequest<GetProductGroupSortOrder>(_kioskConfiguration.KioskClientDefault.InventoryApiUrl, endpoint, null, HttpMethod.Get, GetHeaders(_kioskId));
	}

	public async Task<InventoryResponse> Sync(InventorySnapshotRequest request)
	{
		APIResponse<InventoryResponse> response = await _httpService.SendGZipRequest<InventoryResponse>(_kioskConfiguration.KioskClientDefault.InventoryApiUrl, "api/kiosk/sync", request, HttpMethod.Post, HeadersFromRequest(request));
		InventoryResponse inventoryResponse;
		if (response.HttpResponse == null)
		{
			inventoryResponse = new InventoryResponse
			{
				Success = false,
				StatusCode = 500
			};
			inventoryResponse.Errors.Add(new Error
			{
				Code = response.HttpResponse.StatusCode.ToString(),
				Message = "No response from api call"
			});
		}
		else
		{
			string text = await response.HttpResponse.Content.ReadAsStringAsync();
			if (response.HttpResponse.IsSuccessStatusCode)
			{
				inventoryResponse = JsonConvert.DeserializeObject<InventoryResponse>(text);
			}
			else
			{
				inventoryResponse = new InventoryResponse
				{
					Success = false,
					StatusCode = (int)response.HttpResponse.StatusCode
				};
				inventoryResponse.Errors.Add(new Error
				{
					Code = response.HttpResponse.StatusCode.ToString(),
					Message = text
				});
			}
		}
		return inventoryResponse;
	}

	public async Task<InventoryResponse> DiscAction(DiscActionRequest request)
	{
		APIResponse<InventoryResponse> response = await _httpService.SendRequest<InventoryResponse>(_kioskConfiguration.KioskClientDefault.InventoryApiUrl, "api/disc/discaction", request, HttpMethod.Post, HeadersFromRequest(request));
		InventoryResponse inventoryResponse;
		if (response.HttpResponse == null)
		{
			inventoryResponse = new InventoryResponse
			{
				Success = false,
				StatusCode = 500
			};
			inventoryResponse.Errors.Add(new Error
			{
				Code = response.HttpResponse.StatusCode.ToString(),
				Message = "No response from api call"
			});
		}
		else
		{
			string text = await response.HttpResponse.Content.ReadAsStringAsync();
			if (response.HttpResponse.IsSuccessStatusCode)
			{
				inventoryResponse = JsonConvert.DeserializeObject<InventoryResponse>(text);
			}
			else
			{
				inventoryResponse = new InventoryResponse
				{
					Success = false,
					StatusCode = (int)response.HttpResponse.StatusCode
				};
				inventoryResponse.Errors.Add(new Error
				{
					Code = response.HttpResponse.StatusCode.ToString(),
					Message = text
				});
			}
		}
		return inventoryResponse;
	}

	public async Task<InventoryResponse> DiscFraud(DiscFraudRequest request)
	{
		APIResponse<InventoryResponse> response = await _httpService.SendRequest<InventoryResponse>(_kioskConfiguration.KioskClientDefault.InventoryApiUrl, "api/disc/discfraud", request, HttpMethod.Post, HeadersFromRequest(request));
		InventoryResponse inventoryResponse;
		if (response.HttpResponse == null)
		{
			inventoryResponse = new InventoryResponse
			{
				Success = false,
				StatusCode = 500
			};
			inventoryResponse.Errors.Add(new Error
			{
				Code = response.HttpResponse.StatusCode.ToString(),
				Message = "No response from api call"
			});
		}
		else
		{
			string text = await response.HttpResponse.Content.ReadAsStringAsync();
			if (response.HttpResponse.IsSuccessStatusCode)
			{
				inventoryResponse = JsonConvert.DeserializeObject<InventoryResponse>(text);
			}
			else
			{
				inventoryResponse = new InventoryResponse
				{
					Success = false,
					StatusCode = (int)response.HttpResponse.StatusCode
				};
				inventoryResponse.Errors.Add(new Error
				{
					Code = response.HttpResponse.StatusCode.ToString(),
					Message = text
				});
			}
		}
		return inventoryResponse;
	}

	private List<Header> HeadersFromRequest(InventoryBaseRequest request)
	{
		List<Header> headers = GetHeaders();
		headers.Remove(headers.SingleOrDefault((Header h) => h.Key == "x-redbox-activityid"));
		headers.Add(new Header("x-redbox-activityid", request.MessageId.ToString()));
		headers.Remove(headers.SingleOrDefault((Header h) => h.Key == "x-redbox-kioskid"));
		headers.Add(new Header("x-redbox-kioskid", request.KioskId.ToString()));
		return headers;
	}
}
