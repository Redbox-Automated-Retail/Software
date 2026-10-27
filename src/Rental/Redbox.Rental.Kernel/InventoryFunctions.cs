using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.KioskServices;
using Redbox.Lua;
using Redbox.Rental.Model;

namespace Redbox.Rental.Kernel;

public static class InventoryFunctions
{
	[KernelFunction(Name = "kernel.rental_inventory_get_rapidstatus_all")]
	internal static LuaTable GetRapidStatus()
	{
		try
		{
			return GetAllRapidStatusAsLuaFormat(null)?.ToLuaTable();
		}
		catch (Exception e)
		{
			LogHelper.Instance.LogException("InventoryFunctions.GetRapidStatus - an unhanded exception occurred.", e);
			return null;
		}
	}

	[KernelFunction(Name = "kernel.rental_inventory_get_rapidstatus_by_status")]
	internal static LuaTable GetRapidStatusByStatus(string status)
	{
		try
		{
			return GetAllRapidStatusAsLuaFormat(status)?.ToLuaTable();
		}
		catch (Exception e)
		{
			LogHelper.Instance.LogException("InventoryFunctions.GetRapidStatusByStatus - an unhanded exception occurred.", e);
			return null;
		}
	}

	[KernelFunction(Name = "kernel.rental_inventory_get_barcodes_by_status")]
	internal static LuaTable GetBarcodesByStatus(string status, bool orderByTitle)
	{
		try
		{
			List<Inventory> allInventory = GetAllInventory(status);
			if (allInventory == null)
			{
				return null;
			}
			if (orderByTitle)
			{
				return (from a in allInventory
					orderby a.TitleId
					select a.Barcode).ToList().ToLuaTable();
			}
			return (from a in allInventory
				orderby a.Barcode
				select a.Barcode).ToList().ToLuaTable();
		}
		catch (Exception e)
		{
			LogHelper.Instance.LogException("InventoryFunctions.GetBarcodesByStatus - an unhanded exception occurred.", e);
			return null;
		}
	}

	[KernelFunction(Name = "kernel.rental_inventory_get_rapidstatus")]
	internal static LuaTable GetRapidStatus(string barcode)
	{
		try
		{
			return GetRapidStatusAsLuaFormat(barcode)?.ToLuaTable();
		}
		catch (Exception e)
		{
			LogHelper.Instance.LogException("InventoryFunctions.GetRapidStatus - an unhanded exception occurred.", e);
			return null;
		}
	}

	[KernelFunction(Name = "kernel.rental_inventory_get_rebalancesin")]
	internal static LuaTable GetRebalancesIn()
	{
		LuaTable result = new List<Dictionary<string, object>>().ToLuaTable();
		try
		{
			List<Dictionary<string, object>> rebalancesInAsLuaFormat = GetRebalancesInAsLuaFormat();
			if (rebalancesInAsLuaFormat == null)
			{
				return result;
			}
			return rebalancesInAsLuaFormat.ToLuaTable();
		}
		catch (Exception e)
		{
			LogHelper.Instance.LogException("InventoryFunctions.GetRebalancesIn - an unhanded exception occurred.", e);
			return result;
		}
	}

	[KernelFunction(Name = "kernel.rental_inventory_get_fill_quantity")]
	internal static int GetFillQuantity()
	{
		try
		{
			return ServiceLocator.Instance.GetService<IInventoryService>()?.GetFillQuantity() ?? 0;
		}
		catch (Exception e)
		{
			LogHelper.Instance.LogException("InventoryFunctions.GetFillQuantity - an unhanded exception occurred.", e);
			return 0;
		}
	}

	[KernelFunction(Name = "kernel.rental_inventory_check_for_new_files")]
	internal static void CheckForNewFiles(Action inventoryCompletion, Action newRapidStatus)
	{
		try
		{
			ServiceLocator.Instance.GetService<IInventoryService>()?.CheckForNewFiles(inventoryCompletion, newRapidStatus);
		}
		catch (Exception e)
		{
			LogHelper.Instance.LogException("InventoryFunctions.CheckForNewFiles - an unhanded exception occurred.", e);
		}
	}

