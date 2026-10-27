using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework;

[NativeJob(ProgramName = "reset-quick-return-counters")]
internal sealed class QuickReturnResetCountersJob : NativeJobAdapter
{
	protected override void ExecuteInner()
	{
		int errors = 0;
		IPersistentCounterService pcs = ServiceLocator.Instance.GetService<IPersistentCounterService>();
		new QuickReturnCounters().DoForeach(delegate(string counter)
		{
			if (!pcs.Reset(counter))
			{
				int num = errors + 1;
				errors = num;
			}
		});
		if (errors > 0)
		{
			AddError("Failed to reset counter.");
		}
	}

	internal QuickReturnResetCountersJob(ExecutionResult result, ExecutionContext ctx)
		: base(result, ctx)
	{
	}
}
