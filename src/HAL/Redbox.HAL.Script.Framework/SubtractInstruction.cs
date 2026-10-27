using System;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

[Serializable]
public sealed class SubtractInstruction : Instruction
{
	public override string Mnemonic => "SUB";

	protected override void ParseOperands(TokenList operandTokens, ExecutionResult result)
	{
		if (operandTokens.Count != 2)
		{
			result.AddInvalidOperandError("Expectd a symbol as target of the subtraction and either a numeric literal or symbol as the subtrand.");
		}
		else if (operandTokens[0].Type != TokenType.Symbol)
		{
			result.AddInvalidOperandError("Expected a (non-CONST) symbol as target of the subtraction operation.");
		}
		else if (operandTokens[1].Type != TokenType.NumericLiteral && operandTokens[1].Type != TokenType.Symbol && operandTokens[1].Type != TokenType.ConstSymbol)
		{
			result.AddInvalidOperandError("Expectd either a numeric literal or symbol as the subtrand.");
		}
	}

	protected override void OnExecute(ExecutionResult result, ExecutionContext context)
	{
		Token token = base.Operands[0];
		object obj = context.GetSymbolValue(token.Value, result.Errors);
		if (obj == null || result.Errors.Count > 0)
		{
			return;
		}
		object obj2 = 0;
		Token token2 = base.Operands[1];
		if (token2.Type == TokenType.NumericLiteral)
		{
			obj2 = token2.ConvertValue();
		}
		else if (token2.Type == TokenType.Symbol || token2.Type == TokenType.ConstSymbol)
		{
			obj2 = context.GetSymbolValue(token2.Value, result.Errors);
			if (obj2 == null || result.Errors.Count > 0)
			{
				return;
			}
		}
		Type type = obj.GetType();
		if (type != typeof(int) && type != typeof(decimal))
		{
			AddError("E004", "Only numeric values of type integer and decimal may be subtracted.", result.Errors);
			return;
		}
		if (type == typeof(int))
		{
			obj = (int)decimal.Subtract(Convert.ToDecimal(obj), Convert.ToDecimal(obj2));
		}
		else if (type == typeof(decimal))
		{
			obj = decimal.Subtract((decimal)obj, (decimal)obj2);
		}
		context.SetSymbolValue(token.Value, obj);
	}
}
