using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Redbox.NetCore.Logging.Extensions;

namespace UpdateClientService.API.Services.FileSets;

public class StateFileRepository : IStateFileRepository
{
	private readonly ILogger<StateFileRepository> _logger;

	private static SemaphoreSlim _lock = new SemaphoreSlim(1, 1);

	private readonly int _lockWait = 2000;

	private const string FileSetStateActiveExt = ".active";

	private const string FileSetStateInProgressExt = ".inprogress";

	public StateFileRepository(ILogger<StateFileRepository> logger)
	{
		_logger = logger;
	}

	public async Task<bool> Delete(long fileSetId)
	{
		return await Delete(fileSetId, new List<string> { ".active", ".inprogress" });
	}

	public async Task<bool> DeleteInProgress(long fileSetId)
	{
		return await Delete(fileSetId, new List<string> { ".inprogress" });
	}

	private async Task<bool> Delete(long fileSetId, List<string> fileExtensions)
	{
		if (await _lock.WaitAsync(_lockWait))
		{
			try
			{
				if (fileExtensions == null)
				{
					return false;
				}
				foreach (string fileExtension in fileExtensions)
				{
					Shared.SafeDelete(GetStateFilePath(fileSetId, fileExtension));
				}
				return true;
			}
			catch (Exception ex)
			{
				_logger.LogErrorWithSource(ex, $"Exception while deleting StateFile for FileSetId {fileSetId}.", "Delete", "/sln/src/UpdateClientService.API/Services/FileSets/StateFileRepository.cs");
				return false;
			}
			finally
			{
				_lock.Release();
			}
		}
		_logger.LogErrorWithSource("Lock failed.", "Delete", "/sln/src/UpdateClientService.API/Services/FileSets/StateFileRepository.cs");
		return false;
	}

	public async Task<bool> Save(StateFile stateFile)
	{
		if (stateFile == null)
		{
			_logger.LogErrorWithSource("StateFile is null", "Save", "/sln/src/UpdateClientService.API/Services/FileSets/StateFileRepository.cs");
			return false;
		}
		if (await _lock.WaitAsync(_lockWait))
		{
			try
			{
				bool flag = await SaveClientFileSetState(new ClientFileSetState
				{
					FileSetId = stateFile.FileSetId,
					RevisionId = stateFile.ActiveRevisionId,
					FileSetState = FileSetState.Active
				});
				if (flag)
				{
					flag = await SaveClientFileSetState(new ClientFileSetState
					{
						FileSetId = stateFile.FileSetId,
						RevisionId = stateFile.InProgressRevisionId,
						FileSetState = ((stateFile.InProgressRevisionId != 0L) ? stateFile.InProgressFileSetState : FileSetState.Error)
					});
				}
				return flag;
			}
			catch (Exception ex)
			{
				_logger.LogErrorWithSource(ex, $"Exception while saving StateFile for FileSetId {stateFile?.FileSetId}.", "Save", "/sln/src/UpdateClientService.API/Services/FileSets/StateFileRepository.cs");
				return false;
			}
			finally
			{
				_lock.Release();
			}
		}
		_logger.LogErrorWithSource("Lock failed.", "Save", "/sln/src/UpdateClientService.API/Services/FileSets/StateFileRepository.cs");
		return false;
	}

	private async Task<bool> SaveClientFileSetState(ClientFileSetState clientFileSetState)
	{
		if (clientFileSetState == null)
		{
			return false;
		}
		string fileExtension = ((clientFileSetState.FileSetState == FileSetState.Active) ? ".active" : ".inprogress");
		string stateFilePath = GetStateFilePath(clientFileSetState.FileSetId, fileExtension);
		if (clientFileSetState.RevisionId != 0L)
		{
			try
			{
				await File.WriteAllTextAsync(stateFilePath, clientFileSetState.ToJson());
				return true;
			}
			catch (Exception ex)
			{
				_logger.LogErrorWithSource(ex, "Exception saving StateFile to " + stateFilePath, "SaveClientFileSetState", "/sln/src/UpdateClientService.API/Services/FileSets/StateFileRepository.cs");
				return false;
			}
		}
		Shared.SafeDelete(stateFilePath);
		return true;
	}

	public async Task<(bool, StateFile)> Get(long fileSetId)
	{
		(bool, List<ClientFileSetState>) tuple = await GetClientFileSetStates(fileSetId);
		bool item = tuple.Item1;
		List<ClientFileSetState> item2 = tuple.Item2;
		StateFile item3 = (item ? CreateStateFile(item2, fileSetId) : null);
		return (item, item3);
	}

