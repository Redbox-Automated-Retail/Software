using System;

namespace DeviceService.ComponentModel.Responses;

public class CardReaderState
{
	public bool IsConnected { get; set; }

	public bool IsTampered { get; set; }

	public ConnectionState ConnectionState { get; set; }

	public bool SupportsEMV { get; set; }

	public bool SupportsVas { get; set; }

	public Version Version { get; set; }

	public ErrorState ErrorState { get; set; }
}
