using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Redbox.NetCore.Logging.Extensions;
using UpdateClientService.API.Services.FileSets;
using UpdateClientService.API.Services.IoT.Commands;
using UpdateClientService.API.Services.IoT.IoTCommand;

namespace UpdateClientService.API.Services.IoT.FileSets;

public class KioskFileSetVersionsService : IKioskFileSetVersionsService
{
	private readonly IIoTCommandClient _iotCommandClient;

	private readonly IStoreService _store;

	private readonly IStateFileService _stateFileService;

	private readonly ILogger<KioskFileSetVersionsService> _logger;

	public KioskFileSetVersionsService(IIoTCommandClient iotCommandClient, IStoreService store, IStateFileService stateFileService, ILogger<KioskFileSetVersionsService> logger)
	{
		_iotCommandClient = iotCommandClient;
		_store = store;
		_stateFileService = stateFileService;
		_logger = logger;
	}

	public async Task<ReportFileSetVersionsResponse> ReportFileSetVersion(FileSetPollRequest fileSetPollRequest)
	{
		if (fileSetPollRequest == null)
		{
			_logger.LogErrorWithSource("FileSetPollRequest is null.", "ReportFileSetVersion", "/sln/src/UpdateClientService.API/Services/IoT/FileSets/KioskFileSetVersionsService.cs");
			return new ReportFileSetVersionsResponse
			{
				StatusCode = HttpStatusCode.InternalServerError
			};
		}
		return await InnerReportFileSetVersions(fileSetPollRequest);
	}

	public async Task<ReportFileSetVersionsResponse> ReportFileSetVersions()
	{
		return await InnerReportFileSetVersions();
	}

	private async Task<ReportFileSetVersionsResponse> InnerReportFileSetVersions(FileSetPollRequest fileSetPollRequest = null)
	{
		ReportFileSetVersionsResponse response = new ReportFileSetVersionsResponse();
		var (flag, fileSetPollRequestList) = await GetPollRequest();
		if (!flag)
		{
			_logger.LogErrorWithSource("Couldn't report file set versions", "InnerReportFileSetVersions", "/sln/src/UpdateClientService.API/Services/IoT/FileSets/KioskFileSetVersionsService.cs");
		}
		else
		{
			if (fileSetPollRequest != null)
			{
				FileSetPollRequest fileSetPollRequest2 = fileSetPollRequestList.FileSetPollRequests.FirstOrDefault((FileSetPollRequest x) => x.FileSetId == fileSetPollRequest.FileSetId && x.FileSetRevisionId == fileSetPollRequest.FileSetRevisionId);
				if (fileSetPollRequest2 != null)
				{
					fileSetPollRequestList.FileSetPollRequests.Remove(fileSetPollRequest2);
				}
				fileSetPollRequestList.FileSetPollRequests.Add(fileSetPollRequest);
			}
			response = await ReportFileSetVersions(fileSetPollRequestList);
		}
		return response;
	}

	private async Task<ReportFileSetVersionsResponse> ReportFileSetVersions(FileSetPollRequestList fileSetPollRequestList)
	{
		ReportFileSetVersionsResponse response = new ReportFileSetVersionsResponse();
		if (fileSetPollRequestList == null)
		{
			response.StatusCode = HttpStatusCode.InternalServerError;
			_logger.LogErrorWithSource("FileSetPollRequestList is null", "ReportFileSetVersions", "/sln/src/UpdateClientService.API/Services/IoT/FileSets/KioskFileSetVersionsService.cs");
		}
		else
		{
			IoTCommandResponse<ObjectResult> ioTCommandResponse = await _iotCommandClient.PerformIoTCommand<ObjectResult>(new IoTCommandModel
			{
				RequestId = Guid.NewGuid().ToString(),
				Version = 3,
				SourceId = _store.KioskId.ToString(),
				Payload = fileSetPollRequestList,
				Command = CommandEnum.ReportFileSetVersions
			}, new PerformIoTCommandParameters
			{
				RequestTimeout = TimeSpan.FromSeconds(15.0),
				IoTTopic = "$aws/rules/kioskrestcall",
				WaitForResponse = true
			});
			if (ioTCommandResponse != null && ioTCommandResponse.StatusCode == 200 && ioTCommandResponse != null && ioTCommandResponse.Payload?.StatusCode == 200)
			{
				try
				{
					response.ClientFileSetRevisionChangeSets = JsonConvert.DeserializeObject<List<ClientFileSetRevisionChangeSet>>(ioTCommandResponse.Payload.Value.ToString());
				}
				catch (Exception ex)
				{
					_logger.LogErrorWithSource(ex, "Exception deserializing response", "ReportFileSetVersions", "/sln/src/UpdateClientService.API/Services/IoT/FileSets/KioskFileSetVersionsService.cs");
				}
			}
			else
			{
				response.StatusCode = HttpStatusCode.InternalServerError;
				_logger.LogErrorWithSource("Unable to report FileSet revision deployment states.", "ReportFileSetVersions", "/sln/src/UpdateClientService.API/Services/IoT/FileSets/KioskFileSetVersionsService.cs");
			}
		}
		return response;
	}

	private async Task<(bool success, FileSetPollRequestList)> GetPollRequest()
	{
		StateFilesResponse stateFilesResponse = await _stateFileService.GetAll();
		if (stateFilesResponse.StatusCode != HttpStatusCode.OK && stateFilesResponse.StatusCode != HttpStatusCode.NotFound)
		{
			_logger?.LogErrorWithSource("Unable to get StateFiles.", "GetPollRequest", "/sln/src/UpdateClientService.API/Services/IoT/FileSets/KioskFileSetVersionsService.cs");
			return (success: false, null);
		}
		FileSetPollRequestList fileSetPollRequestList = new FileSetPollRequestList();
		if (stateFilesResponse.StateFiles != null)
		{
			foreach (StateFile stateFile in stateFilesResponse.StateFiles)
			{
				fileSetPollRequestList.FileSetPollRequests.Add(new FileSetPollRequest
				{
					FileSetId = stateFile.FileSetId,
					FileSetRevisionId = stateFile.CurrentRevisionId,
					FileSetState = stateFile.CurrentFileSetState
				});
			}
		}
		return (success: true, fileSetPollRequestList);
	}
}
