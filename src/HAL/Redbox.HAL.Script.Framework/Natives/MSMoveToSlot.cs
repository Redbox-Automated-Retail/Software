using System;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework.Natives;

[NativeJob(ProgramName = "tester-move-to-slot")]
internal sealed class MSMoveToSlot : NativeJobAdapter
{
	protected override void ExecuteInner()
	{
		int deck = base.Context.PopTop<int>();
		int slot = base.Context.PopTop<int>();
		int failures = 0;
		IControlSystem controlSystem = ServiceLocator.Instance.GetService<IControlSystem>();
		if (!controlSystem.GetBoardVersion(ControlBoards.Aux).ReadSuccess)
		{
			base.Context.CreateInfoResult("BoardVersionTimeout", "Move: AUX board not responsive.");
			failures++;
		}
		if (!controlSystem.GetBoardVersion(ControlBoards.Picker).ReadSuccess)
		{
			base.Context.CreateInfoResult("BoardVersionTimeout", "Move: PCB not responsive.");
			failures++;
		}
		IMotionControlService motionControlService = ServiceLocator.Instance.GetService<IMotionControlService>();
		IMotionControlLimitResponse motionControlLimitResponse = motionControlService.ReadLimits();
		if (!motionControlLimitResponse.ReadOk)
		{
			base.Context.CreateInfoResult(ErrorCodes.ArcusNotResponsive.ToString().ToUpper(), "Unable to read limits.");
			failures++;
		}
		else
		{
			Array.ForEach(motionControlLimitResponse.Limits, delegate(IMotionControlLimit limit)
			{
				if (limit.Blocked)
				{
					base.Context.CreateInfoResult("LimitBlocked", $"MOVE: {limit.Limit.ToString().ToUpper()} LIMIT BLOCKED");
					failures++;
				}
			});
		}
		if (failures > 0)
		{
			AddError("Job failure");
			return;
		}
		IFormattedLogFactoryService formattedLogFactoryService = ServiceLocator.Instance.GetService<IFormattedLogFactoryService>();
		ErrorCodes errorCodes = motionControlService.MoveTo(deck, slot, MoveMode.None, formattedLogFactoryService.NilLog);
		base.Context.CreateInfoResult("MoveResult", $"MOVE DECK={deck} SLOT={slot} {errorCodes.ToString().ToUpper()}");
	}

	internal MSMoveToSlot(ExecutionResult result, ExecutionContext ctx)
		: base(result, ctx)
	{
	}
}
