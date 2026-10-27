using System;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Extensions;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

[Serializable]
public sealed class MoveVendInstruction : Instruction
{
	private const string Mode = "MODE";

	private const string ResetOnCommunicationFailure = "RESETONCOMMERROR";

	public override string Mnemonic => "MOVEVEND";

	protected override ExpectedOperands Expected => ExpectedOperands.Optional;

	protected override void ParseOperands(TokenList operandTokens, ExecutionResult result)
	{
		if (operandTokens.Count > 2)
		{
			result.AddError(ExecutionErrors.InvalidOperand, "Expected either no arguments, or single key value pair MODE=GET.");
		}
	}

	protected override void OnExecute(ExecutionResult result, ExecutionContext context)
	{
		MoveMode mode = MoveMode.None;
		if (base.Operands.Count > 0)
		{
			KeyValuePair keyValuePair = base.Operands.GetKeyValuePair("MODE");
			if (keyValuePair != null)
			{
				mode = Enum<MoveMode>.ParseIgnoringCase(keyValuePair.Value, MoveMode.None);
			}
		}
		KeyValuePair keyValuePair2 = base.Operands.GetKeyValuePair("RESETONCOMMERROR");
		if (keyValuePair2 != null)
		{
			new ErrorList();
			bool.TryParse(keyValuePair2.Value, out var _);
		}
		string instance = ServiceLocator.Instance.GetService<IMotionControlService>().MoveVend(mode, context.AppLog).ToString()
			.ToUpper();
		context.PushTop(instance);
	}
}
