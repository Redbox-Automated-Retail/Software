using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using KioskClientService.API.App;
using KioskClientService.API.Services.App;
using KioskClientService.API.Services.Configuration;
using KioskClientService.API.Services.DataTableService;
using KioskClientService.API.Services.KioskEngineApi;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace KioskClientService.API.Services.ProfileData;

public class ProfileDataService : IProfileDataService
{
	private class GetProductIdsForKioskResponse
	{
		public bool Success { get; set; }

		public List<RecordRequest> RecordRequests { get; set; } = new List<RecordRequest>();
	}

	private class ProductIdStatistics
	{
		public int ProductsFromKiosk { get; set; }

		public int ProductsFromProductGroups { get; set; }
	}

	private readonly ILogger<ProfileDataService> _logger;

	private readonly IKioskEngineApiProfileDataClient _kioskEngineApiProfileDataClient;

	private readonly ProfileDataConfiguration _profileDataConfiguration;

	private readonly IPersistentDataCacheService _persistentDataCacheService;

	private readonly ProfileDataSettings _profileDataSettings;

	private readonly IDataTableService _dataTableService;

	private static ProfileDataInstance _profileDataInstance;

	private static ConcurrentDictionary<Type, bool> _pendingTableUpdates = new ConcurrentDictionary<Type, bool>();

	private static int _processingProfileDataInstanceChanges = 0;

	private const string ProfileDataUpdateStatusFileName = "ProfileDataUpdateStatus.json";

	public ProfileDataService(ILogger<ProfileDataService> logger, IKioskEngineApiProfileDataClient kioskEngineApiProfileDataClient, IOptionsSnapshotKioskConfiguration optionsSnapshotKioskConfiguration, IApplicationSettings applicationSettings, IPersistentDataCacheService persistentDataCacheService, IDataTableService dataTableService)
	{
		_logger = logger;
		_kioskEngineApiProfileDataClient = kioskEngineApiProfileDataClient;
		_profileDataConfiguration = optionsSnapshotKioskConfiguration.ProfileData;
		_profileDataSettings = applicationSettings.AppSettings.ProfileDataSettings;
		_persistentDataCacheService = persistentDataCacheService;
		_dataTableService = dataTableService;
	}

