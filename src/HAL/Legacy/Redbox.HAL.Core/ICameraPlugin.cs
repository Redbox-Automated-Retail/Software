using System.Collections.Generic;

namespace Redbox.HAL.Core;

public interface ICameraPlugin
{
	bool IsRunning { get; }

	// Not in 1.3.0.84: added in 1.3.0.119 (not recoverable), whose plugins implement and use it; 1.4.0.246 has it too.
	bool SupportsReset { get; }

	bool Start();

	string Snap();

	bool Stop();

	void TurnOff();

	void InitWithProperties(IDictionary<string, object> properties);
}
