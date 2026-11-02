using System;
using System.Collections.Generic;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Extensions;

namespace Redbox.HAL.Client.Executors;

internal sealed class HWCorrectionStat : IHardwareCorrectionStatistic
{
	public HardwareCorrectionStatistic Statistic { get; internal set; }

	// Set by From's object initializer.
	public string ProgramName { get; internal set; } = null!;

	public bool CorrectionOk { get; internal set; }

	public DateTime CorrectionTime { get; internal set; }

	internal static void From(HardwareJob job, List<IHardwareCorrectionStatistic> stats)
	{
		if (job.GetStack(out var stack).Success)
		{
			int number = int.Parse(stack.Pop());
			for (int i = 0; i < number; i++)
			{
				HWCorrectionStat item = new HWCorrectionStat
				{
					Statistic = Enum<HardwareCorrectionStatistic>.ParseIgnoringCase(stack.Pop(), HardwareCorrectionStatistic.None),
					CorrectionOk = stack.Pop().Equals("SUCCESS", StringComparison.CurrentCultureIgnoreCase),
					ProgramName = stack.Pop(),
					CorrectionTime = DateTime.Parse(stack.Pop())
				};
				stats.Add(item);
			}
		}
	}

	private HWCorrectionStat()
	{
	}
}
