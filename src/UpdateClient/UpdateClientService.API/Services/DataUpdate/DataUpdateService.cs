using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Redbox.NetCore.Logging.Extensions;
using UpdateClientService.API.Services.IoT;
using UpdateClientService.API.Services.IoT.Commands;
using UpdateClientService.API.Services.IoT.Commands.KioskFiles;
using UpdateClientService.API.Services.IoT.IoTCommand;

namespace UpdateClientService.API.Services.DataUpdate;

public class DataUpdateService : IDataUpdateService
{
	private class RecordChangesData
	{
		public GetRecordChangesResponse GetRecordChangesResponse { get; set; } = new GetRecordChangesResponse();

		public DataUpdateResponse DataUpdateResponse { get; set; } = new DataUpdateResponse();

		public DataUpdateResponse CurrentDataUpdateResponse { get; set; }

		public IoTCommandResponse<ObjectResult> IoTCommandResponse { get; set; }

		public DataUpdateRequest DataUpdateRequest { get; set; }

		public bool AllPagesLoadedOrAborted { get; set; }
	}

	private readonly ILogger<DataUpdateService> _logger;

	private readonly IIoTCommandClient _iotCommandClient;

	private string DefaultDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Redbox\\Data");

	public DataUpdateService(ILogger<DataUpdateService> logger, IIoTCommandClient iotCommandClient)
	{
		_logger = logger;
		_iotCommandClient = iotCommandClient;
	}

