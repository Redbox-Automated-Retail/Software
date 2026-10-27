using System;

namespace Redbox.HardwareServices.Proxy.ComponentModel;

public interface IHardwareCorrectionStatistic
{
	HardwareCorrectionStatisticType StatisticType { get; }

	string ProgramName { get; }

	bool CorrectionOk { get; }

	DateTime CorrectionTime { get; }
}
