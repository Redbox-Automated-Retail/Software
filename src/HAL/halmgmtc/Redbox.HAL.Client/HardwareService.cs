using System.Collections.Generic;
using System.Text;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Core;
using Redbox.HAL.IPC.Framework;
using Redbox.IPC.Framework;

namespace Redbox.HAL.Client;

public sealed class HardwareService
{
	private const int DefaultTimeout = 60000;

	public bool HasABEDevice
	{
		get
		{
			using MachineConfiguration machineConfiguration = new MachineConfiguration(this);
			machineConfiguration.Run();
			return machineConfiguration.HasABEDevice;
		}
	}

	public bool HasFraudDevice
	{
		get
		{
			using MachineConfiguration machineConfiguration = new MachineConfiguration(this);
			machineConfiguration.Run();
			return machineConfiguration.HasFraudDevice;
		}
	}

	public int? CommandTimeout { get; set; }

	internal IPCProtocol Protocol { get; private set; }

	public HardwareService(IPCProtocol protocol)
	{
		Protocol = protocol;
	}

	public HardwareCommandResult Init(HardwareJobSchedule schedule, out HardwareJob job)
	{
		return ScheduleJob("init", schedule, out job);
	}

	public HardwareCommandResult Unload(HardwareJobSchedule schedule, out HardwareJob job)
	{
		return ScheduleJob("qlm-unload", schedule, out job);
	}

	public HardwareCommandResult CleanZone(HardwareJobSchedule schedule, out HardwareJob job)
	{
		return ScheduleJob("clean-vmz", schedule, out job);
	}

	public HardwareCommandResult CleanZone(out HardwareJob job)
	{
		HardwareJobSchedule schedule = new HardwareJobSchedule
		{
			Priority = HardwareJobPriority.Low
		};
		return ScheduleJob("clean-vmz", schedule, out job);
	}

	public HardwareCommandResult Thin(string[] ids, HardwareJobSchedule schedule, out HardwareJob job)
	{
		HardwareCommandResult hardwareCommandResult = ScheduleJob("thin", schedule, out job);
		if (hardwareCommandResult.Success)
		{
			PushArray(ids, job);
		}
		return hardwareCommandResult;
	}

	public HardwareCommandResult ThinVMZ(string[] toBin, string[] rebalance, string[] thins, HardwareJobSchedule schedule, out HardwareJob job)
	{
		HardwareCommandResult hardwareCommandResult = ScheduleJob("thin-vmz", schedule, out job);
		if (hardwareCommandResult.Success)
		{
			PushArray(thins, job);
			PushArray(null, job);
			PushArray(rebalance, job);
			PushArray(toBin, job);
		}
		return hardwareCommandResult;
	}

	public HardwareCommandResult ThinVMZ(string[] toBin, string[] rebalance, string[] thinRedeploys, string[] thins, HardwareJobSchedule schedule, out HardwareJob job)
	{
		HardwareCommandResult hardwareCommandResult = ScheduleJob("thin-vmz", schedule, out job);
		if (hardwareCommandResult.Success)
		{
			PushArray(thins, job);
			PushArray(thinRedeploys, job);
			PushArray(rebalance, job);
			PushArray(toBin, job);
		}
		return hardwareCommandResult;
	}

	public HardwareCommandResult ThinVMZ(string[] toBin, string[] thins, HardwareJobSchedule schedule, out HardwareJob job)
	{
		HardwareCommandResult hardwareCommandResult = ScheduleJob("thin-vmz", schedule, out job);
		if (hardwareCommandResult.Success)
		{
			PushArray(thins, job);
			PushArray(null, job);
			PushArray(null, job);
			PushArray(toBin, job);
		}
		return hardwareCommandResult;
	}

	public HardwareCommandResult GetNonBarcodeInventory(HardwareJobSchedule schedule, out HardwareJob job)
	{
		return ScheduleJob("get-non-barcode-inventory", schedule, out job);
	}

	public HardwareCommandResult GetInventoryStats(HardwareJobSchedule schedule, out HardwareJob job)
	{
		return ScheduleJob("get-inventory-stats", schedule, out job);
	}

