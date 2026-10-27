using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework;

[NativeJob(ProgramName = "get-quick-return-counters")]
internal sealed class QuickReturnCountersJob : NativeJobAdapter
{
	protected override void ExecuteInner()
	{
		IPersistentCounterService pcs = ServiceLocator.Instance.GetService<IPersistentCounterService>();
		new QuickReturnCounters().DoForeach(delegate(string counter)
		{
			IPersistentCounter persistentCounter = pcs.Find(counter);
			string message = ((persistentCounter == null) ? "0" : persistentCounter.Value.ToString());
			base.Context.CreateInfoResult(counter, message);
		});
	}

	internal QuickReturnCountersJob(ExecutionResult result, ExecutionContext ctx)
		: base(result, ctx)
	{
	}
}
