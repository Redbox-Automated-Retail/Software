using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework;

[NativeJob(ProgramName = "air-exchanger-status", Operand = "AIR-EXCHANGER-STATUS")]
internal sealed class AirExchangerStatusJob : NativeJobAdapter
{
	protected override void ExecuteInner()
	{
		IAirExchangerService airExchangerService = ServiceLocator.Instance.GetService<IAirExchangerService>();
		if (!airExchangerService.Configured)
		{
			base.Context.CreateInfoResult("AirExchangerNotConfigured", "There is no air exchanger configured.");
			return;
		}
		base.Context.CreateInfoResult("AirExchangerConfigured", "There is an air exchanger configured.");
		if (!airExchangerService.ShouldRetry())
		{
			base.Context.CreateInfoResult("PersistentErrorStatus", "The board is in an persistent error state.");
		}
		else
		{
			base.Context.CreateInfoResult("BoardStatusOk", "The board is not in a persistent error state.");
		}
	}

	internal AirExchangerStatusJob(ExecutionResult result, ExecutionContext ctx)
		: base(result, ctx)
	{
	}
}
