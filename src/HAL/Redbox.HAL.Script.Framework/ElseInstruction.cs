using System;

namespace Redbox.HAL.Script.Framework;

[Serializable]
public sealed class ElseInstruction : Instruction
{
	public override string Mnemonic => "ELSE";

	protected override ExpectedOperands Expected => ExpectedOperands.None;

	internal override Branch BranchType => Branch.Conditional;

	internal override bool ValidateConditionalBranch(Instruction target, ExecutionResult result)
	{
		if (target.Mnemonic.Equals("ENDIF"))
		{
			return true;
		}
		LogIncorrectBranch(target.Mnemonic, result);
		return false;
	}
}
