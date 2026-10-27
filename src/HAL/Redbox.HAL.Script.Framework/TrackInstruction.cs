using System;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Extensions;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

[Serializable]
public sealed class TrackInstruction : Instruction
{
	private enum Commands
	{
		None,
		Open,
		Close,
		Status,
		Cycle
	}

	public override string Mnemonic => "TRACK";

	protected override bool IsOperandRecognized(string operand)
	{
		return Enum<Commands>.ParseIgnoringCase(operand, Commands.None) != Commands.None;
	}

	protected override void ParseOperands(TokenList operandTokens, ExecutionResult result)
	{
		if (operandTokens.Count != 1)
		{
			result.AddMissingOperandError("Expected a symbol: OPEN, CLOSE, CYCLE or STATUS.");
		}
		else if (operandTokens[0].Type != TokenType.Symbol || !IsOperandRecognized(operandTokens[0].Value))
		{
			result.AddInvalidOperandError("Expected a symbol: OPEN, CLOSE, CYCLE or STATUS.");
		}
	}

	protected override void OnExecute(ExecutionResult result, ExecutionContext context)
	{
		Token token = base.Operands[0];
		IControlSystem service = ServiceLocator.Instance.GetService<IControlSystem>();
		switch (Enum<Commands>.ParseIgnoringCase(token.Value, Commands.None))
		{
		case Commands.Status:
			context.PushTop(service.TrackState.ToString().ToUpper());
			break;
		case Commands.Open:
			HandleError(service.TrackOpen(), context);
			break;
		case Commands.Close:
			HandleError(service.TrackClose(), context);
			break;
		case Commands.Cycle:
			context.PushTop(service.TrackCycle().ToString().ToUpper());
			break;
		}
	}
}
