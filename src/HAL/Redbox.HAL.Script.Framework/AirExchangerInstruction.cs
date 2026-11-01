using System;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Extensions;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

[Serializable]
public sealed class AirExchangerInstruction : Instruction
{
	private enum ExpectedOperand
	{
		None,
		IsConfigured,
		FanOn,
		FanOff,
		FanStatus,
		ResetFailureCounter,
		BoardStatus,
		Reset
	}

	public override string Mnemonic => "AIRXCHGR";

	protected override bool IsOperandRecognized(string operand)
	{
		return IsValidOperand(operand);
	}

	protected override void ParseOperands(TokenList operandTokens, ExecutionResult result)
	{
		if (!IsValidOperand(operandTokens[0].Value))
		{
			result.AddInvalidOperandError($"Unexpected operand '{operandTokens[0].Value}'");
		}
	}

	protected override void OnExecute(ExecutionResult result, ExecutionContext context)
	{
		ExpectedOperand expectedOperand = Enum<ExpectedOperand>.ParseIgnoringCase(base.Operands[0].Value, ExpectedOperand.None);
		IAirExchangerService airExchangerService = ServiceLocator.Instance.GetService<IAirExchangerService>();
		switch (expectedOperand)
		{
		case ExpectedOperand.None:
			LogHelper.Instance.WithContext(LogEntryType.Error, "{0} instruction: converted '{1}' to operand None", Mnemonic, base.Operands[0].Value);
			break;
		case ExpectedOperand.IsConfigured:
			context.PushTop(airExchangerService.Configured);
			break;
		case ExpectedOperand.FanOn:
			airExchangerService.TurnOnFan();
			break;
		case ExpectedOperand.FanOff:
			airExchangerService.TurnOffFan();
			break;
		case ExpectedOperand.FanStatus:
			context.PushTop(airExchangerService.FanStatus.ToString().ToUpper());
			break;
		case ExpectedOperand.ResetFailureCounter:
			airExchangerService.ResetFailureCount();
			context.PushTop(airExchangerService.PersistentFailureCount());
			break;
		case ExpectedOperand.BoardStatus:
			context.PushTop(airExchangerService.CheckStatus().ToString().ToUpper());
			break;
		case ExpectedOperand.Reset:
			context.PushTop(airExchangerService.Reset().ToString().ToUpper());
			break;
		}
	}

	private bool IsValidOperand(string operand)
	{
		return Enum<ExpectedOperand>.ParseIgnoringCase(operand, ExpectedOperand.None) != ExpectedOperand.None;
	}
}
