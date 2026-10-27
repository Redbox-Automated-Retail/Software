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
		IInventoryService service = ServiceLocator.Instance.GetService<IInventoryService>();
		KeyValuePair keyValuePair = base.Operands.GetKeyValuePair("SRC-DECK");
		int? keyValuePairValue = GetKeyValuePairValue<int?>(keyValuePair, result.Errors, context);
		if (!keyValuePairValue.HasValue)
		{
			AddError("E004", "Source deck or source slot are undefined, reissue the XFER instruction specifying both source deck and slot values.", result.Errors);
			return;
		}
		int value = keyValuePairValue.Value;
		KeyValuePair keyValuePair2 = base.Operands.GetKeyValuePair("SRC-SLOT");
		int? keyValuePairValue2 = GetKeyValuePairValue<int?>(keyValuePair2, result.Errors, context);
		if (!keyValuePairValue2.HasValue)
		{
			AddError("E004", "Source deck or source slot are undefined, reissue the XFER instruction specifying both source deck and slot values.", result.Errors);
			return;
		}
		int value2 = keyValuePairValue2.Value;
		KeyValuePair keyValuePair3 = base.Operands.GetKeyValuePair("DEST-DECK");
		int? keyValuePairValue3 = GetKeyValuePairValue<int?>(keyValuePair3, result.Errors, context);
		KeyValuePair keyValuePair4 = base.Operands.GetKeyValuePair("DEST-SLOT");
		int? keyValuePairValue4 = GetKeyValuePairValue<int?>(keyValuePair4, result.Errors, context);
		if (result.Errors.Count <= 0)
		{
			if (!keyValuePairValue3.HasValue || !keyValuePairValue4.HasValue)
			{
				AddError("E004", "Expected key value pair SRC-DECK=y, SRC-SLOT=x, DEST-DECK=y, DEST-SLOT=x. The values of x and y may be numeric literals or symbols.", result.Errors);
				return;
			}
			ILocation location = service.Get(value, value2);
			ILocation location2 = service.Get(keyValuePairValue3.Value, keyValuePairValue4.Value);
			LogHelper.Instance.Log("XFER: {0} to {1}", location.ToString(), location2.ToString());
			_ = context.AppLog;
			ITransferResult transferResult = ServiceLocator.Instance.GetService<IControllerService>().Transfer(location, location2, preserveFlags: false);
			context.PushTop(transferResult.Transferred ? base.SuccessMessage : transferResult.TransferError.ToString().ToUpper());
		}
	}
}
