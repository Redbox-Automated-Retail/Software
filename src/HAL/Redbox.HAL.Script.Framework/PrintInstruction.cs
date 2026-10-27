using System;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

[Serializable]
public sealed class PrintInstruction : Instruction
{
	public override string Mnemonic => "PRINT";

	protected override ExpectedOperands Expected => ExpectedOperands.Optional;

	protected override void ParseOperands(TokenList operandTokens, ExecutionResult result)
	{
		if (operandTokens[0].Type != TokenType.StringLiteral)
		{
			result.AddError(ExecutionErrors.InvalidOperand, "Expected a string literal and any number of optional subtitution parameters.");
		}
	}

	protected override void OnExecute(ExecutionResult result, ExecutionContext context)
	{
		if (base.Operands.Count > 0)
		{
			object[] arg = ConvertSymbolsToParameterArray(result.Errors, context);
			if (result.Errors.Count <= 0)
			{
				Console.WriteLine(base.Operands[0].Value, arg);
			}
		}
		else if (context.StackDepth > 0)
		{
			object obj = context.PopTop();
			if (obj != null)
			{
				Console.WriteLine(obj);
			}
		}
	}
}
