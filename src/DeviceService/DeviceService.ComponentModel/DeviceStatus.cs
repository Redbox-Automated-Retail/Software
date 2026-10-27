using System;

namespace DeviceService.ComponentModel;

public class DeviceStatus
{
	public long Id { get; set; }

	public string Serial { get; set; }

	public string MfgSerial { get; set; }

	public DateTime LocalTime { get; set; }

	public string Assembly { get; set; }

	public string RBA { get; set; }

	public string TgzVersion { get; set; }

	public int RevisionNumber { get; set; }

	public string EMVContactVersion { get; set; }

	public string EMVClessVersion { get; set; }

	public bool SupportsVas { get; set; }

	public string InjectedKeys { get; set; }

	public ErrorState ErrorState { get; set; }
}
