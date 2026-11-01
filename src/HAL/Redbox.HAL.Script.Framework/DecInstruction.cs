using System;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

[Serializable]
public sealed class DecInstruction : Instruction
{
	public override string Mnemonic => "DEC";

	protected override void ParseOperands(TokenList operandTokens, ExecutionResult result)
	{
		if (operandTokens.Count != 1)
		{
			result.AddInvalidOperandError($"The {Mnemonic} instruction expects one operand.");
		}
		else if (operandTokens[0].Type == TokenType.ConstSymbol)
		{
			result.AddError(ExecutionErrors.InvalidOperand, "Illegal attempt to " + Mnemonic + " a CONST symbol.");
		}
	}

	protected override void OnExecute(ExecutionResult result, ExecutionContext context)
	{
		Token token = base.Operands[0];
		if (token.Type != TokenType.Symbol)
		{
			return;
		}
		object symbolValue = context.GetSymbolValue(token.Value, result.Errors);
		if (symbolValue == null || result.Errors.Count > 0)
		{
			return;
		}
		Type type = symbolValue.GetType();
		if (type != typeof(int) && type != typeof(decimal))
		{
			AddError("E002", "Only numeric values of type integer and decimal may be decremented.", result.Errors);
			return;
		}
		if (type == typeof(int))
		{
			symbolValue = (int)symbolValue - 1;
		}
		else if (type == typeof(decimal))
		{
			symbolValue = (decimal)symbolValue - 1m;
		}
		context.SetSymbolValue(token.Value, Convert.ChangeType(symbolValue, type));
	}
}
