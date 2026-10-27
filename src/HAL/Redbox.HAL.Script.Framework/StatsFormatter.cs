using System;
using System.Collections.Generic;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework;

internal struct StatsFormatter : IDisposable
{
	private readonly List<IHardwareCorrectionStatistic> Stats;

	public void Dispose()
	{
		Stats.Clear();
	}

	internal void Format(ExecutionContext context)
	{
		Stats.Sort((IHardwareCorrectionStatistic x, IHardwareCorrectionStatistic y) => x.CorrectionTime.CompareTo(y.CorrectionTime));
		Stats.ForEach(delegate(IHardwareCorrectionStatistic s)
		{
			context.PushTop(s.CorrectionTime.ToString());
			context.PushTop(s.ProgramName);
			context.PushTop(s.CorrectionOk ? "SUCCESS" : "FAILURE");
			context.PushTop(s.Statistic.ToString());
		});
		context.PushTop(Stats.Count);
	}

	internal StatsFormatter(List<IHardwareCorrectionStatistic> s)
	{
		this = default(StatsFormatter);
		Stats = s;
	}
}
