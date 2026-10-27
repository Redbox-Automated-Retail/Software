using System.Collections.Generic;

namespace Redbox.HAL.Component.Model;

public interface ICameraPlugin
{
	bool IsRunning { get; }

	bool SupportsReset { get; }

	bool Start();

	void Snap(string file);

	bool Stop();

	void InitWithProperties(IDictionary<string, object> properties);
}
