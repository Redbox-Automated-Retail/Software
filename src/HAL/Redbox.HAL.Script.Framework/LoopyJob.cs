using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework;

[NativeJob(ProgramName = "loopy")]
internal sealed class LoopyJob : NativeJobAdapter
{
	protected override void ExecuteInner()
	{
		IRuntimeService service = ServiceLocator.Instance.GetService<IRuntimeService>();
		int num = base.Context.PopTop<int>();
		for (int i = 0; i < num; i++)
		{
			base.Context.Send($"Message-{i}");
			service.Wait(1500);
		}
	}

	internal LoopyJob(ExecutionResult result, ExecutionContext context)
		: base(result, context)
	{
	}
}
