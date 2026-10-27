using System.Collections.Generic;
using System.Collections.ObjectModel;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model;

namespace Redbox.KioskEngine.Environment;

internal class ProductLookupDataStoreProvider : DataStoreProvider
{
	private class Constants
	{
		public const string Version = "Version";

		public const string BarcodeToProduct = "BarcodeToProduct";

		public const string OriginalMachine = "origin_machine";

		public const string GeneratedOn = "generated_on";
	}

	protected override long OnCount(string dataStore, string tableName)
	{
		long result = 0L;
		if (!(tableName == "Version"))
		{
			if (tableName == "BarcodeToProduct")
			{
				result = ServiceLocator.Instance.GetService<IInventoryService>()?.GetInventoryDataCount() ?? (-1);
			}
		}
		else
		{
			result = 2L;
		}
		return result;
	}

	protected override object OnGet(string dataStore, string tableName, string keyName)
	{
		object result = null;
		IInventoryService service = ServiceLocator.Instance.GetService<IInventoryService>();
		if (service != null)
		{
			if (tableName == "BarcodeToProduct")
			{
				Inventory inventory = service.GetInventory(keyName);
				if (inventory != null)
				{
					string arg = inventory.Code.ToString();
					if (inventory.Code == InventoryStatusCode.WrongTitle)
					{
						arg = "Wrong Title";
					}
					result = "{ " + $"product_id = {inventory.TitleId}, status = '{arg}', total_rental_count = {inventory.TotalRentalCount}" + " }";
				}
			}
			else if (tableName == "Version")
			{
				if (!(keyName == "origin_machine"))
				{
					if (keyName == "generated_on")
					{
						result = "{ date = '" + service.GetInventoryDataCreatedOn()?.Trim() + "' }";
					}
				}
				else
				{
					result = "{ name = '" + service.GetInventoryDataMachineOrigin()?.Trim() + "' }";
				}
			}
		}
		return result;
	}

	protected override void OnBulkSet(string dataStore, string tableName, IDictionary<string, object> values)
	{
		_ = tableName == "Version";
	}

	protected override ReadOnlyCollection<string> OnGetKeys(string dataStore, string tableName)
	{
		if (!(tableName == "Version"))
		{
			if (tableName == "BarcodeToProduct")
			{
				return new List<string>().AsReadOnly();
			}
			return new List<string>().AsReadOnly();
		}
		return new List<string> { "origin_machine", "generated_on" }.AsReadOnly();
	}

	protected override ReadOnlyCollection<string> OnGetTables(string dataStore)
	{
		return new List<string> { "Version", "BarcodeToProduct" }.AsReadOnly();
	}
}
