using System;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

[Serializable]
public sealed class PushFormatInstruction : Instruction
{
	private static class StackEnds
	{
		public const string Bottom = "bottom";

		public const string Top = "top";
	}

	private static class KeyNames
	{
		public const string End = "END";
	}

	public override string Mnemonic => "PUSHF";

	protected override void ParseOperands(TokenList operandTokens, ExecutionResult result)
	{
		if (operandTokens[0].Type != TokenType.StringLiteral)
		{
			result.AddError(ExecutionErrors.InvalidOperand, "Expected a string literal and any number of optional subtitution parameters.");
		}
	}

	protected override void OnExecute(ExecutionResult result, ExecutionContext context)
	{
		KeyValuePair keyValuePair = base.Operands.GetKeyValuePair("END");
		bool flag = keyValuePair != null && string.Compare(keyValuePair.Value, "bottom", ignoreCase: true) == 0;
		object[] args = ConvertSymbolsToParameterArray(result.Errors, context);
		if (result.Errors.Count <= 0)
		{
			string instance = string.Format(base.Operands[0].Value, args);
			if (flag)
			{
				context.PushBottom(instance);
			}
			else
			{
				context.PushTop(instance);
			}
		}
	}
}
