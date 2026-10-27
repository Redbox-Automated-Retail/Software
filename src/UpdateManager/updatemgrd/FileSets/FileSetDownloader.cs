using System;
using System.Collections.Generic;
using System.Linq;
using Redbox.Core;
using Redbox.UpdateManager.ComponentModel;
using Redbox.UpdateManager.DownloadService;
using Redbox.UpdateManager.FileCache;

namespace Redbox.UpdateManager.FileSets;

internal class FileSetDownloader
{
	private ClientFileSetRevision _revision;

	private List<IDownloader> _downloads;

	private string _downloadUrl;

	private DownloadPriority _downloadPriority;

	private List<FileSetFileInfo> _fileSetInfoList;

	private IFileCacheService _fileCacheService;

	private IDownloadService _downloadService;

	public ClientFileSetRevision Revision => _revision;

	internal List<FileSetFileInfo> FileSetInfoList
	{
		get
		{
			if (_fileSetInfoList == null)
			{
				UpdateFileSetInfoList();
			}
			return _fileSetInfoList;
		}
	}

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

	internal FileSetDownloader(ClientFileSetRevision revision, List<IDownloader> downloads, string downloadUrl, DownloadPriority downloadPriority)
	{
		_revision = revision;
		_downloads = downloads;
		_downloadUrl = downloadUrl;
		_downloadPriority = downloadPriority;
	}

	internal bool IsDownloading()
	{
		bool result = false;
		_downloads.ForEach(delegate(IDownloader _downloader)
		{
			if (_downloader.DownloadData.DownloadState == DownloadState.Downloading || _downloader.DownloadData.DownloadState == DownloadState.Error)
			{
				result = true;
			}
		});
		return result;
	}

	internal bool IsDownloaded()
	{
		return GetDownloadMethod() == DownloadMethod.None;
	}