	public HardwareCommandResult ChangeAudioChannelState(HardwareJobSchedule schedule, SpeakerState newState, out HardwareJob job)
	{
		HardwareCommandResult hardwareCommandResult = ScheduleJob("change-audio-channel-state", schedule, out job);
		if (hardwareCommandResult.Success)
		{
			job.Push(newState.ToString());
		}
		return hardwareCommandResult;
	}

	public HardwareCommandResult ResetLocations(List<Location> locs, HardwareJobSchedule schedule, out HardwareJob job)
	{
		HardwareCommandResult hardwareCommandResult = ScheduleJob("reset-locations", schedule, out job);
		if (hardwareCommandResult.Success)
		{
			PushLocs(locs, job);
		}
		return hardwareCommandResult;
	}

	public HardwareCommandResult SyncUnknowns(HardwareJobSchedule schedule, out HardwareJob job)
	{
		HardwareCommandResult hardwareCommandResult = ExecuteServiceCommand("SERVICE sync-unknowns");
		job = (hardwareCommandResult.Success ? HardwareJob.Parse(this, hardwareCommandResult.CommandMessages[0]) : null);
		return hardwareCommandResult;
	}

	public HardwareCommandResult PrepUnloadZone(int rotation, HardwareJobSchedule schedule, out HardwareJob job)
	{
		HardwareCommandResult hardwareCommandResult = ScheduleJob("prep-zone-for-unload", schedule, out job);
		if (hardwareCommandResult.Success)
		{
			job.Push(0);
			job.Push(rotation);
		}
		return hardwareCommandResult;
	}

	public HardwareCommandResult PrepUnloadZone(int rotation, int deckCount, HardwareJobSchedule schedule, out HardwareJob job)
	{
		HardwareCommandResult hardwareCommandResult = ScheduleJob("prep-zone-for-unload", schedule, out job);
		if (hardwareCommandResult.Success)
		{
			job.Push(deckCount);
			job.Push(rotation);
		}
		return hardwareCommandResult;
	}

	public HardwareCommandResult PrepDeckForRedeploy(string deckNumber, HardwareJobSchedule schedule, out HardwareJob job)
	{
		HardwareCommandResult hardwareCommandResult = ScheduleJob("prep-deck-for-redeploy", schedule, out job);
		if (hardwareCommandResult.Success)
		{
			job.Push(deckNumber);
			job.Push(1);
		}
		return hardwareCommandResult;
	}

	public HardwareCommandResult PrepForRedeploy(string[] decks, HardwareJobSchedule schedule, out HardwareJob job)
	{
		HardwareCommandResult hardwareCommandResult = ScheduleJob("prep-deck-for-redeploy", schedule, out job);
		if (hardwareCommandResult.Success)
		{
			PushArray(decks, job);
		}
		return hardwareCommandResult;
	}

	public HardwareCommandResult VMZRemovedThins(HardwareJobSchedule schedule, out HardwareJob job)
	{
		return ScheduleJob("vmz-removed-thins", schedule, out job);
	}

	public HardwareCommandResult ClearMerchStatus(string[] barcodesToClear, HardwareJobSchedule schedule, out HardwareJob job)
	{
		HardwareCommandResult hardwareCommandResult = ScheduleJob("clear-merch-status", schedule, out job);
		if (hardwareCommandResult.Success)
		{
			PushArray(barcodesToClear, job);
		}
		return hardwareCommandResult;
	}

	public HardwareCommandResult LoadBin(List<Location> locs, HardwareJobSchedule schedule, out HardwareJob job)
	{
		HardwareCommandResult hardwareCommandResult = ScheduleJob("load-bin", schedule, out job);
		if (hardwareCommandResult.Success)
		{
			PushLocs(locs, job);
			job.Push("LOCATION");
		}
		return hardwareCommandResult;
	}

	public HardwareCommandResult Vend(string[] ids, HardwareJobSchedule schedule, out HardwareJob job)
	{
		HardwareCommandResult hardwareCommandResult = ScheduleJob("vend", schedule, out job);
		if (hardwareCommandResult.Success)
		{
			PushArray(ids, job);
			job.Push("BY-ID");
		}
		return hardwareCommandResult;
	}

