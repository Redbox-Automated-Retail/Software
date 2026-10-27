using System;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

[Serializable]
public sealed class IncInstruction : Instruction
{
	public override string Mnemonic => "INC";

	protected override void ParseOperands(TokenList operandTokens, ExecutionResult result)
	{
		if (operandTokens.Count != 1)
		{
			result.AddError(ExecutionErrors.InvalidOperand, "Expectd a symbol to increment.");
		}
		else
		{
			CheckAssignment(operandTokens[0].Type, result);
		}
	}

	protected override void OnExecute(ExecutionResult result, ExecutionContext context)
	{
		Token token = base.Operands[0];
		if (token.Type != TokenType.Symbol)
		{
			return;
		}
		object obj = context.GetSymbolValue(token.Value, result.Errors);
		if (obj == null || result.Errors.Count > 0)
		{
			return;
		}
		Type type = obj.GetType();
		if (type != typeof(int) && type != typeof(decimal))
		{
			AddError("E002", "Only numeric values of type integer and decimal may be incremented.", result.Errors);
			return;
		}
		if (type == typeof(int))
		{
			obj = (int)obj + 1;
		}
		else if (type == typeof(decimal))
		{
			obj = (decimal)obj + 1m;
		}
		context.SetSymbolValue(token.Value, Convert.ChangeType(obj, type));
	}
}
