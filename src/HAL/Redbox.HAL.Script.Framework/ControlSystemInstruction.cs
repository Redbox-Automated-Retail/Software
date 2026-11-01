using System;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Extensions;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

[Serializable]
public sealed class ControlSystemInstruction : Instruction
{
	private enum Ops
	{
		None,
		Center,
		RinglightOn,
		RinglightOff,
		PickerSensorsOn,
		PickerSensorsOff,
		EngageQlm,
		DisengageQlm,
		DropQlm,
		LiftQlm,
		HaltQlmOperation
	}

	public override string Mnemonic => "CONTROLSYSTEM";

	protected override void ParseOperands(TokenList operandTokens, ExecutionResult result)
	{
		if (operandTokens.Count == 0)
		{
			result.AddInvalidOperandError($"The {Mnemonic} instruction expects one operand.");
		}
		else if (Enum<Ops>.ParseIgnoringCase(operandTokens[0].Value, Ops.None) == Ops.None)
		{
			result.AddInvalidOperandError($"The operand {operandTokens[0].Value} is not recognized by {Mnemonic}.");
		}
	}

	protected override void OnExecute(ExecutionResult result, ExecutionContext context)
	{
		Ops ops = Enum<Ops>.ParseIgnoringCase(base.Operands[0].Value, Ops.None);
		IControlSystem controlSystem = ServiceLocator.Instance.GetService<IControlSystem>();
		IControlResponse controlResponse = null;
		IFormattedLogFactoryService formattedLogFactoryService = ServiceLocator.Instance.GetService<IFormattedLogFactoryService>();
		switch (ops)
		{
		case Ops.Center:
			context.PushTop(controlSystem.Center(CenterDiskMethod.VendDoorAndBack).ToString());
			break;
		case Ops.RinglightOn:
			controlResponse = controlSystem.ToggleRingLight(on: true, null);
			break;
		case Ops.RinglightOff:
			controlResponse = controlSystem.ToggleRingLight(on: false, null);
			break;
		case Ops.PickerSensorsOn:
			controlResponse = controlSystem.SetSensors(on: true);
			break;
		case Ops.PickerSensorsOff:
			controlResponse = controlSystem.SetSensors(on: false);
			break;
		case Ops.EngageQlm:
			context.PushTop(controlSystem.EngageQlm(formattedLogFactoryService.NilLog).ToString().ToUpper());
			break;
		case Ops.DisengageQlm:
			context.PushTop(controlSystem.DisengageQlm(home: true, formattedLogFactoryService.NilLog).ToString().ToUpper());
			break;
		case Ops.DropQlm:
			controlResponse = controlSystem.DropQlm();
			break;
		case Ops.LiftQlm:
			controlResponse = controlSystem.LiftQlm();
			break;
		case Ops.HaltQlmOperation:
			controlResponse = controlSystem.HaltQlmLifter();
			break;
		default:
			result.AddError(ExecutionErrors.InvalidSymbolValue, $"The value {base.Operands[0].Value} is unrecognized.");
			break;
		}
		if (controlResponse != null)
		{
			HandleError(controlResponse, context);
		}
	}
}
