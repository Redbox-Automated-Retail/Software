namespace Redbox.HAL.Script.Framework;

[NativeJob(ProgramName = "get-and-read")]
internal sealed class GetAndReadJob : NativeJobAdapter
{
	protected override void ExecuteInner()
	{
		if (new GetAndCenterOperation(base.Context).Run())
		{
			using (Scanner scanner = new Scanner(base.ControlSystem))
			{
				using ScanResult scanResult = scanner.Read();
				scanResult.PushTo(base.Context);
				return;
			}
		}
		AddError("get-and-read failed.");
	}

	internal GetAndReadJob(ExecutionResult result, ExecutionContext ctx)
		: base(result, ctx)
	{
	}
}
