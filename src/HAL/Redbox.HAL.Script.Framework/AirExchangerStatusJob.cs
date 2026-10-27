using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework;

[NativeJob(ProgramName = "air-exchanger-status", Operand = "AIR-EXCHANGER-STATUS")]
internal sealed class AirExchangerStatusJob : NativeJobAdapter
{
	protected override void ExecuteInner()
	{
		IAirExchangerService service = ServiceLocator.Instance.GetService<IAirExchangerService>();
		if (!service.Configured)
		{
			base.Context.CreateInfoResult("AirExchangerNotConfigured", "There is no air exchanger configured.");
			return;
		}
		base.Context.CreateInfoResult("AirExchangerConfigured", "There is an air exchanger configured.");
		if (!service.ShouldRetry())
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
