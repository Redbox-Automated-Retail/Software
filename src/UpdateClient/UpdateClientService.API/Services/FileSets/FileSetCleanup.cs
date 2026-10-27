using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Redbox.NetCore.Logging.Extensions;
using UpdateClientService.API.Services.FileCache;

namespace UpdateClientService.API.Services.FileSets;

public class FileSetCleanup : IFileSetCleanup
{
	private class CleanupData
	{
		public List<StateFile> StateFiles { get; set; } = new List<StateFile>();

		public bool IsComplete { get; set; }
	}

	private class FileSetCleanupData
	{
		public long FileSetId { get; set; }

		public CleanupData CleanupData { get; set; }

		public StateFile StateFile { get; set; }

		public List<long> ActiveOrInProgressFileSetRevisionIds { get; set; } = new List<long>();

		public List<ClientFileSetRevision> UsedClientFileSetRevisions { get; set; } = new List<ClientFileSetRevision>();

		public List<FileSetFileInfo> FileSetFileInfos { get; set; } = new List<FileSetFileInfo>();

		public List<FileSetFileInfo> RequiredFileSetFileInfos { get; set; } = new List<FileSetFileInfo>();

		public int RetentionDays { get; set; }

		public int RetentionRevisions { get; set; }

		public int RetainedRevisions { get; set; }

		public bool IsComplete { get; set; }
	}

	private readonly IFileCacheService _fileCacheService;

	private readonly IFileSetService _fileSetService;

	private readonly IStateFileService _stateFileService;

	private readonly ILogger<FileSetCleanup> _logger;

	public FileSetCleanup(IFileCacheService fileCacheService, IFileSetService fileSetService, IStateFileService stateFileService, ILogger<FileSetCleanup> logger)
	{
		_fileCacheService = fileCacheService;
		_fileSetService = fileSetService;
		_stateFileService = stateFileService;
		_logger = logger;
	}

