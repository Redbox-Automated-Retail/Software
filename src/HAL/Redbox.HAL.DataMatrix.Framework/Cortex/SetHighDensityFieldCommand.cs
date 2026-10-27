namespace Redbox.HAL.DataMatrix.Framework.Cortex;

internal sealed class SetHighDensityFieldCommand : TextCommand
{
	protected override string Prefix => "P";

	protected override string Data => "(AE)#640";
}
