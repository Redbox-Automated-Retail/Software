using System;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Extensions;
using Redbox.HAL.Controller.Framework;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

[Serializable]
public sealed class DumpBinInstruction : Instruction
{
	private enum ExpectedOperand
	{
		None,
		IsConfigured,
		SetCapacity,
		GetCapacity,
		ResetCounter,
		GetCurrentCount,
		ResetCounterMove,
		FileDisk,
		DumpInventory
	}

	public override string Mnemonic => "DUMPBIN";

	protected override void ParseOperands(TokenList operandTokens, ExecutionResult result)
	{
		if (operandTokens.Count < 1 || !IsValidOperand(operandTokens[0].Value))
		{
			result.AddInvalidOperandError($"The {Mnemonic} instruction expects at lest one operand.");
		}
	}

	protected override void OnExecute(ExecutionResult result, ExecutionContext context)
	{
		ExpectedOperand expectedOperand = Enum<ExpectedOperand>.ParseIgnoringCase(base.Operands[0].Value, ExpectedOperand.None);
		IDumpbinService service = ServiceLocator.Instance.GetService<IDumpbinService>();
		switch (expectedOperand)
		{
		case ExpectedOperand.None:
		{
			string text = $"DumpBinInstruction: unrecognized operand '{base.Operands[0].Value}'.";
			LogHelper.Instance.WithContext(text);
			result.Errors.Add(Error.NewError("E001", "Unrecognized operand.", text));
			break;
		}
		case ExpectedOperand.IsConfigured:
			context.PushTop(ControllerConfiguration.Instance.IsVMZMachine);
			break;
		case ExpectedOperand.GetCapacity:
			context.PushTop(service.Capacity);
			break;
		case ExpectedOperand.SetCapacity:
			result.Errors.Add(Error.NewError("E011", "Unsupported operand.", "The SetCapacity operand is no longer supported."));
			break;
		case ExpectedOperand.GetCurrentCount:
			context.PushTop(service.CurrentCount());
			break;
		case ExpectedOperand.ResetCounter:
			service.ClearItems();
			break;
		case ExpectedOperand.FileDisk:
		{
			string iD = GetID(result, context);
			if (result.Errors.ContainsError() || string.IsNullOrEmpty(iD))
			{
				context.PushTop(MerchandizeResult.LookupFailure);
				break;
			}
			DumpBinThinHelper dumpBinThinHelper = new DumpBinThinHelper(result, context);
			context.PushTop(dumpBinThinHelper.ThinDisk(iD, MerchFlags.Thin).ToString().ToUpper());
			break;
		}
		case ExpectedOperand.ResetCounterMove:
			service.ClearItems();
			VMZHelperFunctions.MoveViewable(context);
			break;
		case ExpectedOperand.DumpInventory:
			service.DumpContents(context.AppLog);
			break;
		}
	}

	private bool IsValidOperand(string op)
	{
		return Enum<ExpectedOperand>.ParseIgnoringCase(op, ExpectedOperand.None) != ExpectedOperand.None;
	}
}
