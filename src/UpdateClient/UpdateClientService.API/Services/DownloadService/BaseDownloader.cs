using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Redbox.NetCore.Logging.Extensions;
using UpdateClientService.API.App;

namespace UpdateClientService.API.Services.DownloadService;

public class BaseDownloader : IDownloader, IPersistentData
{
	private readonly IPersistentDataCacheService _persistentDataCacheService;

	private readonly ILogger<BaseDownloader> _logger;

	public BaseDownloader(IPersistentDataCacheService cache, ILogger<BaseDownloader> logger)
	{
		_logger = logger;
		_persistentDataCacheService = cache;
	}

	public virtual async Task<bool> ProcessDownload(DownloadData downloadData)
	{
		return true;
	}

	public virtual async Task<bool> SaveDownload(DownloadData downloadData)
	{
		bool flag;
		try
		{
			flag = await _persistentDataCacheService.Write(downloadData, downloadData.FileName, UpdateClientServiceConstants.DownloadDataFolder);
			if (!flag)
			{
				_logger.LogErrorWithSource("Unable to save DownloadData " + downloadData?.ToJson(), "SaveDownload", "/sln/src/UpdateClientService.API/Services/DownloadService/BaseDownloader.cs");
			}
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "Exception while saving DownloadData " + downloadData?.ToJson(), "SaveDownload", "/sln/src/UpdateClientService.API/Services/DownloadService/BaseDownloader.cs");
			flag = false;
		}
		return flag;
	}

	public virtual async Task<bool> DeleteDownload(DownloadData downloadData)
	{
		bool flag;
		try
		{
			flag = await _persistentDataCacheService.Delete(downloadData.FileName, UpdateClientServiceConstants.DownloadDataFolder);
			if (!flag)
			{
				_logger.LogErrorWithSource("Unable to delete DownloadData with key: " + downloadData?.Key, "DeleteDownload", "/sln/src/UpdateClientService.API/Services/DownloadService/BaseDownloader.cs");
			}
		}
		catch (Exception ex)
		{
			flag = false;
			_logger.LogErrorWithSource(ex, "Exception while deleting DownloadData with key " + downloadData?.Key, "DeleteDownload", "/sln/src/UpdateClientService.API/Services/DownloadService/BaseDownloader.cs");
		}
		return flag;
	}

	public virtual async Task<bool> Complete(DownloadData downloadData)
	{
		bool success = true;
		try
		{
			downloadData.DownloadState = DownloadState.Complete;
			_logger.LogInfoWithSource($"Setting DownloadState = {downloadData.DownloadState} for DownloadData {downloadData.FileName}", "Complete", "/sln/src/UpdateClientService.API/Services/DownloadService/BaseDownloader.cs");
			await SaveDownload(downloadData);
		}
		catch (Exception ex)
		{
			success = false;
			_logger.LogErrorWithSource(ex, "Exception while completing DownloadData with key: " + downloadData.Key, "Complete", "/sln/src/UpdateClientService.API/Services/DownloadService/BaseDownloader.cs");
		}
		return success;
	}

	public virtual bool Cleanup(DownloadDataList downloadDataList)
	{
		return true;
	}
}
