using System;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.DataStorage;

internal sealed class HardwareCorrectionStat : IHardwareCorrectionStatistic
{
	public HardwareCorrectionStatistic Statistic { get; internal set; }

	public string ProgramName { get; internal set; } = null!; // set by HardwareCorrectionStatsTable or TableTypeFactory

	public bool CorrectionOk { get; internal set; }

	public DateTime CorrectionTime { get; internal set; }

	internal HardwareCorrectionStat()
	{
	}
}
