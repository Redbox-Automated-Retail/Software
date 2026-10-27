using System;

namespace Redbox.KioskEngine.ComponentModel;

public interface IKioskFunctionCheckData
{
	string CameraDriverTestResult { get; }

	string InitTestResult { get; }

	string SnapDecodeTestResult { get; }

	DateTime Timestamp { get; }

	string TouchscreenDriverTestResult { get; }

	string TrackTestResult { get; }

	string UserIdentifier { get; }

	string VendDoorTestResult { get; }

	string VerticalSlotTestResult { get; }
}
