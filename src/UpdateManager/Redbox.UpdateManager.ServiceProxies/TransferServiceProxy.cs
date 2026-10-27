using System;
using System.Collections.Generic;
using Redbox.Core;
using Redbox.IPC.Framework;
using Redbox.UpdateManager.ComponentModel;

namespace Redbox.UpdateManager.ServiceProxies;

public class TransferServiceProxy : ITransferService
{
	private string m_url;

	public static TransferServiceProxy Instance => Singleton<TransferServiceProxy>.Instance;

	public int MaxBandwidthWhileWithInSchedule
	{
		get
		{
			return BandwidthUsageSettings.MaxBandwidthWhileWithInSchedule;
		}
		private set
		{
			BandwidthUsageSettings.MaxBandwidthWhileWithInSchedule = value;
		}
	}

	public int MaxBandwidthWhileOutsideOfSchedule
	{
		get
		{
			return BandwidthUsageSettings.MaxBandwidthWhileWithInSchedule;
		}
		private set
		{
			BandwidthUsageSettings.MaxBandwidthWhileWithInSchedule = value;
		}
	}

	public byte StartOfScheduleInHoursFromMidnight
	{
		get
		{
			return BandwidthUsageSettings.StartOfScheduleInHoursFromMidnight;
		}
		private set
		{
			BandwidthUsageSettings.StartOfScheduleInHoursFromMidnight = value;
		}
	}

	public byte EndOfScheduleInHoursFromMidnight
	{
		get
		{
			return BandwidthUsageSettings.EndOfScheduleInHoursFromMidnight;
		}
		private set
		{
			BandwidthUsageSettings.EndOfScheduleInHoursFromMidnight = value;
		}
	}

	public bool UseSystemMaxOutsideOfSchedule
	{
		get
		{
			return BandwidthUsageSettings.UseSystemMaxOutsideOfSchedule;
		}
		private set
		{
			BandwidthUsageSettings.UseSystemMaxOutsideOfSchedule = value;
		}
	}

	public bool EnableMaximumBandwitdthThrottle
	{
		get
		{
			return BandwidthUsageSettings.EnableMaximumBandwitdthThrottle;
		}
		private set
		{
			BandwidthUsageSettings.EnableMaximumBandwitdthThrottle = value;
		}
	}

	public void Initialize(string url)
	{
		m_url = url;
		ServiceLocator.Instance.AddService(typeof(ITransferService), this);
	}

	public Redbox.UpdateManager.ComponentModel.ErrorList CancelAll()
	{
		return ConvertFromIPCErrors(ExecuteCommandString("transfer cancel-all").Errors);
	}

	public Redbox.UpdateManager.ComponentModel.ErrorList GetRepositoriesInTransit(out HashSet<string> inTransit)
	{
		inTransit = new HashSet<string>();
		ClientCommandResult clientCommandResult = ExecuteCommandString("transfer repositories-in-transit");
		if (clientCommandResult.Success)
		{
			inTransit = new HashSet<string>(clientCommandResult.CommandMessages[0].ToObject<List<string>>());
		}
		return ConvertFromIPCErrors(clientCommandResult.Errors);
	}

	public Redbox.UpdateManager.ComponentModel.ErrorList GetJobs(out List<ITransferJob> jobs, bool allUsers)
	{
		jobs = new List<ITransferJob>();
		ClientCommandResult clientCommandResult = ExecuteCommandString($"transfer list allUsers: {allUsers}");
		if (clientCommandResult.Success)
		{
			List<TransferJobProxy> list = clientCommandResult.CommandMessages[0].ToObject<List<TransferJobProxy>>();
			list.ForEach(delegate(TransferJobProxy p)
			{
				p.SetUrl(m_url);
			});
			jobs = list.ConvertAll((Converter<TransferJobProxy, ITransferJob>)((TransferJobProxy p) => p));
		}
		return ConvertFromIPCErrors(clientCommandResult.Errors);
	}

	public Redbox.UpdateManager.ComponentModel.ErrorList CreateDownloadJob(string name, out ITransferJob job)
	{
		job = null;
		ClientCommandResult clientCommandResult = ExecuteCommandString($"transfer create-download name: '{name}'");
		if (clientCommandResult.Success)
		{
			TransferJobProxy transferJobProxy = clientCommandResult.CommandMessages[0].ToObject<TransferJobProxy>();
			transferJobProxy.SetUrl(m_url);
			job = transferJobProxy;
		}
		return ConvertFromIPCErrors(clientCommandResult.Errors);
	}

