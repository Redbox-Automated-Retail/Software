using System;

namespace Redbox.KioskEngine.ComponentModel;

public class AuditEventPayload
{
	public Guid MessageId = Guid.NewGuid();

	public string MessageType => "AuditEvent";

	public int KioskId { get; set; }

	public string EngineVersion { get; set; }

	public string BundleVersion { get; set; }

	public string EventType { get; set; }

	public DateTime LogDate { get; set; }

	public string UserName { get; set; }

	public string Message { get; set; }

	public string Source { get; set; }
}
