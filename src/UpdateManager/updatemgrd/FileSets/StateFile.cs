using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Redbox.Core;
using Redbox.UpdateManager.ComponentModel;
using Redbox.UpdateService.Model;

namespace Redbox.UpdateManager.FileSets;

internal class StateFile
{
	private static object _lock = new object();

	private long _fileSetId;

	private long _revisionId;

	private long _inProgressRevisionId;

	private FileSetState _fileSetState;

	private FileSetState _inProgressFileSetState;

	private const string FileSetStateActiveExt = ".active";

	private const string FileSetStateInProgressExt = ".inprogress";

	internal long FileSetId => _fileSetId;

	internal long RevisionId
	{
		get
		{
			return _revisionId;
		}
		set
		{
			_revisionId = value;
		}
	}

	internal long InProgressRevisionId
	{
		get
		{
			return _inProgressRevisionId;
		}
		set
		{
			_inProgressRevisionId = value;
		}
	}

	internal FileSetState FileSetState
	{
		get
		{
			return _fileSetState;
		}
		set
		{
			_fileSetState = value;
		}
	}

	internal FileSetState InProgressFileSetState => _inProgressFileSetState;

	internal StateFile(long fileSetId)
	{
		_fileSetId = fileSetId;
	}

	internal bool HasInProgress()
	{
		return _inProgressRevisionId > 0;
	}

	internal bool HasActive()
	{
		return _revisionId > 0;
	}

	internal ErrorList Activate()
	{
		_revisionId = InProgressRevisionId;
		_inProgressRevisionId = 0L;
		FileSetState = FileSetState.Active;
		DeleteInProgress();
		return Save();
	}

	internal void SetInProgressState(FileSetState state)
	{
		new ClientFileSetState
		{
			FileSetId = _fileSetId,
			RevisionId = _revisionId,
			FileSetState = state
		};
		if (state == FileSetState.Active)
		{
			_revisionId = _inProgressRevisionId;
			_inProgressRevisionId = 0L;
			_fileSetState = state;
		}
		else
		{
			_inProgressFileSetState = state;
		}
		Save();
	}

