using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Redbox.Core;
using Redbox.HardwareServices.Proxy.ComponentModel;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Lua;
using Redbox.Rental.Model.Health;

namespace Redbox.HardwareServices.Kernel;

public static class HardwareFunctions
{
	[KernelFunction(Name = "kernel.hardwareinint")]
	internal static LuaTable HardwareInit(HardwareEventHandler eventHandler, HardwareStatusChangeHandler statusChangeHandler)
	{
		IHardwareService service = ServiceLocator.Instance.GetService<IHardwareService>();
		if (service == null)
		{
			return null;
		}
		ErrorList errors = service.Init(out var jobId, eventHandler, statusChangeHandler);
		return CreateResultTable(jobId, errors);
	}

	[KernelFunction(Name = "kernel.hardwarecheckpicker")]
	internal static LuaTable HardwareCheckPicker(HardwareEventHandler eventHandler, HardwareStatusChangeHandler statusChangeHandler)
	{
		IHardwareService service = ServiceLocator.Instance.GetService<IHardwareService>();
		if (service == null)
		{
			return null;
		}
		ErrorList errors = service.CheckPicker(eventHandler, statusChangeHandler, out var jobId);
		return CreateResultTable(jobId, errors);
	}

	[KernelFunction(Name = "kernel.getnonthinsinvmz")]
	internal static LuaTable GetNonThinsInVMZ(HardwareEventHandler eventHandler, HardwareStatusChangeHandler statusChangeHandler)
	{
		IHardwareService service = ServiceLocator.Instance.GetService<IHardwareService>();
		if (service == null)
		{
			return null;
		}
		ErrorList nonThinsInVMZ = service.GetNonThinsInVMZ(eventHandler, statusChangeHandler, out var jobId);
		return CreateResultTable(jobId, nonThinsInVMZ);
	}

	[KernelFunction(Name = "kernel.vmzremovedthins")]
	internal static LuaTable VMZRemovedThins(HardwareEventHandler eventHandler, HardwareStatusChangeHandler statusChangeHandler)
	{
		IHardwareService service = ServiceLocator.Instance.GetService<IHardwareService>();
		if (service == null)
		{
			return null;
		}
		ErrorList errors = service.VMZRemovedThins(eventHandler, statusChangeHandler, out var jobId);
		return CreateResultTable(jobId, errors);
	}

	[KernelFunction(Name = "kernel.getbarcodesinbin")]
	internal static LuaTable GetBarcodesInBin(HardwareEventHandler eventHandler, HardwareStatusChangeHandler statusChangeHandler)
	{
		IHardwareService service = ServiceLocator.Instance.GetService<IHardwareService>();
		if (service == null)
		{
			return null;
		}
		ErrorList barcodesInBin = service.GetBarcodesInBin(eventHandler, statusChangeHandler, out var jobId);
		return CreateResultTable(jobId, barcodesInBin);
	}

	[KernelFunction(Name = "kernel.getinventorystats")]
	internal static LuaTable GetInventoryStats(HardwareEventHandler eventHandler, HardwareStatusChangeHandler statusChangeHandler)
	{
		IHardwareService service = ServiceLocator.Instance.GetService<IHardwareService>();
		if (service == null)
		{
			return null;
		}
		ErrorList inventoryStats = service.GetInventoryStats(eventHandler, statusChangeHandler, out var jobId);
		return CreateResultTable(jobId, inventoryStats);
	}

	[KernelFunction(Name = "kernel.vmzmerchsummary")]
	internal static LuaTable VMZMerchSummary(HardwareEventHandler eventHandler, HardwareStatusChangeHandler statusChangeHandler)
	{
		IHardwareService service = ServiceLocator.Instance.GetService<IHardwareService>();
		if (service == null)
		{
			return null;
		}
		ErrorList errors = service.VMZMerchSummary(eventHandler, statusChangeHandler, out var jobId);
		return CreateResultTable(jobId, errors);
	}

	[KernelFunction(Name = "kernel.filemarkerdisk")]
	internal static LuaTable FileMarkerDisk(HardwareEventHandler eventHandler, HardwareStatusChangeHandler statusChangeHandler, object markerDeck, object markerSlot)
	{
		LogHelper.Instance.Log($"KernelFunction filemarkerdisk| markerDeck:{markerDeck} markerSlot:{markerSlot}");
		IHardwareService service = ServiceLocator.Instance.GetService<IHardwareService>();
		if (service == null)
		{
			return null;
		}
		int num = -1;
		int num2 = -1;
		if (markerDeck != null)
		{
			num = Convert.ToInt32(markerDeck);
		}
		if (markerSlot != null)
		{
			num2 = Convert.ToInt32(markerSlot);
		}
		LogHelper.Instance.Log($"KernelFunction filemarkerdisk| deck:{num} slot:{num2}");
		ErrorList errors = service.FileMarkerDisk(eventHandler, statusChangeHandler, num, num2, out var jobId);
		return CreateResultTable(jobId, errors);
	}

