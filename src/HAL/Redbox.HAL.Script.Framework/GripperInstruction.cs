using System;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Controller.Framework;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

[Serializable]
public sealed class GripperInstruction : Instruction
{
	private static class Commands
	{
		public const string Open = "OPEN";

		public const string Close = "CLOSE";

		public const string Status = "STATUS";

		public const string Extend = "EXTEND";

		public const string ExtendForTime = "EXTEND-FOR-TIME";

		public const string Retract = "RETRACT";

		public const string Rent = "RENT";

		public const string Push = "PUSH";

		public const string Peek = "PEEK";

		public const string Clear = "CLEAR";

		public const string PushOut = "PUSH-OUT";

		public const string PushIn = "PUSH-IN";

		public const string AcceptDiscAtDoor = "ACCEPT-DISC-AT-DOOR";

		public const string RejectDiscAtDoor = "REJECT-DISC-AT-DOOR";

		public const string VendDiscAtDoor = "VEND-DISC-AT-DOOR";

		public const string CheckPicker = "CHECK-PICKER";
	}

	public override string Mnemonic => "GRIPPER";

	protected override bool IsOperandRecognized(string operand)
	{
		return ValidateOperand(operand);
	}

	protected override void ParseOperands(TokenList operandTokens, ExecutionResult result)
	{
		if (operandTokens.Count <= 2 && operandTokens[0].Type != TokenType.Symbol && !ValidateOperand(operandTokens[0].Value))
		{
			result.AddError(ExecutionErrors.InvalidOperand, "Expected a symbol: OPEN, CLOSE, RENT, STATUS, CLEAR, EXTEND, EXTEND-FOR-TIME, RETRACT, PUSH, PEEK and an optional TIMEOUT=m and WAIT=TRUE|FALSE key value pair.");
		}
	}

	protected override void OnExecute(ExecutionResult result, ExecutionContext context)
	{
		_ = context.ProgramName;
		int? timeOutValue = GetTimeOutValue(result.Errors, context);
		Token token = base.Operands[0];
		if (token.Type != TokenType.Symbol)
		{
			return;
		}
		ErrorList errors = new ErrorList();
		string text = token.Value.ToUpper();
		if (context.GetSymbolValue(token.Value, errors) is string text2)
		{
			text = text2;
		}
		IControlSystem service = ServiceLocator.Instance.GetService<IControlSystem>();
		IMotionControlService service2 = ServiceLocator.Instance.GetService<IMotionControlService>();
		IControllerService service3 = ServiceLocator.Instance.GetService<IControllerService>();
		switch (text)
		{
		case "VEND-DISC-AT-DOOR":
			context.PushTop(service3.VendItemInPicker().Status.ToString().ToUpper());
			break;
		case "REJECT-DISC-AT-DOOR":
			context.PushTop(service3.RejectDiskInPicker().ToString().ToUpper());
			break;
		case "ACCEPT-DISC-AT-DOOR":
			context.PushTop(service3.AcceptDiskAtDoor().ToString().ToUpper());
			break;
		case "OPEN":
			TestFingerOperation(GripperFingerState.Open, context);
			break;
		case "CLOSE":
			TestFingerOperation(GripperFingerState.Closed, context);
			break;
		case "RENT":
			TestFingerOperation(GripperFingerState.Rent, context);
			break;
		case "STATUS":
		{
			IPickerSensorReadResult pickerSensorReadResult = service.ReadPickerSensors();
			if (!pickerSensorReadResult.Success || pickerSensorReadResult.IsFull)
			{
				context.PushTop("FULL");
			}
			else
			{
				context.PushTop("EMPTY");
			}
			break;
		}
		case "EXTEND":
		{
			IControlResponse controlResponse = service.ExtendArm();
			if (controlResponse.TimedOut)
			{
				service.RetractArm();
			}
			HandleError(controlResponse, context);
			break;
		}
		case "EXTEND-FOR-TIME":
			if (timeOutValue.HasValue)
			{
				service.TimedExtend(timeOutValue.Value);
			}
			else
			{
				service.TimedExtend();
			}
			break;
		case "RETRACT":
		{
			IControlResponse response = service.RetractArm();
			HandleError(response, context);
			break;
		}
		case "PEEK":
		{
			ILocation currentLocation = service2.CurrentLocation;
			if (currentLocation == null)
			{
				context.PushTop("LOCATIONOUTOFRANGE");
				break;
			}
			IPeekResult peekResult = new PeekOperation().Execute();
			if (!peekResult.TestOk)
			{
				context.PushTop(peekResult.Error.ToString().ToUpper());
			}
			else
			{
				context.PushTop(peekResult.IsFull ? "FULL" : "EMPTY");
			}
			break;
		}
		case "CLEAR":
			context.PushTop(service3.ClearGripper() == ErrorCodes.Success);
			break;
		case "PUSH":
		case "PUSH-IN":
		{
			using ClearPickerFrontOperation clearPickerFrontOperation = new ClearPickerFrontOperation();
			clearPickerFrontOperation.Execute();
			break;
		}
		case "PUSH-OUT":
			context.PushTop(service3.PushOut().ToString().ToUpper());
			break;
		case "CHECK-PICKER":
		{
			string vendId = null;
			KeyValuePair keyValuePair = base.Operands.GetKeyValuePair("VEND-ID");
			if (keyValuePair != null)
			{
				string keyValuePairValue = GetKeyValuePairValue<string>(keyValuePair, result.Errors, context);
				if (result.Errors.Count > 0)
				{
					break;
				}
				vendId = keyValuePairValue;
			}
			using CheckPickerOperation checkPickerOperation = new CheckPickerOperation(context);
			context.PushTop(checkPickerOperation.CheckPicker(result, vendId).ToString().ToUpper());
			break;
		}
		default:
		{
			string text3 = $"Unrecognized operand {text}";
			LogHelper.Instance.WithContext(LogEntryType.Error, text3);
			result.Errors.Add(Error.NewError("E001", "Unknown operand", text3));
			break;
		}
		}
	}

	private void TestFingerOperation(GripperFingerState state, ExecutionContext context)
	{
		IControlResponse response = ServiceLocator.Instance.GetService<IControlSystem>().SetFinger(state);
		HandleError(response, context);
	}

	private bool ValidateOperand(string operand)
	{
		if (string.Compare(operand, "OPEN", ignoreCase: true) != 0 && string.Compare(operand, "CLOSE", ignoreCase: true) != 0 && string.Compare(operand, "STATUS", ignoreCase: true) != 0 && string.Compare(operand, "EXTEND", ignoreCase: true) != 0 && string.Compare(operand, "EXTEND-FOR-TIME", ignoreCase: true) != 0 && string.Compare(operand, "RETRACT", ignoreCase: true) != 0 && string.Compare(operand, "RENT", ignoreCase: true) != 0 && string.Compare(operand, "PUSH", ignoreCase: true) != 0 && string.Compare(operand, "PEEK", ignoreCase: true) != 0 && string.Compare(operand, "CLEAR", ignoreCase: true) != 0 && string.Compare(operand, "PUSH-OUT", ignoreCase: true) != 0 && string.Compare(operand, "PUSH-IN", ignoreCase: true) != 0 && string.Compare(operand, "ACCEPT-DISC-AT-DOOR", ignoreCase: true) != 0 && string.Compare(operand, "REJECT-DISC-AT-DOOR", ignoreCase: true) != 0 && string.Compare(operand, "VEND-DISC-AT-DOOR", ignoreCase: true) != 0)
		{
			return string.Compare(operand, "CHECK-PICKER", ignoreCase: true) == 0;
		}
		return true;
	}
}
