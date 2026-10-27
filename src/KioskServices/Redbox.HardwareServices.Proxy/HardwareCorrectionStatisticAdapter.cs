using System;
using Redbox.HAL.Component.Model;
using Redbox.HardwareServices.Proxy.ComponentModel;

namespace Redbox.HardwareServices.Proxy;

internal class HardwareCorrectionStatisticAdapter : Redbox.HardwareServices.Proxy.ComponentModel.IHardwareCorrectionStatistic, Redbox.HAL.Component.Model.IHardwareCorrectionStatistic
{
	public HardwareCorrectionStatisticType StatisticType { get; set; }

	public HardwareCorrectionStatistic Statistic { get; set; }

	public string ProgramName { get; }

	public bool CorrectionOk { get; }

	public DateTime CorrectionTime { get; }

	public HardwareCorrectionStatisticAdapter()
	{
	}

	public HardwareCorrectionStatisticAdapter(Redbox.HAL.Component.Model.IHardwareCorrectionStatistic stat)
	{
		Statistic = stat.Statistic;
		StatisticType = stat.Statistic.ToStatType();
		ProgramName = stat.ProgramName;
		CorrectionOk = stat.CorrectionOk;
		CorrectionTime = stat.CorrectionTime;
	}
}
