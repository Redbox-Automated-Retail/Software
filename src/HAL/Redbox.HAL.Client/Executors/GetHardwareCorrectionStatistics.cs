using System.Collections.Generic;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Client.Executors;

public sealed class GetHardwareCorrectionStatistics : JobExecutor
{
	private readonly HardwareCorrectionStatistic Stat;

	public List<IHardwareCorrectionStatistic> Stats { get; private set; }

	protected override string JobName => "get-hardware-statistics";

	public GetHardwareCorrectionStatistics(HardwareService s, HardwareCorrectionStatistic stat, HardwareJobPriority p)
		: base(s, p)
	{
		Stat = stat;
		Stats = new List<IHardwareCorrectionStatistic>();
	}

	public GetHardwareCorrectionStatistics(HardwareService s, HardwareCorrectionStatistic stat)
		: this(s, stat, HardwareJobPriority.Highest)
	{
	}

	protected override void SetupJob()
	{
		base.Job.Push(Stat.ToString());
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