	public Redbox.UpdateManager.ComponentModel.ErrorList CreateUploadJob(string name, out ITransferJob job)
	{
		job = null;
		ClientCommandResult clientCommandResult = ExecuteCommandString($"transfer create-upload name: '{name}'");
		if (clientCommandResult.Success)
		{
			TransferJobProxy transferJobProxy = clientCommandResult.CommandMessages[0].ToObject<TransferJobProxy>();
			transferJobProxy.SetUrl(m_url);
			job = transferJobProxy;
		}
		return ConvertFromIPCErrors(clientCommandResult.Errors);
	}

	public Redbox.UpdateManager.ComponentModel.ErrorList GetJob(Guid id, out ITransferJob job)
	{
		job = null;
		ClientCommandResult clientCommandResult = ExecuteCommandString($"transfer get-job id: '{id}'");
		if (clientCommandResult.Success)
		{
			TransferJobProxy transferJobProxy = clientCommandResult.CommandMessages[0].ToObject<TransferJobProxy>();
			transferJobProxy.SetUrl(m_url);
			job = transferJobProxy;
		}
		return ConvertFromIPCErrors(clientCommandResult.Errors);
	}

	public Redbox.UpdateManager.ComponentModel.ErrorList AreJobsRunning(out bool isRunning)
	{
		isRunning = false;
		ClientCommandResult clientCommandResult = ExecuteCommandString("transfer are-jobs-running");
		if (clientCommandResult.Success)
		{
			isRunning = clientCommandResult.CommandMessages[0].ToObject<bool>();
		}
		return ConvertFromIPCErrors(clientCommandResult.Errors);
	}

	public bool SetMaxBandwidthWhileWithInSchedule(int i)
	{
		bool result = MaxBandwidthWhileWithInSchedule != i;
		MaxBandwidthWhileWithInSchedule = i;
		return result;
	}

	public bool SetMaxBandwidthWhileOutsideOfSchedule(int max)
	{
		bool result = false;
		try
		{
			result = MaxBandwidthWhileOutsideOfSchedule != max;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("Error Accessing BITS settings.", e);
		}
		MaxBandwidthWhileOutsideOfSchedule = max;
		return result;
	}

	public bool SetStartOfScheduleInHoursFromMidnight(byte b)
	{
		bool result = false;
		try
		{
			result = StartOfScheduleInHoursFromMidnight != b;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("Error Accessing BITS settings.", e);
		}
		StartOfScheduleInHoursFromMidnight = b;
		return result;
	}

	public bool SetEndOfScheduleInHoursFromMidnight(byte b)
	{
		bool result = false;
		try
		{
			result = EndOfScheduleInHoursFromMidnight != b;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("Error Accessing BITS settings.", e);
		}
		EndOfScheduleInHoursFromMidnight = b;
		return result;
	}

	public bool SetUseSystemMaxOutsideOfSchedule(bool flag)
	{
		bool result = false;
		try
		{
			result = UseSystemMaxOutsideOfSchedule != flag;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("Error Accessing BITS settings.", e);
		}
		UseSystemMaxOutsideOfSchedule = flag;
		return result;
	}

	public bool SetEnableMaximumBandwitdthThrottle(bool flag)
	{
		bool result = false;
		try
		{
			result = EnableMaximumBandwitdthThrottle != flag;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("Error Accessing BITS settings.", e);
		}
		EnableMaximumBandwitdthThrottle = flag;
		return result;
	}

	private ClientCommandResult ExecuteCommandString(string command)
	{
		using UpdateManagerService updateManagerService = new UpdateManagerService(m_url);
		return updateManagerService.ExecuteCommandString(command);
	}

	private static Redbox.UpdateManager.ComponentModel.ErrorList ConvertFromIPCErrors(List<Redbox.IPC.Framework.Error> ipc)
	{
		Redbox.UpdateManager.ComponentModel.ErrorList errors = new Redbox.UpdateManager.ComponentModel.ErrorList();
		ipc.ForEach(delegate(Redbox.IPC.Framework.Error e)
		{
			errors.Add(e.IsWarning ? Redbox.UpdateManager.ComponentModel.Error.NewWarning(e.Code, e.Description, e.Details) : Redbox.UpdateManager.ComponentModel.Error.NewError(e.Code, e.Description, e.Details));
		});
		return errors;
	}

	private TransferServiceProxy()
	{
	}
}
