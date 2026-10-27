using System;

namespace Redbox.HAL.Script.Framework;

[Serializable]
public class ElseIfInstruction : IfInstruction
{
	public override string Mnemonic => "ELSEIF";

	public override bool BeginsContext => false;

	internal override Branch BranchType => Branch.Conditional;

	internal override bool ValidateConditionalBranch(Instruction target, ExecutionResult result)
	{
		if (target.Mnemonic.Equals("ELSEIF") || target.Mnemonic.Equals("ELSE") || target.Mnemonic.Equals("ENDIF"))
		{
			FalseBranchTarget = target.LineNumber;
			return true;
		}
		LogIncorrectBranch(target.Mnemonic, result);
		return false;
	}
}
