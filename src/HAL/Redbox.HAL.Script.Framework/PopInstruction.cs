using System;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

[Serializable]
public sealed class PopInstruction : Instruction
{
	public override string Mnemonic => "POP";

	internal override bool CreatesVariableDef => true;

	protected override ExpectedOperands Expected => ExpectedOperands.Optional;

	protected override void ParseOperands(TokenList operandTokens, ExecutionResult result)
	{
		if (operandTokens.Count == 0 || operandTokens.Count > 2)
		{
			result.AddError(ExecutionErrors.InvalidOperand, "Expected one symbol to receive top of stack, with an optional DEPTH=n key value pair.");
		}
		else if (operandTokens.Count >= 1 && !operandTokens[0].IsSymbolOrConst && operandTokens[0].Type != TokenType.NumericLiteral)
		{
			result.AddError(ExecutionErrors.InvalidOperand, "Expected no operand or a symbol operand, with an optional DEPTH=n key value pair.");
		}
	}

	protected override void OnExecute(ExecutionResult result, ExecutionContext context)
	{
		object value = null;
		int? num = GetKeyValuePairValue<int?>(base.Operands.GetKeyValuePair("DEPTH"), result.Errors, context);
		if (result.Errors.Count > 0)
		{
			return;
		}
		if (!num.HasValue)
		{
			num = 1;
		}
		if (context.StackDepth >= num)
		{
			for (int i = 0; i < num; i++)
			{
				value = context.PopTop();
			}
			if (base.Operands.Count != 0)
			{
				Token token = base.Operands[0];
				if (token.Type == TokenType.Symbol)
				{
					context.SetSymbolValue(token.Value, value);
				}
			}
		}
		else
		{
			AddError("E002", $"The specified DEPTH of {num} exceeds the current depth of the stack: {context.StackDepth}.", result.Errors);
		}
	}
}
