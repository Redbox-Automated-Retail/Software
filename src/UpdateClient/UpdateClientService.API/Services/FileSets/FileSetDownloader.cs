using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Redbox.NetCore.Logging.Extensions;
using UpdateClientService.API.Services.DownloadService;
using UpdateClientService.API.Services.FileCache;

namespace UpdateClientService.API.Services.FileSets;

public class FileSetDownloader : IFileSetDownloader
{
	private class DownloadMethodSize
	{
		public DownloadMethod DownloadMethod { get; set; }

		public long Size { get; set; }

		public int NumberOfDownloads { get; set; }
	}

	private readonly IFileCacheService _fileCacheService;

	private readonly IDownloadService _downloadService;

	private readonly IZipDownloadHelper _zipHelper;

	private readonly IDownloader _downloader;

	private readonly AppSettings _settings;

	private readonly ILogger<FileSetDownloader> _logger;

	public FileSetDownloader(IFileCacheService fileCacheService, IDownloadService downloadService, IZipDownloadHelper zipHelper, IDownloader downloaderInterface, IOptionsMonitor<AppSettings> settings, ILogger<FileSetDownloader> logger)
	{
		_fileCacheService = fileCacheService;
		_downloadService = downloadService;
		_zipHelper = zipHelper;
		_downloader = downloaderInterface;
		_settings = settings.CurrentValue;
		_logger = logger;
	}

	public bool IsDownloaded(ClientFileSetRevision clientFileSetRevision)
	{
		List<FileSetFileInfo> fileSetInfoList = GetFileSetInfoList(clientFileSetRevision);
		return GetDownloadMethod(clientFileSetRevision, fileSetInfoList) == DownloadMethod.None;
	}

