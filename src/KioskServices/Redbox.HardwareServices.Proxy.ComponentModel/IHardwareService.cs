using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.HardwareServices.Proxy.ComponentModel;

public interface IHardwareService
{
	int CurrentCameraGeneration { get; }

	int BarcodeDecoder { get; }

	void Reset();

	bool CanSwitch();

	ErrorList Initialize();

	bool IsTest();

	ErrorList Init(out string jobId, HardwareEventHandler eventHandler, HardwareStatusChangeHandler statusChangeHandler);

	ErrorList Vend(HardwareEventHandler eventHandler, HardwareStatusChangeHandler statusChangeHandler, string[] barcodes, int? prepositionedDeck, int? prepositionedSlot, out string jobId);

	ErrorList PreposVend(HardwareEventHandler eventHandler, HardwareStatusChangeHandler statusChangeHandler, string[] barcodes, out string jobId);

	ErrorList VendLocations(HardwareEventHandler eventHandler, HardwareStatusChangeHandler statusChangeHandler, IList<int> locations, out string jobId);

	ErrorList ResetLocations(HardwareEventHandler eventHandler, HardwareStatusChangeHandler statusChangeHandler, IList<int> locations, out string jobId);

	ErrorList VendUnknown(HardwareEventHandler eventHandler, HardwareStatusChangeHandler statusChangeHandler, int deck, int slot, out string jobId);

	ErrorList Return(HardwareEventHandler eventHandler, HardwareStatusChangeHandler statusChangeHandler, out string jobId);

	ErrorList ReturnUnknown(HardwareEventHandler eventHandler, HardwareStatusChangeHandler statusChangeHandler, string returnTime, out string jobId);

	ErrorList SoftSync(HardwareEventHandler eventHandler, HardwareStatusChangeHandler statusChangeHandler, out string jobId);

	ErrorList SoftSync(HardwareEventHandler eventHandler, HardwareStatusChangeHandler statusChangeHandler, DateTime? startTime, string label, out string jobId);

	ErrorList Sync(HardwareEventHandler eventHandler, HardwareStatusChangeHandler statusChangeHandler, string label, DateTime? startTime, int startDeck, int startSlot, int endDeck, int endSlot, out string jobId);

	ErrorList CheckPicker(HardwareEventHandler eventHandler, HardwareStatusChangeHandler statusChangeHandler, out string jobId);

	ErrorList SyncLocations(HardwareEventHandler eventHandler, HardwareStatusChangeHandler statusChangeHandler, string label, DateTime? startTime, IList<int> locations, out string jobId);

	ErrorList GetMachineInfo(HardwareEventHandler eventHandler, HardwareStatusChangeHandler statusChangeHandler, out string jobId);

	ErrorList GetHardwareStatus(DateTime? startTime, HardwareEventHandler eventHandler, HardwareStatusChangeHandler statusChangeHandler, out string jobId);

	ErrorList QlmUnload(string label, DateTime? startTime, HardwareEventHandler eventHandler, HardwareStatusChangeHandler statusChangeHandler, out string jobId);

	ErrorList QlmThin(string label, DateTime? startTime, string[] ids, HardwareEventHandler eventHandler, HardwareStatusChangeHandler statusChangeHandler, out string jobId);

	ErrorList VmzThin(string label, DateTime? startTime, string[] barcodesToThin, string[] rebalances, string[] redeployments, string[] barcodesToVmz, HardwareEventHandler eventHandler, HardwareStatusChangeHandler statusChangeHandler, out string jobId);

	ErrorList QlmUnloadAndThin(string label, DateTime? startTime, string[] ids, HardwareEventHandler eventHandler, HardwareStatusChangeHandler statusChangeHandler, out string jobId);

	ErrorList GetNonThinsInVMZ(HardwareEventHandler eventHandler, HardwareStatusChangeHandler statusChangeHandler, out string jobId);

	ErrorList VMZRemovedThins(HardwareEventHandler eventHandler, HardwareStatusChangeHandler statusChangeHandler, out string jobId);

	ErrorList GetBarcodesInBin(HardwareEventHandler eventHandler, HardwareStatusChangeHandler statusChangeHandler, out string jobId);