	public HardwareCommandResult Vend(List<Location> locations, HardwareJobSchedule schedule, out HardwareJob job)
	{
		HardwareCommandResult hardwareCommandResult = ScheduleJob("vend", schedule, out job);
		if (hardwareCommandResult.Success)
		{
			PushLocs(locations, job);
			job.Push("BY-LOCATION");
		}
		return hardwareCommandResult;
	}

	public HardwareCommandResult GetBarcodeVMZPosition(string[] idsToQuery, HardwareJobSchedule schedule, out HardwareJob job)
	{
		HardwareCommandResult hardwareCommandResult = ScheduleJob("get-barcode-vmz-location", schedule, out job);
		if (hardwareCommandResult.Success)
		{
			PushArray(idsToQuery, job);
		}
		return hardwareCommandResult;
	}

	public HardwareCommandResult VMZDetail(HardwareJobSchedule schedule, out HardwareJob job)
	{
		return ScheduleJob("vmz-details", schedule, out job);
	}

	public HardwareCommandResult VMZMerchSummary(HardwareJobSchedule schedule, out HardwareJob job)
	{
		return ScheduleJob("vmz-merch-summary", schedule, out job);
	}

	public HardwareCommandResult FileMarkerDisk(int deck, int slot, HardwareJobSchedule schedule, out HardwareJob job)
	{
		HardwareCommandResult hardwareCommandResult = ScheduleJob("file-marker-disk", schedule, out job);
		if (hardwareCommandResult.Success)
		{
			job.Push(slot);
			job.Push(deck);
		}
		return hardwareCommandResult;
	}

	public HardwareCommandResult MerchandizeClearAndOffset(int deck, int slot, HardwareJobSchedule schedule, out HardwareJob job)
	{
		HardwareCommandResult hardwareCommandResult = ScheduleJob("merch-clear-and-offset", schedule, out job);
		if (hardwareCommandResult.Success)
		{
			job.Push(slot);
			job.Push(deck);
		}
		return hardwareCommandResult;
	}

	public HardwareCommandResult NotifyMaintModeState(bool goingIntoMM)
	{
		return ExecuteCommand($"SERVICE mm-status status: '{goingIntoMM.ToString()}'");
	}

	public HardwareCommandResult GetNonThinsInVMZ(HardwareJobSchedule schedule, out HardwareJob job)
	{
		return ScheduleJob("get-nonthins-in-vmz", schedule, out job);
	}

	public HardwareCommandResult GetBarcodesInBin(HardwareJobSchedule schedule, out HardwareJob job)
	{
		return ScheduleJob("get-barcodes-in-bin", schedule, out job);
	}

	public HardwareCommandResult HardSync(SyncRange range, HardwareJobSchedule schedule, out HardwareJob job)
	{
		HardwareCommandResult hardwareCommandResult = ScheduleJob("sync", schedule, out job);
		if (hardwareCommandResult.Success)
		{
			job.Push(range.Slots.End, range.EndDeck, range.Slots.Start, range.StartDeck);
		}
		return hardwareCommandResult;
	}

	public HardwareCommandResult FieldInsertFraudWithCheck(HardwareJobSchedule schedule, out HardwareJob job)
	{
		return ScheduleJob("field-insert-with-fraud-check", schedule, out job);
	}

	public HardwareCommandResult FraudHardwarePost(HardwareJobSchedule schedule, out HardwareJob job)
	{
		return ScheduleJob("fraud-sensor-post-test", schedule, out job);
	}

	public HardwareCommandResult ReadFraudDisc(HardwareJobSchedule schedule, out HardwareJob job)
	{
		return ScheduleJob("read-fraud-disc", schedule, out job);
	}

	public HardwareCommandResult Return(HardwareJobSchedule schedule, out HardwareJob job)
	{
		return ScheduleJob("return", schedule, out job);
	}

	public HardwareCommandResult SoftSync(HardwareJobSchedule schedule, out HardwareJob job)
	{
		return ScheduleJob("soft-sync", schedule, out job);
	}