	[KernelFunction(Name = "kernel.clearmerchstatus")]
	internal static LuaTable ClearMerchStatus(LuaTable barcodes, HardwareEventHandler eventHandler, HardwareStatusChangeHandler statusChangeHandler)
	{
		IHardwareService service = ServiceLocator.Instance.GetService<IHardwareService>();
		if (service == null)
		{
			return null;
		}
		List<string> list = new List<string>();
		foreach (LuaTable value in barcodes.Values)
		{
			string item = (string)value["barcode"];
			if (!list.Contains(item))
			{
				list.Add(item);
			}
		}
		ErrorList errors = service.ClearMerchStatus(list.ToArray(), eventHandler, statusChangeHandler, out var jobId);
		return CreateResultTable(jobId, errors);
	}

	[KernelFunction(Name = "kernel.getbarcodevmzposition")]
	internal static LuaTable GetBarcodeVMZPosition(LuaTable barcodes, HardwareEventHandler eventHandler, HardwareStatusChangeHandler statusChangeHandler)
	{
		IHardwareService service = ServiceLocator.Instance.GetService<IHardwareService>();
		if (service == null)
		{
			return null;
		}
		List<string> list = new List<string>();
		foreach (LuaTable value in barcodes.Values)
		{
			string item = (string)value["barcode"];
			if (!list.Contains(item))
			{
				list.Add(item);
			}
		}
		ErrorList barcodeVMZPosition = service.GetBarcodeVMZPosition(list.ToArray(), eventHandler, statusChangeHandler, out var jobId);
		return CreateResultTable(jobId, barcodeVMZPosition);
	}

	[KernelFunction(Name = "kernel.hardwarevend")]
	internal static LuaTable HardwareVend(HardwareEventHandler eventHandler, HardwareStatusChangeHandler statusChangeHandler, object prepositionedDeck, object prepositionedSlot, LuaTable barcodes)
	{
		IHardwareService service = ServiceLocator.Instance.GetService<IHardwareService>();
		if (service == null)
		{
			return null;
		}
		List<string> list = new List<string>();
		foreach (object key in barcodes.Keys)
		{
			object obj = barcodes[key];
			if (obj != null)
			{
				string item = obj.ToString();
				if (!list.Contains(item))
				{
					list.Add(item);
				}
			}
		}
		barcodes.Dispose();
		if (list.Count == 0)
		{
			return CreateResultTable(null, new ErrorList { Redbox.KioskEngine.ComponentModel.Error.NewError("H034", "No barcodes specified to vend.", "The vend command requires barcodes on the stack to vend.") });
		}
		list.Sort();
		int? prepositionedDeck2 = null;
		int? prepositionedSlot2 = null;
		if (prepositionedDeck != null)
		{
			prepositionedDeck2 = Convert.ToInt32(prepositionedDeck);
		}
		if (prepositionedSlot != null)
		{
			prepositionedSlot2 = Convert.ToInt32(prepositionedSlot);
		}
		ErrorList errors = service.Vend(eventHandler, statusChangeHandler, list.ToArray(), prepositionedDeck2, prepositionedSlot2, out var jobId);
		return CreateResultTable(jobId, errors);
	}

	[KernelFunction(Name = "kernel.hardwarepreposvend")]
	internal static LuaTable HardwarePreposVend(HardwareEventHandler eventHandler, HardwareStatusChangeHandler statusChangeHandler, LuaTable barcodes)
	{
		IHardwareService service = ServiceLocator.Instance.GetService<IHardwareService>();
		if (service == null)
		{
			return null;
		}
		List<string> list = new List<string>();
		foreach (object key in barcodes.Keys)
		{
			object obj = barcodes[key];
			if (obj != null)
			{
				string item = obj.ToString();
				if (!list.Contains(item))
				{
					list.Add(item);
				}
			}
		}
		barcodes.Dispose();
		if (list.Count == 0)
		{
			return CreateResultTable(null, new ErrorList { Redbox.KioskEngine.ComponentModel.Error.NewError("H034", "No barcodes specified to preposvend.", "The preposvend command requires barcodes on the stack to vend.") });
		}
		list.Sort();
		ErrorList errors = service.PreposVend(eventHandler, statusChangeHandler, list.ToArray(), out var jobId);
		return CreateResultTable(jobId, errors);
	}

	[KernelFunction(Name = "kernel.hardwarevendlocations")]
	internal static LuaTable HardwareVendLocations(HardwareEventHandler eventHandler, HardwareStatusChangeHandler statusChangeHandler, LuaTable locationTable)
	{
		IHardwareService service = ServiceLocator.Instance.GetService<IHardwareService>();
		if (service == null)
		{
			return null;
		}
		List<int> list = new List<int>();
		foreach (object value in locationTable.Values)
		{
			list.Add(Convert.ToInt32(((LuaTable)value)["deck"]));
			list.Add(Convert.ToInt32(((LuaTable)value)["slot"]));
		}
		locationTable.Dispose();
		if (list.Count == 0)
		{
			return CreateResultTable(null, new ErrorList { Redbox.KioskEngine.ComponentModel.Error.NewError("H034", "No locations specified to vendlocation.", "The vendlocation command requires locations on the stack to vend.") });
		}
		ErrorList errors = service.VendLocations(eventHandler, statusChangeHandler, list, out var jobId);
		return CreateResultTable(jobId, errors);
	}

