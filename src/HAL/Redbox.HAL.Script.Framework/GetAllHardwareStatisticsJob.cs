using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework;

[NativeJob(ProgramName = "get-all-hardware-statistics")]
internal sealed class GetAllHardwareStatisticsJob : NativeJobAdapter
{
	protected override void ExecuteInner()
	{
		IHardwareCorrectionStatisticService service = ServiceLocator.Instance.GetService<IHardwareCorrectionStatisticService>();
		using StatsFormatter statsFormatter = new StatsFormatter(service.GetStats());
		statsFormatter.Format(base.Context);
	}

	internal GetAllHardwareStatisticsJob(ExecutionResult result, ExecutionContext ctx)
		: base(result, ctx)
	{
	}
}
