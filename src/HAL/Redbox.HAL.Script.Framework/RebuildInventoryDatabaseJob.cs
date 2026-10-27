namespace Redbox.HAL.Script.Framework;

[NativeJob(ProgramName = "rebuild-inventory-database")]
internal sealed class RebuildInventoryDatabaseJob : NativeJobAdapter
{
	protected override void ExecuteInner()
	{
		base.InventoryService.Rebuild(base.Result.Errors);
		if (base.Result.Errors.ContainsError())
		{
			AddError("Failed to rebuild database.");
		}
	}

	internal RebuildInventoryDatabaseJob(ExecutionResult result, ExecutionContext ctx)
		: base(result, ctx)
	{
	}
}