	public async Task<(bool, List<StateFile>)> GetAll()
	{
		List<StateFile> stateFiles = new List<StateFile>();
		var (item, list) = await GetClientFileSetStates();
		foreach (long item2 in list.Select((ClientFileSetState x) => x.FileSetId).Distinct())
		{
			StateFile stateFile = CreateStateFile(list, item2);
			if (stateFile != null)
			{
				stateFiles.Add(stateFile);
			}
		}
		return (item, stateFiles);
	}

	private StateFile CreateStateFile(List<ClientFileSetState> clientFileSetStates, long fileSetId)
	{
		ClientFileSetState clientFileSetState = clientFileSetStates?.FirstOrDefault((ClientFileSetState x) => x.FileSetId == fileSetId && x.FileSetState == FileSetState.Active);
		ClientFileSetState clientFileSetState2 = clientFileSetStates?.FirstOrDefault((ClientFileSetState x) => x.FileSetId == fileSetId && x.FileSetState != FileSetState.Active);
		if (clientFileSetState == null && clientFileSetState2 == null)
		{
			return null;
		}
		return new StateFile(fileSetId, clientFileSetState?.RevisionId ?? 0, clientFileSetState2?.RevisionId ?? 0, clientFileSetState2?.FileSetState ?? FileSetState.Active);
	}

	private async Task<(bool, List<ClientFileSetState>)> GetClientFileSetStates(long? fileSetId = null)
	{
		List<ClientFileSetState> stateFiles = new List<ClientFileSetState>();
		if (await _lock.WaitAsync(_lockWait))
		{
			try
			{
				if (!Directory.Exists(Constants.FileSetsRoot))
				{
					_logger.LogErrorWithSource("StateFile directory " + Constants.FileSetsRoot + " does not exist.", "GetClientFileSetStates", "/sln/src/UpdateClientService.API/Services/FileSets/StateFileRepository.cs");
					return (false, stateFiles);
				}
				List<string> list = new List<string>();
				string text = (fileSetId.HasValue ? fileSetId.ToString() : "*");
				list.AddRange(Directory.GetFiles(Constants.FileSetsRoot, text + ".active", SearchOption.TopDirectoryOnly).ToList());
				list.AddRange(Directory.GetFiles(Constants.FileSetsRoot, text + ".inprogress", SearchOption.TopDirectoryOnly).ToList());
				foreach (string item in list)
				{
					ClientFileSetState clientFileSetState = await Load(item);
					if (clientFileSetState != null)
					{
						stateFiles.Add(clientFileSetState);
					}
				}
			}
			catch (Exception ex)
			{
				_logger.LogErrorWithSource(ex, "Exception while getting StateFiles.", "GetClientFileSetStates", "/sln/src/UpdateClientService.API/Services/FileSets/StateFileRepository.cs");
				return (false, stateFiles);
			}
			finally
			{
				_lock.Release();
			}
		}
		else
		{
			_logger.LogErrorWithSource("Lock failed.", "GetClientFileSetStates", "/sln/src/UpdateClientService.API/Services/FileSets/StateFileRepository.cs");
		}
		return (true, stateFiles);
	}

	private async Task<ClientFileSetState> Load(string filePath)
	{
		ClientFileSetState result = null;
		try
		{
			if (!string.IsNullOrEmpty(await File.ReadAllTextAsync(filePath)))
			{
				try
				{
					result = File.ReadAllText(filePath).ToObject<ClientFileSetState>();
				}
				catch (Exception ex)
				{
					_logger.LogErrorWithSource(ex, "Exception while deserializing StateFile " + filePath, "Load", "/sln/src/UpdateClientService.API/Services/FileSets/StateFileRepository.cs");
				}
			}
			else
			{
				_logger.LogErrorWithSource("StateFile " + filePath + " was null", "Load", "/sln/src/UpdateClientService.API/Services/FileSets/StateFileRepository.cs");
			}
		}
		catch (Exception ex2)
		{
			_logger.LogErrorWithSource(ex2, "Exception while reading file " + filePath, "Load", "/sln/src/UpdateClientService.API/Services/FileSets/StateFileRepository.cs");
		}
		if (result == null)
		{
			Shared.SafeDelete(filePath);
		}
		return result;
	}

	private static string GetStateFilePath(long fileSetId, string fileExtension)
	{
		return Path.ChangeExtension(Path.Combine(Constants.FileSetsRoot, fileSetId.ToString()), fileExtension);
	}
}
