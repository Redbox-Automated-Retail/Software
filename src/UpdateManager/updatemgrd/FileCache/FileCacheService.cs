using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Redbox.Core;
using Redbox.DeltaCompression;
using Redbox.DeltaCompression.Binary;
using Redbox.UpdateManager.ComponentModel;

namespace Redbox.UpdateManager.FileCache;

internal class FileCacheService : IFileCacheService
{
	private string _root;

	public static FileCacheService Instance => Singleton<FileCacheService>.Instance;

	public Redbox.UpdateManager.ComponentModel.ErrorList Initialize(string rootPath)
	{
		Redbox.UpdateManager.ComponentModel.ErrorList errorList = new Redbox.UpdateManager.ComponentModel.ErrorList();
		try
		{
			_root = rootPath;
			if (!Directory.Exists(_root))
			{
				Directory.CreateDirectory(_root);
			}
			LogHelper.Instance.Log("FileCacheService Directory: {0}", rootPath);
			ServiceLocator.Instance.AddService(typeof(IFileCacheService), this);
			LogHelper.Instance.Log("Initialized the FileCacheService", LogEntryType.Info);
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("FileCacheService.Initialize", "Unhandled exception occurred.", e));
		}
		return errorList;
	}

	public bool FileExists(long fileSetId, long fileId, long fileRevisionId)
	{
		Redbox.UpdateManager.ComponentModel.ErrorList errorList = new Redbox.UpdateManager.ComponentModel.ErrorList();
		try
		{
			return File.Exists(GetFilePath(fileSetId, fileId, fileRevisionId));
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("FileCacheService.FileExists", "Unhandled exception occurred.", e));
		}
		return false;
	}

	public bool RevisionExists(long fileSetId, long revisionId)
	{
		Redbox.UpdateManager.ComponentModel.ErrorList errorList = new Redbox.UpdateManager.ComponentModel.ErrorList();
		try
		{
			return GetAllRevisionFiles(fileSetId, revisionId).Count() > 0;
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("FileCacheService.RevisionExists", "Unhandled exception occurred.", e));
		}
		return false;
	}

	public bool RevisionExists(long fileSetId, long revisionId, long patchRevisionId)
	{
		Redbox.UpdateManager.ComponentModel.ErrorList errorList = new Redbox.UpdateManager.ComponentModel.ErrorList();
		try
		{
			return File.Exists(GetRevisionPath(fileSetId, revisionId, patchRevisionId));
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("FileCacheService.RevisionExists", "Unhandled exception occurred.", e));
		}
		return false;
	}

	public Redbox.UpdateManager.ComponentModel.ErrorList AddFile(long fileSetId, long fileId, long fileRevisionId, byte[] data, string info)
	{
		Redbox.UpdateManager.ComponentModel.ErrorList errorList = new Redbox.UpdateManager.ComponentModel.ErrorList();
		try
		{
			CheckFilePath(fileSetId, fileId);
			File.WriteAllBytes(GetFilePath(fileSetId, fileId, fileRevisionId), data);
			File.WriteAllText(GetFileInfoPath(fileSetId, fileId, fileRevisionId), info);
			LogHelper.Instance.Log($"FileCacheService.AddFile - Info: {info}");
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("FileCacheService.AddFile", "Unhandled exception occurred.", e));
		}
		return errorList;
	}

	public Redbox.UpdateManager.ComponentModel.ErrorList AddFilePatch(long fileSetId, long fileId, long fileRevisionId, long filePatchRevisionId, byte[] data, string info)
	{
		Redbox.UpdateManager.ComponentModel.ErrorList errorList = new Redbox.UpdateManager.ComponentModel.ErrorList();
		try
		{
			CheckFilePath(fileSetId, fileId);
			if (!GetFile(fileSetId, fileId, filePatchRevisionId, out var _))
			{
				errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("FileCacheService.AddFilePatch", $"Patch source file is missing FileId: {fileId} FileRevisionId {filePatchRevisionId}"));
				return errorList;
			}
			using (FileStream fileStream = new FileStream(GetFilePath(fileSetId, fileId, filePatchRevisionId), FileMode.Open, FileAccess.Read))
			{
				fileStream.Position = 0L;
				using MemoryStream memoryStream = new MemoryStream(data);
				memoryStream.Position = 0L;
				using FileStream target = new FileStream(GetFilePath(fileSetId, fileId, fileRevisionId), FileMode.OpenOrCreate, FileAccess.Write);
				errorList.AddRange(ConvertFromXDeltaErrors(XDeltaHelper.Apply(fileStream, memoryStream, target)));
			}
			File.WriteAllText(GetFileInfoPath(fileSetId, fileId, fileRevisionId), info);
			LogHelper.Instance.Log($"FileCacheService.AddFilePatch - Info: {info}");
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("FileCacheService.AddFilePatch", "Unhandled exception occurred.", e));
		}
		return errorList;
	}

	private static Redbox.UpdateManager.ComponentModel.ErrorList ConvertFromXDeltaErrors(List<Redbox.DeltaCompression.Error> xDeltaErrors)
	{
		Redbox.UpdateManager.ComponentModel.ErrorList errors = new Redbox.UpdateManager.ComponentModel.ErrorList();
		xDeltaErrors.ForEach(delegate(Redbox.DeltaCompression.Error e)
		{
			errors.Add(e.IsWarning ? Redbox.UpdateManager.ComponentModel.Error.NewWarning(e.Code, e.Description, e.Details) : Redbox.UpdateManager.ComponentModel.Error.NewError(e.Code, e.Description, e.Details));
		});
		return errors;
	}

	public Redbox.UpdateManager.ComponentModel.ErrorList AddRevision(long fileSetId, long revisionId, long patchRevisionId, byte[] data, string info)
	{
		Redbox.UpdateManager.ComponentModel.ErrorList errorList = new Redbox.UpdateManager.ComponentModel.ErrorList();
		try
		{
			CheckFileSetPath(fileSetId);
			File.WriteAllBytes(GetRevisionPath(fileSetId, revisionId, patchRevisionId), data);
			File.WriteAllText(GetRevisionInfoPath(fileSetId, revisionId, patchRevisionId), info);
			LogHelper.Instance.Log($"FileCacheService.AddRevision - Info: {info}");
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("FileCacheService.AddRevision", "Unhandled exception occurred.", e));
		}
		return errorList;
	}

	public Redbox.UpdateManager.ComponentModel.ErrorList AddFile(long fileSetId, long fileId, long fileRevisionId, string pathToMoveFrom)
	{
		Redbox.UpdateManager.ComponentModel.ErrorList errorList = new Redbox.UpdateManager.ComponentModel.ErrorList();
		try
		{
			CheckFilePath(fileSetId, fileId);
			string filePath = GetFilePath(fileSetId, fileId, fileRevisionId);
			File.Move(pathToMoveFrom, filePath);
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("FileCacheService.AddFile", "Unhandled exception occurred.", e));
		}
		return errorList;
	}

	public Redbox.UpdateManager.ComponentModel.ErrorList AddRevision(long fileSetId, long revisionId, long patchRevisionId, string pathToMoveFrom)
	{
		Redbox.UpdateManager.ComponentModel.ErrorList errorList = new Redbox.UpdateManager.ComponentModel.ErrorList();
		try
		{
			CheckFileSetPath(fileSetId);
			string revisionPath = GetRevisionPath(fileSetId, revisionId, patchRevisionId);
			File.Move(pathToMoveFrom, revisionPath);
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("FileCacheService.AddRevision", "Unhandled exception occurred.", e));
		}
		return errorList;
	}

	public void DeleteFile(long fileSetId, long fileId, long fileRevisionId)
	{
		Redbox.UpdateManager.ComponentModel.ErrorList errorList = new Redbox.UpdateManager.ComponentModel.ErrorList();
		try
		{
			string filePath = GetFilePath(fileSetId, fileId, fileRevisionId);
			if ((int)(DateTime.Now - File.GetCreationTime(filePath)).TotalDays <= 1)
			{
				LogHelper.Instance.Log("FileCacheService.DeleteFile - Cannot delete files less than 2 days - FileSetId {0}, FileId {1}, FileRevisionId {2}", fileSetId, fileId, fileRevisionId);
				return;
			}
			LogHelper.Instance.Log("FileCacheService.DeleteFile - FileSetId {0}, FileId {1}, FileRevisionId {2}", fileSetId, fileId, fileRevisionId);
			if (File.Exists(filePath))
			{
				File.Delete(filePath);
			}
			string fileInfoPath = GetFileInfoPath(fileSetId, fileId, fileRevisionId);
			if (File.Exists(fileInfoPath))
			{
				File.Delete(fileInfoPath);
			}
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("FileCacheService.DeleteFile", "Unhandled exception occurred.", e));
		}
	}

	public void DeleteRevision(long fileSetId, long revisionId, long patchRevisionId)
	{
		Redbox.UpdateManager.ComponentModel.ErrorList errorList = new Redbox.UpdateManager.ComponentModel.ErrorList();
		try
		{
			LogHelper.Instance.Log("FileCacheService.DeleteRevision - FileSetId {0}, RevisionId {1}, PatchRevisionId {2}", fileSetId, revisionId, patchRevisionId);
			string revisionPath = GetRevisionPath(fileSetId, revisionId, patchRevisionId);
			if (File.Exists(revisionPath))
			{
				File.Delete(revisionPath);
			}
			string revisionInfoPath = GetRevisionInfoPath(fileSetId, revisionId, patchRevisionId);
			if (File.Exists(revisionInfoPath))
			{
				File.Delete(revisionInfoPath);
			}
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("FileCacheService.DeleteRevision", "Unhandled exception occurred.", e));
		}
	}

	public bool GetFile(long fileSetId, long fileId, long fileRevisionId, out byte[] data)
	{
		data = null;
		Redbox.UpdateManager.ComponentModel.ErrorList errorList = new Redbox.UpdateManager.ComponentModel.ErrorList();
		try
		{
			string filePath = GetFilePath(fileSetId, fileId, fileRevisionId);
			if (File.Exists(filePath))
			{
				data = File.ReadAllBytes(filePath);
				return true;
			}
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("FileCacheService.GetFile", "Unhandled exception occurred.", e));
		}
		return false;
	}

	public bool GetFileInfo(string path, out byte[] data)
	{
		data = null;
		Redbox.UpdateManager.ComponentModel.ErrorList errorList = new Redbox.UpdateManager.ComponentModel.ErrorList();
		try
		{
			if (File.Exists(path))
			{
				data = File.ReadAllBytes(path);
				return true;
			}
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("FileCacheService.GetFileInfo", "Unhandled exception occurred.", e));
		}
		return false;
	}

	public bool CopyFileToPath(long fileSetId, long fileId, long fileRevisionId, string copyToPath)
	{
		Redbox.UpdateManager.ComponentModel.ErrorList errorList = new Redbox.UpdateManager.ComponentModel.ErrorList();
		try
		{
			string filePath = GetFilePath(fileSetId, fileId, fileRevisionId);
			if (File.Exists(filePath))
			{
				File.Copy(filePath, copyToPath, overwrite: true);
				return true;
			}
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("FileCacheService.CopyFileToPath", "Unhandled exception occurred.", e));
		}
		return false;
	}

	public bool GetRevision(long fileSetId, long revisionId, out byte[] data)
	{
		data = null;
		Redbox.UpdateManager.ComponentModel.ErrorList errorList = new Redbox.UpdateManager.ComponentModel.ErrorList();
		try
		{
			List<string> allRevisionFiles = GetAllRevisionFiles(fileSetId, revisionId);
			if (allRevisionFiles.Count() == 0)
			{
				return false;
			}
			string text = allRevisionFiles.OrderByDescending((string f) => f).FirstOrDefault((string f) => Path.GetExtension(f).Equals(".revision", StringComparison.CurrentCultureIgnoreCase));
			if (string.IsNullOrEmpty(text))
			{
				return false;
			}
			if (File.Exists(text))
			{
				data = File.ReadAllBytes(text);
				return true;
			}
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("FileCacheService.GetRevision", "Unhandled exception occurred.", e));
		}
		return false;
	}

	public bool GetRevision(long fileSetId, long revisionId, long patchRevisionId, out byte[] data)
	{
		data = null;
		Redbox.UpdateManager.ComponentModel.ErrorList errorList = new Redbox.UpdateManager.ComponentModel.ErrorList();
		try
		{
			string revisionPath = GetRevisionPath(fileSetId, revisionId, patchRevisionId);
			if (File.Exists(revisionPath))
			{
				data = File.ReadAllBytes(revisionPath);
				return true;
			}
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("FileCacheService.GetRevision", "Unhandled exception occurred.", e));
		}
		return false;
	}

	public bool GetRevision(string path, out byte[] data)
	{
		data = null;
		Redbox.UpdateManager.ComponentModel.ErrorList errorList = new Redbox.UpdateManager.ComponentModel.ErrorList();
		try
		{
			if (File.Exists(path))
			{
				data = File.ReadAllBytes(path);
				return true;
			}
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("FileCacheService.GetRevision", "Unhandled exception occurred.", e));
		}
		return false;
	}

	public List<string> GetAllRevisionFiles(long fileSetId)
	{
		return Directory.GetFiles(GetRevisionPath(fileSetId), "*.revision", SearchOption.TopDirectoryOnly).ToList();
	}

	public List<long> GetAllFileSets()
	{
		List<long> fileSets = new List<long>();
		Directory.GetDirectories(_root).ForEach(delegate(string d)
		{
			string fileName = Path.GetFileName(d);
			try
			{
				fileSets.Add(Convert.ToInt64(fileName));
			}
			catch
			{
			}
		});
		return fileSets;
	}

	private List<string> GetAllRevisionFiles(long fileSetId, long revisionId)
	{
		return Directory.GetFiles(GetRevisionPath(fileSetId), $"{revisionId}-*.revision", SearchOption.TopDirectoryOnly).ToList();
	}

	public List<string> GetAllFiles(long fileSetId)
	{
		return Directory.GetFiles(GetFilePath(fileSetId), "*.file", SearchOption.TopDirectoryOnly).ToList();
	}

	public List<string> GetAllFileInfos(long fileSetId)
	{
		return Directory.GetFiles(GetFilePath(fileSetId), "*.fileinfo", SearchOption.TopDirectoryOnly).ToList();
	}

	private string GetRevisionPath(long fileSetId)
	{
		return Path.Combine(_root, $"{fileSetId}\\");
	}

	private string GetRevisionPath(long fileSetId, long revisionId, long patchRevisionId)
	{
		return Path.Combine(_root, $"{fileSetId}\\{GetRevisionName(revisionId, patchRevisionId)}");
	}

	private string GetRevisionInfoPath(long fileSetId, long revisionId, long patchRevisionId)
	{
		return Path.Combine(_root, $"{fileSetId}\\{GetRevisionInfoName(revisionId, patchRevisionId)}");
	}

	private string GetRevisionName(long revisionId, long patchRevisionId)
	{
		return $"{revisionId}-{patchRevisionId}.revision";
	}

	private string GetRevisionInfoName(long revisionId, long patchRevisionId)
	{
		return $"{revisionId}-{patchRevisionId}.revisioninfo";
	}

	private string GetFilePath(long fileSetId)
	{
		return Path.Combine(_root, $"{fileSetId}\\File\\");
	}

	private string GetFilePath(long fileSetId, long fileId, long fileRevisionId)
	{
		return Path.Combine(_root, $"{fileSetId}\\File\\{GetFileName(fileId, fileRevisionId)}");
	}

	private string GetFileInfoPath(long fileSetId, long fileId, long fileRevisionId)
	{
		return Path.Combine(_root, $"{fileSetId}\\File\\{GetFileInfoName(fileId, fileRevisionId)}");
	}

	public string GetFileName(long fileId, long fileRevisionId)
	{
		return $"{fileId}-{fileRevisionId}.file";
	}

	private string GetFileInfoName(long fileId, long fileRevisionId)
	{
		return $"{fileId}-{fileRevisionId}.fileinfo";
	}

	private void CheckFilePath(long fileSetId, long fileId)
	{
		string path = Path.Combine(_root, $"{fileSetId}\\File");
		if (!Directory.Exists(path))
		{
			Directory.CreateDirectory(path);
		}
	}

	private void CheckFileSetPath(long fileSetId)
	{
		string path = Path.Combine(_root, $"{fileSetId}");
		if (!Directory.Exists(path))
		{
			Directory.CreateDirectory(path);
		}
	}

	private FileCacheService()
	{
	}
}
