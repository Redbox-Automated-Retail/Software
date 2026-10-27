using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using Redbox.Core;
using Redbox.HardwareServices.Proxy.ComponentModel;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.HardwareServices.Proxy;

public abstract class LocationSyncJobProcessorBase
{
	protected IHardwareService _hardwareService;

	protected bool _isEnabled;

	protected TimeSpan _timespanBegin;

	protected TimeSpan _timespanEnd;

	protected Timer _timer;

	protected void StartSync(string label = null, DateTime? startTime = null)
	{
		ErrorList errorList = _hardwareService.SoftSync(null, null, startTime, label, out var jobId);
		LogErrors(errorList, "SoftSync: " + label);
		if (errorList.Count == 0)
		{
			LogHelper.Instance.Log("Scheduled SoftSync job ID " + jobId + " with label " + label);
		}
	}

	protected DateTime ScheduleSyncTime(TimeSpan timespanBegin, TimeSpan timespanEnd, bool startIfInwindow, int delay)
	{
		if (timespanBegin > timespanEnd)
		{
			timespanEnd = timespanEnd.Add(TimeSpan.FromDays(1.0));
		}
		DateTime now = DateTime.Now;
		DateTime dateTime = now.Date + timespanBegin;
		DateTime dateTime2 = now.Date + timespanEnd;
		DateTime dateTime3 = now;
		if (startIfInwindow)
		{
			dateTime3 = dateTime3.AddMinutes(delay);
			if ((dateTime3 >= dateTime && dateTime3 <= dateTime2) || (dateTime3.AddDays(1.0) >= dateTime && dateTime3.AddDays(1.0) <= dateTime2))
			{
				return dateTime3;
			}
		}
		if (dateTime3 < dateTime)
		{
			return dateTime.AddMinutes(dateTime3.Minute);
		}
		return dateTime.AddDays(1.0).AddMinutes(dateTime3.Minute);
	}

	protected static void LogErrors(ErrorList errors, string jobName)
	{
		if (errors.ErrorCount <= 0)
		{
			return;
		}
		LogHelper.Instance.Log(jobName + " encountered the following errors:");
		foreach (Redbox.KioskEngine.ComponentModel.Error error in errors)
		{
			LogHelper.Instance.LogError(error.Code, error.Description);
		}
	}

	protected bool IsJobAlreadyScheduled(List<string> labels)
	{
		if (labels == null || labels.Count == 0)
		{
			return false;
		}
		ErrorList hardwareJobs = _hardwareService.GetHardwareJobs(out var hardwareJobs2);
		if (hardwareJobs.Any())
		{
			LogErrors(hardwareJobs, labels[0] + " - Unable to retrieve job list.");
			return false;
		}
		foreach (string label in labels)
		{
			if (hardwareJobs2.Any((IHardwareJob j) => j.Label == label && (j.Status == "Pending" || j.Status == "Running" || j.Status == "Suspended")))
			{
				return true;
			}
		}
		return false;
	}

	protected IHardwareJob GetCompetedJob(List<string> labels)
	{
		if (labels == null || labels.Count == 0)
		{
			return null;
		}
		ErrorList hardwareJobs = _hardwareService.GetHardwareJobs(out var hardwareJobs2);
		if (hardwareJobs.Any())
		{
			LogErrors(hardwareJobs, labels[0] + " - Unable to retrieve job list.");
			return null;
		}
		foreach (string label in labels)
		{
			IHardwareJob hardwareJob = hardwareJobs2.FirstOrDefault((IHardwareJob j) => j.Label == label);
			if (hardwareJob != null && _hardwareService.IsComplete(hardwareJob.Status))
			{
				return hardwareJob;
			}
		}
		return null;
	}
}