	internal ErrorList Load()
	{
		ErrorList errorList = new ErrorList();
		try
		{
			lock (_lock)
			{
				string activeStatePath = GetActiveStatePath();
				string inProgressStatePath = GetInProgressStatePath();
				_revisionId = 0L;
				_inProgressRevisionId = 0L;
				_fileSetState = FileSetState.Active;
				_inProgressFileSetState = FileSetState.InProgress;
				if (File.Exists(activeStatePath))
				{
					try
					{
						ClientFileSetState clientFileSetState = File.ReadAllText(activeStatePath).ToObject<ClientFileSetState>();
						_revisionId = clientFileSetState.RevisionId;
					}
					catch (Exception e)
					{
						errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("StateFile.Load", $"Error parsing state file deleting {activeStatePath} ", e));
						Shared.SafeDelete(activeStatePath);
					}
				}
				if (File.Exists(inProgressStatePath))
				{
					try
					{
						ClientFileSetState clientFileSetState2 = File.ReadAllText(inProgressStatePath).ToObject<ClientFileSetState>();
						_inProgressRevisionId = clientFileSetState2.RevisionId;
						_inProgressFileSetState = clientFileSetState2.FileSetState;
					}
					catch (Exception e2)
					{
						errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("StateFile.Load", $"Error parsing state file deleting {inProgressStatePath} ", e2));
						Shared.SafeDelete(inProgressStatePath);
					}
				}
			}
		}
		catch (Exception e3)
		{
			errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("StateManager.Load", "An unhandled exception occurred.", e3));
		}
		return errorList;
	}

	internal ErrorList Delete()
	{
		ErrorList errorList = new ErrorList();
		try
		{
			lock (_lock)
			{
				Shared.SafeDelete(GetActiveStatePath());
				Shared.SafeDelete(GetInProgressStatePath());
			}
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("StateManager.Delete", "An unhandled exception occurred.", e));
		}
		return errorList;
	}

	internal ErrorList DeleteInProgress()
	{
		ErrorList errorList = new ErrorList();
		try
		{
			lock (_lock)
			{
				Shared.SafeDelete(GetInProgressStatePath());
			}
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("StateManager.DeleteInProgress", "An unhandled exception occurred.", e));
		}
		return errorList;
	}

	internal ErrorList Save()
	{
		ErrorList errorList = new ErrorList();
		try
		{
			lock (_lock)
			{
				string activeStatePath = GetActiveStatePath();
				string inProgressStatePath = GetInProgressStatePath();
				if (_revisionId == 0L)
				{
					Shared.SafeDelete(activeStatePath);
				}
				else
				{
					File.WriteAllText(activeStatePath, new ClientFileSetState
					{
						FileSetId = _fileSetId,
						RevisionId = _revisionId,
						FileSetState = _fileSetState
					}.ToJson());
				}
				if (_inProgressRevisionId == 0L)
				{
					Shared.SafeDelete(inProgressStatePath);
				}
				else
				{
					File.WriteAllText(inProgressStatePath, new ClientFileSetState
					{
						FileSetId = _fileSetId,
						RevisionId = _inProgressRevisionId,
						FileSetState = _inProgressFileSetState
					}.ToJson());
				}
			}
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("StateManager.Save", "An unhandled exception occurred.", e));
		}
		return errorList;
	}

	internal static ErrorList CreateFromChangeSet(ClientFileSetRevisionChangeSet set)
	{
		ErrorList errorList = new ErrorList();
		try
		{
			StateFile stateFile = new StateFile(set.FileSetId);
			stateFile.Load();
			stateFile.InProgressRevisionId = set.RevisionId;
			stateFile.SetInProgressState(FileSetState.InProgress);
			stateFile.Save();
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("StateManager.CreateFromState", "An unhandled exception occurred.", e));
		}
		return errorList;
	}

	internal static ErrorList GetCurrentState(out FileSetPollRequestList fileSetPollRequestList)
	{
		ErrorList errorList = new ErrorList();
		fileSetPollRequestList = new FileSetPollRequestList();
		try
		{
			lock (_lock)
			{
				errorList.AddRange(GetAllFiles(out var stateFiles));
				if (errorList.ContainsError())
				{
					return errorList;
				}
				Dictionary<long, FileSetPollRequest> list = new Dictionary<long, FileSetPollRequest>();
				stateFiles.Where((ClientFileSetState item) => item.FileSetState == FileSetState.Active).ForEach(delegate(ClientFileSetState item)
				{
					FileSetPollRequest value = new FileSetPollRequest
					{
						FileSetId = item.FileSetId,
						FileSetRevisionId = item.RevisionId,
						FileSetState = item.FileSetState
					};
					list[item.FileSetId] = value;
				});
				stateFiles.Where((ClientFileSetState item) => item.FileSetState != FileSetState.Active).ForEach(delegate(ClientFileSetState item)
				{
					FileSetPollRequest value = new FileSetPollRequest
					{
						FileSetId = item.FileSetId,
						FileSetRevisionId = item.RevisionId,
						FileSetState = item.FileSetState
					};
					list[item.FileSetId] = value;
				});
				FileSetPollRequestList requestList = new FileSetPollRequestList();
				list.ForEach(delegate(KeyValuePair<long, FileSetPollRequest> item)
				{
					requestList.FileSetPollRequests.Add(item.Value);
				});
				fileSetPollRequestList = requestList;
			}
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("StateManager.GetCurrentState", "Unhandled exception occurred", e));
		}
		return errorList;
	}

	internal static ErrorList GetAllInProgress(out List<StateFile> list)
	{
		ErrorList errorList = new ErrorList();
		list = new List<StateFile>();
		try
		{
			errorList.AddRange(GetAll(out var list2));
			if (errorList.ContainsError())
			{
				return errorList;
			}
			list = list2.Where((StateFile item) => item.HasInProgress()).ToList();
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("StateManager.GetCurrentState", "Unhandled exception occurred", e));
		}
		return errorList;
	}

	internal static ErrorList GetAll(out List<StateFile> list)
	{
		ErrorList errorList = new ErrorList();
		list = new List<StateFile>();
		try
		{
			lock (_lock)
			{
				List<ClientFileSetState> stateFiles = null;
				errorList.AddRange(GetAllFiles(out stateFiles));
				if (errorList.ContainsError())
				{
					return errorList;
				}
				List<long> list2 = stateFiles.Select((ClientFileSetState a) => a.FileSetId).Distinct().ToList();
				List<StateFile> tempList = list;
				list2.ForEach(delegate(long fileSetId)
				{
					StateFile stateFile = new StateFile(fileSetId);
					if (stateFile.Load().ContainsError())
					{
						Shared.SafeDelete(GetActiveStatePath(fileSetId));
						Shared.SafeDelete(GetInProgressStatePath(fileSetId));
					}
					else
					{
						tempList.Add(stateFile);
					}
				});
			}
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("StateManager.GetAll", "Unhandled exception occurred", e));
		}
		return errorList;
	}

	private static ErrorList GetAllFiles(out List<ClientFileSetState> stateFiles)
	{
		stateFiles = new List<ClientFileSetState>();
		ErrorList errorList = new ErrorList();
		try
		{
			if (!Directory.Exists(FileSetService.Instance.RootPath))
			{
				errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("StateManager.GetAllFiles", "No directory found at " + FileSetService.Instance.RootPath, "No fileset state will be sent to the server"));
				return errorList;
			}
			string[] files = Directory.GetFiles(FileSetService.Instance.RootPath, '*' + ".active", SearchOption.TopDirectoryOnly);
			string[] files2 = Directory.GetFiles(FileSetService.Instance.RootPath, '*' + ".inprogress", SearchOption.TopDirectoryOnly);
			List<string> list = new List<string>();
			files.ForEach(list.Add);
			files2.ForEach(list.Add);
			LogHelper.Instance.Log($"StateManager.GetAllFiles json: {list.ToJson()} ", LogEntryType.Debug);
			foreach (string item2 in list)
			{
				try
				{
					ClientFileSetState item = File.ReadAllText(item2).ToObject<ClientFileSetState>();
					stateFiles.Add(item);
				}
				catch (Exception e)
				{
					errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("StateManager.GetAllFiles", $"Error parsing state file deleting {item2} ", e));
					Shared.SafeDelete(item2);
				}
			}
		}
		catch (Exception e2)
		{
			errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("StateManager.GetAllFiles", "Unhandled exception occurred.", e2));
		}
		return errorList;
	}

	private string GetActiveStatePath()
	{
		return GetActiveStatePath(_fileSetId);
	}

	private string GetInProgressStatePath()
	{
		return GetInProgressStatePath(_fileSetId);
	}

	private static string GetActiveStatePath(long fileSetId)
	{
		return Path.ChangeExtension(Path.Combine(FileSetService.Instance.RootPath, fileSetId.ToString()), ".active");
	}

	private static string GetInProgressStatePath(long fileSetId)
	{
		return Path.ChangeExtension(Path.Combine(FileSetService.Instance.RootPath, fileSetId.ToString()), ".inprogress");
	}
}
