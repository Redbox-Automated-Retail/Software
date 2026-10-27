using System;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework;

[NativeJob(ProgramName = "get-controller-timeouts")]
internal sealed class GetControllerTimeoutsJob : NativeJobAdapter
{
	protected override void ExecuteInner()
	{
		IPersistentCounterService service = ServiceLocator.Instance.GetService<IPersistentCounterService>();
		foreach (TimeoutCounters value in Enum.GetValues(typeof(TimeoutCounters)))
		{
			MakeResult(service.Find(value));
		}
		MakeResult(service.Find("EmptyOrStuckCount"));
	}

	internal GetControllerTimeoutsJob(ExecutionResult result, ExecutionContext ctx)
		: base(result, ctx)
	{
	}

	private void MakeResult(IPersistentCounter counter)
	{
		if (counter != null)
		{
			base.Context.CreateInfoResult(counter.Name, counter.Value.ToString());
		}
	}
}
