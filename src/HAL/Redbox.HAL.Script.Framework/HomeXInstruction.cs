using System;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework;

[Serializable]
public sealed class HomeXInstruction : Instruction
{
	public override string Mnemonic => "HOMEX";

	protected override ExpectedOperands Expected => ExpectedOperands.None;

	protected override void OnExecute(ExecutionResult result, ExecutionContext context)
	{
		string instance = ServiceLocator.Instance.GetService<IMotionControlService>().HomeAxis(Axis.X).ToString()
			.ToUpper();
		context.PushTop(instance);
	}
}
