using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using KioskClientService.API.App;
using KioskClientService.API.Services.ProfileData;
using KioskClientService.API.Services.UpdateClientServiceApi;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace KioskClientService.API.Services.DataTableService;

public class DataTableService : IDataTableService
{
	private class ProcessDataUpdateData<T> : IDataTableRequest
	{
		public DataUpdateRequest DataUpdateRequest { get; set; }

		public UpdateDataTable TableName => DataUpdateRequest?.TableName ?? UpdateDataTable.None;

		public FileTypeEnum? FileType => DataUpdateRequest?.FileType;

		public string FileName { get; set; }

		public DataUpdateResponse DataUpdateResponse { get; set; }

		public DataTable<T> DataTable { get; set; } = new DataTable<T>();

		public DataUpdateStats DataUpdateStats { get; set; } = new DataUpdateStats();

		public bool IsComplete { get; set; }

		public DataUpdateResult DateUpdateResult { get; set; } = new DataUpdateResult();

		public string IdentifyingText
		{
			get
			{
				string fileName = FileName;
				try
				{
					fileName = Path.GetFileName(FileName);
				}
				catch
				{
				}
				return "for filename " + fileName;
			}
		}
	}

	private class DataUpdateStats
	{
		public int AddedCount { get; set; }

		public int ChangedCount { get; set; }

		public int DeletedCount { get; set; }

		public int DeleteNotFoundCount { get; set; }

		public int NoChangeCount { get; set; }

		public int ServerErrorCount { get; set; }

		public int DeserializationErrorCount { get; set; }

		public bool AnyTableFileUpdates()
		{
			if (AddedCount <= 0 && ChangedCount <= 0)
			{
				return DeletedCount > 0;
			}
			return true;
		}
	}

	private readonly ILogger<DataTableService> _logger;

	private readonly IUpdateClientServiceClient _updateClientServiceClient;

	private readonly IStoreService _store;

	private string _defaultDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Redbox\\Data");

	private static ConcurrentDictionary<UpdateDataTable, object> _dataTables = new ConcurrentDictionary<UpdateDataTable, object>();

	private static int _processingChanges = 0;

	public DataTableService(ILogger<DataTableService> logger, IStoreService store, IUpdateClientServiceClient updateClientServiceClient)
	{
		_logger = logger;
		_store = store;
		_updateClientServiceClient = updateClientServiceClient;
	}

