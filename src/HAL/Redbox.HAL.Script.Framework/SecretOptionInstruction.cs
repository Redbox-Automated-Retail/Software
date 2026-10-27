using System;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Extensions;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

[Serializable]
public sealed class SecretOptionInstruction : Instruction
{
	private enum Operation
	{
		None,
		Set,
		SetEncrypted,
		Get,
		GetEncrypted,
		Remove
	}

	public override string Mnemonic => "SECRETOPTION";

	protected override void ParseOperands(TokenList operandTokens, ExecutionResult result)
	{
		string value = operandTokens[0].Value;
		if (!OperationRecognized(result, value))
		{
			result.AddInvalidOperandError($"The {value} is unrecognized.");
		}
	}

	protected override void OnExecute(ExecutionResult result, ExecutionContext context)
	{
		Operation operation = ComputeOperation(result, base.Operands[0].Value);
		if (!LocateKey(result, context, out var key))
		{
			AddError("E001", "Missing 'NAME' operand.", result.Errors);
			return;
		}
		IPersistentMap map = ServiceLocator.Instance.GetService<IPersistentMapService>().GetMap();
		string val = null;
		LocateValue(result, context, out val);
		switch (operation)
		{
		case Operation.Remove:
			map.Remove(key);
			break;
		case Operation.Get:
		{
			string value = map.GetValue(key, string.Empty);
			context.PushTop(string.IsNullOrEmpty(value) ? "UNKNOWN" : value);
			break;
		}
		case Operation.GetEncrypted:
			result.Errors.Add(Error.NewError("E666", "Unsuppported operation.", "The get encrypted operation on SecretOptions is not supported."));
			break;
		case Operation.Set:
			if (string.IsNullOrEmpty(val))
			{
				AddError("E001", "A 'VALUE' operand is expected.", result.Errors);
			}
			else
			{
				map.SetValue(key, val);
			}
			break;
		case Operation.SetEncrypted:
			result.Errors.Add(Error.NewError("E666", "Unsuppported operation.", "The set encrypted operation on SecretOptions is not supported."));
			break;
		}
	}

	private bool LocateKey(ExecutionResult result, ExecutionContext context, out string key)
	{
		KeyValuePair keyValuePair = base.Operands.GetKeyValuePair("NAME");
		key = GetKeyValuePairValue<string>(keyValuePair, result.Errors, context);
		if (result.Errors.Count <= 0)
		{
			return true;
		}
		return false;
	}

	private bool LocateValue(ExecutionResult result, ExecutionContext context, out string val)
	{
		KeyValuePair keyValuePair = base.Operands.GetKeyValuePair("VALUE");
		val = GetKeyValuePairValue<string>(keyValuePair, result.Errors, context);
		if (result.Errors.Count <= 0)
		{
			return true;
		}
		return false;
	}

	private bool OperationRecognized(ExecutionResult result, string op)
	{
		return ComputeOperation(result, op) != Operation.None;
	}

	private Operation ComputeOperation(ExecutionResult result, string operation)
	{
		Operation operation2 = Enum<Operation>.ParseIgnoringCase(operation, Operation.None);
		if (operation2 != Operation.None)
		{
			return operation2;
		}
		string message = $"Unrecognized OPERATION argument: {operation}";
		LogHelper.Instance.Log(message, LogEntryType.Info);
		AddError("E001", message, result.Errors);
		return Operation.None;
	}
}
