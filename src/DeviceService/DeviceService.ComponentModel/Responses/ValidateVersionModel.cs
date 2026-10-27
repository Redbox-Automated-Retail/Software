using System;

namespace DeviceService.ComponentModel.Responses;

public class ValidateVersionModel
{
	public bool IsCompatible { get; set; }

	public Version DeviceServiceVersion { get; set; }
}
