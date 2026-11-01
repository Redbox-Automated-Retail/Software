using System;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Extensions;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

[Serializable]
public sealed class QlmInstruction : Instruction
{
	private static class Commands
	{
		public const string Engage = "ENGAGE";

		public const string Disengage = "DISENGAGE";

		public const string Halt = "HALT";

		public const string Status = "STATUS";

		public const string Lift = "LIFT";

		public const string Drop = "DROP";

		public const string EndQlmOp = "ENDQLMOP";

		public const string ClearPicker = "CLEAR-PICKER";

		public const string UnloadDisk = "UNLOAD-DISK";

		public const string Setup = "SETUP";

		public const string ThinDisk = "THIN-DISK";
	}

	public override string Mnemonic => "QLM";

	protected override void ParseOperands(TokenList operandTokens, ExecutionResult result)
	{
		if (operandTokens.Count == 1 || operandTokens.Count == 2)
		{
			if (operandTokens[0].Type != TokenType.Symbol || (string.Compare(operandTokens[0].Value, "ENGAGE", ignoreCase: true) != 0 && string.Compare(operandTokens[0].Value, "DISENGAGE", ignoreCase: true) != 0 && string.Compare(operandTokens[0].Value, "LIFT", ignoreCase: true) != 0 && string.Compare(operandTokens[0].Value, "DROP", ignoreCase: true) != 0 && string.Compare(operandTokens[0].Value, "HALT", ignoreCase: true) != 0 && string.Compare(operandTokens[0].Value, "ENDQLMOP", ignoreCase: true) != 0 && string.Compare(operandTokens[0].Value, "CLEAR-PICKER", ignoreCase: true) != 0 && string.Compare(operandTokens[0].Value, "UNLOAD-DISK", ignoreCase: true) != 0 && string.Compare(operandTokens[0].Value, "THIN-DISK", ignoreCase: true) != 0 && string.Compare(operandTokens[0].Value, "SETUP", ignoreCase: true) != 0 && string.Compare(operandTokens[0].Value, "STATUS", ignoreCase: true) != 0))
			{
				result.AddError(ExecutionErrors.InvalidOperand, "Expected a symbol: ENGAGE, DISENGAGE, HALT, UP, DOWN or STATUS.");
			}
			else
			{
				base.Operands.AddRange(operandTokens);
			}
		}
		else
		{
			result.AddError(ExecutionErrors.InvalidOperand, "Expected a symbol: ENGAGE, DISENGAGE, UP, DOWN or HALT.");
		}
	}

