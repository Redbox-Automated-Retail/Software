using System;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Extensions;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

[Serializable]
public sealed class RinglightInstruction : Instruction
{
	private enum RinglightState
	{
		None,
		On,
		Off
	}

	public override string Mnemonic => "RINGLIGHT";

	protected override bool IsOperandRecognized(string operand)
	{
		return IsOperandValid(operand);
	}

	protected override void ParseOperands(TokenList operandTokens, ExecutionResult result)
	{
		if (operandTokens.Count != 1)
		{
			result.AddMissingOperandError("Expected a symbol: ON, OFF.");
		}
		else if (operandTokens[0].Type != TokenType.Symbol || !IsOperandValid(operandTokens[0].Value))
		{
			result.AddInvalidOperandError("Expected a symbol: ON, OFF.");
		}
	}

	protected override void OnExecute(ExecutionResult result, ExecutionContext context)
	{
		RinglightState ringlightState = Enum<RinglightState>.ParseIgnoringCase(base.Operands[0].Value, RinglightState.Off);
		ServiceLocator.Instance.GetService<IControlSystem>().ToggleRingLight((RinglightState.On == ringlightState) ? true : false, null);
	}

	private bool IsOperandValid(string op)
	{
		return Enum<RinglightState>.ParseIgnoringCase(op, RinglightState.None) != RinglightState.None;
	}
}