	[KernelFunction(Name = "kernel.hardwareresetlocations")]
	internal static LuaTable HardwareResetLocations(HardwareEventHandler eventHandler, HardwareStatusChangeHandler statusChangeHandler, LuaTable locationTable)
	{
		IHardwareService service = ServiceLocator.Instance.GetService<IHardwareService>();
		if (service == null)
		{
			return null;
		}
		List<int> list = new List<int>();
		foreach (object value in locationTable.Values)
		{
			list.Add(Convert.ToInt32(((LuaTable)value)["deck"]));
			list.Add(Convert.ToInt32(((LuaTable)value)["slot"]));
		}
		locationTable.Dispose();
		if (list.Count == 0)
		{
			return CreateResultTable(null, new ErrorList { Redbox.KioskEngine.ComponentModel.Error.NewError("H034", "No locations specified to resetlocation.", "The resetlocation command requires locations on the stack to vend.") });
		}
		ErrorList errors = service.ResetLocations(eventHandler, statusChangeHandler, list, out var jobId);
		return CreateResultTable(jobId, errors);
	}

	[KernelFunction(Name = "kernel.hardwarevendunknown")]
	internal static LuaTable HardwareVendUnknown(HardwareEventHandler eventHandler, HardwareStatusChangeHandler statusChangeHandler, int deck, int slot)
	{
		IHardwareService service = ServiceLocator.Instance.GetService<IHardwareService>();
		if (service == null)
		{
			return null;
		}
		if (deck == 0 || slot == 0)
		{
			return CreateResultTable(null, new ErrorList { Redbox.KioskEngine.ComponentModel.Error.NewError("H034", "No locations specified to vendunknown.", "The vendunknown command requires locations on the stack to vend.") });
		}
		ErrorList errors = service.VendUnknown(eventHandler, statusChangeHandler, deck, slot, out var jobId);
		return CreateResultTable(jobId, errors);
	}

	[KernelFunction(Name = "kernel.hardwarereturn")]
	internal static LuaTable HardwareReturn(HardwareEventHandler eventHandler, HardwareStatusChangeHandler statusChangeHandler)
	{
		IHardwareService service = ServiceLocator.Instance.GetService<IHardwareService>();
		if (service == null)
		{
			return null;
		}
		ErrorList errors = service.Return(eventHandler, statusChangeHandler, out var jobId);
		return CreateResultTable(jobId, errors);
	}

	[KernelFunction(Name = "kernel.hardwarereturnunknown")]
	internal static LuaTable HardwareReturnUnknown(HardwareEventHandler eventHandler, HardwareStatusChangeHandler statusChangeHandler, string returnTime)
	{
		IHardwareService service = ServiceLocator.Instance.GetService<IHardwareService>();
		if (service == null)
		{
			return null;
		}
		ErrorList errors = service.ReturnUnknown(eventHandler, statusChangeHandler, returnTime, out var jobId);
		return CreateResultTable(jobId, errors);
	}

	[KernelFunction(Name = "kernel.hardwareisbusy")]
	internal static LuaTable HardwareIsBusy()
	{
		IHardwareService service = ServiceLocator.Instance.GetService<IHardwareService>();
		if (service == null)
		{
			return null;
		}
		ErrorList schedulerStatus = service.GetSchedulerStatus(out var status);
		return CreateResultTable(status, schedulerStatus);
	}

	[KernelFunction(Name = "kernel.hardwarenotifyidle")]
	internal static void HardwareNotifyIdle(string jobid, HardwareNotifyIdleHandler idleHandler)
	{
		ITimerService timerService = ServiceLocator.Instance.GetService<ITimerService>();
		if (timerService == null)
		{
			return;
		}
		timerService.CreateTimer(jobid, 1500, 250, delegate
		{
			IHardwareService service = ServiceLocator.Instance.GetService<IHardwareService>();
			if (service != null && (service.GetSchedulerStatus(out var status).ContainsError() || status == jobid || status == "Idle"))
			{
				timerService.RemoveTimer(jobid);
				idleHandler(status);
			}
		}).Start();
	}

	[KernelFunction(Name = "kernel.hardwarenotifymaintmodestate")]
	internal static LuaTable HardwareNotifyMaintModeState(bool goingIntoMM)
	{
		IHardwareService service = ServiceLocator.Instance.GetService<IHardwareService>();
		if (service == null)
		{
			return null;
		}
		ErrorList errors = service.NotifyMaintModeState(goingIntoMM);
		return CreateResultTable(string.Empty, errors);
	}

