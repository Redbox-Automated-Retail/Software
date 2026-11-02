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
	internal static LuaTable? HardwareInit(HardwareEventHandler eventHandler, HardwareStatusChangeHandler statusChangeHandler)
	{
		IHardwareService hardwareService = ServiceLocator.Instance.GetService<IHardwareService>();
		if (hardwareService == null)
		{
			return null;
		}
		ErrorList errors = hardwareService.Init(out var jobId, eventHandler, statusChangeHandler);
		return CreateResultTable(jobId, errors);
	}

	[KernelFunction(Name = "kernel.hardwarecheckpicker")]
	internal static LuaTable? HardwareCheckPicker(HardwareEventHandler eventHandler, HardwareStatusChangeHandler statusChangeHandler)
	{
		IHardwareService hardwareService = ServiceLocator.Instance.GetService<IHardwareService>();
		if (hardwareService == null)
		{
			return null;
		}
		ErrorList errors = hardwareService.CheckPicker(eventHandler, statusChangeHandler, out var jobId);
		return CreateResultTable(jobId, errors);
	}

	[KernelFunction(Name = "kernel.getnonthinsinvmz")]
	internal static LuaTable? GetNonThinsInVMZ(HardwareEventHandler eventHandler, HardwareStatusChangeHandler statusChangeHandler)
	{
		IHardwareService hardwareService = ServiceLocator.Instance.GetService<IHardwareService>();
		if (hardwareService == null)
		{
			return null;
		}
		ErrorList nonThinsInVMZ = hardwareService.GetNonThinsInVMZ(eventHandler, statusChangeHandler, out var jobId);
		return CreateResultTable(jobId, nonThinsInVMZ);
	}

	[KernelFunction(Name = "kernel.vmzremovedthins")]
	internal static LuaTable? VMZRemovedThins(HardwareEventHandler eventHandler, HardwareStatusChangeHandler statusChangeHandler)
	{
		IHardwareService hardwareService = ServiceLocator.Instance.GetService<IHardwareService>();
		if (hardwareService == null)
		{
			return null;
		}
		ErrorList errors = hardwareService.VMZRemovedThins(eventHandler, statusChangeHandler, out var jobId);
		return CreateResultTable(jobId, errors);
	}

	[KernelFunction(Name = "kernel.getbarcodesinbin")]
	internal static LuaTable? GetBarcodesInBin(HardwareEventHandler eventHandler, HardwareStatusChangeHandler statusChangeHandler)
	{
		IHardwareService hardwareService = ServiceLocator.Instance.GetService<IHardwareService>();
		if (hardwareService == null)
		{
			return null;
		}
		ErrorList barcodesInBin = hardwareService.GetBarcodesInBin(eventHandler, statusChangeHandler, out var jobId);
		return CreateResultTable(jobId, barcodesInBin);
	}

	[KernelFunction(Name = "kernel.getinventorystats")]
	internal static LuaTable? GetInventoryStats(HardwareEventHandler eventHandler, HardwareStatusChangeHandler statusChangeHandler)
	{
		IHardwareService hardwareService = ServiceLocator.Instance.GetService<IHardwareService>();
		if (hardwareService == null)
		{
			return null;
		}
		ErrorList inventoryStats = hardwareService.GetInventoryStats(eventHandler, statusChangeHandler, out var jobId);
		return CreateResultTable(jobId, inventoryStats);
	}

	[KernelFunction(Name = "kernel.vmzmerchsummary")]
	internal static LuaTable? VMZMerchSummary(HardwareEventHandler eventHandler, HardwareStatusChangeHandler statusChangeHandler)
	{
		IHardwareService hardwareService = ServiceLocator.Instance.GetService<IHardwareService>();
		if (hardwareService == null)
		{
			return null;
		}
		ErrorList errors = hardwareService.VMZMerchSummary(eventHandler, statusChangeHandler, out var jobId);
		return CreateResultTable(jobId, errors);
	}

	[KernelFunction(Name = "kernel.filemarkerdisk")]
	internal static LuaTable? FileMarkerDisk(HardwareEventHandler eventHandler, HardwareStatusChangeHandler statusChangeHandler, object markerDeck, object markerSlot)
	{
		LogHelper.Instance.Log($"KernelFunction filemarkerdisk| markerDeck:{markerDeck} markerSlot:{markerSlot}");
		IHardwareService hardwareService = ServiceLocator.Instance.GetService<IHardwareService>();
		if (hardwareService == null)
		{
			return null;
		}
		int deckNumber = -1;
		int slotNumber = -1;
		if (markerDeck != null)
		{
			deckNumber = Convert.ToInt32(markerDeck);
		}
		if (markerSlot != null)
		{
			slotNumber = Convert.ToInt32(markerSlot);
		}
		LogHelper.Instance.Log($"KernelFunction filemarkerdisk| deck:{deckNumber} slot:{slotNumber}");
		ErrorList errors = hardwareService.FileMarkerDisk(eventHandler, statusChangeHandler, deckNumber, slotNumber, out var jobId);
		return CreateResultTable(jobId, errors);
	}

	[KernelFunction(Name = "kernel.clearmerchstatus")]
	internal static LuaTable? ClearMerchStatus(LuaTable barcodes, HardwareEventHandler eventHandler, HardwareStatusChangeHandler statusChangeHandler)
	{
		IHardwareService hardwareService = ServiceLocator.Instance.GetService<IHardwareService>();
		if (hardwareService == null)
		{
			return null;
		}
		List<string?> barcodeList = new List<string?>();
		foreach (LuaTable value in barcodes.Values)
		{
			string? barcodeValue = (string?)value["barcode"];
			if (!barcodeList.Contains(barcodeValue))
			{
				barcodeList.Add(barcodeValue);
			}
		}
		ErrorList errors = hardwareService.ClearMerchStatus(barcodeList.ToArray(), eventHandler, statusChangeHandler, out var jobId);
		return CreateResultTable(jobId, errors);
	}

	[KernelFunction(Name = "kernel.getbarcodevmzposition")]
	internal static LuaTable? GetBarcodeVMZPosition(LuaTable barcodes, HardwareEventHandler eventHandler, HardwareStatusChangeHandler statusChangeHandler)
	{
		IHardwareService hardwareService = ServiceLocator.Instance.GetService<IHardwareService>();
		if (hardwareService == null)
		{
			return null;
		}
		List<string?> barcodeList = new List<string?>();
		foreach (LuaTable value in barcodes.Values)
		{
			string? barcodeValue = (string?)value["barcode"];
			if (!barcodeList.Contains(barcodeValue))
			{
				barcodeList.Add(barcodeValue);
			}
		}
		ErrorList barcodeVMZPosition = hardwareService.GetBarcodeVMZPosition(barcodeList.ToArray(), eventHandler, statusChangeHandler, out var jobId);
		return CreateResultTable(jobId, barcodeVMZPosition);
	}

	[KernelFunction(Name = "kernel.hardwarevend")]
	internal static LuaTable? HardwareVend(HardwareEventHandler eventHandler, HardwareStatusChangeHandler statusChangeHandler, object prepositionedDeck, object prepositionedSlot, LuaTable barcodes)
	{
		IHardwareService hardwareService = ServiceLocator.Instance.GetService<IHardwareService>();
		if (hardwareService == null)
		{
			return null;
		}
		List<string?> barcodeList = new List<string?>();
		foreach (object key in barcodes.Keys)
		{
			object? value = barcodes[key];
			if (value != null)
			{
				string? barcode = value.ToString();
				if (!barcodeList.Contains(barcode))
				{
					barcodeList.Add(barcode);
				}
			}
		}
		barcodes.Dispose();
		if (barcodeList.Count == 0)
		{
			return CreateResultTable(null, new ErrorList { Redbox.KioskEngine.ComponentModel.Error.NewError("H034", "No barcodes specified to vend.", "The vend command requires barcodes on the stack to vend.") });
		}
		barcodeList.Sort();
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
		ErrorList errors = hardwareService.Vend(eventHandler, statusChangeHandler, barcodeList.ToArray(), prepositionedDeck2, prepositionedSlot2, out var jobId);
		return CreateResultTable(jobId, errors);
	}

	[KernelFunction(Name = "kernel.hardwarepreposvend")]
	internal static LuaTable? HardwarePreposVend(HardwareEventHandler eventHandler, HardwareStatusChangeHandler statusChangeHandler, LuaTable barcodes)
	{
		IHardwareService hardwareService = ServiceLocator.Instance.GetService<IHardwareService>();
		if (hardwareService == null)
		{
			return null;
		}
		List<string?> barcodeList = new List<string?>();
		foreach (object key in barcodes.Keys)
		{
			object? value = barcodes[key];
			if (value != null)
			{
				string? barcode = value.ToString();
				if (!barcodeList.Contains(barcode))
				{
					barcodeList.Add(barcode);
				}
			}
		}
		barcodes.Dispose();
		if (barcodeList.Count == 0)
		{
			return CreateResultTable(null, new ErrorList { Redbox.KioskEngine.ComponentModel.Error.NewError("H034", "No barcodes specified to preposvend.", "The preposvend command requires barcodes on the stack to vend.") });
		}
		barcodeList.Sort();
		ErrorList errors = hardwareService.PreposVend(eventHandler, statusChangeHandler, barcodeList.ToArray(), out var jobId);
		return CreateResultTable(jobId, errors);
	}

	[KernelFunction(Name = "kernel.hardwarevendlocations")]
	internal static LuaTable? HardwareVendLocations(HardwareEventHandler eventHandler, HardwareStatusChangeHandler statusChangeHandler, LuaTable locationTable)
	{
		IHardwareService hardwareService = ServiceLocator.Instance.GetService<IHardwareService>();
		if (hardwareService == null)
		{
			return null;
		}
		List<int> barcodeList = new List<int>();
		foreach (object value in locationTable.Values)
		{
			barcodeList.Add(Convert.ToInt32(((LuaTable)value)["deck"]));
			barcodeList.Add(Convert.ToInt32(((LuaTable)value)["slot"]));
		}
		locationTable.Dispose();
		if (barcodeList.Count == 0)
		{
			return CreateResultTable(null, new ErrorList { Redbox.KioskEngine.ComponentModel.Error.NewError("H034", "No locations specified to vendlocation.", "The vendlocation command requires locations on the stack to vend.") });
		}
		ErrorList errors = hardwareService.VendLocations(eventHandler, statusChangeHandler, barcodeList, out var jobId);
		return CreateResultTable(jobId, errors);
	}

	[KernelFunction(Name = "kernel.hardwareresetlocations")]
	internal static LuaTable? HardwareResetLocations(HardwareEventHandler eventHandler, HardwareStatusChangeHandler statusChangeHandler, LuaTable locationTable)
	{
		IHardwareService hardwareService = ServiceLocator.Instance.GetService<IHardwareService>();
		if (hardwareService == null)
		{
			return null;
		}
		List<int> barcodeList = new List<int>();
		foreach (object value in locationTable.Values)
		{
			barcodeList.Add(Convert.ToInt32(((LuaTable)value)["deck"]));
			barcodeList.Add(Convert.ToInt32(((LuaTable)value)["slot"]));
		}
		locationTable.Dispose();
		if (barcodeList.Count == 0)
		{
			return CreateResultTable(null, new ErrorList { Redbox.KioskEngine.ComponentModel.Error.NewError("H034", "No locations specified to resetlocation.", "The resetlocation command requires locations on the stack to vend.") });
		}
		ErrorList errors = hardwareService.ResetLocations(eventHandler, statusChangeHandler, barcodeList, out var jobId);
		return CreateResultTable(jobId, errors);
	}

	[KernelFunction(Name = "kernel.hardwarevendunknown")]
	internal static LuaTable? HardwareVendUnknown(HardwareEventHandler eventHandler, HardwareStatusChangeHandler statusChangeHandler, int deck, int slot)
	{
		IHardwareService hardwareService = ServiceLocator.Instance.GetService<IHardwareService>();
		if (hardwareService == null)
		{
			return null;
		}
		if (deck == 0 || slot == 0)
		{
			return CreateResultTable(null, new ErrorList { Redbox.KioskEngine.ComponentModel.Error.NewError("H034", "No locations specified to vendunknown.", "The vendunknown command requires locations on the stack to vend.") });
		}
		ErrorList errors = hardwareService.VendUnknown(eventHandler, statusChangeHandler, deck, slot, out var jobId);
		return CreateResultTable(jobId, errors);
	}

	[KernelFunction(Name = "kernel.hardwarereturn")]
	internal static LuaTable? HardwareReturn(HardwareEventHandler eventHandler, HardwareStatusChangeHandler statusChangeHandler)
	{
		IHardwareService hardwareService = ServiceLocator.Instance.GetService<IHardwareService>();
		if (hardwareService == null)
		{
			return null;
		}
		ErrorList errors = hardwareService.Return(eventHandler, statusChangeHandler, out var jobId);
		return CreateResultTable(jobId, errors);
	}

	[KernelFunction(Name = "kernel.hardwarereturnunknown")]
	internal static LuaTable? HardwareReturnUnknown(HardwareEventHandler eventHandler, HardwareStatusChangeHandler statusChangeHandler, string returnTime)
	{
		IHardwareService hardwareService = ServiceLocator.Instance.GetService<IHardwareService>();
		if (hardwareService == null)
		{
			return null;
		}
		ErrorList errors = hardwareService.ReturnUnknown(eventHandler, statusChangeHandler, returnTime, out var jobId);
		return CreateResultTable(jobId, errors);
	}

	[KernelFunction(Name = "kernel.hardwareisbusy")]
	internal static LuaTable? HardwareIsBusy()
	{
		IHardwareService hardwareService = ServiceLocator.Instance.GetService<IHardwareService>();
		if (hardwareService == null)
		{
			return null;
		}
		ErrorList schedulerStatus = hardwareService.GetSchedulerStatus(out var status);
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
			IHardwareService hardwareService = ServiceLocator.Instance.GetService<IHardwareService>();
			if (hardwareService != null && (hardwareService.GetSchedulerStatus(out var status).ContainsError() || status == jobid || status == "Idle"))
			{
				timerService.RemoveTimer(jobid);
				idleHandler(status);
			}
		}).Start();
	}

	[KernelFunction(Name = "kernel.hardwarenotifymaintmodestate")]
	internal static LuaTable? HardwareNotifyMaintModeState(bool goingIntoMM)
	{
		IHardwareService hardwareService = ServiceLocator.Instance.GetService<IHardwareService>();
		if (hardwareService == null)
		{
			return null;
		}
		ErrorList errors = hardwareService.NotifyMaintModeState(goingIntoMM);
		return CreateResultTable(string.Empty, errors);
	}

	[KernelFunction(Name = "kernel.hardwaresoftsync")]
	internal static LuaTable? HardwareSoftSync(HardwareEventHandler eventHandler, HardwareStatusChangeHandler statusChangeHandler)
	{
		IHardwareService hardwareService = ServiceLocator.Instance.GetService<IHardwareService>();
		if (hardwareService == null)
		{
			return null;
		}
		ErrorList errors = hardwareService.SoftSync(eventHandler, statusChangeHandler, out var jobId);
		return CreateResultTable(jobId, errors);
	}

	[KernelFunction(Name = "kernel.hardwaregetmachineinfo")]
	internal static LuaTable HardwareGetMachineInfo()
	{
		IHardwareState hardwareState = ServiceLocator.Instance.GetService<IHardwareState>();
		IMachineSettingsStore machineSettingsStore = ServiceLocator.Instance.GetService<IMachineSettingsStore>();
		if (!string.IsNullOrEmpty(hardwareState.QlmDeckAsString))
		{
			machineSettingsStore.SetValue("Store", "QlmDeck", hardwareState.QlmDeckAsString);
		}
		if (hardwareState.MaxDeck > 0)
		{
			machineSettingsStore.SetValue("Store", "MaxDecks", hardwareState.MaxDeck.ToString());
		}
		if (hardwareState.MaxSlot > 0)
		{
			machineSettingsStore.SetValue("Store", "MaxSlots", hardwareState.MaxSlot.ToString());
		}
		LuaTable value = (ServiceLocator.Instance.GetService<IKernelService>().NewTable() as LuaTable)!; // NewTable returns a LuaTable
		value["QlmDeck"] = hardwareState.QlmDeckAsString;
		value["MaxDecks"] = hardwareState.MaxDeck;
		value["MaxSlots"] = hardwareState.MaxSlot;
		value["MerchType"] = hardwareState.MerchandizeType;
		value["BufferSlot"] = hardwareState.BufferSlot;
		return value;
	}

	[KernelFunction(Name = "kernel.hardwaregethardwarestatus")]
	internal static LuaTable? HardwareGetHardwareStatus(LuaTable startTime, HardwareEventHandler eventHandler, HardwareStatusChangeHandler statusChangeHandler)
	{
		IHardwareService hardwareService = ServiceLocator.Instance.GetService<IHardwareService>();
		if (hardwareService == null)
		{
			return null;
		}
		ErrorList hardwareStatus = hardwareService.GetHardwareStatus(startTime.ToDateTime(), eventHandler, statusChangeHandler, out var jobId);
		startTime.Dispose();
		return CreateResultTable(jobId, hardwareStatus);
	}

	[KernelFunction(Name = "kernel.hardwaresync")]
	internal static LuaTable? HardwareSync(HardwareEventHandler eventHandler, HardwareStatusChangeHandler statusChangeHandler, string label, LuaTable startTime, int startDeck, int startSlot, int endDeck, int endSlot)
	{
		IHardwareService hardwareService = ServiceLocator.Instance.GetService<IHardwareService>();
		if (hardwareService == null)
		{
			return null;
		}
		ErrorList errors = hardwareService.Sync(eventHandler, statusChangeHandler, label, startTime.ToDateTime(), startDeck, startSlot, endDeck, endSlot, out var jobId);
		startTime.Dispose();
		return CreateResultTable(jobId, errors);
	}

	[KernelFunction(Name = "kernel.hardwaresynclocations")]
	internal static LuaTable? HardwareSyncLocations(HardwareEventHandler eventHandler, HardwareStatusChangeHandler statusChangeHandler, string label, LuaTable startTime, LuaTable locationTable)
	{
		IHardwareService hardwareService = ServiceLocator.Instance.GetService<IHardwareService>();
		if (hardwareService == null)
		{
			return null;
		}
		List<int> barcodeList = new List<int>();
		foreach (object value in locationTable.Values)
		{
			barcodeList.Add(Convert.ToInt32(((LuaTable)value)["slot"]));
			barcodeList.Add(Convert.ToInt32(((LuaTable)value)["deck"]));
		}
		ErrorList errors = hardwareService.SyncLocations(eventHandler, statusChangeHandler, label, startTime.ToDateTime(), barcodeList, out var jobId);
		startTime.Dispose();
		locationTable.Dispose();
		return CreateResultTable(jobId, errors);
	}

	[KernelFunction(Name = "kernel.hardwareqlmunload")]
	internal static LuaTable? QlmUnload(HardwareEventHandler eventHandler, HardwareStatusChangeHandler statusChangeHandler, string label, LuaTable startTime)
	{
		IHardwareService hardwareService = ServiceLocator.Instance.GetService<IHardwareService>();
		if (hardwareService == null)
		{
			return null;
		}
		ErrorList errors = hardwareService.QlmUnload(label, startTime.ToDateTime(), eventHandler, statusChangeHandler, out var jobId);
		startTime.Dispose();
		return CreateResultTable(jobId, errors);
	}

	[KernelFunction(Name = "kernel.hardwareqlmthin")]
	internal static LuaTable? QlmThin(HardwareEventHandler eventHandler, HardwareStatusChangeHandler statusChangeHandler, string label, LuaTable startTime, LuaTable inventory)
	{
		IHardwareService hardwareService = ServiceLocator.Instance.GetService<IHardwareService>();
		if (hardwareService == null)
		{
			return null;
		}
		List<string?> barcodeList = new List<string?>();
		foreach (LuaTable value in inventory.Values)
		{
			string? barcodeValue = (string?)value["barcode"];
			if (!barcodeList.Contains(barcodeValue))
			{
				barcodeList.Add(barcodeValue);
			}
		}
		ErrorList errors = hardwareService.QlmThin(label, startTime.ToDateTime(), barcodeList.ToArray(), eventHandler, statusChangeHandler, out var jobId);
		startTime.Dispose();
		inventory.Dispose();
		return CreateResultTable(jobId, errors);
	}

	[KernelFunction(Name = "kernel.hardwarevmzthin")]
	internal static LuaTable? VmzThin(HardwareEventHandler eventHandler, HardwareStatusChangeHandler statusChangeHandler, string label, LuaTable startTime, LuaTable barcodesToBin, LuaTable rebalances, LuaTable redeployments, LuaTable barcodesToVmz)
	{
		IHardwareService hardwareService = ServiceLocator.Instance.GetService<IHardwareService>();
		if (hardwareService == null)
		{
			return null;
		}
		string?[] barcodesToThin = ((barcodesToBin != null) ? (from LuaTable r in barcodesToBin.Values
			where r != null
			select (string?)r["barcode"]).Distinct().ToArray() : Array.Empty<string?>());
		string?[] rebalances2 = ((rebalances != null) ? (from LuaTable r in rebalances.Values
			where r != null
			select (string?)r["barcode"]).Distinct().ToArray() : Array.Empty<string?>());
		string?[] redeployments2 = ((redeployments != null) ? (from LuaTable r in redeployments.Values
			where r != null
			select (string?)r["barcode"]).Distinct().ToArray() : Array.Empty<string?>());
		string?[] barcodesToVmz2 = ((barcodesToVmz != null) ? (from LuaTable r in barcodesToVmz.Values
			where r != null
			select (string?)r["barcode"]).Distinct().ToArray() : Array.Empty<string?>());
		ErrorList errors = hardwareService.VmzThin(label, startTime.ToDateTime(), barcodesToThin, rebalances2, redeployments2, barcodesToVmz2, eventHandler, statusChangeHandler, out var jobId);
		startTime.Dispose();
		barcodesToBin!.Dispose(); // the four tables are disposed even when nil (throws, as before)
		rebalances!.Dispose();
		redeployments!.Dispose();
		barcodesToVmz!.Dispose();
		return CreateResultTable(jobId, errors);
	}

	[KernelFunction(Name = "kernel.hardwareqlmunloadandthin")]
	internal static LuaTable? QlmUnloadAndThin(HardwareEventHandler eventHandler, HardwareStatusChangeHandler statusChangeHandler, string label, LuaTable startTime, LuaTable inventory)
	{
		IHardwareService hardwareService = ServiceLocator.Instance.GetService<IHardwareService>();
		if (hardwareService == null)
		{
			return null;
		}
		List<string?> barcodeList = new List<string?>();
		foreach (LuaTable value in inventory.Values)
		{
			string? barcodeValue = (string?)value["barcode"];
			if (!barcodeList.Contains(barcodeValue))
			{
				barcodeList.Add(barcodeValue);
			}
		}
		ErrorList errors = hardwareService.QlmUnloadAndThin(label, startTime.ToDateTime(), barcodeList.ToArray(), eventHandler, statusChangeHandler, out var jobId);
		startTime.Dispose();
		inventory.Dispose();
		return CreateResultTable(jobId, errors);
	}

	[KernelFunction(Name = "kernel.prepforredeploy")]
	internal static LuaTable? PrepForRedeploy(LuaTable decksToChange, HardwareEventHandler eventHandler, HardwareStatusChangeHandler statusChangeHandler)
	{
		IHardwareService hardwareService = ServiceLocator.Instance.GetService<IHardwareService>();
		if (hardwareService == null)
		{
			return null;
		}
		List<string?> deckList = new List<string?>();
		foreach (LuaTable value in decksToChange.Values)
		{
			string? deckValue = (string?)value["deck"];
			if (!string.IsNullOrEmpty(deckValue) && !deckList.Contains(deckValue))
			{
				deckList.Add(deckValue);
			}
		}
		ErrorList errors = hardwareService.PrepForRedeploy(deckList.ToArray(), eventHandler, statusChangeHandler, out var jobId);
		decksToChange.Dispose();
		return CreateResultTable(jobId, errors);
	}

	[KernelFunction(Name = "kernel.hardwareschedulejob")]
	internal static LuaTable? ScheduleJob(string scriptName, string label, bool enableDebugging, string priority, string startTimeString, object stack, HardwareEventHandler eventHandler, HardwareStatusChangeHandler statusChangeHandler, bool allowMultiple)
	{
		IHardwareService hardwareService = ServiceLocator.Instance.GetService<IHardwareService>();
		if (hardwareService == null)
		{
			return null;
		}
		List<object?> barcodeList = new List<object?>();
		if (stack is LuaTable result)
		{
			for (int i = 1; i <= result.Keys.Count; i++)
			{
				object? value = result[i];
				if (value is double)
				{
					value = Convert.ToInt32(value);
				}
				barcodeList.Add(value);
			}
		}
		ErrorList errors = hardwareService.ScheduleJob(scriptName, label, enableDebugging, priority, startTimeString, barcodeList.AsReadOnly(), eventHandler, statusChangeHandler, allowMultiple, out var jobId);
		return CreateResultTable(jobId, errors);
	}

	[KernelFunction(Name = "kernel.hardwaresuspendjob")]
	internal static LuaTable? HardwareSuspendJob(string jobId)
	{
		IHardwareService hardwareService = ServiceLocator.Instance.GetService<IHardwareService>();
		if (hardwareService == null)
		{
			return null;
		}
		return CreateResultTable(jobId, hardwareService.Suspend(jobId));
	}

	[KernelFunction(Name = "kernel.hardwareresumejob")]
	internal static LuaTable? HardwareResumeJob(string jobId)
	{
		IHardwareService hardwareService = ServiceLocator.Instance.GetService<IHardwareService>();
		if (hardwareService == null)
		{
			return null;
		}
		return CreateResultTable(jobId, hardwareService.Resume(jobId));
	}

	[KernelFunction(Name = "kernel.hardwaretrashjob")]
	internal static LuaTable? HardwareTrashJob(string jobId)
	{
		IHardwareService hardwareService = ServiceLocator.Instance.GetService<IHardwareService>();
		if (hardwareService == null)
		{
			return null;
		}
		return CreateResultTable(jobId, hardwareService.Trash(jobId));
	}

	[KernelFunction(Name = "kernel.hardwareterminatejob")]
	internal static LuaTable? HardwareTerminateJob(string jobId)
	{
		IHardwareService hardwareService = ServiceLocator.Instance.GetService<IHardwareService>();
		if (hardwareService == null)
		{
			return null;
		}
		return CreateResultTable(jobId, hardwareService.Terminate(jobId));
	}

	[KernelFunction(Name = "kernel.hardwareresumealljobs")]
	internal static LuaTable? HardwareResumeAllJobs()
	{
		IHardwareService hardwareService = ServiceLocator.Instance.GetService<IHardwareService>();
		if (hardwareService == null)
		{
			return null;
		}
		return CreateResultTable(string.Empty, hardwareService.ResumeAll());
	}

	[KernelFunction(Name = "kernel.hardwaresuspendalljobs")]
	internal static LuaTable? HardwareSuspendAllJobs()
	{
		IHardwareService hardwareService = ServiceLocator.Instance.GetService<IHardwareService>();
		if (hardwareService == null)
		{
			return null;
		}
		return CreateResultTable(string.Empty, hardwareService.SuspendAll());
	}

	[KernelFunction(Name = "kernel.hardwaremaintenancemodesuspendjobs")]
	internal static LuaTable? HardwareMaintenanceModeSuspendJobs()
	{
		IHardwareService hardwareService = ServiceLocator.Instance.GetService<IHardwareService>();
		if (hardwareService == null)
		{
			return null;
		}
		return CreateResultTable(string.Empty, hardwareService.SuspendAllJobsForMaintenance());
	}

	[KernelFunction(Name = "kernel.hardwaremaintenancemoderesumejobs")]
	internal static LuaTable? HardwareMaintenanceModeResumeJobs()
	{
		IHardwareService hardwareService = ServiceLocator.Instance.GetService<IHardwareService>();
		if (hardwareService == null)
		{
			return null;
		}
		return CreateResultTable(string.Empty, hardwareService.ResumeAllMaintenanceSuspendedJobs());
	}

	[KernelFunction(Name = "kernel.hardwareexecuteimmediate")]
	internal static LuaTable? HardwareExecuteImmediate(string command, HardwareImmediateExecutionHandler eventHandler, bool clearStack)
	{
		IHardwareService hardwareService = ServiceLocator.Instance.GetService<IHardwareService>();
		if (hardwareService == null)
		{
			return null;
		}
		IResourceBundleService resourceBundleService = ServiceLocator.Instance.GetService<IResourceBundleService>();
		return CreateResultTable(string.Empty, hardwareService.ExecuteImmediate(command, eventHandler, clearStack, resourceBundleService?.CurrentBundleName));
	}

	[KernelFunction(Name = "kernel.hardwaregetprogramresults")]
	internal static LuaTable? HardwareGetProgramResults(string jobId)
	{
		IHardwareService hardwareService = ServiceLocator.Instance.GetService<IHardwareService>();
		if (hardwareService == null)
		{
			return null;
		}
		ErrorList programResults = hardwareService.GetProgramResults(jobId, out var hardwareResults);
		LuaTable result = CreateResultTable(jobId, programResults);
		IKernelService kernelService = ServiceLocator.Instance.GetService<IKernelService>();
		LuaTable resultsTable = (LuaTable)(result["results"] = (LuaTable)kernelService.NewTable());
		int index = 1;
		foreach (IHardwareResult each in hardwareResults)
		{
			LuaTable entry = (LuaTable)kernelService.NewTable();
			entry["id"] = each.ID;
			entry["code"] = each.Code;
			entry["deck"] = each.Deck;
			entry["slot"] = each.Slot;
			entry["item_id"] = each.ItemID;
			entry["message"] = each.Message;
			entry["meta_data"] = each.MetaData;
			DateTime dateTime = DateTime.Parse(each.Timestamp);
			entry["time_stamp"] = dateTime.ToString("yyyyMMddhhmm");
			entry["time_stamp24"] = dateTime.ToString("yyyyMMddHHmm");
			entry["previous_item"] = each.PreviousItem;
			entry["return_time"] = each.ReturnTime;
			resultsTable[index++] = entry;
		}
		return result;
	}

	[KernelFunction(Name = "kernel.hardwaregetjoberrors")]
	internal static LuaTable? HardwareGetJobErrors(string jobId)
	{
		IHardwareService hardwareService = ServiceLocator.Instance.GetService<IHardwareService>();
		if (hardwareService == null)
		{
			return null;
		}
		LuaTable result = CreateResultTable(jobId, hardwareService.GetJobErrors(jobId, out var errors));
		IKernelService kernelService = ServiceLocator.Instance.GetService<IKernelService>();
		LuaTable resultsTable = (LuaTable)(result["results"] = (LuaTable)kernelService.NewTable());
		int index = 1;
		if (errors != null)
		{
			foreach (Redbox.KioskEngine.ComponentModel.Error each in errors)
			{
				LuaTable entry = (LuaTable)kernelService.NewTable();
				entry["code"] = each.Code;
				entry["details"] = each.Details;
				entry["is_warning"] = each.IsWarning;
				entry["description"] = each.Description;
				resultsTable[index++] = entry;
			}
		}
		return result;
	}

	[KernelFunction(Name = "kernel.is_active_hardware_job")]
	internal static bool IsActiveHardwareJob(string jobId)
	{
		IHardwareService hardwareService = ServiceLocator.Instance.GetService<IHardwareService>();
		ReadOnlyCollection<IHardwareJob>? hardwareJobs = null;
		hardwareService?.GetHardwareJobs(out hardwareJobs);
		bool isActive = hardwareJobs?.Any((IHardwareJob x) => x.ID == jobId && (x.Status == "Pending" || x.Status == "Running" || x.Status == "Suspended")) ?? false;
		LogHelper.Instance.Log(string.Format("IsActiveHardwareJob {0}: {1};  Job count: {2}, jobs: {3} ", jobId, isActive, hardwareJobs?.Count, string.Join(", ", hardwareJobs!.Select((IHardwareJob x) => x.ID + ":" + x.Status).ToArray())));
		return isActive;
	}

	[KernelFunction(Name = "kernel.hardwaregetjobs")]
	internal static LuaTable? HardwareGetJobs()
	{
		IHardwareService hardwareService = ServiceLocator.Instance.GetService<IHardwareService>();
		if (hardwareService == null)
		{
			return null;
		}
		LuaTable result = CreateResultTable(string.Empty, hardwareService.GetHardwareJobs(out var hardwareJobs));
		IKernelService kernelService = ServiceLocator.Instance.GetService<IKernelService>();
		LuaTable resultsTable = (LuaTable)(result["results"] = (LuaTable)kernelService.NewTable());
		int index = 1;
		foreach (IHardwareJob each in hardwareJobs)
		{
			LuaTable entry = (LuaTable)kernelService.NewTable();
			entry["id"] = each.ID;
			entry["status"] = each.Status;
			entry["label"] = each.Label;
			entry["program"] = each.ProgramName;
			entry["starttime"] = each.StartTime;
			entry["executiontime"] = each.ExecutionTime;
			entry["priority"] = each.Priority;
			resultsTable[index++] = entry;
		}
		return result;
	}

	[KernelFunction(Name = "kernel.hardwaregetsuspendedmaintenancemodejobs")]
	internal static LuaTable? HardwareGetSuspendedMaintenanceModeJobs()
	{
		IHardwareService hardwareService = ServiceLocator.Instance.GetService<IHardwareService>();
		if (hardwareService == null)
		{
			return null;
		}
		ErrorList suspendedMaintenanceModeJobs = hardwareService.GetSuspendedMaintenanceModeJobs(out var suspendedJobs);
		LuaTable result = CreateResultTable(string.Empty, suspendedMaintenanceModeJobs);
		IKernelService kernelService = ServiceLocator.Instance.GetService<IKernelService>();
		LuaTable resultsTable = (LuaTable)(result["results"] = (LuaTable)kernelService.NewTable());
		int index = 1;
		foreach (IHardwareJob each in suspendedJobs)
		{
			LuaTable entry = (LuaTable)kernelService.NewTable();
			entry["id"] = each.ID;
			entry["status"] = each.Status;
			entry["label"] = each.Label;
			entry["program"] = each.ProgramName;
			entry["starttime"] = each.StartTime;
			entry["executiontime"] = each.ExecutionTime;
			entry["priority"] = each.Priority;
			resultsTable[index++] = entry;
		}
		return result;
	}

	[KernelFunction(Name = "kernel.hardwareexecuteservicecommand")]
	internal static LuaTable? HardwareExecuteServiceCommand(string command)
	{
		IHardwareService hardwareService = ServiceLocator.Instance.GetService<IHardwareService>();
		if (hardwareService == null)
		{
			return null;
		}
		return CreateResultTable(string.Empty, hardwareService.ExecuteServiceCommand(command));
	}

	[KernelFunction(Name = "kernel.hardwaregetinitstatus")]
	internal static LuaTable? HardwareGetInitStatus()
	{
		IHardwareService hardwareService = ServiceLocator.Instance.GetService<IHardwareService>();
		if (hardwareService == null)
		{
			return null;
		}
		ErrorList initStatus = hardwareService.GetInitStatus(out var status);
		LuaTable result = CreateResultTable(string.Empty, initStatus);
		result["status"] = status;
		return result;
	}

	[KernelFunction(Name = "kernel.hardwarereadfrauddisc")]
	internal static LuaTable? HardwareReadFraudDisc(HardwareEventHandler eventHandler, HardwareStatusChangeHandler statusChangeHandler)
	{
		IHardwareService hardwareService = ServiceLocator.Instance.GetService<IHardwareService>();
		if (hardwareService == null)
		{
			return null;
		}
		ErrorList errors = hardwareService.ReadFraudDisc(eventHandler, statusChangeHandler, out var jobId);
		return CreateResultTable(jobId, errors);
	}

	[KernelFunction(Name = "kernel.hardwareprepunloadzone")]
	internal static LuaTable? HardwarePrepUnloadZone(int rotation, int last_deck, HardwareEventHandler eventHandler, HardwareStatusChangeHandler statusChangeHandler)
	{
		IHardwareService hardwareService = ServiceLocator.Instance.GetService<IHardwareService>();
		if (hardwareService == null)
		{
			return null;
		}
		LogHelper.Instance.Log($"kernel.hardwareprepunloadzone| rotation:{rotation} last_deck:{last_deck}");
		ErrorList errors = hardwareService.PrepUnloadZone(rotation, last_deck, eventHandler, statusChangeHandler, out var jobId);
		return CreateResultTable(jobId, errors);
	}

	[KernelFunction(Name = "kernel.hasabedevice")]
	internal static string? HasAbeDevice()
	{
		IHardwareService hardwareService = ServiceLocator.Instance.GetService<IHardwareService>();
		if (hardwareService == null)
		{
			return null;
		}
		hardwareService.HasAbeDevice(out var hasAbeDevice);
		return hasAbeDevice.ToString();
	}

	[KernelFunction(Name = "kernel.irhardwareinstalled")]
	internal static string? IRHardwareInstalled()
	{
		IHardwareService hardwareService = ServiceLocator.Instance.GetService<IHardwareService>();
		if (hardwareService == null)
		{
			return null;
		}
		hardwareService.IRHardwareInstalled(out var installDate);
		string? result = null;
		if (installDate.HasValue)
		{
			result = installDate.Value.ToString("o");
		}
		return result;
	}

	[KernelFunction(Name = "kernel.enqueue_hardware_properties")]
	internal static void EnqueueHardwareProperties()
	{
		IHardwareState hardwareState = ServiceLocator.Instance.GetService<IHardwareState>();
		IHealthServices healthServices = ServiceLocator.Instance.GetService<IHealthServices>();
		if (hardwareState == null)
		{
			LogHelper.Instance.Log("Hardware State was not found.");
		}
		if (healthServices == null)
		{
			LogHelper.Instance.Log("Health Service was not found.");
		}
		LogHelper.Instance.Log("Enqueue Hardware State properties.", LogEntryType.Info);
		LogHelper.Instance.Log("Camera: " + hardwareState!.CurrentCameraGeneration + ", Decoder: " + hardwareState.BarcodeDecoder);
		if (hardwareState.CurrentCameraGeneration >= 3)
		{
			healthServices!.EnqueueProperty("CameraType", hardwareState.CurrentCameraGeneration.ToString());
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

	private static LuaTable CreateResultTable(string? jobId, ErrorList errors)
	{
		IKernelService kernelService = ServiceLocator.Instance.GetService<IKernelService>();
		LuaTable result = (LuaTable)kernelService.NewTable();
		result["success"] = !errors.ContainsError();
		result["job_id"] = jobId;
		LuaTable errorsTable = (LuaTable)kernelService.NewTable();
		foreach (Redbox.KioskEngine.ComponentModel.Error error in errors)
		{
			LogHelper.Instance.Log("HardwareFunctions.CreateResultTable contains error " + error.ToString());
			errorsTable[error.Code] = error.Description;
		}
		result["errors"] = errorsTable;
		return result;
	}
}
