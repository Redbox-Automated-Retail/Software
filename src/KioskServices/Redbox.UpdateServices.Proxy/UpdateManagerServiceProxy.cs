using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Redbox.Core;
using Redbox.IPC.Framework;
using Redbox.KioskEngine.ComponentModel;
using Redbox.UpdateManager.Client;
using Redbox.UpdateServices.Proxy.ComponentModel;

namespace Redbox.UpdateServices.Proxy;

public class UpdateManagerServiceProxy : IUpdateManagerService
{
	internal class DownloadData : IDownloadData
	{
		public Guid Id { get; set; }

		public string Name { get; set; }

		public string Owner { get; set; }

		public string Status { get; set; }

		public string JobType { get; set; }

		public ulong TotalBytes { get; set; }

		public DateTime StartTime { get; set; }

		public DateTime? FinishTime { get; set; }

		public DateTime ModifiedTime { get; set; }

		public ulong TotalBytesTransferred { get; set; }
	}

	internal const int DefaultUpdateServiceTimeout = 600000;

	internal const string DefaultUpdateServiceUrl = "rcp://localhost:7004";

	private static UpdateManagerService m_service;

	public static UpdateManagerServiceProxy Instance => Singleton<UpdateManagerServiceProxy>.Instance;

	public int Timeout
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Store Services", "UpdateManagerServiceTimeout", 600000);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Store Services", "UpdateManagerServiceTimeout", value);
			ResetService();
		}
	}

	public string Url
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Remote Services\\Store Services", "UpdateManagerServiceURL", "rcp://localhost:7004");
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Remote Services\\Store Services", "UpdateManagerServiceURL", value);
			ResetService();
		}
	}

	public Redbox.KioskEngine.ComponentModel.ErrorList Initialize()
	{
		_ = 5;
		ResetService();
		ServiceLocator.Instance.AddService(typeof(IUpdateManagerService), (object)Instance);
		return new Redbox.KioskEngine.ComponentModel.ErrorList();
	}

	public Redbox.KioskEngine.ComponentModel.ErrorList ForceScheduleEvent(string eventName, out bool isRunning)
	{
		return MakeLocalErrorList(m_service.ForceScheduleEvent(eventName, out isRunning).Errors);
	}

	public Redbox.KioskEngine.ComponentModel.ErrorList GetDownloadState(out ReadOnlyCollection<IDownloadData> downloads)
	{
		List<IDownloadData> list = new List<IDownloadData>();
		ClientCommandResult currentDownloads = m_service.GetCurrentDownloads(out var jobs);
		foreach (Download item in jobs)
		{
			list.Add(new DownloadData
			{
				Id = item.ID,
				Name = item.Name,
				Owner = item.Owner,
				Status = item.Status.ToString(),
				JobType = item.JobType.ToString(),
				TotalBytes = item.TotalBytes,
				StartTime = item.StartTime,
				FinishTime = item.FinishTime,
				ModifiedTime = item.ModifiedTime,
				TotalBytesTransferred = item.TotalBytesTransfered
			});
		}
		downloads = list.AsReadOnly();
		return MakeLocalErrorList(currentDownloads.Errors);
	}

	public Redbox.KioskEngine.ComponentModel.ErrorList GetSubscriptionState(out DateTime lastRun, out string subscriptionState)
	{
		ClientCommandResult dvdDataState = m_service.GetDvdDataState(out lastRun, out var state);
		subscriptionState = state.ToString();
		return MakeLocalErrorList(dvdDataState.Errors);
	}

	public Redbox.KioskEngine.ComponentModel.ErrorList ForceSubscriptionPublish()
	{
		return MakeLocalErrorList(m_service.ForceDvdDataPublish().Errors);
	}

	public Redbox.KioskEngine.ComponentModel.ErrorList ExecuteCommand(string command, out ReadOnlyCollection<string> messages)
	{
		ClientCommandResult clientCommandResult = m_service.ExecuteCommandString(command);
		messages = clientCommandResult.CommandMessages.AsReadOnly();
		return MakeLocalErrorList(clientCommandResult.Errors);
	}

	internal void ResetService()
	{
		try
		{
			if (m_service != null)
			{
				m_service.Dispose();
				m_service = null;
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was raised in UpdateManagerServiceProxy.ResetService.", e);
		}
		m_service = UpdateManagerService.GetService(Url, Timeout);
	}

	private UpdateManagerServiceProxy()
	{
	}

	private static Redbox.KioskEngine.ComponentModel.ErrorList MakeLocalErrorList(IEnumerable<Redbox.IPC.Framework.Error> foreignErrors)
	{
		Redbox.KioskEngine.ComponentModel.ErrorList errorList = new Redbox.KioskEngine.ComponentModel.ErrorList();
		foreach (Redbox.IPC.Framework.Error foreignError in foreignErrors)
		{
			errorList.Add(foreignError.IsWarning ? Redbox.KioskEngine.ComponentModel.Error.NewWarning(foreignError.Code, foreignError.Description, foreignError.Details) : Redbox.KioskEngine.ComponentModel.Error.NewError(foreignError.Code, foreignError.Description, foreignError.Details));
		}
		return errorList;
	}
}
