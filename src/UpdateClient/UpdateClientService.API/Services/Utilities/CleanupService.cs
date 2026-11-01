using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Coravel.Invocable;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Redbox.NetCore.Logging.Extensions;

namespace UpdateClientService.API.Services.Utilities;

public class CleanupService : ICleanupService, IInvocable
{
	private readonly IOptions<AppSettings> _appSettings;

	private readonly ILogger<CleanupService> _logger;

	private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);

	public CleanupService(IOptions<AppSettings> appSettings, ILogger<CleanupService> logger)
	{
		_appSettings = appSettings;
		_logger = logger;
	}

	public async Task Invoke()
	{
		await DoWork();
	}

	private async Task DoWork()
	{
		await _lock.WaitAsync();
		_logger.LogInfoWithSource("Begin Cleanup of Old Files.", "DoWork", "/sln/src/UpdateClientService.API/Services/Utilities/CleanupService.cs");
		IEnumerable<CleanupFiles> cleanUpFiles = _appSettings.Value.CleanUpFiles;
		if (!cleanUpFiles.Any())
		{
			_logger.LogInfoWithSource("No Files Defined For Cleanup.", "DoWork", "/sln/src/UpdateClientService.API/Services/Utilities/CleanupService.cs");
			return;
		}
		int totalDeleted = 0;
		foreach (CleanupFiles item in cleanUpFiles)
		{
			try
			{
				if (!Directory.Exists(item.Path))
				{
					_logger.LogErrorWithSource($"Skipping files in folder {item.Path} with maxage {item.MaxAge} - Directory doesn't exist.", "DoWork", "/sln/src/UpdateClientService.API/Services/Utilities/CleanupService.cs");
					continue;
				}
				string[] files = Directory.GetFiles(item.Path);
				_logger.LogInfoWithSource($"Cleanup Folder {item.Path} with {files.Count()} files.", "DoWork", "/sln/src/UpdateClientService.API/Services/Utilities/CleanupService.cs");
				int deletedInFolder = 0;
				foreach (string file in files)
				{
					try
					{
						DateTime lastWriteTime = File.GetLastWriteTime(file);
						if (lastWriteTime.Date < DateTime.Now.Date.AddDays(-1 * item.MaxAge))
						{
							_logger.LogInfoWithSource($"Deleting File: {file} - it is {(DateTime.Now.Date - lastWriteTime.Date).Days} days old.", "DoWork", "/sln/src/UpdateClientService.API/Services/Utilities/CleanupService.cs");
							File.Delete(file);
							deletedInFolder++;
						}
						else
						{
							_logger.LogDebugWithSource($"Skipping File: {file} - it is {(DateTime.Now.Date - lastWriteTime.Date).Days} days old.", "DoWork", "/sln/src/UpdateClientService.API/Services/Utilities/CleanupService.cs");
						}
					}
					catch (Exception ex)
					{
						_logger.LogErrorWithSource(ex, $"Unhandled Exception when deleting files in folder {item.Path} with maxage {item.MaxAge}", "DoWork", "/sln/src/UpdateClientService.API/Services/Utilities/CleanupService.cs");
					}
				}
				totalDeleted += deletedInFolder;
				int remainingCount = Directory.GetFiles(item.Path).Count();
				_logger.LogInfoWithSource($"Cleanup Folder {item.Path} Completed with {remainingCount} files remaining. {deletedInFolder} files were deleted.", "DoWork", "/sln/src/UpdateClientService.API/Services/Utilities/CleanupService.cs");
			}
			catch (Exception ex2)
			{
				_logger.LogErrorWithSource(ex2, $"Unhandled Exception when deleting files in folder {item.Path} with maxage {item.MaxAge}", "DoWork", "/sln/src/UpdateClientService.API/Services/Utilities/CleanupService.cs");
			}
		}
		_logger.LogInfoWithSource($"End Cleanup of Old Files. {totalDeleted} files were deleted.", "DoWork", "/sln/src/UpdateClientService.API/Services/Utilities/CleanupService.cs");
	}
}
