using System;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

[Serializable]
public sealed class YieldInstruction : Instruction
{
	public override string Mnemonic => "YIELD";

	public override bool IsCooperativeMultitask => true;

	protected override ExpectedOperands Expected => ExpectedOperands.Optional;

	protected override bool IsOperandRecognized(string operand)
	{
		return "RUNTIME-ONLY".Equals(operand);
	}

	protected override void ParseOperands(TokenList operandTokens, ExecutionResult result)
	{
		if (operandTokens.Count != 0 && operandTokens[0].Type != TokenType.NumericLiteral && !operandTokens[0].IsSymbolOrConst)
		{
			result.AddInvalidOperandError("Expected a numeric literal.");
		}
	}

	protected override void OnExecute(ExecutionResult result, ExecutionContext context)
	{
		if (base.Operands.Count == 1)
		{
			double? num = null;
			if (base.Operands[0].IsSymbolOrConst)
			{
				object symbolValue = context.GetSymbolValue(base.Operands[0].Value, result.Errors);
				if (symbolValue == null || result.Errors.Count > 0)
				{
					return;
				}
				if (symbolValue is int || symbolValue is decimal)
				{
					num = (double)Convert.ChangeType(symbolValue, typeof(double));
				}
			}
			else if (base.Operands[0].Type == TokenType.NumericLiteral)
			{
				num = (double)Convert.ChangeType(base.Operands[0].ConvertValue(), typeof(double));
			}
			if (num.HasValue)
			{
				DateTime value = DateTime.Now.AddMilliseconds(num.Value);
				context.StartTime = value;
			}
		}
		ExecutionEngine.Instance.PerformContextSwitch(runtimeOnly: false);
	}
}
