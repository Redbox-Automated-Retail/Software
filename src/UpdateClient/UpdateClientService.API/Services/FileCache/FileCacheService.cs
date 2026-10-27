using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Redbox.NetCore.Logging.Extensions;
using Redbox.NetCore.Middleware.Http;
using UpdateClientService.API.Services.FileSets;

namespace UpdateClientService.API.Services.FileCache;

public class FileCacheService : IFileCacheService
{
	private readonly ILogger<FileCacheService> _logger;

	public FileCacheService(ILogger<FileCacheService> logger)
	{
		_logger = logger;
	}

	public bool DoesFileExist(long fileSetId, long fileId, long fileRevisionId)
	{
		try
		{
			string revisionFilePath = GetRevisionFilePath(fileSetId, fileId, fileRevisionId);
			return File.Exists(revisionFilePath);
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, $"Exception while checking the existance of File with FileSetId {fileSetId}, FileId {fileId}, FileRevisionId {fileRevisionId}", "DoesFileExist", "/sln/src/UpdateClientService.API/Services/FileCache/FileCacheService.cs");
		}
		return false;
	}

	public bool IsFileHashValid(long fileSetId, long fileId, long fileRevisionId, string fileHash)
	{
		try
		{
			string revisionFilePath = GetRevisionFilePath(fileSetId, fileId, fileRevisionId);
			return IsFileHashValid(revisionFilePath, fileHash);
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, $"Exception while checking the hash for File with FileSetId {fileSetId}, FileId {fileId}, FileRevisionId {fileRevisionId}", "IsFileHashValid", "/sln/src/UpdateClientService.API/Services/FileCache/FileCacheService.cs");
		}
		return false;
	}

	public bool IsFileHashValid(string filePath, string hash)
	{
		try
		{
			using FileStream stream = File.OpenRead(filePath);
			string sHA1Hash = stream.GetSHA1Hash();
			return sHA1Hash == hash;
		}
		catch (Exception exception)
		{
			_logger.LogError(exception, "Exception while checking hash for file " + filePath + ".");
		}
		return false;
	}

	public bool DoesRevisionExist(RevisionChangeSetKey revisionChangeSetKey)
	{
		try
		{
			string revisionPath = GetRevisionPath(revisionChangeSetKey);
			return File.Exists(revisionPath);
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "Exception while checking the existence of a revsion for " + revisionChangeSetKey?.IdentifyingText(), "DoesRevisionExist", "/sln/src/UpdateClientService.API/Services/FileCache/FileCacheService.cs");
		}
		return false;
	}

	public bool AddFile(long fileSetId, long fileId, long fileRevisionId, byte[] data, string info)
	{
		bool result = false;
		try
		{
			CheckFilePath(fileSetId, fileId);
			string revisionFilePath = GetRevisionFilePath(fileSetId, fileId, fileRevisionId);
			File.WriteAllBytes(revisionFilePath, data);
			string fileInfoPath = GetFileInfoPath(fileSetId, fileId, fileRevisionId);
			File.WriteAllText(fileInfoPath, info);
			_logger.LogInformation("FileCacheService.AddFile - Info: " + info);
			result = true;
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, $"Exception while adding file FileSetId {fileSetId}, FileId {fileId}, FileRevisionId {fileRevisionId}", "AddFile", "/sln/src/UpdateClientService.API/Services/FileCache/FileCacheService.cs");
		}
		return result;
	}

	public bool AddFilePatch(long fileSetId, long fileId, long fileRevisionId, long filePatchRevisionId, byte[] data, string info)
	{
		bool result = false;
		try
		{
			CheckFilePath(fileSetId, fileId);
			if (!GetFile(fileSetId, fileId, filePatchRevisionId, out var _))
			{
				_logger.LogError($"Patch source file is missing FileId: {fileId} FileRevisionId {filePatchRevisionId}");
				return result;
			}
			string revisionFilePath = GetRevisionFilePath(fileSetId, fileId, filePatchRevisionId);
			List<Error> list = new List<Error>();
			using (FileStream fileStream = new FileStream(revisionFilePath, FileMode.Open, FileAccess.Read))
			{
				fileStream.Position = 0L;
				using MemoryStream memoryStream = new MemoryStream(data);
				memoryStream.Position = 0L;
				string revisionFilePath2 = GetRevisionFilePath(fileSetId, fileId, fileRevisionId);
				using FileStream target = new FileStream(revisionFilePath2, FileMode.OpenOrCreate, FileAccess.Write);
				list.AddRange(XDeltaHelper.Apply(fileStream, memoryStream, target));
			}
			string fileInfoPath = GetFileInfoPath(fileSetId, fileId, fileRevisionId);
			File.WriteAllText(fileInfoPath, info);
			_logger.LogInfoWithSource("FileCacheService.AddFilePatch - Info: " + info, "AddFilePatch", "/sln/src/UpdateClientService.API/Services/FileCache/FileCacheService.cs");
			if (list.Count > 0)
			{
				_logger.LogErrorWithSource(list[0].Message, "AddFilePatch", "/sln/src/UpdateClientService.API/Services/FileCache/FileCacheService.cs");
			}
			else
			{
				result = true;
			}
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, $"Exception while adding File Patch with FileSetId {fileSetId}, FileId {fileId}, FileRevisionId {fileRevisionId}, FilePatchRevisionId {filePatchRevisionId}", "AddFilePatch", "/sln/src/UpdateClientService.API/Services/FileCache/FileCacheService.cs");
		}
		return result;
	}

	public bool AddRevision(RevisionChangeSetKey revisionChangeSetKey, byte[] data, string info)
	{
		bool result = false;
		try
		{
			CheckFileSetPath(revisionChangeSetKey.FileSetId);
			string revisionPath = GetRevisionPath(revisionChangeSetKey);
			File.WriteAllBytes(revisionPath, data);
			string revisionInfoPath = GetRevisionInfoPath(revisionChangeSetKey);
			File.WriteAllText(revisionInfoPath, info);
			_logger.LogInfoWithSource("Adding Revision: " + info, "AddRevision", "/sln/src/UpdateClientService.API/Services/FileCache/FileCacheService.cs");
			result = true;
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "Exception while adding revision for " + revisionChangeSetKey?.IdentifyingText(), "AddRevision", "/sln/src/UpdateClientService.API/Services/FileCache/FileCacheService.cs");
		}
		return result;
	}

	public void DeleteFile(long fileSetId, long fileId, long fileRevisionId, bool force = false)
	{
		try
		{
			string revisionFilePath = GetRevisionFilePath(fileSetId, fileId, fileRevisionId);
			if (!force)
			{
				double totalDays = (DateTime.Now - File.GetCreationTime(revisionFilePath)).TotalDays;
				if (totalDays <= 1.0)
				{
					_logger.LogInfoWithSource($"Cannot delete files less than a day old - FileSetId {fileSetId}, FileId {fileId}, FileRevisionId {fileRevisionId}", "DeleteFile", "/sln/src/UpdateClientService.API/Services/FileCache/FileCacheService.cs");
					return;
				}
			}
			_logger.LogInfoWithSource($"Deleting file for FileSetId {fileSetId}, FileId {fileId}, FileRevisionId {fileRevisionId}", "DeleteFile", "/sln/src/UpdateClientService.API/Services/FileCache/FileCacheService.cs");
			DeleteFileIfExists(revisionFilePath);
			string fileInfoPath = GetFileInfoPath(fileSetId, fileId, fileRevisionId);
			DeleteFileIfExists(fileInfoPath);
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, $"Exception while deleting File for FileSetId {fileSetId}, FileId {fileId}, FileRevisionId {fileRevisionId}", "DeleteFile", "/sln/src/UpdateClientService.API/Services/FileCache/FileCacheService.cs");
		}
	}

	private void DeleteFileIfExists(string filePath)
	{
		try
		{
			if (File.Exists(filePath))
			{
				File.Delete(filePath);
			}
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "Exception while deleting file " + filePath, "DeleteFileIfExists", "/sln/src/UpdateClientService.API/Services/FileCache/FileCacheService.cs");
		}
	}

	public void DeleteRevision(RevisionChangeSetKey revisionChangeSetKey)
	{
		try
		{
			_logger.LogInfoWithSource("Deleting revision with " + revisionChangeSetKey?.IdentifyingText(), "DeleteRevision", "/sln/src/UpdateClientService.API/Services/FileCache/FileCacheService.cs");
			string revisionPath = GetRevisionPath(revisionChangeSetKey);
			DeleteFileIfExists(revisionPath);
			string revisionInfoPath = GetRevisionInfoPath(revisionChangeSetKey);
			DeleteFileIfExists(revisionInfoPath);
			string stagingDirectory = GetStagingDirectory(revisionChangeSetKey.FileSetId, revisionChangeSetKey.RevisionId);
			if (Directory.Exists(stagingDirectory))
			{
				Directory.Delete(stagingDirectory, recursive: true);
			}
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "Exception while deleting revision with " + revisionChangeSetKey?.IdentifyingText(), "DeleteRevision", "/sln/src/UpdateClientService.API/Services/FileCache/FileCacheService.cs");
		}
	}

	public bool GetFile(long fileSetId, long fileId, long fileRevisionId, out byte[] data)
	{
		data = null;
		List<Error> list = new List<Error>();
		try
		{
			string revisionFilePath = GetRevisionFilePath(fileSetId, fileId, fileRevisionId);
			if (File.Exists(revisionFilePath))
			{
				data = File.ReadAllBytes(revisionFilePath);
				return true;
			}
		}
		catch (Exception arg)
		{
			list.Add(new Error
			{
				Message = $"FileCacheService.GetFile Unhandled exception occurred. {arg}"
			});
		}
		return false;
	}

	public async Task<List<FileSetFileInfo>> GetFileInfos(long fileSetId)
	{
		List<FileSetFileInfo> result = new List<FileSetFileInfo>();
		List<string> fileInfoPaths = GetFileInfoPaths(fileSetId);
		foreach (string item in fileInfoPaths)
		{
			FileSetFileInfo fileSetFileInfo = await GetFileInfo(item);
			if (fileSetFileInfo != null)
			{
				result.Add(fileSetFileInfo);
			}
		}
		return result;
	}

	public async Task<FileSetFileInfo> GetFileInfo(string filePath)
	{
		FileSetFileInfo result = null;
		try
		{
			var (flag, bytes) = await ReadFileBytes(filePath);
			if (flag)
			{
				result = Encoding.ASCII.GetString(bytes).ToObject<FileSetFileInfo>();
			}
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "Exception while getting FileSetFileInfo from file " + filePath, "GetFileInfo", "/sln/src/UpdateClientService.API/Services/FileCache/FileCacheService.cs");
		}
		return result;
	}

	public bool CopyFileToPath(long fileSetId, long fileId, long fileRevisionId, string copyToPath)
	{
		try
		{
			string revisionFilePath = GetRevisionFilePath(fileSetId, fileId, fileRevisionId);
			if (File.Exists(revisionFilePath))
			{
				File.Copy(revisionFilePath, copyToPath, overwrite: true);
				return true;
			}
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, $"Exception while copying file FileSetId {fileSetId}, FileId {fileId}, FileRevisionId {fileRevisionId}, CopyToPath {copyToPath}", "CopyFileToPath", "/sln/src/UpdateClientService.API/Services/FileCache/FileCacheService.cs");
		}
		return false;
	}

	public async Task<ClientFileSetRevision> GetAnyClientFileSetRevision(long fileSetId, long fileSetRevisionId)
	{
		ClientFileSetRevision result = null;
		try
		{
			List<string> clientFileSetRevisionFilePaths = GetClientFileSetRevisionFilePaths(fileSetId, fileSetRevisionId);
			string filePath = clientFileSetRevisionFilePaths.FirstOrDefault();
			result = await GetClientFileSetRevision(filePath);
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, $"Exception while getting ClientFileSetRevision for FileSetId {fileSetId} and FileSetRevisionId {fileSetRevisionId}", "GetAnyClientFileSetRevision", "/sln/src/UpdateClientService.API/Services/FileCache/FileCacheService.cs");
		}
		return result;
	}

	public async Task<ClientFileSetRevision> GetClientFileSetRevision(RevisionChangeSetKey revisionChangeSetKey)
	{
		ClientFileSetRevision result = null;
		try
		{
			string revisionPath = GetRevisionPath(revisionChangeSetKey);
			result = await GetClientFileSetRevision(revisionPath);
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "Exception while reading ClienfFileSetRevision for " + revisionChangeSetKey?.IdentifyingText(), "GetClientFileSetRevision", "/sln/src/UpdateClientService.API/Services/FileCache/FileCacheService.cs");
		}
		return result;
	}

	public async Task<ClientFileSetRevision> GetClientFileSetRevision(string filePath)
	{
		ClientFileSetRevision result = null;
		try
		{
			var (flag, bytes) = await ReadFileBytes(filePath);
			if (flag)
			{
				result = Encoding.ASCII.GetString(bytes).ToObject<ClientFileSetRevision>();
			}
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "Exception while reading ClienfFileSetRevision from file " + filePath, "GetClientFileSetRevision", "/sln/src/UpdateClientService.API/Services/FileCache/FileCacheService.cs");
		}
		return result;
	}

	private async Task<(bool, byte[])> ReadFileBytes(string filePath)
	{
		bool result = false;
		byte[] fileBytes = null;
		try
		{
			if (File.Exists(filePath))
			{
				fileBytes = await File.ReadAllBytesAsync(filePath);
				result = true;
			}
		}
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "Exception while reading file " + filePath, "ReadFileBytes", "/sln/src/UpdateClientService.API/Services/FileCache/FileCacheService.cs");
		}
		return (result, fileBytes);
	}

	public List<string> GetClientFileSetRevisionFilePaths(long fileSetId)
	{
		List<string> result = new List<string>();
		string fileSetDirectory = GetFileSetDirectory(fileSetId);
		if (Directory.Exists(fileSetDirectory))
		{
			result = Directory.GetFiles(fileSetDirectory, "*.revision", SearchOption.TopDirectoryOnly).ToList();
		}
		return result;
	}

	public List<long> GetFileSetIds()
	{
		List<long> list = new List<long>();
		if (!Directory.Exists(Constants.FileCacheRootPath))
		{
			return list;
		}
		string[] directories = Directory.GetDirectories(Constants.FileCacheRootPath);
		string[] array = directories;
		foreach (string path in array)
		{
			string fileName = Path.GetFileName(path);
			try
			{
				list.Add(Convert.ToInt64(fileName));
			}
			catch
			{
			}
		}
		return list;
	}

	public List<long> GetFileSetRevisionIds(long fileSetId)
	{
		List<long> list = new List<long>();
		List<string> clientFileSetRevisionFilePaths = GetClientFileSetRevisionFilePaths(fileSetId);
		foreach (string item2 in clientFileSetRevisionFilePaths)
		{
			string fileName = Path.GetFileName(item2);
			var (flag, item) = ParseRevisionIdFromRevisionFileName(fileName);
			if (flag)
			{
				list.Add(item);
			}
		}
		return list.Distinct().ToList();
	}

	private (bool success, long revisionId) ParseRevisionIdFromRevisionFileName(string revisionFileName)
	{
		bool item = false;
		long result = 0L;
		if (!string.IsNullOrEmpty(revisionFileName) && revisionFileName.Contains("-"))
		{
			string[] array = revisionFileName.Split('-');
			if (array != null && array.Count() > 0 && long.TryParse(array[0], out result))
			{
				item = true;
			}
		}
		return (success: item, revisionId: result);
	}

	public List<string> GetClientFileSetRevisionFilePaths(long fileSetId, long revisionId)
	{
		List<string> result = new List<string>();
		string fileSetDirectory = GetFileSetDirectory(fileSetId);
		if (Directory.Exists(fileSetDirectory))
		{
			result = Directory.GetFiles(fileSetDirectory, $"{revisionId}-*.revision", SearchOption.TopDirectoryOnly).ToList();
		}
		return result;
	}

	public DateTime GetRevisionCreationDate(long fileSetId, long revisionId)
	{
		List<string> clientFileSetRevisionFilePaths = GetClientFileSetRevisionFilePaths(fileSetId, revisionId);
		return GetMaxCreationdDate(clientFileSetRevisionFilePaths);
	}

	private DateTime GetMaxCreationdDate(List<string> filePaths)
	{
		DateTime dateTime = DateTime.MinValue;
		foreach (string filePath in filePaths)
		{
			DateTime creationTime = File.GetCreationTime(filePath);
			if (creationTime > dateTime)
			{
				dateTime = creationTime;
			}
		}
		return dateTime;
	}

	public List<string> GetFileInfoPaths(long fileSetId)
	{
		List<string> result = new List<string>();
		string revisionFileDirectory = GetRevisionFileDirectory(fileSetId);
		if (Directory.Exists(revisionFileDirectory))
		{
			result = Directory.GetFiles(revisionFileDirectory, "*.fileinfo", SearchOption.TopDirectoryOnly).ToList();
		}
		return result;
	}

	private string GetFileSetDirectory(long fileSetId)
	{
		return Path.Combine(Constants.FileCacheRootPath, $"{fileSetId}");
	}

	private string GetRevisionPath(RevisionChangeSetKey revisionChangeSetKey)
	{
		return GetRevisionPath(revisionChangeSetKey?.FileSetId ?? 0, revisionChangeSetKey?.RevisionId ?? 0, revisionChangeSetKey?.PatchRevisionId ?? 0);
	}

	private string GetRevisionPath(long fileSetId, long revisionId, long patchRevisionId)
	{
		return Path.Combine(GetFileSetDirectory(fileSetId), GetRevisionName(revisionId, patchRevisionId));
	}

	private string GetRevisionInfoPath(RevisionChangeSetKey revisionChangeSetKey)
	{
		return GetRevisionInfoPath(revisionChangeSetKey?.FileSetId ?? 0, revisionChangeSetKey?.RevisionId ?? 0, revisionChangeSetKey?.PatchRevisionId ?? 0);
	}

	private string GetRevisionInfoPath(long fileSetId, long revisionId, long patchRevisionId)
	{
		return Path.Combine(GetFileSetDirectory(fileSetId), GetRevisionInfoName(revisionId, patchRevisionId));
	}

	private string GetStagingDirectory(long fileSetId, long revisionId)
	{
		return Path.Combine(Constants.FileSetsRoot, "staging", $"{fileSetId}", $"{revisionId}");
	}

	private string GetRevisionName(long revisionId, long patchRevisionId)
	{
		return $"{revisionId}-{patchRevisionId}.revision";
	}

	private string GetRevisionInfoName(long revisionId, long patchRevisionId)
	{
		return $"{revisionId}-{patchRevisionId}.revisioninfo";
	}

	private string GetRevisionFileDirectory(long fileSetId)
	{
		return Path.Combine(GetFileSetDirectory(fileSetId), "File");
	}

	private string GetRevisionFilePath(long fileSetId, long fileId, long fileRevisionId)
	{
		return Path.Combine(GetRevisionFileDirectory(fileSetId), GetRevisionFileName(fileId, fileRevisionId));
	}

	private string GetFileInfoPath(long fileSetId, long fileId, long fileRevisionId)
	{
		return Path.Combine(GetRevisionFileDirectory(fileSetId), GetFileInfoName(fileId, fileRevisionId));
	}

	public string GetRevisionFileName(long fileId, long fileRevisionId)
	{
		return $"{fileId}-{fileRevisionId}.file";
	}

	private string GetFileInfoName(long fileId, long fileRevisionId)
	{
		return $"{fileId}-{fileRevisionId}.fileinfo";
	}

	private void CheckFilePath(long fileSetId, long fileId)
	{
		string directoryPath = Path.Combine(GetRevisionFileDirectory(fileSetId));
		CreateDirectoryIfNeeded(directoryPath);
	}

	private void CheckFileSetPath(long fileSetId)
	{
		string directoryPath = Path.Combine(GetFileSetDirectory(fileSetId));
		CreateDirectoryIfNeeded(directoryPath);
	}

	private void CreateDirectoryIfNeeded(string directoryPath)
	{
		if (!Directory.Exists(directoryPath))
		{
			Directory.CreateDirectory(directoryPath);
		}
	}
}
