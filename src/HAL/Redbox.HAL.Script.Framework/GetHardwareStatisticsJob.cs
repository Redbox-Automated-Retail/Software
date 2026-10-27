using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Extensions;

namespace Redbox.HAL.Script.Framework;

[NativeJob(ProgramName = "get-hardware-statistics")]
internal sealed class GetHardwareStatisticsJob : NativeJobAdapter
{
	protected override void ExecuteInner()
	{
		HardwareCorrectionStatistic hardwareCorrectionStatistic = Enum<HardwareCorrectionStatistic>.ParseIgnoringCase(base.Context.PopTop<string>(), HardwareCorrectionStatistic.None);
		if (hardwareCorrectionStatistic != HardwareCorrectionStatistic.None)
		{
			IHardwareCorrectionStatisticService service = ServiceLocator.Instance.GetService<IHardwareCorrectionStatisticService>();
			using StatsFormatter statsFormatter = new StatsFormatter(service.GetStats(hardwareCorrectionStatistic));
			statsFormatter.Format(base.Context);
		}
	}

	internal GetHardwareStatisticsJob(ExecutionResult result, ExecutionContext ctx)
		: base(result, ctx)
	{
	}
}
