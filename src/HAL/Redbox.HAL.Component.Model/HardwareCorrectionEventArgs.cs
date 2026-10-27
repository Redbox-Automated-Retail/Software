using System;

namespace Redbox.HAL.Component.Model;

public sealed class HardwareCorrectionEventArgs : EventArgs
{
	public HardwareCorrectionStatistic Statistic { get; private set; }

	public bool CorrectionOk { get; set; }

	public HardwareCorrectionEventArgs(HardwareCorrectionStatistic stat)
	{
		Statistic = stat;
	}
}
