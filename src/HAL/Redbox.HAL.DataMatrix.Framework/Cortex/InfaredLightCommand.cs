namespace Redbox.HAL.DataMatrix.Framework.Cortex;

internal sealed class InfaredLightCommand : TextCommand
{
	protected override string Prefix => "Q";

	protected override string Data => "(1f5)1";
}
