using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework;

[NativeJob(ProgramName = "vmz-removed-thins", Operand = "VMZ-REMOVED-THINS")]
internal sealed class VMZRemovedThinsJob : NativeJobAdapter
{
	protected override void ExecuteInner()
	{
		ApplicationLog applicationLog = ApplicationLog.ConfigureLog(base.Context, logAppends: true, "VMZ", flushOnWrite: false, "vmz-removed-disks");
		applicationLog.Write("** Remove thins from VMZ **");
		VMZ.Instance.DumpZone(applicationLog);
		IInventoryService service = ServiceLocator.Instance.GetService<IInventoryService>();
		int totalEmptyNow = service.GetMachineEmptyCount();
		ILocation location = SegmentManager.Instance.FindHighest();
		if (location != null)
		{
			VMZ.Instance.ForAllBetweenDo(delegate(ILocation loc)
			{
				if (loc.IsEmpty)
				{
					return true;
				}
				if (SegmentManager.Instance.IsThinType(loc.Flags))
				{
					base.Context.CreateResult("ThinTransferSuccessful", "The disk was removed from the VMZ.", loc.Deck, loc.Slot, loc.ID, null, null);
					base.Context.CreateResult($"{loc.Flags.ToString()}DiskRemovedFromVMZ", "Disk merchandizing metadata.", loc.Deck, loc.Slot, loc.ID, null, null);
				}
				else
				{
					base.Context.CreateResult("NonThinRemoved", "The disk was removed from the VMZ.", loc.Deck, loc.Slot, loc.ID, null, null);
				}
				int num = totalEmptyNow + 1;
				totalEmptyNow = num;
				return true;
			}, location);
		}
		base.Context.CreateInfoResult("MachineEmptySlots", totalEmptyNow.ToString());
	}

	internal VMZRemovedThinsJob(ExecutionResult result, ExecutionContext ctx)
		: base(result, ctx)
	{
	}
}
