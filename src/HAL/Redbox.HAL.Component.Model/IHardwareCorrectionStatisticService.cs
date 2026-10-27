using System;
using System.Collections.Generic;

namespace Redbox.HAL.Component.Model;

public interface IHardwareCorrectionStatisticService
{
	bool Insert(HardwareCorrectionEventArgs args, IExecutionContext context);

	bool Insert(HardwareCorrectionStatistic statistic, IExecutionContext context, bool correctionOk);

	bool Insert(HardwareCorrectionStatistic statistic, IExecutionContext context, bool correctionOk, DateTime timestamp);

	bool RemoveAll();

	bool RemoveAll(HardwareCorrectionStatistic stat);

	List<IHardwareCorrectionStatistic> GetStats();

	List<IHardwareCorrectionStatistic> GetStats(HardwareCorrectionStatistic key);
}
