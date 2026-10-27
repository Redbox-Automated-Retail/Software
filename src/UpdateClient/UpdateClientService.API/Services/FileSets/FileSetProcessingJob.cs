using System;
using System.Threading.Tasks;
using Coravel.Invocable;
using Microsoft.Extensions.Logging;
using Redbox.NetCore.Logging.Extensions;

namespace UpdateClientService.API.Services.FileSets;

public class FileSetProcessingJob : IFileSetProcessingJob, IInvocable
{
	private readonly IFileSetService _fileSetService;

	private readonly ILogger<FileSetProcessingJob> _logger;

	public FileSetProcessingJob(ILogger<FileSetProcessingJob> logger, IFileSetService fileSetService)
	{
		_fileSetService = fileSetService;
		_logger = logger;
	}

	public async Task Invoke()
	{
		try
		{
			await _fileSetService.ProcessInProgressRevisionChangeSets();
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "Exception while running FileSetProcessingJob.", "Invoke", "/sln/src/UpdateClientService.API/Services/FileSets/FileSetProcessingJob.cs");
		}
	}
}
