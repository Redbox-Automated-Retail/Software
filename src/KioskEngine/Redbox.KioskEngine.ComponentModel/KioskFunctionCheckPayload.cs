using System;

namespace Redbox.KioskEngine.ComponentModel;

public class KioskFunctionCheckPayload
{
	public string MessageType = "KioskFunctionCheck";

	public Guid MessageId { get; set; }

	public int KioskId { get; set; }

	public string EngineVersion { get; set; }

	public string BundleVersion { get; set; }

	public DateTime ReportTime { get; set; }

	public string UserName { get; set; }

	public string VerticalSlot { get; set; }

	public string InitTest { get; set; }

	public string VendDoor { get; set; }

	public string TrackTest { get; set; }

	public string SnapDecode { get; set; }

	public string TouchScreenDriver { get; set; }

	public string CameraDriver { get; set; }
}
