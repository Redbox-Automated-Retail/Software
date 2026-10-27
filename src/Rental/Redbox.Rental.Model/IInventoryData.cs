using System;
using System.Collections.Generic;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.Rental.Model;

public interface IInventoryData
{
	void CheckForNewFiles(out bool rapidStatusChanged);

	Inventory GetRapidStatus(string barcode);

	Inventory GetInventory(string barcode);

	Dictionary<string, Inventory> GetInventories(List<string> barcodes);

	string GetInventoryDataMachineOrigin();

	string GetInventoryDataCreatedOn();

	long GetInventoryDataCount();

	ProductLookupCatalogModel GetRapidStatus();

	InventoryActionFile GetInventoryActionFile();

	DateTime? GetLastCompletedDate();

	void SetLastCompletedDate(DateTime dt);

	DateTime? GetLastRunThinJobDate();

	void SetLastRunThinJobDate(DateTime dt);

	DateTime? GetInventoryActionModificationTime();

	DateTime? GetRapidStatusModificationTime();

	void ReportUnmerchandizedBarcodes(List<string> barcodesToReport);

	void ReportMerchandizedBarcodes();
}
