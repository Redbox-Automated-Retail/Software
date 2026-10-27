using System;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Controller.Framework;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

[Serializable]
public sealed class SetConfigInstruction : Instruction
{
	public override string Mnemonic => "SETCFG";

	protected override void ParseOperands(TokenList operandTokens, ExecutionResult result)
	{
		if (operandTokens[0].Type != TokenType.StringLiteral)
		{
			result.AddInvalidOperandError("Expected a string literal.");
		}
	}

	protected override void OnExecute(ExecutionResult result, ExecutionContext context)
	{
		KeyValuePair keyValuePair = base.Operands.GetKeyValuePair("TYPE");
		object[] array = OperandsHelper.ConvertSymbolsToParameterArray(base.Operands, result.Errors, context);
		if (result.Errors.Count <= 0)
		{
			string value = base.Operands[0].Value;
			if (string.Compare(keyValuePair.Value, "KIOSK", ignoreCase: true) == 0)
			{
				Redbox.HAL.Controller.Framework.KioskConfiguration.Instance.SetConfig(value, array[0] as string, saveValue: true);
			}
			else
			{
				ServiceLocator.Instance.GetService<IConfigurationService>().SetPropertyByName(keyValuePair.Value, value, array);
			}
		}
	}
}