	public HardwareCommandResult QlmUnload(HardwareJobSchedule schedule, out HardwareJob job)
	{
		return ScheduleJob("qlm-unload", schedule, out job);
	}

	public HardwareCommandResult CheckPicker(HardwareJobSchedule schedule, out HardwareJob job)
	{
		return ScheduleJob("check-picker", schedule, out job);
	}

	public HardwareCommandResult GetMachineInfo(HardwareJobSchedule schedule, out HardwareJob job)
	{
		return ScheduleJob("get-machine-info", schedule, out job);
	}

	public HardwareCommandResult GetHardwareStatus(HardwareJobSchedule schedule, out HardwareJob job)
	{
		return ScheduleJob("hardware-status", schedule, out job);
	}

	public HardwareCommandResult QlmUnloadAndThin(string[] ids, HardwareJobSchedule schedule, out HardwareJob job)
	{
		HardwareCommandResult hardwareCommandResult = ScheduleJob("unload-thin", schedule, out job);
		if (hardwareCommandResult.Success)
		{
			PushArray(ids, job);
		}
		return hardwareCommandResult;
	}

	public HardwareCommandResult VendUnknown(int deck, int slot, HardwareJobSchedule schedule, out HardwareJob job)
	{
		HardwareCommandResult hardwareCommandResult = ScheduleJob("vend-unknown", schedule, out job);
		if (hardwareCommandResult.Success)
		{
			job.Push(slot);
			job.Push(deck);
		}
		return hardwareCommandResult;
	}

	public HardwareCommandResult ReturnUnknown(HardwareJobSchedule schedule, string returnTime, out HardwareJob job)
	{
		HardwareCommandResult hardwareCommandResult = ScheduleJob("return-unknown", schedule, out job);
		if (hardwareCommandResult.Success)
		{
			job.Push(returnTime);
		}
		return hardwareCommandResult;
	}

	public HardwareCommandResult PreposVend(HardwareJobSchedule schedule, string[] ids, out HardwareJob job)
	{
		HardwareCommandResult hardwareCommandResult = ScheduleJob("vend", schedule, out job);
		if (hardwareCommandResult.Success)
		{
			PushArray(ids, job);
			job.Push("PREPOSITION-PICKER");
		}
		return hardwareCommandResult;
	}

	public HardwareCommandResult ExchangerStatus(HardwareJobSchedule schedule, out HardwareJob job)
	{
		return ScheduleJob("air-exchanger-status", schedule, out job);
	}

	public HardwareCommandResult MarkBarcodesUnknown(HardwareJobSchedule s, string[] barcodes, out HardwareJob job)
	{
		HardwareCommandResult hardwareCommandResult = ScheduleJob("mark-barcodes-unknown", s, out job);
		if (hardwareCommandResult.Success)
		{
			PushArray(barcodes, job);
		}
		return hardwareCommandResult;
	}

	public HardwareCommandResult MarkLocationsUnknown(HardwareJobSchedule s, List<Location> locs, out HardwareJob job)
	{
		HardwareCommandResult hardwareCommandResult = ScheduleJob("mark-locations-unknown", s, out job);
		if (hardwareCommandResult.Success)
		{
			PushLocs(locs, job);
		}
		return hardwareCommandResult;
	}

	public HardwareCommandResult ResetTouchscreenController(HardwareJobSchedule s, out HardwareJob job)
	{
		return ScheduleJob("reset-touchscreen-controller", s, out job);
	}

	public HardwareCommandResult GetConfiguration(string name)
	{
		return ExecuteCommand($"CONFIG get name: '{name}'");
	}

	public HardwareCommandResult SetConfiguration(string name, string xmlConfig)
	{
		byte[] inputArray = CompressionAlgorithm.GetAlgorithm(CompressionType.LZMA).Compress(Encoding.ASCII.GetBytes(xmlConfig));
		return ExecuteCommand($"CONFIG set name: '{name}' data: '{ByteHelper.ToBase64(inputArray)}'");
	}

