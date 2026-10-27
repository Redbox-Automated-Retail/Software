namespace Redbox.HAL.Script.Framework.Natives;

[NativeJob(ProgramName = "refurbish-kiosk-test", HideFromList = true)]
internal sealed class KioskTestJob : NativeJobAdapter
{
	protected override void ExecuteInner()
	{
		using KioskTestAdapter kioskTestAdapter = new KioskTestAdapter(base.Result, base.Context);
		kioskTestAdapter.Run();
	}

	internal KioskTestJob(ExecutionResult r, ExecutionContext c)
		: base(r, c)
	{
	}
}
