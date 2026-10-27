namespace Redbox.HAL.Script.Framework;

[NativeJob(ProgramName = "get-and-center")]
internal sealed class GetAndCenterJob : NativeJobAdapter
{
	protected override void ExecuteInner()
	{
		if (!new GetAndCenterOperation(base.Context).Run())
		{
			AddError("get-and-center failed.");
		}
	}

	internal GetAndCenterJob(ExecutionResult result, ExecutionContext ctx)
		: base(result, ctx)
	{
	}
}
