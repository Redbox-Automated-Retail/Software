using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework;

[NativeJob(ProgramName = "loopy")]
internal sealed class LoopyJob : NativeJobAdapter
{
	protected override void ExecuteInner()
	{
		IRuntimeService runtimeService = ServiceLocator.Instance.GetService<IRuntimeService>();
		int operand = base.Context.PopTop<int>();
		for (int i = 0; i < operand; i++)
		{
			base.Context.Send($"Message-{i}");
			runtimeService.Wait(1500);
		}
	}

	internal LoopyJob(ExecutionResult result, ExecutionContext context)
		: base(result, context)
	{
	}
}
