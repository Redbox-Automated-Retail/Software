using System;

namespace Redbox.KioskEngine.ComponentModel;

public class AWSKioskFunctionCheckPayload
{
	public int KioskId { get; set; }

	public DateTime ReportTime { get; set; }

	public string UserName { get; set; }

	public string InitTest { get; set; }

	public string TrackTest { get; set; }

	public string VendDoor { get; set; }

	public string VerticalSlot { get; set; }

	public string CameraDriver { get; set; }

	public string SnapDecode { get; set; }

	public string TouchScreenDriver { get; set; }
}
