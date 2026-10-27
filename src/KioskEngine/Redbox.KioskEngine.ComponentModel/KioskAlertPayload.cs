using System;

namespace Redbox.KioskEngine.ComponentModel;

public class KioskAlertPayload
{
	public Guid MessageId = Guid.NewGuid();

	public string MessageType => "KioskAlert";

	public int KioskId { get; set; }

	public string EngineVersion { get; set; }

	public string BundleVersion { get; set; }

	public string SubType { get; set; }

	public string Detail { get; set; }

	public string Type { get; set; }

	public DateTime Time { get; set; }
}
