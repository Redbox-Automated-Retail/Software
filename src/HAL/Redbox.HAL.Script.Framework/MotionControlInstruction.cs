using System;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Extensions;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

[Serializable]
public sealed class MotionControlInstruction : Instruction
{
	private enum Operand
	{
		None,
		Readlimits,
		Comstatus,
		Initstatus,
		Positions,
		XPos,
		YPos,
		Homeall,
		ComCheck,
		Restart,
		TestAndReset
	}

	public override string Mnemonic => "MOTIONCONTROL";

	protected override void ParseOperands(TokenList operandTokens, ExecutionResult result)
	{
		Operand operand = Enum<Operand>.ParseIgnoringCase(operandTokens[0].Value, Operand.None);
		if (operandTokens.Count < 1 || operand == Operand.None)
		{
			result.AddError(ExecutionErrors.MissingOperand, "MOTIONCONTROL instruction expects an operand.");
		}
	}

	protected override void OnExecute(ExecutionResult result, ExecutionContext context)
	{
		Operand operand = Enum<Operand>.ParseIgnoringCase(base.Operands[0].Value, Operand.None);
		IMotionControlService service = ServiceLocator.Instance.GetService<IMotionControlService>();
		switch (operand)
		{
		case Operand.Readlimits:
		{
			IMotionControlLimitResponse motionControlLimitResponse = service.ReadLimits();
			if (motionControlLimitResponse.ReadOk)
			{
				IMotionControlLimit[] limits = motionControlLimitResponse.Limits;
				foreach (IMotionControlLimit motionControlLimit in limits)
				{
					context.PushTop(string.Format("{0} LIMIT: {1} ", motionControlLimit.Limit.ToString().ToUpper(), motionControlLimit.Blocked ? "BLOCKED" : "CLEAR"));
				}
				context.PushTop(motionControlLimitResponse.Limits.Length);
			}
			else
			{
				context.PushTop("INVALID LIMITS RESPONSE");
				context.PushTop(1);
			}
			break;
		}
		case Operand.ComCheck:
			context.PushTop((!service.CommunicationOk()) ? ErrorCodes.CommunicationError : ErrorCodes.Success);
			break;
		case Operand.Comstatus:
			context.PushTop(service.CommunicationOk() ? "OK" : "COMM ERROR");
			break;
		case Operand.Initstatus:
			context.PushTop(service.IsInitialized ? "OK" : "ERROR");
			break;
		case Operand.Positions:
		{
			IControllerPosition controllerPosition = service.ReadPositions();
			if (!controllerPosition.ReadOk)
			{
				context.PushTop("MOTIONCONTROL ERROR");
			}
			else
			{
				context.PushTop($"X = {controllerPosition.XCoordinate.Value} Y = {controllerPosition.YCoordinate.Value}");
			}
			break;
		}
		case Operand.XPos:
		{
			IControllerPosition controllerPosition = service.ReadPositions();
			if (!controllerPosition.ReadOk)
			{
				context.PushTop("MOTIONCONTROL ERROR");
			}
			else
			{
				context.PushTop(controllerPosition.XCoordinate.Value);
			}
			break;
		}
		case Operand.YPos:
		{
			IControllerPosition controllerPosition = service.ReadPositions();
			if (!controllerPosition.ReadOk)
			{
				context.PushTop("MOTIONCONTROL ERROR");
			}
			else
			{
				context.PushTop(controllerPosition.YCoordinate.Value);
			}
			break;
		}
		case Operand.Homeall:
		{
			if (!service.CommunicationOk() && service.Reset(quick: true) != ErrorCodes.Success)
			{
				context.PushTop(false);
				break;
			}
			ErrorCodes errorCodes = service.InitAxes();
			context.PushTop(errorCodes == ErrorCodes.Success);
			break;
		}
		case Operand.Restart:
			context.PushTop((service.Reset(quick: true) == ErrorCodes.Success) ? base.SuccessMessage : base.TimeoutMessage);
			break;
		case Operand.TestAndReset:
			context.PushTop(service.TestAndReset(quick: true) ? base.SuccessMessage : base.TimeoutMessage);
			break;
		}
	}
}
