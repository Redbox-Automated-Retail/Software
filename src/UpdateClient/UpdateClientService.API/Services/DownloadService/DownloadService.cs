using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Coravel.Invocable;
using Microsoft.Extensions.Logging;
using Redbox.NetCore.Logging.Extensions;
using UpdateClientService.API.App;
using UpdateClientService.API.Services.DownloadService.Responses;
using UpdateClientService.API.Services.IoT.Commands;
using UpdateClientService.API.Services.IoT.IoTCommand;

namespace UpdateClientService.API.Services.DownloadService;

public class DownloadService : IDownloadService, IInvocable
{
	private readonly ILogger<DownloadService> _logger;

	private readonly IPersistentDataCacheService _cache;

	private readonly IDownloader _downloader;

	private readonly IIoTCommandClient _iotCommandClient;

	private readonly TimeSpan _proxiedS3UrlRequestTimeout = TimeSpan.FromSeconds(60.0);

	public const string DownloadExtension = ".dldat";

	private static SemaphoreSlim _lock => new SemaphoreSlim(1, 1);

	public DownloadService(ILogger<DownloadService> logger, IPersistentDataCacheService cache, IDownloader downloader, IIoTCommandClient iotCommandClient)
	{
		_logger = logger;
		_cache = cache;
		_downloader = downloader;
		_iotCommandClient = iotCommandClient;
	}

