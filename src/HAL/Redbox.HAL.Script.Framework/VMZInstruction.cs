using System;
using System.Collections.Generic;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Controller.Framework;
using Redbox.HAL.Core;

namespace Redbox.HAL.Script.Framework;

[Serializable]
public sealed class VMZInstruction : Instruction
{
	public override string Mnemonic => "VMZ";

	protected override void ParseOperands(TokenList operandTokens, ExecutionResult result)
	{
		if (operandTokens[0].Type != TokenType.Symbol)
		{
			result.AddInvalidOperandError("Expected symbol: GET-THIN, GET-UNLOAD or MARK-EMPTY.");
		}
	}

	protected override void OnExecute(ExecutionResult result, ExecutionContext context)
	{
		ServiceLocator.Instance.GetService<IMotionControlService>();
		if (string.Compare(base.Operands[0].Value, "IS-CONFIGURED", ignoreCase: true) == 0)
		{
			context.PushTop(ControllerConfiguration.Instance.IsVMZMachine);
		}
		else
		{
			if (string.Compare(base.Operands[0].Value, "REGISTER-CLEAN", ignoreCase: true) == 0)
			{
				return;
			}
			if (string.Compare(base.Operands[0].Value, "RANDOMIZE-CLEAN", ignoreCase: true) == 0)
			{
				result.Errors.Add(Error.NewError("E203", "Deprecated operand", "The RANDOMIZE-CLEAN operand is deprecated."));
			}
			else if (string.Compare(base.Operands[0].Value, "FETCH-LAST-THIN", ignoreCase: true) == 0)
			{
				FetchLast(MerchFlags.Thin, context);
			}
			else if (string.Compare(base.Operands[0].Value, "FETCH-LAST-REBALANCE", ignoreCase: true) == 0)
			{
				FetchLast(MerchFlags.Rebalance, context);
			}
			else if (string.Compare(base.Operands[0].Value, "NON-THIN-COUNT", ignoreCase: true) == 0)
			{
				AddError("E101", "NON-THIN-COUNT is not supported.", result.Errors);
			}
			else if (string.Compare(base.Operands[0].Value, "AGGREGATE-CLEAN", ignoreCase: true) == 0)
			{
				AggregateCleanData(context);
			}
			else if (string.Compare(base.Operands[0].Value, "CLEAN-SLOT", ignoreCase: true) == 0)
			{
				CleanResult result2 = default(CleanResult);
				CleanManager.Instance.CleanSlot(ref result2, context, SegmentManager.Instance);
				ProcessAnyTransfers(result2, context);
				if (result2.VisitedAllSlots)
				{
					context.PushTop("CLEAN");
				}
				else if (result2.NoEmptySlots)
				{
					context.PushTop(MerchandizeResult.MachineFull.ToString().ToUpper());
				}
				else
				{
					context.PushTop((result2.TransferFailures > 0) ? "TRANSFERFAILURE" : base.SuccessMessage);
				}
			}
			else if (string.Compare(base.Operands[0].Value, "MARK-UNLOAD", ignoreCase: true) == 0)
			{
				int num = 0;
				KeyValuePair keyValuePair = base.Operands.GetKeyValuePair("LAST-DECK");
				int? keyValuePairValue = GetKeyValuePairValue<int?>(keyValuePair, result.Errors, context);
				if (result.Errors.Count > 0 || !keyValuePairValue.HasValue)
				{
					context.PushTop("ERROR");
					return;
				}
				num = keyValuePairValue.Value;
				context.PushTop(VMZ.Instance.ReserveUnload(num));
			}
			else if (string.Compare(base.Operands[0].Value, "MARK-EMPTY", ignoreCase: true) == 0)
			{
				ILocation location = SegmentManager.Instance.FindHighest();
				if (location == null)
				{
					context.PushTop(0);
				}
				else
				{
					context.PushTop(VMZ.Instance.ResetCompressedZone(location));
				}
			}
			else if (string.Compare(base.Operands[0].Value, "DUMP-ZONE", ignoreCase: true) == 0)
			{
				AddError("E002", "The operand DUMP-ZONE is no longer supported.", result.Errors);
			}
			else if (string.Compare(base.Operands[0].Value, "DUMP-ZONE-TO-APPLOG", ignoreCase: true) == 0)
			{
				VMZ.Instance.DumpZone(context.AppLog);
			}
			else if (string.Compare(base.Operands[0].Value, "THIN-DISC", ignoreCase: true) == 0)
			{
				ThinType(result, context, MerchFlags.Thin);
			}
			else if (string.Compare(base.Operands[0].Value, "THIN-REBALANCE", ignoreCase: true) == 0)
			{
				ThinType(result, context, MerchFlags.Rebalance);
			}
			else if (string.Compare(base.Operands[0].Value, "THIN-REDEPLOY", ignoreCase: true) == 0)
			{
				ThinType(result, context, MerchFlags.ThinRedeploy);
			}
			else if (string.Compare(base.Operands[0].Value, "UNLOAD-DISC", ignoreCase: true) == 0)
			{
				context.PushTop(VMZHelperFunctions.Instance.UnloadDisc(result, context).ToString().ToUpper());
			}
			else if (string.Compare(base.Operands[0].Value, "GET-LAST-THIN", ignoreCase: true) == 0)
			{
				LocateLast(MerchFlags.Thin, context);
			}
			else if (string.Compare(base.Operands[0].Value, "GET-LAST-REBALANCE", ignoreCase: true) == 0)
			{
				LocateLast(MerchFlags.Rebalance, context);
			}
			else if (string.Compare(base.Operands[0].Value, "GET-LAST-THIN-REDEPLOY", ignoreCase: true) == 0)
			{
				LocateLast(MerchFlags.ThinRedeploy, context);
			}
			else if (string.Compare(base.Operands[0].Value, "THIN-ZONE-COUNT", ignoreCase: true) == 0)
			{
				PushCount(MerchFlags.Thin, context);
			}
			else if (string.Compare(base.Operands[0].Value, "REBALANCE-COUNT", ignoreCase: true) == 0)
			{
				PushCount(MerchFlags.Rebalance, context);
			}
			else if (string.Compare(base.Operands[0].Value, "THIN-REDEPLOY-COUNT", ignoreCase: true) == 0)
			{
				PushCount(MerchFlags.ThinRedeploy, context);
			}
			else if (string.Compare(base.Operands[0].Value, "MOVE-VIEWABLE", ignoreCase: true) == 0)
			{
				context.PushTop(VMZHelperFunctions.MoveViewable(context).ToString().ToUpper());
			}
			else if (string.Compare(base.Operands[0].Value, "ZONE-NON-THIN-COUNT", ignoreCase: true) == 0)
			{
				List<ILocation> nonThinItemsInCompressedZone = SegmentManager.Instance.GetNonThinItemsInCompressedZone();
				context.PushTop(nonThinItemsInCompressedZone.Count.ToString());
				nonThinItemsInCompressedZone.Clear();
			}
		}
	}

