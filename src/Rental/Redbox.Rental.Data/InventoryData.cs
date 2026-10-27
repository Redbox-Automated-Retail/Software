using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.ProductLookupCatalog;
using Redbox.Rental.Model;
using Redbox.Rental.Model.Cache;
using Redbox.Rental.Model.DataService;
using Redbox.Rental.Model.KioskClientService;
using Redbox.Rental.Model.KioskClientService.Inventory;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Services.Configuration;

namespace Redbox.Rental.Data;

public class InventoryData : IInventoryData
{
	private class BarcodeAndTotalRents
	{
		public string Barcode { get; set; }

		public int RentalCount { get; set; }
	}

	private class BarcodeForProductResult
	{
		public long ProductId { get; set; }

		public string Barcode { get; set; }
	}

	private IKernelHelperService _kernelHelperService;

	private string _runningPath;

	private string _dataPath;

	private string _rapidStatusStagedPath;

	private string _rapidStatusActivePath;

	private string _inventoryDataPath;

	private const string InventoryDataName = "inventory.data";

	private const string RapidStatusStagedName = "RapidStatus.data";

	private const string RapidStatusActiveName = "RapidStatus.active";

	private string _inventoryActionStagedPath;

	private string _inventoryActionActivePath;

	private const string InventoryActionStagedName = "InventoryAction.data";

	private const string InventoryActionActiveName = "InventoryAction.active";

	private string _inventoryDataJsonPath;

	private const string InventoryDataJsonName = "InventoryData.json";

	private string _merchOrdersJsonPath;

	private const string MerchOrdersJsonName = "MerchOrders.json";

	private string _thinBarcodesJsonPath;

	private const string _thinBarcodesJsonName = "ThinBarcodes.json";

