using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Redbox.NetCore.Logging.Extensions;
using Redbox.NetCore.Middleware.Http;
using UpdateClientService.API.Services.IoT;
using UpdateClientService.API.Services.IoT.Commands;
using UpdateClientService.API.Services.IoT.Commands.KioskFiles;
using UpdateClientService.API.Services.IoT.IoTCommand;

namespace UpdateClientService.API.Services.Configuration;

public class ConfigurationService : IConfigurationService
{
	private class ConfigurationSettingChangesData
	{
		public KioskSettingChangesResponse KioskConfigurationChangesResponse { get; set; }

		public GetKioskConfigurationSettingValues KioskConfigurationSettingValues { get; set; }

		public KioskConfigurationSettingChangesRequest KioskConfigurationSettingChangesRequest { get; set; }

		public string TempConfigFileName { get; set; }

		public bool Success { get; set; } = true;

		public bool AllPagesLoadedOrAborted { get; set; }

		public bool IsVersionCurrent => KioskConfigurationChangesResponse?.VersionIsCurrent ?? false;

		public long CurrentVersionId => KioskConfigurationSettingChangesRequest?.CurrentConfigurationVersionId ?? 0;

		public long NewVersionId => (KioskConfigurationChangesResponse?.NewConfigurationSettingValues?.ConfigurationVersion).GetValueOrDefault();
	}

	private readonly ILogger<IConfigurationService> _logger;

	private readonly IStoreService _storeService;

	private IIoTCommandClient _iotCommandClient;

	private readonly IOptionsMonitor<AppSettings> _settings;

	private readonly IPersistentDataCacheService _persistentDataCacheService;

	private static DateTime _lastCallToGetKioskConfigurationSettingChanges = DateTime.MinValue;

	private static SemaphoreSlim _lock = new SemaphoreSlim(1, 1);

	private readonly int _lockWait = 2000;

	private const string ConfigurationFileName = "configuration.json";

	private const string OldConfigurationFileName = "configuration old.json";

	private const string ConfigurationStatusFileName = "ConfigurationStatus.json";

	private int _processingChanges;

	private static string ConfigurationFileDirectory => FilePaths.TypeMappings[FileTypeEnum.Configuration];

	public static string ConfigurationFilePath => Path.Combine(ConfigurationFileDirectory, "configuration.json");

	private string OldConfigurationFilePath => Path.Combine(ConfigurationFileDirectory, "configuration old.json");

	public ConfigurationService(ILogger<IConfigurationService> logger, IStoreService storeService, IIoTCommandClient ioTCommandClient, IOptionsMonitor<AppSettings> settings, IPersistentDataCacheService persistentDataCacheService)
	{
		_logger = logger;
		_storeService = storeService;
		_iotCommandClient = ioTCommandClient;
		_settings = settings;
		_persistentDataCacheService = persistentDataCacheService;
	}