	[KernelFunction(Name = "kernel.hardwaresoftsync")]
	internal static LuaTable HardwareSoftSync(HardwareEventHandler eventHandler, HardwareStatusChangeHandler statusChangeHandler)
	{
		IHardwareService service = ServiceLocator.Instance.GetService<IHardwareService>();
		if (service == null)
		{
			return null;
		}
		ErrorList errors = service.SoftSync(eventHandler, statusChangeHandler, out var jobId);
		return CreateResultTable(jobId, errors);
	}

	[KernelFunction(Name = "kernel.hardwaregetmachineinfo")]
	internal static LuaTable HardwareGetMachineInfo()
	{
		IHardwareState service = ServiceLocator.Instance.GetService<IHardwareState>();
		IMachineSettingsStore service2 = ServiceLocator.Instance.GetService<IMachineSettingsStore>();
		if (!string.IsNullOrEmpty(service.QlmDeckAsString))
		{
			service2.SetValue("Store", "QlmDeck", service.QlmDeckAsString);
		}
		if (service.MaxDeck > 0)
		{
			service2.SetValue("Store", "MaxDecks", service.MaxDeck.ToString());
		}
		if (service.MaxSlot > 0)
		{
			service2.SetValue("Store", "MaxSlots", service.MaxSlot.ToString());
		}
		LuaTable obj = ServiceLocator.Instance.GetService<IKernelService>().NewTable() as LuaTable;
		obj["QlmDeck"] = service.QlmDeckAsString;
		obj["MaxDecks"] = service.MaxDeck;
		obj["MaxSlots"] = service.MaxSlot;
		obj["MerchType"] = service.MerchandizeType;
		obj["BufferSlot"] = service.BufferSlot;
		return obj;
	}

	[KernelFunction(Name = "kernel.hardwaregethardwarestatus")]
	internal static LuaTable HardwareGetHardwareStatus(LuaTable startTime, HardwareEventHandler eventHandler, HardwareStatusChangeHandler statusChangeHandler)
	{
		IHardwareService service = ServiceLocator.Instance.GetService<IHardwareService>();
		if (service == null)
		{
			return null;
		}
		ErrorList hardwareStatus = service.GetHardwareStatus(startTime.ToDateTime(), eventHandler, statusChangeHandler, out var jobId);
		startTime.Dispose();
		return CreateResultTable(jobId, hardwareStatus);
	}

	[KernelFunction(Name = "kernel.hardwaresync")]
	internal static LuaTable HardwareSync(HardwareEventHandler eventHandler, HardwareStatusChangeHandler statusChangeHandler, string label, LuaTable startTime, int startDeck, int startSlot, int endDeck, int endSlot)
	{
		IHardwareService service = ServiceLocator.Instance.GetService<IHardwareService>();
		if (service == null)
		{
			return null;
		}
		ErrorList errors = service.Sync(eventHandler, statusChangeHandler, label, startTime.ToDateTime(), startDeck, startSlot, endDeck, endSlot, out var jobId);
		startTime.Dispose();
		return CreateResultTable(jobId, errors);
	}

	[KernelFunction(Name = "kernel.hardwaresynclocations")]
	internal static LuaTable HardwareSyncLocations(HardwareEventHandler eventHandler, HardwareStatusChangeHandler statusChangeHandler, string label, LuaTable startTime, LuaTable locationTable)
	{
		IHardwareService service = ServiceLocator.Instance.GetService<IHardwareService>();
		if (service == null)
		{
			return null;
		}
		List<int> list = new List<int>();
		foreach (object value in locationTable.Values)
		{
			list.Add(Convert.ToInt32(((LuaTable)value)["slot"]));
			list.Add(Convert.ToInt32(((LuaTable)value)["deck"]));
		}
		ErrorList errors = service.SyncLocations(eventHandler, statusChangeHandler, label, startTime.ToDateTime(), list, out var jobId);
		startTime.Dispose();
		locationTable.Dispose();
		return CreateResultTable(jobId, errors);
	}

	[KernelFunction(Name = "kernel.hardwareqlmunload")]
	internal static LuaTable QlmUnload(HardwareEventHandler eventHandler, HardwareStatusChangeHandler statusChangeHandler, string label, LuaTable startTime)
	{
		IHardwareService service = ServiceLocator.Instance.GetService<IHardwareService>();
		if (service == null)
		{
			return null;
		}
		ErrorList errors = service.QlmUnload(label, startTime.ToDateTime(), eventHandler, statusChangeHandler, out var jobId);
		startTime.Dispose();
		return CreateResultTable(jobId, errors);
	}

	[KernelFunction(Name = "kernel.hardwareqlmthin")]
	internal static LuaTable QlmThin(HardwareEventHandler eventHandler, HardwareStatusChangeHandler statusChangeHandler, string label, LuaTable startTime, LuaTable inventory)
	{
		IHardwareService service = ServiceLocator.Instance.GetService<IHardwareService>();
		if (service == null)
		{
			return null;
		}
		List<string> list = new List<string>();
		foreach (LuaTable value in inventory.Values)
		{
			string item = (string)value["barcode"];
			if (!list.Contains(item))
			{
				list.Add(item);
			}
		}
		ErrorList errors = service.QlmThin(label, startTime.ToDateTime(), list.ToArray(), eventHandler, statusChangeHandler, out var jobId);
		startTime.Dispose();
		inventory.Dispose();
		return CreateResultTable(jobId, errors);
	}