	[KernelFunction(Name = "kernel.rental_inventory_get_inventory_action_modification_time")]
	internal static string GetInventoryActionModificationTime(string format)
	{
		try
		{
			IInventoryService service = ServiceLocator.Instance.GetService<IInventoryService>();
			if (service == null)
			{
				return null;
			}
			if (!service.InventoryActionModificationTime.HasValue)
			{
				return null;
			}
			return service.InventoryActionModificationTime.Value.ToLocalTime().ToString(format);
		}
		catch (Exception e)
		{
			LogHelper.Instance.LogException("InventoryFunctions.GetInventoryActionModificationTime - an unhanded exception occurred.", e);
			return null;
		}
	}

	[KernelFunction(Name = "kernel.rental_inventory_get_rapid_status_modification_time")]
	internal static string GetRadpidStatusModificationTime(string format)
	{
		try
		{
			IInventoryService service = ServiceLocator.Instance.GetService<IInventoryService>();
			if (service == null)
			{
				return null;
			}
			if (!service.RapidStatusModificationTime.HasValue)
			{
				return null;
			}
			return service.RapidStatusModificationTime.Value.ToLocalTime().ToString(format);
		}
		catch (Exception e)
		{
			LogHelper.Instance.LogException("InventoryFunctions.GetRapidStatusModificationTime - an unhanded exception occurred.", e);
			return null;
		}
	}

	[KernelFunction(Name = "kernel.report_unmerchandized_barcodes")]
	internal static void ReportUnmerchandizedBarcodes(LuaTable unmerchandizedBarcodesTable)
	{
		try
		{
			IInventoryService service = ServiceLocator.Instance.GetService<IInventoryService>();
			List<string> list = new List<string>();
			foreach (LuaTable value in unmerchandizedBarcodesTable.Values)
			{
				list.Add(value["barcode"].ToString());
			}
			service?.ReportUnmerchandizedBarcodes(list);
		}
		catch (Exception e)
		{
			LogHelper.Instance.LogException("InventoryFunctions.ReportUnmerchandizedBarcodes - an unhanded exception occurred.", e);
		}
	}

	private static Dictionary<string, object> GetAllRapidStatusAsLuaFormat(string filter)
	{
		IInventoryService service = ServiceLocator.Instance.GetService<IInventoryService>();
		if (service == null)
		{
			return null;
		}
		ProductLookupCatalogModel rapidStatus = service.GetRapidStatus();
		if (rapidStatus == null)
		{
			return null;
		}
		Dictionary<string, object> dict = new Dictionary<string, object>();
		rapidStatus.Barcodes.ForEach(delegate(KeyValuePair<string, Inventory> item)
		{
			string text = item.Value.Code.ToString();
			if (item.Value.Code == InventoryStatusCode.WrongTitle)
			{
				text = "Wrong Title";
			}
			if (string.IsNullOrEmpty(filter) || text.Equals(filter, StringComparison.CurrentCultureIgnoreCase))
			{
				Dictionary<string, object> value = new Dictionary<string, object>
				{
					["barcode"] = item.Value.Barcode,
					["product_id"] = item.Value.TitleId,
					["status"] = text,
					["total_rental_count"] = item.Value.TotalRentalCount
				};
				dict[item.Value.Barcode] = value;
			}
		});
		return dict;
	}

	private static List<Inventory> GetAllInventory(string filter)
	{
		IInventoryService service = ServiceLocator.Instance.GetService<IInventoryService>();
		if (service == null)
		{
			return null;
		}
		ProductLookupCatalogModel rapidStatus = service.GetRapidStatus();
		if (rapidStatus == null)
		{
			return null;
		}
		List<Inventory> list = new List<Inventory>();
		rapidStatus.Barcodes.ForEach(delegate(KeyValuePair<string, Inventory> item)
		{
			string text = item.Value.Code.ToString();
			if (item.Value.Code == InventoryStatusCode.WrongTitle)
			{
				text = "Wrong Title";
			}
			if (string.IsNullOrEmpty(filter) || text.Equals(filter, StringComparison.CurrentCultureIgnoreCase))
			{
				list.Add(item.Value);
			}
		});
		return list;
	}

