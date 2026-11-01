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
		bool success = true;
		try
		{
			List<FileSetFileInfo> fileSetInfoList = GetFileSetInfoList(revision);
			DownloadMethod downloadMethod = GetDownloadMethod(revision, fileSetInfoList);
			_logger.LogInfoWithSource($"Download method is {downloadMethod} for {revision.IdentifyingText()}", "DownloadFileSet", "/sln/src/UpdateClientService.API/Services/FileSets/FileSetDownloader.cs");
			switch (downloadMethod)
			{
			case DownloadMethod.FileSet:
				success = await DownloadByFileSet(revision, downloadDataList, priority);
				break;
			case DownloadMethod.PatchSet:
				success = await DownloadByPatchSet(revision, fileSetInfoList, downloadDataList, priority);
				break;
			case DownloadMethod.Files:
				success = await DownloadByFiles(fileSetInfoList, downloadDataList, priority);
				break;
			case DownloadMethod.Patches:
				success = await DownloadByPatches(fileSetInfoList, downloadDataList, priority);
				break;
			}
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "Exception while downloading file set " + revision?.IdentifyingText(), "DownloadFileSet", "/sln/src/UpdateClientService.API/Services/FileSets/FileSetDownloader.cs");
			success = false;
		}
		return success;
	}

	public bool CompleteDownloads(ClientFileSetRevision clientFileSetRevision, DownloadDataList downloadDataList)
	{
		bool allExtracted = true;
		foreach (DownloadData downloadData in downloadDataList.Where((DownloadData x) => x.DownloadState == DownloadState.PostDownload))
		{
			if (_zipHelper.Extract(downloadData.Path, clientFileSetRevision))
			{
				_downloader.Complete(downloadData);
			}
			else
			{
				allExtracted = false;
			}
		}
		return allExtracted;
	}

	private List<FileSetFileInfo> GetFileSetInfoList(ClientFileSetRevision clientFileSetRevision)
	{
		List<FileSetFileInfo> fileSetInfoList = new List<FileSetFileInfo>();
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
					bool patchExists = _fileCacheService.DoesFileExist(clientFileSetRevision.FileSetId, eachClientFileSetFile.FileId, clientPatchFileSetFile.PatchFileRevisionId);
					if (patchExists && !_fileCacheService.IsFileHashValid(clientFileSetRevision.FileSetId, eachClientFileSetFile.FileId, clientPatchFileSetFile.PatchFileRevisionId, clientPatchFileSetFile.ContentHash))
					{
						_logger.LogWarning($"Invalid Hash for file with FileSetId {clientFileSetRevision.FileSetId}, FileId {eachClientFileSetFile.FileId}, FileRevisionId {clientPatchFileSetFile.PatchFileRevisionId}");
						patchExists = false;
						_fileCacheService.DeleteFile(clientFileSetRevision.FileSetId, eachClientFileSetFile.FileId, clientPatchFileSetFile.PatchFileRevisionId, force: true);
					}
					if (patchExists)
					{
						fileSetFileInfo.PatchSize = clientPatchFileSetFile.FileSize;
						fileSetFileInfo.PatchUrl = GetFileUrl(eachClientFileSetFile, clientFileSetRevision.FileSetId, eachClientFileSetFile.FileRevisionId, clientPatchFileSetFile.PatchFileRevisionId);
						fileSetFileInfo.PatchKey = fileKey;
						fileSetFileInfo.PatchHash = clientPatchFileSetFile.FileHash;
					}
				}
			}
			fileSetInfoList.Add(fileSetFileInfo);
		}
		return fileSetInfoList;
	}

	private DownloadMethod GetDownloadMethod(ClientFileSetRevision clientFileSetRevision, List<FileSetFileInfo> fileSetInfoList)
	{
		long filesSize = fileSetInfoList.Sum((FileSetFileInfo x) => (!x.Exists) ? x.FileSize : 0);
		if (filesSize == 0L)
		{
			return DownloadMethod.None;
		}
		int numberOfDownloads = fileSetInfoList.Count((FileSetFileInfo x) => !x.Exists);
		List<DownloadMethodSize> downloadMethodSizes = new List<DownloadMethodSize>();
		downloadMethodSizes.Add(new DownloadMethodSize
		{
			DownloadMethod = DownloadMethod.Files,
			Size = filesSize,
			NumberOfDownloads = numberOfDownloads
		});
		downloadMethodSizes.Add(new DownloadMethodSize
		{
			DownloadMethod = DownloadMethod.FileSet,
			Size = clientFileSetRevision.SetFileSize,
			NumberOfDownloads = 1
		});
		if (clientFileSetRevision.PatchSetFileSize > 0)
		{
			long patchesSize = fileSetInfoList.Sum((FileSetFileInfo x) => (!x.Exists) ? ((x.PatchSize <= 0) ? x.FileSize : x.PatchSize) : 0);
			downloadMethodSizes.Add(new DownloadMethodSize
			{
				DownloadMethod = DownloadMethod.Patches,
				Size = patchesSize,
				NumberOfDownloads = numberOfDownloads
			});
			int patchSetDownloads = fileSetInfoList.Count((FileSetFileInfo x) => !x.Exists || x.PatchSize == 0);
			long patchSetSize = fileSetInfoList.Sum((FileSetFileInfo x) => (!x.Exists && x.PatchSize <= 0) ? x.FileSize : 0);
			patchSetSize += clientFileSetRevision.PatchSetFileSize;
			downloadMethodSizes.Add(new DownloadMethodSize
			{
				DownloadMethod = DownloadMethod.PatchSet,
				Size = patchSetSize,
				NumberOfDownloads = patchSetDownloads + 1
			});
		}
		DownloadMethodSize downloadMethodSize = (from x in downloadMethodSizes
			orderby x.Size, x.NumberOfDownloads
			select x).FirstOrDefault();
		return downloadMethodSize.DownloadMethod;
	}

	private async Task<bool> DownloadByFileSet(ClientFileSetRevision clientFileSetRevision, DownloadDataList downloadDataList, DownloadPriority priority)
	{
		bool success = false;
		try
		{
			string key = GetRevisionSetKey(clientFileSetRevision);
			string fileSetUrl = GetFileSetUrl(clientFileSetRevision);
			if (!downloadDataList.ExistsByKey(key))
			{
				var (added, downloadData) = await AddDownload(key, clientFileSetRevision.SetFileHash, fileSetUrl, priority);
				if (added)
				{
					downloadDataList.Add(downloadData);
					success = true;
				}
				else
				{
					_logger.LogErrorWithSource("Unable to add DownloadData for " + key, "DownloadByFileSet", "/sln/src/UpdateClientService.API/Services/FileSets/FileSetDownloader.cs");
				}
			}
			else
			{
				success = true;
			}
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "Excepton while adding DownloadData by FileSet for " + clientFileSetRevision?.IdentifyingText(), "DownloadByFileSet", "/sln/src/UpdateClientService.API/Services/FileSets/FileSetDownloader.cs");
		}
		return success;
	}

	private async Task<bool> DownloadByPatchSet(ClientFileSetRevision clientFileSetRevision, List<FileSetFileInfo> fileSetInfoList, DownloadDataList downloadDataList, DownloadPriority priority)
	{
		bool success = false;
		try
		{
			string revisionPatchSetKey = GetRevisionPatchSetKey(clientFileSetRevision);
			string patchSetFileUrl = GetPatchSetFileUrl(clientFileSetRevision);
			if (!downloadDataList.ExistsByKey(revisionPatchSetKey))
			{
				var (added, downloadData) = await AddDownload(revisionPatchSetKey, clientFileSetRevision.PatchSetFileHash, patchSetFileUrl, priority);
				if (added)
				{
					downloadDataList.Add(downloadData);
					success = await AddDownloadsForNewFilesInPatchSet(fileSetInfoList, downloadDataList, priority);
				}
			}
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "Exception while adding DownloadData for Patch Set with " + clientFileSetRevision?.IdentifyingText(), "DownloadByPatchSet", "/sln/src/UpdateClientService.API/Services/FileSets/FileSetDownloader.cs");
		}
		return success;
	}

	private async Task<bool> AddDownloadsForNewFilesInPatchSet(List<FileSetFileInfo> fileSetInfoList, DownloadDataList downloadDataList, DownloadPriority priority)
	{
		bool success = true;
		foreach (FileSetFileInfo fileInfo in fileSetInfoList.Where((FileSetFileInfo x) => !x.Exists && x.PatchSize == 0))
		{
			if (!downloadDataList.ExistsByKey(fileInfo.Key))
			{
				var (added, downloadData) = await AddDownload(fileInfo.Key, fileInfo.FileHash, fileInfo.Url, priority);
				if (added)
				{
					downloadDataList.Add(downloadData);
				}
				else
				{
					success = false;
				}
			}
		}
		return success;
	}

	private async Task<bool> DownloadByPatches(List<FileSetFileInfo> fileSetInfoList, DownloadDataList downloadDataList, DownloadPriority priority)
	{
		bool success = true;
		try
		{
			foreach (FileSetFileInfo fileInfo in fileSetInfoList.Where((FileSetFileInfo x) => !x.Exists))
			{
				bool usePatch = fileInfo.PatchSize > 0;
				string key = (usePatch ? fileInfo.PatchKey : fileInfo.Key);
				if (!downloadDataList.ExistsByKey(key))
				{
					string hash = (usePatch ? fileInfo.PatchHash : fileInfo.FileHash);
					string url = (usePatch ? fileInfo.PatchUrl : fileInfo.Url);
					var (added, downloadData) = await AddDownload(key, hash, url, priority);
					if (added)
					{
						downloadDataList.Add(downloadData);
					}
					else
					{
						success = false;
					}
				}
			}
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "Exception while adding DownloadData for Patch files", "DownloadByPatches", "/sln/src/UpdateClientService.API/Services/FileSets/FileSetDownloader.cs");
			success = false;
		}
		return success;
	}

	private async Task<bool> DownloadByFiles(List<FileSetFileInfo> fileSetInfoList, DownloadDataList downloadDataList, DownloadPriority downloadPriority)
	{
		bool success = true;
		try
		{
			foreach (FileSetFileInfo fileInfo in fileSetInfoList.Where((FileSetFileInfo x) => !x.Exists))
			{
				if (!downloadDataList.ExistsByKey(fileInfo.Key))
				{
					var (added, downloadData) = await AddDownload(fileInfo.Key, fileInfo.FileHash, fileInfo.Url, downloadPriority);
					if (added)
					{
						downloadDataList.Add(downloadData);
					}
					else
					{
						success = false;
					}
				}
			}
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "Exception while adding DownloadData for files", "DownloadByFiles", "/sln/src/UpdateClientService.API/Services/FileSets/FileSetDownloader.cs");
			success = false;
		}
		return success;
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
		string escapedPath = Uri.EscapeDataString($"filesets/{fileSetId}/File/{file.FileId}-{fileRevisionId}-{patchFileRevisionId}.zip");
		return new Uri(string.Format("{0}/{1}/{2}/{3}", _settings.BaseServiceUrl, "api/downloads/s3/proxy", escapedPath, DownloadPathType.None)).ToString();
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
			string escapedPath = Uri.EscapeDataString(path);
			return new Uri(string.Format("{0}/{1}/{2}/{3}", _settings.BaseServiceUrl, "api/downloads/s3/proxy", escapedPath, DownloadPathType.None)).ToString();
		}
		return string.Empty;
	}
}
