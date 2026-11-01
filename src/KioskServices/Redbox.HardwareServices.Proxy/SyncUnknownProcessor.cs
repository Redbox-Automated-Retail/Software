using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Timers;
using Redbox.Core;
using Redbox.HardwareServices.Proxy.ComponentModel;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.Environment;
using Redbox.Rental.Model;

namespace Redbox.HardwareServices.Proxy;

public class SyncUnknownProcessor : LocationSyncJobProcessorBase, ISyncUnknownProcessor
{
	private IDataFileStoreService _fileService;

	private IMachineSettingsStore _machineSettings;

	private string _dataPath;

	private int _maxSyncCount;

	private int? _qlmDeck;

	private const string DATAFILENAME = "SyncCounts";

	private const string SOFTSYNCJOBNAME = "SyncUnknowns";

	private const string SYNCJOBNAME = "SyncUnknownLocations";

	public static SyncUnknownProcessor Instance => Singleton<SyncUnknownProcessor>.Instance;

	private SyncUnknownProcessor()
	{
	}

	public void Initialize()
	{
		_machineSettings = ServiceLocator.Instance.GetService<IMachineSettingsStore>();
		_hardwareService = ServiceLocator.Instance.GetService<IHardwareService>();
		if (_hardwareService == null)
		{
			LogHelper.Instance.Log("SyncUnknownProcessor: Unable to locate Hardware Service.");
			return;
		}
		IHardwareState hardwareState = ServiceLocator.Instance.GetService<IHardwareState>();
		if (hardwareState == null)
		{
			LogHelper.Instance.Log("SyncUnknownProcessor: Unable to locate Hardware State.");
			return;
		}
		_qlmDeck = hardwareState.QlmDeckAsNumber;
		IEngineApplication engineApplication = ServiceLocator.Instance.GetService<IEngineApplication>();
		_dataPath = Path.Combine(engineApplication.DataPath, "SyncUnknowns");
		_fileService = new DataFileStoreService();
		_fileService.Initialize("SyncUnknownCounts", _dataPath, "json", "corrupt", useEncryption: false);
		IConfigurationService configurationService = ServiceLocator.Instance.GetService<IConfigurationService>();
		if (!configurationService.TryGetValue<bool>("system", "General", "SyncUnknownEnabled", out _isEnabled))
		{
			_isEnabled = true;
		}
		configurationService.TryGetValue<int>("system", "General", "MaxUnknownSyncs", out _maxSyncCount);
		if (_maxSyncCount == 0)
		{
			_maxSyncCount = 3;
		}
		if (!configurationService.TryGetValue<string>("system", "General", "SyncUnknownStartTimeRangeBegin", out var rangeBeginText))
		{
			rangeBeginText = "1:00";
		}
		if (!configurationService.TryGetValue<string>("system", "General", "SyncUnknownStartTimeRangeEnd", out var rangeEndText))
		{
			rangeEndText = "3:00";
		}
		_timespanBegin = TimeSpan.Parse(rangeBeginText);
		_timespanEnd = TimeSpan.Parse(rangeEndText);
		_timer = new Timer(60000.0);
		_timer.Elapsed += TimerElapsed;
		_timer.Enabled = true;
	}

	private void TimerElapsed(object sender, ElapsedEventArgs e)
	{
		IHardwareJob competedJob = GetCompetedJob(new List<string> { "SyncUnknowns" });
		if (competedJob != null)
		{
			ProcessSoftSyncComplete(competedJob);
		}
	}

	public void ScheduleJob(bool forceSync = false)
	{
		if (!_isEnabled)
		{
			return;
		}
		bool runSync = forceSync;
		DateTime syncTime = ScheduleSyncTime(_timespanBegin, _timespanEnd, startIfInwindow: true, 0);
		if (!forceSync)
		{
			IStoreManager storeManager = ServiceLocator.Instance.GetService<IStoreManager>();
			if (storeManager != null)
			{
				_ = storeManager.KioskId;
				if (0 == 0 && storeManager.KioskId != 0L)
				{
					DateTime lastSyncDate;
					if (IsJobAlreadyScheduled(new List<string> { "SyncUnknowns", "SyncUnknownLocations" }))
					{
						LogHelper.Instance.Log($"HardwareFunctions.SyncUnknowns: Uknown Sync already scheduled. This run would have been scheduled for {syncTime.ToString()}. Run Sync = {runSync}");
						runSync = false;
					}
					else if (DateTime.TryParse(_machineSettings.GetValue("Local Services\\Hardware Services", "LastUknownSyncDate", string.Empty), out lastSyncDate))
					{
						runSync = syncTime.Date > lastSyncDate.Date;
						LogHelper.Instance.Log($"HardwareFunctions.SyncUnknowns: Previous Uknown Sync run was {lastSyncDate.ToString()}. Next run scheduled for {syncTime.ToString()}. Run Sync = {runSync}");
					}
					else
					{
						runSync = true;
						LogHelper.Instance.Log("HardwareFunctions.SyncUnknowns: No previous Unknown Sync run found. Running Sync");
					}
					goto IL_0134;
				}
			}
			runSync = false;
			LogHelper.Instance.Log("No Kiosk ID was found. SyncUnknowns will not be run today.");
			UpdateLastUnknownSyncDate();
		}
		goto IL_0134;
		IL_0134:
		if (runSync)
		{
			StartSync("SyncUnknowns", syncTime);
		}
	}

