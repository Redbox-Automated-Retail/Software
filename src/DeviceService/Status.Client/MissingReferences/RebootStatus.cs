// From DeviceService.ComponentModel 1.10.6.0 (update-store history): Status.Client was built against an
// older DeviceService.ComponentModel that still had this type; the shipped 1.10.13.0 does not.
using System;

namespace DeviceService.ComponentModel;

public class RebootStatus
{
	public string Id { get; set; }

	public RebootType Type { get; set; }

	public DateTime? DisconnectTime { get; set; }

	public DateTime? ConnectTime => DateTime.Now;

	public DateTime? ExpectedTime { get; set; }

	public DateTime? DeviceTime { get; set; }

	public long KioskId { get; set; }
}
