using System;
using System.IO;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Extensions;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

[Serializable]
public sealed class DoorSensorInstruction : Instruction
{
	private enum ExpectedOperand
	{
		None,
		IsConfigured,
		Query,
		Status,
		SoftwareOverride,
		TesterQuery
	}

	public override string Mnemonic => "DOORSENSORS";

	protected override bool IsOperandRecognized(string operand)
	{
		return Enum<ExpectedOperand>.ParseIgnoringCase(operand, ExpectedOperand.None) != ExpectedOperand.None;
	}

	protected override void ParseOperands(TokenList operandTokens, ExecutionResult result)
	{
		if (Enum<ExpectedOperand>.ParseIgnoringCase(operandTokens[0].Value, ExpectedOperand.None) == ExpectedOperand.None)
		{
			result.AddInvalidOperandError("Expected a 'Query' or 'IsConfigured' operand.");
		}
	}

	protected override void OnExecute(ExecutionResult result, ExecutionContext context)
	{
		ExpectedOperand expectedOperand = Enum<ExpectedOperand>.ParseIgnoringCase(base.Operands[0].Value, ExpectedOperand.None);
		IDoorSensorService service = ServiceLocator.Instance.GetService<IDoorSensorService>();
		switch (expectedOperand)
		{
		case ExpectedOperand.None:
			LogHelper.Instance.WithContext("DoorSensor instruction: converted '{0}' to operand None", base.Operands[0].Value);
			break;
		case ExpectedOperand.IsConfigured:
			context.PushTop(service.SensorsEnabled);
			break;
		case ExpectedOperand.SoftwareOverride:
		{
			if (!GetKeyValuePairValue<string>("OVERRIDE", out var val, result.Errors, context))
			{
				val = "off";
			}
			bool flag = (service.SoftwareOverride = val.Equals("off", StringComparison.CurrentCultureIgnoreCase));
			using (StreamWriter streamWriter = new StreamWriter(File.Open(Path.Combine(ServiceLocator.Instance.GetService<IFormattedLogFactoryService>().LogsBasePath, "Service\\DoorSensorConfiguration.log"), FileMode.Append, FileAccess.Write, FileShare.Read)))
			{
				streamWriter.WriteLine(flag ? $"{DateTime.Now.ToShortDateString()} {DateTime.Now.ToShortTimeString()}: Door sensors have a software override in place: they are not active - *UNCHECKED MOVEMENT IS POSSIBLE*." : $"{DateTime.Now.ToShortDateString()} {DateTime.Now.ToShortTimeString()}: Door sensors do not have a software override; they are active.");
			}
			context.PushTop(service.SensorsEnabled);
			break;
		}
		case ExpectedOperand.Query:
			RunQuery(service, context);
			context.PushTop(1);
			break;
		case ExpectedOperand.Status:
			context.PushTop(service.Query().ToString().ToUpper());
			break;
		case ExpectedOperand.TesterQuery:
			RunQuery(service, context);
			break;
		}
	}

	private void RunQuery(IDoorSensorService service, ExecutionContext context)
	{
		switch (service.QueryStateForDisplay())
		{
		case DoorSensorResult.AuxReadError:
			context.PushTop("AUX board not responsive.");
			break;
		case DoorSensorResult.FrontDoor:
			context.PushTop("FRONT DOOR OPEN");
			break;
		case DoorSensorResult.Ok:
			context.PushTop("FRONT DOOR CLOSED");
			break;
		case DoorSensorResult.SoftwareOverride:
			context.PushTop("OVERRIDE IN PLACE.");
			break;
		}
	}
}
