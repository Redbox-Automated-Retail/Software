using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using Redbox.Core;
using Redbox.HardwareServices.Proxy.ComponentModel;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.HardwareServices.Proxy;

public class SyncEmptiesProcessor : LocationSyncJobProcessorBase, ISyncEmptiesProcessor
{
	private const string SYNCJOBNAME = "SyncEmptyLocations";

	private const string SOFTSYNCJOBNAME = "SyncEmpty";

	public static SyncEmptiesProcessor Instance => Singleton<SyncEmptiesProcessor>.Instance;

	private SyncEmptiesProcessor()
	{
	}

	public void Initialize()
	{
		_hardwareService = ServiceLocator.Instance.GetService<IHardwareService>();
		if (_hardwareService == null)
		{
			LogHelper.Instance.Log("SyncEmptiesProcessor: Unable to locate Hardware Service.");
			return;
		}
		IConfigurationService configurationService = ServiceLocator.Instance.GetService<IConfigurationService>();
		if (!configurationService.TryGetValue<bool>("system", "General", "SyncEmptyEnabled", out _isEnabled))
		{
			_isEnabled = true;
		}
		if (!configurationService.TryGetValue<string>("system", "General", "SyncEmptyStartTimeRangeBegin", out var rangeBeginText))
		{
			rangeBeginText = "1:00";
		}
		if (!configurationService.TryGetValue<string>("system", "General", "SyncEmptyStartTimeRangeEnd", out var rangeEndText))
		{
			rangeEndText = "3:00";
		}
		_timespanBegin = TimeSpan.Parse(rangeBeginText);
		_timespanEnd = TimeSpan.Parse(rangeEndText);
		_timer = new Timer(60000.0);
		_timer.Elapsed += _timer_Elapsed;
		_timer.Enabled = true;
	}

	private void _timer_Elapsed(object sender, ElapsedEventArgs e)
	{
		IHardwareJob competedJob = GetCompetedJob(new List<string> { "SyncEmpty" });
		if (competedJob != null)
		{
			ProcessSoftSyncComplete(competedJob);
		}
	}

	public void ScheduleJob()
	{
		if (_isEnabled)
		{
			DateTime syncTime = ScheduleSyncTime(_timespanBegin, _timespanEnd, startIfInwindow: false, 20);
			bool startSync = true;
			if (IsJobAlreadyScheduled(new List<string> { "SyncEmpty", "SyncEmptyLocations" }))
			{
				LogHelper.Instance.Log("HardwareFunctions.SyncEmpties: Sync Empty Locations already scheduled. This run would have been scheduled for " + syncTime.ToString() + ".");
				startSync = false;
			}
			if (startSync)
			{
				LogHelper.Instance.Log("Scheduling Sync Empty job for " + syncTime.ToString());
				StartSync("SyncEmpty", syncTime);
			}
		}
	}

	private void ProcessSoftSyncComplete(IHardwareJob job)
	{
		ErrorList programResults = _hardwareService.GetProgramResults(job.ID, out var hardwareResults);
		if (programResults.ErrorCount == 0)
		{
			List<int> list = new List<int>();
			foreach (IHardwareResult item in hardwareResults)
			{
				if (item.Code == "SoftSync" && item.ItemID == "EMPTY")
				{
					list.Add(item.Slot);
					list.Add(item.Deck);
				}
			}
			if (list.Count > 1)
			{
				ErrorList source = _hardwareService.SyncLocations(null, null, "SyncEmptyLocations", null, list, out var jobId);
				string text = ((!string.IsNullOrEmpty(jobId) && !source.Any()) ? "Successfully" : "Unsuccessfully");
				LogHelper.Instance.Log(string.Format("{0} scheduled job: {1} - {2} with {3} locations.", text, jobId ?? string.Empty, "SyncEmptyLocations", list.Count() / 2));
			}
			else
			{
				LogHelper.Instance.Log("No Empty locations found to sync.");
			}
		}
		else
		{
			LocationSyncJobProcessorBase.LogErrors(programResults, "GetProgramResults");
		}
		_hardwareService.Trash(job.ID);
	}
}