	private ReaderWriterLockSlim _lock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);

	private int _lockTimeout = 3000;

	private InventoryActionFile _inventoryActionFile;

	private ProductLookupCatalogModel _plcModel;

	private InventoryDataJsonFile _inventoryDataJsonFile;

	private Dictionary<string, MerchOrderBarcode> _merchOrderBarcodes;

	private List<MerchandizingOrder> _merchOrders;

	private DateTime? _inventoryActionModificationTime;

	private DateTime? _rapidStatusModificationTime;

	private DateTime? _lastMerchOrderDate;

	public string DataPath => _dataPath;

	public string RunningPath => _runningPath;

	public InventoryData(IKernelHelperService kernelHelperService)
	{
		_kernelHelperService = kernelHelperService;
		IEngineApplication service = ServiceLocator.Instance.GetService<IEngineApplication>();
		_runningPath = service.RunningPath;
		_dataPath = service.DataPath;
		_rapidStatusStagedPath = Path.Combine(_dataPath, "RapidStatus.data");
		_rapidStatusActivePath = Path.Combine(_dataPath, "RapidStatus.active");
		_inventoryActionStagedPath = Path.Combine(_dataPath, "InventoryAction.data");
		_inventoryActionActivePath = Path.Combine(_dataPath, "InventoryAction.active");
		_inventoryDataJsonPath = Path.Combine(_dataPath, "InventoryData.json");
		_inventoryDataPath = Path.Combine(_dataPath, "inventory.data");
		_thinBarcodesJsonPath = Path.Combine(_dataPath, "ThinBarcodes.json");
		_merchOrdersJsonPath = Path.Combine(_dataPath, "MerchOrders.json");
	}

	public void Initialize()
	{
		if (!_lock.TryEnterWriteLock(_lockTimeout))
		{
			LogHelper.Instance.LogError("InventoryData.Initialize", "TryEnterWriteLock timeout expired prior to acquiring the lock.");
			return;
		}
		try
		{
			LoadInventoryData();
			LoadRapidStatus();
			if (Configuration.Instance.UseMerchOrdersForThinning)
			{
				_merchOrders = LoadMerchOrderData();
				_inventoryActionFile = LoadInventoryActionFileFromMercOrders(_inventoryDataJsonFile.LastCompletedDate);
				_merchOrderBarcodes = LoadMerchOrderBarcodeData();
				MergeMerchOrdersWithRapidStatus();
			}
			else
			{
				LoadInventoryAction();
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("InventoryData.Initialize - An unhandled exception occurred.", e);
		}
		finally
		{
			_lock.ExitWriteLock();
		}
	}

	public void CheckForNewFiles(out bool rapidStatusChanged)
	{
		rapidStatusChanged = false;
		if (!_lock.TryEnterWriteLock(_lockTimeout))
		{
			LogHelper.Instance.LogError("InventoryData.CheckForNewFile", "TryEnterWriteLock timeout expired prior to acquiring the lock.");
			return;
		}
		try
		{
			rapidStatusChanged = CheckForRapidStatusStagedFile();
			if (Configuration.Instance.UseMerchOrdersForThinning)
			{
				rapidStatusChanged |= CheckForNewMerchandizingOrders();
				if (rapidStatusChanged)
				{
					MergeMerchOrdersWithRapidStatus();
				}
			}
			else
			{
				CheckForInventoryActionStagedFile();
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("InventoryData.CheckForNewFile - An unhandled exception occurred.", e);
		}
		finally
		{
			_lock.ExitWriteLock();
		}
	}

	public long GetInventoryDataCount()
	{
		long result = 0L;
		if (!_lock.TryEnterWriteLock(_lockTimeout))
		{
			LogHelper.Instance.LogError("InventoryData.GetInventoryDataCount", "TryEnterWriteLock timeout expired prior to acquiring the lock.");
		}
		else
		{
			try
			{
				using Archive archive = Archive.Open(_inventoryDataPath);
				result = archive.Count;
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log("InventoryData.GetInventoryDataCount - An unhandled exception occurred.", e);
			}
			finally
			{
				_lock.ExitWriteLock();
			}
		}
		return result;
	}

	public string GetInventoryDataMachineOrigin()
	{
		string result = null;
		if (!_lock.TryEnterWriteLock(_lockTimeout))
		{
			LogHelper.Instance.LogError("InventoryData.GetInventoryDataMachineOrigin", "TryEnterWriteLock timeout expired prior to acquiring the lock.");
		}
		else
		{
			try
			{
				using Archive archive = Archive.Open(_inventoryDataPath);
				result = archive.GetOriginMachine();
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log("InventoryData.GetInventoryDataMachineOrigin - An unhandled exception occurred.", e);
			}
			finally
			{
				_lock.ExitWriteLock();
			}
		}
		return result;
	}

	public string GetInventoryDataCreatedOn()
	{
		string result = null;
		if (!_lock.TryEnterWriteLock(_lockTimeout))
		{
			LogHelper.Instance.LogError("InventoryData.GetInventoryDataGeneratedOn", "TryEnterWriteLock timeout expired prior to acquiring the lock.");
		}
		else
		{
			try
			{
				using Archive archive = Archive.Open(_inventoryDataPath);
				result = archive.GetCreatedOn();
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log("InventoryData.GetInventoryDataGeneratedOn - An unhandled exception occurred.", e);
			}
			finally
			{
				_lock.ExitWriteLock();
			}
		}
		return result;
	}

	public Redbox.KioskEngine.ComponentModel.Inventory GetInventory(string barcode)
	{
		Redbox.KioskEngine.ComponentModel.Inventory value = null;
		if (!_lock.TryEnterWriteLock(_lockTimeout))
		{
			LogHelper.Instance.LogError("InventoryData.GetInventory", "TryEnterWriteLock timeout expired prior to acquiring the lock.");
		}
		else
		{
			try
			{
				GetInventoriesAlreadyLocked(new List<string> { barcode }).TryGetValue(barcode, out value);
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log("InventoryData.GetInventory - An unhandled exception occurred.", e);
			}
			finally
			{
				_lock.ExitWriteLock();
			}
		}
		return value;
	}

	public Dictionary<string, Redbox.KioskEngine.ComponentModel.Inventory> GetInventories(List<string> barcodes)
	{
		Dictionary<string, Redbox.KioskEngine.ComponentModel.Inventory> result = null;
		if (!_lock.TryEnterWriteLock(_lockTimeout))
		{
			LogHelper.Instance.LogError("InventoryData.GetInventories", "TryEnterWriteLock timeout expired prior to acquiring the lock.");
		}
		else
		{
			try
			{
				result = GetInventoriesAlreadyLocked(barcodes);
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log("InventoryData.GetInventories - An unhandled exception occurred.", e);
			}
			finally
			{
				_lock.ExitWriteLock();
			}
		}
		return result;
	}

	private Dictionary<string, Redbox.KioskEngine.ComponentModel.Inventory> GetInventoriesAlreadyLocked(List<string> barcodes)
	{
		if (CheckForRapidStatusStagedFile())
		{
			MergeMerchOrdersWithRapidStatus();
		}
		return GetInventoriesInternal(barcodes);
	}

	private Dictionary<string, Redbox.KioskEngine.ComponentModel.Inventory> GetInventoriesInternal(List<string> barcodes)
	{
		List<string> list = new List<string>();
		Dictionary<string, Redbox.KioskEngine.ComponentModel.Inventory> dictionary = new Dictionary<string, Redbox.KioskEngine.ComponentModel.Inventory>();
		if (_plcModel != null)
		{
			foreach (string barcode in barcodes)
			{
				_plcModel.Barcodes.TryGetValue(barcode, out var value);
				if (value != null)
				{
					dictionary[barcode] = value;
				}
				else
				{
					list.Add(barcode);
				}
			}
		}
		else
		{
			list.AddRange(barcodes);
		}
		if (list.Count > 0)
		{
			foreach (KeyValuePair<string, Redbox.KioskEngine.ComponentModel.Inventory> item in GetInventoriesFromInventoryDataFile(list))
			{
				dictionary[item.Key] = item.Value;
			}
		}
		return dictionary;
	}

	private Dictionary<string, Redbox.KioskEngine.ComponentModel.Inventory> GetInventoriesFromInventoryDataFile(List<string> barcodes)
	{
		Dictionary<string, Redbox.KioskEngine.ComponentModel.Inventory> dictionary = new Dictionary<string, Redbox.KioskEngine.ComponentModel.Inventory>();
		if (File.Exists(_inventoryDataPath))
		{
			using Archive archive = Archive.Open(_inventoryDataPath);
			foreach (string barcode in barcodes)
			{
				Redbox.ProductLookupCatalog.Inventory inventory = archive.Find(barcode);
				if (inventory != null)
				{
					dictionary[barcode] = new Redbox.KioskEngine.ComponentModel.Inventory
					{
						Barcode = barcode,
						Code = CheckForOverrideThinningStatus((Redbox.KioskEngine.ComponentModel.InventoryStatusCode)inventory.Code),
						TitleId = inventory.TitleId,
						TotalRentalCount = inventory.TotalRentalCount
					};
				}
			}
		}
		return dictionary;
	}

	private Redbox.KioskEngine.ComponentModel.InventoryStatusCode CheckForOverrideThinningStatus(Redbox.KioskEngine.ComponentModel.InventoryStatusCode code)
	{
		if (Configuration.Instance.UseMerchOrdersForThinning && (code == Redbox.KioskEngine.ComponentModel.InventoryStatusCode.Rebalancing || code == Redbox.KioskEngine.ComponentModel.InventoryStatusCode.Thinned || code == Redbox.KioskEngine.ComponentModel.InventoryStatusCode.Redeployment))
		{
			return Redbox.KioskEngine.ComponentModel.InventoryStatusCode.Known;
		}
		return code;
	}

	public Redbox.KioskEngine.ComponentModel.Inventory GetRapidStatus(string barcode)
	{
		if (!_lock.TryEnterWriteLock(_lockTimeout))
		{
			LogHelper.Instance.LogError("InventoryData.GetRapidStatus", "TryEnterWriteLock timeout expired prior to acquiring the lock.");
			return null;
		}
		try
		{
			if (CheckForRapidStatusStagedFile())
			{
				MergeMerchOrdersWithRapidStatus();
			}
			if (_plcModel == null)
			{
				return null;
			}
			if (_plcModel.Barcodes.TryGetValue(barcode, out var value))
			{
				return value;
			}
			return null;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("InventoryData.GetRapidStatus - An unhandled exception occurred.", e);
		}
		finally
		{
			_lock.ExitWriteLock();
		}
		return null;
	}

	public ProductLookupCatalogModel GetRapidStatus()
	{
		if (!_lock.TryEnterWriteLock(_lockTimeout))
		{
			LogHelper.Instance.LogError("InventoryData.GetRapidStatus", "TryEnterWriteLock timeout expired prior to acquiring the lock.");
			return null;
		}
		try
		{
			if (CheckForRapidStatusStagedFile())
			{
				MergeMerchOrdersWithRapidStatus();
			}
			return _plcModel;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("InventoryData.GetRapidStatus - An unhandled exception occurred.", e);
		}
		finally
		{
			_lock.ExitWriteLock();
		}
		return null;
	}

	public InventoryActionFile GetInventoryActionFile()
	{
		InventoryActionFile result = null;
		if (!_lock.TryEnterWriteLock(_lockTimeout))
		{
			LogHelper.Instance.LogError("InventoryData.GetInventoryActionData", "TryEnterWriteLock timeout expired prior to acquiring the lock.");
			return result;
		}
		try
		{
			if (!Configuration.Instance.UseMerchOrdersForThinning)
			{
				CheckForInventoryActionStagedFile();
			}
			return _inventoryActionFile;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("InventoryData.GetInventoryActionData - An unhandled exception occurred.", e);
			return result;
		}
		finally
		{
			_lock.ExitWriteLock();
		}
	}

	public DateTime? GetLastCompletedDate()
	{
		DateTime? result = null;
		if (!_lock.TryEnterWriteLock(_lockTimeout))
		{
			LogHelper.Instance.LogError("InventoryData.GetLastCompletedDate", "TryEnterWriteLock timeout expired prior to acquiring the lock.");
			return result;
		}
		try
		{
			if (_inventoryDataJsonFile == null)
			{
				return null;
			}
			return _inventoryDataJsonFile.LastCompletedDate;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("InventoryData.GetInventoryActionData - An unhandled exception occurred.", e);
			return result;
		}
		finally
		{
			_lock.ExitWriteLock();
		}
	}

	public DateTime? GetLastRunThinJobDate()
	{
		DateTime? result = null;
		if (!_lock.TryEnterWriteLock(_lockTimeout))
		{
			LogHelper.Instance.LogError("InventoryData.GetLastRunThinJobDate", "TryEnterWriteLock timeout expired prior to acquiring the lock.");
			return result;
		}
		try
		{
			if (_inventoryDataJsonFile == null)
			{
				return null;
			}
			return _inventoryDataJsonFile.LastRunThinJobDate;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("InventoryData.GetLastRunThinJobDate - An unhandled exception occurred.", e);
			return result;
		}
		finally
		{
			_lock.ExitWriteLock();
		}
	}

	public void SetLastCompletedDate(DateTime dt)
	{
		if (!_lock.TryEnterWriteLock(_lockTimeout))
		{
			LogHelper.Instance.LogError("InventoryData.SetLastCompletedDate", "TryEnterWriteLock timeout expired prior to acquiring the lock.");
			return;
		}
		try
		{
			if (_inventoryDataJsonFile == null)
			{
				_inventoryDataJsonFile = new InventoryDataJsonFile();
			}
			_inventoryDataJsonFile.LastCompletedDate = dt;
			WriteInventoryData();
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("InventoryData.SetLastCompletedDate - An unhandled exception occurred.", e);
		}
		finally
		{
			_lock.ExitWriteLock();
		}
	}

	public void SetLastRunThinJobDate(DateTime dt)
	{
		if (!_lock.TryEnterWriteLock(_lockTimeout))
		{
			LogHelper.Instance.LogError("InventoryData.SetLastRunThinJobDate", "TryEnterWriteLock timeout expired prior to acquiring the lock.");
			return;
		}
		try
		{
			if (_inventoryDataJsonFile == null)
			{
				_inventoryDataJsonFile = new InventoryDataJsonFile();
			}
			_inventoryDataJsonFile.LastRunThinJobDate = dt;
			WriteInventoryData();
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("InventoryData.SetLastRunThinJobDate - An unhandled exception occurred.", e);
		}
		finally
		{
			_lock.ExitWriteLock();
		}
	}

	public DateTime? GetInventoryActionModificationTime()
	{
		if (!_lock.TryEnterWriteLock(_lockTimeout))
		{
			LogHelper.Instance.LogError("InventoryData.GetInventoryActionModificationTime", "TryEnterWriteLock timeout expired prior to acquiring the lock.");
			return null;
		}
		try
		{
			return _inventoryActionModificationTime;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("InventoryData.GetInventoryActionModificationTime - An unhandled exception occurred.", e);
		}
		finally
		{
			_lock.ExitWriteLock();
		}
		return null;
	}

	public DateTime? GetRapidStatusModificationTime()
	{
		if (!_lock.TryEnterWriteLock(_lockTimeout))
		{
			LogHelper.Instance.LogError("InventoryData.GetRapidStatusModificationTime", "TryEnterWriteLock timeout expired prior to acquiring the lock.");
			return null;
		}
		try
		{
			return _rapidStatusModificationTime;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("InventoryData.GetRapidStatusModificationTime - An unhandled exception occurred.", e);
		}
		finally
		{
			_lock.ExitWriteLock();
		}
		return null;
	}

	public void ReportMerchandizedBarcodes()
	{
		if (!Configuration.Instance.UseMerchOrdersForThinning)
		{
			return;
		}
		LogHelper.Instance.Log($"InventoryData.ReportMerchandizedBarcodes called with {_merchOrderBarcodes?.Count ?? 0} barcodes from merchandizing orders.");
		if (_merchOrderBarcodes == null || _merchOrderBarcodes.Count <= 0)
		{
			return;
		}
		List<MerchStatusUpdate> list = new List<MerchStatusUpdate>();
		foreach (KeyValuePair<string, MerchOrderBarcode> item in _merchOrderBarcodes.Where((KeyValuePair<string, MerchOrderBarcode> b) => !b.Value.WasSent))
		{
			list.Add(new MerchStatusUpdate
			{
				Barcode = item.Key,
				MerchandStatus = 2L,
				ThinReasonCode = ConvertToThinReasonCode(item.Value.Code),
				ThinDateUTC = DateTime.UtcNow,
				UpdateDateTimeUTC = DateTime.UtcNow
			});
			item.Value.WasSent = true;
		}
		if (list.Count > 0)
		{
			ServiceLocator.Instance.GetService<IQueueService>().Enqueue(105, "UpdateMerchandizingStatus", list);
			WriteMerchOrderBarcodeData();
		}
	}

	public void ReportUnmerchandizedBarcodes(List<string> barcodesToReport)
	{
		if (!Configuration.Instance.UseMerchOrdersForThinning)
		{
			return;
		}
		LogHelper.Instance.Log($"InventoryService.ReportUnmerchandizedBarcodes called with {barcodesToReport?.Count ?? 0} barcodes.");
		if (barcodesToReport == null || barcodesToReport.Count <= 0)
		{
			return;
		}
		List<MerchStatusUpdate> list = new List<MerchStatusUpdate>();
		foreach (string item in barcodesToReport)
		{
			list.Add(new MerchStatusUpdate
			{
				Barcode = item,
				MerchandStatus = 1L,
				ThinReasonCode = null,
				ThinDateUTC = null,
				UpdateDateTimeUTC = DateTime.UtcNow
			});
		}
		ServiceLocator.Instance.GetService<IQueueService>().Enqueue(105, "UpdateMerchandizingStatus", list);
	}

	private long? ConvertToThinReasonCode(Redbox.KioskEngine.ComponentModel.InventoryStatusCode code)
	{
		return code switch
		{
			Redbox.KioskEngine.ComponentModel.InventoryStatusCode.Thinned => 1L, 
			Redbox.KioskEngine.ComponentModel.InventoryStatusCode.Rebalancing => 5L, 
			_ => null, 
		};
	}

	private bool CheckForRapidStatusStagedFile()
	{
		if (File.Exists(_rapidStatusStagedPath))
		{
			LogHelper.Instance.Log("InventoryData.CheckForRapidStatusStagedFile - found staged rapid status file, activating.");
			if (File.Exists(_rapidStatusActivePath))
			{
				File.Delete(_rapidStatusActivePath);
			}
			File.Move(_rapidStatusStagedPath, _rapidStatusActivePath);
			LoadRapidStatus();
			return true;
		}
		return false;
	}

	private void CheckForInventoryActionStagedFile()
	{
		if (File.Exists(_inventoryActionStagedPath))
		{
			LogHelper.Instance.Log("InventoryData.CheckForInventoryActionStagedFile - found staged inventory action file, activating.");
			if (File.Exists(_inventoryActionActivePath))
			{
				File.Delete(_inventoryActionActivePath);
			}
			File.Move(_inventoryActionStagedPath, _inventoryActionActivePath);
			LoadInventoryAction();
			if (_kernelHelperService != null)
			{
				_kernelHelperService.EnqueueStatistic("InventoryActionData", File.GetLastWriteTimeUtc(_inventoryActionActivePath).ToString("yyyy/MM/dd HH:mm:ss"));
			}
		}
	}

	private bool CheckForNewMerchandizingOrders()
	{
		bool result = false;
		IKioskClientService service = ServiceLocator.Instance.GetService<IKioskClientService>();
		DateTime date = DateTime.Now.Date;
		if ((!_lastMerchOrderDate.HasValue || _lastMerchOrderDate < DateTime.UtcNow.Date) && DateTime.Now > date + Configuration.Instance.ThinJobRangeStart && DateTime.Now < date + Configuration.Instance.ThinJobRangeEnd)
		{
			IGetMerchandizingOrdersResponse currentMerchandizingOrders = service.Inventory.GetCurrentMerchandizingOrders();
			if (currentMerchandizingOrders != null && currentMerchandizingOrders.Success)
			{
				LogHelper.Instance.Log($"InventoryData.CheckForNewMerchandizingOrders - retrieved new merch orders. {currentMerchandizingOrders.MerchandizingOrders?.Count ?? 0} orders received.");
				_lastMerchOrderDate = DateTime.UtcNow.Date;
				_inventoryDataJsonFile.LastMerchOrderDate = _lastMerchOrderDate;
				if ((currentMerchandizingOrders.MerchandizingOrders?.Count ?? 0) > 0)
				{
					result = true;
					_merchOrders = currentMerchandizingOrders.MerchandizingOrders;
					_inventoryDataJsonFile.LastMerchOrdersReceivedDate = DateTime.Now;
					_inventoryActionFile = LoadInventoryActionFileFromMercOrders(DateTime.Now);
					SetMerchandizingBarcodes(currentMerchandizingOrders.MerchandizingOrders);
					WriteMerchOrderBarcodeData();
					WriteMerchOrderData(_merchOrders);
					WriteInventoryData();
				}
			}
		}
		return result;
	}

	private void SetMerchandizingBarcodes(List<MerchandizingOrder> merchandizingOrders)
	{
		RemoveOldThinAndRebalanceFromPlc();
		List<string> list = (from b in _plcModel.Barcodes.Values
			where b.Code != Redbox.KioskEngine.ComponentModel.InventoryStatusCode.Rebalancing && b.Code != Redbox.KioskEngine.ComponentModel.InventoryStatusCode.Thinned && b.Code != Redbox.KioskEngine.ComponentModel.InventoryStatusCode.Redeployment && b.Code != Redbox.KioskEngine.ComponentModel.InventoryStatusCode.Known
			select b.Barcode).ToList();
		List<MerchandizingOrder> searchOrders = merchandizingOrders.Where((MerchandizingOrder o) => o.MerchandizingOrderType == MerchandizingOrderType.RebalancingExtract).ToList();
		List<BarcodeForProductResult> barcodesForMerchOrders = GetBarcodesForMerchOrders(searchOrders, list, new List<Redbox.KioskEngine.ComponentModel.InventoryStatusCode>
		{
			Redbox.KioskEngine.ComponentModel.InventoryStatusCode.Rebalancing,
			Redbox.KioskEngine.ComponentModel.InventoryStatusCode.Redeployment,
			Redbox.KioskEngine.ComponentModel.InventoryStatusCode.Thinned,
			Redbox.KioskEngine.ComponentModel.InventoryStatusCode.Known
		});
		list.AddRange(barcodesForMerchOrders.Select((BarcodeForProductResult b) => b.Barcode).ToList());
		List<MerchandizingOrder> searchOrders2 = merchandizingOrders.Where((MerchandizingOrder o) => o.MerchandizingOrderType == MerchandizingOrderType.ThinningExtract).ToList();
		List<BarcodeForProductResult> barcodesForMerchOrders2 = GetBarcodesForMerchOrders(searchOrders2, list, new List<Redbox.KioskEngine.ComponentModel.InventoryStatusCode>
		{
			Redbox.KioskEngine.ComponentModel.InventoryStatusCode.Thinned,
			Redbox.KioskEngine.ComponentModel.InventoryStatusCode.Redeployment,
			Redbox.KioskEngine.ComponentModel.InventoryStatusCode.Rebalancing,
			Redbox.KioskEngine.ComponentModel.InventoryStatusCode.Known
		});
		_merchOrderBarcodes = new Dictionary<string, MerchOrderBarcode>();
		foreach (BarcodeForProductResult item in barcodesForMerchOrders)
		{
			_merchOrderBarcodes.Add(item.Barcode, ConvertToPlcInventory(item, Redbox.KioskEngine.ComponentModel.InventoryStatusCode.Rebalancing));
		}
		foreach (BarcodeForProductResult item2 in barcodesForMerchOrders2)
		{
			_merchOrderBarcodes.Add(item2.Barcode, ConvertToPlcInventory(item2, Redbox.KioskEngine.ComponentModel.InventoryStatusCode.Thinned));
		}
	}

	private MerchOrderBarcode ConvertToPlcInventory(BarcodeForProductResult barcodeResult, Redbox.KioskEngine.ComponentModel.InventoryStatusCode statusCode)
	{
		return new MerchOrderBarcode
		{
			Barcode = barcodeResult.Barcode,
			TitleId = (uint)barcodeResult.ProductId,
			Code = statusCode,
			WasSent = false
		};
	}

	private List<BarcodeForProductResult> GetBarcodesForMerchOrders(List<MerchandizingOrder> searchOrders, List<string> excludeBarcodesList, List<Redbox.KioskEngine.ComponentModel.InventoryStatusCode> statusSearchList)
	{
		IDataService service = ServiceLocator.Instance.GetService<IDataService>();
		List<BarcodeForProductResult> list = new List<BarcodeForProductResult>();
		if (searchOrders != null && searchOrders.Count > 0)
		{
			if (statusSearchList == null)
			{
				statusSearchList = new List<Redbox.KioskEngine.ComponentModel.InventoryStatusCode>
				{
					Redbox.KioskEngine.ComponentModel.InventoryStatusCode.Known,
					Redbox.KioskEngine.ComponentModel.InventoryStatusCode.Thinned,
					Redbox.KioskEngine.ComponentModel.InventoryStatusCode.Redeployment,
					Redbox.KioskEngine.ComponentModel.InventoryStatusCode.Rebalancing
				};
			}
			LogHelper.Instance.Log("GetBarcodesForMerchOrders Begin.  searching for products: " + searchOrders.Select((MerchandizingOrder x) => x.TitleId).ToList().Join(", "));
			foreach (MerchandizingOrder searchOrder in searchOrders)
			{
				Dictionary<Redbox.KioskEngine.ComponentModel.InventoryStatusCode, List<BarcodeAndTotalRents>> dictionary = new Dictionary<Redbox.KioskEngine.ComponentModel.InventoryStatusCode, List<BarcodeAndTotalRents>>();
				service.GetProduct(searchOrder.TitleId, excludeTitleRollup: true, out var kioskProduct);
				if (kioskProduct is ITitleProduct titleProduct)
				{
					if (titleProduct.BarCodeProducts == null)
					{
						continue;
					}
					foreach (Redbox.KioskEngine.ComponentModel.Inventory eachInventoryEntry in GetInventoriesInternal((from b in titleProduct.BarCodeProducts.Values
						where b.Status != "Reserved"
						select b.Barcode).ToList()).Values)
					{
						if (!list.Any((BarcodeForProductResult x) => x.Barcode == eachInventoryEntry.Barcode) && (excludeBarcodesList == null || !excludeBarcodesList.Contains(eachInventoryEntry.Barcode)))
						{
							BarcodeAndTotalRents item = new BarcodeAndTotalRents
							{
								Barcode = eachInventoryEntry.Barcode,
								RentalCount = (int)eachInventoryEntry.TotalRentalCount
							};
							if (!dictionary.Keys.Contains(eachInventoryEntry.Code))
							{
								dictionary[eachInventoryEntry.Code] = new List<BarcodeAndTotalRents>();
							}
							dictionary[eachInventoryEntry.Code].Add(item);
						}
					}
					int num = searchOrder.UnitCount;
					foreach (Redbox.KioskEngine.ComponentModel.InventoryStatusCode statusSearch in statusSearchList)
					{
						if (num <= 0 || !dictionary.ContainsKey(statusSearch) || dictionary[statusSearch].Count <= 0)
						{
							continue;
						}
						foreach (BarcodeAndTotalRents item3 in dictionary[statusSearch].OrderByDescending((BarcodeAndTotalRents b) => b.RentalCount))
						{
							BarcodeForProductResult item2 = new BarcodeForProductResult
							{
								ProductId = searchOrder.TitleId,
								Barcode = item3.Barcode
							};
							list.Add(item2);
							num--;
							if (num == 0)
							{
								break;
							}
						}
					}
					if (num > 0)
					{
						LogHelper.Instance.Log($">  WARNING: Merchandizing Order for {searchOrder.MerchandizingOrderType.ToString()} for title ID {searchOrder.TitleId} specified {searchOrder.UnitCount} units but only {searchOrder.UnitCount - num} barcodes found for product id: {searchOrder.TitleId}");
					}
				}
				else
				{
					LogHelper.Instance.Log($">  Warning: Unable to find title product for title Id: {searchOrder.TitleId}");
				}
			}
			LogHelper.Instance.Log("GetBarcodesForMerchOrders End.");
		}
		return list;
	}

	private void MergeMerchOrdersWithRapidStatus()
	{
		if (!Configuration.Instance.UseMerchOrdersForThinning)
		{
			return;
		}
		RemoveOldThinAndRebalanceFromPlc();
		foreach (KeyValuePair<string, MerchOrderBarcode> merchOrderBarcode in _merchOrderBarcodes)
		{
			if (_plcModel.Barcodes.ContainsKey(merchOrderBarcode.Key))
			{
				_plcModel.Barcodes[merchOrderBarcode.Key] = merchOrderBarcode.Value;
			}
			else
			{
				_plcModel.Barcodes.Add(merchOrderBarcode.Key, merchOrderBarcode.Value);
			}
		}
	}

	private bool MerchandizingOrdersAreNotExpired()
	{
		int num = ServiceLocator.Instance.GetService<IConfiguration>()?.MerchOrderExpirationDays ?? 7;
		if (_inventoryDataJsonFile.LastMerchOrdersReceivedDate.HasValue)
		{
			return _inventoryDataJsonFile.LastMerchOrdersReceivedDate > DateTime.Now.AddDays(-num);
		}
		return true;
	}

	private void RemoveOldThinAndRebalanceFromPlc()
	{
		foreach (string item in (from b in _plcModel.Barcodes.Values
			where b.Code == Redbox.KioskEngine.ComponentModel.InventoryStatusCode.Rebalancing || b.Code == Redbox.KioskEngine.ComponentModel.InventoryStatusCode.Thinned || b.Code == Redbox.KioskEngine.ComponentModel.InventoryStatusCode.Redeployment
			select b.Barcode).ToList())
		{
			_plcModel.Barcodes.Remove(item);
		}
	}

	private InventoryActionFile LoadInventoryActionFileFromMercOrders(DateTime? lastCompletedDate)
	{
		InventoryActionFile inventoryActionFile = new InventoryActionFile
		{
			CompletedDate = lastCompletedDate?.ToString("yyyyMMddHHmmss"),
			RebalancesIn = new List<RebalancesIn>()
		};
		if (MerchandizingOrdersAreNotExpired())
		{
			foreach (MerchandizingOrder item in _merchOrders.Where((MerchandizingOrder o) => o.MerchandizingOrderType == MerchandizingOrderType.Fill))
			{
				inventoryActionFile.RebalancesIn.Add(new RebalancesIn
				{
					Id = item.TitleId,
					Qty = item.UnitCount
				});
			}
			inventoryActionFile.FillQty = 0;
		}
		return inventoryActionFile;
	}

	private void WriteInventoryData()
	{
		try
		{
			if (_inventoryDataJsonFile == null)
			{
				LogHelper.Instance.Log("InventoryData.WriteInventoryData - no data to write");
				return;
			}
			File.WriteAllBytes(_inventoryDataJsonPath, Encoding.ASCII.GetBytes(_inventoryDataJsonFile.ToJson()));
			LogHelper.Instance.Log("InventoryData.WriteInventoryData - Writing {0}.", _inventoryDataJsonFile.ToJson());
		}
		catch (Exception e)
		{
			LogHelper.Instance.LogException($"InventoryData.LoadInventoryData - An unhandled exception occurred", e);
			_inventoryDataJsonFile = null;
		}
	}

	private void WriteMerchOrderBarcodeData()
	{
		try
		{
			if (_merchOrderBarcodes == null)
			{
				LogHelper.Instance.Log("InventoryData.WriteMerchOrderBarcodeData - no data to write");
				return;
			}
			File.WriteAllBytes(_thinBarcodesJsonPath, Encoding.ASCII.GetBytes(_merchOrderBarcodes.ToJson()));
			LogHelper.Instance.Log("InventoryData.WriteMerchOrderBarcodeData - Writing {0} barcodes.", _merchOrderBarcodes.Count);
		}
		catch (Exception e)
		{
			LogHelper.Instance.LogException($"InventoryData.WriteMerchOrderBarcodeData - An unhandled exception occurred", e);
		}
	}

	private void WriteMerchOrderData(List<MerchandizingOrder> orders)
	{
		try
		{
			if (orders == null)
			{
				LogHelper.Instance.Log("InventoryData.WriteMerchOrderData - no data to write");
				return;
			}
			File.WriteAllBytes(_merchOrdersJsonPath, Encoding.ASCII.GetBytes(orders.ToJson()));
			LogHelper.Instance.Log("InventoryData.WriteMerchOrderData - Writing current orders.");
		}
		catch (Exception e)
		{
			LogHelper.Instance.LogException($"InventoryData.WriteMerchOrderData - An unhandled exception occurred", e);
		}
	}

	private void LoadInventoryData()
	{
		_inventoryDataJsonFile = null;
		try
		{
			if (!File.Exists(_inventoryDataJsonPath))
			{
				LogHelper.Instance.Log("InventoryData.LoadInventoryData - InventoryData file ({0}) does not exist", _inventoryDataJsonPath);
				return;
			}
			_inventoryDataJsonFile = Encoding.ASCII.GetString(File.ReadAllBytes(_inventoryDataJsonPath)).ToObject<InventoryDataJsonFile>();
			LogHelper.Instance.Log("InventoryData.LoadInventoryData - InventoryData file contains LastCompletedDate {0}.", _inventoryDataJsonFile.LastCompletedDate);
			_lastMerchOrderDate = _inventoryDataJsonFile.LastMerchOrderDate;
		}
		catch (Exception e)
		{
			LogHelper.Instance.LogException($"InventoryData.LoadInventoryData - An unhandled exception occurred", e);
			_inventoryDataJsonFile = null;
			SafeDelete(_inventoryDataJsonPath);
		}
	}

	private Dictionary<string, MerchOrderBarcode> LoadMerchOrderBarcodeData()
	{
		Dictionary<string, MerchOrderBarcode> dictionary = new Dictionary<string, MerchOrderBarcode>();
		try
		{
			if (!File.Exists(_thinBarcodesJsonPath))
			{
				LogHelper.Instance.Log("InventoryData.LoadMerchOrderBarcodeData - Merchandising Order file ({0}) does not exist", _thinBarcodesJsonPath);
				return dictionary;
			}
			dictionary = Encoding.ASCII.GetString(File.ReadAllBytes(_thinBarcodesJsonPath)).ToObject<Dictionary<string, MerchOrderBarcode>>();
			LogHelper.Instance.Log("InventoryData.LoadMerchOrderBarcodeData - InventoryData file contains {0} barcodes.", dictionary.Count);
		}
		catch (Exception e)
		{
			LogHelper.Instance.LogException($"InventoryData.LoadMerchOrderBarcodeData - An exception occurred", e);
			SafeDelete(_thinBarcodesJsonPath);
		}
		return dictionary;
	}

	private List<MerchandizingOrder> LoadMerchOrderData()
	{
		List<MerchandizingOrder> list = new List<MerchandizingOrder>();
		try
		{
			if (!File.Exists(_merchOrdersJsonPath))
			{
				LogHelper.Instance.Log("InventoryData.LoadMerchOrderData - Merchandising Order file ({0}) does not exist", _merchOrdersJsonPath);
				return list;
			}
			list = Encoding.ASCII.GetString(File.ReadAllBytes(_merchOrdersJsonPath)).ToObject<List<MerchandizingOrder>>();
			LogHelper.Instance.Log("InventoryData.LoadMerchOrderData - Merchandizing Order file contains {0} orders.", list.Count);
		}
		catch (Exception e)
		{
			LogHelper.Instance.LogException($"InventoryData.LoadMerchOrderData - An exception occurred", e);
			SafeDelete(_merchOrdersJsonPath);
		}
		return list;
	}

	private void LoadRapidStatus()
	{
		try
		{
			_plcModel = null;
			_rapidStatusModificationTime = null;
			if (!File.Exists(_rapidStatusActivePath))
			{
				LogHelper.Instance.Log("InventoryData.LoadRapidStatus - RapidStatus file ({0}) does not exist", _rapidStatusActivePath);
				return;
			}
			ProductLookupCatalogModel productLookupCatalog = ServiceLocator.Instance.GetService<IProductLookupCatalogService>().GetProductLookupCatalog(_rapidStatusActivePath);
			if (productLookupCatalog == null)
			{
				LogHelper.Instance.LogError("InventoryData.LoadRapidStatus", "An error occurred loading file. Deleting file.");
				SafeDelete(_rapidStatusActivePath);
			}
			_plcModel = productLookupCatalog;
			_rapidStatusModificationTime = File.GetLastWriteTime(_rapidStatusActivePath);
			LogHelper.Instance.Log("InventoryData.LoadRapidStatus - Rapid status file contains {0} barcodes.", productLookupCatalog.Barcodes.Count);
		}
		catch (Exception e)
		{
			LogHelper.Instance.LogException($"InventoryData.LoadRapidStatus - An unhandled exception occurred", e);
		}
	}

	private void LoadInventoryAction()
	{
		_inventoryActionFile = null;
		_inventoryActionModificationTime = null;
		try
		{
			if (!File.Exists(_inventoryActionActivePath))
			{
				LogHelper.Instance.Log("InventoryData.LoadInventoryAction - InventoryAction file ({0}) does not exist", _inventoryActionActivePath);
				return;
			}
			_inventoryActionFile = Encoding.ASCII.GetString(File.ReadAllBytes(_inventoryActionActivePath)).ToObject<InventoryActionFile>();
			_inventoryActionModificationTime = File.GetLastWriteTime(_inventoryActionActivePath);
			LogHelper.Instance.Log("InventoryData.LoadInventoryAction - InventoryAction file contains {0} rebalance titles, Completed Date {1}, Fill Quantity {2}.", _inventoryActionFile.RebalancesIn.Count, _inventoryActionFile.CompletedDate, _inventoryActionFile.FillQty);
		}
		catch (Exception e)
		{
			LogHelper.Instance.LogException($"InventoryData.LoadInventoryAction - An unhandled exception occurred", e);
			_inventoryActionFile = null;
			SafeDelete(_inventoryActionActivePath);
		}
	}

	private void SafeDelete(string fileName)
	{
		try
		{
			if (File.Exists(fileName))
			{
				LogHelper.Instance.Log("InventoryData.SafeDelete - file ({0})", fileName);
				File.Delete(fileName);
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.LogException($"InventoryData.SafeDelete - file ({fileName})", e);
		}
	}
}
