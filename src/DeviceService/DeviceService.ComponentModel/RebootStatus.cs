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
