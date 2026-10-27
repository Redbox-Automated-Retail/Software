using System;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

[Serializable]
public sealed class LookupInstruction : Instruction
{
	public override string Mnemonic => "LOOKUP";

	protected override ExpectedOperands Expected => ExpectedOperands.Optional;

	protected override void ParseOperands(TokenList operandTokens, ExecutionResult result)
	{
		if (operandTokens.Count != 1 || (operandTokens[0].Type != TokenType.StringLiteral && operandTokens[0].Type != TokenType.Symbol))
		{
			result.AddError(ExecutionErrors.InvalidOperand, "Expected a string literal or symbol.");
		}
	}

	protected override void OnExecute(ExecutionResult result, ExecutionContext context)
	{
		string text = null;
		IInventoryService service = ServiceLocator.Instance.GetService<IInventoryService>();
		if (base.Operands.Count == 1)
		{
			Token token = base.Operands[0];
			if (token.Type == TokenType.Symbol)
			{
				text = context.GetSymbolValue(token.Value, result.Errors) as string;
				if (result.Errors.Count > 0)
				{
					return;
				}
			}
			else if (token.Type == TokenType.StringLiteral)
			{
				text = token.Value;
			}
		}
		else if (context.StackDepth > 0)
		{
			text = context.PopTop() as string;
		}
		if (text != null)
		{
			ILocation location = service.Lookup(text);
			if (location == null)
			{
				context.PushTop("NOT FOUND");
				return;
			}
			context.PushTop(location.Slot);
			context.PushTop(location.Deck);
			context.PushTop("FOUND");
		}
		else
		{
			AddError("E004", "LOOKUP expects a valid string on the stack, symbol, or string literal representing the ID to find.", result.Errors);
		}
	}
}