	protected override void OnExecute(ExecutionResult result, ExecutionContext context)
	{
		Token token = base.Operands[0];
		if (token.Type != TokenType.Symbol)
		{
			return;
		}
		IMotionControlService motionControlService = ServiceLocator.Instance.GetService<IMotionControlService>();
		bool home = true;
		KeyValuePair homePair = base.Operands.GetKeyValuePair("HOME");
		if (homePair != null && bool.TryParse(homePair.Value, out var parsedHome))
		{
			home = parsedHome;
		}
		IControlSystem controlSystem = ServiceLocator.Instance.GetService<IControlSystem>();
		if (string.Compare(token.Value, "HALT", ignoreCase: true) == 0)
		{
			context.PushTop(controlSystem.HaltQlmLifter().Success ? base.SuccessMessage : base.TimeoutMessage);
		}
		else if (string.Compare(token.Value, "ENGAGE", ignoreCase: true) == 0)
		{
			ErrorCodes errorCodes = controlSystem.EngageQlm(home, context.AppLog);
			if (errorCodes == ErrorCodes.Success && !context.IsImmediate)
			{
				motionControlService.TestAndReset();
			}
			context.PushTop(errorCodes.ToString().ToUpper());
		}
		else if (string.Compare(token.Value, "LIFT", ignoreCase: true) == 0)
		{
			controlSystem.LiftQlm();
		}
		else if (string.Compare(token.Value, "DROP", ignoreCase: true) == 0)
		{
			controlSystem.DropQlm();
		}
		else if (string.Compare(token.Value, "DISENGAGE", ignoreCase: true) == 0)
		{
			ErrorCodes disengageResult = controlSystem.DisengageQlm(home, context.AppLog);
			if (disengageResult == ErrorCodes.Success && !context.IsImmediate)
			{
				motionControlService.TestAndReset();
			}
			context.PushTop(disengageResult.ToString().ToUpper());
		}
		else if (string.Compare(token.Value, "STATUS", ignoreCase: true) == 0)
		{
			context.PushTop(controlSystem.GetQlmStatus().ToString().ToUpper());
		}
		else if (string.Compare(token.Value, "ENDQLMOP", ignoreCase: true) == 0)
		{
			ErrorCodes failCode = ErrorCodes.Success;
			KeyValuePair moveErrorPair = base.Operands.GetKeyValuePair("MOVE-EC");
			if (moveErrorPair != null)
			{
				string keyValuePairValue = GetKeyValuePairValue<string>(moveErrorPair, result.Errors, context);
				if (result.Errors.Count == 0)
				{
					failCode = Enum<ErrorCodes>.ParseIgnoringCase(keyValuePairValue, ErrorCodes.Success);
				}
			}
			if (MerchandizingHelper.Instance.CleanupJob(result, context, failCode) && !context.IsImmediate)
			{
				motionControlService.TestAndReset();
			}
		}
		else if (string.Compare(token.Value, "CLEAR-PICKER") == 0)
		{
			new UnloadHelper(context, result).CheckAndClearPicker(result);
		}
		else if (string.Compare(token.Value, "SETUP") == 0)
		{
			KeyValuePair checkSlotsPair = base.Operands.GetKeyValuePair("CHECK-SLOTS");
			bool checkSlots = true;
			if (checkSlotsPair != null && bool.TryParse(checkSlotsPair.Value, out var parsedCheckSlots))
			{
				checkSlots = parsedCheckSlots;
			}
			context.PushTop(MerchandizingHelper.Instance.Setup(result, checkSlots, context));
		}
		else if (string.Compare(token.Value, "UNLOAD-DISK") == 0)
		{
			IInventoryService inventoryService = ServiceLocator.Instance.GetService<IInventoryService>();
			int slot = CurrentSlot(result, context);
			if (slot != 0)
			{
				if (!inventoryService.EmptyCountExceeds(0))
				{
					context.PushTop(MerchandizeResult.MachineFull.ToString().ToUpper());
				}
				IDecksService decksService = ServiceLocator.Instance.GetService<IDecksService>();
				MerchandizeResult merchandizeResult = new UnloadHelper(context, result).UnloadDisk(decksService.QlmDeck.Number, slot);
				string status = merchandizeResult.ToString().ToUpper();
				if (merchandizeResult != MerchandizeResult.Success)
				{
					LogHelper.Instance.WithContext("Unload-Disc returned status {0}", status);
				}
				context.PushTop(status);
			}
		}
		else
		{
			if (string.Compare(token.Value, "THIN-DISK") != 0)
			{
				return;
			}
			string iD = GetID(result, context);
			if (!string.IsNullOrEmpty(iD))
			{
				MerchandizeResult thinResult = ThinHelper.MakeInstance(result, context).ThinDisk(iD, MerchFlags.None);
				string thinStatus = thinResult.ToString().ToUpper();
				if (thinResult != MerchandizeResult.Success)
				{
					LogHelper.Instance.WithContext("thin-disk returned status {0}", thinStatus);
				}
				context.PushTop(thinStatus);
			}
		}
	}

	private int CurrentSlot(ExecutionResult result, ExecutionContext context)
	{
		KeyValuePair keyValuePair = base.Operands.GetKeyValuePair("CURRENT-SLOT");
		if (keyValuePair == null)
		{
			result.Errors.Add(Error.NewError("E099", "Missing operand.", "The CURRENT-SLOT operand is missing."));
			return 0;
		}
		int keyValuePairValue = GetKeyValuePairValue<int>(keyValuePair, result.Errors, context);
		if (result.Errors.Count != 0)
		{
			return 0;
		}
		return keyValuePairValue;
	}
}
