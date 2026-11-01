using System;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

[Serializable]
public sealed class WaitInstruction : Instruction
{
	public override string Mnemonic => "WAIT";

	protected override void ParseOperands(TokenList operandTokens, ExecutionResult result)
	{
		if (operandTokens.Count != 1)
		{
			result.AddMissingOperandError("The WAIT mnemonic requires a single numeric literal or symbol.");
		}
		else if (operandTokens[0].Type != TokenType.NumericLiteral && operandTokens[0].Type != TokenType.Symbol)
		{
			result.AddInvalidOperandError("Expected a numeric literal or symbol.");
		}
	}

	protected override void OnExecute(ExecutionResult result, ExecutionContext context)
	{
		int ms = 0;
		object duration = base.Operands[0].Value;
		if (base.Operands[0].Type == TokenType.Symbol)
		{
			duration = context.GetSymbolValue(base.Operands[0].Value, result.Errors);
			if (duration == null)
			{
				return;
			}
		}
		if (int.TryParse(duration.ToString(), out var parsed))
		{
			ms = parsed;
		}
		ServiceLocator.Instance.GetService<IRuntimeService>().Wait(ms);
	}
}
