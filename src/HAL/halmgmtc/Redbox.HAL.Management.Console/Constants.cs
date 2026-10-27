namespace Redbox.HAL.Management.Console;

internal class Constants
{
	public enum LeftPanel
	{
		Explorer,
		Properties,
		Sensors
	}

	public enum ListView
	{
		Error,
		Events,
		Immediate,
		Job,
		Output,
		Results,
		Stack,
		Symbols
	}

	public static readonly string None = "( None )";

	public static readonly HardwareJobWrapper NullJob = new HardwareJobWrapper(null);
}