	private static Dictionary<string, object> GetRapidStatusAsLuaFormat(string barcode)
	{
		IInventoryService service = ServiceLocator.Instance.GetService<IInventoryService>();
		if (service == null)
		{
			return null;
		}
		Inventory rapidStatus = service.GetRapidStatus(barcode);
		if (rapidStatus == null)
		{
			return null;
		}
		string value = rapidStatus.Code.ToString();
		if (rapidStatus.Code == InventoryStatusCode.WrongTitle)
		{
			value = "Wrong Title";
		}
		return new Dictionary<string, object>
		{
			["barcode"] = rapidStatus.Barcode,
			["product_id"] = rapidStatus.TitleId,
			["status"] = value,
			["total_rental_count"] = rapidStatus.TotalRentalCount
		};
	}

	private static List<Dictionary<string, object>> GetRebalancesInAsLuaFormat()
	{
		IInventoryService service = ServiceLocator.Instance.GetService<IInventoryService>();
		if (service == null)
		{
			return null;
		}
		List<RebalancesInModel> rebalancesIn = service.GetRebalancesIn();
		if (rebalancesIn == null)
		{
			return null;
		}
		List<Dictionary<string, object>> result = new List<Dictionary<string, object>>();
		rebalancesIn.ForEach(delegate(RebalancesInModel item)
		{
			Dictionary<string, object> item2 = new Dictionary<string, object>
			{
				["TitleId"] = item.TitleId,
				["Quantity"] = item.Quantity
			};
			result.Add(item2);
		});
		return result;
	}

	[KernelFunction(Name = "kernel.invsvcs_return")]
	internal static void Return(string storeNumber, string returnType, string barcode, int deck, int slot, string returnTime)
	{
		InnerReturn(storeNumber, returnType, barcode, deck, slot, returnTime, failedSecurityRead: false, null, null);
	}

	[KernelFunction(Name = "kernel.invsvcs_return2")]
	internal static void Return2(string storeNumber, string returnType, string barcode, int deck, int slot, string returnTime, bool failedSecurityRead, string fileName)
	{
		InnerReturn(storeNumber, returnType, barcode, deck, slot, returnTime, failedSecurityRead, fileName, null);
	}

	private static void InnerReturn(string storeNumber, string returnType, string barcode, int deck, int slot, string returnTime, bool failedSecurityRead, string fileName, LuaFunction callback)
	{
		IInventoryService service = ServiceLocator.Instance.GetService<IInventoryService>();
		if (service == null)
		{
			return;
		}
		if (string.IsNullOrEmpty(barcode))
		{
			LogHelper.Instance.Log("A null or empty string was passed for barcode in InventoryFunctions.Return; skipping message enqueue.", LogEntryType.Error);
			return;
		}
		ICallbackService callbackService = ServiceLocator.Instance.GetService<ICallbackService>();
		if (callbackService == null)
		{
			return;
		}
		RemoteServiceCallback callback2 = null;
		if (callback != null)
		{
			callback2 = delegate(IRemoteServiceResult r)
			{
				ICallbackService callbackService2 = callbackService;
				ResultCallbackEntry resultCallbackEntry = new ResultCallbackEntry
				{
					Function = callback
				};
				object[] parameters = new LuaTable[1] { r.AsTable() };
				resultCallbackEntry.Parameters = parameters;
				callbackService2.EnqueueCallback(resultCallbackEntry);
			};
		}
		if (!DateTime.TryParse(returnTime, null, DateTimeStyles.AssumeLocal, out var result))
		{
			LogHelper.Instance.Log("Invalid date time passed to InnerReturn : {0}.", returnTime);
			result = DateTime.Now;
		}
		service.Return(storeNumber, Enum<ReturnType>.ParseIgnoringCase(returnType, ReturnType.KioskReturn), barcode, deck, slot, result.ToString(), failedSecurityRead, fileName, callback2);
	}

	[KernelFunction(Name = "kernel.invsvcs_snapshot")]
	internal static void Snapshot(string storeNumber, LuaTable items)
	{
		List<IInventoryItem> list = new List<IInventoryItem>();
		IInventoryService service = ServiceLocator.Instance.GetService<IInventoryService>();
		if (service != null)
		{
			InnerSnapshot(items, list);
			service.Snapshot(storeNumber, list.AsReadOnly(), null);
		}
	}

