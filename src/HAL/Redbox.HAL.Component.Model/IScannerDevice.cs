using System;

namespace Redbox.HAL.Component.Model;

public interface IScannerDevice : IDisposable
{
	bool RequiresExternalLighting { get; }

	bool IsConnected { get; }

	bool InError { get; }

	bool IsLegacy { get; }

	bool SupportsSecureReads { get; }

	ScannerServices Service { get; }

	CameraGeneration CurrentCameraGeneration { get; }

	bool Start();

	bool Start(bool testFunction);

	bool Stop();

	bool Restart();

	IScanResult Scan();

	ISnapResult Snap();

	bool ValidateSettings();

	bool ValidateSettings(IMessageSink sink);
}
