using System;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

[Serializable]
public sealed class TransferInstruction : Instruction
{
	private const string SourceDeck = "SRC-DECK";

	private const string SourceSlot = "SRC-SLOT";

	private const string DestDeck = "DEST-DECK";

	private const string DestSlot = "DEST-SLOT";

	public override string Mnemonic => "XFER";

	protected override void ParseOperands(TokenList operandTokens, ExecutionResult result)
	{
		if (operandTokens.Count != 4)
		{
			result.AddInvalidOperandError("Expected key value pair SRC-DECK=y, SRC-SLOT=x, DEST-DECK=y, DEST-SLOT=x. The values of x and y may be numeric literals or symbols.");
		}
	}

	protected override void OnExecute(ExecutionResult result, ExecutionContext context)
	{
		IPickerSensorReadResult pickerSensorReadResult = ServiceLocator.Instance.GetService<IControlSystem>().ReadPickerSensors();
		if (!pickerSensorReadResult.Success)
		{
			context.PushTop(ErrorCodes.SensorReadError.ToString().ToUpper());
			return;
		}
		if (pickerSensorReadResult.IsFull)
		{
			context.PushTop("PICKERFULL");
			LogHelper.Instance.Log("XFER: the picker is full - bail.");
			return;
		}
		IInventoryService inventoryService = ServiceLocator.Instance.GetService<IInventoryService>();
		KeyValuePair sourceDeckPair = base.Operands.GetKeyValuePair("SRC-DECK");
		int? sourceDeckValue = GetKeyValuePairValue<int?>(sourceDeckPair, result.Errors, context);
		if (!sourceDeckValue.HasValue)
		{
			AddError("E004", "Source deck or source slot are undefined, reissue the XFER instruction specifying both source deck and slot values.", result.Errors);
			return;
		}
		int sourceDeck = sourceDeckValue.Value;
		KeyValuePair sourceSlotPair = base.Operands.GetKeyValuePair("SRC-SLOT");
		int? sourceSlotValue = GetKeyValuePairValue<int?>(sourceSlotPair, result.Errors, context);
		if (!sourceSlotValue.HasValue)
		{
			AddError("E004", "Source deck or source slot are undefined, reissue the XFER instruction specifying both source deck and slot values.", result.Errors);
			return;
		}
		int sourceSlot = sourceSlotValue.Value;
		KeyValuePair destDeckPair = base.Operands.GetKeyValuePair("DEST-DECK");
		int? destDeck = GetKeyValuePairValue<int?>(destDeckPair, result.Errors, context);
		KeyValuePair destSlotPair = base.Operands.GetKeyValuePair("DEST-SLOT");
		int? destSlot = GetKeyValuePairValue<int?>(destSlotPair, result.Errors, context);
		if (result.Errors.Count <= 0)
		{
			if (!destDeck.HasValue || !destSlot.HasValue)
			{
				AddError("E004", "Expected key value pair SRC-DECK=y, SRC-SLOT=x, DEST-DECK=y, DEST-SLOT=x. The values of x and y may be numeric literals or symbols.", result.Errors);
				return;
			}
			ILocation source = inventoryService.Get(sourceDeck, sourceSlot);
			ILocation destination = inventoryService.Get(destDeck.Value, destSlot.Value);
			LogHelper.Instance.Log("XFER: {0} to {1}", source.ToString(), destination.ToString());
			_ = context.AppLog;
			ITransferResult transferResult = ServiceLocator.Instance.GetService<IControllerService>().Transfer(source, destination, preserveFlags: false);
			context.PushTop(transferResult.Transferred ? base.SuccessMessage : transferResult.TransferError.ToString().ToUpper());
		}
	}
}
