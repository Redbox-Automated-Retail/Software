using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.KioskServices;

namespace Redbox.Rental.Model;

public interface IInventoryService
{
	DateTime? InventoryActionModificationTime { get; }

	DateTime? RapidStatusModificationTime { get; }

	void CheckForNewFiles(Action inventoryCompletion, Action newRapidStatus);

	Inventory GetRapidStatus(string barcode);

	ProductLookupCatalogModel GetRapidStatus();

	List<RebalancesInModel> GetRebalancesIn();

	int GetFillQuantity();

	Inventory GetInventory(string barcode);

	Dictionary<string, Inventory> GetInventories(List<string> barcodes);

	string GetInventoryDataMachineOrigin();

	string GetInventoryDataCreatedOn();

	long GetInventoryDataCount();

	void ReportUnmerchandizedBarcodes(List<string> barcodesToReport);

	void Return(string storeNumber, ReturnType returnType, string barcode, int deck, int slot, string returnTime, bool failedSecurityRead, string fileName, RemoteServiceCallback callback);

	void Snapshot(string storeNumber, ReadOnlyCollection<IInventoryItem> items, RemoteServiceCallback callback);

	void UpdateFraudStatus(string storeNumber, string barcode, string isFraud, string readTime, RemoteServiceCallback completeCallback, long? transactionId = null, long? discId = null, DateTime? returnDate = null);

	void FmaDiscAction(string storeNumber, string username, string jobName, string jobTimeStamp, List<Dictionary<string, string>> barcode_list, RemoteServiceCallback completeCallback);

	IInventoryItem CreateInventoryItem(string barcode, int? deck, int? slot, InventoryItemStatus status);
}