	ErrorList GetInventoryStats(HardwareEventHandler eventHandler, HardwareStatusChangeHandler statusChangeHandler, out string jobId);

	ErrorList VMZMerchSummary(HardwareEventHandler eventHandler, HardwareStatusChangeHandler statusChangeHandler, out string jobId);

	ErrorList FileMarkerDisk(HardwareEventHandler eventHandler, HardwareStatusChangeHandler statusChangeHandler, int markerDeck, int markerSlot, out string jobId);

	ErrorList ClearMerchStatus(string[] ids, HardwareEventHandler eventHandler, HardwareStatusChangeHandler statusChangeHandler, out string jobId);

	ErrorList GetBarcodeVMZPosition(string[] ids, HardwareEventHandler eventHandler, HardwareStatusChangeHandler statusChangeHandler, out string jobId);

	ErrorList GetKioskFunctionCheckData(out IList<IKioskFunctionCheckData> data);

	ErrorList Compile(string path, string name);

	ErrorList ScheduleJob(string scriptName, string label, bool enableDebugging, string priority, string startTimeString, ReadOnlyCollection<object> stackValues, HardwareEventHandler eventHandler, HardwareStatusChangeHandler statusChangeHandler, bool allowMultiple, out string jobId);

	ErrorList Trash(string jobId);

	ErrorList Resume(string jobId);

	ErrorList Suspend(string jobId);

	ErrorList Terminate(string jobId);

	ErrorList ResumeAll();

	ErrorList SuspendAll();

	ErrorList GetJobErrors(string jobId, out ErrorList errors);

	ErrorList ResumeAllMaintenanceSuspendedJobs();

	ErrorList SuspendAllJobsForMaintenance();

	ErrorList GetSuspendedMaintenanceModeJobs(out ReadOnlyCollection<IHardwareJob> suspendedJobs);

	ErrorList GetProgramResults(string jobId, out ReadOnlyCollection<IHardwareResult> hardwareResults);

	ErrorList ExecuteImmediate(string command, HardwareImmediateExecutionHandler eventHandler, bool clearStack, string bundleName);

	ErrorList GetSchedulerStatus(out string status);

	ErrorList ExecuteServiceCommand(string command);

	ErrorList GetHardwareJobs(out ReadOnlyCollection<IHardwareJob> hardwareJobs);

	ErrorList GetInitStatus(out string status);

	ErrorList GetQuickReturnStatus(out string status);

	ErrorList NotifyMaintModeState(bool goingIntoMM);

	ErrorList ReadFraudDisc(HardwareEventHandler eventHandler, HardwareStatusChangeHandler statusChangeHandler, out string jobId);

	ErrorList PrepForRedeploy(string[] decksToChange, HardwareEventHandler eventHandler, HardwareStatusChangeHandler statusChangeHandler, out string jobId);

	ErrorList PrepUnloadZone(int rotation, int last_deck, HardwareEventHandler eventHandler, HardwareStatusChangeHandler statusChangeHandler, out string jobId);

	ErrorList HasAbeDevice(out bool hasAbeDevice);

	ErrorList SupportsRouterReset(out bool isSupported);

	ErrorList ResetTouchScreenController(HardwareEventHandler eventHandler, HardwareStatusChangeHandler statusChangeHandler, out string jobId);

	ErrorList PowerCycleRouter(HardwareEventHandler eventHandler, HardwareStatusChangeHandler statusChangeHandler, out string jobId);

	ErrorList GetHardwareCorrectionStats(out ReadOnlyCollection<IHardwareCorrectionStatistic> stats);

	ErrorList GetHardwareCorrectionStats(HardwareCorrectionStatisticType stat, out ReadOnlyCollection<IHardwareCorrectionStatistic> stats);

	ErrorList RemoveHardwareCorrectionStats(HardwareEventHandler eventHandler, HardwareStatusChangeHandler statusChangeHandler, out string jobId);

	ErrorList RemoveHardwareCorrectionStats(HardwareCorrectionStatisticType stat, HardwareEventHandler eventHandler, HardwareStatusChangeHandler statusChangeHandler, out string jobId);

	ErrorList IRHardwareInstalled(out DateTime? installDate);

	bool IsComplete(string status);
}
