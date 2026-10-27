using System;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Extensions;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

[Serializable]
public sealed class AudioInstruction : Instruction
{
	public override string Mnemonic => "AUDIO";

	protected override bool IsOperandRecognized(string operand)
	{
		return Enum<AudioChannelState>.ParseIgnoringCase(operand, AudioChannelState.None) != AudioChannelState.None;
	}

	protected override void ParseOperands(TokenList operandTokens, ExecutionResult result)
	{
		if (!IsOperandRecognized(operandTokens[0].Value))
		{
			result.Errors.Add(Error.NewError("C001", "Unexpected form", $"The operand {operandTokens[0].Value} for AUDIO isn't recognized."));
		}
	}

	protected override void OnExecute(ExecutionResult result, ExecutionContext context)
	{
		AudioChannelState audio = Enum<AudioChannelState>.ParseIgnoringCase(base.Operands[0].Value, AudioChannelState.None);
		IControlSystem service = ServiceLocator.Instance.GetService<IControlSystem>();
		HandleError(service.SetAudio(audio), context);
	}
}
