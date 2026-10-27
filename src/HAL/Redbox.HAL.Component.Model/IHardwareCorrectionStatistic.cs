using System;

namespace Redbox.HAL.Component.Model;

public interface IHardwareCorrectionStatistic
{
	HardwareCorrectionStatistic Statistic { get; }

	string ProgramName { get; }

	bool CorrectionOk { get; }

	DateTime CorrectionTime { get; }
}
