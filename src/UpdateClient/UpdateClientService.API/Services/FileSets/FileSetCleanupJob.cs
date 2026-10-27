using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Coravel.Invocable;
using Microsoft.Extensions.Logging;
using Redbox.NetCore.Logging.Extensions;

namespace UpdateClientService.API.Services.FileSets;

public class FileSetCleanupJob : IFileSetCleanupJob, IInvocable
{
	private readonly IFileSetCleanup _fileSetCleanup;

	private readonly ILogger<FileSetCleanupJob> _logger;

	public FileSetCleanupJob(IFileSetCleanup fileSetCleanup, ILogger<FileSetCleanupJob> logger)
	{
		_fileSetCleanup = fileSetCleanup;
		_logger = logger;
	}

	public async Task Invoke()
	{
		try
		{
			Stopwatch sw = Stopwatch.StartNew();
			_logger.LogInfoWithSource("Starting FileSetCleanup", "Invoke", "/sln/src/UpdateClientService.API/Services/FileSets/FileSetCleanupJob.cs");
			await _fileSetCleanup.Run();
			_logger.LogInfoWithSource($"Finished FileSetCleanup in {sw.ElapsedMilliseconds} ms", "Invoke", "/sln/src/UpdateClientService.API/Services/FileSets/FileSetCleanupJob.cs");
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "Exception while running FileSet cleanup.", "Invoke", "/sln/src/UpdateClientService.API/Services/FileSets/FileSetCleanupJob.cs");
		}
	}
}
