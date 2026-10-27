using System;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Script.Framework.Natives;

[NativeJob(ProgramName = "tester-move-to-slot")]
internal sealed class MSMoveToSlot : NativeJobAdapter
{
	protected override void ExecuteInner()
	{
		int num = base.Context.PopTop<int>();
		int num2 = base.Context.PopTop<int>();
		int failures = 0;
		IControlSystem service = ServiceLocator.Instance.GetService<IControlSystem>();
		if (!service.GetBoardVersion(ControlBoards.Aux).ReadSuccess)
		{
			base.Context.CreateInfoResult("BoardVersionTimeout", "Move: AUX board not responsive.");
			int num3 = failures + 1;
			failures = num3;
		}
		if (!service.GetBoardVersion(ControlBoards.Picker).ReadSuccess)
		{
			base.Context.CreateInfoResult("BoardVersionTimeout", "Move: PCB not responsive.");
			int num3 = failures + 1;
			failures = num3;
		}
		IMotionControlService service2 = ServiceLocator.Instance.GetService<IMotionControlService>();
		IMotionControlLimitResponse motionControlLimitResponse = service2.ReadLimits();
		if (!motionControlLimitResponse.ReadOk)
		{
			base.Context.CreateInfoResult(ErrorCodes.ArcusNotResponsive.ToString().ToUpper(), "Unable to read limits.");
			int num3 = failures + 1;
			failures = num3;
		}
		else
		{
			Array.ForEach(motionControlLimitResponse.Limits, delegate(IMotionControlLimit limit)
			{
				if (limit.Blocked)
				{
					base.Context.CreateInfoResult("LimitBlocked", $"MOVE: {limit.Limit.ToString().ToUpper()} LIMIT BLOCKED");
					int num4 = failures + 1;
					failures = num4;
				}
			});
		}
		if (failures > 0)
		{
			AddError("Job failure");
			return;
		}
		IFormattedLogFactoryService service3 = ServiceLocator.Instance.GetService<IFormattedLogFactoryService>();
		ErrorCodes errorCodes = service2.MoveTo(num, num2, MoveMode.None, service3.NilLog);
		base.Context.CreateInfoResult("MoveResult", $"MOVE DECK={num} SLOT={num2} {errorCodes.ToString().ToUpper()}");
	}

	internal MSMoveToSlot(ExecutionResult result, ExecutionContext ctx)
		: base(result, ctx)
	{
	}
}
