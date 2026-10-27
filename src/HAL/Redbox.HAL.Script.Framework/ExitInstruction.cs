using System;

namespace Redbox.HAL.Script.Framework;

[Serializable]
public sealed class ExitInstruction : Instruction
{
	public override string Mnemonic => "EXIT";

	public override bool ExitsContext => true;

	protected override ExpectedOperands Expected => ExpectedOperands.Optional;

	protected override void OnExecute(ExecutionResult result, ExecutionContext context)
	{
		if (base.Operands.Count > 0)
		{
			string text = base.Operands[0].Value;
			if (base.Operands[0].IsSymbolOrConst)
			{
				text = (string)context.GetSymbolValue(base.Operands[0].Value, result.Errors);
				if (text == null || result.Errors.Count > 0)
				{
					AddError("E999", "Unspecified context exit; check the logs.", result.Errors);
					return;
				}
			}
			AddError("E999", text, result.Errors);
		}
		else
		{
			AddError("E999", "Unspecified context exit; check the logs.", result.Errors);
		}
	}
}
