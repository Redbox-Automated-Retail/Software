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
		IMotionControlService service = ServiceLocator.Instance.GetService<IMotionControlService>();
		bool home = true;
		KeyValuePair keyValuePair = base.Operands.GetKeyValuePair("HOME");
		if (keyValuePair != null && bool.TryParse(keyValuePair.Value, out var result2))
		{
			home = result2;
		}
		IControlSystem service2 = ServiceLocator.Instance.GetService<IControlSystem>();
		if (string.Compare(token.Value, "HALT", ignoreCase: true) == 0)
		{
			context.PushTop(service2.HaltQlmLifter().Success ? base.SuccessMessage : base.TimeoutMessage);
		}
		else if (string.Compare(token.Value, "ENGAGE", ignoreCase: true) == 0)
		{
			ErrorCodes errorCodes = service2.EngageQlm(home, context.AppLog);
			if (errorCodes == ErrorCodes.Success && !context.IsImmediate)
			{
				service.TestAndReset();
			}
			context.PushTop(errorCodes.ToString().ToUpper());
		}
		else if (string.Compare(token.Value, "LIFT", ignoreCase: true) == 0)
		{
			service2.LiftQlm();
		}
		else if (string.Compare(token.Value, "DROP", ignoreCase: true) == 0)
		{
			service2.DropQlm();
		}
		else if (string.Compare(token.Value, "DISENGAGE", ignoreCase: true) == 0)
		{
			ErrorCodes errorCodes2 = service2.DisengageQlm(home, context.AppLog);
			if (errorCodes2 == ErrorCodes.Success && !context.IsImmediate)
			{
				service.TestAndReset();
			}
			context.PushTop(errorCodes2.ToString().ToUpper());
		}
		else if (string.Compare(token.Value, "STATUS", ignoreCase: true) == 0)
		{
			context.PushTop(service2.GetQlmStatus().ToString().ToUpper());
		}
		else if (string.Compare(token.Value, "ENDQLMOP", ignoreCase: true) == 0)
		{
			ErrorCodes failCode = ErrorCodes.Success;
			KeyValuePair keyValuePair2 = base.Operands.GetKeyValuePair("MOVE-EC");
			if (keyValuePair2 != null)
			{
				string keyValuePairValue = GetKeyValuePairValue<string>(keyValuePair2, result.Errors, context);
				if (result.Errors.Count == 0)
				{
					failCode = Enum<ErrorCodes>.ParseIgnoringCase(keyValuePairValue, ErrorCodes.Success);
				}
			}
			if (MerchandizingHelper.Instance.CleanupJob(result, context, failCode) && !context.IsImmediate)
			{
				service.TestAndReset();
			}
		}
		else if (string.Compare(token.Value, "CLEAR-PICKER") == 0)
		{
			new UnloadHelper(context, result).CheckAndClearPicker(result);
		}
		else if (string.Compare(token.Value, "SETUP") == 0)
		{
			KeyValuePair keyValuePair3 = base.Operands.GetKeyValuePair("CHECK-SLOTS");
			bool checkSlots = true;
			if (keyValuePair3 != null && bool.TryParse(keyValuePair3.Value, out var result3))
			{
				checkSlots = result3;
			}
			context.PushTop(MerchandizingHelper.Instance.Setup(result, checkSlots, context));
		}
		else if (string.Compare(token.Value, "UNLOAD-DISK") == 0)
		{
			IInventoryService service3 = ServiceLocator.Instance.GetService<IInventoryService>();
			int num = CurrentSlot(result, context);
			if (num != 0)
			{
				if (!service3.EmptyCountExceeds(0))
				{
					context.PushTop(MerchandizeResult.MachineFull.ToString().ToUpper());
				}
				IDecksService service4 = ServiceLocator.Instance.GetService<IDecksService>();
				MerchandizeResult merchandizeResult = new UnloadHelper(context, result).UnloadDisk(service4.QlmDeck.Number, num);
				string text = merchandizeResult.ToString().ToUpper();
				if (merchandizeResult != MerchandizeResult.Success)
				{
					LogHelper.Instance.WithContext("Unload-Disc returned status {0}", text);
				}
				context.PushTop(text);
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
				MerchandizeResult merchandizeResult2 = ThinHelper.MakeInstance(result, context).ThinDisk(iD, MerchFlags.None);
				string text2 = merchandizeResult2.ToString().ToUpper();
				if (merchandizeResult2 != MerchandizeResult.Success)
				{
					LogHelper.Instance.WithContext("thin-disk returned status {0}", text2);
				}
				context.PushTop(text2);
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
