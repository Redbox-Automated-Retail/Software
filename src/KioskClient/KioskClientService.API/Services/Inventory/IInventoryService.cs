using System.Collections.Generic;
using System.Threading.Tasks;
using KioskClientService.API.Services.Inventory.Models;

namespace KioskClientService.API.Services.Inventory;

public interface IInventoryService
{
	Task<GetMerchandizingOrdersResponse> GetAllCurrentMerchandizingOrders();

	Task<UpdateMerchandizingStatusResponse> UpdateMerchandizingStatus(List<MerchandStatusUpdate> updates);

	Task<InventoryResponse> Sync(InventorySnapshotRequest request);

	Task<InventoryResponse> DiscAction(DiscActionRequest request);

	Task<InventoryResponse> DiscFraud(DiscFraudRequest request);

	Task<KioskTitlesCountResponse> KioskTitlesCount(KiosksAndTitlesPayload payload);
}
