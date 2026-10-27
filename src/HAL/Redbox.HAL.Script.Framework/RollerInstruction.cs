using System;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

[Serializable]
public sealed class RollerInstruction : Instruction
{
	private static class Commands
	{
		public const string In = "IN";

		public const string Out = "OUT";

		public const string Stop = "STOP";
	}

	public override string Mnemonic => "ROLLER";

	protected override bool IsOperandRecognized(string operand)
	{
		return IsOperandValid(operand);
	}

	protected override void ParseOperands(TokenList operandTokens, ExecutionResult result)
	{
		if (!operandTokens[0].IsKeyValuePair && operandTokens[0].Type != TokenType.Symbol && IsOperandValid(operandTokens[0].Value))
		{
			result.AddInvalidOperandError("Expected a symbol: IN, OUT, or STOP.");
		}
	}

	protected override void OnExecute(ExecutionResult result, ExecutionContext context)
	{
		IControlSystem service = ServiceLocator.Instance.GetService<IControlSystem>();
		GetWaitFlag();
		int? num = GetTimeOutValue(result.Errors, context);
		Token token = base.Operands[0];
		if (token.IsKeyValuePair)
		{
			KeyValuePair obj = (KeyValuePair)token.ConvertValue();
			RollerPosition position = RollerPosition.Position4;
			if (int.TryParse(obj.Value, out var result2))
			{
				position = (RollerPosition)result2;
			}
			if (!num.HasValue)
			{
				num = 8000;
			}
			HandleError(service.RollerToPosition(position, num.Value), context);
		}
		else if (token.Type == TokenType.Symbol)
		{
			if (string.Compare(token.Value, "IN", ignoreCase: true) == 0)
			{
				HandleError(service.StartRollerIn(), context);
			}
			else if (string.Compare(token.Value, "OUT", ignoreCase: true) == 0)
			{
				HandleError(service.StartRollerOut(), context);
			}
			else if (string.Compare(token.Value, "STOP", ignoreCase: true) == 0)
			{
				HandleError(service.StopRoller(), context);
			}
		}
	}

	private bool IsOperandValid(string operand)
	{
		if (string.Compare(operand, "IN", ignoreCase: true) != 0 && string.Compare(operand, "OUT", ignoreCase: true) != 0)
		{
			return string.Compare(operand, "STOP", ignoreCase: true) == 0;
		}
		return true;
	}
}
