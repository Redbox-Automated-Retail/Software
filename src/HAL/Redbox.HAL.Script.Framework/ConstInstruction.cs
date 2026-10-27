using System;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

[Serializable]
public sealed class ConstInstruction : Instruction
{
	public override string Mnemonic => "CONST";

	internal override bool CreatesVariableDef => true;

	protected override void ParseOperands(TokenList operandTokens, ExecutionResult result)
	{
		if (operandTokens.Count != 2)
		{
			result.AddInvalidOperandError($"The {Mnemonic} expects a symbol operand with a value.");
		}
		else if (operandTokens[0].Type != TokenType.ConstSymbol)
		{
			result.AddError(ExecutionErrors.InvalidOperand, "Expected first operand to be a symbol operand.");
		}
		else if (operandTokens[1].Type != TokenType.StringLiteral && operandTokens[1].Type != TokenType.NumericLiteral)
		{
			result.AddError(ExecutionErrors.InvalidOperand, "Expected second operand to be a string literal or numeric literal.");
		}
	}

	protected override void OnExecute(ExecutionResult result, ExecutionContext context)
	{
		Token token = base.Operands[0];
		Token token2 = base.Operands[1];
		new ErrorList();
		_ = token2.Value;
		if (token.Type == TokenType.ConstSymbol)
		{
			object symbolValue = context.GetSymbolValue(token.Value, result.Errors);
			if (symbolValue != null)
			{
				string text = "Attempt to redefine CONST " + token.Value;
				AddError("E002", text + "; not allowing update.", result.Errors);
			}
			else
			{
				object value = token2.ConvertValue();
				context.SetSymbolValue(token.Value, value);
			}
		}
	}
}
