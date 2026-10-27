using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using Redbox.NetCore.Logging.Extensions;
using Redbox.NetCore.Middleware.Http;
using UpdateClientService.API.App;

namespace UpdateClientService.API.Services.Transfer;

public class TransferService : ITransferService
{
	private const uint EnumAllUsers = 1u;

	private const uint EnumCurrentUser = 0u;

	private readonly ILogger<TransferService> _logger;

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

	public TransferService(ILogger<TransferService> logger)
	{
		_logger = logger;
	}

	public List<Error> AreJobsRunning(out bool isRunning)
	{
		isRunning = false;
		List<Error> list = new List<Error>();
		list.AddRange(GetJobs(out var jobs, allUsers: false));
		if (list.Any())
		{
			return list;
		}
		if (jobs.Count > 0 && jobs.FindIndex((ITransferJob j) => j.Name.StartsWith("<~") && j.Name.EndsWith("~>")) > -1)
		{
			isRunning = true;
		}
		return list;
	}

	public List<Error> CancelAll()
	{
		List<Error> jobs = GetJobs(out var jobs2, allUsers: false);
		if (!jobs.Any())
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

	public List<Error> GetRepositoriesInTransit(out HashSet<long> inTransit)
	{
		inTransit = new HashSet<long>();
		List<Error> list = new List<Error>();
		list.AddRange(GetJobs(out var jobs, allUsers: false));
		if (list.Any())
		{
			return list;
		}
		foreach (ITransferJob item in jobs)
		{
			_ = item.Status;
			_ = 4;
		}
		return list;
	}

	public List<Error> GetJobs(out List<ITransferJob> jobs, bool allUsers)
	{
		jobs = new List<ITransferJob>();
		List<Error> list = new List<Error>();
		IBackgroundCopyManager backgroundCopyManager = (IBackgroundCopyManager)new BackgroundCopyManager();
		IEnumBackgroundCopyJobs ppenum = null;
		try
		{
			backgroundCopyManager.EnumJobs(allUsers ? 1u : 0u, out ppenum);
			ppenum.GetCount(out var puCount);
			for (int i = 0; i < puCount; i++)
			{
				ppenum.Next(1u, out var rgelt, out var _);
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
		catch (COMException ex)
		{
			HandleCOMException("Error getting BITS job", list, ex);
		}
		catch (Exception e)
		{
			HandleGenericException(list, e);
		}
		finally
		{
			Marshal.ReleaseComObject(backgroundCopyManager);
			if (ppenum != null)
			{
				Marshal.ReleaseComObject(ppenum);
			}
		}
		return list;
	}

	public List<Error> CreateDownloadJob(string name, out ITransferJob job)
	{
		job = null;
		List<Error> list = new List<Error>();
		IBackgroundCopyManager backgroundCopyManager = (IBackgroundCopyManager)new BackgroundCopyManager();
		IBackgroundCopyJob ppJob = null;
		try
		{
			backgroundCopyManager.CreateJob(name, BG_JOB_TYPE.BG_JOB_TYPE_DOWNLOAD, out var pJobId, out ppJob);
			job = new TransferJob(pJobId, ppJob);
		}
		catch (COMException ex)
		{
			HandleCOMException("Error creating BITS job", list, ex);
		}
		catch (Exception e)
		{
			HandleGenericException(list, e);
		}
		finally
		{
			Marshal.ReleaseComObject(backgroundCopyManager);
			if (ppJob != null)
			{
				Marshal.ReleaseComObject(ppJob);
			}
		}
		return list;
	}

	public List<Error> CreateUploadJob(string name, out ITransferJob job)
	{
		job = null;
		List<Error> list = new List<Error>();
		IBackgroundCopyManager backgroundCopyManager = (IBackgroundCopyManager)new BackgroundCopyManager();
		IBackgroundCopyJob ppJob = null;
		try
		{
			backgroundCopyManager.CreateJob(name, BG_JOB_TYPE.BG_JOB_TYPE_UPLOAD, out var pJobId, out ppJob);
			job = new TransferJob(pJobId, ppJob);
		}
		catch (COMException ex)
		{
			HandleCOMException("Error creating BITS job", list, ex);
		}
		catch (Exception e)
		{
			HandleGenericException(list, e);
		}
		finally
		{
			Marshal.ReleaseComObject(backgroundCopyManager);
			if (ppJob != null)
			{
				Marshal.ReleaseComObject(ppJob);
			}
		}
		return list;
	}

	public List<Error> GetJob(Guid id, out ITransferJob job)
	{
		List<Error> list = new List<Error>();
		job = null;
		try
		{
			job = new TransferJob(id);
		}
		catch (COMException ex)
		{
			HandleCOMException("Error accessing BITS job", list, ex);
		}
		catch (Exception e)
		{
			HandleGenericException(list, e);
		}
		return list;
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
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "Error Accessing BITS settings.", "SetMaxBandwidthWhileOutsideOfSchedule", "/sln/src/UpdateClientService.API/Services/TransferService/TransferService.cs");
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
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "Error Accessing BITS settings.", "SetStartOfScheduleInHoursFromMidnight", "/sln/src/UpdateClientService.API/Services/TransferService/TransferService.cs");
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
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "Error Accessing BITS settings.", "SetEndOfScheduleInHoursFromMidnight", "/sln/src/UpdateClientService.API/Services/TransferService/TransferService.cs");
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
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "Error Accessing BITS settings.", "SetUseSystemMaxOutsideOfSchedule", "/sln/src/UpdateClientService.API/Services/TransferService/TransferService.cs");
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
		catch (Exception ex)
		{
			_logger.LogErrorWithSource(ex, "Error Accessing BITS settings.", "SetEnableMaximumBandwitdthThrottle", "/sln/src/UpdateClientService.API/Services/TransferService/TransferService.cs");
		}
		EnableMaximumBandwitdthThrottle = flag;
		return result;
	}

	private void HandleCOMException(string baseMessage, List<Error> errors, COMException ex)
	{
		errors.AddRange(new List<Error>
		{
			new Error
			{
				Code = "C999",
				Message = baseMessage + ". Exception -> " + ex.GetFullMessage()
			},
			GetLastWin32Error()
		});
	}

	private void HandleGenericException(List<Error> errors, Exception e)
	{
		errors.Add(new Error
		{
			Code = "C998",
			Message = "Error accessing BITS job. Exception -> " + e.GetFullMessage()
		});
	}

	private static Error GetLastWin32Error()
	{
		return new Error
		{
			Code = "B999",
			Message = "Error accessing BITS job. Exception -> " + new Win32Exception(Marshal.GetLastWin32Error()).GetFullMessage()
		};
	}
}