	private void FetchLast(MerchFlags flags, ExecutionContext context)
	{
		IRuntimeService service = ServiceLocator.Instance.GetService<IRuntimeService>();
		IMotionControlService service2 = ServiceLocator.Instance.GetService<IMotionControlService>();
		ErrorList errors = new ErrorList();
		service.Wait(GetTimeOutValue(errors, context) ?? 8000);
		ILocation location = VMZ.Instance.FindHighestLocation(flags);
		if (location == null)
		{
			VMZHelperFunctions.MoveViewable(context);
			context.PushTop("NONE");
			return;
		}
		ErrorCodes errorCodes = service2.MoveTo(location, MoveMode.Get, context.AppLog);
		if (errorCodes != ErrorCodes.Success)
		{
			context.PushTop(errorCodes.ToString().ToUpper());
			return;
		}
		IGetResult getResult = ServiceLocator.Instance.GetService<IControllerService>().Get();
		if (getResult.Success)
		{
			VMZHelperFunctions.MoveViewable(context);
			context.PushTop(base.SuccessMessage);
		}
		else
		{
			context.PushTop(getResult.ToString().ToUpper());
		}
	}

	private void PushCount(MerchFlags flags, ExecutionContext context)
	{
		MerchSegment merchSegment = MerchSegmentFactory.Get(flags);
		int num = ((context != null) ? merchSegment.ItemCount() : 0);
		context.PushTop(num);
	}

	private void ThinType(ExecutionResult result, ExecutionContext context, MerchFlags flags)
	{
		string iD = GetID(result, context);
		MerchandizeResult merchandizeResult = ThinHelper.MakeInstance(result, context).ThinDisk(iD, flags);
		if (merchandizeResult != MerchandizeResult.Success)
		{
			LogHelper.Instance.WithContext("Thin {0} returned status {1}", flags.ToString(), merchandizeResult);
		}
		context.PushTop(merchandizeResult.ToString().ToUpper());
	}

	private void ProcessAnyTransfers(CleanResult result, ExecutionContext context)
	{
		if (result.Transfers != 0)
		{
			int num = 0;
			ErrorList errorList = new ErrorList();
			object symbolValue = context.GetSymbolValue("TRANSFER-COUNT", errorList);
			if (errorList.Count == 0)
			{
				num = (int)symbolValue;
			}
			num += result.Transfers;
			context.SetSymbolValue("TRANSFER-COUNT", num);
		}
	}

	private void LocateLast(MerchFlags flags, ExecutionContext context)
	{
		ILocation location = VMZ.Instance.FindHighestLocation(flags);
		if (location == null)
		{
			context.PushTop("NONE");
			return;
		}
		context.PushTop(location.Slot);
		context.PushTop(location.Deck);
		context.PushTop("FOUND");
	}

	private void AggregateCleanData(ExecutionContext ctx)
	{
		int num = 0;
		ErrorList errorList = new ErrorList();
		object symbolValue = ctx.GetSymbolValue("TRANSFER-COUNT", errorList);
		if (errorList.Count == 0)
		{
			num = (int)symbolValue;
		}
		ctx.CreateInfoResult("TransferredDiskCount", num.ToString());
		foreach (MerchFlags item in SegmentManager.Instance.DecreasingPriorityOrder)
		{
			MerchSegment merchSegment = MerchSegmentFactory.Get(item);
			ctx.CreateInfoResult($"{item.ToString()}CountInZone", merchSegment.ItemCount().ToString());
			ILocation location = VMZ.Instance.FindHighestLocation(item);
			if (location != null)
			{
				ctx.CreateInfoResult($"High{item.ToString()}Location", location.ToString());
			}
		}
	}
}