	public HardwareCommandResult LoadConfiguration(string path)
	{
		string command = string.Format("CONFIG load {0}", (path != null) ? ("path: '" + path + "'") : string.Empty);
		return ExecuteCommand(command);
	}

	public HardwareCommandResult SaveConfiguration(string path)
	{
		string command = string.Format("CONFIG save {0}", (path != null) ? ("path: '" + path + "'") : string.Empty);
		return ExecuteCommand(command);
	}

	public HardwareCommandResult RestoreConfiguration(string path)
	{
		string command = string.Format("CONFIG restore {0}", (path != null) ? ("path: '" + path + "'") : string.Empty);
		return ExecuteCommand(command);
	}

	public HardwareCommandResult GetInventoryState()
	{
		return ExecuteCommand("CONFIG get-inventory-state");
	}

	public HardwareCommandResult SetInventoryState(string xml)
	{
		byte[] inputArray = CompressionAlgorithm.GetAlgorithm(CompressionType.LZMA).Compress(Encoding.ASCII.GetBytes(xml));
		return ExecuteCommand($"CONFIG set-inventory-state data: '{ByteHelper.ToBase64(inputArray)}'");
	}

	public HardwareCommandResult GetProgramScript(string programName)
	{
		return ExecuteCommand($"PROGRAM get name: '{programName}'");
	}

	public HardwareCommandResult SetProgramScript(string programName, string script)
	{
		byte[] inputArray = CompressionAlgorithm.GetAlgorithm(CompressionType.LZMA).Compress(Encoding.ASCII.GetBytes(script));
		return ExecuteCommand($"PROGRAM set name: '{programName}' data: '{ByteHelper.ToBase64(inputArray)}'");
	}

	public HardwareCommandResult SetProgramRequiresClientConnection(string programName, bool requiresClientConnection)
	{
		return ExecuteCommand($"PROGRAM properties name: '{programName}' requires-client-connection: {requiresClientConnection}");
	}

	public HardwareCommandResult ResumeJobs(HardwareJob[] jobs)
	{
		HardwareCommandResult hardwareCommandResult = new HardwareCommandResult();
		for (int i = 0; i < jobs.Length; i++)
		{
			HardwareCommandResult hardwareCommandResult2 = jobs[i].Resume();
			if (!hardwareCommandResult2.Success && hardwareCommandResult2.Errors.Count > 0)
			{
				hardwareCommandResult.Errors.AddRange(hardwareCommandResult2.Errors);
			}
		}
		return hardwareCommandResult;
	}

	public HardwareCommandResult SuspendJobsBelowPriority(HardwareJobPriority priority, out HardwareJob[] suspendedJobs)
	{
		List<HardwareJob> list = new List<HardwareJob>();
		HardwareJob[] jobs = null;
		HardwareCommandResult hardwareCommandResult = new HardwareCommandResult();
		HardwareCommandResult jobs2 = GetJobs(out jobs);
		if (!jobs2.Success)
		{
			if (jobs2.Errors.Count > 0)
			{
				hardwareCommandResult.Errors.AddRange(jobs2.Errors);
			}
			suspendedJobs = list.ToArray();
			return hardwareCommandResult;
		}
		HardwareJob[] array = jobs;
		foreach (HardwareJob hardwareJob in array)
		{
			if (hardwareJob.ID != Constants.ExecutionContexts.ImmediateModeContext && (hardwareJob.Status == HardwareJobStatus.Pending || hardwareJob.Status == HardwareJobStatus.Running) && hardwareJob.Priority > priority)
			{
				HardwareCommandResult hardwareCommandResult2 = hardwareJob.Suspend();
				if (hardwareCommandResult2.Success)
				{
					list.Add(hardwareJob);
				}
				else if (hardwareCommandResult2.Errors.Count > 0)
				{
					hardwareCommandResult.Errors.AddRange(hardwareCommandResult2.Errors);
				}
			}
		}
		suspendedJobs = list.ToArray();
		return hardwareCommandResult;
	}

	public HardwareCommandResult SuspendAll()
	{
		return ExecuteCommand("JOB suspend-all");
	}

	public HardwareCommandResult ResumeAll()
	{
		return ExecuteCommand("JOB resume-all");
	}