	public async Task<bool> DownloadFileSet(ClientFileSetRevision revision, DownloadDataList downloadDataList, DownloadPriority priority)
	{
		bool result = true;
		try
		{
			List<FileSetFileInfo> fileSetInfoList = GetFileSetInfoList(revision);
			DownloadMethod downloadMethod = GetDownloadMethod(revision, fileSetInfoList);
			_logger.LogInfoWithSource($"Download method is {downloadMethod} for {revision.IdentifyingText()}", "DownloadFileSet", "/sln/src/UpdateClientService.API/Services/FileSets/FileSetDownloader.cs");
			switch (downloadMethod)
			{
			case DownloadMethod.FileSet:
				result = await DownloadByFileSet(revision, downloadDataList, priority);
				break;
			case DownloadMethod.PatchSet:
				result = await DownloadByPatchSet(revision, fileSetInfoList, downloadDataList, priority);
				break;
			case DownloadMethod.Files:
				result = await DownloadByFiles(fileSetInfoList, downloadDataList, priority);
				break;
			case DownloadMethod.Patches:
				result = await DownloadByPatches(fileSetInfoList, downloadDataList, priority);
				break;
			}
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "Exception while downloading file set " + revision?.IdentifyingText(), "DownloadFileSet", "/sln/src/UpdateClientService.API/Services/FileSets/FileSetDownloader.cs");
			result = false;
		}
		return result;
	}

	public bool CompleteDownloads(ClientFileSetRevision clientFileSetRevision, DownloadDataList downloadDataList)
	{
		bool result = true;
		foreach (DownloadData item in downloadDataList.Where((DownloadData x) => x.DownloadState == DownloadState.PostDownload))
		{
			if (_zipHelper.Extract(item.Path, clientFileSetRevision))
			{
				_downloader.Complete(item);
			}
			else
			{
				result = false;
			}
		}
		return result;
	}

	private List<FileSetFileInfo> GetFileSetInfoList(ClientFileSetRevision clientFileSetRevision)
	{
		List<FileSetFileInfo> list = new List<FileSetFileInfo>();
		foreach (ClientFileSetFile eachClientFileSetFile in clientFileSetRevision.Files)
		{
			FileSetFileInfo fileSetFileInfo = new FileSetFileInfo
			{
				FileId = eachClientFileSetFile.FileId,
				FileRevisionId = eachClientFileSetFile.FileRevisionId,
				Key = GetFileKey(clientFileSetRevision.FileSetId, eachClientFileSetFile.FileId, eachClientFileSetFile.FileRevisionId),
				FileSize = eachClientFileSetFile.FileSize,
				PatchSize = 0L,
				FileHash = eachClientFileSetFile.FileHash
			};
			fileSetFileInfo.Exists = _fileCacheService.DoesFileExist(clientFileSetRevision.FileSetId, eachClientFileSetFile.FileId, eachClientFileSetFile.FileRevisionId);
			if (fileSetFileInfo.Exists && !_fileCacheService.IsFileHashValid(clientFileSetRevision.FileSetId, eachClientFileSetFile.FileId, eachClientFileSetFile.FileRevisionId, eachClientFileSetFile.ContentHash))
			{
				_logger.LogWarning($"Invalid Hash for file with FileSetId {clientFileSetRevision.FileSetId}, FileId {eachClientFileSetFile.FileId}, FileRevisionId {eachClientFileSetFile.FileRevisionId}");
				fileSetFileInfo.Exists = false;
				_fileCacheService.DeleteFile(clientFileSetRevision.FileSetId, eachClientFileSetFile.FileId, eachClientFileSetFile.FileRevisionId, force: true);
			}
			fileSetFileInfo.Url = GetFileUrl(eachClientFileSetFile, clientFileSetRevision.FileSetId, eachClientFileSetFile.FileRevisionId, 0L);
			if (!fileSetFileInfo.Exists)
			{
				ClientPatchFileSetFile clientPatchFileSetFile = clientFileSetRevision.PatchFiles.FirstOrDefault((ClientPatchFileSetFile pf) => pf.FileId == eachClientFileSetFile.FileId);
				if (clientPatchFileSetFile != null)
				{
					string fileKey = GetFileKey(clientFileSetRevision.FileSetId, eachClientFileSetFile.FileId, clientPatchFileSetFile.PatchFileRevisionId);
					bool flag = _fileCacheService.DoesFileExist(clientFileSetRevision.FileSetId, eachClientFileSetFile.FileId, clientPatchFileSetFile.PatchFileRevisionId);
					if (flag && !_fileCacheService.IsFileHashValid(clientFileSetRevision.FileSetId, eachClientFileSetFile.FileId, clientPatchFileSetFile.PatchFileRevisionId, clientPatchFileSetFile.ContentHash))
					{
						_logger.LogWarning($"Invalid Hash for file with FileSetId {clientFileSetRevision.FileSetId}, FileId {eachClientFileSetFile.FileId}, FileRevisionId {clientPatchFileSetFile.PatchFileRevisionId}");
						flag = false;
						_fileCacheService.DeleteFile(clientFileSetRevision.FileSetId, eachClientFileSetFile.FileId, clientPatchFileSetFile.PatchFileRevisionId, force: true);
					}
					if (flag)
					{
						fileSetFileInfo.PatchSize = clientPatchFileSetFile.FileSize;
						fileSetFileInfo.PatchUrl = GetFileUrl(eachClientFileSetFile, clientFileSetRevision.FileSetId, eachClientFileSetFile.FileRevisionId, clientPatchFileSetFile.PatchFileRevisionId);
						fileSetFileInfo.PatchKey = fileKey;
						fileSetFileInfo.PatchHash = clientPatchFileSetFile.FileHash;
					}
				}
			}
			list.Add(fileSetFileInfo);
		}
		return list;
	}

	private DownloadMethod GetDownloadMethod(ClientFileSetRevision clientFileSetRevision, List<FileSetFileInfo> fileSetInfoList)
	{
		long num = fileSetInfoList.Sum((FileSetFileInfo x) => (!x.Exists) ? x.FileSize : 0);
		if (num == 0L)
		{
			return DownloadMethod.None;
		}
		int numberOfDownloads = fileSetInfoList.Count((FileSetFileInfo x) => !x.Exists);
		List<DownloadMethodSize> list = new List<DownloadMethodSize>();
		list.Add(new DownloadMethodSize
		{
			DownloadMethod = DownloadMethod.Files,
			Size = num,
			NumberOfDownloads = numberOfDownloads
		});
		list.Add(new DownloadMethodSize
		{
			DownloadMethod = DownloadMethod.FileSet,
			Size = clientFileSetRevision.SetFileSize,
			NumberOfDownloads = 1
		});
		if (clientFileSetRevision.PatchSetFileSize > 0)
		{
			long size = fileSetInfoList.Sum((FileSetFileInfo x) => (!x.Exists) ? ((x.PatchSize <= 0) ? x.FileSize : x.PatchSize) : 0);
			list.Add(new DownloadMethodSize
			{
				DownloadMethod = DownloadMethod.Patches,
				Size = size,
				NumberOfDownloads = numberOfDownloads
			});
			int num2 = fileSetInfoList.Count((FileSetFileInfo x) => !x.Exists || x.PatchSize == 0);
			long num3 = fileSetInfoList.Sum((FileSetFileInfo x) => (!x.Exists && x.PatchSize <= 0) ? x.FileSize : 0);
			num3 += clientFileSetRevision.PatchSetFileSize;
			list.Add(new DownloadMethodSize
			{
				DownloadMethod = DownloadMethod.PatchSet,
				Size = num3,
				NumberOfDownloads = num2 + 1
			});
		}
		DownloadMethodSize downloadMethodSize = (from x in list
			orderby x.Size, x.NumberOfDownloads
			select x).FirstOrDefault();
		return downloadMethodSize.DownloadMethod;
	}

	private async Task<bool> DownloadByFileSet(ClientFileSetRevision clientFileSetRevision, DownloadDataList downloadDataList, DownloadPriority priority)
	{
		bool result = false;
		try
		{
			string key = GetRevisionSetKey(clientFileSetRevision);
			string fileSetUrl = GetFileSetUrl(clientFileSetRevision);
			if (!downloadDataList.ExistsByKey(key))
			{
				var (flag, item) = await AddDownload(key, clientFileSetRevision.SetFileHash, fileSetUrl, priority);
				if (flag)
				{
					downloadDataList.Add(item);
					result = true;
				}
				else
				{
					_logger.LogErrorWithSource("Unable to add DownloadData for " + key, "DownloadByFileSet", "/sln/src/UpdateClientService.API/Services/FileSets/FileSetDownloader.cs");
				}
			}
			else
			{
				result = true;
			}
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "Excepton while adding DownloadData by FileSet for " + clientFileSetRevision?.IdentifyingText(), "DownloadByFileSet", "/sln/src/UpdateClientService.API/Services/FileSets/FileSetDownloader.cs");
		}
		return result;
	}

	private async Task<bool> DownloadByPatchSet(ClientFileSetRevision clientFileSetRevision, List<FileSetFileInfo> fileSetInfoList, DownloadDataList downloadDataList, DownloadPriority priority)
	{
		bool result = false;
		try
		{
			string revisionPatchSetKey = GetRevisionPatchSetKey(clientFileSetRevision);
			string patchSetFileUrl = GetPatchSetFileUrl(clientFileSetRevision);
			if (!downloadDataList.ExistsByKey(revisionPatchSetKey))
			{
				var (flag, item) = await AddDownload(revisionPatchSetKey, clientFileSetRevision.PatchSetFileHash, patchSetFileUrl, priority);
				if (flag)
				{
					downloadDataList.Add(item);
					result = await AddDownloadsForNewFilesInPatchSet(fileSetInfoList, downloadDataList, priority);
				}
			}
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "Exception while adding DownloadData for Patch Set with " + clientFileSetRevision?.IdentifyingText(), "DownloadByPatchSet", "/sln/src/UpdateClientService.API/Services/FileSets/FileSetDownloader.cs");
		}
		return result;
	}

	private async Task<bool> AddDownloadsForNewFilesInPatchSet(List<FileSetFileInfo> fileSetInfoList, DownloadDataList downloadDataList, DownloadPriority priority)
	{
		bool result = true;
		foreach (FileSetFileInfo item2 in fileSetInfoList.Where((FileSetFileInfo x) => !x.Exists && x.PatchSize == 0))
		{
			if (!downloadDataList.ExistsByKey(item2.Key))
			{
				var (flag, item) = await AddDownload(item2.Key, item2.FileHash, item2.Url, priority);
				if (flag)
				{
					downloadDataList.Add(item);
				}
				else
				{
					result = false;
				}
			}
		}
		return result;
	}

	private async Task<bool> DownloadByPatches(List<FileSetFileInfo> fileSetInfoList, DownloadDataList downloadDataList, DownloadPriority priority)
	{
		bool result = true;
		try
		{
			foreach (FileSetFileInfo item2 in fileSetInfoList.Where((FileSetFileInfo x) => !x.Exists))
			{
				bool flag = item2.PatchSize > 0;
				string key = (flag ? item2.PatchKey : item2.Key);
				if (!downloadDataList.ExistsByKey(key))
				{
					string hash = (flag ? item2.PatchHash : item2.FileHash);
					string url = (flag ? item2.PatchUrl : item2.Url);
					var (flag2, item) = await AddDownload(key, hash, url, priority);
					if (flag2)
					{
						downloadDataList.Add(item);
					}
					else
					{
						result = false;
					}
				}
			}
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "Exception while adding DownloadData for Patch files", "DownloadByPatches", "/sln/src/UpdateClientService.API/Services/FileSets/FileSetDownloader.cs");
			result = false;
		}
		return result;
	}

	private async Task<bool> DownloadByFiles(List<FileSetFileInfo> fileSetInfoList, DownloadDataList downloadDataList, DownloadPriority downloadPriority)
	{
		bool result = true;
		try
		{
			foreach (FileSetFileInfo item2 in fileSetInfoList.Where((FileSetFileInfo x) => !x.Exists))
			{
				if (!downloadDataList.ExistsByKey(item2.Key))
				{
					var (flag, item) = await AddDownload(item2.Key, item2.FileHash, item2.Url, downloadPriority);
					if (flag)
					{
						downloadDataList.Add(item);
					}
					else
					{
						result = false;
					}
				}
			}
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "Exception while adding DownloadData for files", "DownloadByFiles", "/sln/src/UpdateClientService.API/Services/FileSets/FileSetDownloader.cs");
			result = false;
		}
		return result;
	}

	private async Task<(bool success, DownloadData downloadData)> AddDownload(string key, string hash, string url, DownloadPriority downloadPriority)
	{
		return await _downloadService.AddRetrieveDownload(key, hash, url, downloadPriority, completeOnFinish: false);
	}

	private string GetFileKey(long fileSetId, long fileId, long fileRevisionId)
	{
		return $"{2},{fileSetId},{fileId},{fileRevisionId}";
	}

	private string GetFileUrl(ClientFileSetFile file, long fileSetId, long fileRevisionId, long patchFileRevisionId = 0L)
	{
		string text = Uri.EscapeDataString($"filesets/{fileSetId}/File/{file.FileId}-{fileRevisionId}-{patchFileRevisionId}.zip");
		return new Uri(string.Format("{0}/{1}/{2}/{3}", _settings.BaseServiceUrl, "api/downloads/s3/proxy", text, DownloadPathType.None)).ToString();
	}

	private string GetFilePatchKey(long fileSetId, long fileId, long fileRevisionId, long patchFileRevisionId)
	{
		return $"{3},{fileSetId},{fileId},{fileRevisionId},{patchFileRevisionId}";
	}

	private string GetRevisionSetKey(ClientFileSetRevision revision)
	{
		return $"{4},{revision.FileSetId},{revision.RevisionId}";
	}

	private string GetRevisionPatchSetKey(ClientFileSetRevision revision)
	{
		return $"{5},{revision.FileSetId},{revision.RevisionId}";
	}

	private string GetPatchSetFileUrl(ClientFileSetRevision revision)
	{
		return GetProxiedS3Url(revision.PatchSetPath);
	}

	private string GetFileSetUrl(ClientFileSetRevision revision)
	{
		return GetProxiedS3Url(revision.SetPath);
	}

	private string GetProxiedS3Url(string path)
	{
		if (!string.IsNullOrWhiteSpace(path))
		{
			string text = Uri.EscapeDataString(path);
			return new Uri(string.Format("{0}/{1}/{2}/{3}", _settings.BaseServiceUrl, "api/downloads/s3/proxy", text, DownloadPathType.None)).ToString();
		}
		return string.Empty;
	}
}
