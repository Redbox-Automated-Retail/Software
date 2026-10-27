using System;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

[Serializable]
public sealed class MessageInstruction : Instruction
{
	public override string Mnemonic => "MSG";

	protected override void ParseOperands(TokenList operandTokens, ExecutionResult result)
	{
		if (operandTokens.Count < 1)
		{
			result.AddError(ExecutionErrors.InvalidOperand, "Expected a string literal and any number of optional subtitution parameters.");
		}
		else if (operandTokens[0].Type != TokenType.StringLiteral && operandTokens[0].Type != TokenType.ConstSymbol)
		{
			result.AddError(ExecutionErrors.InvalidOperand, "Expected a string literal and any number of optional subtitution parameters.");
		}
	}

	protected override void OnExecute(ExecutionResult result, ExecutionContext context)
	{
		if (base.Operands.Count > 0)
		{
			object[] args = ConvertSymbolsToParameterArray(result.Errors, context);
			if (!result.Errors.ContainsError())
			{
				string message = string.Format((base.Operands[0].Type == TokenType.StringLiteral) ? base.Operands[0].Value : ((string)context.GetSymbolValue(base.Operands[0].Value, result.Errors)), args);
				context.Send(message);
			}
		}
	}
}