	public HardwareCommandResult CollectGarbage(bool force)
	{
		return ExecuteCommand(string.Format("JOB collect-garbage force: {0}", force ? "true" : "false"));
	}

	public HardwareCommandResult TrashJob(string jobId)
	{
		return ExecuteCommand($"JOB trash job: {jobId}");
	}

	public HardwareCommandResult GetJob(string jobId, out HardwareJob job)
	{
		job = null;
		HardwareCommandResult hardwareCommandResult = ExecuteCommand($"JOB get job: '{jobId}'");
		if (hardwareCommandResult.Success)
		{
			job = HardwareJob.Parse(this, hardwareCommandResult.CommandMessages[0]);
		}
		return hardwareCommandResult;
	}

	public HardwareCommandResult GetJobs(out HardwareJob[] jobs)
	{
		HardwareCommandResult hardwareCommandResult = ExecuteCommand("JOB list");
		jobs = (hardwareCommandResult.Success ? hardwareCommandResult.CommandMessages.ConvertAll((string each) => HardwareJob.Parse(this, each)).ToArray() : new HardwareJob[0]);
		return hardwareCommandResult;
	}

	public HardwareCommandResult CompileProgram(string path, string programName, bool requiresClientConnection)
	{
		return ExecuteCommand($"PROGRAM compile path: @'{path}' name: '{programName}' requires-client-connection: {requiresClientConnection}");
	}

	public HardwareCommandResult RemoveProgram(string programName)
	{
		return ExecuteCommand($"PROGRAM remove name: '{programName}'");
	}

	public HardwareCommandResult ScheduleJob(string programName, string label, bool enableDebugging, HardwareJobSchedule schedule, out HardwareJob job)
	{
		string text = $"JOB schedule name: '{programName}' priority: {schedule.Priority}";
		if (enableDebugging)
		{
			text += $" debugging: {enableDebugging}";
		}
		if (schedule.StartTime.HasValue)
		{
			text += $" startTime: '{schedule.StartTime}'";
		}
		if (label != null)
		{
			text += $" label: '{label}'";
		}
		job = null;
		HardwareCommandResult hardwareCommandResult = ExecuteCommand(text);
		if (hardwareCommandResult.Success)
		{
			job = HardwareJob.Parse(this, hardwareCommandResult.CommandMessages[0]);
		}
		return hardwareCommandResult;
	}

	public HardwareCommandResult GetPrograms()
	{
		return ExecuteCommand("PROGRAM list");
	}

	public HardwareCommandResult ExecuteImmediate(string statement, int? timeout, out HardwareJob job)
	{
		job = null;
		HardwareCommandResult hardwareCommandResult = ExecuteCommand($"JOB execute-immediate statement: '{statement}'", timeout.HasValue ? timeout.Value : 30000);
		if (hardwareCommandResult.Success)
		{
			job = HardwareJob.Parse(this, hardwareCommandResult.CommandMessages[0]);
		}
		return hardwareCommandResult;
	}

	public HardwareCommandResult ExecuteImmediate(string statement, out HardwareJob job)
	{
		job = null;
		HardwareCommandResult hardwareCommandResult = ExecuteCommand($"JOB execute-immediate statement: '{statement}'");
		if (hardwareCommandResult.Success)
		{
			job = HardwareJob.Parse(this, hardwareCommandResult.CommandMessages[0]);
		}
		return hardwareCommandResult;
	}

	public HardwareCommandResult GetSchedulerStatus(out string status)
	{
		status = null;
		HardwareCommandResult hardwareCommandResult = ExecuteCommand("JOB scheduler-status");
		if (hardwareCommandResult.Success)
		{
			status = hardwareCommandResult.CommandMessages[0];
		}
		return hardwareCommandResult;
	}

	public HardwareCommandResult ExecuteImmediateProgram(byte[] bytes, out HardwareJob job)
	{
		job = null;
		HardwareCommandResult hardwareCommandResult = ExecuteCommand($"JOB execute-immediate-base64 statement: '{ByteHelper.ToBase64(bytes)}'");
		if (hardwareCommandResult.Success)
		{
			job = HardwareJob.Parse(this, hardwareCommandResult.CommandMessages[0]);
		}
		return hardwareCommandResult;
	}

