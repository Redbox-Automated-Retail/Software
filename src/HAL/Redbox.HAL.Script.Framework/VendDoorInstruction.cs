using System;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Extensions;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

[Serializable]
public sealed class VendDoorInstruction : Instruction
{
	private enum DoorCommands
	{
		None,
		Open,
		Rent,
		Close,
		Status
	}

	public override string Mnemonic => "VENDDOOR";

	protected override bool IsOperandRecognized(string operand)
	{
		return Enum<DoorCommands>.ParseIgnoringCase(operand, DoorCommands.None) != DoorCommands.None;
	}

	protected override void ParseOperands(TokenList operandTokens, ExecutionResult result)
	{
		if (operandTokens.Count != 1)
		{
			result.AddMissingOperandError("Expected a symbol: OPEN, CLOSE, RENT, or STATUS.");
		}
		else if (operandTokens[0].Type != TokenType.Symbol || !IsOperandRecognized(operandTokens[0].Value))
		{
			result.AddInvalidOperandError("Expected a symbol: OPEN, CLOSE, RENT, or STATUS.");
		}
	}

	protected override void OnExecute(ExecutionResult result, ExecutionContext context)
	{
		Token token = base.Operands[0];
		if (token.Type == TokenType.Symbol)
		{
			IControlSystem service = ServiceLocator.Instance.GetService<IControlSystem>();
			switch (Enum<DoorCommands>.ParseIgnoringCase(token.Value, DoorCommands.None))
			{
			case DoorCommands.Status:
				context.PushTop(service.ReadVendDoorPosition().ToString().ToUpper());
				break;
			case DoorCommands.Open:
				context.PushTop(base.TimeoutMessage);
				break;
			case DoorCommands.Rent:
				HandleError(service.VendDoorRent(), context);
				break;
			case DoorCommands.Close:
				HandleError(service.VendDoorClose(), context);
				break;
			default:
				result.AddError(ExecutionErrors.InvalidSymbolValue, $"The value {token.Value} is unrecognized.");
				break;
			}
		}
	}
}
