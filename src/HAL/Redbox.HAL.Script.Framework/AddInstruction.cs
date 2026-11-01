using System;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

[Serializable]
public sealed class AddInstruction : Instruction
{
	public override string Mnemonic => "ADD";

	protected override void ParseOperands(TokenList operandTokens, ExecutionResult result)
	{
		if (operandTokens.Count != 2)
		{
			result.AddInvalidOperandError("Expected a symbol as target of the addition and either a numeric literal or symbol as the addend.");
		}
		else if (operandTokens[0].Type != TokenType.Symbol)
		{
			result.AddInvalidOperandError("Expected a (non-CONST) symbol as target of the addition operation.");
		}
		else if (operandTokens[1].Type != TokenType.NumericLiteral && operandTokens[1].Type != TokenType.Symbol && operandTokens[1].Type != TokenType.ConstSymbol)
		{
			result.AddInvalidOperandError("Expected either a numeric literal or (CONST) symbol as the addend.");
		}
	}

	protected override void OnExecute(ExecutionResult result, ExecutionContext context)
	{
		Token target = base.Operands[0];
		object symbolValue = context.GetSymbolValue(target.Value, result.Errors);
		if (symbolValue == null || result.Errors.Count > 0)
		{
			return;
		}
		object operand = 0;
		Token operandToken = base.Operands[1];
		if (operandToken.Type == TokenType.NumericLiteral)
		{
			operand = operandToken.ConvertValue();
		}
		else if (operandToken.Type == TokenType.Symbol || operandToken.Type == TokenType.ConstSymbol)
		{
			operand = context.GetSymbolValue(operandToken.Value, result.Errors);
			if (operand == null || result.Errors.Count > 0)
			{
				return;
			}
		}
		Type type = symbolValue.GetType();
		if (type != typeof(int) && type != typeof(decimal))
		{
			AddError("E002", "Only numeric values of type integer and decimal may be added.", result.Errors);
			return;
		}
		if (type == typeof(int))
		{
			symbolValue = (int)decimal.Add(Convert.ToDecimal(symbolValue), Convert.ToDecimal(operand));
		}
		else if (type == typeof(decimal))
		{
			symbolValue = decimal.Add((decimal)symbolValue, (decimal)operand);
		}
		context.SetSymbolValue(target.Value, symbolValue);
	}
}