	public GetProfileDataInstancePendingUpdateResponse HasPendingUpdates()
	{
		GetProfileDataInstancePendingUpdateResponse getProfileDataInstancePendingUpdateResponse = new GetProfileDataInstancePendingUpdateResponse();
		try
		{
			getProfileDataInstancePendingUpdateResponse.HasPendingUpdate = _profileDataInstance == null || _pendingTableUpdates.Any();
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "Unable to get pending updates", "HasPendingUpdates");
			getProfileDataInstancePendingUpdateResponse.StatusCode = HttpStatusCode.InternalServerError;
		}
		return getProfileDataInstancePendingUpdateResponse;
	}

	public async Task<GetProfileDataInstanceResponse> GetProfileDataInstance(bool refreshFromFiles = false)
	{
		GetProfileDataInstanceResponse response = new GetProfileDataInstanceResponse();
		if (Interlocked.CompareExchange(ref _processingProfileDataInstanceChanges, 1, 0) == 1)
		{
			_logger.LogInfoWithSource("Locking prevented getting ProfileDataInstance", "GetProfileDataInstance");
			response.StatusCode = HttpStatusCode.InternalServerError;
			return response;
		}
		try
		{
			if ((_profileDataInstance == null) | refreshFromFiles)
			{
				_pendingTableUpdates.Clear();
				ProfileDataInstance tempProfileDataInstance = new ProfileDataInstance();
				ProfileDataInstance profileDataInstance = tempProfileDataInstance;
				profileDataInstance.Products = (await _dataTableService.GetDataTable<Product>())?.GetData();
				_profileDataInstance = tempProfileDataInstance;
			}
			response.ProfileDataInstance = _profileDataInstance;
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "Exception while getting ProfileDataInstance", "GetProfileDataInstance");
			response.StatusCode = HttpStatusCode.InternalServerError;
		}
		finally
		{
			_processingProfileDataInstanceChanges = 0;
		}
		return response;
	}

	public async Task ExecuteScheduledDataUpdateIfNeeded()
	{
		DataTableUpdateStatus dataTableUpdateStatus = await GetDataTableUpdateStatus<Product>();
		if (dataTableUpdateStatus?.IsDataTableUpdateNeeded(_profileDataSettings) ?? true)
		{
			_logger.LogInfoWithSource($"Scheduled DataUpdate DataTable: {dataTableUpdateStatus?.UpdateDataTable}, LastUpdateAttempt: {dataTableUpdateStatus?.LastUpdateAttempt}, NextRetryIntervalMinutes: {dataTableUpdateStatus?.NextRetryIntervalMinutes},  LastSuccessfullUpdate: {dataTableUpdateStatus?.LastSuccessfullUpdate}", "ExecuteScheduledDataUpdateIfNeeded");
			await TriggerProductDataUpdate();
		}
	}

	private async Task<DataTableUpdateStatus> GetDataTableUpdateStatus<T>(ProfileDataUpdateStatus profileDataUpdateStatus = null)
	{
		UpdateDataTable updateDataTable = _dataTableService.GetUpdateDataTableAttribute<T>();
		if (profileDataUpdateStatus == null)
		{
			profileDataUpdateStatus = await GetProfileDataUpdateStatusFromPersistentDataCacheService();
		}
		return profileDataUpdateStatus?.UpdateStatusList?.FirstOrDefault((DataTableUpdateStatus x) => x.UpdateDataTable == updateDataTable);
	}

	private async Task<ProfileDataUpdateStatus> GetProfileDataUpdateStatusFromPersistentDataCacheService()
	{
		ProfileDataUpdateStatus result = new ProfileDataUpdateStatus();
		try
		{
			result = (await _persistentDataCacheService.Read<ProfileDataUpdateStatus>("ProfileDataUpdateStatus.json", useCache: true, log: false))?.Data;
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "Exception getting ProfileDataUpdateStatus", "GetProfileDataUpdateStatusFromPersistentDataCacheService");
		}
		return result;
	}

	public async Task<DataTable<Product>> GetProducts()
	{
		return await _dataTableService.GetDataTable<Product>();
	}

	private async Task<bool> RecordUpdateAttempt<T>()
	{
		return await WriteDataTableUpdateStatus<T>(delegate(DataTableUpdateStatus dataTableUpdateStatus)
		{
			dataTableUpdateStatus.LastUpdateAttempt = DateTime.Now;
			dataTableUpdateStatus.BackoffRetryInterval(_profileDataSettings);
		});
	}

	private async Task<bool> RecordUpdateSuccess<T>()
	{
		return await WriteDataTableUpdateStatus<T>(delegate(DataTableUpdateStatus dataTableUpdateStatus)
		{
			dataTableUpdateStatus.LastSuccessfullUpdate = DateTime.Now;
			dataTableUpdateStatus.InitializeNextRetryInterval(_profileDataSettings);
		});
	}

	private async Task<bool> WriteDataTableUpdateStatus<T>(Action<DataTableUpdateStatus> updateAction)
	{
		ProfileDataUpdateStatus profileDataUpdateStatus = await GetProfileDataUpdateStatusFromPersistentDataCacheService();
		if (profileDataUpdateStatus == null)
		{
			profileDataUpdateStatus = new ProfileDataUpdateStatus();
		}
		UpdateDataTable updateDataTable = _dataTableService.GetUpdateDataTableAttribute<T>();
		if (!_dataTableService.IsValidUpdateDataTable(updateDataTable))
		{
			return false;
		}
		DataTableUpdateStatus dataTableUpdateStatus = await GetDataTableUpdateStatus<T>(profileDataUpdateStatus);
		if (dataTableUpdateStatus == null)
		{
			dataTableUpdateStatus = new DataTableUpdateStatus
			{
				UpdateDataTable = updateDataTable
			};
			profileDataUpdateStatus.UpdateStatusList.Add(dataTableUpdateStatus);
		}
		updateAction?.Invoke(dataTableUpdateStatus);
		return await _persistentDataCacheService.Write(profileDataUpdateStatus, "ProfileDataUpdateStatus.json");
	}

	public async Task<TriggerProductDataUpdateResponse> TriggerProductDataUpdate()
	{
		TriggerProductDataUpdateResponse response = new TriggerProductDataUpdateResponse();
		try
		{
			if (!(await RecordUpdateAttempt<Product>()))
			{
				_logger.LogErrorWithSource("Error while recording data update attempt for Product", "TriggerProductDataUpdate");
				return response;
			}
			GetProductIdsForKioskResponse getProductIdsForKioskResponse = await GetProductIdsForKiosk();
			if (getProductIdsForKioskResponse == null || !getProductIdsForKioskResponse.Success)
			{
				response.StatusCode = HttpStatusCode.InternalServerError;
				return response;
			}
			DataUpdateResult result = await RequestDataTableUpdate<Product>(getProductIdsForKioskResponse.RecordRequests);
			List<long> list = await GetMissingProductIdsFromProductGroups();
			if (list.Any())
			{
				_logger.LogInfoWithSource($"Triggering update of {list.Count} that were found in product groups", "TriggerProductDataUpdate");
				List<RecordRequest> missingProductRecordRequests = new List<RecordRequest>();
				list.ForEach(delegate(long eachMissingProductId)
				{
					missingProductRecordRequests.Add(new RecordRequest
					{
						Id = eachMissingProductId
					});
				});
				DataUpdateResult dataUpdateResult = await RequestDataTableUpdate<Product>(missingProductRecordRequests);
				result.Success &= dataUpdateResult.Success;
				result.TableFileUpdated |= dataUpdateResult.TableFileUpdated;
			}
			response.StatusCode = (result.Success ? HttpStatusCode.OK : HttpStatusCode.InternalServerError);
			if (result.Success)
			{
				await RecordUpdateSuccess<Product>();
			}
		}
		catch (Exception ex)
		{
			response.StatusCode = HttpStatusCode.InternalServerError;
			_logger.LogErrorWithSource(ex, "Exception while trigger product data update", "TriggerProductDataUpdate");
		}
		return response;
	}

	private async Task<DataUpdateResult> RequestDataTableUpdate<T>(List<RecordRequest> recordRequests)
	{
		if (!_profileDataConfiguration.ProfileDataUpdateEnabled)
		{
			_logger.LogWarningWithSource("DataTable update disabled by configuration setting ProfileDataUpdateEnabled", "RequestDataTableUpdate");
			return new DataUpdateResult
			{
				Success = false
			};
		}
		DataUpdateResult dataUpdateResult = await _dataTableService.RequestDataTableUpdate<T>(recordRequests);
		if (dataUpdateResult != null && dataUpdateResult.TableFileUpdated)
		{
			_pendingTableUpdates.TryAdd(typeof(T), value: true);
		}
		return dataUpdateResult;
	}

	public async Task<AddDataRecordResponse<Product>> AddProducts(List<long> productIds)
	{
		AddDataRecordResponse<Product> firstPassResponse = await AddDataRecords<Product>(productIds);
		if (firstPassResponse.StatusCode == HttpStatusCode.OK)
		{
			List<long> list = await GetMissingProductIdsFromProductGroups(firstPassResponse.DataObjects);
			if (list != null && list.Any())
			{
				AddDataRecordResponse<Product> addDataRecordResponse = await AddDataRecords<Product>(list);
				if (addDataRecordResponse.StatusCode == HttpStatusCode.OK)
				{
					firstPassResponse.DataObjects.AddRange(addDataRecordResponse.DataObjects);
				}
				firstPassResponse.StatusCode = addDataRecordResponse.StatusCode;
			}
		}
		return firstPassResponse;
	}

	private async Task<AddDataRecordResponse<T>> AddDataRecords<T>(List<long> ids)
	{
		if (!_profileDataConfiguration.ProfileDataUpdateEnabled)
		{
			_logger.LogWarningWithSource("Add DataRecords disabled by configuration setting ProfileDataUpdateEnabled", "AddDataRecords");
			return new AddDataRecordResponse<T>
			{
				StatusCode = HttpStatusCode.InternalServerError
			};
		}
		AddDataRecordResponse<T> addDataRecordResponse = await _dataTableService.AddDataRecords<T>(ids);
		if (addDataRecordResponse != null && addDataRecordResponse.TableFileUpdated)
		{
			_pendingTableUpdates.TryAdd(typeof(T), value: true);
		}
		return addDataRecordResponse;
	}

	private async Task<GetProductIdsForKioskResponse> GetProductIdsForKiosk()
	{
		GetProductIdsForKioskResponse response = new GetProductIdsForKioskResponse();
		HashSet<long> productIds = new HashSet<long>();
		HashSet<long> hashSet = productIds;
		hashSet.UnionWith(await _dataTableService.GetExistingDataRecordIds<Product>());
		APIResponse<GetKioskProductIdsResponse> aPIResponse = await _kioskEngineApiProfileDataClient.GetKioskProductIds();
		if (aPIResponse == null || aPIResponse.StatusCode != HttpStatusCode.OK)
		{
			_logger.LogErrorWithSource("Unable to get product ids for kiosk from Kiosk Engine", "GetProductIdsForKiosk");
			return response;
		}
		productIds.UnionWith(aPIResponse.Response.Content);
		ProductIdStatistics productIdStatistics = new ProductIdStatistics
		{
			ProductsFromKiosk = aPIResponse.Response.Content.Count
		};
		hashSet = productIds;
		hashSet.UnionWith(await GetMissingProductIdsFromProductGroups());
		foreach (long item in productIds)
		{
			response.RecordRequests.Add(new RecordRequest
			{
				Id = item
			});
		}
		response.Success = true;
		_logger.LogInfoWithSource("Kiosk Product ids count: " + JsonConvert.SerializeObject(productIdStatistics), "GetProductIdsForKiosk");
		return response;
	}

	private async Task<List<long>> GetMissingProductIdsFromProductGroups()
	{
		List<long> missingProductIds = new List<long>();
		DataTable<Product> dataTable = await GetProducts();
		if (dataTable?.Data?.Any() == true)
		{
			List<Product> list = new List<Product>();
			list.AddRange(from x in dataTable.Data.ToList()
				select x.Value.Data);
			missingProductIds = await GetMissingProductIdsFromProductGroups(list);
		}
		return missingProductIds;
	}

	private async Task<List<long>> GetMissingProductIdsFromProductGroups(List<Product> products)
	{
		List<long> missingProductIds = new List<long>();
		if (products?.Any() ?? false)
		{
			List<long> list = new List<long>();
			foreach (Product product in products)
			{
				if (product.ProductGroup?.ProductFormats?.Any() != true)
				{
					continue;
				}
				foreach (ProductFormat productFormat in product.ProductGroup.ProductFormats)
				{
					if (product.ProductId != productFormat.ProductId)
					{
						list.Add(productFormat.ProductId);
					}
				}
			}
			missingProductIds.AddRange((await _dataTableService.GetDataObjects<Product>(list)).MissingIds);
		}
		return missingProductIds;
	}
}
