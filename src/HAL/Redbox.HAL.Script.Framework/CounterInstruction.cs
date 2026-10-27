using System;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Extensions;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

[Serializable]
public sealed class CounterInstruction : Instruction
{
	private enum Ops
	{
		None,
		Increment,
		Decrement,
		Get,
		Create,
		Reset
	}

	public override string Mnemonic => "COUNTER";

	protected override void ParseOperands(TokenList operandTokens, ExecutionResult result)
	{
		if (!operandTokens[0].IsKeyValuePair)
		{
			result.AddInvalidOperandError($"The {Mnemonic} instruction expected at least a key-value pair.");
		}
	}

	protected override void OnExecute(ExecutionResult result, ExecutionContext context)
	{
		KeyValuePair keyValuePair = base.Operands.GetKeyValuePair("NAME");
		string keyValuePairValue = GetKeyValuePairValue<string>(keyValuePair, result.Errors, context);
		IPersistentCounterService service = ServiceLocator.Instance.GetService<IPersistentCounterService>();
		IDumpbinService service2 = ServiceLocator.Instance.GetService<IDumpbinService>();
		string value = base.Operands[1].Value;
		switch (Enum<Ops>.ParseIgnoringCase(value, Ops.None))
		{
		case Ops.None:
		{
			string message = $"Unrecognized operation: {value}";
			AddError("E001", message, result.Errors);
			break;
		}
		case Ops.Create:
			context.PushTop((service.Find(keyValuePairValue) != null) ? base.SuccessMessage : "FAILURE");
			break;
		case Ops.Decrement:
		{
			IPersistentCounter persistentCounter = service.Decrement(keyValuePairValue);
			if (persistentCounter == null)
			{
				context.PushTop("FAILURE");
				break;
			}
			context.PushTop(persistentCounter.Value);
			context.PushTop(base.SuccessMessage);
			break;
		}
		case Ops.Increment:
		{
			IPersistentCounter persistentCounter = service.Find(keyValuePairValue);
			if (persistentCounter == null)
			{
				context.PushTop("FAILURE");
				break;
			}
			context.PushTop(persistentCounter.Value);
			context.PushTop(base.SuccessMessage);
			break;
		}
		case Ops.Get:
		{
			if (keyValuePairValue.Equals("DUMPBIN", StringComparison.CurrentCultureIgnoreCase))
			{
				context.PushTop(service2.CurrentCount());
				context.PushTop(base.SuccessMessage);
				break;
			}
			IPersistentCounter persistentCounter = service.Find(keyValuePairValue);
			if (persistentCounter == null)
			{
				context.PushTop("FAILURE");
				break;
			}
			context.PushTop(persistentCounter.Value);
			context.PushTop(base.SuccessMessage);
			break;
		}
		case Ops.Reset:
			context.PushTop(service.Reset(keyValuePairValue) ? base.SuccessMessage : "FAILURE");
			break;
		}
	}
}