	public async Task<DataTable<T>> GetDataTable<T>()
	{
		DataTable<T> dataTable = null;
		try
		{
			UpdateDataTable updateDataTable = GetUpdateDataTableAttribute<T>();
			if (IsValidUpdateDataTable(updateDataTable))
			{
				object value = null;
				if (_dataTables.TryGetValue(updateDataTable, out value))
				{
					dataTable = value as DataTable<T>;
				}
				else
				{
					DataTableRequest dataTableRequest = new DataTableRequest
					{
						FileType = FileTypeEnum.ProfileData,
						TableName = updateDataTable
					};
					DataTableResponse<T> dataTableResponse = await ReadDataTable<T>(dataTableRequest);
					if (dataTableResponse.Success)
					{
						if (_dataTables.TryAdd(updateDataTable, dataTableResponse.DataTable))
						{
							dataTable = dataTableResponse.DataTable;
						}
						else if (_dataTables.TryGetValue(updateDataTable, out value))
						{
							dataTable = value as DataTable<T>;
						}
					}
				}
			}
			else
			{
				_logger.LogErrorWithSource("Class " + typeof(T).Name + " does not have an UpdateDataTableAttribute", "GetDataTable");
			}
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "Exception while getting data table for " + typeof(T).Name, "GetDataTable");
		}
		return dataTable;
	}

	public async Task<GetDataObjectsResult<T>> GetDataObjects<T>(List<long> ids)
	{
		GetDataObjectsResult<T> result = new GetDataObjectsResult<T>();
		if (ids?.Any() ?? false)
		{
			DataTable<T> dataTable = await GetDataTable<T>();
			if (dataTable?.Data?.Any() == true)
			{
				foreach (long id in ids)
				{
					if (dataTable.Data.TryGetValue(id, out var value))
					{
						result.DataObjectList[id] = value.Data;
					}
					else
					{
						result.MissingIds.Add(id);
					}
				}
			}
			else
			{
				result.MissingIds.AddRange(ids);
			}
		}
		return result;
	}

	public async Task<List<long>> GetExistingDataRecordIds<T>()
	{
		List<long> result = new List<long>();
		DataTable<T> dataTable = await GetDataTable<T>();
		if (dataTable?.Data?.Any() == true)
		{
			foreach (KeyValuePair<long, DataRecord<T>> datum in dataTable.Data)
			{
				result.Add(datum.Key);
			}
		}
		return result;
	}

	public async Task<AddDataRecordResponse<T>> AddDataRecords<T>(List<long> ids)
	{
		AddDataRecordResponse<T> response = new AddDataRecordResponse<T>();
		try
		{
			if (!(ids?.Any() ?? false))
			{
				_logger.LogErrorWithSource("No ids specified", "AddDataRecords");
				response.StatusCode = HttpStatusCode.BadRequest;
				return response;
			}
			GetDataObjectsResult<T> getDataObjectsResult = await GetDataObjects<T>(ids);
			if (getDataObjectsResult.DataObjectList?.Any() ?? false)
			{
				response.DataObjects.AddRange(getDataObjectsResult.DataObjectList.Values);
				string arg = string.Join(",", getDataObjectsResult.DataObjectList.Keys.Take(10)) + ((getDataObjectsResult.DataObjectList.Keys.Count > 10) ? ",..." : null);
				_logger.LogInfoWithSource($"{getDataObjectsResult.DataObjectList.Count} requested {typeof(T).Name} data record(s) already exist. Ids: {arg}", "AddDataRecords");
			}
			if (getDataObjectsResult.MissingIds.Any())
			{
				List<RecordRequest> list = new List<RecordRequest>();
				foreach (long missingId in getDataObjectsResult.MissingIds)
				{
					list.Add(new RecordRequest
					{
						Id = missingId
					});
				}
				DataUpdateResult dataUpdateResult = await RequestDataTableUpdate<T>(list);
				response.TableFileUpdated = dataUpdateResult?.TableFileUpdated ?? false;
				if (dataUpdateResult.Success)
				{
					GetDataObjectsResult<T> getDataObjectsResult2 = await GetDataObjects<T>(getDataObjectsResult.MissingIds);
					response.DataObjects.AddRange(getDataObjectsResult2.DataObjectList.Values);
				}
			}
			response.StatusCode = ((ids.Count == response.DataObjects.Count) ? HttpStatusCode.OK : HttpStatusCode.InternalServerError);
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "Exception adding ids " + string.Join(", ", ids.ToArray()), "AddDataRecords");
			response.StatusCode = HttpStatusCode.InternalServerError;
		}
		return response;
	}

	private async Task ReadDataTableForUpdate<T>(ProcessDataUpdateData<T> processDataUpdateData)
	{
		if (!processDataUpdateData.IsComplete)
		{
			DataTableResponse<T> dataTableResponse = await ReadDataTable<T>(processDataUpdateData);
			if (dataTableResponse != null && !dataTableResponse.Success)
			{
				_logger.LogErrorWithSource("Unable to read DataTable for " + typeof(T).Name, "ReadDataTableForUpdate");
				processDataUpdateData.IsComplete = true;
			}
			else
			{
				processDataUpdateData.DataTable = dataTableResponse.DataTable;
			}
		}
	}

	private async Task<DataTableResponse<T>> ReadDataTable<T>(IDataTableRequest dataTableRequest)
	{
		DataTableResponse<T> dataTableResponse = new DataTableResponse<T>
		{
			DataTableRequest = dataTableRequest
		};
		await Task.Run(delegate
		{
			try
			{
				string dataTableFullPath = GetDataTableFullPath(dataTableRequest);
				if (File.Exists(dataTableFullPath))
				{
					string value = File.ReadAllText(dataTableFullPath);
					try
					{
						dataTableResponse.DataTable = JsonConvert.DeserializeObject<DataTable<T>>(value);
						if (dataTableResponse.DataTable == null)
						{
							_logger.LogErrorWithSource("Deserialized DataTable is null for file " + GetDataTableFullPath(dataTableRequest), "ReadDataTable");
						}
						dataTableResponse.Success = true;
						_logger.LogInfoWithSource($"Read {dataTableResponse.DataTable.Data.Count} records from table {dataTableRequest.TableName}", "ReadDataTable");
						return;
					}
					catch (Exception ex)
					{
						_logger.LogErrorWithSource(ex, "Exception while deserializing data table file " + GetDataTableFullPath(dataTableRequest), "ReadDataTable");
						return;
					}
				}
				dataTableResponse.DataTable = new DataTable<T>();
				dataTableResponse.Success = true;
				_logger.LogInfoWithSource("File " + dataTableFullPath + " does not exist. No records were read.", "ReadDataTable");
			}
			catch (Exception ex2)
			{
				_logger.LogErrorWithSource(ex2, "Exception while reading data table file " + GetDataTableFullPath(dataTableRequest), "ReadDataTable");
			}
		});
		return dataTableResponse;
	}

	private async Task GetHashValues<T>(DataUpdateRequest dataUpdateRequest)
	{
		IEnumerable<RecordRequest> recordRequestsWithoutHash = dataUpdateRequest.RecordRequests?.Where((RecordRequest x) => string.IsNullOrEmpty(x.Hash));
		if (!(recordRequestsWithoutHash?.Any() ?? false))
		{
			return;
		}
		int hashCount = 0;
		DataTable<T> dataTable = await GetDataTable<T>();
		if (dataTable?.Data?.Any() == true)
		{
			foreach (RecordRequest item in recordRequestsWithoutHash)
			{
				if (dataTable.Data.TryGetValue(item.Id, out var value))
				{
					item.Hash = value.Hash;
					hashCount++;
				}
			}
		}
		_logger.LogInfoWithSource($"Popluated DataUpdateRequest with {hashCount} hash values.", "GetHashValues");
	}

	public async Task<DataUpdateResult> RequestDataTableUpdate<T>(List<RecordRequest> recordRequests)
	{
		UpdateDataTable updateDataTable = GetUpdateDataTableAttribute<T>();
		if (!IsValidUpdateDataTable(updateDataTable))
		{
			_logger.LogErrorWithSource("Class " + typeof(T).Name + " does not have a UpdateDataTableAttribute", "RequestDataTableUpdate");
			return new DataUpdateResult();
		}
		DataUpdateRequest dataUpdateRequest = new DataUpdateRequest
		{
			RequestId = Guid.NewGuid(),
			KioskId = _store.KioskId,
			TableName = updateDataTable,
			RequestType = DataUpdateRequestType.List,
			FileType = FileTypeEnum.ProfileData,
			RecordRequests = recordRequests
		};
		string text = null;
		if (recordRequests?.Any() ?? false)
		{
			text = string.Join(",", recordRequests.Select((RecordRequest x) => x?.Id).Take(10)) + ((recordRequests.Count > 10) ? ",..." : null);
		}
		_logger.LogInformation($"Triggering update of {dataUpdateRequest.RecordRequests?.Count ?? 0} {updateDataTable} data records with requestId: {dataUpdateRequest.RequestId}  Ids: {text}");
		DataUpdateResult dataUpdateResult = await GetDataTableUpdates<T>(dataUpdateRequest);
		if (dataUpdateResult.TableFileUpdated)
		{
			ClearDataTable(updateDataTable);
		}
		return dataUpdateResult;
	}

	private async Task<DataUpdateResult> GetDataTableUpdates<T>(DataUpdateRequest dataUpdateRequest)
	{
		DataUpdateResult result = new DataUpdateResult();
		try
		{
			await GetHashValues<T>(dataUpdateRequest);
			APIResponse<GetRecordChangesResponse> aPIResponse = await _updateClientServiceClient.GetRecordChanges<GetRecordChangesResponse>(dataUpdateRequest);
			if (aPIResponse == null || aPIResponse.StatusCode != HttpStatusCode.OK || aPIResponse.Response == null || aPIResponse.Response.StatusCode != HttpStatusCode.OK)
			{
				return result;
			}
			return await ProcessDataUpdateFile<T>(dataUpdateRequest, aPIResponse.Response?.DataUpdateResponseFileName);
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, $"Execption while getting data updates for requestId {dataUpdateRequest?.RequestId}", "GetDataTableUpdates");
			return result;
		}
	}

	private async Task<DataUpdateResult> ProcessDataUpdateFile<T>(DataUpdateRequest dataUpdateRequest, string fileName)
	{
		ProcessDataUpdateData<T> processDataUpdateData = new ProcessDataUpdateData<T>
		{
			DataUpdateRequest = dataUpdateRequest,
			FileName = fileName
		};
		if (Interlocked.CompareExchange(ref _processingChanges, 1, 0) == 1)
		{
			_logger.LogInfoWithSource("Locking prevented processing of data update " + processDataUpdateData.IdentifyingText, "ProcessDataUpdateFile");
			return processDataUpdateData.DateUpdateResult;
		}
		try
		{
			_ = 6;
			try
			{
				_logger.LogInfoWithSource("Starting processing " + processDataUpdateData.IdentifyingText, "ProcessDataUpdateFile");
				await ReadDataUpdates(processDataUpdateData);
				ValidateDataUpdates(processDataUpdateData);
				await ReadDataTableForUpdate(processDataUpdateData);
				await ApplyDataUpdatesToTable(processDataUpdateData);
				await SaveUpdatedDataTableToStageFile(processDataUpdateData);
				await RenameActiveDataTableAsOld(processDataUpdateData);
				await RenameStagedFileToActive(processDataUpdateData);
				LogDataUpdateResults(processDataUpdateData);
				await CleanupFailedDataUpdateAttempt(processDataUpdateData);
			}
			catch (Exception ex)
			{
				_logger.LogErrorWithSource(ex, "Exception while processing data update " + processDataUpdateData.IdentifyingText, "ProcessDataUpdateFile");
			}
		}
		finally
		{
			_processingChanges = 0;
			await DeleteDataUpdateFile(processDataUpdateData);
		}
		return processDataUpdateData.DateUpdateResult;
	}

	private async Task ReadDataUpdates<T>(ProcessDataUpdateData<T> processDataUpdateData)
	{
		await Task.Run(delegate
		{
			if (!processDataUpdateData.IsComplete)
			{
				try
				{
					if (!File.Exists(processDataUpdateData.FileName))
					{
						_logger.LogErrorWithSource("Unable to find file " + processDataUpdateData.IdentifyingText, "ReadDataUpdates");
						processDataUpdateData.IsComplete = true;
					}
					else
					{
						string value = File.ReadAllText(processDataUpdateData.FileName);
						processDataUpdateData.DataUpdateResponse = JsonConvert.DeserializeObject<DataUpdateResponse>(value);
						if (processDataUpdateData.DataUpdateResponse == null)
						{
							processDataUpdateData.IsComplete = true;
							_logger.LogErrorWithSource("DataUpdateResponse is null " + processDataUpdateData.IdentifyingText, "ReadDataUpdates");
						}
						else
						{
							_logger.LogInfoWithSource($"read {(processDataUpdateData.DataUpdateResponse?.RecordResponses?.Count).GetValueOrDefault()} update records {processDataUpdateData.IdentifyingText}", "ReadDataUpdates");
						}
					}
				}
				catch (Exception ex)
				{
					processDataUpdateData.IsComplete = true;
					_logger.LogErrorWithSource(ex, "Exception while reading data updates " + processDataUpdateData.IdentifyingText, "ReadDataUpdates");
				}
			}
		});
	}

	private void ValidateDataUpdates<T>(ProcessDataUpdateData<T> processDataUpdateData)
	{
		if (!processDataUpdateData.IsComplete)
		{
			if (processDataUpdateData.DataUpdateResponse == null || !IsValidUpdateDataTable(processDataUpdateData.DataUpdateResponse.TableName))
			{
				processDataUpdateData.IsComplete = true;
				_logger.LogErrorWithSource($"Invalid table name {processDataUpdateData.DataUpdateResponse?.TableName} {processDataUpdateData.IdentifyingText}", "ValidateDataUpdates");
			}
			if (processDataUpdateData.DataUpdateResponse?.RequestId != processDataUpdateData.DataUpdateRequest.RequestId)
			{
				processDataUpdateData.IsComplete = true;
				_logger.LogErrorWithSource($"Id in DataUpateResponse {processDataUpdateData.DataUpdateResponse?.RequestId} doesn't match RequestId {processDataUpdateData.DataUpdateRequest.RequestId}", "ValidateDataUpdates");
			}
			if (processDataUpdateData.DataUpdateResponse?.TableName != processDataUpdateData.DataUpdateRequest.TableName)
			{
				processDataUpdateData.IsComplete = true;
				_logger.LogErrorWithSource($"TableName in DataUpateResponse {processDataUpdateData.DataUpdateResponse?.RequestId} doesn't match Request TableName {processDataUpdateData.DataUpdateRequest.RequestId} {processDataUpdateData.IdentifyingText}", "ValidateDataUpdates");
			}
		}
	}

	private async Task ApplyDataUpdatesToTable<T>(ProcessDataUpdateData<T> processDataUpdateData)
	{
		await Task.Run(delegate
		{
			if (!processDataUpdateData.IsComplete)
			{
				try
				{
					if (processDataUpdateData.DataUpdateResponse?.RecordResponses?.Any() == true)
					{
						foreach (RecordResponse recordResponse in processDataUpdateData.DataUpdateResponse.RecordResponses)
						{
							switch (recordResponse.RecordStatus)
							{
							case DataUpdateRecordStatus.NoChange:
								processDataUpdateData.DataUpdateStats.NoChangeCount++;
								break;
							case DataUpdateRecordStatus.Changed:
								try
								{
									T data = JsonConvert.DeserializeObject<T>(recordResponse.Data);
									DataRecord<T> value = new DataRecord<T>
									{
										Id = recordResponse.Id,
										Data = data,
										Hash = recordResponse.Hash
									};
									if (processDataUpdateData.DataTable.Data.ContainsKey(recordResponse.Id))
									{
										processDataUpdateData.DataUpdateStats.ChangedCount++;
									}
									else
									{
										processDataUpdateData.DataUpdateStats.AddedCount++;
									}
									processDataUpdateData.DataTable.Data[recordResponse.Id] = value;
								}
								catch (Exception ex)
								{
									processDataUpdateData.DataUpdateStats.DeserializationErrorCount++;
									_logger.LogErrorWithSource(ex, $"Exception while updating {processDataUpdateData.DataUpdateRequest.TableName} record with id {recordResponse.Id}", "ApplyDataUpdatesToTable");
								}
								break;
							case DataUpdateRecordStatus.Deleted:
								if (processDataUpdateData.DataTable.Data.ContainsKey(recordResponse.Id))
								{
									processDataUpdateData.DataUpdateStats.DeletedCount++;
									processDataUpdateData.DataTable.Data.Remove(recordResponse.Id);
								}
								else
								{
									processDataUpdateData.DataUpdateStats.DeleteNotFoundCount++;
								}
								break;
							case DataUpdateRecordStatus.Error:
								_logger.LogInfoWithSource($"Record Status Error for {processDataUpdateData.DataUpdateRequest.TableName} record with id {recordResponse.Id}", "ApplyDataUpdatesToTable");
								processDataUpdateData.DataUpdateStats.ServerErrorCount++;
								break;
							}
						}
						_logger.LogInfoWithSource("DataUpates applied: " + JsonConvert.SerializeObject(processDataUpdateData.DataUpdateStats, new JsonSerializerSettings
						{
							DefaultValueHandling = DefaultValueHandling.Ignore
						}), "ApplyDataUpdatesToTable");
						if (!processDataUpdateData.DataUpdateStats.AnyTableFileUpdates())
						{
							processDataUpdateData.DateUpdateResult.Success = true;
							processDataUpdateData.IsComplete = true;
							_logger.LogInfoWithSource($"No {processDataUpdateData.TableName} table updates to apply {processDataUpdateData.IdentifyingText}", "ApplyDataUpdatesToTable");
						}
					}
				}
				catch (Exception ex2)
				{
					processDataUpdateData.IsComplete = true;
					_logger.LogErrorWithSource(ex2, "Exception while applying data updates " + processDataUpdateData.IdentifyingText, "ApplyDataUpdatesToTable");
				}
			}
		});
	}

	private async Task SaveUpdatedDataTableToStageFile<T>(ProcessDataUpdateData<T> processDataUpdateData)
	{
		if (processDataUpdateData.IsComplete)
		{
			return;
		}
		try
		{
			string stagedFileName = GetDataTableStagedFullPath(processDataUpdateData);
			string directoryName = Path.GetDirectoryName(stagedFileName);
			if (!Directory.Exists(directoryName))
			{
				Directory.CreateDirectory(directoryName);
			}
			if (!(await DeleteFile(stagedFileName)))
			{
				processDataUpdateData.IsComplete = true;
				_logger.LogErrorWithSource("Unable to delete " + stagedFileName, "SaveUpdatedDataTableToStageFile");
			}
			if (!processDataUpdateData.IsComplete)
			{
				SortedDictionary<long, DataRecord<T>> dictionary = new SortedDictionary<long, DataRecord<T>>(processDataUpdateData.DataTable.Data);
				processDataUpdateData.DataTable.Data = new Dictionary<long, DataRecord<T>>(dictionary);
				string contents = JsonConvert.SerializeObject(processDataUpdateData.DataTable, Formatting.Indented);
				File.WriteAllText(stagedFileName, contents);
				_logger.LogInfoWithSource("Saved staged file to " + stagedFileName, "SaveUpdatedDataTableToStageFile");
			}
		}
		catch (Exception ex)
		{
			processDataUpdateData.IsComplete = true;
			_logger.LogErrorWithSource(ex, "Exception while saving updates to stage file " + processDataUpdateData.IdentifyingText, "SaveUpdatedDataTableToStageFile");
		}
	}

	private async Task RenameActiveDataTableAsOld<T>(ProcessDataUpdateData<T> processDataUpdateData)
	{
		if (processDataUpdateData.IsComplete)
		{
			return;
		}
		try
		{
			string oldDataTableFileName = GetOldDataTableFullPath(processDataUpdateData);
			if (!(await DeleteFile(oldDataTableFileName)))
			{
				processDataUpdateData.IsComplete = true;
				_logger.LogErrorWithSource("Unable to delete " + oldDataTableFileName, "RenameActiveDataTableAsOld");
				return;
			}
			string dataTableFileName = GetDataTableFullPath(processDataUpdateData);
			if (File.Exists(dataTableFileName))
			{
				if (!(await RenameFile(dataTableFileName, oldDataTableFileName)))
				{
					processDataUpdateData.IsComplete = true;
					_logger.LogErrorWithSource("Unable to rename " + dataTableFileName + " to " + oldDataTableFileName, "RenameActiveDataTableAsOld");
					return;
				}
				_logger.LogInfoWithSource("Renamed active DataTable file " + Path.GetFileName(dataTableFileName) + " to " + Path.GetFileName(oldDataTableFileName) + " ", "RenameActiveDataTableAsOld");
			}
		}
		catch (Exception ex)
		{
			processDataUpdateData.IsComplete = true;
			_logger.LogErrorWithSource(ex, "Exception while renaming active data table file " + processDataUpdateData.IdentifyingText, "RenameActiveDataTableAsOld");
		}
	}

	private async Task RenameStagedFileToActive<T>(ProcessDataUpdateData<T> processDataUpdateData)
	{
		if (processDataUpdateData.IsComplete)
		{
			return;
		}
		try
		{
			string stagedDataTableFileName = GetDataTableStagedFullPath(processDataUpdateData);
			string dataTableFileName = GetDataTableFullPath(processDataUpdateData);
			if (await RenameFile(stagedDataTableFileName, dataTableFileName))
			{
				processDataUpdateData.DateUpdateResult.Success = true;
				processDataUpdateData.DateUpdateResult.TableFileUpdated = true;
				_logger.LogInfoWithSource("Renamed staged DataTable file " + Path.GetFileName(stagedDataTableFileName) + " to " + Path.GetFileName(dataTableFileName) + " " + processDataUpdateData.IdentifyingText, "RenameStagedFileToActive");
			}
		}
		catch (Exception ex)
		{
			processDataUpdateData.IsComplete = true;
			_logger.LogErrorWithSource(ex, "Exception while renaming staged file " + processDataUpdateData.IdentifyingText, "RenameStagedFileToActive");
		}
	}

	private void LogDataUpdateResults<T>(ProcessDataUpdateData<T> processDataUpdateData)
	{
		processDataUpdateData.DateUpdateResult.Success = processDataUpdateData.DateUpdateResult.Success && processDataUpdateData.DataUpdateStats.ServerErrorCount == 0 && processDataUpdateData.DataUpdateStats.DeserializationErrorCount == 0;
		string text = (processDataUpdateData.DateUpdateResult.Success ? "succeeded" : "failed");
		_logger.LogInfoWithSource("Data update " + text + " " + processDataUpdateData.IdentifyingText, "LogDataUpdateResults");
	}

	private async Task CleanupFailedDataUpdateAttempt<T>(ProcessDataUpdateData<T> processDataUpdateData)
	{
		if (!processDataUpdateData.DateUpdateResult.Success)
		{
			string dataTableFullPath = GetDataTableFullPath(processDataUpdateData);
			string oldDataTableFullPath = GetOldDataTableFullPath(processDataUpdateData);
			if (!File.Exists(dataTableFullPath) && File.Exists(oldDataTableFullPath))
			{
				await RenameFile(oldDataTableFullPath, dataTableFullPath);
			}
			string dataTableStagedFullPath = GetDataTableStagedFullPath(processDataUpdateData);
			await DeleteFile(dataTableStagedFullPath);
		}
	}

	private async Task DeleteDataUpdateFile<T>(ProcessDataUpdateData<T> processDataUpdateData)
	{
		await Task.Run(async delegate
		{
			try
			{
				await DeleteFile(processDataUpdateData.FileName);
			}
			catch (Exception ex)
			{
				_logger.LogErrorWithSource(ex, "Exception while deleting data update file " + processDataUpdateData.IdentifyingText, "DeleteDataUpdateFile");
			}
		});
	}

	private void ClearDataTable(UpdateDataTable updateDataTable)
	{
		_dataTables.TryRemove(updateDataTable, out var _);
	}

	public UpdateDataTable GetUpdateDataTableAttribute<T>()
	{
		UpdateDataTable result = UpdateDataTable.None;
		if (typeof(T).GetCustomAttributes(typeof(UpdateDataTableAttribute), inherit: true).FirstOrDefault() is UpdateDataTableAttribute updateDataTableAttribute)
		{
			result = updateDataTableAttribute.UpdateDataTable;
		}
		return result;
	}

	public bool IsValidUpdateDataTable(UpdateDataTable updateDataTable)
	{
		return updateDataTable != UpdateDataTable.None;
	}

	private async Task<bool> RenameFile(string originalFileName, string newFileName)
	{
		bool result = false;
		await Task.Run(delegate
		{
			try
			{
				File.Move(originalFileName, newFileName);
				if (File.Exists(newFileName))
				{
					result = true;
				}
			}
			catch (Exception ex)
			{
				_logger.LogErrorWithSource(ex, "Unable to rename file " + originalFileName + " to " + newFileName, "RenameFile");
			}
		});
		return result;
	}

	private async Task<bool> DeleteFile(string fileName)
	{
		bool result = false;
		await Task.Run(delegate
		{
			try
			{
				if (File.Exists(fileName))
				{
					File.Delete(fileName);
					result = !File.Exists(fileName);
				}
				else
				{
					result = true;
				}
			}
			catch (Exception ex)
			{
				_logger.LogErrorWithSource(ex, "Unable to delete file " + fileName, "DeleteFile");
			}
		});
		return result;
	}

	private string GetDataTableFullPath(IDataTableRequest dataTableRequest)
	{
		return Path.Combine(GetDataTableDirectory(dataTableRequest), GetDataTableFileName(dataTableRequest));
	}

	private string GetDataTableDirectory(IDataTableRequest dataTableRequest)
	{
		string path = dataTableRequest.TableName.ToString();
		string value = null;
		if (!dataTableRequest.FileType.HasValue || !FilePaths.TypeMappings.TryGetValue(dataTableRequest.FileType.Value, out value))
		{
			value = _defaultDataFolder;
		}
		return Path.Combine(value, path);
	}

	private string GetDataTableFileName(IDataTableRequest dataTableRequest)
	{
		string text = dataTableRequest.TableName.ToString();
		return text + "Data.json";
	}

	private string GetDataTableStagedFullPath(IDataTableRequest dataTableRequest)
	{
		return Path.ChangeExtension(GetDataTableFullPath(dataTableRequest), ".staged");
	}

	private string GetOldDataTableFileName(IDataTableRequest dataTableRequest)
	{
		string text = dataTableRequest.TableName.ToString();
		return text + "Data old.json";
	}

	private string GetOldDataTableFullPath(IDataTableRequest dataTableRequest)
	{
		return Path.Combine(GetDataTableDirectory(dataTableRequest), GetOldDataTableFileName(dataTableRequest));
	}
}
