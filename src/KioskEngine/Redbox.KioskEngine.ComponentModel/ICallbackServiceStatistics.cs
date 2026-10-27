using System;

namespace Redbox.KioskEngine.ComponentModel;

public interface ICallbackServiceStatistics
{
	int NumberOfCallbacksExecuted { get; set; }

	TimeSpan TotalCallbackDuration { get; set; }

	TimeSpan MaxCallbackDuration { get; set; }

	TimeSpan AverageCallbackDuration { get; set; }

	TimeSpan TimeSinceLastLogged { get; set; }

	string MaxCallbackDurationMessage { get; set; }
}
