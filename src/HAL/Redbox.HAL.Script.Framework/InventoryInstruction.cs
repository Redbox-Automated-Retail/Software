using System;
using System.Collections.Generic;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Extensions;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

[Serializable]
public sealed class InventoryInstruction : Instruction
{
	private enum InventoryOperands
	{
		None,
		Deck,
		Slot,
		SetUnknown,
		SetEmpty,
		GetReturnTime,
		SoftSync,
		IsDuplicate,
		ClearAll,
		EmptyCount
	}

	public override string Mnemonic => "INVENTORY";

	protected override void ParseOperands(TokenList operandTokens, ExecutionResult result)
	{
		if (Enum<InventoryOperands>.ParseIgnoringCase(operandTokens[0].Value, InventoryOperands.None) == InventoryOperands.None && !operandTokens[0].Value.Equals("MARK-EMPTY-UNKNOWN", StringComparison.CurrentCultureIgnoreCase))
		{
			result.Errors.Add(Error.NewError("C001", "Unexpected form", $"The operand {operandTokens[0].Value} for INVENTORY isn't recognized."));
		}
	}

	protected override void OnExecute(ExecutionResult result, ExecutionContext context)
	{
		InventoryOperands inventoryOperands = Enum<InventoryOperands>.ParseIgnoringCase(base.Operands[0].Value, InventoryOperands.None);
		IInventoryService inventoryService = ServiceLocator.Instance.GetService<IInventoryService>();
		IDecksService service = ServiceLocator.Instance.GetService<IDecksService>();
		ILocation location;
		if (inventoryOperands == InventoryOperands.None && base.Operands[0].Value.Equals("MARK-EMPTY-UNKNOWN", StringComparison.CurrentCultureIgnoreCase))
		{
			int? deckOperand = GetDeckOperand(result.Errors, context);
			if (!deckOperand.HasValue)
			{
				context.PushTop("ERROR");
			}
			IDeck byNumber = service.GetByNumber(deckOperand.Value);
			Range slotRange = new Range(1, byNumber.NumberOfSlots);
			List<int> list = inventoryService.SwapEmptyWith(byNumber, "UNKNOWN", MerchFlags.None, slotRange);
			context.PushTop(list);
			LogHelper.Instance.Log("Inventory: reset {0} empty slots on deck {1} to UNKNOWN", list, deckOperand.Value);
		}
		else if (InventoryOperands.IsDuplicate == inventoryOperands)
		{
			KeyValuePair keyValuePair = base.Operands.GetKeyValuePair("ID");
			string keyValuePairValue = GetKeyValuePairValue<string>(keyValuePair, result.Errors, context);
			if (result.Errors.Count > 0)
			{
				context.PushTop("FAILURE");
			}
			else
			{
				context.PushTop(inventoryService.IsBarcodeDuplicate(keyValuePairValue, out var _));
			}
		}
		else if (InventoryOperands.ClearAll == inventoryOperands)
		{
			service.ForAllDecksDo(delegate(IDeck _d0)
			{
				inventoryService.MarkDeckInventory(_d0, "EMPTY");
				return true;
			});
		}
		else if (InventoryOperands.EmptyCount == inventoryOperands)
		{
			context.PushTop(inventoryService.GetMachineEmptyCount());
		}
		else if (!GetLocation(result, context, out location))
		{
			AddError("E004", "Deck or slot are undefined, reissue the INVENTORY instruction specifying both deck and slot values.", result.Errors);
			context.PushTop("ERROR");
		}
		else if (InventoryOperands.GetReturnTime == inventoryOperands)
		{
			DateTime? returnDate = location.ReturnDate;
			context.PushTop((!returnDate.HasValue) ? "NONE" : returnDate.Value.ToString());
		}
		else if (InventoryOperands.SetUnknown == inventoryOperands)
		{
			location.ID = "UNKNOWN";
			inventoryService.Save(location);
			context.PushTop(base.SuccessMessage);
		}
		else if (InventoryOperands.SetEmpty == inventoryOperands)
		{
			inventoryService.Reset(location);
			context.PushTop(base.SuccessMessage);
		}
		else
		{
			result.Errors.Add(Error.NewError("E999", "Invalid operand.", $"The operand {base.Operands[0].Value} is invalid."));
		}
	}
}
