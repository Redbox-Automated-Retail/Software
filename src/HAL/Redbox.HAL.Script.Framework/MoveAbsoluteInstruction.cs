using System;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

[Serializable]
public sealed class MoveAbsoluteInstruction : Instruction
{
	private static class KeyNames
	{
		public const string Axis = "AXIS";

		public const string Units = "UNITS";

		public const string SecondaryUnits = "SECONDARY-UNITS";

		public const string SensorCheck = "SENSOR-CHECK";
	}

	public override string Mnemonic => "MOVEABS";

	protected override void ParseOperands(TokenList operandTokens, ExecutionResult result)
	{
		if (operandTokens.Count == 0 || operandTokens.Count > 3)
		{
			result.AddError(ExecutionErrors.InvalidOperand, "Expected key value pair AXIS=a, UNITS=n, SECONDARY-UNITS=n. The values of AXIS should be X or Y. The value for UNITS and SECONDARY-UNITS may be numeric literals or symbols.");
		}
	}

	protected override void OnExecute(ExecutionResult result, ExecutionContext context)
	{
		bool checkSensors = true;
		KeyValuePair keyValuePair = base.Operands.GetKeyValuePair("SENSOR-CHECK");
		if (keyValuePair != null && bool.TryParse(keyValuePair.Value, out var result2))
		{
			checkSensors = result2;
		}
		KeyValuePair keyValuePair2 = base.Operands.GetKeyValuePair("AXIS");
		KeyValuePair keyValuePair3 = base.Operands.GetKeyValuePair("UNITS");
		KeyValuePair keyValuePair4 = base.Operands.GetKeyValuePair("SECONDARY-UNITS");
		int? keyValuePairValue = GetKeyValuePairValue<int?>(keyValuePair3, result.Errors, context);
		int? keyValuePairValue2 = GetKeyValuePairValue<int?>(keyValuePair4, result.Errors, context);
		if (result.Errors.Count <= 0 && keyValuePairValue.HasValue)
		{
			Axis axis = Axis.X;
			if (string.Compare(keyValuePair2.Value, Axis.X.ToString(), ignoreCase: true) == 0)
			{
				axis = Axis.X;
			}
			else if (string.Compare(keyValuePair2.Value, Axis.Y.ToString(), ignoreCase: true) == 0)
			{
				axis = Axis.Y;
			}
			else if (string.Compare(keyValuePair2.Value, Axis.XY.ToString(), ignoreCase: true) == 0)
			{
				axis = Axis.XY;
			}
			int? xunits = null;
			int? yunits = null;
			if (axis == Axis.X)
			{
				xunits = keyValuePairValue;
				yunits = null;
			}
			else if (Axis.XY == axis)
			{
				xunits = keyValuePairValue;
				yunits = keyValuePairValue2;
			}
			else if (Axis.Y == axis)
			{
				xunits = null;
				yunits = keyValuePairValue;
			}
			context.PushTop(ServiceLocator.Instance.GetService<IMotionControlService>().MoveAbsolute(axis, xunits, yunits, checkSensors).ToString()
				.ToUpper());
		}
	}
}
