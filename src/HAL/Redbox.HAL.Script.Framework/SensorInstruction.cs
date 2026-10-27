using System;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

[Serializable]
public sealed class SensorInstruction : Instruction
{
	private static class KeyNames
	{
		public const string AuxillarySensor = "AUX-SENSOR";

		public const string PickerSensor = "PICKER-SENSOR";
	}

	private static class Commands
	{
		public const string PickerOn = "PICKER-ON";

		public const string PickerOff = "PICKER-OFF";

		public const string Read = "READ";

		public const string ReadAuxInputs = "READ-AUX-INPUTS";

		public const string ReadPickerInputs = "READ-PICKER-INPUTS";
	}

	public override string Mnemonic => "SENSOR";

	protected override void ParseOperands(TokenList operandTokens, ExecutionResult result)
	{
		if (operandTokens[0].Type != TokenType.Symbol || !IsOperandRecognized(operandTokens[0].Value))
		{
			result.AddInvalidOperandError("Expected a symbol: PICKER-ON, PICKER-OFF, READ-PICKER, READ-AUX-INPUTS, READ-PICKER-INPUTS");
		}
	}

	protected override void OnExecute(ExecutionResult result, ExecutionContext context)
	{
		IControlSystem service = ServiceLocator.Instance.GetService<IControlSystem>();
		Token token = base.Operands[0];
		if (string.Compare(token.Value, "PICKER-ON", ignoreCase: true) == 0)
		{
			context.PushTop(service.SetSensors(on: true).Success);
		}
		else if (string.Compare(token.Value, "PICKER-OFF", ignoreCase: true) == 0)
		{
			context.PushTop(service.SetSensors(on: false).Success);
		}
		else if (string.Compare(token.Value, "READ-AUX-INPUTS", ignoreCase: true) == 0)
		{
			PushInputs(context, service.ReadAuxInputs());
		}
		else if (string.Compare(token.Value, "READ-PICKER-INPUTS", ignoreCase: true) == 0)
		{
			PushInputs(context, service.ReadPickerInputs());
		}
		else
		{
			if (string.Compare(token.Value, "READ", ignoreCase: true) != 0)
			{
				return;
			}
			IPickerSensorReadResult sensorReadResult = service.ReadPickerSensors(forceTrackClose: false);
			if (!sensorReadResult.Success)
			{
				context.PushTop(0);
				return;
			}
			int count = 1;
			sensorReadResult.Foreach(delegate(PickerInputs each)
			{
				string instance = string.Format("SENSOR{0}-{1}", count++, sensorReadResult.IsInputActive(each) ? "BLOCKED" : "CLEAR");
				context.PushTop(instance);
			});
			context.PushTop(sensorReadResult.InputCount);
		}
	}

	protected override bool IsOperandRecognized(string op)
	{
		return OperandIsValid(op);
	}

	private bool OperandIsValid(string operand)
	{
		if (string.Compare(operand, "PICKER-ON", ignoreCase: true) != 0 && string.Compare(operand, "PICKER-OFF", ignoreCase: true) != 0 && string.Compare(operand, "READ", ignoreCase: true) != 0 && string.Compare(operand, "READ-PICKER-INPUTS", ignoreCase: true) != 0)
		{
			return string.Compare(operand, "READ-AUX-INPUTS", ignoreCase: true) == 0;
		}
		return true;
	}

	private void PushInputs<T>(ExecutionContext ctx, IReadInputsResult<T> sensors)
	{
		if (sensors.Success)
		{
			sensors.Foreach(delegate(T each)
			{
				ctx.PushTop(sensors.IsInputActive(each) ? "1" : "0");
			});
			ctx.PushTop(sensors.InputCount);
		}
		ctx.PushTop(sensors.Error);
	}
}
