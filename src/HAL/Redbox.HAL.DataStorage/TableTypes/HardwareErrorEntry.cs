using System;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.DataStorage.TableTypes;

internal sealed class HardwareErrorEntry : IHardwareErrorEntry
{
	public ErrorCodes Error { get; internal set; }

	public DateTime Timestamp { get; internal set; }

	public ILocation Location { get; internal set; }

	public string ProgramName { get; internal set; }

	public string ProgramId { get; internal set; }
}
