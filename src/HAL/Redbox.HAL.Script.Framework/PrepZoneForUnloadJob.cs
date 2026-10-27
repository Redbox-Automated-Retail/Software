using System;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Controller.Framework;

namespace Redbox.HAL.Script.Framework;

[NativeJob(ProgramName = "prep-zone-for-unload", Operand = "PREP-ZONE-FOR-UNLOAD")]
internal sealed class PrepZoneForUnloadJob : NativeJobAdapter
{
	protected override void ExecuteInner()
	{
		ApplicationLog applicationLog = ApplicationLog.ConfigureLog(base.Context, logAppends: true, "VMZ", flushOnWrite: false, null);
		int num = base.Context.PopTop<int>();
		int deckCount = base.Context.PopTop<int>();
		int totalReset = 0;
		if (!ControllerConfiguration.Instance.IsVMZMachine)
		{
			AddError("Prep Zone for unload is supported only on VMZ machines. I'm sorry.");
			applicationLog.Write("Prep Zone for unload is supported only on VMZ machines. I'm sorry.");
			return;
		}
		DoorSensorResult doorSensorResult = ServiceLocator.Instance.GetService<IDoorSensorService>().Query();
		if (doorSensorResult != DoorSensorResult.Ok)
		{
			string text = $"Door sensor query returned error {doorSensorResult.ToString()}";
			AddError(text);
			applicationLog.Write(text);
			base.Context.CreateInfoResult("DoorOpen", text);
			return;
		}
		IDeck first = base.DecksService.First;
		if (num >= first.Quadrants.Count)
		{
			AddError("The drum as fully rotated.");
			applicationLog.Write("The drum is fully rotated.");
			base.Context.CreateInfoResult("RotationsExceeded", "There are no more rotations to be made.");
			return;
		}
		int num2 = ((num > 0) ? (first.Quadrants.Count - num) : 0);
		IQuadrant currentQuadrant = first.Quadrants[num2];
		base.DecksService.ForAllDecksDo(delegate(IDeck deck)
		{
			if (deckCount > 0 && deck.Number > deckCount)
			{
				return false;
			}
			totalReset += base.InventoryService.SwapEmptyWith(deck, "UNKNOWN", MerchFlags.Unload, currentQuadrant.Slots).Count;
			return true;
		});
		applicationLog.WriteFormatted("{0} locations were marked for unload covering slots {1} -> {2}.", totalReset, currentQuadrant.Slots.Start, currentQuadrant.Slots.End);
		base.Context.CreateInfoResult("LocationChangedCount", totalReset.ToString());
		if (num == 0)
		{
			num2 = first.Quadrants.Count;
		}
		if (--num2 == 0)
		{
			AddError("The drum as fully rotated.");
			applicationLog.Write("The drum is fully rotated.");
			base.Context.CreateInfoResult("RotationsExceeded", "There are no more rotations to be made.");
			return;
		}
		int num3 = first.Quadrants[num2].Slots.Start - 5;
		if (num3 <= 0)
		{
			num3 = first.NumberOfSlots - Math.Abs(num3);
		}
		applicationLog.Write($"PrepZone: Rotations = {num}, move to target Deck = {first.Number} Slot = {num3}");
		ErrorCodes errorCodes = base.MotionService.MoveTo(1, num3, MoveMode.None, base.Context.AppLog);
		if (errorCodes != ErrorCodes.Success)
		{
			string text2 = $"The MOVE instruction failed with error {errorCodes.ToString()}";
			base.Context.CreateInfoResult("MachineError", text2);
			AddError(text2);
		}
	}

	internal PrepZoneForUnloadJob(ExecutionResult result, ExecutionContext ctx)
		: base(result, ctx)
	{
	}
}