	public HardwareCommandResult GetInitStatus(out string status)
	{
		status = null;
		HardwareCommandResult hardwareCommandResult = ExecuteCommand("JOB init-status");
		if (hardwareCommandResult.Success)
		{
			status = hardwareCommandResult.CommandMessages[0];
		}
		return hardwareCommandResult;
	}

	public HardwareCommandResult GetKioskID(out string id)
	{
		id = "UNKNOWN";
		HardwareCommandResult hardwareCommandResult = ExecuteCommand("SERVICE get-kiosk-id");
		if (hardwareCommandResult.Success)
		{
			id = hardwareCommandResult.CommandMessages[0];
		}
		return hardwareCommandResult;
	}

	public HardwareCommandResult GetQuickReturnStatus(out string status)
	{
		status = null;
		HardwareCommandResult hardwareCommandResult = ExecuteCommand("JOB quick-return-status");
		if (hardwareCommandResult.Success)
		{
			status = hardwareCommandResult.CommandMessages[0];
		}
		return hardwareCommandResult;
	}

	public HardwareCommandResult ServiceUnknownSync()
	{
		return ExecuteCommand("SERVICE sync-unknowns");
	}

	public HardwareCommandResult ServiceEmptySync()
	{
		return ExecuteCommand("SERVICE sync-empty");
	}

	public HardwareCommandResult FullSync()
	{
		return ExecuteCommand("SERVICE full-sync");
	}

	public HardwareCommandResult TestSomeData(int dataSize)
	{
		return ExecuteCommand($"ipctest test-ipc-xfer size: '{dataSize.ToString()}'");
	}

	public HardwareCommandResult GetKioskFunctionCheckData(out IList<IKioskFunctionCheckData> sessions)
	{
		sessions = new List<IKioskFunctionCheckData>();
		using KioskFunctionCheckExecutor kioskFunctionCheckExecutor = new KioskFunctionCheckExecutor(this);
		kioskFunctionCheckExecutor.Run();
		if (kioskFunctionCheckExecutor.ScheduleResult.Success)
		{
			foreach (IKioskFunctionCheckData session in kioskFunctionCheckExecutor.Sessions)
			{
				sessions.Add(session);
			}
		}
		return kioskFunctionCheckExecutor.ScheduleResult;
	}

	public HardwareCommandResult ExecuteServiceCommand(string command)
	{
		return ExecuteCommand(command);
	}

	public HardwareCommandResult ExecuteServiceCommand(string command, int timeout)
	{
		return ExecuteCommand(command, timeout);
	}

	public HardwareCommandResult QueryHealth(string outputFile)
	{
		return ExecuteCommand($"HEALTH check path: @'{outputFile}'");
	}

	internal HardwareCommandResult ExecuteCommand(string command)
	{
		return ClientCommand<HardwareCommandResult>.ExecuteCommand(Protocol, command);
	}

	internal HardwareCommandResult ExecuteCommand(string command, int timeout)
	{
		return ClientCommand<HardwareCommandResult>.ExecuteCommand(Protocol, timeout, command);
	}

	internal IIpcClientSession GetSession()
	{
		return ClientSessionFactory.GetClientSession(Protocol);
	}

	private HardwareCommandResult ScheduleJob(string name, HardwareJobSchedule schedule, out HardwareJob job)
	{
		return ScheduleJob(name, null, enableDebugging: false, schedule, out job);
	}

	private void PushLocs(List<Location> locs, HardwareJob job)
	{
		if (locs == null || locs.Count == 0)
		{
			job.Push(0);
			return;
		}
		foreach (Location loc in locs)
		{
			job.Push(loc.Slot);
			job.Push(loc.Deck);
		}
		job.Push(locs.Count);
	}

	private void PushArray(string[] array, HardwareJob job)
	{
		if (array == null || array.Length == 0)
		{
			job.Push(0);
		}
		else
		{
			job.Push(array);
			job.Push(array.Length);
		}
	}
}
