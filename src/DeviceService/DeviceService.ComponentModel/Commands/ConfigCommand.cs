using System;

namespace DeviceService.ComponentModel.Commands;

public class ConfigCommand : BaseCommandRequest
{
	public string GroupNumber { get; set; }

	public string IndexNumber { get; set; }

	public string Value { get; set; }

	public ConfigCommand(string commandName, Version deviceServiceClientVersion)
		: base(commandName, deviceServiceClientVersion)
	{
	}
}
