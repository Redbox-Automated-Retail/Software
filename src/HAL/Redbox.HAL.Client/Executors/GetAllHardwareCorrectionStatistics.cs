using System.Collections.Generic;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Client.Executors;

public sealed class GetAllHardwareCorrectionStatistics : JobExecutor
{
	public List<IHardwareCorrectionStatistic> Stats { get; private set; }

	protected override string JobName => "get-all-hardware-statistics";

	public GetAllHardwareCorrectionStatistics(HardwareService s, HardwareJobPriority p)
		: base(s, p)
	{
		Stats = new List<IHardwareCorrectionStatistic>();
	}

	public GetAllHardwareCorrectionStatistics(HardwareService s)
		: this(s, HardwareJobPriority.Highest)
	{
	}

	protected override void DisposeInner()
	{
		Stats.Clear();
	}

	protected override void OnJobCompleted()
	{
		HWCorrectionStat.From(base.Job, Stats);
	}
}
