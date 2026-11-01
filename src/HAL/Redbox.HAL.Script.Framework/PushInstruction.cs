using System;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

[Serializable]
public sealed class PushInstruction : Instruction
{
	public override string Mnemonic => "PUSH";

	protected override void OnExecute(ExecutionResult result, ExecutionContext context)
	{
		foreach (Token operand in base.Operands)
		{
			object operandValue = operand.ConvertValue();
			if (operand.Type == TokenType.Symbol && operand.Value.Equals(operandValue))
			{
				operandValue = context.GetSymbolValue(operand.Value, result.Errors);
				if (operandValue == null)
				{
					break;
				}
			}
			else
			{
				operandValue = operand.ConvertValue();
			}
			context.PushTop(operandValue);
		}
	}
}
