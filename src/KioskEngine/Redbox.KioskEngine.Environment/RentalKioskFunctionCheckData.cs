using System;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.KioskEngine.Environment;

internal class RentalKioskFunctionCheckData : IKioskFunctionCheckData
{
	public string CameraDriverTestResult { get; set; }

	public string InitTestResult { get; set; }

	public string SnapDecodeTestResult { get; set; }

	public DateTime Timestamp { get; set; }

	public string TouchscreenDriverTestResult { get; set; }

	public string TrackTestResult { get; set; }

	public string UserIdentifier { get; set; }

	public string VendDoorTestResult { get; set; }

	public string VerticalSlotTestResult { get; set; }
}
