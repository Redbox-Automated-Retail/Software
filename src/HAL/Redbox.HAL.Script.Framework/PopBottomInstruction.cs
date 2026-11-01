using System;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

[Serializable]
public sealed class PopBottomInstruction : Instruction
{
	public override string Mnemonic => "POPB";

	protected override void ParseOperands(TokenList operandTokens, ExecutionResult result)
	{
		if (operandTokens.Count == 0 || operandTokens.Count > 2)
		{
			result.AddInvalidOperandError("Expected one symbol to receive top of stack, with an optional DEPTH=n key value pair.");
		}
		else if (operandTokens[0].Type != TokenType.Symbol)
		{
			result.AddError(ExecutionErrors.InvalidOperand, "Expected no operand or a symbol operand, with an optional DEPTH=n key value pair.");
		}
	}

	protected override void OnExecute(ExecutionResult result, ExecutionContext context)
	{
		object value = null;
		int? depth = GetKeyValuePairValue<int?>(base.Operands.GetKeyValuePair("DEPTH"), result.Errors, context);
		if (result.Errors.Count > 0)
		{
			return;
		}
		if (!depth.HasValue)
		{
			depth = 1;
		}
		if (context.StackDepth >= depth)
		{
			for (int i = 0; i < depth; i++)
			{
				value = context.PopBottom();
			}
			Token token = base.Operands[0];
			if (token.Type == TokenType.Symbol)
			{
				context.SetSymbolValue(token.Value, value);
			}
		}
		else
		{
			AddError("E002", $"The specified DEPTH of {depth} exceeds the current depth of the stack: {context.StackDepth}.", result.Errors);
		}
	}
}