	internal ErrorList DownloadFileSet()
	{
		ErrorList errorList = new ErrorList();
		try
		{
			switch (GetDownloadMethod())
			{
			case DownloadMethod.None:
				return errorList;
			case DownloadMethod.FileSet:
				errorList.AddRange(DownloadByFileSet());
				break;
			case DownloadMethod.PatchSet:
				errorList.AddRange(DownloadByPatchSet());
				break;
			case DownloadMethod.Files:
				errorList.AddRange(DownloadByFiles());
				break;
			case DownloadMethod.Patches:
				errorList.AddRange(DownloadByPatches());
				break;
			}
			return errorList;
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("FileSetDownloader", "An unhandled exception occurred in DownloadFileSet", e));
			return errorList;
		}
	}

	internal ErrorList CompleteDownloads()
	{
		ErrorList errors = new ErrorList();
		_downloads.ForEach(delegate(IDownloader _downloader)
		{
			if (_downloader.DownloadData.DownloadState == DownloadState.PostDownload)
			{
				ErrorList errorList = new ZipDownloadHelper(_downloader.DownloadData.Path, _revision.FileSetId, _revision.RevisionId, _revision.PatchRevisionId).Extract();
				if (!errorList.ContainsError())
				{
					_downloader.Complete();
				}
				errors.AddRange(errorList);
			}
		});
		return errors;
	}

	private void UpdateFileSetInfoList()
	{
		List<FileSetFileInfo> fileSetInfoList = new List<FileSetFileInfo>();
		_revision.Files.ForEach(delegate(ClientFileSetFile f)
		{
			FileSetFileInfo fileSetFileInfo = new FileSetFileInfo
			{
				FileId = f.FileId,
				FileRevisionId = f.FileRevisionId,
				Key = GetFileKey(f.FileId, f.FileRevisionId),
				FileSize = f.FileSize,
				PatchSize = 0L,
				FileHash = f.FileHash
			};
			fileSetFileInfo.Exists = FileCacheService.FileExists(_revision.FileSetId, f.FileId, f.FileRevisionId);
			fileSetFileInfo.Url = GetFileUrl(f);
			if (!fileSetFileInfo.Exists)
			{
				ClientPatchFileSetFile clientPatchFileSetFile = _revision.PatchFiles.FirstOrDefault((ClientPatchFileSetFile pf) => pf.FileId == f.FileId);
				if (clientPatchFileSetFile != null)
				{
					string fileKey = GetFileKey(f.FileId, clientPatchFileSetFile.PatchFileRevisionId);
					if (FileCacheService.FileExists(_revision.FileSetId, f.FileId, clientPatchFileSetFile.PatchFileRevisionId))
					{
						fileSetFileInfo.PatchSize = clientPatchFileSetFile.FileSize;
						fileSetFileInfo.PatchUrl = GetPatchFileUrl(clientPatchFileSetFile);
						fileSetFileInfo.PatchKey = fileKey;
						fileSetFileInfo.PatchHash = clientPatchFileSetFile.FileHash;
					}
				}
			}
			fileSetInfoList.Add(fileSetFileInfo);
		});
		_fileSetInfoList = fileSetInfoList;
	}

	private DownloadMethod GetDownloadMethod()
	{
		UpdateFileSetInfoList();
		long num = FileSetInfoList.Sum((FileSetFileInfo item) => item.Exists ? 0 : item.FileSize);
		long num2 = 0L;
		if (_revision.PatchSetFileSize > 0)
		{
			num2 = FileSetInfoList.Sum(delegate(FileSetFileInfo item)
			{
				if (item.Exists)
				{
					return 0L;
				}
				return (item.PatchSize > 0) ? item.PatchSize : item.FileSize;
			});
		}
		long num3 = 0L;
		if (_revision.PatchSetFileSize > 0)
		{
			num3 = FileSetInfoList.Sum(delegate(FileSetFileInfo item)
			{
				if (item.Exists)
				{
					return 0L;
				}
				return (item.PatchSize == 0L) ? item.FileSize : 0;
			});
			num3 += _revision.PatchSetFileSize;
		}
		if (num == 0L)
		{
			return DownloadMethod.None;
		}
		if (num3 > 0 && (num2 == 0L || num3 <= num2) && num3 < num && num3 < _revision.SetFileSize)
		{
			return DownloadMethod.PatchSet;
		}
		if (num2 > 0 && num2 < num && num2 < _revision.SetFileSize)
		{
			return DownloadMethod.Patches;
		}
		if (num < _revision.SetFileSize)
		{
			return DownloadMethod.Files;
		}
		return DownloadMethod.FileSet;
	}

	private ErrorList DownloadByFileSet()
	{
		ErrorList errorList = new ErrorList();
		try
		{
			string key = GetRevisionSetKey();
			string fileSetUrl = GetFileSetUrl();
			if (!_downloads.Any((IDownloader d) => d.DownloadData.Key == key))
			{
				errorList.AddRange(AddDownload(key, _revision.SetFileHash, fileSetUrl, out var download));
				if (!errorList.ContainsError())
				{
					_downloads.Add(download);
				}
			}
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("ChangeSetFile", "An unhandled exception occurred in DownloadByFileSet", e));
		}
		return errorList;
	}

	private ErrorList DownloadByPatchSet()
	{
		ErrorList errors = new ErrorList();
		try
		{
			string key = GetRevisionPatchSetKey();
			string patchSetFileUrl = GetPatchSetFileUrl();
			if (!_downloads.Any((IDownloader d) => d.DownloadData.Key == key))
			{
				errors.AddRange(AddDownload(key, _revision.PatchSetFileHash, patchSetFileUrl, out var download));
				if (errors.ContainsError())
				{
					return errors;
				}
				_downloads.Add(download);
			}
			FileSetInfoList.ForEach(delegate(FileSetFileInfo item)
			{
				if (!item.Exists && item.PatchSize == 0L && !_downloads.Any((IDownloader d) => d.DownloadData.Key == item.Key))
				{
					errors.AddRange(AddDownload(item.Key, item.FileHash, item.Url, out var download2));
					if (!errors.ContainsError())
					{
						_downloads.Add(download2);
					}
				}
			});
		}
		catch (Exception e)
		{
			errors.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("ChangeSetFile", "An unhandled exception occurred in DownloadByPatchSet", e));
		}
		return errors;
	}

	private ErrorList DownloadByPatches()
	{
		ErrorList errors = new ErrorList();
		try
		{
			FileSetInfoList.ForEach(delegate(FileSetFileInfo item)
			{
				if (!item.Exists)
				{
					if (item.PatchSize == 0L)
					{
						if (!_downloads.Any((IDownloader d) => d.DownloadData.Key == item.Key))
						{
							errors.AddRange(AddDownload(item.Key, item.FileHash, item.Url, out var download));
							if (!errors.ContainsError())
							{
								_downloads.Add(download);
							}
						}
					}
					else if (!_downloads.Any((IDownloader d) => d.DownloadData.Key == item.PatchKey))
					{
						errors.AddRange(AddDownload(item.PatchKey, item.PatchHash, item.PatchUrl, out var download2));
						if (!errors.ContainsError())
						{
							_downloads.Add(download2);
						}
					}
				}
			});
		}
		catch (Exception e)
		{
			errors.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("ChangeSetFile", "An unhandled exception occurred in DownloadByPatches", e));
		}
		return errors;
	}

	private ErrorList DownloadByFiles()
	{
		ErrorList errors = new ErrorList();
		try
		{
			FileSetInfoList.ForEach(delegate(FileSetFileInfo item)
			{
				if (!item.Exists && !_downloads.Any((IDownloader d) => d.DownloadData.Key == item.Key))
				{
					errors.AddRange(AddDownload(item.Key, item.FileHash, item.Url, out var download));
					if (!errors.ContainsError())
					{
						_downloads.Add(download);
					}
				}
			});
		}
		catch (Exception e)
		{
			errors.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("ChangeSetFile", "An unhandled exception occurred in DownloadByFiles", e));
		}
		return errors;
	}

	private ErrorList AddDownload(string key, string hash, string url, out IDownloader download)
	{
		Redbox.UpdateManager.DownloadService.DownloadPriority downloadPriority = (Redbox.UpdateManager.DownloadService.DownloadPriority)_downloadPriority;
		return DownloadService.AddDownload(key, hash, url, downloadPriority, out download);
	}

	private string GetFileKey(long fileId, long fileRevisionId)
	{
		return $"{2},{_revision.FileSetId},{fileId},{fileRevisionId}";
	}

	private string GetFileUrl(ClientFileSetFile file)
	{
		return $"{_downloadUrl}/{file.Path}";
	}

	private string GetPatchFileUrl(ClientPatchFileSetFile file)
	{
		return $"{_downloadUrl}/{file.Path}";
	}

	private string GetFilePatchKey(long fileId, long fileRevisionId, long patchFileRevisionId)
	{
		return $"{3},{_revision.FileSetId},{fileId},{fileRevisionId},{patchFileRevisionId}";
	}

	private string GetRevisionSetKey()
	{
		return $"{4},{_revision.FileSetId},{_revision.RevisionId}";
	}

	private string GetRevisionPatchSetKey()
	{
		return $"{5},{_revision.FileSetId},{_revision.RevisionId}";
	}

	private string GetPatchSetFileUrl()
	{
		return $"{_downloadUrl}/{_revision.PatchSetPath}";
	}

	private string GetFileSetUrl()
	{
		return $"{_downloadUrl}/{_revision.SetPath}";
	}
}
