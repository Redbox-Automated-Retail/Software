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
		bool atBottom = keyValuePair != null && string.Compare(keyValuePair.Value, "bottom", ignoreCase: true) == 0;
		string code = GetKeyValuePairValue<string>(base.Operands.GetKeyValuePair("CODE"), result.Errors, context);
		string message = GetKeyValuePairValue<string>(base.Operands.GetKeyValuePair("MESSAGE"), result.Errors, context);
		int? deck = GetKeyValuePairValue<int?>(base.Operands.GetKeyValuePair("DECK"), result.Errors, context);
		int? slot = GetKeyValuePairValue<int?>(base.Operands.GetKeyValuePair("SLOT"), result.Errors, context);
		string item = GetKeyValuePairValue<string>(base.Operands.GetKeyValuePair("ITEM"), result.Errors, context);
		string returnTimeText = GetKeyValuePairValue<string>(base.Operands.GetKeyValuePair("RETURNTIME"), result.Errors, context);
		string previous = GetKeyValuePairValue<string>(base.Operands.GetKeyValuePair("PREVIOUS"), result.Errors, context);
		if (result.Errors.Count > 0)
		{
			return;
		}
		DateTime? returnTime = null;
		if (!string.IsNullOrEmpty(returnTimeText))
		{
			if (!DateTime.TryParse(returnTimeText, out var parsedReturnTime))
			{
				if (!"NONE".Equals(returnTimeText))
				{
					LogHelper.Instance.Log($"Unable to parse date time {returnTimeText}", LogEntryType.Error);
				}
				returnTime = null;
			}
			else
			{
				returnTime = parsedReturnTime;
			}
		}
		context.CreateResult((!atBottom) ? StackEnd.Top : StackEnd.Bottom, code, message, deck, slot, item, returnTime, previous);
	}
}
