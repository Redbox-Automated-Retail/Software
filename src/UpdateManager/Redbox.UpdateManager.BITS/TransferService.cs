using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using Redbox.Core;
using Redbox.UpdateManager.ComponentModel;
using Redbox.UpdateService.Client;

namespace Redbox.UpdateManager.BITS;

public class TransferService : ITransferService
{
	private const uint EnumAllUsers = 1u;

	private const uint EnumCurrentUser = 0u;

	public static TransferService Instance => Singleton<TransferService>.Instance;

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

	public void Initialize()
	{
		ServiceLocator.Instance.AddService(typeof(ITransferService), this);
	}

	public ErrorList AreJobsRunning(out bool isRunning)
	{
		isRunning = false;
		ErrorList errorList = new ErrorList();
		errorList.AddRange(GetJobs(out var jobs, allUsers: false));
		if (errorList.ContainsError())
		{
			return errorList;
		}
		if (jobs.Count > 0 && jobs.FindIndex((ITransferJob j) => j.Name.StartsWith("<~") && j.Name.EndsWith("~>")) > -1)
		{
			isRunning = true;
		}
		return errorList;
	}

	public ErrorList CancelAll()
	{
		ErrorList jobs = GetJobs(out var jobs2, allUsers: false);
		if (!jobs.ContainsError())
		{
			jobs2.ForEach(delegate(ITransferJob j)
			{
				if (j.Status == TransferStatus.Transferred)
				{
					j.Complete();
				}
				else
				{
					j.Cancel();
				}
			});
		}
		return jobs;
	}

	public ErrorList GetRepositoriesInTransit(out HashSet<string> inTransit)
	{
		inTransit = new HashSet<string>();
		IDataStoreService service = ServiceLocator.Instance.GetService<IDataStoreService>();
		ErrorList errorList = new ErrorList();
		errorList.AddRange(GetJobs(out var jobs, allUsers: false));
		if (errorList.ContainsError())
		{
			return errorList;
		}
		foreach (ITransferJob item in jobs)
		{
			if (item.Status != TransferStatus.Error)
			{
				ChangeSet changeSet = service.Get<ChangeSet>(item.ID);
				if (changeSet != null && !inTransit.Contains(changeSet.Name))
				{
					inTransit.Add(changeSet.Name);
				}
			}
		}
		return errorList;
	}

	public ErrorList GetJobs(out List<ITransferJob> jobs, bool allUsers)
	{
		jobs = new List<ITransferJob>();
		ErrorList errorList = new ErrorList();
		IBackgroundCopyManager backgroundCopyManager = (IBackgroundCopyManager)new BackgroundCopyManager();
		IEnumBackgroundCopyJobs ppenum = null;
		try
		{
			backgroundCopyManager.EnumJobs(allUsers ? 1u : 0u, out ppenum);
			ppenum.GetCount(out var puCount);
			for (int i = 0; i < puCount; i++)
			{
				uint pceltFetched = 0u;
				ppenum.Next(1u, out var rgelt, out pceltFetched);
				rgelt.GetId(out var pVal);
				jobs.Add(new TransferJob(pVal, rgelt));
				Marshal.ReleaseComObject(rgelt);
			}
			jobs.Sort(delegate(ITransferJob lhs, ITransferJob rhs)
			{
				if (lhs.FinishTime.HasValue && rhs.FinishTime.HasValue)
				{
					return lhs.FinishTime.Value.CompareTo(rhs.FinishTime.Value);
				}
				if (lhs.FinishTime.HasValue && !rhs.FinishTime.HasValue)
				{
					return 1;
				}
				return (!lhs.FinishTime.HasValue && rhs.FinishTime.HasValue) ? (-1) : lhs.StartTime.CompareTo(rhs.StartTime);
			});
		}
		catch (COMException e)
		{
			errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("B999", "Error creating BITS job.", e));
			errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("B999", "Error accesing BITS job.", new Win32Exception(Marshal.GetLastWin32Error())));
		}
		finally
		{
			Marshal.ReleaseComObject(backgroundCopyManager);
			if (ppenum != null)
			{
				Marshal.ReleaseComObject(ppenum);
			}
		}
		return errorList;
	}

	public ErrorList CreateDownloadJob(string name, out ITransferJob job)
	{
		job = null;
		ErrorList errorList = new ErrorList();
		IBackgroundCopyManager backgroundCopyManager = (IBackgroundCopyManager)new BackgroundCopyManager();
		IBackgroundCopyJob ppJob = null;
		try
		{
			backgroundCopyManager.CreateJob(name, BG_JOB_TYPE.BG_JOB_TYPE_DOWNLOAD, out var pJobId, out ppJob);
			job = new TransferJob(pJobId, ppJob);
		}
		catch (COMException e)
		{
			errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("B999", "Error creating BITS job.", e));
		}
		finally
		{
			Marshal.ReleaseComObject(backgroundCopyManager);
			if (ppJob != null)
			{
				Marshal.ReleaseComObject(ppJob);
			}
		}
		return errorList;
	}

	public ErrorList CreateUploadJob(string name, out ITransferJob job)
	{
		job = null;
		ErrorList errorList = new ErrorList();
		IBackgroundCopyManager backgroundCopyManager = (IBackgroundCopyManager)new BackgroundCopyManager();
		IBackgroundCopyJob ppJob = null;
		try
		{
			backgroundCopyManager.CreateJob(name, BG_JOB_TYPE.BG_JOB_TYPE_UPLOAD, out var pJobId, out ppJob);
			job = new TransferJob(pJobId, ppJob);
		}
		catch (COMException e)
		{
			errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("B999", "Error creating BITS job.", e));
		}
		finally
		{
			Marshal.ReleaseComObject(backgroundCopyManager);
			if (ppJob != null)
			{
				Marshal.ReleaseComObject(ppJob);
			}
		}
		return errorList;
	}

	public ErrorList GetJob(Guid id, out ITransferJob job)
	{
		ErrorList errorList = new ErrorList();
		job = null;
		try
		{
			job = new TransferJob(id);
		}
		catch (COMException e)
		{
			errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("B999", "Error accesing BITS job.", e));
			errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("B999", "Error accesing BITS job.", new Win32Exception(Marshal.GetLastWin32Error())));
		}
		return errorList;
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

	private TransferService()
	{
	}
}
