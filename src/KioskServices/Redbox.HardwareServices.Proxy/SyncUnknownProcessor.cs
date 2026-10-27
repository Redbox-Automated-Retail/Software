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
		IHardwareState service = ServiceLocator.Instance.GetService<IHardwareState>();
		if (service == null)
		{
			LogHelper.Instance.Log("SyncUnknownProcessor: Unable to locate Hardware State.");
			return;
		}
		_qlmDeck = service.QlmDeckAsNumber;
		IEngineApplication service2 = ServiceLocator.Instance.GetService<IEngineApplication>();
		_dataPath = Path.Combine(service2.DataPath, "SyncUnknowns");
		_fileService = new DataFileStoreService();
		_fileService.Initialize("SyncUnknownCounts", _dataPath, "json", "corrupt", useEncryption: false);
		IConfigurationService service3 = ServiceLocator.Instance.GetService<IConfigurationService>();
		if (!service3.TryGetValue<bool>("system", "General", "SyncUnknownEnabled", out _isEnabled))
		{
			_isEnabled = true;
		}
		service3.TryGetValue<int>("system", "General", "MaxUnknownSyncs", out _maxSyncCount);
		if (_maxSyncCount == 0)
		{
			_maxSyncCount = 3;
		}
		if (!service3.TryGetValue<string>("system", "General", "SyncUnknownStartTimeRangeBegin", out var value))
		{
			value = "1:00";
		}
		if (!service3.TryGetValue<string>("system", "General", "SyncUnknownStartTimeRangeEnd", out var value2))
		{
			value2 = "3:00";
		}
		_timespanBegin = TimeSpan.Parse(value);
		_timespanEnd = TimeSpan.Parse(value2);
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
		bool flag = forceSync;
		DateTime value = ScheduleSyncTime(_timespanBegin, _timespanEnd, startIfInwindow: true, 0);
		if (!forceSync)
		{
			IStoreManager service = ServiceLocator.Instance.GetService<IStoreManager>();
			if (service != null)
			{
				_ = service.KioskId;
				if (0 == 0 && service.KioskId != 0L)
				{
					DateTime result;
					if (IsJobAlreadyScheduled(new List<string> { "SyncUnknowns", "SyncUnknownLocations" }))
					{
						LogHelper.Instance.Log($"HardwareFunctions.SyncUnknowns: Uknown Sync already scheduled. This run would have been scheduled for {value.ToString()}. Run Sync = {flag}");
						flag = false;
					}
					else if (DateTime.TryParse(_machineSettings.GetValue("Local Services\\Hardware Services", "LastUknownSyncDate", string.Empty), out result))
					{
						flag = value.Date > result.Date;
						LogHelper.Instance.Log($"HardwareFunctions.SyncUnknowns: Previous Uknown Sync run was {result.ToString()}. Next run scheduled for {value.ToString()}. Run Sync = {flag}");
					}
					else
					{
						flag = true;
						LogHelper.Instance.Log("HardwareFunctions.SyncUnknowns: No previous Unknown Sync run found. Running Sync");
					}
					goto IL_0134;
				}
			}
			flag = false;
			LogHelper.Instance.Log("No Kiosk ID was found. SyncUnknowns will not be run today.");
			UpdateLastUnknownSyncDate();
		}
		goto IL_0134;
		IL_0134:
		if (flag)
		{
			StartSync("SyncUnknowns", value);
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
		List<int> list = new List<int>();
		foreach (IHardwareResult hardwareResult in hardwareResults)
		{
			if (!(hardwareResult.Code == "SoftSync"))
			{
				continue;
			}
			int key = LocationKey(hardwareResult.Deck, hardwareResult.Slot);
			if (hardwareResult.ItemID == "UNKNOWN" && (!_qlmDeck.HasValue || hardwareResult.Deck != _qlmDeck))
			{
				bool flag = true;
				if (locationCounts.ContainsKey(key))
				{
					if (locationCounts[key].TimesSyncedUnknown >= _maxSyncCount)
					{
						flag = false;
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
				if (flag)
				{
					list.Add(hardwareResult.Slot);
					list.Add(hardwareResult.Deck);
				}
			}
			else if (locationCounts.ContainsKey(key))
			{
				locationCounts.Remove(key);
			}
		}
		return list;
	}

	private bool QueueSyncLocations(string jobId, List<int> syncLocations)
	{
		ErrorList source = _hardwareService.SyncLocations(null, null, "SyncUnknownLocations", null, syncLocations, out var jobId2);
		bool flag = !string.IsNullOrEmpty(jobId) && !source.Any();
		string text = (flag ? "Successfully" : "Unsuccessfully");
		LogHelper.Instance.Log(string.Format("{0} scheduled job: {1} - {2} with {3} locations.", text, jobId2 ?? string.Empty, "SyncUnknownLocations", syncLocations.Count() / 2));
		return flag;
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
