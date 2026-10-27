using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using Redbox.Core;
using Redbox.UpdateManager.ComponentModel;
using Redbox.UpdateManager.FileCache;

namespace Redbox.UpdateManager.FileSets;

internal class FileSetTransition
{
	[Flags]
	private enum MoveFileFlags
	{
		MOVEFILE_REPLACE_EXISTING = 1,
		MOVEFILE_COPY_ALLOWED = 2,
		MOVEFILE_DELAY_UNTIL_REBOOT = 4,
		MOVEFILE_WRITE_THROUGH = 8
	}

	private const string ResultTableName = "Results";

	private ClientFileSetRevision _revision;

	private IFileCacheService _fileCacheService;

	private const string FileSetChangeSetPath = "staging";

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

	internal FileSetTransition(ClientFileSetRevision revision)
	{
		_revision = revision;
	}

	internal ErrorList Stage()
	{
		ErrorList errors = new ErrorList();
		try
		{
			string stagingFileSetRevisionPath = GetStagingFileSetRevisionPath();
			if (!Directory.Exists(stagingFileSetRevisionPath))
			{
				Directory.CreateDirectory(stagingFileSetRevisionPath);
			}
			List<FileData> fileData = GetFileData();
			fileData.ForEach(delegate(FileData f)
			{
				if (!FileCacheService.FileExists(_revision.FileSetId, f.FileId, f.FileRevisionId))
				{
					errors.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("FileSetTranition.Stage", $"FileId {f.FileId} Revision {f.FileRevisionId} doesn't exist"));
				}
			});
			if (errors.ContainsError())
			{
				return errors;
			}
			fileData.ForEach(delegate(FileData f)
			{
				if (!FileCacheService.CopyFileToPath(_revision.FileSetId, f.FileId, f.FileRevisionId, f.StagePath))
				{
					errors.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("FileSetTranition.Stage", $"FileId {f.FileId} Revision {f.FileRevisionId} couldn't be copied to it's staging directory"));
				}
			});
			if (errors.ContainsError())
			{
				return errors;
			}
			fileData.ForEach(delegate(FileData f)
			{
				if (!CheckHash(f.StagePath, f.Hash))
				{
					errors.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("FileSetTranition.Stage", $"FileId {f.FileId} Revision {f.FileRevisionId} hash check failed"));
				}
			});
			return errors;
		}
		catch (Exception e)
		{
			errors.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("FileSetTransition.Stage", "An unhandled exception occurred", e));
		}
		return errors;
	}

	internal ErrorList CheckMeetsDependency(Dictionary<long, FileSetDependencyState> dependencyStates, out bool meetsDependencies)
	{
		ErrorList errorList = new ErrorList();
		meetsDependencies = false;
		try
		{
			IEnumerable<ClientFileSetRevisionDependency> dependencies = _revision.Dependencies;
			if (dependencies.Count() == 0)
			{
				meetsDependencies = true;
				return errorList;
			}
			bool meetsAll = true;
			dependencies.ForEach(delegate(ClientFileSetRevisionDependency dep)
			{
				if (!dependencyStates.ContainsKey(dep.DependsOnFileSetId))
				{
					LogHelper.Instance.Log($"FileSetTransition.CheckMeetsDependency FileSet {_revision.FileSetId} Revision {_revision.RevisionId} Message: Depends on FileSet {dep.DependsOnFileSetId} not found.");
					meetsAll = false;
				}
				else if (!dependencyStates[dep.DependsOnFileSetId].IsDependencyMet(dep.DependencyType, dep.MinimumVersion, dep.MaximumVersion))
				{
					meetsAll = false;
				}
			});
			meetsDependencies = meetsAll;
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("FileSetTransition.CheckMeetsDependency", "An unhandled exception occurred", e));
		}
		return errorList;
	}

	internal ErrorList BeforeActivate(out bool success)
	{
		success = true;
		ErrorList errorList = new ErrorList();
		try
		{
			FileData fileData = (from item in GetFileData()
				where Path.GetFileName(item.FileDestination).ToLower() == "beforeactivate.lua"
				select item).FirstOrDefault();
			if (fileData != null)
			{
				errorList.AddRange(RunScript(fileData.StagePath, out var result));
				bool flag = false;
				if (result.ContainsKey("Failed"))
				{
					flag = Convert.ToBoolean(result["Failed"]);
				}
				string details = string.Empty;
				if (result.ContainsKey("Message"))
				{
					details = Convert.ToString(result["Message"]);
				}
				if (result.ContainsKey("Success"))
				{
					success = Convert.ToBoolean(result["Success"]);
				}
				if (flag)
				{
					success = false;
					errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("FileSetTransition.BeforeActivate", "BeforeActivate script returned failed == true", details));
				}
			}
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("FileSetTransition.BeforeActivate", "An unhandled exception occurred", e));
		}
		return errorList;
	}

	internal ErrorList AfterActivate()
	{
		ErrorList errorList = new ErrorList();
		try
		{
			FileData fileData = (from item in GetFileData()
				where Path.GetFileName(item.FileDestination).ToLower() == "afteractivate.lua"
				select item).FirstOrDefault();
			if (fileData != null)
			{
				errorList.AddRange(RunScript(fileData.FileDestination, out var result));
				bool flag = false;
				if (result.ContainsKey("Failed"))
				{
					flag = Convert.ToBoolean(result["Failed"]);
				}
				string details = string.Empty;
				if (result.ContainsKey("Message"))
				{
					details = Convert.ToString(result["Message"]);
				}
				if (flag)
				{
					errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("FileSetTransition.AfterActivate", "BeforeActivate script returned failed == true", details));
				}
			}
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("FileSetTransition.AfterActivate", "An unhandled exception occurred", e));
		}
		return errorList;
	}

	internal ErrorList Activate()
	{
		ErrorList errors = new ErrorList();
		try
		{
			List<FileData> fileData = GetFileData();
			fileData.ForEach(delegate(FileData f)
			{
				if (!File.Exists(f.StagePath))
				{
					errors.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("FileSetTranition.Activate", $"Staged FileId {f.FileId} Revision {f.FileRevisionId} doesn't exist"));
				}
			});
			if (errors.ContainsError())
			{
				return errors;
			}
			fileData.ForEach(delegate(FileData f)
			{
				string directoryName = Path.GetDirectoryName(f.FileDestination);
				if (!Directory.Exists(directoryName))
				{
					Directory.CreateDirectory(directoryName);
				}
				try
				{
					if (File.Exists(f.FileDestination))
					{
						File.Delete(f.FileDestination);
					}
					File.Move(f.StagePath, f.FileDestination);
				}
				catch (Exception e2)
				{
					try
					{
						LogHelper.Instance.Log("{0} is locked by anther process attempting to retry.", f.FileDestination);
						LogHelper.Instance.Log("Error from activate copy - retrying.", e2);
						ManualResetEvent manualResetEvent = new ManualResetEvent(initialState: false);
						manualResetEvent.WaitOne(TimeSpan.FromMilliseconds(100.0));
						manualResetEvent.Close();
						if (File.Exists(f.FileDestination))
						{
							File.Delete(f.FileDestination);
						}
						File.Move(f.StagePath, f.FileDestination);
					}
					catch (Exception e3)
					{
						LogHelper.Instance.Log("{0} is locked by anther process doing a deferred move.", f.FileDestination);
						LogHelper.Instance.Log("Error from activate copy.", e3);
						string tempFileName = Path.GetTempFileName();
						if (File.Exists(tempFileName))
						{
							File.Delete(tempFileName);
						}
						File.Move(f.StagePath, tempFileName);
						MoveLockedFileSystemEntry(tempFileName, f.FileDestination);
						FileSetService.Instance.MarkRebootRequired();
					}
				}
			});
			errors.ContainsError();
			return errors;
		}
		catch (Exception e)
		{
			errors.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("FileSetTransition.Activate", "An unhandled exception occurred", e));
		}
		return errors;
	}

	private static void MoveLockedFileSystemEntry(string source, string destination)
	{
		MoveFileFlags moveFileFlags = MoveFileFlags.MOVEFILE_DELAY_UNTIL_REBOOT;
		if (!Directory.Exists(source) && !Directory.Exists(destination))
		{
			moveFileFlags |= MoveFileFlags.MOVEFILE_REPLACE_EXISTING;
		}
		MoveFileEx(source, destination, moveFileFlags);
	}

	[DllImport("Kernel32.dll")]
	private static extern bool MoveFileEx(string lpExistingFileName, string lpNewFileName, MoveFileFlags dwFlags);

	private List<FileData> GetFileData()
	{
		List<FileData> result = new List<FileData>();
		string path = GetStagingFileSetRevisionPath();
		_revision.Files.ForEach(delegate(ClientFileSetFile f)
		{
			FileData item = new FileData
			{
				FileId = f.FileId,
				FileRevisionId = f.FileRevisionId,
				Size = f.ContentSize,
				Hash = f.ContentHash,
				StagePath = Path.Combine(path, FileCacheService.GetFileName(f.FileId, f.FileRevisionId)),
				FileDestination = f.FileDestination
			};
			result.Add(item);
		});
		return result;
	}

	private ErrorList RunScript(string script, out Dictionary<string, object> result)
	{
		ErrorList errorList = new ErrorList();
		bool scriptCompleted = true;
		result = new Dictionary<string, object>();
		if (string.IsNullOrEmpty(script))
		{
			return errorList;
		}
		IKernelService service = ServiceLocator.Instance.GetService<IKernelService>();
		LogHelper.Instance.Log("FileSetTransition.RunScript for FileSet: ", _revision.FileSetName);
		try
		{
			service.ExecuteChunkNoLock(script, reset: true, "Results", out var result2, out scriptCompleted);
			if (result2 != null)
			{
				Dictionary<string, object> list = new Dictionary<string, object>();
				result2.ForEach(delegate(KeyValuePair<object, object> item)
				{
					list[(string)item.Key] = item.Value;
				});
				result = list;
			}
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("FileSetTransition.RunScript", string.Format("FileSetTransition.RunScript: unhandled exception in script. Script for FileSet: Name {1}", _revision.FileSetName), e));
		}
		return errorList;
	}

	private string GetStagingDirectory()
	{
		return Path.Combine(FileSetService.Instance.RootPath, "staging");
	}

	private string GetStagingFileSetPath()
	{
		return Path.Combine(GetStagingDirectory(), _revision.FileSetId.ToString());
	}

	private string GetStagingFileSetRevisionPath()
	{
		return Path.Combine(GetStagingFileSetPath(), _revision.RevisionId.ToString());
	}

	private bool CheckHash(string filePath, string hash)
	{
		ErrorList errorList = new ErrorList();
		try
		{
			using FileStream inputStream = File.OpenRead(filePath);
			return inputStream.ToASCIISHA1Hash() == hash;
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("FileSetTransition", "An unhandled exception occurred in CheckHash.", e));
		}
		return false;
	}
}