	public async Task<bool> UpdateConfigurationStatusIfNeeded()
	{
		bool response = false;
		try
		{
			PersistentDataWrapper<ConfigurationStatus> persistentDataWrapper = await GetConfigurationStatusFromPersistentDataCacheService();
			if (persistentDataWrapper == null || IsMoreThan26HoursAgo(persistentDataWrapper.Modified))
			{
				ConfigurationStatusResponse configurationStatusResponse = await UpdateConfigurationStatus();
				if (configurationStatusResponse != null && configurationStatusResponse.StatusCode == HttpStatusCode.OK)
				{
					response = true;
				}
			}
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "Exception while getting config status", "UpdateConfigurationStatusIfNeeded", "/sln/src/UpdateClientService.API/Services/Configuration/ConfigurationService.cs");
		}
		return response;
	}

	private bool IsMoreThan26HoursAgo(DateTime dateTime)
	{
		return (DateTime.Now - dateTime).TotalHours > 26.0;
	}

	public async Task<ConfigurationStatusResponse> UpdateConfigurationStatus()
	{
		ConfigurationStatusResponse response = new ConfigurationStatusResponse();
		try
		{
			IoTCommandResponse<ObjectResult> ioTCommandResponse = await _iotCommandClient.PerformIoTCommand<ObjectResult>(new IoTCommandModel
			{
				Version = 1,
				RequestId = Guid.NewGuid().ToString(),
				Command = CommandEnum.GetConfigStatus,
				QualityOfServiceLevel = QualityOfServiceLevel.AtLeastOnce,
				LogResponse = false
			}, new PerformIoTCommandParameters
			{
				IoTTopic = "$aws/rules/kioskrestcall",
				WaitForResponse = true
			});
			if (ioTCommandResponse != null && ioTCommandResponse.StatusCode == 200)
			{
				ConfigurationStatusResponse configurationStatusResponse = JsonConvert.DeserializeObject<ConfigurationStatusResponse>(ioTCommandResponse?.Payload?.Value?.ToString());
				if (configurationStatusResponse?.ConfigurationStatus != null && configurationStatusResponse.StatusCode == HttpStatusCode.OK)
				{
					if (await SetConfigurationStatus(configurationStatusResponse.ConfigurationStatus))
					{
						response.StatusCode = HttpStatusCode.OK;
						response.ConfigurationStatus = configurationStatusResponse.ConfigurationStatus;
					}
				}
				else
				{
					response.StatusCode = HttpStatusCode.InternalServerError;
					_logger.LogErrorWithSource("Error updating configuration status.", "UpdateConfigurationStatus", "/sln/src/UpdateClientService.API/Services/Configuration/ConfigurationService.cs");
				}
			}
			else
			{
				response.StatusCode = HttpStatusCode.InternalServerError;
				_logger.LogErrorWithSource("Error updating configuration status.", "UpdateConfigurationStatus", "/sln/src/UpdateClientService.API/Services/Configuration/ConfigurationService.cs");
			}
		}
		catch (Exception ex)
		{
			response.StatusCode = HttpStatusCode.InternalServerError;
			_logger.LogErrorWithSource(ex, "Exception updating configuration status.", "UpdateConfigurationStatus", "/sln/src/UpdateClientService.API/Services/Configuration/ConfigurationService.cs");
		}
		return response;
	}

	public async Task<ConfigurationStatusResponse> GetConfigurationStatus()
	{
		ConfigurationStatusResponse response = new ConfigurationStatusResponse
		{
			ConfigurationStatus = new ConfigurationStatus()
		};
		try
		{
			PersistentDataWrapper<ConfigurationStatus> persistentDataWrapper = await GetConfigurationStatusFromPersistentDataCacheService();
			if (persistentDataWrapper?.Data != null)
			{
				response.ConfigurationStatus = persistentDataWrapper.Data;
			}
		}
		catch (Exception ex)
		{
			response.StatusCode = HttpStatusCode.InternalServerError;
			_logger.LogErrorWithSource(ex, "Exception retrieving configuration status.", "GetConfigurationStatus", "/sln/src/UpdateClientService.API/Services/Configuration/ConfigurationService.cs");
		}
		return response;
	}

	private async Task<PersistentDataWrapper<ConfigurationStatus>> GetConfigurationStatusFromPersistentDataCacheService()
	{
		PersistentDataWrapper<ConfigurationStatus> result = new PersistentDataWrapper<ConfigurationStatus>();
		try
		{
			PersistentDataWrapper<ConfigurationStatus> persistentDataWrapper = await _persistentDataCacheService.Read<ConfigurationStatus>("ConfigurationStatus.json", useCache: true, null, log: false);
			if (persistentDataWrapper != null)
			{
				result = persistentDataWrapper;
			}
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "Exception getting configuration status", "GetConfigurationStatusFromPersistentDataCacheService", "/sln/src/UpdateClientService.API/Services/Configuration/ConfigurationService.cs");
		}
		return result;
	}

	private async Task<bool> SetConfigurationStatus(ConfigurationStatus configurationStatus)
	{
		bool result = false;
		try
		{
			result = await _persistentDataCacheService.Write(configurationStatus, "ConfigurationStatus.json");
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "Exception setting configuration status", "SetConfigurationStatus", "/sln/src/UpdateClientService.API/Services/Configuration/ConfigurationService.cs");
		}
		return result;
	}

	public async Task<ApiBaseResponse> TriggerUpdateConfigurationStatus(TriggerUpdateConfigStatusRequest triggerUpdateConfigStatusRequest)
	{
		ApiBaseResponse apiResponse = new ApiBaseResponse();
		try
		{
			if (triggerUpdateConfigStatusRequest != null)
			{
				if (!triggerUpdateConfigStatusRequest.ExecutionTimeFrameMs.HasValue || triggerUpdateConfigStatusRequest.ExecutionTimeFrameMs == 0)
				{
					apiResponse = await UpdateConfigurationStatus();
				}
				else
				{
					Task.Run(async delegate
					{
						int num = new Random().Next((int)triggerUpdateConfigStatusRequest.ExecutionTimeFrameMs.Value);
						_logger.LogInfoWithSource($"waiting {num} ms before calling UpdateConfigurationStatus", "TriggerUpdateConfigurationStatus", "/sln/src/UpdateClientService.API/Services/Configuration/ConfigurationService.cs");
						await Task.Delay(num);
						await UpdateConfigurationStatus();
					});
				}
			}
			else
			{
				_logger.LogErrorWithSource("parameter triggerUpdateConfigStatusRequest must not be null.", "TriggerUpdateConfigurationStatus", "/sln/src/UpdateClientService.API/Services/Configuration/ConfigurationService.cs");
				apiResponse.StatusCode = HttpStatusCode.InternalServerError;
			}
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "Exception triggering UpdateConfigurationStatus.", "TriggerUpdateConfigurationStatus", "/sln/src/UpdateClientService.API/Services/Configuration/ConfigurationService.cs");
			apiResponse.StatusCode = HttpStatusCode.InternalServerError;
		}
		return apiResponse;
	}

	public async Task<ApiBaseResponse> TriggerGetKioskConfigurationSettingChanges(TriggerGetConfigChangesRequest triggerGetConfigChangesRequest)
	{
		ApiBaseResponse apiResponse = new ApiBaseResponse();
		try
		{
			if (triggerGetConfigChangesRequest != null)
			{
				TriggerGetConfigChangesRequest triggerGetConfigChangesRequest2 = triggerGetConfigChangesRequest;
				if (triggerGetConfigChangesRequest2 != null && triggerGetConfigChangesRequest2.RequestedConfigurationVersionId == 0)
				{
					triggerGetConfigChangesRequest.RequestedConfigurationVersionId = null;
				}
				if (triggerGetConfigChangesRequest == null || triggerGetConfigChangesRequest.ExecutionTimeFrameMs == 0)
				{
					apiResponse = await GetKioskConfigurationSettingChanges(triggerGetConfigChangesRequest?.RequestedConfigurationVersionId);
				}
				else
				{
					Task.Run(async delegate
					{
						int randomMs = new Random().Next((int)triggerGetConfigChangesRequest.ExecutionTimeFrameMs.Value);
						_logger.LogInfoWithSource($"waiting {randomMs} ms before calling GetKioskConfigurationSettingChanges", "TriggerGetKioskConfigurationSettingChanges", "/sln/src/UpdateClientService.API/Services/Configuration/ConfigurationService.cs");
						DateTime lastCall = await GetLastCallToGetKioskConfigurationSettingChanges();
						await Task.Delay(randomMs);
						DateTime dateTime = lastCall;
						if (!(dateTime != await GetLastCallToGetKioskConfigurationSettingChanges()))
						{
							await GetKioskConfigurationSettingChanges(triggerGetConfigChangesRequest?.RequestedConfigurationVersionId);
						}
					});
				}
			}
			else
			{
				_logger.LogErrorWithSource("parameter triggerGetConfigChangesRequest must not be null.", "TriggerGetKioskConfigurationSettingChanges", "/sln/src/UpdateClientService.API/Services/Configuration/ConfigurationService.cs");
				apiResponse.StatusCode = HttpStatusCode.InternalServerError;
			}
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "Exception triggering GetKioskConfigurationChanges.", "TriggerGetKioskConfigurationSettingChanges", "/sln/src/UpdateClientService.API/Services/Configuration/ConfigurationService.cs");
			apiResponse.StatusCode = HttpStatusCode.InternalServerError;
		}
		return apiResponse;
	}

	public async Task<ApiBaseResponse> GetKioskConfigurationSettingChanges(long? requestedConfigurationVersionId)
	{
		await SetLastCallToGetKioskConfigurationSettingChanges();
		ApiBaseResponse response = new ApiBaseResponse();
		try
		{
			ConfigurationStatusResponse configurationStatusResponse = await GetConfigurationStatus();
			if (configurationStatusResponse == null || !configurationStatusResponse.ConfigurationStatus.EnableUpdates)
			{
				_logger.LogInfoWithSource("aborting GetKioskConfigurationSettingChanges because ConfigurationStatus.EnableUpdates = false", "GetKioskConfigurationSettingChanges", "/sln/src/UpdateClientService.API/Services/Configuration/ConfigurationService.cs");
				response.StatusCode = HttpStatusCode.ServiceUnavailable;
				return response;
			}
			ConfigurationSettingChangesData configurationSettingChangesData = await CreateKioskConfigurationSettingChangesRequest(requestedConfigurationVersionId);
			while (!configurationSettingChangesData.AllPagesLoadedOrAborted)
			{
				IoTCommandResponse<ObjectResult> ioTCommandResponse = await _iotCommandClient.PerformIoTCommand<ObjectResult>(new IoTCommandModel
				{
					Version = 1,
					RequestId = Guid.NewGuid().ToString(),
					Command = CommandEnum.GetConfigChanges,
					Payload = configurationSettingChangesData.KioskConfigurationSettingChangesRequest,
					QualityOfServiceLevel = QualityOfServiceLevel.AtLeastOnce,
					LogResponse = false
				}, new PerformIoTCommandParameters
				{
					IoTTopic = "$aws/rules/kioskrestcall",
					WaitForResponse = true
				});
				if (ioTCommandResponse != null && ioTCommandResponse.StatusCode == 200)
				{
					KioskSettingChangesResponse kioskSettingChangesResponse = JsonConvert.DeserializeObject<KioskSettingChangesResponse>(ioTCommandResponse?.Payload?.Value?.ToString());
					if (kioskSettingChangesResponse != null)
					{
						if (!kioskSettingChangesResponse.IsFirstPage)
						{
							await AddNewSettings(configurationSettingChangesData.KioskConfigurationChangesResponse?.NewConfigurationSettingValues, kioskSettingChangesResponse.NewConfigurationSettingValues);
						}
						else
						{
							configurationSettingChangesData.KioskConfigurationChangesResponse = kioskSettingChangesResponse;
							if (!kioskSettingChangesResponse.IsLastPage)
							{
								_logger.LogInfoWithSource($"Response has {kioskSettingChangesResponse?.PageCount} pages", "GetKioskConfigurationSettingChanges", "/sln/src/UpdateClientService.API/Services/Configuration/ConfigurationService.cs");
							}
						}
						if (kioskSettingChangesResponse.IsLastPage)
						{
							if (await ProcessConfigurationSettingChanges(configurationSettingChangesData))
							{
								response.StatusCode = HttpStatusCode.OK;
							}
							else
							{
								response.StatusCode = HttpStatusCode.InternalServerError;
							}
							configurationSettingChangesData.AllPagesLoadedOrAborted = true;
						}
						else
						{
							configurationSettingChangesData.KioskConfigurationSettingChangesRequest.PageNumber = kioskSettingChangesResponse.PageNumber + 1;
						}
					}
					else
					{
						response.StatusCode = HttpStatusCode.InternalServerError;
						_logger.LogErrorWithSource("Unable to deserialize Iot command payload as GetKioskConfigurationSettingChangesResponse", "GetKioskConfigurationSettingChanges", "/sln/src/UpdateClientService.API/Services/Configuration/ConfigurationService.cs");
						configurationSettingChangesData.AllPagesLoadedOrAborted = true;
					}
				}
				else
				{
					response.StatusCode = HttpStatusCode.InternalServerError;
					_logger.LogErrorWithSource($"Iot command {CommandEnum.GetConfigChanges} returned statuscode {ioTCommandResponse?.StatusCode}", "GetKioskConfigurationSettingChanges", "/sln/src/UpdateClientService.API/Services/Configuration/ConfigurationService.cs");
					configurationSettingChangesData.AllPagesLoadedOrAborted = true;
				}
			}
		}
		catch (Exception ex)
		{
			response.StatusCode = HttpStatusCode.InternalServerError;
			_logger.LogErrorWithSource(ex, "Error in GetKioskConfigurationSettingChanges", "GetKioskConfigurationSettingChanges", "/sln/src/UpdateClientService.API/Services/Configuration/ConfigurationService.cs");
		}
		return response;
	}

	private async Task<DateTime> GetLastCallToGetKioskConfigurationSettingChanges()
	{
		DateTime result = DateTime.MinValue;
		if (await _lock.WaitAsync(_lockWait))
		{
			try
			{
				result = _lastCallToGetKioskConfigurationSettingChanges;
			}
			finally
			{
				_lock.Release();
			}
		}
		else
		{
			_logger.LogErrorWithSource("Lock failed. Unable to get last call to GetKioskConfigurationSettingChanges", "GetLastCallToGetKioskConfigurationSettingChanges", "/sln/src/UpdateClientService.API/Services/Configuration/ConfigurationService.cs");
		}
		return result;
	}

	private async Task SetLastCallToGetKioskConfigurationSettingChanges()
	{
		if (await _lock.WaitAsync(_lockWait))
		{
			try
			{
				_lastCallToGetKioskConfigurationSettingChanges = DateTime.Now;
				return;
			}
			finally
			{
				_lock.Release();
			}
		}
		_logger.LogErrorWithSource("Lock failed. Unable to set last call to GetKioskConfigurationSettingChanges", "SetLastCallToGetKioskConfigurationSettingChanges", "/sln/src/UpdateClientService.API/Services/Configuration/ConfigurationService.cs");
	}

	private async Task ActivateConfigurationVersion(KioskConfigurationVersionActivationRequest kioskConfigurationVersionActivationRequest)
	{
		string arg = (((await _iotCommandClient.PerformIoTCommand<ObjectResult>(new IoTCommandModel
		{
			Version = 1,
			RequestId = Guid.NewGuid().ToString(),
			Command = CommandEnum.ActivateConfigVersion,
			Payload = kioskConfigurationVersionActivationRequest,
			QualityOfServiceLevel = QualityOfServiceLevel.AtLeastOnce,
			LogResponse = false
		}, new PerformIoTCommandParameters
		{
			IoTTopic = "$aws/rules/kioskrestcall",
			WaitForResponse = true
		})).StatusCode == 200) ? "successful" : "failed");
		_logger.LogInfoWithSource($"Activation of configuration version {kioskConfigurationVersionActivationRequest.ConfigurationVersionId} {arg}", "ActivateConfigurationVersion", "/sln/src/UpdateClientService.API/Services/Configuration/ConfigurationService.cs");
	}

	private async Task<ConfigurationSettingChangesData> CreateKioskConfigurationSettingChangesRequest(long? requestedConfigurationVersionId)
	{
		KioskConfigurationSettingChangesRequest request = new KioskConfigurationSettingChangesRequest
		{
			KioskId = _storeService.KioskId,
			RequestedConfigurationVersionId = requestedConfigurationVersionId,
			SettingChangesRequestId = Guid.NewGuid().ToString()
		};
		(bool, GetKioskConfigurationSettingValues) tuple = await ReadConfigurationFile(ConfigurationFilePath);
		_ = tuple.Item1;
		GetKioskConfigurationSettingValues getKioskConfigurationSettingValues = tuple.Item2;
		if (getKioskConfigurationSettingValues == null)
		{
			getKioskConfigurationSettingValues = new GetKioskConfigurationSettingValues();
		}
		request.CurrentConfigurationVersionId = getKioskConfigurationSettingValues.ConfigurationVersion;
		request.ConfigurationVersionHash = ComputeConfigurationSettingValuesHashValue(getKioskConfigurationSettingValues);
		return new ConfigurationSettingChangesData
		{
			KioskConfigurationSettingChangesRequest = request,
			KioskConfigurationSettingValues = getKioskConfigurationSettingValues
		};
	}

	private string ComputeConfigurationSettingValuesHashValue(GetKioskConfigurationSettingValues getKioskConfigurationSettingValues)
	{
		string result = null;
		if (getKioskConfigurationSettingValues != null)
		{
			StringBuilder stringBuilder = new StringBuilder($"{getKioskConfigurationSettingValues.KioskId},{getKioskConfigurationSettingValues.ConfigurationVersion}");
			getKioskConfigurationSettingValues.Categories.Sort(sortCategories);
			foreach (KioskConfigurationCategory category in getKioskConfigurationSettingValues.Categories)
			{
				stringBuilder.Append("," + category.Name);
				category.Settings.Sort(sortSettings);
				foreach (KioskSetting setting in category.Settings)
				{
					stringBuilder.Append($",{setting.SettingId},{setting.Name},{setting.DataType}");
					setting.SettingValues.Sort(sortSettingValues);
					foreach (KioskSettingValue settingValue in setting.SettingValues)
					{
						string text = (settingValue.EncryptionType.HasValue ? $"{settingValue.EncryptionType}," : null);
						stringBuilder.Append(string.Format(",{0},{1}{2},{3},{4},{5}", settingValue.ConfigurationSettingValueId, text, settingValue.Value, settingValue.Rank, settingValue.EffectiveDateTime.ToString("u"), settingValue.ExpireDateTime?.ToString("u")));
					}
				}
			}
			SHA512 sHA = new SHA512Managed();
			byte[] inArray = sHA.ComputeHash(Encoding.UTF8.GetBytes(stringBuilder.ToString()));
			result = Convert.ToBase64String(inArray);
		}
		return result;
		static int sortCategories(KioskConfigurationCategory a, KioskConfigurationCategory b)
		{
			return a.Name.CompareTo(b.Name);
		}
		static int sortSettingValues(KioskSettingValue a, KioskSettingValue b)
		{
			return a.ConfigurationSettingValueId.CompareTo(b.ConfigurationSettingValueId);
		}
		static int sortSettings(KioskSetting a, KioskSetting b)
		{
			return a.Name.CompareTo(b.Name);
		}
	}

	private async Task<bool> ProcessConfigurationSettingChanges(ConfigurationSettingChangesData configurationSettingChangesData)
	{
		if (Interlocked.CompareExchange(ref _processingChanges, 1, 0) == 1)
		{
			_logger.LogInfoWithSource($"Prevented attempt to process configuration setting changes for version {configurationSettingChangesData?.NewVersionId}.", "ProcessConfigurationSettingChanges", "/sln/src/UpdateClientService.API/Services/Configuration/ConfigurationService.cs");
			return false;
		}
		try
		{
			if (!configurationSettingChangesData.IsVersionCurrent)
			{
				await ValidateResponse(configurationSettingChangesData);
				await InitializePath(configurationSettingChangesData);
				await ClearOldSettings(configurationSettingChangesData);
				await AddNewSettings(configurationSettingChangesData);
				await SaveTempConfigurationFile(configurationSettingChangesData);
				await RenameCurrentConfigFile(configurationSettingChangesData);
				await RenameTempFileAsCurrent(configurationSettingChangesData);
				await ActivateConfigVersion(configurationSettingChangesData);
				await CleanupFailedAttempt(configurationSettingChangesData);
			}
		}
		finally
		{
			_processingChanges = 0;
		}
		if (configurationSettingChangesData.IsVersionCurrent)
		{
			_logger.LogInfoWithSource($"No configuration changes to process.  Version {configurationSettingChangesData.CurrentVersionId} is current.", "ProcessConfigurationSettingChanges", "/sln/src/UpdateClientService.API/Services/Configuration/ConfigurationService.cs");
		}
		else
		{
			string arg = (configurationSettingChangesData.Success ? "Successful" : "Failed");
			_logger.LogInfoWithSource($"Processing of configuration changes from verion {configurationSettingChangesData.CurrentVersionId} to version {configurationSettingChangesData.NewVersionId} {arg}", "ProcessConfigurationSettingChanges", "/sln/src/UpdateClientService.API/Services/Configuration/ConfigurationService.cs");
		}
		return configurationSettingChangesData.Success;
	}

	private async Task ActivateConfigVersion(ConfigurationSettingChangesData configurationSettingChangesData)
	{
		if (!configurationSettingChangesData.Success || (configurationSettingChangesData.KioskConfigurationChangesResponse?.VersionIsCurrent ?? false))
		{
			return;
		}
		await Task.Run(delegate
		{
			KioskConfigurationVersionActivationRequest kioskConfigurationVersionActivationRequest = new KioskConfigurationVersionActivationRequest
			{
				KioskId = _storeService.KioskId,
				ModifiedBy = "System",
				ActivationDateTimeUtc = DateTime.UtcNow,
				ConfigurationVersionId = configurationSettingChangesData.NewVersionId
			};
			Task.Run(async delegate
			{
				await ActivateConfigurationVersion(kioskConfigurationVersionActivationRequest);
			});
		});
	}

	private async Task ValidateResponse(ConfigurationSettingChangesData configurationSettingChangesData)
	{
		if (!configurationSettingChangesData.Success)
		{
			return;
		}
		await Task.Run(delegate
		{
			if (configurationSettingChangesData.KioskConfigurationChangesResponse.OriginalConfigurationVersionId != configurationSettingChangesData.CurrentVersionId)
			{
				configurationSettingChangesData.Success = false;
				_logger.LogErrorWithSource($"Error in processing configuration changes.  Current configuration version id {configurationSettingChangesData?.CurrentVersionId} doesn't match change results original configuration version id {configurationSettingChangesData?.KioskConfigurationChangesResponse?.OriginalConfigurationVersionId}", "ValidateResponse", "/sln/src/UpdateClientService.API/Services/Configuration/ConfigurationService.cs");
			}
		});
	}

	private async Task RenameTempFileAsCurrent(ConfigurationSettingChangesData configurationSettingChangesData)
	{
		if (configurationSettingChangesData.Success)
		{
			configurationSettingChangesData.Success = await RenameFile(configurationSettingChangesData.TempConfigFileName, ConfigurationFilePath);
		}
	}

	private async Task RenameCurrentConfigFile(ConfigurationSettingChangesData configurationSettingChangesData)
	{
		if (configurationSettingChangesData.Success)
		{
			ConfigurationSettingChangesData configurationSettingChangesData2 = configurationSettingChangesData;
			configurationSettingChangesData2.Success = await DeleteFile(OldConfigurationFilePath);
			if (configurationSettingChangesData.Success && File.Exists(ConfigurationFilePath))
			{
				configurationSettingChangesData2 = configurationSettingChangesData;
				configurationSettingChangesData2.Success = await RenameFile(ConfigurationFilePath, OldConfigurationFilePath);
			}
		}
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
				_logger.LogErrorWithSource(ex, "Unable to rename file " + originalFileName + " to " + newFileName, "RenameFile", "/sln/src/UpdateClientService.API/Services/Configuration/ConfigurationService.cs");
			}
		});
		return result;
	}

	private async Task SaveTempConfigurationFile(ConfigurationSettingChangesData configurationSettingChangesData)
	{
		if (configurationSettingChangesData.Success)
		{
			configurationSettingChangesData.Success = await SaveConfigurationFile(configurationSettingChangesData.TempConfigFileName, configurationSettingChangesData.KioskConfigurationSettingValues);
		}
	}

	private async Task<bool> SaveConfigurationFile(string configurationFileName, GetKioskConfigurationSettingValues kioskConfigurationSettingValues)
	{
		bool result = false;
		string json = null;
		await Task.Run(delegate
		{
			try
			{
				KioskConfigurationSettingsFile value = new KioskConfigurationSettingsFile
				{
					KioskConfigurationSettings = kioskConfigurationSettingValues
				};
				json = JsonConvert.SerializeObject(value, Formatting.Indented);
				File.WriteAllText(configurationFileName, json);
				result = true;
			}
			catch (Exception ex)
			{
				_logger.LogErrorWithSource(ex, $"Error saving configiration file {configurationFileName}.  json length: {json.Length}", "SaveConfigurationFile", "/sln/src/UpdateClientService.API/Services/Configuration/ConfigurationService.cs");
			}
		});
		return result;
	}

	private async Task AddNewSettings(ConfigurationSettingChangesData configurationSettingChangesData)
	{
		if (!configurationSettingChangesData.Success)
		{
			return;
		}
		GetKioskConfigurationSettingValues getKioskConfigurationSettingValues = configurationSettingChangesData?.KioskConfigurationChangesResponse?.NewConfigurationSettingValues;
		GetKioskConfigurationSettingValues kioskConfigurationSettingValues = configurationSettingChangesData.KioskConfigurationSettingValues;
		kioskConfigurationSettingValues.ConfigurationVersion = getKioskConfigurationSettingValues.ConfigurationVersion;
		kioskConfigurationSettingValues.ConfigurationVersionHash = configurationSettingChangesData.KioskConfigurationChangesResponse?.ConfigurationVersionHash;
		kioskConfigurationSettingValues.KioskId = getKioskConfigurationSettingValues.KioskId;
		_logger.LogInfoWithSource($"Updating {getKioskConfigurationSettingValues.Categories.Sum((KioskConfigurationCategory c) => c.Settings.Sum((KioskSetting s) => s.SettingValues.Count))} setting Values", "AddNewSettings", "/sln/src/UpdateClientService.API/Services/Configuration/ConfigurationService.cs");
		await AddNewSettings(kioskConfigurationSettingValues, getKioskConfigurationSettingValues);
	}

	private async Task AddNewSettings(GetKioskConfigurationSettingValues settingValuesDestination, GetKioskConfigurationSettingValues settingValuesToAdd)
	{
		await Task.Run(delegate
		{
			if (settingValuesToAdd != null && settingValuesDestination != null)
			{
				foreach (KioskConfigurationCategory category in settingValuesToAdd.Categories)
				{
					KioskConfigurationCategory orAddCategory = GetOrAddCategory(category, settingValuesDestination);
					foreach (KioskSetting setting in category.Settings)
					{
						KioskSetting orAddSetting = GetOrAddSetting(setting, orAddCategory);
						foreach (KioskSettingValue settingValue in setting.SettingValues)
						{
							AddSettingValue(settingValue, orAddSetting);
						}
						orAddSetting.SettingValues.Sort(sortSettingValues);
					}
					orAddCategory.Settings.Sort(sortSettings);
				}
				settingValuesDestination.Categories.Sort(sortCategories);
			}
		});
		static int sortCategories(KioskConfigurationCategory a, KioskConfigurationCategory b)
		{
			return a.Name.CompareTo(b.Name);
		}
		static int sortSettingValues(KioskSettingValue a, KioskSettingValue b)
		{
			return a.ConfigurationSettingValueId.CompareTo(b.ConfigurationSettingValueId);
		}
		static int sortSettings(KioskSetting a, KioskSetting b)
		{
			return a.Name.CompareTo(b.Name);
		}
	}

	private void AddSettingValue(KioskSettingValue kioskSettingValue, KioskSetting kioskSetting)
	{
		KioskSettingValue kioskSettingValue2 = kioskSetting.SettingValues.FirstOrDefault((KioskSettingValue x) => x.ConfigurationSettingValueId == kioskSettingValue.ConfigurationSettingValueId);
		if (kioskSettingValue2 == null)
		{
			kioskSettingValue2 = new KioskSettingValue
			{
				ConfigurationSettingValueId = kioskSettingValue.ConfigurationSettingValueId,
				EffectiveDateTime = kioskSettingValue.EffectiveDateTime,
				ExpireDateTime = kioskSettingValue.ExpireDateTime,
				Rank = kioskSettingValue.Rank,
				SegmentationName = kioskSettingValue.SegmentationName,
				SegmentId = kioskSettingValue.SegmentId,
				SegmentName = kioskSettingValue.SegmentName,
				EncryptionType = kioskSettingValue.EncryptionType,
				Value = kioskSettingValue.Value
			};
			kioskSetting.SettingValues.Add(kioskSettingValue2);
		}
	}

	private KioskSetting GetOrAddSetting(KioskSetting kioskSetting, KioskConfigurationCategory kioskConfigurationCategory)
	{
		KioskSetting kioskSetting2 = kioskConfigurationCategory.Settings.FirstOrDefault((KioskSetting x) => x.SettingId == kioskSetting.SettingId);
		if (kioskSetting2 == null)
		{
			kioskSetting2 = new KioskSetting
			{
				Name = kioskSetting.Name,
				SettingId = kioskSetting.SettingId,
				DataType = kioskSetting.DataType
			};
			kioskConfigurationCategory.Settings.Add(kioskSetting2);
		}
		return kioskSetting2;
	}

	private KioskConfigurationCategory GetOrAddCategory(KioskConfigurationCategory kioskConfigurationCategory, GetKioskConfigurationSettingValues kioskConfigurationSettingValues)
	{
		KioskConfigurationCategory kioskConfigurationCategory2 = kioskConfigurationSettingValues.Categories.FirstOrDefault((KioskConfigurationCategory x) => x.Name == kioskConfigurationCategory?.Name);
		if (kioskConfigurationCategory2 == null)
		{
			kioskConfigurationCategory2 = new KioskConfigurationCategory
			{
				Name = kioskConfigurationCategory.Name
			};
			kioskConfigurationSettingValues.Categories.Add(kioskConfigurationCategory2);
		}
		return kioskConfigurationCategory2;
	}

	private async Task ClearOldSettings(ConfigurationSettingChangesData configurationSettingChangesData)
	{
		if (!configurationSettingChangesData.Success)
		{
			return;
		}
		await Task.Run(delegate
		{
			ConfigurationSettingChangesData configurationSettingChangesData2 = configurationSettingChangesData;
			if (configurationSettingChangesData2 != null && configurationSettingChangesData2.KioskConfigurationChangesResponse?.RemoveAllExistingConfigurationSettingValues == true)
			{
				configurationSettingChangesData.KioskConfigurationSettingValues.Categories.Clear();
			}
			else
			{
				ConfigurationSettingChangesData configurationSettingChangesData3 = configurationSettingChangesData;
				if (configurationSettingChangesData3 != null && configurationSettingChangesData3.KioskConfigurationChangesResponse?.RemovedConfigurationSettingValueIds?.Count() > 0)
				{
					foreach (KioskConfigurationCategory eachCategory in configurationSettingChangesData.KioskConfigurationSettingValues.Categories)
					{
						foreach (KioskSetting eachSetting in eachCategory.Settings)
						{
							List<KioskSettingValue> list = eachSetting.SettingValues.Where((KioskSettingValue x) => configurationSettingChangesData.KioskConfigurationChangesResponse.RemovedConfigurationSettingValueIds.Contains(x.ConfigurationSettingValueId)).ToList();
							list.ForEach(delegate(KioskSettingValue x)
							{
								eachSetting.SettingValues.Remove(x);
							});
						}
						List<KioskSetting> list2 = eachCategory.Settings.Where((KioskSetting x) => !x.SettingValues.Any()).ToList();
						list2.ForEach(delegate(KioskSetting x)
						{
							eachCategory.Settings.Remove(x);
						});
					}
					List<KioskConfigurationCategory> list3 = configurationSettingChangesData.KioskConfigurationSettingValues.Categories.Where((KioskConfigurationCategory x) => !x.Settings.Any()).ToList();
					list3.ForEach(delegate(KioskConfigurationCategory x)
					{
						configurationSettingChangesData.KioskConfigurationSettingValues.Categories.Remove(x);
					});
				}
			}
		});
	}

	private async Task<(bool, GetKioskConfigurationSettingValues)> ReadConfigurationFile(string configurationFileName)
	{
		bool result = false;
		GetKioskConfigurationSettingValues getKioskConfigurationSettingValues = null;
		await Task.Run(delegate
		{
			try
			{
				if (File.Exists(configurationFileName))
				{
					string value = File.ReadAllText(configurationFileName);
					getKioskConfigurationSettingValues = JsonConvert.DeserializeObject<KioskConfigurationSettingsFile>(value)?.KioskConfigurationSettings;
					result = true;
				}
				else
				{
					result = false;
				}
			}
			catch (Exception ex)
			{
				_logger.LogErrorWithSource(ex, "Exception reading configuration file " + configurationFileName, "ReadConfigurationFile", "/sln/src/UpdateClientService.API/Services/Configuration/ConfigurationService.cs");
			}
		});
		return (result, getKioskConfigurationSettingValues);
	}

	private async Task CleanupFailedAttempt(ConfigurationSettingChangesData configurationSettingChangesData)
	{
		if (!configurationSettingChangesData.Success)
		{
			if (!File.Exists(ConfigurationFilePath) && File.Exists(OldConfigurationFilePath))
			{
				await RenameFile(OldConfigurationFilePath, ConfigurationFilePath);
			}
			await DeleteTempConfigurationFile(configurationSettingChangesData);
		}
	}

	private async Task InitializePath(ConfigurationSettingChangesData configurationSettingChangesData)
	{
		if (configurationSettingChangesData.Success)
		{
			ConfigurationSettingChangesData configurationSettingChangesData2 = configurationSettingChangesData;
			configurationSettingChangesData2.Success = await CreateConfigFilePathIfNeeded(ConfigurationFileDirectory);
			if (configurationSettingChangesData.Success)
			{
				configurationSettingChangesData.TempConfigFileName = Path.Combine(ConfigurationFileDirectory, $"configuration v{configurationSettingChangesData?.KioskConfigurationChangesResponse?.NewConfigurationSettingValues?.ConfigurationVersion}.json");
				configurationSettingChangesData2 = configurationSettingChangesData;
				configurationSettingChangesData2.Success = await DeleteTempConfigurationFile(configurationSettingChangesData);
			}
		}
	}

	private async Task<bool> CreateConfigFilePathIfNeeded(string configFilePath)
	{
		bool result = true;
		await Task.Run(delegate
		{
			if (!Directory.Exists(configFilePath))
			{
				DirectoryInfo directoryInfo = Directory.CreateDirectory(configFilePath);
				if (!directoryInfo.Exists)
				{
					result = false;
					_logger.LogErrorWithSource("Unable to create configuration path " + configFilePath, "CreateConfigFilePathIfNeeded", "/sln/src/UpdateClientService.API/Services/Configuration/ConfigurationService.cs");
				}
			}
		});
		return result;
	}

	private async Task<bool> DeleteTempConfigurationFile(ConfigurationSettingChangesData configurationSettingChangesData)
	{
		return await DeleteFile(configurationSettingChangesData.TempConfigFileName);
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
				_logger.LogErrorWithSource(ex, "Unable to delete file " + fileName, "DeleteFile", "/sln/src/UpdateClientService.API/Services/Configuration/ConfigurationService.cs");
			}
		});
		return result;
	}
}
