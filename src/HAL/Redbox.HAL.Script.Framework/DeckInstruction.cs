using System;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Extensions;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

[Serializable]
public sealed class DeckInstruction : Instruction
{
	private enum DeckOperands
	{
		None,
		MaxSlots,
		Reset
	}

	public override string Mnemonic => "DECK";

	protected override void ParseOperands(TokenList operandTokens, ExecutionResult result)
	{
		if (Enum<DeckOperands>.ParseIgnoringCase(operandTokens[0].Value, DeckOperands.None) == DeckOperands.None)
		{
			result.Errors.Add(Error.NewError("C001", "Unexpected form", $"The operand {operandTokens[0].Value} for DECK isn't recognized."));
		}
	}

	protected override void OnExecute(ExecutionResult result, ExecutionContext context)
	{
		DeckOperands deckOperands = Enum<DeckOperands>.ParseIgnoringCase(base.Operands[0].Value, DeckOperands.None);
		int? nullableNumericOperand = GetNullableNumericOperand(result, "NUMBER", context);
		if (!nullableNumericOperand.HasValue)
		{
			AddError("E777", "Missing 'NUMBER' operand to DECK.", result.Errors);
			return;
		}
		IDeck byNumber = ServiceLocator.Instance.GetService<IDecksService>().GetByNumber(nullableNumericOperand.Value);
		if (deckOperands == DeckOperands.MaxSlots)
		{
			context.PushTop(byNumber.NumberOfSlots);
		}
		else if (DeckOperands.Reset == deckOperands)
		{
			ServiceLocator.Instance.GetService<IInventoryService>().ResetAndMark(byNumber, "EMPTY");
		}
	}
}