	public async Task Run()
	{
		_ = 1;
		try
		{
			CleanupData fileSetCleanupData = new CleanupData();
			await GetStateFiles(fileSetCleanupData);
			List<long> fileSetIds = _fileCacheService.GetFileSetIds();
			foreach (long item in fileSetIds)
			{
				await CleanupFileSet(fileSetCleanupData, item);
			}
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "Exception while cleaning up FileSets", "Run", "/sln/src/UpdateClientService.API/Services/FileSets/FileSetCleanup.cs");
		}
	}

	private async Task CleanupFileSet(CleanupData cleanupData, long fileSetId)
	{
		if (!cleanupData.IsComplete)
		{
			_logger.LogInfoWithSource($"FileSet Cleanup for FileSetId {fileSetId}", "CleanupFileSet", "/sln/src/UpdateClientService.API/Services/FileSets/FileSetCleanup.cs");
			FileSetCleanupData fileSetCleanupData = new FileSetCleanupData
			{
				CleanupData = cleanupData,
				FileSetId = fileSetId
			};
			GetActiveOrInProgressFileSetRevisionIds(fileSetCleanupData);
			await GetUsedClientFileSetRevisions(fileSetCleanupData);
			GetRetentionCriteria(fileSetCleanupData);
			await GetFileSetFileInfos(fileSetCleanupData);
			await CleanupFileSetRevisions(fileSetCleanupData, fileSetId);
			RemoveUnneededRevisionFiles(fileSetCleanupData);
		}
	}

	private async Task CleanupFileSetRevisions(FileSetCleanupData fileSetCleanupData, long fileSetId)
	{
		if (fileSetCleanupData.IsComplete)
		{
			return;
		}
		List<long> list = (from x in _fileCacheService.GetFileSetRevisionIds(fileSetId)
			orderby x descending
			select x).ToList();
		foreach (long item in list)
		{
			bool revisionRetained = IsRevisionRetained(fileSetCleanupData, item);
			List<string> clientFileSetRevisionFilePaths = _fileCacheService.GetClientFileSetRevisionFilePaths(fileSetId, item);
			foreach (string item2 in clientFileSetRevisionFilePaths)
			{
				ClientFileSetRevision clientFileSetRevision = await _fileCacheService.GetClientFileSetRevision(item2);
				if (clientFileSetRevision != null)
				{
					if (revisionRetained)
					{
						AddRequiredFiles(fileSetCleanupData, clientFileSetRevision);
					}
					else
					{
						_fileCacheService.DeleteRevision(clientFileSetRevision);
					}
				}
			}
			if (revisionRetained)
			{
				fileSetCleanupData.RetainedRevisions++;
			}
		}
	}

	private bool IsRevisionRetained(FileSetCleanupData fileSetCleanupData, long fileSetRevisionId)
	{
		if (fileSetCleanupData.ActiveOrInProgressFileSetRevisionIds.Any((long x) => x == fileSetRevisionId))
		{
			return true;
		}
		double totalDays = (DateTime.Now - _fileCacheService.GetRevisionCreationDate(fileSetCleanupData.FileSetId, fileSetRevisionId)).TotalDays;
		if (totalDays <= 1.0)
		{
			return true;
		}
		if (fileSetCleanupData.RetentionDays > 0 && totalDays > (double)fileSetCleanupData.RetentionDays)
		{
			_logger.LogInfoWithSource($"FileSetId {fileSetCleanupData.FileSetId} FileSetRevisionId {fileSetRevisionId} has exceeded the retention {fileSetCleanupData.RetentionDays} days and will be deleted", "IsRevisionRetained", "/sln/src/UpdateClientService.API/Services/FileSets/FileSetCleanup.cs");
			return false;
		}
		if (fileSetCleanupData.RetentionRevisions > 0 && fileSetCleanupData.RetainedRevisions >= fileSetCleanupData.RetentionRevisions)
		{
			_logger.LogInfoWithSource($"FileSetId {fileSetCleanupData.FileSetId} FileSetRevisionId {fileSetRevisionId} has exceeded the retention count {fileSetCleanupData.RetentionRevisions} and will be deleted", "IsRevisionRetained", "/sln/src/UpdateClientService.API/Services/FileSets/FileSetCleanup.cs");
			return false;
		}
		return true;
	}

	private async Task GetFileSetFileInfos(FileSetCleanupData fileSetCleanupData)
	{
		if (!fileSetCleanupData.IsComplete)
		{
			fileSetCleanupData.FileSetFileInfos = await _fileCacheService.GetFileInfos(fileSetCleanupData.FileSetId);
		}
	}

	private void GetRetentionCriteria(FileSetCleanupData fileSetCleanupData)
	{
		if (!fileSetCleanupData.IsComplete)
		{
			long maxFileSetRevisionId = fileSetCleanupData.UsedClientFileSetRevisions.Max((ClientFileSetRevision x) => x.RevisionId);
			ClientFileSetRevision clientFileSetRevision = fileSetCleanupData.UsedClientFileSetRevisions.FirstOrDefault((ClientFileSetRevision x) => x.RevisionId == maxFileSetRevisionId);
			fileSetCleanupData.RetentionDays = clientFileSetRevision.RetentionDays;
			fileSetCleanupData.RetentionRevisions = clientFileSetRevision.RetentionRevisions;
			if (fileSetCleanupData.RetentionDays == 0 && fileSetCleanupData.RetentionRevisions == 0)
			{
				fileSetCleanupData.IsComplete = true;
			}
		}
	}

	private async Task GetUsedClientFileSetRevisions(FileSetCleanupData fileSetCleanupData)
	{
		if (fileSetCleanupData.IsComplete)
		{
			return;
		}
		foreach (long activeOrInProgressFileSetRevisionId in fileSetCleanupData.ActiveOrInProgressFileSetRevisionIds)
		{
			ClientFileSetRevision clientFileSetRevision = await _fileCacheService.GetAnyClientFileSetRevision(fileSetCleanupData.FileSetId, activeOrInProgressFileSetRevisionId);
			if (clientFileSetRevision != null)
			{
				fileSetCleanupData.UsedClientFileSetRevisions.Add(clientFileSetRevision);
			}
		}
		if (!fileSetCleanupData.UsedClientFileSetRevisions.Any())
		{
			fileSetCleanupData.IsComplete = true;
		}
	}

	private void GetActiveOrInProgressFileSetRevisionIds(FileSetCleanupData fileSetCleanupData)
	{
		fileSetCleanupData.StateFile = fileSetCleanupData.CleanupData.StateFiles.FirstOrDefault((StateFile x) => x.FileSetId == fileSetCleanupData.FileSetId);
		if (fileSetCleanupData.StateFile != null)
		{
			if (fileSetCleanupData.StateFile.HasActiveRevision)
			{
				fileSetCleanupData.ActiveOrInProgressFileSetRevisionIds.Add(fileSetCleanupData.StateFile.ActiveRevisionId);
			}
			if (fileSetCleanupData.StateFile.IsRevisionDownloadInProgress)
			{
				fileSetCleanupData.ActiveOrInProgressFileSetRevisionIds.Add(fileSetCleanupData.StateFile.InProgressRevisionId);
			}
		}
		fileSetCleanupData.ActiveOrInProgressFileSetRevisionIds = fileSetCleanupData.ActiveOrInProgressFileSetRevisionIds.Distinct().ToList();
		if (!fileSetCleanupData.ActiveOrInProgressFileSetRevisionIds.Any())
		{
			fileSetCleanupData.IsComplete = true;
		}
	}

	private async Task GetStateFiles(CleanupData fileSetCleanupData)
	{
		StateFilesResponse stateFilesResponse = await _stateFileService.GetAll();
		if (stateFilesResponse.StatusCode == HttpStatusCode.InternalServerError)
		{
			_logger.LogWarningWithSource("Unable to get StateFiles", "GetStateFiles", "/sln/src/UpdateClientService.API/Services/FileSets/FileSetCleanup.cs");
			fileSetCleanupData.IsComplete = true;
		}
		else
		{
			fileSetCleanupData.StateFiles = stateFilesResponse.StateFiles;
		}
	}

	private void AddRequiredFiles(FileSetCleanupData fileSetCleanupData, ClientFileSetRevision clientFileSetRevision)
	{
		List<FileSetFileInfo> collection = fileSetCleanupData.FileSetFileInfos.Where((FileSetFileInfo x) => clientFileSetRevision.Files.Any((ClientFileSetFile y) => y.FileId == x.FileId && y.FileRevisionId == x.FileRevisionId)).ToList();
		List<FileSetFileInfo> collection2 = fileSetCleanupData.FileSetFileInfos.Where((FileSetFileInfo x) => clientFileSetRevision.PatchFiles.Any((ClientPatchFileSetFile y) => y.FileId == x.FileId && y.PatchFileRevisionId == x.FileRevisionId)).ToList();
		fileSetCleanupData.RequiredFileSetFileInfos.AddRange(collection);
		fileSetCleanupData.RequiredFileSetFileInfos.AddRange(collection2);
	}

	private void RemoveUnneededRevisionFiles(FileSetCleanupData fileSetCleanupData)
	{
		List<FileSetFileInfo> list = fileSetCleanupData.FileSetFileInfos.Except(fileSetCleanupData.RequiredFileSetFileInfos.Distinct()).ToList();
		list.ForEach(delegate(FileSetFileInfo eachFileToRemove)
		{
			_logger.LogInfoWithSource($"Deleting FileSetId: {fileSetCleanupData.FileSetId} FileId {eachFileToRemove.FileId} FileRevisionId {eachFileToRemove.FileRevisionId}", "RemoveUnneededRevisionFiles", "/sln/src/UpdateClientService.API/Services/FileSets/FileSetCleanup.cs");
			_fileCacheService.DeleteFile(fileSetCleanupData.FileSetId, eachFileToRemove.FileId, eachFileToRemove.FileRevisionId);
		});
	}
}
