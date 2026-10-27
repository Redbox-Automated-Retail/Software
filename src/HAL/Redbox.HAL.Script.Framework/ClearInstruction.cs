using System;

namespace Redbox.HAL.Script.Framework;

[Serializable]
public sealed class ClearInstruction : Instruction
{
	public override string Mnemonic => "CLEAR";

	protected override ExpectedOperands Expected => ExpectedOperands.None;

	protected override void OnExecute(ExecutionResult result, ExecutionContext context)
	{
		context.ClearStack();
	}
}
