using System;

namespace Redbox.HAL.Script.Framework;

[Serializable]
public sealed class NopInstruction : Instruction
{
	public override string Mnemonic => "NOP";

	protected override ExpectedOperands Expected => ExpectedOperands.None;
}
