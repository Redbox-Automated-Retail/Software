using System;
using System.Collections.Generic;
using System.IO;
using Redbox.Core;
using Redbox.UpdateManager.BITS;
using Redbox.UpdateManager.ComponentModel;
using Redbox.UpdateManager.Environment;
using Redbox.UpdateManager.Kernel;
using Redbox.UpdateManager.Remoting;
using Redbox.UpdateManager.ServiceProxies;
using Redbox.UpdateManager.TaskScheduler;
using Redbox.UpdateManager.Tool.Properties;

namespace Redbox.UpdateManager.Tool;

public class Updater
{
	private const string ServiceApplicationId = "{C423B0E3-95B3-4191-B326-AB3EA516F825}";

	public string StoreNumber { get; set; }

	public ErrorList Initialize(string updateServiceUrl)
	{
		ErrorList result = new ErrorList();
		MacroService macroService = new MacroService();
		ServiceLocator.Instance.AddService(typeof(IMacroService), macroService);
		string directoryName = Path.GetDirectoryName(typeof(Updater).Assembly.Location);
		macroService["StoreNumber"] = StoreNumber;
		macroService["ProgramFiles"] = "C:\\Program Files";
		macroService["REDS"] = "C:\\Program Files\\Redbox\\REDS";
		macroService["data"] = "C:\\Program Files\\Redbox\\REDS\\data";
		macroService["RedboxProgramFiles"] = "C:\\Program Files\\Redbox";
		macroService["engine"] = "C:\\Program Files\\Redbox\\REDS\\Kiosk Engine";
		macroService["Scripts"] = Path.Combine(directoryName, "..\\scripts");
		macroService["WorkingDirectory"] = directoryName;
		WindowsTaskScheduler.Instance.Initialize();
		KioskEngineService.Instance.Initialize(Settings.Default.KioskEnginePath, Settings.Default.KioskEngineUrl);
		Redbox.UpdateManager.Remoting.UpdateService.Instance.Initialize(updateServiceUrl, TimeSpan.FromSeconds(120.0), StoreNumber, Settings.Default.MinimumRetryDelay);
		KernelService.Instance.Initialize();
		if (IsServiceRunning())
		{
			DataStoreServiceProxy.Instance.Initialize(Settings.Default.UpdateManagerUrl);
			QueueServiceProxy.Instance.Initialize(Settings.Default.UpdateManagerUrl);
			TransferServiceProxy.Instance.Initialize(Settings.Default.UpdateManagerUrl);
			RepositoryServiceProxy.Instance.Initialize(Settings.Default.UpdateManagerUrl);
			SchedulerProxy.Instance.Initialize(Settings.Default.UpdateManagerUrl);
			return result;
		}
		DataStoreService.Instance.Initialize(Settings.Default.DataStoreRoot);
		QueueService.Instance.Initialize(Settings.Default.DataStoreRoot);
		TransferService.Instance.Initialize();
		RepositoryService.Instance.Initialize(Settings.Default.ManifestRoot);
		Scheduler.Instance.Initialize();
		return result;
	}

	public ErrorList StartUpdate()
	{
		return ServiceLocator.Instance.GetService<IUpdateService>().StartDownloads();
	}

	public ErrorList FinishUpdates()
	{
		return ServiceLocator.Instance.GetService<IUpdateService>().FinishDownloads();
	}

	public ErrorList FinishUpdate(Guid jobId)
	{
		return ServiceLocator.Instance.GetService<IUpdateService>().FinishDownload(jobId);
	}

	public ErrorList Activate(string name)
	{
		bool deferredMove;
		return ServiceLocator.Instance.GetService<IRepositoryService>().ActivateToHead(name, out deferredMove);
	}

	public ErrorList Activate(string name, string hash)
	{
		bool deferredMove;
		return ServiceLocator.Instance.GetService<IRepositoryService>().ActivateTo(name, hash, out deferredMove);
	}

	public ErrorList UpdateTo(string name, string hash)
	{
		return ServiceLocator.Instance.GetService<IRepositoryService>().UpdateTo(name, hash);
	}

	public ErrorList UpdateTo(string name)
	{
		return ServiceLocator.Instance.GetService<IRepositoryService>().UpdateToHead(name);
	}

	public List<string> GetManifests()
	{
		return ServiceLocator.Instance.GetService<IRepositoryService>().GetAllRepositories();
	}

	public List<IRevLog> GetPendingChangeSets(string name)
	{
		return ServiceLocator.Instance.GetService<IRepositoryService>().GetPendingChanges(name);
	}

	public List<IRevLog> GetAppliedChangeSets(string name)
	{
		return ServiceLocator.Instance.GetService<IRepositoryService>().GetAppliedChanges(name);
	}

	public List<IRevLog> GetAllChangesSets(string name)
	{
		return ServiceLocator.Instance.GetService<IRepositoryService>().GetAllChanges(name);
	}

	public ErrorList ResetBITS()
	{
		return ServiceLocator.Instance.GetService<ITransferService>().CancelAll();
	}

	public void ResetManifest()
	{
		ServiceLocator.Instance.GetService<IRepositoryService>().Reset(out var _);
	}

	public void Reset()
	{
		ServiceLocator.Instance.GetService<IRepositoryService>().Reset(out var _);
		ServiceLocator.Instance.GetService<ITransferService>().CancelAll();
		ServiceLocator.Instance.GetService<IDataStoreService>().Reset();
	}

	private static bool IsServiceRunning()
	{
		return !new NamedLock("{C423B0E3-95B3-4191-B326-AB3EA516F825}", LockScope.Local).Exists();
	}
}
