using System.Collections.Generic;
using System.Linq;
using System.Text;
using Redbox.Core;
using Redbox.UpdateManager.ComponentModel;
using Redbox.UpdateManager.DownloadService;
using Redbox.UpdateManager.FileCache;

namespace Redbox.UpdateManager.FileSets;

internal class FileSetRevisionDownloader
{
	private IFileCacheService _fileCacheService;

	private IDownloadService _downloadService;

	private IDownloader _downloader;

	private long _fileSetId;

	private long _revisionId;

	private long _patchRevisionId;

	private string _hash;

	private string _downloadUrl;

	private string _path;

	private string _contentHash;

	private DownloadPriority _downloadPriority;

	internal IFileCacheService FileCacheService
	{
		get
		{
			if (_fileCacheService == null)
			{
				_fileCacheService = ServiceLocator.Instance.GetService<IFileCacheService>();
			}
			return _fileCacheService;
		}
	}

	public IDownloadService DownloadService
	{
		get
		{
			if (_downloadService == null)
			{
				_downloadService = ServiceLocator.Instance.GetService<IDownloadService>();
			}
			return _downloadService;
		}
	}

	internal IDownloader Downloader => _downloader;

	internal FileSetRevisionDownloader(List<IDownloader> downloads, long fileSetId, long revisionId, long patchRevisionId, string hash, string downloadUrl, string path, string contentHash, DownloadPriority priority)
	{
		_fileSetId = fileSetId;
		_revisionId = revisionId;
		_patchRevisionId = patchRevisionId;
		_hash = hash;
		_downloadUrl = downloadUrl;
		_path = path;
		_contentHash = contentHash;
		_downloadPriority = priority;
		string key = GetRevisionKey();
		_downloader = downloads.Where((IDownloader d) => d.DownloadData.Key == key).FirstOrDefault();
	}

	internal bool Exists()
	{
		return FileCacheService.RevisionExists(_fileSetId, _revisionId, _patchRevisionId);
	}

	internal bool DownloadExists()
	{
		return _downloader != null;
	}

	internal bool IsDownloadError()
	{
		if (_downloader == null)
		{
			return false;
		}
		return _downloader.DownloadData.DownloadState == DownloadState.Error;
	}

	internal bool IsDownloadComplete()
	{
		if (Exists())
		{
			return true;
		}
		if (_downloader == null)
		{
			return false;
		}
		return _downloader.DownloadData.DownloadState == DownloadState.PostDownload;
	}

	internal IDownloader AddDownload()
	{
		if (AddDownload(GetRevisionKey(), _hash, GetRevisionUrl(), out var download).ContainsError())
		{
			return null;
		}
		_downloader = download;
		return _downloader;
	}

	private ErrorList AddDownload(string key, string hash, string url, out IDownloader download)
	{
		Redbox.UpdateManager.DownloadService.DownloadPriority downloadPriority = (Redbox.UpdateManager.DownloadService.DownloadPriority)_downloadPriority;
		return DownloadService.AddDownload(key, hash, url, downloadPriority, out download);
	}

	internal bool CompleteDownload()
	{
		ErrorList errorList = new ZipDownloadHelper(_downloader.DownloadData.Path, _fileSetId, _revisionId, _patchRevisionId).Extract();
		if (!errorList.ContainsError())
		{
			_downloader.Complete();
		}
		return !errorList.ContainsError();
	}

	internal ClientFileSetRevision GetRevision()
	{
		if (!FileCacheService.GetRevision(_fileSetId, _revisionId, _patchRevisionId, out var data))
		{
			return null;
		}
		return Encoding.ASCII.GetString(data).ToObject<ClientFileSetRevision>();
	}

	private string GetRevisionKey()
	{
		return $"{1},{_fileSetId},{_revisionId}-{_patchRevisionId}";
	}

	private string GetRevisionUrl()
	{
		return $"{_downloadUrl}/{_path}";
	}
}
