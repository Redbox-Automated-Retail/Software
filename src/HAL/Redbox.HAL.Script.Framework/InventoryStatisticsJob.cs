using System.Collections.Generic;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Controller.Framework;

namespace Redbox.HAL.Script.Framework;

[NativeJob(ProgramName = "get-inventory-stats")]
internal sealed class InventoryStatisticsJob : NativeJobAdapter
{
	protected override void ExecuteInner()
	{
		ApplicationLog applicationLog = ApplicationLog.ConfigureLog(base.Context, logAppends: true, "Service", flushOnWrite: false, "InventoryStats");
		if (!TestInventoryStore())
		{
			return;
		}
		int totalMachineSlots = 0;
		base.DecksService.ForAllDecksDo(delegate(IDeck deck)
		{
			if (!deck.IsQlm)
			{
				totalMachineSlots += deck.NumberOfSlots;
			}
			return true;
		});
		int machineEmptyCount = base.InventoryService.GetMachineEmptyCount();
		List<ILocation> unknowns = base.InventoryService.GetUnknowns();
		int excludedSlotsCount = base.InventoryService.GetExcludedSlotsCount();
		int num = machineEmptyCount - ControllerConfiguration.Instance.ReturnSlotBuffer;
		if (num < 0)
		{
			num = 0;
		}
		applicationLog.Write("Inventory snapshot.");
		applicationLog.WriteFormatted(" Total Empty: {0}", machineEmptyCount);
		applicationLog.WriteFormatted(" Usable empty: {0}", num);
		applicationLog.WriteFormatted(" Unknown: {0}", unknowns.Count);
		applicationLog.WriteFormatted(" Excluded: {0}", excludedSlotsCount);
		applicationLog.WriteFormatted(" Total Slots: {0}", totalMachineSlots);
		applicationLog.WriteFormatted(" Dumpbin capacity: {0}", base.DumpbinService.Capacity);
		applicationLog.WriteFormatted(" Dumpbin current count: {0}", base.DumpbinService.CurrentCount());
		base.Context.CreateInfoResult("TotalEmptyCount", machineEmptyCount.ToString());
		base.Context.CreateInfoResult("UsableEmptyCount", num.ToString());
		base.Context.CreateInfoResult("UnknownCount", unknowns.Count.ToString());
		base.Context.CreateInfoResult("ExcludedCount", excludedSlotsCount.ToString());
		base.Context.CreateInfoResult("TotalMachineSlots", totalMachineSlots.ToString());
		base.Context.CreateInfoResult("DumpBinCapacity", base.DumpbinService.Capacity.ToString());
		base.Context.CreateInfoResult("DumpBinCount", base.DumpbinService.CurrentCount().ToString());
		if (base.InventoryService.MachineIsFull())
		{
			base.Context.CreateMachineFullResult();
		}
	}

	internal InventoryStatisticsJob(ExecutionResult result, ExecutionContext ctx)
		: base(result, ctx)
	{
	}
}