	public async Task<GetRecordChangesResponse> GetRecordChanges(DataUpdateRequest dataUpdateRequest)
	{
		RecordChangesData recordChangesData = new RecordChangesData
		{
			DataUpdateRequest = dataUpdateRequest
		};
		try
		{
			while (!recordChangesData.AllPagesLoadedOrAborted)
			{
				await SendDataUpdateRequest(recordChangesData);
				GetDataUpdateResponse(recordChangesData);
				await ProcessDataUpdateResponse(recordChangesData);
			}
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, $"Error in GetRecordChanges for Table  {dataUpdateRequest?.TableName}.", "GetRecordChanges", "/sln/src/UpdateClientService.API/Services/DataUpdate/DataUpdateService.cs");
			recordChangesData.GetRecordChangesResponse.StatusCode = HttpStatusCode.InternalServerError;
		}
		return recordChangesData.GetRecordChangesResponse;
	}

	private async Task ProcessDataUpdateResponse(RecordChangesData recordChangesData)
	{
		if (recordChangesData.AllPagesLoadedOrAborted)
		{
			return;
		}
		if (recordChangesData.CurrentDataUpdateResponse.IsFirstPage)
		{
			recordChangesData.DataUpdateResponse = recordChangesData.CurrentDataUpdateResponse;
			if (!recordChangesData.CurrentDataUpdateResponse.IsLastPage)
			{
				_logger.LogInfoWithSource($"Response has {recordChangesData.CurrentDataUpdateResponse?.PageCount} pages", "ProcessDataUpdateResponse", "/sln/src/UpdateClientService.API/Services/DataUpdate/DataUpdateService.cs");
			}
		}
		else if (recordChangesData.CurrentDataUpdateResponse.RecordResponses?.Any() ?? false)
		{
			recordChangesData.DataUpdateResponse.RecordResponses.AddRange(recordChangesData.CurrentDataUpdateResponse.RecordResponses);
		}
		if (recordChangesData.CurrentDataUpdateResponse.IsLastPage)
		{
			recordChangesData.DataUpdateResponse.PageCount = null;
			recordChangesData.DataUpdateResponse.PageNumber = null;
			if (await WriteRecordUpatesToFile(recordChangesData))
			{
				recordChangesData.GetRecordChangesResponse.StatusCode = HttpStatusCode.OK;
			}
			else
			{
				recordChangesData.GetRecordChangesResponse.StatusCode = HttpStatusCode.InternalServerError;
			}
			recordChangesData.AllPagesLoadedOrAborted = true;
		}
		else
		{
			recordChangesData.DataUpdateRequest.PageNumber = recordChangesData.CurrentDataUpdateResponse.PageNumber + 1;
		}
	}

	private void GetDataUpdateResponse(RecordChangesData recordChangesData)
	{
		if (recordChangesData.AllPagesLoadedOrAborted)
		{
			return;
		}
		IoTCommandResponse<ObjectResult> ioTCommandResponse = recordChangesData.IoTCommandResponse;
		if (ioTCommandResponse != null && ioTCommandResponse.StatusCode == 200)
		{
			try
			{
				recordChangesData.CurrentDataUpdateResponse = JsonConvert.DeserializeObject<DataUpdateResponse>(recordChangesData.IoTCommandResponse?.Payload?.Value?.ToString());
				if (recordChangesData.CurrentDataUpdateResponse == null)
				{
					recordChangesData.GetRecordChangesResponse.StatusCode = HttpStatusCode.InternalServerError;
					_logger.LogErrorWithSource(string.Format("Unable to deserialize Iot command payload as {0} for RequestId {1}", "DataUpdateResponse", recordChangesData?.DataUpdateRequest?.RequestId), "GetDataUpdateResponse", "/sln/src/UpdateClientService.API/Services/DataUpdate/DataUpdateService.cs");
					recordChangesData.AllPagesLoadedOrAborted = true;
				}
				return;
			}
			catch (Exception ex)
			{
				_logger.LogErrorWithSource(ex, string.Format("Exception while deserializing {0} for RequestId {1}", "DataUpdateResponse", recordChangesData?.DataUpdateRequest?.RequestId), "GetDataUpdateResponse", "/sln/src/UpdateClientService.API/Services/DataUpdate/DataUpdateService.cs");
				return;
			}
		}
		recordChangesData.GetRecordChangesResponse.StatusCode = HttpStatusCode.InternalServerError;
		_logger.LogErrorWithSource($"Iot command {CommandEnum.GetRecordChanges} returned statuscode {recordChangesData.IoTCommandResponse?.StatusCode}", "GetDataUpdateResponse", "/sln/src/UpdateClientService.API/Services/DataUpdate/DataUpdateService.cs");
		recordChangesData.AllPagesLoadedOrAborted = true;
	}

	private async Task SendDataUpdateRequest(RecordChangesData recordChangesData)
	{
		recordChangesData.IoTCommandResponse = await _iotCommandClient.PerformIoTCommand<ObjectResult>(new IoTCommandModel
		{
			Version = 1,
			RequestId = Guid.NewGuid().ToString(),
			Command = CommandEnum.GetRecordChanges,
			Payload = recordChangesData.DataUpdateRequest,
			QualityOfServiceLevel = QualityOfServiceLevel.AtLeastOnce,
			LogResponse = false,
			LogRequest = false
		}, new PerformIoTCommandParameters
		{
			IoTTopic = "$aws/rules/kioskrestcall",
			WaitForResponse = true
		});
	}

	private async Task<bool> WriteRecordUpatesToFile(RecordChangesData recordChangesData)
	{
		bool result = false;
		string json = null;
		string filePath = GetDataUpdateResponseFileName(recordChangesData);
		await Task.Run(async delegate
		{
			try
			{
				bool flag = !string.IsNullOrEmpty(filePath);
				bool flag2 = flag;
				if (flag2)
				{
					flag2 = await CreateFilePathIfNeeded(filePath);
				}
				if (flag2 && recordChangesData?.DataUpdateResponse != null)
				{
					json = recordChangesData.DataUpdateResponse.ToJsonIndented();
					File.WriteAllText(filePath, json);
					recordChangesData.GetRecordChangesResponse.DataUpdateResponseFileName = filePath;
					result = true;
				}
			}
			catch (Exception ex)
			{
				_logger.LogErrorWithSource(ex, $"Error saving configiration file {filePath}.  json length: {json.Length}", "WriteRecordUpatesToFile", "/sln/src/UpdateClientService.API/Services/DataUpdate/DataUpdateService.cs");
			}
		});
		return result;
	}

	private async Task<bool> CreateFilePathIfNeeded(string filePath)
	{
		bool result = true;
		await Task.Run(delegate
		{
			try
			{
				string directoryName = Path.GetDirectoryName(filePath);
				if (!Directory.Exists(directoryName))
				{
					DirectoryInfo directoryInfo = Directory.CreateDirectory(directoryName);
					if (!directoryInfo.Exists)
					{
						result = false;
						_logger.LogErrorWithSource("Unable to create directory " + directoryName, "CreateFilePathIfNeeded", "/sln/src/UpdateClientService.API/Services/DataUpdate/DataUpdateService.cs");
					}
				}
			}
			catch (Exception ex)
			{
				_logger.LogErrorWithSource(ex, "Exception while checking/creating directory for " + filePath, "CreateFilePathIfNeeded", "/sln/src/UpdateClientService.API/Services/DataUpdate/DataUpdateService.cs");
			}
		});
		return result;
	}

	private string GetDataUpdateResponseFileName(RecordChangesData recordChangesData)
	{
		if (recordChangesData?.DataUpdateRequest == null)
		{
			_logger.LogErrorWithSource("DataUpdateRequest must have a value.", "GetDataUpdateResponseFileName", "/sln/src/UpdateClientService.API/Services/DataUpdate/DataUpdateService.cs");
			return null;
		}
		string path = $"{recordChangesData.DataUpdateRequest?.TableName}-update-{recordChangesData.DataUpdateRequest?.RequestId}.json";
		string value = null;
		if (!recordChangesData.DataUpdateRequest.FileType.HasValue || !FilePaths.TypeMappings.TryGetValue(recordChangesData.DataUpdateRequest.FileType.Value, out value))
		{
			value = DefaultDataFolder;
		}
		value = Path.Combine(value, recordChangesData.DataUpdateRequest.TableName.ToString());
		return Path.Combine(value, path);
	}
}
