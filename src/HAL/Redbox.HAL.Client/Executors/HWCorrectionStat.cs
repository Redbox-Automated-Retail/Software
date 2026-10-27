using System;
using System.Collections.Generic;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Extensions;

namespace Redbox.HAL.Client.Executors;

internal sealed class HWCorrectionStat : IHardwareCorrectionStatistic
{
	public HardwareCorrectionStatistic Statistic { get; internal set; }

	public string ProgramName { get; internal set; }

	public bool CorrectionOk { get; internal set; }

	public DateTime CorrectionTime { get; internal set; }

	internal static void From(HardwareJob job, List<IHardwareCorrectionStatistic> stats)
	{
		if (job.GetStack(out var stack).Success)
		{
			int num = int.Parse(stack.Pop());
			for (int i = 0; i < num; i++)
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