	private void ProcessSoftSyncComplete(IHardwareJob job)
	{
		ErrorList programResults = _hardwareService.GetProgramResults(job.ID, out var hardwareResults);
		if (programResults.ErrorCount == 0)
		{
			Dictionary<int, SyncCount> locationCounts = GetLocationCounts();
			List<int> locationsToSync = GetLocationsToSync(hardwareResults, locationCounts);
			if (locationsToSync.Count > 1)
			{
				if (QueueSyncLocations(job.ID, locationsToSync))
				{
					SaveLocationCounts(locationCounts);
				}
			}
			else
			{
				LogHelper.Instance.Log("SyncUnknowns: No Unknown locations found to sync.");
				SaveLocationCounts(locationCounts);
				UpdateLastUnknownSyncDate();
			}
		}
		else
		{
			LocationSyncJobProcessorBase.LogErrors(programResults, "GetProgramResults");
		}
		_hardwareService.Trash(job.ID);
	}

	private List<int> GetLocationsToSync(ReadOnlyCollection<IHardwareResult> hardwareResults, Dictionary<int, SyncCount> locationCounts)
	{
		List<int> locations = new List<int>();
		foreach (IHardwareResult hardwareResult in hardwareResults)
		{
			if (!(hardwareResult.Code == "SoftSync"))
			{
				continue;
			}
			int key = LocationKey(hardwareResult.Deck, hardwareResult.Slot);
			if (hardwareResult.ItemID == "UNKNOWN" && (!_qlmDeck.HasValue || hardwareResult.Deck != _qlmDeck))
			{
				bool shouldSync = true;
				if (locationCounts.ContainsKey(key))
				{
					if (locationCounts[key].TimesSyncedUnknown >= _maxSyncCount)
					{
						shouldSync = false;
					}
					else
					{
						locationCounts[key].TimesSyncedUnknown++;
					}
				}
				else
				{
					locationCounts.Add(key, new SyncCount
					{
						Deck = hardwareResult.Deck,
						Slot = hardwareResult.Slot,
						TimesSyncedUnknown = 1
					});
				}
				if (shouldSync)
				{
					locations.Add(hardwareResult.Slot);
					locations.Add(hardwareResult.Deck);
				}
			}
			else if (locationCounts.ContainsKey(key))
			{
				locationCounts.Remove(key);
			}
		}
		return locations;
	}

	private bool QueueSyncLocations(string jobId, List<int> syncLocations)
	{
		ErrorList source = _hardwareService.SyncLocations(null, null, "SyncUnknownLocations", null, syncLocations, out var queuedJobId);
		bool success = !string.IsNullOrEmpty(jobId) && !source.Any();
		string outcome = (success ? "Successfully" : "Unsuccessfully");
		LogHelper.Instance.Log(string.Format("{0} scheduled job: {1} - {2} with {3} locations.", outcome, queuedJobId ?? string.Empty, "SyncUnknownLocations", syncLocations.Count() / 2));
		return success;
	}

	public void ProcessSyncLocationsComplete(string jobId)
	{
		ErrorList programResults = _hardwareService.GetProgramResults(jobId, out var _);
		if (programResults.ErrorCount == 0)
		{
			UpdateLastUnknownSyncDate();
		}
		else
		{
			LocationSyncJobProcessorBase.LogErrors(programResults, "SyncUnknownLocations");
		}
	}

	private static void UpdateLastUnknownSyncDate()
	{
		string text = DateTime.Now.ToString();
		LogHelper.Instance.Log("SyncUnknownProcessor: Setting LastUnknownSyncDate to " + text);
		ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Local Services\\Hardware Services", "LastUknownSyncDate", text);
	}

	private void SaveLocationCounts(Dictionary<int, SyncCount> locationCounts)
	{
		if (!_fileService.Set("SyncCounts", new SyncCounts
		{
			Counts = locationCounts
		}))
		{
			LogHelper.Instance.Log("SyncUnknownProcessor failed to save sync counts.");
		}
	}

	private Dictionary<int, SyncCount> GetLocationCounts()
	{
		if (_fileService.Get<SyncCounts>("SyncCounts", out var deserializeObject))
		{
			return deserializeObject.Counts;
		}
		return new Dictionary<int, SyncCount>();
	}

	private static int LocationKey(int Deck, int Slot)
	{
		return Deck * 1000 + Slot;
	}
}
