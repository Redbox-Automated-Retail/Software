using System;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

[Serializable]
public sealed class ResultInstruction : Instruction
{
	public static class Prefixes
	{
		public const string ResultPrefix = "RESULT";
	}

	private static class StackEnds
	{
		public const string Bottom = "bottom";

		public const string Top = "top";
	}

	private static class KeyNames
	{
		public const string End = "END";

		public const string Deck = "DECK";

		public const string Slot = "SLOT";

		public const string Code = "CODE";

		public const string Item = "ITEM";

		public const string Message = "MESSAGE";

		public const string ReturnTime = "RETURNTIME";

		public const string PreviousID = "PREVIOUS";
	}

	public override string Mnemonic => "RESULT";

	protected override void ParseOperands(TokenList operandTokens, ExecutionResult result)
	{
		if (operandTokens.Count < 2)
		{
			result.AddInvalidOperandError("Expected the key value parameters CODE=c and MESSAGE=m.  Optional pairs can be: DECK=n, SLOT=n, and END=BOTTOM|TOP.");
		}
		else if (operandTokens.GetKeyValuePair("CODE") == null || operandTokens.GetKeyValuePair("MESSAGE") == null)
		{
			result.AddInvalidOperandError($"Expected key value parameters CODE=c and MESSAGE=m. (line {base.LineNumber})");
		}
	}

	protected override void OnExecute(ExecutionResult result, ExecutionContext context)
	{
		KeyValuePair keyValuePair = base.Operands.GetKeyValuePair("END");
		bool flag = keyValuePair != null && string.Compare(keyValuePair.Value, "bottom", ignoreCase: true) == 0;
		string keyValuePairValue = GetKeyValuePairValue<string>(base.Operands.GetKeyValuePair("CODE"), result.Errors, context);
		string keyValuePairValue2 = GetKeyValuePairValue<string>(base.Operands.GetKeyValuePair("MESSAGE"), result.Errors, context);
		int? keyValuePairValue3 = GetKeyValuePairValue<int?>(base.Operands.GetKeyValuePair("DECK"), result.Errors, context);
		int? keyValuePairValue4 = GetKeyValuePairValue<int?>(base.Operands.GetKeyValuePair("SLOT"), result.Errors, context);
		string keyValuePairValue5 = GetKeyValuePairValue<string>(base.Operands.GetKeyValuePair("ITEM"), result.Errors, context);
		string keyValuePairValue6 = GetKeyValuePairValue<string>(base.Operands.GetKeyValuePair("RETURNTIME"), result.Errors, context);
		string keyValuePairValue7 = GetKeyValuePairValue<string>(base.Operands.GetKeyValuePair("PREVIOUS"), result.Errors, context);
		if (result.Errors.Count > 0)
		{
			return;
		}
		DateTime? returnTime = null;
		if (!string.IsNullOrEmpty(keyValuePairValue6))
		{
			if (!DateTime.TryParse(keyValuePairValue6, out var result2))
			{
				if (!"NONE".Equals(keyValuePairValue6))
				{
					LogHelper.Instance.Log($"Unable to parse date time {keyValuePairValue6}", LogEntryType.Error);
				}
				returnTime = null;
			}
			else
			{
				returnTime = result2;
			}
		}
		context.CreateResult((!flag) ? StackEnd.Top : StackEnd.Bottom, keyValuePairValue, keyValuePairValue2, keyValuePairValue3, keyValuePairValue4, keyValuePairValue5, returnTime, keyValuePairValue7);
	}
}