	[KernelFunction(Name = "kernel.hardwarevmzthin")]
	internal static LuaTable VmzThin(HardwareEventHandler eventHandler, HardwareStatusChangeHandler statusChangeHandler, string label, LuaTable startTime, LuaTable barcodesToBin, LuaTable rebalances, LuaTable redeployments, LuaTable barcodesToVmz)
	{
		IHardwareService service = ServiceLocator.Instance.GetService<IHardwareService>();
		if (service == null)
		{
			return null;
		}
		string[] barcodesToThin = ((barcodesToBin != null) ? (from LuaTable r in barcodesToBin.Values
			where r != null
			select (string)r["barcode"]).Distinct().ToArray() : Array.Empty<string>());
		string[] rebalances2 = ((rebalances != null) ? (from LuaTable r in rebalances.Values
			where r != null
			select (string)r["barcode"]).Distinct().ToArray() : Array.Empty<string>());
		string[] redeployments2 = ((redeployments != null) ? (from LuaTable r in redeployments.Values
			where r != null
			select (string)r["barcode"]).Distinct().ToArray() : Array.Empty<string>());
		string[] barcodesToVmz2 = ((barcodesToVmz != null) ? (from LuaTable r in barcodesToVmz.Values
			where r != null
			select (string)r["barcode"]).Distinct().ToArray() : Array.Empty<string>());
		ErrorList errors = service.VmzThin(label, startTime.ToDateTime(), barcodesToThin, rebalances2, redeployments2, barcodesToVmz2, eventHandler, statusChangeHandler, out var jobId);
		startTime.Dispose();
		barcodesToBin.Dispose();
		rebalances.Dispose();
		redeployments.Dispose();
		barcodesToVmz.Dispose();
		return CreateResultTable(jobId, errors);
	}

	[KernelFunction(Name = "kernel.hardwareqlmunloadandthin")]
	internal static LuaTable QlmUnloadAndThin(HardwareEventHandler eventHandler, HardwareStatusChangeHandler statusChangeHandler, string label, LuaTable startTime, LuaTable inventory)
	{
		IHardwareService service = ServiceLocator.Instance.GetService<IHardwareService>();
		if (service == null)
		{
			return null;
		}
		List<string> list = new List<string>();
		foreach (LuaTable value in inventory.Values)
		{
			string item = (string)value["barcode"];
			if (!list.Contains(item))
			{
				list.Add(item);
			}
		}
		ErrorList errors = service.QlmUnloadAndThin(label, startTime.ToDateTime(), list.ToArray(), eventHandler, statusChangeHandler, out var jobId);
		startTime.Dispose();
		inventory.Dispose();
		return CreateResultTable(jobId, errors);
	}

	[KernelFunction(Name = "kernel.prepforredeploy")]
	internal static LuaTable PrepForRedeploy(LuaTable decksToChange, HardwareEventHandler eventHandler, HardwareStatusChangeHandler statusChangeHandler)
	{
		IHardwareService service = ServiceLocator.Instance.GetService<IHardwareService>();
		if (service == null)
		{
			return null;
		}
		List<string> list = new List<string>();
		foreach (LuaTable value in decksToChange.Values)
		{
			string text = (string)value["deck"];
			if (!string.IsNullOrEmpty(text) && !list.Contains(text))
			{
				list.Add(text);
			}
		}
		ErrorList errors = service.PrepForRedeploy(list.ToArray(), eventHandler, statusChangeHandler, out var jobId);
		decksToChange.Dispose();
		return CreateResultTable(jobId, errors);
	}

	[KernelFunction(Name = "kernel.hardwareschedulejob")]
	internal static LuaTable ScheduleJob(string scriptName, string label, bool enableDebugging, string priority, string startTimeString, object stack, HardwareEventHandler eventHandler, HardwareStatusChangeHandler statusChangeHandler, bool allowMultiple)
	{
		IHardwareService service = ServiceLocator.Instance.GetService<IHardwareService>();
		if (service == null)
		{
			return null;
		}
		List<object> list = new List<object>();
		if (stack is LuaTable luaTable)
		{
			for (int i = 1; i <= luaTable.Keys.Count; i++)
			{
				object obj = luaTable[i];
				if (obj is double)
				{
					obj = Convert.ToInt32(obj);
				}
				list.Add(obj);
			}
		}
		ErrorList errors = service.ScheduleJob(scriptName, label, enableDebugging, priority, startTimeString, list.AsReadOnly(), eventHandler, statusChangeHandler, allowMultiple, out var jobId);
		return CreateResultTable(jobId, errors);
	}

