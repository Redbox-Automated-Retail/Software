using System;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

[Serializable]
public sealed class GetConfigInstruction : Instruction
{
	public override string Mnemonic => "GETCFG";

	protected override void ParseOperands(TokenList operandTokens, ExecutionResult result)
	{
		if (operandTokens.Count < 2 || operandTokens[0].Type != TokenType.StringLiteral)
		{
			result.AddInvalidOperandError("Expected a string literal operand.");
		}
	}

	protected override void OnExecute(ExecutionResult result, ExecutionContext context)
	{
		KeyValuePair keyValuePair = base.Operands.GetKeyValuePair("TYPE");
		object[] args = OperandsHelper.ConvertSymbolsToParameterArray(base.Operands, result.Errors, context);
		if (result.Errors.Count <= 0)
		{
			string name = string.Format(base.Operands[0].Value, args);
			object empty = string.Empty;
			try
			{
				empty = ServiceLocator.Instance.GetService<IConfigurationService>().GetPropertyByName(keyValuePair.Value, name);
			}
			catch (ArgumentException ex)
			{
				AddError("E002", ex.ToString(), result.Errors);
				return;
			}
			if (empty != null)
			{
				context.PushTop(empty);
			}
		}
	}
}