	private static void InnerSnapshot(LuaTable items, List<IInventoryItem> inventoryItems)
	{
		IInventoryService service = ServiceLocator.Instance.GetService<IInventoryService>();
		if (service == null || items == null)
		{
			return;
		}
		foreach (object key in items.Keys)
		{
			LuaTable luaTable = (LuaTable)items[key];
			if (luaTable == null)
			{
				LogHelper.Instance.Log($"InventoryFunctions.InnerShapshot unable to cast item key {key} to a LuaTable; skipping this item.", LogEntryType.Error);
				continue;
			}
			object obj = luaTable["deck"];
			int? num = null;
			if (obj != null)
			{
				num = Convert.ToInt32(obj);
			}
			object obj2 = luaTable["slot"];
			int? num2 = null;
			if (obj2 != null)
			{
				num2 = Convert.ToInt32(obj2);
			}
			string text = Convert.ToString(luaTable["barcode"]);
			if (string.IsNullOrEmpty(text))
			{
				LogHelper.Instance.Log($"A null or empty barcode was passed into InventoryFunctions.Snapshot where deck = {num} and slot = {num2}; skipping this item.", LogEntryType.Error);
				continue;
			}
			inventoryItems.Add(service.CreateInventoryItem(text, num, num2, Enum<InventoryItemStatus>.ParseIgnoringCase(Convert.ToString(luaTable["status"]), InventoryItemStatus.In)));
			luaTable.Dispose();
		}
		items.Dispose();
	}

	[KernelFunction(Name = "kernel.updatefraudstatus")]
	internal static void UpdateFraudStatus(string storeNumber, string barcode, string isFraud, string readTime, LuaFunction completeCallback)
	{
		IInventoryService service = ServiceLocator.Instance.GetService<IInventoryService>();
		if (service == null)
		{
			return;
		}
		ICallbackService callbackService = ServiceLocator.Instance.GetService<ICallbackService>();
		if (callbackService != null)
		{
			service.UpdateFraudStatus(storeNumber, barcode, isFraud, readTime, delegate(IRemoteServiceResult r)
			{
				ICallbackService callbackService2 = callbackService;
				ResultCallbackEntry resultCallbackEntry = new ResultCallbackEntry
				{
					Function = completeCallback
				};
				object[] parameters = new LuaTable[1] { r.AsTable() };
				resultCallbackEntry.Parameters = parameters;
				callbackService2.EnqueueCallback(resultCallbackEntry);
			});
		}
	}

	[KernelFunction(Name = "kernel.fmadiscaction")]
	internal static void FmaDiscAction(string storeNumber, string username, string jobName, string jobTimeStamp, LuaTable barcode_list, LuaFunction completeCallback)
	{
		IInventoryService service = ServiceLocator.Instance.GetService<IInventoryService>();
		if (service == null)
		{
			return;
		}
		ICallbackService callbackService = ServiceLocator.Instance.GetService<ICallbackService>();
		if (callbackService != null)
		{
			List<Dictionary<string, string>> list = new List<Dictionary<string, string>>();
			for (int i = 1; i <= barcode_list.Keys.Count; i++)
			{
				LuaTable luaTable = (LuaTable)barcode_list[i];
				Dictionary<string, string> dictionary = new Dictionary<string, string>();
				dictionary.Add("Barcode", luaTable[1] as string);
				dictionary.Add("Action", luaTable[2] as string);
				list.Add(dictionary);
			}
			service.FmaDiscAction(storeNumber, username, jobName, jobTimeStamp, list, delegate(IRemoteServiceResult r)
			{
				ICallbackService callbackService2 = callbackService;
				ResultCallbackEntry resultCallbackEntry = new ResultCallbackEntry
				{
					Function = completeCallback
				};
				object[] parameters = new LuaTable[1] { r.AsTable() };
				resultCallbackEntry.Parameters = parameters;
				callbackService2.EnqueueCallback(resultCallbackEntry);
			});
		}
	}
}
