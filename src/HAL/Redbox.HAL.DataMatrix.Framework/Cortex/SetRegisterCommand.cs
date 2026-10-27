namespace Redbox.HAL.DataMatrix.Framework.Cortex;

internal sealed class SetRegisterCommand : TextCommand
{
	protected override string Prefix => "C";

	protected override string Data => $"({Register.Register}){Register.Value}";

	internal string Name { get; private set; }

	internal CortexRegister Register { get; private set; }

	internal SetRegisterCommand(CortexRegister register, string name)
	{
		Register = register;
		Name = name;
	}
}
