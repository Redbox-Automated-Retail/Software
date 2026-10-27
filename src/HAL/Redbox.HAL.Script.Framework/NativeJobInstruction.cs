using System;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework;

[Serializable]
public sealed class NativeJobInstruction : Instruction
{
	public override string Mnemonic => "NATIVEJOB";

	protected override void OnExecute(ExecutionResult result, ExecutionContext context)
	{
		NativeJobAdapter nativeJobAdapter = ExecutionEngine.Instance.MakeJobFromOperand(context, result, base.Operands[0].Value);
		if (nativeJobAdapter == null)
		{
			result.Errors.Add(Error.NewError("E777", "Unrecognized job.", $"The job \"{base.Operands[0].Value}\" is not in the native job cache."));
		}
		else
		{
			nativeJobAdapter.Execute();
		}
	}
}
