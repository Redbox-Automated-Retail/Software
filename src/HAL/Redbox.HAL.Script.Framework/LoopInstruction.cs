using System;

namespace Redbox.HAL.Script.Framework;

[Serializable]
public sealed class LoopInstruction : Instruction
{
	public override bool EndsContext => true;

	public override string Mnemonic => "LOOP";

	internal override Branch BranchType => Branch.Conditional;

	protected override ExpectedOperands Expected => ExpectedOperands.None;

	protected override void OnExecute(ExecutionResult result, ExecutionContext context)
	{
		context.Registers.ActiveFrame.X--;
		if (context.Registers.ActiveFrame.X != 0)
		{
			context.Registers.ActiveFrame.ResetPC();
			result.RestartAtCurrentIP = true;
		}
	}
}