	public async Task Invoke()
	{
		try
		{
			if (!(await _lock.WaitAsync(0)))
			{
				_logger.LogWarningWithSource("DownloadService is already being invoked. Skipping...", "Invoke", "/sln/src/UpdateClientService.API/Services/DownloadService/DownloadService.cs");
				return;
			}
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "Could not acquire lock", "Invoke", "/sln/src/UpdateClientService.API/Services/DownloadService/DownloadService.cs");
			return;
		}
		try
		{
			await ProcessDownloads();
		}
		catch (Exception ex2)
		{
			_logger.LogErrorWithSource(ex2, "An unhandled exception occurred.", "Invoke", "/sln/src/UpdateClientService.API/Services/DownloadService/DownloadService.cs");
		}
		finally
		{
			SemaphoreSlim semaphoreSlim = _lock;
			if (semaphoreSlim != null && semaphoreSlim.CurrentCount == 0)
			{
				_lock.Release();
			}
		}
	}

	public async Task<GetDownloadStatusesResponse> GetDownloadsResponse(string pattern = null)
	{
		try
		{
			GetDownloadStatusesResponse getDownloadStatusesResponse = new GetDownloadStatusesResponse();
			GetDownloadStatusesResponse getDownloadStatusesResponse2 = getDownloadStatusesResponse;
			getDownloadStatusesResponse2.Statuses = await GetDownloads(null, pattern);
			return getDownloadStatusesResponse;
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "Exception while getting DownloadDatas for pattern " + pattern + ")", "GetDownloadsResponse", "/sln/src/UpdateClientService.API/Services/DownloadService/DownloadService.cs");
			return new GetDownloadStatusesResponse
			{
				StatusCode = HttpStatusCode.InternalServerError
			};
		}
	}

	public async Task<GetDownloadStatusResponse> GetDownloadResponse(string key)
	{
		try
		{
			DownloadData downloadData = (await GetDownloads())?.FirstOrDefault((DownloadData x) => x.Key == key);
			return new GetDownloadStatusResponse
			{
				Status = downloadData,
				StatusCode = ((downloadData != null) ? HttpStatusCode.OK : HttpStatusCode.NotFound)
			};
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "Exception while getting DownloadData for key " + key + ")", "GetDownloadResponse", "/sln/src/UpdateClientService.API/Services/DownloadService/DownloadService.cs");
			return new GetDownloadStatusResponse
			{
				StatusCode = HttpStatusCode.InternalServerError
			};
		}
	}

	public async Task<DownloadDataList> GetDownloads(string pattern = null)
	{
		return await GetDownloads(null, pattern);
	}

	public async Task<DownloadDataList> GetDownloads(Regex pattern)
	{
		return await GetDownloads(pattern, null);
	}

	private async Task<DownloadDataList> GetDownloads(Regex regexPattern, string stringPattern)
	{
		DownloadDataList result = new DownloadDataList();
		try
		{
			List<PersistentDataWrapper<DownloadData>> source = ((regexPattern == null) ? (await _cache.ReadLike<DownloadData>(string.IsNullOrWhiteSpace(stringPattern) ? DownloadDataConstants.DownloadExtension : stringPattern, UpdateClientServiceConstants.DownloadDataFolder, useCache: false)) : (await _cache.ReadLike<DownloadData>(regexPattern, UpdateClientServiceConstants.DownloadDataFolder, useCache: false)));
			result.AddRange((from d in source
				where d?.Data != null
				select d.Data).ToList());
		}
		catch (Exception ex)
		{
			_logger.LogCriticalWithSource(ex, "An unhandled exception occurred", "GetDownloads", "/sln/src/UpdateClientService.API/Services/DownloadService/DownloadService.cs");
		}
		return result;
	}

	public async Task<GetFileResponse> AddDownload(string hash, string url, DownloadPriority priority, bool completeOnFinish = false)
	{
		GetFileResponse response = new GetFileResponse();
		try
		{
			string key = Guid.NewGuid().ToString();
			if (!(await AddDownload(key, hash, url, priority, completeOnFinish)))
			{
				response.StatusCode = HttpStatusCode.InternalServerError;
			}
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "Exception while adding download for hash " + hash + ", url " + url, "AddDownload", "/sln/src/UpdateClientService.API/Services/DownloadService/DownloadService.cs");
		}
		return response;
	}

	public async Task<bool> AddDownload(string key, string hash, string url, DownloadPriority priority, bool completeOnFinish = false)
	{
		(bool, DownloadData) tuple = await AddRetrieveDownload(key, hash, url, priority, completeOnFinish);
		var (result, _) = tuple;
		_ = tuple.Item2;
		return result;
	}

	public async Task<(bool success, DownloadData downloadData)> AddRetrieveDownload(string key, string hash, string url, DownloadPriority priority, bool completeOnFinish)
	{
		DownloadData downloadData = null;
		_logger.LogInfoWithSource($"Adding download key: {key} hash: {hash} url: {url} priority: {priority}", "AddRetrieveDownload", "/sln/src/UpdateClientService.API/Services/DownloadService/DownloadService.cs");
		bool item;
		try
		{
			downloadData = DownloadData.Initialize(key, hash, url, priority, completeOnFinish);
			item = await _downloader.SaveDownload(downloadData);
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, $"Exception while adding download for {key} hash: {hash} url: {url} priority: {priority}", "AddRetrieveDownload", "/sln/src/UpdateClientService.API/Services/DownloadService/DownloadService.cs");
			item = false;
		}
		return (success: item, downloadData: downloadData);
	}

	public async Task<DeleteDownloadResponse> CancelDownload(string key)
	{
		DeleteDownloadResponse response = new DeleteDownloadResponse();
		_logger.LogInfoWithSource("Deleting DownloadData with key: " + key, "CancelDownload", "/sln/src/UpdateClientService.API/Services/DownloadService/DownloadService.cs");
		try
		{
			DownloadData downloadData = await GetDownloadData(key);
			bool flag = downloadData != null;
			bool flag2 = flag;
			if (flag2)
			{
				flag2 = !(await _downloader.DeleteDownload(downloadData));
			}
			if (flag2)
			{
				response.StatusCode = HttpStatusCode.InternalServerError;
			}
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "Exception while canceling download with key " + key, "CancelDownload", "/sln/src/UpdateClientService.API/Services/DownloadService/DownloadService.cs");
			response.StatusCode = HttpStatusCode.InternalServerError;
		}
		return response;
	}

	public async Task<CompleteDownloadResponse> CompleteDownload(string key)
	{
		CompleteDownloadResponse response = new CompleteDownloadResponse();
		_logger.LogInfoWithSource("Completing download matching key: " + key, "CompleteDownload", "/sln/src/UpdateClientService.API/Services/DownloadService/DownloadService.cs");
		try
		{
			DownloadData downloadData = await GetDownloadData(key);
			if (downloadData != null)
			{
				if (!(await _downloader.Complete(downloadData)))
				{
					response.StatusCode = HttpStatusCode.InternalServerError;
				}
			}
			else
			{
				_logger.LogErrorWithSource("No download found matching key: " + key, "CompleteDownload", "/sln/src/UpdateClientService.API/Services/DownloadService/DownloadService.cs");
				response.StatusCode = HttpStatusCode.InternalServerError;
			}
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "Exception while completing Download with key " + key, "CompleteDownload", "/sln/src/UpdateClientService.API/Services/DownloadService/DownloadService.cs");
			response.StatusCode = HttpStatusCode.InternalServerError;
		}
		return response;
	}

	public async Task<string> GetProxiedS3Url(string key, DownloadPathType type, bool isHead = false)
	{
		PerformIoTCommandParameters performIoTCommandParameters = new PerformIoTCommandParameters
		{
			RequestTimeout = _proxiedS3UrlRequestTimeout,
			WaitForResponse = true
		};
		if (!Debugger.IsAttached)
		{
			performIoTCommandParameters.IoTTopic = "$aws/rules/kioskrestcall";
		}
		string text = await _iotCommandClient.PerformIoTCommandWithStringResult(new IoTCommandModel
		{
			Version = 2,
			Command = CommandEnum.GetPresignedS3Url,
			Payload = new PresignedS3UrlRequest
			{
				Key = key,
				Type = type,
				IsHead = isHead
			}
		}, performIoTCommandParameters);
		if (string.IsNullOrEmpty(text) || !Uri.TryCreate(text, UriKind.Absolute, out Uri _))
		{
			_logger.LogErrorWithSource("Url '" + text + "' is not a valid Url", "GetProxiedS3Url", "/sln/src/UpdateClientService.API/Services/DownloadService/DownloadService.cs");
			text = null;
		}
		return text;
	}

	private async Task<DownloadData> GetDownloadData(string key)
	{
		return (await GetDownloads()).Where((DownloadData d) => d.Key == key).FirstOrDefault();
	}

	private async Task<bool> ProcessDownloads()
	{
		bool result = true;
		try
		{
			DownloadDataList downloadDataList = await GetDownloads();
			if (downloadDataList != null && downloadDataList.Count > 0)
			{
				_logger.LogInfoWithSource($"Start processing {downloadDataList.Count} downloads", "ProcessDownloads", "/sln/src/UpdateClientService.API/Services/DownloadService/DownloadService.cs");
			}
			foreach (DownloadData item in downloadDataList)
			{
				bool flag = result;
				result = flag & await _downloader.ProcessDownload(item);
			}
			result &= _downloader.Cleanup(downloadDataList);
			if (downloadDataList != null && downloadDataList.Count > 0)
			{
				_logger.LogInfoWithSource("Finished executing", "ProcessDownloads", "/sln/src/UpdateClientService.API/Services/DownloadService/DownloadService.cs");
			}
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "Exception while processing downloads", "ProcessDownloads", "/sln/src/UpdateClientService.API/Services/DownloadService/DownloadService.cs");
			result = false;
		}
		return result;
	}
}
