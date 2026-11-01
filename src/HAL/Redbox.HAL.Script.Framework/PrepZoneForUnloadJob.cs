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
		int rotations = base.Context.PopTop<int>();
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
		if (rotations >= first.Quadrants.Count)
		{
			AddError("The drum as fully rotated.");
			applicationLog.Write("The drum is fully rotated.");
			base.Context.CreateInfoResult("RotationsExceeded", "There are no more rotations to be made.");
			return;
		}
		int quadrantIndex = ((rotations > 0) ? (first.Quadrants.Count - rotations) : 0);
		IQuadrant currentQuadrant = first.Quadrants[quadrantIndex];
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
		if (rotations == 0)
		{
			quadrantIndex = first.Quadrants.Count;
		}
		if (--quadrantIndex == 0)
		{
			AddError("The drum as fully rotated.");
			applicationLog.Write("The drum is fully rotated.");
			base.Context.CreateInfoResult("RotationsExceeded", "There are no more rotations to be made.");
			return;
		}
		int targetSlot = first.Quadrants[quadrantIndex].Slots.Start - 5;
		if (targetSlot <= 0)
		{
			targetSlot = first.NumberOfSlots - Math.Abs(targetSlot);
		}
		applicationLog.Write($"PrepZone: Rotations = {rotations}, move to target Deck = {first.Number} Slot = {targetSlot}");
		ErrorCodes errorCodes = base.MotionService.MoveTo(1, targetSlot, MoveMode.None, base.Context.AppLog);
		if (errorCodes != ErrorCodes.Success)
		{
			string error = $"The MOVE instruction failed with error {errorCodes.ToString()}";
			base.Context.CreateInfoResult("MachineError", error);
			AddError(error);
		}
	}

	internal PrepZoneForUnloadJob(ExecutionResult result, ExecutionContext ctx)
		: base(result, ctx)
	{
	}
}
