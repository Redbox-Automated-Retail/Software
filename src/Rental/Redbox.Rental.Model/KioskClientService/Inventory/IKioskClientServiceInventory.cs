using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Redbox.KioskEngine.ComponentModel.KioskServices;

namespace Redbox.Rental.Model.KioskClientService.Inventory;

public interface IKioskClientServiceInventory
{
	IGetMerchandizingOrdersResponse GetCurrentMerchandizingOrders();

	IUpdateMerchandizingStatusResponse SendMerchandizingStatusUpdates(List<MerchStatusUpdate> updates);

	IDictionary<string, object> ProcessQueuedMerchandStatusUpdates(byte[] payload);

	IKioskClientServiceResult Snapshot(string kioskId, IEnumerable<IInventoryItem> items);

	Task<IKioskClientServiceResult> SnapshotAsync(string kioskId, IEnumerable<IInventoryItem> items);

	IKioskClientServiceResult DiscAction(string kioskId, string username, string jobname, DateTime jobTimestamp, List<Dictionary<string, string>> actionList);

	Task<IKioskClientServiceResult> DiscActionAsync(string kioskId, string username, string jobname, DateTime jobTimestamp, List<Dictionary<string, string>> actionList);

	IKioskClientServiceResult DiscFraud(string storeNumber, string barcode, DateTime readDate, Dictionary<string, string> properties);

	Task<IKioskClientServiceResult> DiscFraudAsync(string storeNumber, string barcode, DateTime readDate, Dictionary<string, string> properties);

	IKioskTitlesCountResponse KioskTitlesCount(long sourceKioskId, List<long> kioskIds, List<long> titleIds);

	Task<IKioskTitlesCountResponse> KioskTitlesCountAsync(long sourceKioskId, List<long> kioskIds, List<long> titleIds);
}
