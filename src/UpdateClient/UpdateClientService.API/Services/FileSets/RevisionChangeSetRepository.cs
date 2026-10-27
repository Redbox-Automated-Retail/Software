using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Redbox.NetCore.Logging.Extensions;

namespace UpdateClientService.API.Services.FileSets;

public class RevisionChangeSetRepository : IRevisionChangeSetRepository
{
	private readonly ILogger<RevisionChangeSetRepository> _logger;

	private static SemaphoreSlim _lock = new SemaphoreSlim(1, 1);

	private readonly int _lockWait = 2000;

	private const string FileSetChangeSetExt = ".changeSet";

	private const string FileSetChangeSetPath = "changesets";

	private string ChangeSetDirectory => Path.Combine(Constants.FileSetsRoot, "changesets");

	public RevisionChangeSetRepository(ILogger<RevisionChangeSetRepository> logger)
	{
		_logger = logger;
		CreateChangeSetDirectoryIfNeeded();
	}

	public async Task<List<RevisionChangeSet>> GetAll()
	{
		List<RevisionChangeSet> result = new List<RevisionChangeSet>();
		if (!Directory.Exists(ChangeSetDirectory))
		{
			return result;
		}
		if (await _lock.WaitAsync(_lockWait))
		{
			try
			{
				string[] files = Directory.GetFiles(ChangeSetDirectory, "*.changeSet", SearchOption.TopDirectoryOnly);
				string[] array = files;
				foreach (string eachFile in array)
				{
					try
					{
						RevisionChangeSet item = (await File.ReadAllTextAsync(eachFile)).ToObject<RevisionChangeSet>();
						result.Add(item);
					}
					catch (Exception ex)
					{
						_logger.LogErrorWithSource(ex, "Exception while getting RevisionChangeSet from file " + eachFile, "GetAll", "/sln/src/UpdateClientService.API/Services/FileSets/RevisionChangeSetRepository.cs");
						Shared.SafeDelete(eachFile);
					}
				}
			}
			catch (Exception ex2)
			{
				_logger.LogErrorWithSource(ex2, "Exception while getting RevisionChangeSets", "GetAll", "/sln/src/UpdateClientService.API/Services/FileSets/RevisionChangeSetRepository.cs");
			}
			finally
			{
				_lock.Release();
			}
		}
		else
		{
			_logger.LogErrorWithSource("Lock failed.", "GetAll", "/sln/src/UpdateClientService.API/Services/FileSets/RevisionChangeSetRepository.cs");
		}
		return result;
	}

	public async Task<bool> Save(RevisionChangeSet revisionChangeSet)
	{
		bool result = false;
		if (await _lock.WaitAsync(_lockWait))
		{
			try
			{
				await File.WriteAllTextAsync(GetChangeSetPath(revisionChangeSet), revisionChangeSet.ToJson());
				result = true;
			}
			catch (Exception ex)
			{
				_logger.LogErrorWithSource(ex, "Exception while saving RevisionChangeSet", "Save", "/sln/src/UpdateClientService.API/Services/FileSets/RevisionChangeSetRepository.cs");
			}
			finally
			{
				_lock.Release();
			}
		}
		else
		{
			_logger.LogErrorWithSource("Lock failed.", "Save", "/sln/src/UpdateClientService.API/Services/FileSets/RevisionChangeSetRepository.cs");
		}
		return result;
	}

	public async Task<bool> Delete(RevisionChangeSet revisionChangeSet)
	{
		bool result = false;
		if (await _lock.WaitAsync(_lockWait))
		{
			try
			{
				string changeSetPath = GetChangeSetPath(revisionChangeSet);
				if (File.Exists(changeSetPath))
				{
					File.Delete(changeSetPath);
				}
				result = true;
			}
			catch (Exception ex)
			{
				_logger.LogErrorWithSource(ex, "Exception while deleting RevisionChangeSet " + revisionChangeSet?.ToJson(), "Delete", "/sln/src/UpdateClientService.API/Services/FileSets/RevisionChangeSetRepository.cs");
			}
			finally
			{
				_lock.Release();
			}
		}
		else
		{
			_logger.LogErrorWithSource("Lock failed.", "Delete", "/sln/src/UpdateClientService.API/Services/FileSets/RevisionChangeSetRepository.cs");
		}
		return result;
	}

	public async Task<bool> Cleanup(DateTime deleteDate)
	{
		if (!Directory.Exists(ChangeSetDirectory))
		{
			return true;
		}
		bool result = false;
		if (await _lock.WaitAsync(_lockWait))
		{
			try
			{
				string[] files = Directory.GetFiles(ChangeSetDirectory, "*.changeSet", SearchOption.TopDirectoryOnly);
				result = true;
				string[] array = files;
				foreach (string text in array)
				{
					try
					{
						DateTime creationTime = File.GetCreationTime(text);
						if (creationTime <= deleteDate)
						{
							File.Delete(text);
							_logger.LogInfoWithSource("Deleting file " + text, "Cleanup", "/sln/src/UpdateClientService.API/Services/FileSets/RevisionChangeSetRepository.cs");
						}
					}
					catch (Exception ex)
					{
						_logger.LogErrorWithSource(ex, "Exception while cleaning up RevisionChangeSet from file " + text, "Cleanup", "/sln/src/UpdateClientService.API/Services/FileSets/RevisionChangeSetRepository.cs");
						result = false;
					}
				}
			}
			catch (Exception ex2)
			{
				_logger.LogErrorWithSource(ex2, "Exception while cleaning up RevisionChangeSets", "Cleanup", "/sln/src/UpdateClientService.API/Services/FileSets/RevisionChangeSetRepository.cs");
				result = false;
			}
			finally
			{
				_lock.Release();
			}
		}
		else
		{
			_logger.LogErrorWithSource("Lock failed.", "Cleanup", "/sln/src/UpdateClientService.API/Services/FileSets/RevisionChangeSetRepository.cs");
		}
		return result;
	}

	private string GetChangeSetPath(RevisionChangeSet revisionChangeSet)
	{
		return GetChangeSetPath(revisionChangeSet?.FileSetId ?? 0, revisionChangeSet?.RevisionId ?? 0);
	}

	private string GetChangeSetPath(long fileSetId, long revisionId)
	{
		return Path.ChangeExtension(Path.Combine(ChangeSetDirectory, $"{fileSetId}-{revisionId}"), ".changeSet");
	}

	private void CreateChangeSetDirectoryIfNeeded()
	{
		try
		{
			if (!Directory.Exists(ChangeSetDirectory))
			{
				Directory.CreateDirectory(ChangeSetDirectory);
			}
		}
		catch (Exception ex)
		{
			_logger?.LogErrorWithSource(ex, "Exception creating directory " + ChangeSetDirectory, "CreateChangeSetDirectoryIfNeeded", "/sln/src/UpdateClientService.API/Services/FileSets/RevisionChangeSetRepository.cs");
		}
	}
}
