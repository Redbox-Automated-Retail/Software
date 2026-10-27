namespace Redbox.HAL.Script.Framework;

[NativeJob(ProgramName = "vmz-details", Operand = "VMZ-DETAILS")]
internal sealed class GetVMZDetailJob : NativeJobAdapter
{
	protected override void ExecuteInner()
	{
		using (VMZIterator vMZIterator = new VMZIterator())
		{
			do
			{
				base.Context.CreateResult("VMZLocationDetail", vMZIterator.Flags.ToString(), vMZIterator.Location.Deck, vMZIterator.Location.Slot, vMZIterator.Location.ID, null, null);
			}
			while (vMZIterator.Up());
		}
		base.Context.CreateInfoResult("DumpBinInventoryCount", base.DumpbinService.CurrentCount().ToString());
	}

	internal GetVMZDetailJob(ExecutionResult result, ExecutionContext ctx)
		: base(result, ctx)
	{
	}
}