	[KernelFunction(Name = "kernel.hardwaresuspendjob")]
	internal static LuaTable HardwareSuspendJob(string jobId)
	{
		IHardwareService service = ServiceLocator.Instance.GetService<IHardwareService>();
		if (service == null)
		{
			return null;
		}
		return CreateResultTable(jobId, service.Suspend(jobId));
	}

	[KernelFunction(Name = "kernel.hardwareresumejob")]
	internal static LuaTable HardwareResumeJob(string jobId)
	{
		IHardwareService service = ServiceLocator.Instance.GetService<IHardwareService>();
		if (service == null)
		{
			return null;
		}
		return CreateResultTable(jobId, service.Resume(jobId));
	}

	[KernelFunction(Name = "kernel.hardwaretrashjob")]
	internal static LuaTable HardwareTrashJob(string jobId)
	{
		IHardwareService service = ServiceLocator.Instance.GetService<IHardwareService>();
		if (service == null)
		{
			return null;
		}
		return CreateResultTable(jobId, service.Trash(jobId));
	}

	[KernelFunction(Name = "kernel.hardwareterminatejob")]
	internal static LuaTable HardwareTerminateJob(string jobId)
	{
		IHardwareService service = ServiceLocator.Instance.GetService<IHardwareService>();
		if (service == null)
		{
			return null;
		}
		return CreateResultTable(jobId, service.Terminate(jobId));
	}

	[KernelFunction(Name = "kernel.hardwareresumealljobs")]
	internal static LuaTable HardwareResumeAllJobs()
	{
		IHardwareService service = ServiceLocator.Instance.GetService<IHardwareService>();
		if (service == null)
		{
			return null;
		}
		return CreateResultTable(string.Empty, service.ResumeAll());
	}

	[KernelFunction(Name = "kernel.hardwaresuspendalljobs")]
	internal static LuaTable HardwareSuspendAllJobs()
	{
		IHardwareService service = ServiceLocator.Instance.GetService<IHardwareService>();
		if (service == null)
		{
			return null;
		}
		return CreateResultTable(string.Empty, service.SuspendAll());
	}

	[KernelFunction(Name = "kernel.hardwaremaintenancemodesuspendjobs")]
	internal static LuaTable HardwareMaintenanceModeSuspendJobs()
	{
		IHardwareService service = ServiceLocator.Instance.GetService<IHardwareService>();
		if (service == null)
		{
			return null;
		}
		return CreateResultTable(string.Empty, service.SuspendAllJobsForMaintenance());
	}

	[KernelFunction(Name = "kernel.hardwaremaintenancemoderesumejobs")]
	internal static LuaTable HardwareMaintenanceModeResumeJobs()
	{
		IHardwareService service = ServiceLocator.Instance.GetService<IHardwareService>();
		if (service == null)
		{
			return null;
		}
		return CreateResultTable(string.Empty, service.ResumeAllMaintenanceSuspendedJobs());
	}

	[KernelFunction(Name = "kernel.hardwareexecuteimmediate")]
	internal static LuaTable HardwareExecuteImmediate(string command, HardwareImmediateExecutionHandler eventHandler, bool clearStack)
	{
		IHardwareService service = ServiceLocator.Instance.GetService<IHardwareService>();
		if (service == null)
		{
			return null;
		}
		IResourceBundleService service2 = ServiceLocator.Instance.GetService<IResourceBundleService>();
		return CreateResultTable(string.Empty, service.ExecuteImmediate(command, eventHandler, clearStack, service2?.CurrentBundleName));
	}

	[KernelFunction(Name = "kernel.hardwaregetprogramresults")]
	internal static LuaTable HardwareGetProgramResults(string jobId)
	{
		IHardwareService service = ServiceLocator.Instance.GetService<IHardwareService>();
		if (service == null)
		{
			return null;
		}
		ErrorList programResults = service.GetProgramResults(jobId, out var hardwareResults);
		LuaTable luaTable = CreateResultTable(jobId, programResults);
		IKernelService service2 = ServiceLocator.Instance.GetService<IKernelService>();
		LuaTable luaTable2 = (LuaTable)(luaTable["results"] = (LuaTable)service2.NewTable());
		int num = 1;
		foreach (IHardwareResult item in hardwareResults)
		{
			LuaTable luaTable3 = (LuaTable)service2.NewTable();
			luaTable3["id"] = item.ID;
			luaTable3["code"] = item.Code;
			luaTable3["deck"] = item.Deck;
			luaTable3["slot"] = item.Slot;
			luaTable3["item_id"] = item.ItemID;
			luaTable3["message"] = item.Message;
			luaTable3["meta_data"] = item.MetaData;
			DateTime dateTime = DateTime.Parse(item.Timestamp);
			luaTable3["time_stamp"] = dateTime.ToString("yyyyMMddhhmm");
			luaTable3["time_stamp24"] = dateTime.ToString("yyyyMMddHHmm");
			luaTable3["previous_item"] = item.PreviousItem;
			luaTable3["return_time"] = item.ReturnTime;
			luaTable2[num++] = luaTable3;
		}
		return luaTable;
	}

