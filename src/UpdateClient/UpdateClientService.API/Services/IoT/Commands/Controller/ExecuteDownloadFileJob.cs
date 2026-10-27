using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Redbox.NetCore.Logging.Extensions;
using UpdateClientService.API.Services.IoT.Commands.DownloadFiles;
using UpdateClientService.API.Services.IoT.DownloadFiles;

namespace UpdateClientService.API.Services.IoT.Commands.Controller;

public class ExecuteDownloadFileJob : ICommandIoTController
{
	private readonly IDownloadFilesService _downloadFilesService;

	private readonly IStoreService _store;

	private readonly ILogger<ExecuteDownloadFileJob> _logger;

	public CommandEnum CommandEnum => CommandEnum.ExecuteDownloadFileJob;

	public int Version => 1;

	public ExecuteDownloadFileJob(IDownloadFilesService downloadFilesService, IStoreService store, ILogger<ExecuteDownloadFileJob> logger)
	{
		_downloadFilesService = downloadFilesService;
		_store = store;
		_logger = logger;
	}

	public async Task Execute(IoTCommandModel ioTCommand)
	{
		try
		{
			DownloadFileJob requestModel = JsonConvert.DeserializeObject<DownloadFileJob>(ioTCommand.Payload.ToString());
			if (IsDownloadFileJobValid(requestModel))
			{
				await _downloadFilesService.HandleDownloadFileJob(requestModel);
				DownloadFileJobExecutionState.Executions.TryRemove(requestModel.DownloadFileJobId, out var _);
			}
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "Exception while executing command " + ioTCommand?.ToJson(), "Execute", "/sln/src/UpdateClientService.API/Services/IoT/Commands/Controller/ExecuteDownloadFileJob.cs");
		}
	}

	private bool IsDownloadFileJobValid(DownloadFileJob job)
	{
		string downloadFileJobId = job.DownloadFileJobId;
		if (job == null)
		{
			_logger.LogErrorWithSource("Job cannot be null", "IsDownloadFileJobValid", "/sln/src/UpdateClientService.API/Services/IoT/Commands/Controller/ExecuteDownloadFileJob.cs");
			return false;
		}
		if (!job.TargetKiosks.Contains(_store.KioskId))
		{
			_logger.LogErrorWithSource($"Job's target kiosks does not include {_store.KioskId}", "IsDownloadFileJobValid", "/sln/src/UpdateClientService.API/Services/IoT/Commands/Controller/ExecuteDownloadFileJob.cs");
		}
		if (string.IsNullOrWhiteSpace(downloadFileJobId))
		{
			_logger.LogErrorWithSource("DownloadFileJobId cannot be null.", "IsDownloadFileJobValid", "/sln/src/UpdateClientService.API/Services/IoT/Commands/Controller/ExecuteDownloadFileJob.cs");
			return false;
		}
		if (!DownloadFileJobExecutionState.Executions.TryAdd(downloadFileJobId, value: true))
		{
			_logger.LogInfoWithSource("Another job with DownloadFileJobId " + downloadFileJobId + " is already in progress. Skipping...", "IsDownloadFileJobValid", "/sln/src/UpdateClientService.API/Services/IoT/Commands/Controller/ExecuteDownloadFileJob.cs");
			return false;
		}
		return true;
	}
}
