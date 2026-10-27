using System;

namespace Redbox.HAL.Script.Framework;

[Serializable]
public sealed class EndIfInstruction : Instruction
{
	public override bool EndsContext => true;

	public override string Mnemonic => "ENDIF";

	internal override Branch BranchType => Branch.Conditional;

	protected override ExpectedOperands Expected => ExpectedOperands.None;
}