	[KernelFunction(Name = "kernel.hardwaregetjoberrors")]
	internal static LuaTable HardwareGetJobErrors(string jobId)
	{
		IHardwareService service = ServiceLocator.Instance.GetService<IHardwareService>();
		if (service == null)
		{
			return null;
		}
		LuaTable luaTable = CreateResultTable(jobId, service.GetJobErrors(jobId, out var errors));
		IKernelService service2 = ServiceLocator.Instance.GetService<IKernelService>();
		LuaTable luaTable2 = (LuaTable)(luaTable["results"] = (LuaTable)service2.NewTable());
		int num = 1;
		if (errors != null)
		{
			foreach (Redbox.KioskEngine.ComponentModel.Error item in errors)
			{
				LuaTable luaTable3 = (LuaTable)service2.NewTable();
				luaTable3["code"] = item.Code;
				luaTable3["details"] = item.Details;
				luaTable3["is_warning"] = item.IsWarning;
				luaTable3["description"] = item.Description;
				luaTable2[num++] = luaTable3;
			}
		}
		return luaTable;
	}

	[KernelFunction(Name = "kernel.is_active_hardware_job")]
	internal static bool IsActiveHardwareJob(string jobId)
	{
		IHardwareService service = ServiceLocator.Instance.GetService<IHardwareService>();
		ReadOnlyCollection<IHardwareJob> hardwareJobs = null;
		service?.GetHardwareJobs(out hardwareJobs);
		bool flag = hardwareJobs?.Any((IHardwareJob x) => x.ID == jobId && (x.Status == "Pending" || x.Status == "Running" || x.Status == "Suspended")) ?? false;
		LogHelper.Instance.Log(string.Format("IsActiveHardwareJob {0}: {1};  Job count: {2}, jobs: {3} ", jobId, flag, hardwareJobs?.Count, string.Join(", ", hardwareJobs.Select((IHardwareJob x) => x.ID + ":" + x.Status).ToArray())));
		return flag;
	}

	[KernelFunction(Name = "kernel.hardwaregetjobs")]
	internal static LuaTable HardwareGetJobs()
	{
		IHardwareService service = ServiceLocator.Instance.GetService<IHardwareService>();
		if (service == null)
		{
			return null;
		}
		LuaTable luaTable = CreateResultTable(string.Empty, service.GetHardwareJobs(out var hardwareJobs));
		IKernelService service2 = ServiceLocator.Instance.GetService<IKernelService>();
		LuaTable luaTable2 = (LuaTable)(luaTable["results"] = (LuaTable)service2.NewTable());
		int num = 1;
		foreach (IHardwareJob item in hardwareJobs)
		{
			LuaTable luaTable3 = (LuaTable)service2.NewTable();
			luaTable3["id"] = item.ID;
			luaTable3["status"] = item.Status;
			luaTable3["label"] = item.Label;
			luaTable3["program"] = item.ProgramName;
			luaTable3["starttime"] = item.StartTime;
			luaTable3["executiontime"] = item.ExecutionTime;
			luaTable3["priority"] = item.Priority;
			luaTable2[num++] = luaTable3;
		}
		return luaTable;
	}

	[KernelFunction(Name = "kernel.hardwaregetsuspendedmaintenancemodejobs")]
	internal static LuaTable HardwareGetSuspendedMaintenanceModeJobs()
	{
		IHardwareService service = ServiceLocator.Instance.GetService<IHardwareService>();
		if (service == null)
		{
			return null;
		}
		ErrorList suspendedMaintenanceModeJobs = service.GetSuspendedMaintenanceModeJobs(out var suspendedJobs);
		LuaTable luaTable = CreateResultTable(string.Empty, suspendedMaintenanceModeJobs);
		IKernelService service2 = ServiceLocator.Instance.GetService<IKernelService>();
		LuaTable luaTable2 = (LuaTable)(luaTable["results"] = (LuaTable)service2.NewTable());
		int num = 1;
		foreach (IHardwareJob item in suspendedJobs)
		{
			LuaTable luaTable3 = (LuaTable)service2.NewTable();
			luaTable3["id"] = item.ID;
			luaTable3["status"] = item.Status;
			luaTable3["label"] = item.Label;
			luaTable3["program"] = item.ProgramName;
			luaTable3["starttime"] = item.StartTime;
			luaTable3["executiontime"] = item.ExecutionTime;
			luaTable3["priority"] = item.Priority;
			luaTable2[num++] = luaTable3;
		}
		return luaTable;
	}

	[KernelFunction(Name = "kernel.hardwareexecuteservicecommand")]
	internal static LuaTable HardwareExecuteServiceCommand(string command)
	{
		IHardwareService service = ServiceLocator.Instance.GetService<IHardwareService>();
		if (service == null)
		{
			return null;
		}
		return CreateResultTable(string.Empty, service.ExecuteServiceCommand(command));
	}

