using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using KioskClientService.API.App;
using KioskClientService.API.Services.Inventory.Models;

namespace KioskClientService.API.Services.Inventory.API;

public interface IInventoryApiClient
{
	Task<APIResponse<MerchandizingOrdersResponse>> GetMerchandizingOrders(DateTime orderDate, MerchandizingOrderType? orderType);

	Task<APIResponse<MerchandStatusUpdateResponse>> UpdateMerchandizingStatuses(List<MerchandStatusUpdate> updates);

	Task<APIResponse<GetProductGroupSortOrder>> GetProductGroupOrder(int productGroupOrderType);

	Task<InventoryResponse> Sync(InventorySnapshotRequest request);

	Task<InventoryResponse> DiscAction(DiscActionRequest request);

	Task<InventoryResponse> DiscFraud(DiscFraudRequest request);
}
