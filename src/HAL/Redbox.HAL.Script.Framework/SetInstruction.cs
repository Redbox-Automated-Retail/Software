using System;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

[Serializable]
public sealed class SetInstruction : Instruction
{
	public override string Mnemonic => "SET";

	internal override bool CreatesVariableDef => true;

	protected override void ParseOperands(TokenList operandTokens, ExecutionResult result)
	{
		if (operandTokens.Count != 2)
		{
			result.AddMissingOperandError("A symbol and one of: symbol, numeric literal, string literal or key value pair expected.");
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
		object obj = base.Operands[1].ConvertValue();
		if (IsSymbolic(base.Operands[1]) && base.Operands[1].Value.Equals(obj))
		{
			obj = context.GetSymbolValue(base.Operands[1].Value, result.Errors);
			if (result.Errors.Count > 0)
			{
				return;
			}
		}
		context.SetSymbolValue(token.Value, obj);
	}
}
