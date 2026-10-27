using System;

namespace Redbox.HAL.Component.Model;

public interface IScannerDevice : IDisposable
{
	string Name { get; }

	bool RequiresExternalLighting { get; }

	bool IsConnected { get; }

	bool InError { get; }

	bool Start();

	bool Stop();

	bool Restart();

	IScanResult Scan();

	string FindInstalledPort();

	ISnapResult Snap();

	ISnapResult Snap(string path);
}
