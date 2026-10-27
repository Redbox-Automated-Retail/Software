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
			object obj = operand.ConvertValue();
			if (operand.Type == TokenType.Symbol && operand.Value.Equals(obj))
			{
				obj = context.GetSymbolValue(operand.Value, result.Errors);
				if (obj == null)
				{
					break;
				}
			}
			else
			{
				obj = operand.ConvertValue();
			}
			context.PushTop(obj);
		}
	}
}
