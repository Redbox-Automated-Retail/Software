using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using Redbox.Core;
using Redbox.IPC.Framework;
using Redbox.UpdateManager.ComponentModel;
using Redbox.UpdateService.Client;
using Redbox.UpdateService.Model;

namespace Redbox.UpdateManager.Remoting;

public class UpdateService : IUpdateService
{
	private class ResultEntry
	{
		public string Entry { get; set; }

		public long ID { get; set; }
	}

	private string m_url;

	private TimeSpan m_timeout = TimeSpan.FromSeconds(30.0);

	private const string ResultTableName = "Results";

	private const string InCompleteLabel = ".incomplete";

	private const string ScriptLabel = "script";

	private string m_storeNumber;

	private int m_minimumRetryDelay = 60;

	private const string WorkExtension = ".workdat";

	public static UpdateService Instance => Singleton<UpdateService>.Instance;

	public void Initialize(string url, TimeSpan timeout, string storeNumber, int minimumRetryDelay)
	{
		m_url = url;
		m_storeNumber = storeNumber;
		m_timeout = timeout;
		m_minimumRetryDelay = ((minimumRetryDelay < 60) ? 60 : minimumRetryDelay);
		ServiceLocator.Instance.AddService(typeof(IUpdateService), this);
	}

	public Redbox.UpdateManager.ComponentModel.ErrorList ForcePublish(string name)
	{
		using Redbox.UpdateService.Client.UpdateService updateService = GetService();
		return IPC(updateService.ForcePublishRun(name).Errors);
	}

	public Redbox.UpdateManager.ComponentModel.ErrorList GetSubscriptionState(string name, out DateTime lastRun, out Redbox.UpdateManager.ComponentModel.SubscriptionState state)
	{
		lastRun = DateTime.Now;
		state = Redbox.UpdateManager.ComponentModel.SubscriptionState.Idle;
		using Redbox.UpdateService.Client.UpdateService updateService = GetService();
		ClientCommandResult subscriptionStatus = updateService.GetSubscriptionStatus(name, out var state2);
		if (subscriptionStatus.Success)
		{
			lastRun = state2.ModifiedOn;
			state = ((state2.State != Redbox.UpdateService.Client.SubscriptionState.Processing) ? Redbox.UpdateManager.ComponentModel.SubscriptionState.Idle : Redbox.UpdateManager.ComponentModel.SubscriptionState.Processing);
		}
		return IPC(subscriptionStatus.Errors);
	}

	public Redbox.UpdateManager.ComponentModel.ErrorList UploadFile(string path)
	{
		return new Redbox.UpdateManager.ComponentModel.ErrorList();
	}

	public Redbox.UpdateManager.ComponentModel.ErrorList AddGroupToRepository(string group, string name)
	{
		Redbox.UpdateManager.ComponentModel.ErrorList errorList = new Redbox.UpdateManager.ComponentModel.ErrorList();
		using Redbox.UpdateService.Client.UpdateService updateService = GetService();
		ClientCommandResult clientCommandResult = updateService.LinkRepositoryToGroup(group, name, "UpdateService.Kernel", "Update Service", out var _);
		if (!clientCommandResult.Success)
		{
			errorList.AddRange(IPC(clientCommandResult.Errors));
		}
		return errorList;
	}

	public Redbox.UpdateManager.ComponentModel.ErrorList RemoveGroupFromRepository(string group, string name)
	{
		Redbox.UpdateManager.ComponentModel.ErrorList errorList = new Redbox.UpdateManager.ComponentModel.ErrorList();
		using Redbox.UpdateService.Client.UpdateService updateService = GetService();
		ClientCommandResult clientCommandResult = updateService.UnlinkRepositoryToGroup(group, name, "UpdateService.Kernel", "Update Service", out var _);
		if (!clientCommandResult.Success)
		{
			errorList.AddRange(IPC(clientCommandResult.Errors));
		}
		return errorList;
	}

