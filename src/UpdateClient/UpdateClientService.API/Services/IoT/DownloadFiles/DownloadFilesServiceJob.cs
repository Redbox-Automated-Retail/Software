using System.Threading.Tasks;
using Coravel.Invocable;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Redbox.NetCore.Logging.Extensions;

namespace UpdateClientService.API.Services.IoT.DownloadFiles;

public class DownloadFilesServiceJob : IInvocable
{
	private ILogger<DownloadFilesServiceJob> _logger;

	private IDownloadFilesService _downloadFilesService;

	private DownloadFilesSettings _settings;

	public DownloadFilesServiceJob(ILogger<DownloadFilesServiceJob> logger, IDownloadFilesService downloadFilesService, IOptionsMonitor<AppSettings> settings)
	{
		_logger = logger;
		_downloadFilesService = downloadFilesService;
		_settings = settings.CurrentValue.DownloadFiles;
	}

	public async Task Invoke()
	{
		if (_settings?.Enabled ?? false)
		{
			_logger.LogInfoWithSource("HandleScheduledJobs", "Invoke", "/sln/src/UpdateClientService.API/Services/IoT/DownloadFiles/DownloadFilesServiceJob.cs");
			await _downloadFilesService.HandleScheduledJobs();
		}
	}
}
