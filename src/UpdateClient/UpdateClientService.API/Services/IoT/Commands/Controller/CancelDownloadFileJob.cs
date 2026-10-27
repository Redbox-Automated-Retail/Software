using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Redbox.NetCore.Logging.Extensions;
using UpdateClientService.API.Services.IoT.Commands.DownloadFiles;
using UpdateClientService.API.Services.IoT.DownloadFiles;

namespace UpdateClientService.API.Services.IoT.Commands.Controller;

public class CancelDownloadFileJob : ICommandIoTController
{
	private readonly IDownloadFilesService _downloadFilesService;

	private readonly IStoreService _store;

	private readonly ILogger<CancelDownloadFileJob> _logger;

	public CommandEnum CommandEnum => CommandEnum.CancelDownloadFileJob;

	public int Version => 1;

	public CancelDownloadFileJob(IDownloadFilesService downloadFilesService, IStoreService store, ILogger<CancelDownloadFileJob> logger)
	{
		_downloadFilesService = downloadFilesService;
		_store = store;
		_logger = logger;
	}

	public async Task Execute(IoTCommandModel ioTCommand)
	{
		try
		{
			DownloadFileJob job = JsonConvert.DeserializeObject<DownloadFileJob>(ioTCommand.Payload.ToString());
			if (IsDownloadFileJobValid(job))
			{
				await _downloadFilesService.CancelDownloadFileJob(job);
			}
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "Exception while executing command " + ioTCommand?.ToJson(), "Execute", "/sln/src/UpdateClientService.API/Services/IoT/Commands/Controller/CancelDownloadFileJob.cs");
		}
	}

	private bool IsDownloadFileJobValid(DownloadFileJob job)
	{
		string downloadFileJobId = job.DownloadFileJobId;
		if (job == null)
		{
			_logger.LogErrorWithSource("Job cannot be null", "IsDownloadFileJobValid", "/sln/src/UpdateClientService.API/Services/IoT/Commands/Controller/CancelDownloadFileJob.cs");
			return false;
		}
		if (!job.TargetKiosks.Contains(_store.KioskId))
		{
			_logger.LogErrorWithSource($"Job's target kiosks does not include {_store.KioskId}", "IsDownloadFileJobValid", "/sln/src/UpdateClientService.API/Services/IoT/Commands/Controller/CancelDownloadFileJob.cs");
		}
		if (string.IsNullOrWhiteSpace(downloadFileJobId))
		{
			_logger.LogErrorWithSource("DownloadFileJobId cannot be null.", "IsDownloadFileJobValid", "/sln/src/UpdateClientService.API/Services/IoT/Commands/Controller/CancelDownloadFileJob.cs");
			return false;
		}
		DownloadFileJobExecutionState.Executions.TryAdd(downloadFileJobId, value: true);
		return true;
	}
}
