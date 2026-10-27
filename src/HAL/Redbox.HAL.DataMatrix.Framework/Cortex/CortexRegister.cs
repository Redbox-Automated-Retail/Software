namespace Redbox.HAL.DataMatrix.Framework.Cortex;

internal sealed class CortexRegister
{
	internal string Register { get; private set; }

	internal string Value { get; private set; }

	internal CortexRegister(string r, string v)
	{
		Register = r;
		Value = v;
	}
}
