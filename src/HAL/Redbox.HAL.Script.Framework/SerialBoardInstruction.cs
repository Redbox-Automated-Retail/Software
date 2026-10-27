using System;
using System.Threading;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Extensions;

namespace Redbox.HAL.Script.Framework;

[Serializable]
public sealed class SerialBoardInstruction : Instruction
{
	private enum ExpectedOperand
	{
		None,
		ClosePort,
		Reset
	}

	public override string Mnemonic => "SERIALBOARD";

	protected override void OnExecute(ExecutionResult result, ExecutionContext context)
	{
		ExpectedOperand expectedOperand = Enum<ExpectedOperand>.ParseIgnoringCase(base.Operands[0].Value, ExpectedOperand.None);
		IControlSystem service = ServiceLocator.Instance.GetService<IControlSystem>();
		switch (expectedOperand)
		{
		case ExpectedOperand.None:
			LogHelper.Instance.WithContext("SerialBoardInstruction: converted operand '{0}' to None", base.Operands[0].Value);
			break;
		case ExpectedOperand.ClosePort:
		{
			string instance = (service.Shutdown() ? base.SuccessMessage : base.TimeoutMessage);
			Thread.Sleep(500);
			context.PushTop(instance);
			break;
		}
		case ExpectedOperand.Reset:
		{
			IControlResponse response = service.Initialize();
			HandleError(response, context);
			break;
		}
		}
	}
}
