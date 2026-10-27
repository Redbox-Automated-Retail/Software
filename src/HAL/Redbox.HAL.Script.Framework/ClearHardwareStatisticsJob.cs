using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Extensions;

namespace Redbox.HAL.Script.Framework;

[NativeJob(ProgramName = "clear-hardware-statistics")]
internal sealed class ClearHardwareStatisticsJob : NativeJobAdapter
{
	protected override void ExecuteInner()
	{
		HardwareCorrectionStatistic hardwareCorrectionStatistic = Enum<HardwareCorrectionStatistic>.ParseIgnoringCase(base.Context.PopTop<string>(), HardwareCorrectionStatistic.None);
		if (hardwareCorrectionStatistic != HardwareCorrectionStatistic.None)
		{
			ServiceLocator.Instance.GetService<IHardwareCorrectionStatisticService>().RemoveAll(hardwareCorrectionStatistic);
		}
	}

	internal ClearHardwareStatisticsJob(ExecutionResult result, ExecutionContext ctx)
		: base(result, ctx)
	{
	}
}
