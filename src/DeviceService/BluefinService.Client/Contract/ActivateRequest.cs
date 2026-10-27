using System;

namespace BluefinService.Client.Contract;

public class ActivateRequest
{
	public long KioskId { get; set; }

	public string ReaderSerialNumber { get; set; }

	public string InjectedSerialNumber { get; set; }

	public DateTime LocalDateTime { get; set; }
}
