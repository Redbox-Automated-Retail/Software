using System;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Extensions;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

[Serializable]
public sealed class MoveInstruction : Instruction
{
	private static class KeyNames
	{
		internal const string Deck = "DECK";

		internal const string Slot = "SLOT";

		internal const string Mode = "MODE";

		internal const string Location = "LOCATION";

		internal const string Password = "PASSWORD";

		internal const string SlotOffset = "SLOT-OFFSET";

		internal const string WithOffset = "WITH-OFFSET";

		internal const string Axis = "AXIS";

		internal const string Units = "UNITS";
	}

	public override string Mnemonic => "MOVE";

	protected override void ParseOperands(TokenList operandTokens, ExecutionResult result)
	{
		if (operandTokens[0].Value.StartsWith("SLOT-OFFSET", StringComparison.CurrentCultureIgnoreCase))
		{
			return;
		}
		if (string.Compare(operandTokens[0].Value, "WITH-OFFSET", ignoreCase: true) == 0)
		{
			KeyValuePair keyValuePair = operandTokens.GetKeyValuePair("AXIS");
			if (keyValuePair == null)
			{
				AddError("E004", string.Format("Axis KVP is required for the '{0}' form.", "WITH-OFFSET"), result.Errors);
				return;
			}
			KeyValuePair keyValuePair2 = operandTokens.GetKeyValuePair("UNITS");
			if (keyValuePair2 == null)
			{
				AddError("E004", string.Format("Offset KVP is required for the '{0}' form.", "WITH-OFFSET"), result.Errors);
			}
		}
		else if (operandTokens[0].Value.StartsWith("LOCATION", StringComparison.CurrentCultureIgnoreCase))
		{
			AddError("E004", "The LOCATION form is no longer supported.", result.Errors);
		}
		else if (operandTokens.Count < 2)
		{
			result.AddError(ExecutionErrors.InvalidOperand, "Expected key value pair DECK=y, SLOT=x, MODE=m.  The values of x and y may be numeric literals or symbols.");
		}
	}

	protected override void OnExecute(ExecutionResult result, ExecutionContext context)
	{
		MoveMode mode = MoveMode.None;
		KeyValuePair keyValuePair = base.Operands.GetKeyValuePair("MODE");
		if (keyValuePair != null)
		{
			mode = Enum<MoveMode>.ParseIgnoringCase(keyValuePair.Value, MoveMode.None);
		}
		IMotionControlService service = ServiceLocator.Instance.GetService<IMotionControlService>();
		KeyValuePair keyValuePair2 = base.Operands.GetKeyValuePair("SLOT-OFFSET");
		if (base.Operands.Count == 1 && keyValuePair2 != null)
		{
			int keyValuePairValue = GetKeyValuePairValue<int>(keyValuePair2, result.Errors, context);
			if (result.Errors.Count > 0)
			{
				context.PushTop(ErrorCodes.InvalidLocation.ToString().ToUpper());
				return;
			}
			if (service.CurrentLocation == null)
			{
				context.PushTop(ErrorCodes.InvalidLocation.ToString().ToUpper());
				return;
			}
			using MoveDecoratorService moveDecoratorService = new MoveDecoratorService(context);
			ErrorCodes errorCodes = moveDecoratorService.ShowEmptyStuck(service.CurrentLocation, keyValuePairValue);
			if (errorCodes == ErrorCodes.Success)
			{
				context.PushTop(keyValuePairValue);
			}
			context.PushTop(errorCodes.ToString().ToUpper());
			return;
		}
		if (!GetLocation(result, context, out var location))
		{
			AddError("E004", "Deck or slot are undefined, and no location specified; reissue the MOVE instruction specifying both deck and slot values or the location.", result.Errors);
			return;
		}
		IDumpbinService service2 = ServiceLocator.Instance.GetService<IDumpbinService>();
		if (context.IsImmediate && service2.IsBin(location))
		{
			string instance = ErrorCodes.InvalidMoveToSecureLocation.ToString().ToUpper();
			context.PushTop(instance);
			return;
		}
		ErrorCodes errorCodes2;
		if (string.Compare(base.Operands[0].Value, "WITH-OFFSET", ignoreCase: true) == 0)
		{
			KeyValuePair keyValuePair3 = base.Operands.GetKeyValuePair("AXIS");
			KeyValuePair keyValuePair4 = base.Operands.GetKeyValuePair("UNITS");
			int? keyValuePairValue2 = GetKeyValuePairValue<int?>(keyValuePair4, result.Errors, context);
			if (!keyValuePairValue2.HasValue)
			{
				AddError("E004", "Units conversion error.", result.Errors);
				return;
			}
			OffsetMoveData data = ((Enum<Axis>.ParseIgnoringCase(keyValuePair3.Value, Axis.X) == Axis.X) ? new OffsetMoveData(keyValuePairValue2.Value, 0) : new OffsetMoveData(0, keyValuePairValue2.Value));
			errorCodes2 = service.MoveTo(location, mode, context.AppLog, ref data);
		}
		else
		{
			errorCodes2 = service.MoveTo(location, mode, context.AppLog);
		}
		context.PushTop(errorCodes2.ToString().ToUpper());
	}
}
