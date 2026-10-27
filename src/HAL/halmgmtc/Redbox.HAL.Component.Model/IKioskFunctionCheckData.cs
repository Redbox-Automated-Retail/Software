using System;

namespace Redbox.HAL.Component.Model;

public interface IKioskFunctionCheckData
{
	string VerticalSlotTestResult { get; }

	string InitTestResult { get; }

	string VendDoorTestResult { get; }

	string TrackTestResult { get; }

	string SnapDecodeTestResult { get; }

	string TouchscreenDriverTestResult { get; }

	string CameraDriverTestResult { get; }

	DateTime Timestamp { get; }

	string UserIdentifier { get; }
}