	[KernelFunction(Name = "kernel.hardwaregetinitstatus")]
	internal static LuaTable HardwareGetInitStatus()
	{
		IHardwareService service = ServiceLocator.Instance.GetService<IHardwareService>();
		if (service == null)
		{
			return null;
		}
		ErrorList initStatus = service.GetInitStatus(out var status);
		LuaTable luaTable = CreateResultTable(string.Empty, initStatus);
		luaTable["status"] = status;
		return luaTable;
	}

	[KernelFunction(Name = "kernel.hardwarereadfrauddisc")]
	internal static LuaTable HardwareReadFraudDisc(HardwareEventHandler eventHandler, HardwareStatusChangeHandler statusChangeHandler)
	{
		IHardwareService service = ServiceLocator.Instance.GetService<IHardwareService>();
		if (service == null)
		{
			return null;
		}
		ErrorList errors = service.ReadFraudDisc(eventHandler, statusChangeHandler, out var jobId);
		return CreateResultTable(jobId, errors);
	}

	[KernelFunction(Name = "kernel.hardwareprepunloadzone")]
	internal static LuaTable HardwarePrepUnloadZone(int rotation, int last_deck, HardwareEventHandler eventHandler, HardwareStatusChangeHandler statusChangeHandler)
	{
		IHardwareService service = ServiceLocator.Instance.GetService<IHardwareService>();
		if (service == null)
		{
			return null;
		}
		LogHelper.Instance.Log($"kernel.hardwareprepunloadzone| rotation:{rotation} last_deck:{last_deck}");
		ErrorList errors = service.PrepUnloadZone(rotation, last_deck, eventHandler, statusChangeHandler, out var jobId);
		return CreateResultTable(jobId, errors);
	}

	[KernelFunction(Name = "kernel.hasabedevice")]
	internal static string HasAbeDevice()
	{
		IHardwareService service = ServiceLocator.Instance.GetService<IHardwareService>();
		if (service == null)
		{
			return null;
		}
		service.HasAbeDevice(out var hasAbeDevice);
		return hasAbeDevice.ToString();
	}

	[KernelFunction(Name = "kernel.irhardwareinstalled")]
	internal static string IRHardwareInstalled()
	{
		IHardwareService service = ServiceLocator.Instance.GetService<IHardwareService>();
		if (service == null)
		{
			return null;
		}
		service.IRHardwareInstalled(out var installDate);
		string result = null;
		if (installDate.HasValue)
		{
			result = installDate.Value.ToString("o");
		}
		return result;
	}

	[KernelFunction(Name = "kernel.enqueue_hardware_properties")]
	internal static void EnqueueHardwareProperties()
	{
		IHardwareState service = ServiceLocator.Instance.GetService<IHardwareState>();
		IHealthServices service2 = ServiceLocator.Instance.GetService<IHealthServices>();
		if (service == null)
		{
			LogHelper.Instance.Log("Hardware State was not found.");
		}
		if (service2 == null)
		{
			LogHelper.Instance.Log("Health Service was not found.");
		}
		LogHelper.Instance.Log("Enqueue Hardware State properties.", LogEntryType.Info);
		LogHelper.Instance.Log("Camera: " + service.CurrentCameraGeneration + ", Decoder: " + service.BarcodeDecoder);
		if (service.CurrentCameraGeneration >= 3)
		{
			service2.EnqueueProperty("CameraType", service.CurrentCameraGeneration.ToString());
		}
	}

	[KernelFunction(Name = "kernel.sync_unknowns")]
	internal static void SyncUnknowns(bool forceSync = false)
	{
		LogHelper.Instance.Log("HardwareFunctions.SyncUnknowns called with forceSync = " + forceSync);
		ServiceLocator.Instance.GetService<ISyncUnknownProcessor>().ScheduleJob(forceSync);
	}

	[KernelFunction(Name = "kernel.sync_unknowns_complete")]
	internal static void SyncUnknownsComplete(string jobId)
	{
		LogHelper.Instance.Log("HardwareFunctions.SyncUnknownsComplete called for jobId = " + jobId);
		ServiceLocator.Instance.GetService<ISyncUnknownProcessor>().ProcessSyncLocationsComplete(jobId);
	}

	[KernelFunction(Name = "kernel.sync_empties")]
	internal static void SyncEmpties()
	{
		LogHelper.Instance.Log("HardwareFunctions.SyncEmpties Called.");
		ServiceLocator.Instance.GetService<ISyncEmptiesProcessor>().ScheduleJob();
	}

	private static LuaTable CreateResultTable(string jobId, ErrorList errors)
	{
		IKernelService service = ServiceLocator.Instance.GetService<IKernelService>();
		LuaTable luaTable = (LuaTable)service.NewTable();
		luaTable["success"] = !errors.ContainsError();
		luaTable["job_id"] = jobId;
		LuaTable luaTable2 = (LuaTable)service.NewTable();
		foreach (Redbox.KioskEngine.ComponentModel.Error error in errors)
		{
			LogHelper.Instance.Log("HardwareFunctions.CreateResultTable contains error " + error.ToString());
			luaTable2[error.Code] = error.Description;
		}
		luaTable["errors"] = luaTable2;
		return luaTable;
	}
}
