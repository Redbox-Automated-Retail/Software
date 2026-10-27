using System;

namespace Redbox.HAL.Component.Model;

public interface IDriverDescriptor
{
	Version DriverVersion { get; }

	string Provider { get; }
}