	public Redbox.UpdateManager.ComponentModel.ErrorList AddStoreToRepository(string number, string name)
	{
		Redbox.UpdateManager.ComponentModel.ErrorList errorList = new Redbox.UpdateManager.ComponentModel.ErrorList();
		using Redbox.UpdateService.Client.UpdateService updateService = GetService();
		ClientCommandResult clientCommandResult = updateService.LinkRepositoryToStore(number, name, "UpdateService.Kernel", "Update Service", out var _);
		if (!clientCommandResult.Success)
		{
			errorList.AddRange(IPC(clientCommandResult.Errors));
		}
		return errorList;
	}

	public Redbox.UpdateManager.ComponentModel.ErrorList RemoveStoreFromRepository(string number, string name)
	{
		Redbox.UpdateManager.ComponentModel.ErrorList errorList = new Redbox.UpdateManager.ComponentModel.ErrorList();
		using Redbox.UpdateService.Client.UpdateService updateService = GetService();
		ClientCommandResult clientCommandResult = updateService.UnlinkRepositoryToStore(number, name, "UpdateService.Kernel", "Update Service", out var _);
		if (!clientCommandResult.Success)
		{
			errorList.AddRange(IPC(clientCommandResult.Errors));
		}
		return errorList;
	}

	public Redbox.UpdateManager.ComponentModel.ErrorList ProcessChangeSet(List<ChangeSet> changeSets, StoreScheduleInfo storeScheduleInfo)
	{
		LogHelper.Instance.Log($"Processing {changeSets.Count} changes in changeset from poll reply", LogEntryType.Info);
		Redbox.UpdateManager.ComponentModel.ErrorList errorList = new Redbox.UpdateManager.ComponentModel.ErrorList();
		try
		{
			ITransferService transferService = ServiceLocator.Instance.GetService<ITransferService>();
			IDataStoreService dataStoreService = ServiceLocator.Instance.GetService<IDataStoreService>();
			IRepositoryService repositoryService = ServiceLocator.Instance.GetService<IRepositoryService>();
			if (!changeSets.Any())
			{
				errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("PCS303", "There were no changes in the changeset", "No changeset to process, exiting ProcessChangeSet"));
				return errorList;
			}
			if (transferService.SetEndOfScheduleInHoursFromMidnight(storeScheduleInfo.EndOfScheduleInHoursFromMidnight) || transferService.SetStartOfScheduleInHoursFromMidnight(storeScheduleInfo.StartOfScheduleInHoursFromMidnight) || transferService.SetMaxBandwidthWhileOutsideOfSchedule(storeScheduleInfo.MaxBandwidthWhileOutsideOfSchedule) || transferService.SetMaxBandwidthWhileWithInSchedule(storeScheduleInfo.MaxBandwidthWhileWithInSchedule))
			{
				using ServiceController serviceController = new ServiceController("BITS");
				serviceController.Stop();
				serviceController.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromMinutes(10.0));
				serviceController.Start();
				serviceController.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromMinutes(10.0));
			}
			transferService.GetRepositoriesInTransit(out var inTransit);
			foreach (ChangeSet changeSet in changeSets)
			{
				if (inTransit.Contains(changeSet.Name) || !repositoryService.Subscribed(changeSet.Name))
				{
					continue;
				}
				ITransferJob job = null;
				try
				{
					if (!repositoryService.ContainsRepository(changeSet.Name))
					{
						repositoryService.AddRepository(changeSet.Name);
					}
					string jobName = $"<~{changeSet.Name}-{changeSet.Head.Substring(0, 8)}~>";
					errorList.AddRange(transferService.CreateDownloadJob(jobName, out job));
					errorList.AddRange(job.SetMinimumRetryDelay((uint)m_minimumRetryDelay));
					errorList.AddRange(job.SetNoProgressTimeout(storeScheduleInfo.NoProgressTimeout));
					errorList.AddRange(job.AddItem(changeSet.Url, Path.GetTempFileName()));
					if (errorList.ContainsError())
					{
						errorList.AddRange(job.Cancel());
						continue;
					}
					errorList.AddRange(job.Resume());
					if (!errorList.ContainsError())
					{
						dataStoreService.Set(job.ID, changeSet);
						LogHelper.Instance.Log("BITS job started with name: {0} and guid: {1}.", jobName, job.ID);
					}
				}
				catch (Exception e)
				{
					if (job != null)
					{
						errorList.AddRange(job.Cancel());
					}
					errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("E999", "An unhandled exception occurred in Updater.StartUpdate while creating job for a change set.", e));
				}
			}
		}
		catch (Exception e2)
		{
			errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("E999", "An unhandled exception occurred in Updater.StartUpdate. The Guid on the Error Report is not the BITS job id.", e2));
		}
		return errorList;
	}

	public Redbox.UpdateManager.ComponentModel.ErrorList StartDownloads()
	{
		Redbox.UpdateManager.ComponentModel.ErrorList errorList = new Redbox.UpdateManager.ComponentModel.ErrorList();
		try
		{
			ITransferService transferService = ServiceLocator.Instance.GetService<ITransferService>();
			IDataStoreService dataStoreService = ServiceLocator.Instance.GetService<IDataStoreService>();
			IRepositoryService repositoryService = ServiceLocator.Instance.GetService<IRepositoryService>();
			List<string> allRepositories = repositoryService.GetAllRepositories();
			Dictionary<string, string> headRevisions = new Dictionary<string, string>();
			foreach (string repository in allRepositories)
			{
				if (!string.IsNullOrEmpty(repositoryService.GetHeadRevision(repository)))
				{
					headRevisions.Add(repository, repositoryService.GetHeadRevision(repository));
				}
			}
			Store store;
			List<ChangeSet> jobs;
			using (Redbox.UpdateService.Client.UpdateService updateService = GetService())
			{
				ClientCommandResult changeSet = updateService.GetChangeSet(m_storeNumber, headRevisions, out store, out jobs);
				if (!changeSet.Success)
				{
					errorList.AddRange(IPC(changeSet.Errors));
					return errorList;
				}
			}
			if (jobs.Count < 1)
			{
				return errorList;
			}
			if (errorList.ContainsError())
			{
				return errorList;
			}
			if (transferService.SetEndOfScheduleInHoursFromMidnight(store.EndOfScheduleInHoursFromMidnight) || transferService.SetEndOfScheduleInHoursFromMidnight(store.EndOfScheduleInHoursFromMidnight) || transferService.SetStartOfScheduleInHoursFromMidnight(store.StartOfScheduleInHoursFromMidnight) || transferService.SetMaxBandwidthWhileOutsideOfSchedule((int)store.MaxBandwidthWhileOutsideOfSchedule) || transferService.SetMaxBandwidthWhileWithInSchedule((int)store.MaxBandwidthWhileWithInSchedule))
			{
				using ServiceController serviceController = new ServiceController("BITS");
				serviceController.Stop();
				serviceController.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromMinutes(10.0));
				serviceController.Start();
				serviceController.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromMinutes(10.0));
			}
			transferService.GetRepositoriesInTransit(out var inTransit);
			foreach (ChangeSet pendingChangeSet in jobs)
			{
				if (inTransit.Contains(pendingChangeSet.Name) || !repositoryService.Subscribed(pendingChangeSet.Name))
				{
					continue;
				}
				ITransferJob job = null;
				try
				{
					if (!repositoryService.ContainsRepository(pendingChangeSet.Name))
					{
						repositoryService.AddRepository(pendingChangeSet.Name);
					}
					string jobName = $"<~{pendingChangeSet.Name}-{pendingChangeSet.Head.Substring(0, 8)}~>";
					errorList.AddRange(transferService.CreateDownloadJob(jobName, out job));
					errorList.AddRange(job.SetMinimumRetryDelay((uint)m_minimumRetryDelay));
					errorList.AddRange(job.SetNoProgressTimeout(store.NoProgressTimeout));
					errorList.AddRange(job.AddItem(pendingChangeSet.Url, Path.GetTempFileName()));
					if (errorList.ContainsError())
					{
						errorList.AddRange(job.Cancel());
						continue;
					}
					errorList.AddRange(job.Resume());
					if (!errorList.ContainsError())
					{
						dataStoreService.Set(job.ID, pendingChangeSet);
						LogHelper.Instance.Log("BITS job started with name: {0} and guid: {1}.", jobName, job.ID);
					}
				}
				catch (Exception e)
				{
					if (job != null)
					{
						errorList.AddRange(job.Cancel());
					}
					errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("E999", "An unhandled exception occurred in UpdateService.StartDownloads while creating job for a change set.", e));
				}
			}
		}
		catch (Exception e2)
		{
			errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("E999", "An unhandled exception occurred in UpdateService.StartDownloads.", e2));
		}
		return errorList;
	}

	public Redbox.UpdateManager.ComponentModel.ErrorList FinishDownloads()
	{
		Redbox.UpdateManager.ComponentModel.ErrorList errorList = new Redbox.UpdateManager.ComponentModel.ErrorList();
		ITransferService transferService = ServiceLocator.Instance.GetService<ITransferService>();
		IDataStoreService dataStoreService = ServiceLocator.Instance.GetService<IDataStoreService>();
		errorList.AddRange(transferService.GetJobs(out var jobs, allUsers: false));
		if (errorList.ContainsError())
		{
			return errorList;
		}
		foreach (ITransferJob job in jobs)
		{
			if (!job.Name.StartsWith("<~") || !job.Name.EndsWith("~>"))
			{
				continue;
			}
			switch (job.Status)
			{
			case TransferStatus.Error:
			case TransferStatus.Transferred:
				errorList.AddRange((job.JobType == TransferJobType.Download) ? FinishDownload(job.ID) : FinishUpload(job.ID));
				continue;
			case TransferStatus.Suspended:
				errorList.AddRange(job.Cancel());
				continue;
			}
			if (dataStoreService.Get<ChangeSet>(job.ID) == null)
			{
				errorList.AddRange(job.Cancel());
			}
		}
		if (jobs.Count == 0)
		{
			dataStoreService.CleanUp();
		}
		return errorList;
	}

	public Redbox.UpdateManager.ComponentModel.ErrorList FinishUpload(Guid jobId)
	{
		Redbox.UpdateManager.ComponentModel.ErrorList errorList = new Redbox.UpdateManager.ComponentModel.ErrorList();
		ITransferService transferService = ServiceLocator.Instance.GetService<ITransferService>();
		IDataStoreService dataStoreService = ServiceLocator.Instance.GetService<IDataStoreService>();
		IQueueService queueService = ServiceLocator.Instance.GetService<IQueueService>();
		try
		{
			Upload upload = dataStoreService.Get<Upload>(jobId);
			if (upload == null)
			{
				errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("M001", $"Job file for job: {jobId}", "Job file is missing the bits jobs will be canceled."));
				errorList.AddRange(transferService.GetJob(jobId, out var job));
				if (job != null && !errorList.ContainsError())
				{
					errorList.AddRange(job.Cancel());
				}
				return errorList;
			}
			errorList.AddRange(transferService.GetJob(jobId, out var job2));
			if (errorList.ContainsError())
			{
				return errorList;
			}
			UploadReport entry;
			if (job2.Status == TransferStatus.Error)
			{
				entry = new UploadReport
				{
					AverageSpeedInKPS = 0.0,
					ID = upload.ID,
					End = (job2.FinishTime.HasValue ? job2.FinishTime.Value : DateTime.UtcNow),
					StoreNumber = m_storeNumber,
					Success = false,
					TotalBytesTransfered = job2.TotalBytesTransfered
				};
				errorList.AddRange(job2.Cancel());
			}
			else
			{
				DateTime dateTime = (job2.FinishTime.HasValue ? job2.FinishTime.Value : DateTime.UtcNow);
				double averageSpeedInKPS = (double)job2.TotalBytesTransfered / (dateTime - job2.StartTime).TotalSeconds;
				entry = new UploadReport
				{
					AverageSpeedInKPS = averageSpeedInKPS,
					ID = upload.ID,
					End = (job2.FinishTime.HasValue ? job2.FinishTime.Value : DateTime.UtcNow),
					StoreNumber = m_storeNumber,
					Success = true,
					TotalBytesTransfered = job2.TotalBytesTransfered
				};
				errorList.AddRange(job2.Complete());
			}
			queueService.Enqueue("upload-report", entry);
			dataStoreService.Delete(jobId);
			return errorList;
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("E999", "An unhandled exception occurred in Updater.FinishUpdate.", e));
			return errorList;
		}
	}

	public Redbox.UpdateManager.ComponentModel.ErrorList FinishDownload(Guid jobId)
	{
		Redbox.UpdateManager.ComponentModel.ErrorList errorList = new Redbox.UpdateManager.ComponentModel.ErrorList();
		ITransferService transferService = ServiceLocator.Instance.GetService<ITransferService>();
		IRepositoryService repositoryService = ServiceLocator.Instance.GetService<IRepositoryService>();
		IDataStoreService dataStoreService = ServiceLocator.Instance.GetService<IDataStoreService>();
		IQueueService queueService = ServiceLocator.Instance.GetService<IQueueService>();
		ChangeSet changeSet;
		ITransferJob job2;
		try
		{
			changeSet = dataStoreService.Get<ChangeSet>(jobId);
			if (changeSet == null)
			{
				errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("M001", $"Job file for job: {jobId}", "Job file is missing the bits jobs will be canceled."));
				errorList.AddRange(transferService.GetJob(jobId, out var job));
				if (job != null)
				{
					errorList.AddRange((job.Status == TransferStatus.Transferred) ? job.Complete() : job.Cancel());
				}
				return errorList;
			}
			errorList.AddRange(transferService.GetJob(jobId, out job2));
			if (errorList.ContainsError())
			{
				return errorList;
			}
			if (job2.Status == TransferStatus.Error)
			{
				TransferStatisticReport entry = new TransferStatisticReport
				{
					Repository = changeSet.Name,
					AverageSpeedInKPS = 0.0,
					ChangeSet = changeSet.Head,
					End = (job2.FinishTime.HasValue ? job2.FinishTime.Value : DateTime.UtcNow),
					StoreNumber = m_storeNumber,
					Success = false,
					TotalBytesTransfered = job2.TotalBytesTransfered
				};
				queueService.Enqueue("statistic", entry);
				errorList.AddRange(job2.Cancel());
				return errorList;
			}
			errorList.AddRange(job2.GetItems(out var items));
			if (errorList.ContainsError())
			{
				return errorList;
			}
			if (items.Count != 1)
			{
				errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("D999", $"Job {job2.ID} has {items.Count} items. It have 1 deltaItem.", "This download will be canceled."));
				dataStoreService.Delete(jobId);
				return errorList;
			}
			errorList.AddRange(job2.Complete());
			if (errorList.ContainsError())
			{
				job2.Cancel();
				dataStoreService.Delete(jobId);
				return errorList;
			}
			ITransferItem transferItem = items[0];
			string downloadedFile = Path.Combine(transferItem.Path, transferItem.Name);
			if (!string.IsNullOrEmpty(changeSet.ContentHash))
			{
				using FileStream inputStream = File.OpenRead(downloadedFile);
				string contentHash = inputStream.ToASCIISHA1Hash();
				if (contentHash != changeSet.ContentHash)
				{
					errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("D999", $"{downloadedFile} should have hash {changeSet.ContentHash} but it hash {contentHash}", "This download failed and will need to be restarted."));
					return errorList;
				}
				LogHelper.Instance.Log("{0} matched the server hash of {1}.", downloadedFile, changeSet.ContentHash);
			}
			repositoryService.UnpackChangeSet(changeSet.Name, jobId, downloadedFile);
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("E999", "An unhandled exception occurred in Updater.FinishUpdate.", e));
			dataStoreService.Delete(jobId);
			return errorList;
		}
		DateTime dateTime = (job2.FinishTime.HasValue ? job2.FinishTime.Value : DateTime.UtcNow);
		double averageSpeedInKPS = (double)job2.TotalBytesTransfered / (dateTime - job2.StartTime).TotalSeconds;
		try
		{
			repositoryService.AddRevisions(changeSet.Name, changeSet.Revisions);
			if (!string.IsNullOrEmpty(changeSet.Label))
			{
				repositoryService.SetLabel(changeSet.Name, changeSet.Revisions[changeSet.Revisions.Count - 1], changeSet.Label);
			}
			List<DeltaItem> deltaItems = new List<DeltaItem>();
			foreach (ChangeSetItem changeSetItem in changeSet.Items)
			{
				DeltaItem deltaItem = new DeltaItem
				{
					Revision = changeSetItem.Revision,
					IsSeed = changeSetItem.IsFirst,
					IsPlaceHolder = changeSetItem.Composite,
					ContentHash = changeSetItem.ContentHash,
					TargetName = changeSetItem.TargetName,
					TargetPath = changeSetItem.TargetPath,
					VersionHash = changeSetItem.VersionHash
				};
				deltaItems.Add(deltaItem);
			}
			repositoryService.AddDelta(changeSet.Name, deltaItems);
			LogHelper.Instance.Log("Finished Add patch for ChangeSetName: {0} TransferJobId: {1}", changeSet.Name, job2.ID);
			TransferStatisticReport entry2 = new TransferStatisticReport
			{
				Repository = changeSet.Name,
				AverageSpeedInKPS = averageSpeedInKPS,
				ChangeSet = changeSet.Head,
				End = (job2.FinishTime.HasValue ? job2.FinishTime.Value : DateTime.UtcNow),
				StoreNumber = m_storeNumber,
				Success = true,
				TotalBytesTransfered = job2.TotalBytesTransfered
			};
			queueService.Enqueue("statistic", entry2);
		}
		catch (Exception e2)
		{
			errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("E999", "Error adding delta to repository.", e2));
			repositoryService.RemoveRevisions(changeSet.Name, changeSet.Revisions);
			TransferStatisticReport entry3 = new TransferStatisticReport
			{
				Repository = changeSet.Name,
				AverageSpeedInKPS = averageSpeedInKPS,
				ChangeSet = changeSet.Head,
				End = (job2.FinishTime.HasValue ? job2.FinishTime.Value : DateTime.UtcNow),
				StoreNumber = m_storeNumber,
				TotalBytesTransfered = job2.TotalBytesTransfered
			};
			queueService.Enqueue("statistic", entry3);
		}
		dataStoreService.Delete(jobId);
		return errorList;
	}

	public Redbox.UpdateManager.ComponentModel.ErrorList ServerPoll()
	{
		return ServiceLocator.Instance.GetService<IPollService>().ServerPoll();
	}

	public Redbox.UpdateManager.ComponentModel.ErrorList Poll()
	{
		Redbox.UpdateManager.ComponentModel.ErrorList errorList = new Redbox.UpdateManager.ComponentModel.ErrorList();
		errorList.AddRange(FinishDownloads());
		errorList.AddRange(StartDownloads());
		return errorList;
	}

	public Redbox.UpdateManager.ComponentModel.ErrorList ClearWorkQueue()
	{
		ServiceLocator.Instance.GetService<IDataStoreService>().Delete(".incomplete");
		return new Redbox.UpdateManager.ComponentModel.ErrorList();
	}

	public Redbox.UpdateManager.ComponentModel.ErrorList DeleteFromWorkQueue(string name)
	{
		Redbox.UpdateManager.ComponentModel.ErrorList errorList = new Redbox.UpdateManager.ComponentModel.ErrorList();
		IDataStoreService dataStore = ServiceLocator.Instance.GetService<IDataStoreService>();
		try
		{
			List<Guid> incompleteIds = dataStore.Get<List<Guid>>(".incomplete");
			for (int i = 0; i < incompleteIds.Count; i++)
			{
				if (dataStore.Get<Work>(string.Concat(incompleteIds[i], ".workdat")).ID == Convert.ToInt64(name))
				{
					incompleteIds.RemoveAt(i);
					dataStore.Set(".incomplete", incompleteIds);
					// After RemoveAt, incompleteIds[i] is the next entry: this deletes that entry's .workdat (or throws
					// on the last one) and leaves the removed one's file behind. Kept as shipped.
					dataStore.Delete(string.Concat(incompleteIds[i], ".workdat"));
					return errorList;
				}
			}
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("E999", "An unhandled exception occurred in Updater.DoWork.", e));
		}
		return errorList;
	}

	public Redbox.UpdateManager.ComponentModel.ErrorList StartInstaller(string repositoryHash, string frontEndVersion, out Dictionary<string, string> response)
	{
		Redbox.UpdateManager.ComponentModel.ErrorList errorList = new Redbox.UpdateManager.ComponentModel.ErrorList();
		response = new Dictionary<string, string>();
		using (Redbox.UpdateService.Client.UpdateService updateService = GetService())
		{
			ClientCommandResult clientCommandResult = updateService.StartStoreInstaller("installer", repositoryHash, frontEndVersion);
			if (clientCommandResult.Success)
			{
				response = clientCommandResult.CommandMessages[0].ToDictionary().ToDictionary((KeyValuePair<string, object> k) => k.Key, (KeyValuePair<string, object> v) => v.Value as string);
			}
			else
			{
				LogHelper.Instance.Log("The update service could not be contacted in StartInstaller.");
				errorList.AddRange(clientCommandResult.Errors.Select((Redbox.IPC.Framework.Error e) => Redbox.UpdateManager.ComponentModel.Error.NewError(e.Code, e.Description, e.Details)));
			}
		}
		return errorList;
	}

	public Redbox.UpdateManager.ComponentModel.ErrorList FinishInstaller(string guid, Dictionary<string, string> data)
	{
		Redbox.UpdateManager.ComponentModel.ErrorList errorList = new Redbox.UpdateManager.ComponentModel.ErrorList();
		using Redbox.UpdateService.Client.UpdateService updateService = GetService();
		foreach (KeyValuePair<string, string> datum in data)
		{
			int attempt = 1;
			ClientCommandResult clientCommandResult;
			do
			{
				LogHelper.Instance.Log($"Sending log to server try #{attempt}: {datum.Key}:{datum.Value} ");
				clientCommandResult = updateService.InstallerLog("installer", guid, datum.Key, datum.Value);
				if (!clientCommandResult.Success)
				{
					errorList.AddRange(clientCommandResult.Errors.Select((Redbox.IPC.Framework.Error e) => Redbox.UpdateManager.ComponentModel.Error.NewError(e.Code, e.Description, e.Details)));
				}
				attempt++;
			}
			while (!clientCommandResult.Success && attempt <= 3);
		}
		return errorList;
	}

	public Redbox.UpdateManager.ComponentModel.ErrorList FinishInstaller(string guid, string name, string value)
	{
		Redbox.UpdateManager.ComponentModel.ErrorList errorList = new Redbox.UpdateManager.ComponentModel.ErrorList();
		using (Redbox.UpdateService.Client.UpdateService updateService = GetService())
		{
			LogHelper.Instance.Log($"Sending log to server: {name}:{value} ");
			ClientCommandResult clientCommandResult = updateService.InstallerLog("installer", guid, name, value);
			if (!clientCommandResult.Success)
			{
				errorList.AddRange(clientCommandResult.Errors.Select((Redbox.IPC.Framework.Error e) => Redbox.UpdateManager.ComponentModel.Error.NewError(e.Code, e.Description, e.Details)));
			}
		}
		return errorList;
	}

	public Redbox.UpdateManager.ComponentModel.ErrorList ExecuteScriptFile(string path, out string result)
	{
		return ExecuteScriptFile(path, out result, reset: false);
	}

	public Redbox.UpdateManager.ComponentModel.ErrorList ExecuteScriptFile(string path, out string result, bool reset)
	{
		Redbox.UpdateManager.ComponentModel.ErrorList errorList = new Redbox.UpdateManager.ComponentModel.ErrorList();
		result = string.Empty;
		string path2 = ServiceLocator.Instance.GetService<IMacroService>().ExpandProperties(path);
		if (!File.Exists(path2))
		{
			errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("US9987", "Script path does not exist.", $"Correct Path {path}"));
			return errorList;
		}
		string script = File.ReadAllText(path2);
		return ExecuteScript(script, out result, reset);
	}

	public Redbox.UpdateManager.ComponentModel.ErrorList ExecuteScript(string script, out string result)
	{
		return ExecuteScript(script, out result, reset: false);
	}

	public Redbox.UpdateManager.ComponentModel.ErrorList ExecuteScript(string script, out string result, bool reset)
	{
		Redbox.UpdateManager.ComponentModel.ErrorList errorList = new Redbox.UpdateManager.ComponentModel.ErrorList();
		result = string.Empty;
		IKernelService kernelService = ServiceLocator.Instance.GetService<IKernelService>();
		try
		{
			kernelService.ExecuteChunk(script, reset);
			Dictionary<object, object> table = kernelService.GetTable("Results");
			if (table != null)
			{
				result = table.ToJson();
			}
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("US9986", "Error: unhandled exception in script.", e));
			LogHelper.Instance.Log("(US9986), Error: unhandled exception in script.", e);
		}
		return errorList;
	}

	public Redbox.UpdateManager.ComponentModel.ErrorList DoInCompleteWork(IEnumerable<Guid> ids)
	{
		Redbox.UpdateManager.ComponentModel.ErrorList errorList = new Redbox.UpdateManager.ComponentModel.ErrorList();
		List<Guid> remainingIds = new List<Guid>(ids);
		try
		{
			IDataStoreService dataStore = ServiceLocator.Instance.GetService<IDataStoreService>();
			IQueueService queueService = ServiceLocator.Instance.GetService<IQueueService>();
			foreach (Guid id in ids)
			{
				Work work = dataStore.Get<Work>(string.Concat(id, ".workdat"));
				IKernelService kernelService = ServiceLocator.Instance.GetService<IKernelService>();
				Exception ex = null;
				try
				{
					LogHelper.Instance.Log("Executing incomplete work item with script id: {0}", work.ID);
					kernelService.ExecuteChunk(work.GetScriptText(), reset: true);
				}
				catch (Exception ex2)
				{
					errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("E999", "An unhandled exception occurred in Updater.DoWork.ExecuteChunk", ex2));
					ex = ex2;
				}
				if (ex != null)
				{
					LogHelper.Instance.Log("Script Id: {0} caused an exception.", work.ID);
					Redbox_002EUpdateManager_002ERemoting_002E_003C_003Ef__AnonymousType0<bool, bool, string> failure = new Redbox_002EUpdateManager_002ERemoting_002E_003C_003Ef__AnonymousType0<bool, bool, string>(Result: false, Exception: true, ex.Message);
					queueService.Enqueue("script", new ResultEntry
					{
						ID = work.ID,
						Entry = failure.ToJson()
					});
					remainingIds.Remove(id);
					if (remainingIds.Count == 0)
					{
						dataStore.Delete(".incomplete");
					}
					else
					{
						dataStore.Set(".incomplete", remainingIds);
					}
					dataStore.Delete(string.Concat(id, ".workdat"));
					LogHelper.Instance.Log("Script id: {0} incomplete work removed", work.ID);
				}
				else if (kernelService.ScriptCompleted)
				{
					remainingIds.Remove(id);
					if (remainingIds.Count == 0)
					{
						dataStore.Delete(".incomplete");
					}
					else
					{
						dataStore.Set(".incomplete", remainingIds);
					}
					dataStore.Delete(string.Concat(id, ".workdat"));
					LogHelper.Instance.Log("Script id: {0} completed", work.ID);
					Dictionary<object, object> table = kernelService.GetTable("Results");
					if (table != null)
					{
						string entry = table.ToJson();
						queueService.Enqueue("script", new ResultEntry
						{
							ID = work.ID,
							Entry = entry
						});
					}
				}
				else
				{
					LogHelper.Instance.Log("Script id: {0} is still incomplete.", work.ID);
				}
			}
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("E999", "An unhandled exception occurred in Updater.DoWork.", e));
		}
		return errorList;
	}

	public string StoreNumber()
	{
		return m_storeNumber;
	}

	private UpdateService()
	{
	}

	private Redbox.UpdateService.Client.UpdateService GetService()
	{
		return Redbox.UpdateService.Client.UpdateService.GetService(m_url, Convert.ToInt32(m_timeout.TotalMilliseconds));
	}

	private static Redbox.UpdateManager.ComponentModel.ErrorList IPC(IEnumerable<Redbox.IPC.Framework.Error> errors)
	{
		Redbox.UpdateManager.ComponentModel.ErrorList errorList = new Redbox.UpdateManager.ComponentModel.ErrorList();
		foreach (Redbox.IPC.Framework.Error error in errors)
		{
			errorList.Add(error.IsWarning ? Redbox.UpdateManager.ComponentModel.Error.NewWarning(error.Code, error.Details, error.Details) : Redbox.UpdateManager.ComponentModel.Error.NewError(error.Code, error.Details, error.Details));
		}
		return errorList;
	}
}
