using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using UpdateClientService.API.Services.DownloadService;
using UpdateClientService.API.Services.FileCache;

namespace UpdateClientService.API.Services.FileSets;

public class FileSetRevisionDownloader : IFileSetRevisionDownloader
{
	private IFileCacheService _fileCacheService;

	private IDownloadService _downloadService;

	private readonly IZipDownloadHelper _zipHelper;

	private readonly AppSettings _settings;

	private readonly IDownloader _downloader;

	public FileSetRevisionDownloader(IFileCacheService fileCacheService, IDownloadService downloadService, IZipDownloadHelper zipHelper, IDownloader downloaderInterface, IOptionsMonitor<AppSettings> settings)
	{
		_fileCacheService = fileCacheService;
		_downloadService = downloadService;
		_zipHelper = zipHelper;
		_downloader = downloaderInterface;
		_settings = settings.CurrentValue;
	}

	public bool DoesRevisionExist(RevisionChangeSetKey revisionChangeSetKey)
	{
		return _fileCacheService.DoesRevisionExist(revisionChangeSetKey);
	}

	public bool IsDownloadError(DownloadData downloadData)
	{
		if (downloadData == null)
		{
			return false;
		}
		return downloadData.DownloadState == DownloadState.Error;
	}

	public bool IsDownloadComplete(RevisionChangeSetKey revisionChangeSetKey, DownloadData downloadData)
	{
		if (!DoesRevisionExist(revisionChangeSetKey))
		{
			if (downloadData == null)
			{
				return false;
			}
			return downloadData.DownloadState == DownloadState.PostDownload;
		}
		return true;
	}

	public async Task<DownloadData> AddDownload(RevisionChangeSetKey revisionChangeSetKey, string hash, string path, DownloadPriority downloadPriority)
	{
		var (flag, downloadData) = await AddDownload(DownloadData.GetRevisionKey(revisionChangeSetKey), hash, GetRevisionUrl(path), downloadPriority);
		return flag ? downloadData : null;
	}

	private async Task<(bool success, DownloadData downloadData)> AddDownload(string key, string hash, string url, DownloadPriority downloadPriority)
	{
		return await _downloadService.AddRetrieveDownload(key, hash, url, downloadPriority, completeOnFinish: false);
	}

	public bool CompleteDownload(RevisionChangeSetKey revisionChangeSetKey, DownloadData downloadData)
	{
		bool flag = _zipHelper.Extract(downloadData.Path, revisionChangeSetKey);
		if (flag)
		{
			_downloader.Complete(downloadData);
		}
		return flag;
	}

	private string GetRevisionUrl(string path)
	{
		if (!string.IsNullOrWhiteSpace(path))
		{
			string text = Uri.EscapeDataString(path);
			return new Uri(string.Format("{0}/{1}/{2}/{3}", _settings.BaseServiceUrl, "api/downloads/s3/proxy", text, DownloadPathType.None)).ToString();
		}
		return string.Empty;
	}
}
