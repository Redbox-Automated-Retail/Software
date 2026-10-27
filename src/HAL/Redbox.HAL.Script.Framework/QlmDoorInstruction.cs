using System;
using Redbox.HAL.Component.Model.Extensions;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

[Serializable]
public sealed class QlmDoorInstruction : Instruction
{
	private enum QlmOperands
	{
		None,
		Lock,
		Unlock,
		Status
	}

	public override string Mnemonic => "QLMDOOR";

	protected override void ParseOperands(TokenList operandTokens, ExecutionResult result)
	{
		if (operandTokens.Count != 1)
		{
			result.AddMissingOperandError("Expected a symbol: LOCK, UNLOCK.");
			return;
		}
		QlmOperands qlmOperands = Enum<QlmOperands>.ParseIgnoringCase(operandTokens[0].Value, QlmOperands.None);
		if (operandTokens[0].Type != TokenType.Symbol || qlmOperands == QlmOperands.None)
		{
			result.AddInvalidOperandError("Expected a symbol: LOCK, UNLOCK, or STATUS.");
		}
	}

	protected override void OnExecute(ExecutionResult result, ExecutionContext context)
	{
		Token token = base.Operands[0];
		if (token.Type == TokenType.Symbol)
		{
			switch (Enum<QlmOperands>.ParseIgnoringCase(token.Value, QlmOperands.None))
			{
			case QlmOperands.Lock:
				context.PushTop(base.TimeoutMessage);
				break;
			case QlmOperands.Unlock:
				context.PushTop(base.TimeoutMessage);
				break;
			case QlmOperands.Status:
				context.PushTop("CLOSED");
				break;
			default:
				result.AddError(ExecutionErrors.InvalidSymbolValue, $"The symbol {token.Value} is invalid.");
				break;
			}
		}
	}
}
