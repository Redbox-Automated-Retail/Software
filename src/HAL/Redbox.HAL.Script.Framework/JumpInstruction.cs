using System;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

[Serializable]
public sealed class JumpInstruction : Instruction
{
	public override string Mnemonic => "JUMP";

	internal override Branch BranchType => Branch.Unconditional;

	internal override string TargetLabel => base.Operands[0].Value;

	protected override void ParseOperands(TokenList operandTokens, ExecutionResult result)
	{
		if (operandTokens.Count != 1 || operandTokens[0].Type != TokenType.Symbol)
		{
			result.AddInvalidOperandError($"The {Mnemonic} instruction expects a symbol that equates to a label name.");
		}
	}

	protected override void OnExecute(ExecutionResult result, ExecutionContext context)
	{
		Token token = base.Operands[0];
		if (token.Type == TokenType.Symbol && !context.Registers.JumpToLabel(token.Value))
		{
			AddError("E002", "The label " + token.Value + " does not exist.", result.Errors);
		}
	}
}
