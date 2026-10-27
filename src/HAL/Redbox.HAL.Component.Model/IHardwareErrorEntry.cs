using System;

namespace Redbox.HAL.Component.Model;

public interface IHardwareErrorEntry
{
	ErrorCodes Error { get; }

	DateTime Timestamp { get; }

	ILocation Location { get; }

	string ProgramName { get; }

	string ProgramId { get; }
}
